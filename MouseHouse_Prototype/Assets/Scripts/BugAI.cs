using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using static CatAIFollow;

public class BugAI : MonoBehaviour
{
    // Radius/Sensing variables
    private const float detectionRadius = 2f;
    private const float roamRadius = 30f;
    private Vector3 lastKnownPlayerPos;
    private Transform currTarget;

    // Speed variables
    private const float chaseSpeed = 1.1f;
    private const float roamSpeed = 0.4f;

    // Objects/components
    public Transform player;
    public Rigidbody bugRb;
    private NavMeshAgent bug;
    public PlayerMovement mousePlayer;
    private Coroutine currCoroutine;
    private Animator animator;

    private float lastAttackTime;
    private float attackCooldown = 6f;

    //Used for billboarding
    public Vector3 lookingDir;

    public float wanderCooldown = 10;
    float lastWanderStart;

    public bugState state;

    public enum bugState
    {
        roaming, 
        hunting
    }

    void Start()
    {
        bug = GetComponent<NavMeshAgent>();
        bugRb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        currTarget = player;
    }

    void Update()
    {
        Billboarding();
        DetermineState();

        switch(state)
        {
            case bugState.hunting:
                BugHunting();
                break;

            case bugState.roaming:
                BugWander();
                break;

            default:
                {
                    if (currCoroutine != null)
                    {
                        StopCoroutine(currCoroutine);
                        currCoroutine = null;
                    }

                    // Update speed
                    UpdateSpeed(0f);
                    Debug.Log("Resorted to roaming fallback");
                }
                break;
        }
    }


    private void DetermineState()
    {
        if (Vector3.Distance(player.position, transform.position) <= detectionRadius)
        {
            state = bugState.hunting;
        }
        else
        {
            state = bugState.roaming;
        }
    }


    public void BugHunting()
    {
        if (!HasDestination() || currTarget == null)
        {
            state = bugState.roaming;
            return;
        }

        // Chase player
        float targDist = Vector3.Distance(transform.position, currTarget.transform.position);
        if (targDist <= detectionRadius)
        {
            // record last known position
            lastKnownPlayerPos = currTarget.transform.position;
        }
        else if (Vector3.Distance(lastKnownPlayerPos, transform.position) < 2 || !HasDestination()
            || Vector3.Distance(bug.pathEndPosition, lastKnownPlayerPos) > 3)
        {
            state = bugState.roaming;
            currTarget = null;
        }

        // Update speed
        UpdateSpeed(chaseSpeed);

        // Set new destination
        bug.SetDestination(lastKnownPlayerPos);
        lookingDir = (bug.steeringTarget - transform.position).normalized;
    }


    void BugWander()
    {

        if (currTarget != null)
        {
            state = bugState.hunting;
        }

        // Check current coroutine
        if (HasDestination() && Time.deltaTime < lastWanderStart + wanderCooldown)
        {
            return;
        }

        // Update speed
        UpdateSpeed(roamSpeed);


        Vector3 newDest = GeneratePoint(transform.position, roamRadius);
        // Only set destination if it's meaningfully different
        if ((newDest - bug.destination).sqrMagnitude > 0.01f)
        {
            bug.SetDestination(newDest);
            //Debug.Log("New Cat Destination Set: " + newDest);
        }
        else
        {
            Debug.Log("GeneratePoint returned fallback origin; retrying next tick.");
        }
        lookingDir = (bug.steeringTarget - transform.position).normalized;

    }


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
        else if (bug != null && NavMesh.SamplePosition(bug.transform.position, out originHit, 1f, NavMesh.AllAreas))
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


    private void UpdateSpeed(float speed)
    {
        //Don't move while in sleeping animations
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("TempEmptyAwakeAnim") && !animator.GetCurrentAnimatorStateInfo(0).IsName("catSleep"))
            bug.speed = speed;
        else
            bug.speed = 0f;
        //Animator can set a parameter as a multiplier on speed. Don't need to change animator playback speed.
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("Trigger event called");
        if (other.CompareTag("Player"))
        {
            AttackMouse();
        }
    }

    void AttackMouse()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }
        lastAttackTime = Time.time;
        //audioSource.PlayOneShot(attackSound);

        // TODO: Adjust for when the bug is underneath the player

        Manager.Manager_.TakeDamage(1);
    }

    void Billboarding()
    {
        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);

        Vector2 camProjSpace = new Vector2(Camera.main.transform.position.x - transform.position.x, Camera.main.transform.position.z - transform.position.z).normalized;
        Vector2 catLookProjSpace = new Vector2(lookingDir.x, lookingDir.z).normalized;

        float angleToCam = Vector2.SignedAngle(camProjSpace, catLookProjSpace);
        angleToCam = (angleToCam + 360) % 360;

        animator.SetFloat("angToCam", angleToCam);
    }


    private bool HasDestination()
    {
        if (bug == null) return false;
        if (bug.pathPending) return true;
        if (!bug.hasPath) return false;
        return bug.remainingDistance > bug.stoppingDistance + 0.01f;
    }
}
