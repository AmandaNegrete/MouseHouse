using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CatAIFollow : MonoBehaviour
{
    public float detectionRadius = 1f;
    private float catnipRadius = 2f;
    public float roamRadius = 10f;
    private float chaseSpeed = 1f;
    private float roamSpeed = 0.5f;
    private float playerTraveledAwake = 10f; // How far the player can travel before the cat wakes up
    private bool asleep = true;
    private bool isRoaming = false;
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
        target = player;
        animator = GetComponent<Animator>();
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
            CatMovement();
        }
    }


    private bool HasDestination()
    {
        if (cat.remainingDistance > 0)
        {
            return true;
        }
        return false;
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


    public void CatMovement()
    {
        // Chase player if within radius
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRadius)
        {
            cat.speed = chaseSpeed;
            animator.speed = chaseSpeed;
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            cat.SetDestination(player.position);
        }

        // Cat is roaming
        else
        {
            cat.speed = roamSpeed;
            animator.speed = roamSpeed;
            if (isRoaming && currCoroutine == null && !HasDestination())
            {
                currCoroutine = StartCoroutine(RoamRoutine(10f)); // Parameter is how long to wait before going to a new spot
            }
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
        // try a small radius first, then expand
        if (NavMesh.SamplePosition(origin, out originHit, 1f, NavMesh.AllAreas))
        {
            navOrigin = originHit.position;
        }
        else if (cat != null && NavMesh.SamplePosition(cat.transform.position, out originHit, 1f, NavMesh.AllAreas))
        {
            // fallback to the agent's position if the MonoBehaviour transform isn't on the NavMesh
            navOrigin = originHit.position;
        }
        else if (NavMesh.SamplePosition(origin, out originHit, sampleMaxDistance, NavMesh.AllAreas))
        {
            // last-resort: try a larger radius
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

        // If we couldn't find a valid roaming point, return the navOrigin (on-NavMesh fallback)
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

    IEnumerator Investigate(float invTime)
    {
        yield return new WaitForSeconds(invTime);
        currCoroutine = null;
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
}

