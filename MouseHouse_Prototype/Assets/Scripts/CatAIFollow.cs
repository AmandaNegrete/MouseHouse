using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CatAIFollow : MonoBehaviour
{
    public float chaseRadius = 1f;
    public float roamRadius = 10f;
    private float chaseSpeed = 1f;
    private float roamSpeed = 0.5f;
    [SerializeField] private bool asleep = false;
    [SerializeField] private bool isRoaming = true;
    public Transform player;
    public Sprite catSprite;

    private NavMeshAgent cat;
    private SpriteRenderer art;
    public Animator animator;

    private Coroutine roamCoroutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cat = GetComponent<NavMeshAgent>();
        art = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        art.sprite = catSprite;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for catnip within radius
        if (!asleep)
        {
            AnimateCat();
            CatMovement();
        }
    }

    public void CatMovement()
    {
        // Cat is asleep
        if (asleep)
        {
            return;
        }

        // Chase player if within radius
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= chaseRadius)
        {
            cat.speed = chaseSpeed;
            animator.speed = chaseSpeed;
            if (roamCoroutine != null)
            {
                StopCoroutine(roamCoroutine);
                roamCoroutine = null;
            }

            cat.SetDestination(player.position);
        }

        // Cat is roaming
        else
        {
            cat.speed = roamSpeed;
            animator.speed = roamSpeed;
            if (isRoaming && roamCoroutine == null)
            {
                roamCoroutine = StartCoroutine(RoamRoutine(5f)); // Parameter is how long to wait before going to a new spot
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
        roamCoroutine = null;
    }

    public void AnimateCat()
    {
        bool catmovement = cat.velocity.magnitude > .1f;
        animator.SetBool("catmovement", catmovement);
        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);
    }


    // Original code for cat movement
    public void ChasePlayerV1()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= chaseRadius) { cat.SetDestination(player.position); }

        bool catmovement = cat.velocity.magnitude > .1f;
        animator.SetBool("catmovement", catmovement);

        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);
    }
}

