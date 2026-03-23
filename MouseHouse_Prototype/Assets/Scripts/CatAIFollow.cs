using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CatAIFollow : MonoBehaviour
{
    // Radius/Sensing variables
    private const float detectionRadius = 1f;
    private const float catnipRadius = 2f;
    private const float roamRadius = 7f;
    private const float flipThreshold = 0.05f;
    private Vector3 lastKnownPlayerPos;

    // Speed variables
    private const float chaseSpeed = 0.9f;
    private const float roamSpeed = 0.5f;
    private const float roamInterval = 10f;
    private const float investigateInterval = 4f;
    private const float launchForce = 20f;

    // Delimeters
    private float playerTraveledAwake = 3f; // How far the player can travel before the cat wakes up

    // Boolean flags
    private bool asleep = true;
    private bool isChasingPlayer = false;
    private bool goingToLastKnown = false;
    private bool isRoaming = false;
    private bool isIdle = false;
    private bool facingRight = true;
    private bool calledCatnipRoutine = false;

    // Objects/Components
    public Transform player;
    public Transform target;
    public Sprite catSprite;
    public Rigidbody catRb;
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
        catRb = GetComponent<Rigidbody>();
        target = player;
        art.sprite = catSprite;
    }


    // Update is called once per frame
    void Update()
    {
        // Cat sleep state, check idle triggers
        if (asleep)
        {
            PlaySleepAnimation();
            DetectCatnip();
            DetectMouseMovement();
        }

        // Cat active state 
        else
        {
            AnimateCat();
            CatMovement();
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
            PlayAwakenAnimation();
            asleep = false;
            isRoaming = true;
            cat.SetDestination(catnip.transform.position);
        }
        target = player.transform;
    }


    private void DetectMouseMovement()
    {
        Debug.Log("PlayerDist: " + mousePlayer.distTraveled);
        if (mousePlayer.distTraveled >= playerTraveledAwake)
        {
            PlayAwakenAnimation();
            asleep = false;
            isRoaming = true;
            cat.SetDestination(player.transform.position);
        }
    }

    public void CatMovement()
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
            if (currCoroutine != null && !calledCatnipRoutine)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            // Update speed
            UpdateSpeed(chaseSpeed);

            // Set new destination
            // Stay at catnip then leave after a while
            //Debug.Log("Curr Coroutine: " + currCoroutine + "\nDist: " + Vector3.Distance(cat.transform.position, catnip.transform.position));
            if (catnipDist <= 0.3f && currCoroutine == null)
            {
                calledCatnipRoutine = true;
                currCoroutine = StartCoroutine(LeaveCatnip(5f));
                Debug.Log("Catnip routine called");
            }
            else if(!calledCatnipRoutine)
            {
                cat.SetDestination(catnip.transform.position);
                Debug.Log("set destination to catnip");
            }
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


    IEnumerator LeaveCatnip(float time)
    {
        yield return new WaitForSeconds(time);

        // Randomly choose to either launch the catnip or move away from it
        bool move = Random.Range(0, 2) == 1;
        // Move away from the catnip
        if (move)
        {
            // Set destination to be outside catnip radius
            bool insideRadius = true;
            int maxTries = 30;
            int tries = 0;
            while (insideRadius)
            {
                Vector3 point = GeneratePoint(transform.position, roamRadius);
                tries++;
                if (Vector3.Distance(point, catnip.transform.position) > catnipRadius)
                {
                    insideRadius = false;
                    cat.SetDestination(point);

                    // Give it time to move out of the area, then reset flags
                    yield return new WaitForSeconds(5f);
                }

                // Fallback is that the catnip disappears
                else if (tries >= maxTries)
                {
                    catnip.SetActive(false);

                }
            }
            currCoroutine = null;
            isRoaming = true;
            calledCatnipRoutine = false;
        }

        // Launch the catnip in a random direction
        else
        {
            Vector3 direction = Random.onUnitSphere * 4f;
            Debug.Log("Launching Catnip: " + direction);
            catnip.GetComponent<Rigidbody>().AddForce(direction * launchForce);
            currCoroutine = null;
        }
    }


    public void AnimateCat()
    {
        bool catmovement = cat.velocity.magnitude > .1f;
        animator.SetBool("catmovement", catmovement);

        // Flip sprite if needed
        Vector3 velocity = cat.velocity;
        if (Mathf.Abs(velocity.x) > flipThreshold)
        {
            bool shouldFaceRight = velocity.x > 0f;
            if (shouldFaceRight != facingRight)
            {
                FlipSprite();
            }
        }

        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);
    }

    private void FlipSprite()
    {
        facingRight = !facingRight;
        art.flipX = !art.flipX;
    }


    public void target_Set(Transform new_target)
    {
        target = new_target;
        //TODO 
        //make go back to player after amount of time
    }

    private void PlaySleepAnimation()
    {
        // Add animations here
        Debug.Log("Playing Sleep Animation");
    }

    private void PlayAwakenAnimation()
    {
        // Add animations here
        Debug.Log("Playing awaken animation");
    }
}