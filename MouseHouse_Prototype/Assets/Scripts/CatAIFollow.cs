using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CatAIFollow : MonoBehaviour
{
    // Radius/Sensing variables
    private float detectionRadius = 1f;
    private float catnipRadius = 2f;
    private float roamRadius = 12f;
    private Vector3 lastKnownPlayerPos;

    // Speed variables
    private float chaseSpeed = 0.9f;
    private float roamSpeed = 0.5f;
    private const float roamInterval = 10f;
    private const float investigateInterval = 4f;

    // Delimeters
    private float playerTraveledAwake = 10f; // How far the player can travel before the cat wakes up

    // Boolean flags
    private bool asleep = false;
    private bool isChasingCatnip = false;
    private bool isChasingPlayer = false;
    private bool goingToLastKnown = false;
    private bool isRoaming = true;
    private bool isIdle = false;

    // Objects/Components
    public Transform player;
    public Transform target;
    public Sprite catSprite;
    public GameObject catnip;
    private NavMeshAgent cat;
    private SpriteRenderer art;
    public Animator animator;
    public PlayerMovement mousePlayer;
    private Coroutine currCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cat = GetComponent<NavMeshAgent>();
        art = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        target = player;
        art.sprite = catSprite;
    }


    // Update is called once per frame
    void Update()
    {
        // Check for catnip within radius
        if (asleep)
        {
            DetectCatnip();
            DetectMouseMovement();
        }

        // Disable idle triggers (Catnip ball and mouse movement) 
        else
        {
            AnimateCat();
            CatMovementV2();
        }
    }


    private bool HasDestination()
    {
        if (cat == null) return false;
        if (cat.pathPending) return true;
        if (!cat.hasPath) return false;
        return cat.remainingDistance > cat.stoppingDistance + 0.01f;
    }


    private void DetectCatnip()
    {
        target = cat.transform;
        float catnipDist = Vector3.Distance(target.position, catnip.transform.position);
        Debug.Log("Catnip Dist: " +  catnipDist);
        if (catnipDist <= catnipRadius)
        {
            asleep = false;
            isRoaming = true;
            cat.SetDestination(catnip.transform.position);
        }
        target = player.transform;
    }


    private void DetectMouseMovement()
    {
        if (mousePlayer.distTraveled >= playerTraveledAwake)
        {
            asleep = false;
            isRoaming = true;
            cat.SetDestination(player.transform.position);
        }
    }

    public void CatMovementV2()
    {
        // Chase player
        float playerDist = Vector3.Distance(transform.position, player.position);
        float catnipDist = Vector3.Distance(transform.position, catnip.transform.position);
        if (playerDist <= detectionRadius)
        {
            isChasingPlayer = true;
            goingToLastKnown = false;

            // Stop any current coroutine
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            // record last known position
            lastKnownPlayerPos = player.transform.position;

            // Update speed
            UpdateSpeed(chaseSpeed);

            // Set new destination
            cat.SetDestination(player.position);
        }

        // Player just left detection radius
        else if (isChasingPlayer && !goingToLastKnown)
        {
            isChasingPlayer = false;
            goingToLastKnown = true;

            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            // Update speed
            UpdateSpeed(chaseSpeed);

            // Set new destination
            cat.SetDestination(lastKnownPlayerPos);
        }

        // cat is going to players last known position
        else if (goingToLastKnown)
        {
            if (!cat.pathPending && !HasDestination())
            {
                goingToLastKnown = false;
                currCoroutine = StartCoroutine(Investigate(investigateInterval));
            }
        }

        // Chase catnip
        else if (catnipDist <= detectionRadius)
        {
            // Stop any current coroutine
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            // Update speed
            UpdateSpeed(chaseSpeed);

            // Set new destination
            cat.SetDestination(catnip.transform.position);
        }

        // Roam
        else if (isRoaming)
        {
            // Check current coroutine
            if (currCoroutine != null || HasDestination())
            {
                return;
            }

            // Update speed
            UpdateSpeed(roamSpeed);

            //Set new destination
            currCoroutine = StartCoroutine(RoamRoutine(roamInterval));
        }

        // Idle
        else if (isIdle)
        {
            // Stop any current coroutine
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            // Update speed
            UpdateSpeed(0f);

            // Set new coroutine
            currCoroutine = StartCoroutine(Investigate(investigateInterval));

        }

        // Fallback
        else
        {
            // Stop any current coroutine
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            // Update speed
            UpdateSpeed(0f);
            Debug.Log("Resorted to roaming fallback");
        }
    }


    // Generate a random point
    public Vector3 GeneratePoint(Vector3 origin, float range)
    {
        const int maxAttempts = 30;
        const float sampleMaxDistance = 20f;
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();

        // Ensure we use a position that is actually on the NavMesh as the origin for path calculations.
        Vector3 navOrigin = origin;
        NavMeshHit originHit;

        // try a small radius first then expand
        if (NavMesh.SamplePosition(origin, out originHit, 1f, NavMesh.AllAreas))
        {
            navOrigin = originHit.position;
        }
        else if (cat != null && NavMesh.SamplePosition(cat.transform.position, out originHit, 1f, NavMesh.AllAreas))
        {
            navOrigin = originHit.position;
        }
        else if (NavMesh.SamplePosition(origin, out originHit, sampleMaxDistance, NavMesh.AllAreas))
        {
            navOrigin = originHit.position;
        }

        for (int i = 0; i < maxAttempts; i++)
        {
            // Pick random point around the navOrigin
            Vector3 randomOffset = Random.insideUnitSphere * range;
            randomOffset.y = 0f;
            Vector3 candidate = navOrigin + randomOffset;

            if (NavMesh.SamplePosition(candidate, out hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                // Calculate path starting from navOrigin (which is on the NavMesh)
                if (NavMesh.CalculatePath(navOrigin, hit.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }

        // Fallback
        return navOrigin;
    }


    IEnumerator RoamRoutine(float waitTime)
    {
        while (isRoaming && !asleep)
        {
            Vector3 newDest = GeneratePoint(transform.position, roamRadius);
            // Only set destination if it's meaningfully different
            if ((newDest - cat.destination).sqrMagnitude > 0.01f)
            {
                cat.SetDestination(newDest);
                Debug.Log("New Cat Destination Set: " + newDest);
            }
            else
            {
                Debug.Log("GeneratePoint returned fallback origin; retrying next tick.");
            }
            yield return new WaitForSeconds(waitTime);
        }
        currCoroutine = null;
    }

    private void UpdateSpeed(float speed)
    {
        cat.speed = speed;
        animator.speed = speed;
    }

    IEnumerator Investigate(float invTime)
    {
        isIdle = true;
        yield return new WaitForSeconds(invTime);
        currCoroutine = null;
        isIdle = false;
        isRoaming = true;
    }


    public void AnimateCat()
    {
        bool catmovement = cat.velocity.magnitude > .1f;
        animator.SetBool("catmovement", catmovement);
        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);
    }


    public void target_Set(Transform new_target)
    {
        target = new_target;
        //TODO 
        //make go back to player after amount of time
    }










    // Original code for cat movement
    public void ChasePlayerV1()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRadius) { cat.SetDestination(player.position); }

        bool catmovement = cat.velocity.magnitude > .1f;
        animator.SetBool("catmovement", catmovement);

        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);
    }

    public void CatMovement()
    {
        // Chase player if within radius
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRadius)
        {
            isIdle = false;
            UpdateSpeed(chaseSpeed);
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            cat.SetDestination(player.position);
        }

        // Cat is roaming
        else if (isRoaming && !HasDestination() && !isIdle)
        {
            UpdateSpeed(roamSpeed);
            if (currCoroutine == null)
            {
                currCoroutine = StartCoroutine(RoamRoutine(10f)); // Parameter is how long to wait before going to a new spot
            }
        }

        // Cat stops and investigates when they reach their destination
        else
        {
            if (currCoroutine == null)
            {
                currCoroutine = StartCoroutine(Investigate(3f));
            }
        }
    }
}