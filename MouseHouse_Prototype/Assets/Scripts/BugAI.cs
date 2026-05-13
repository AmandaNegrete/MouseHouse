using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;
using static CatAIFollow;

public class BugAI : MonoBehaviour
{
    // Radius/Sensing variables
    private const float detectionRadius = 2f;
    private const float roamRadius = 30f;
    private Vector3 lastKnownPlayerPos;
    private Transform currTarget;
    private Transform bugTransform;

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
    private Slider healthbarSlider;
    private GameObject healthbar;

    // Body parts
    [SerializeField] private GameObject visuals;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject back;
    [SerializeField] private GameObject legs;
    [SerializeField] private GameObject eyeClosed;
    [SerializeField] private GameObject front;

    private float lastAttackTime;
    private float attackCooldown = 6f;

    //Used for billboarding
    public Vector3 lookingDir;

    public float wanderCooldown = 10;
    float lastWanderStart;

    public bugState state;

    public int health = 6;
    private float damageTimer = 0;
    private float damageCooldown = 3f;
    private bool isDead = false;

    private float flutterTimer = 0;
    private float lastHitTime;
    public float hitCooldown = 1f;
    public ClickPickup playerPickup;
    public enum bugState
    {
        roaming, 
        hunting, 
        dead
    }

    void Start()
    {
        bug = GetComponent<NavMeshAgent>();
        bugTransform = GetComponent<Transform>();
        bugRb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        //currTarget = player;
        healthbarSlider = GetComponentInChildren<Slider>();
        healthbarSlider.value = health;
        healthbar = transform.GetChild(1).gameObject;

        // Body part assignment
        visuals = transform.GetChild(0).gameObject;
        body = visuals.transform.GetChild(0).gameObject;
        back = visuals.transform.GetChild(1).gameObject;
        legs = visuals.transform.GetChild(2).gameObject;
        eyeClosed = visuals.transform.GetChild(3).gameObject;
        front = visuals.transform.GetChild(4).gameObject;
        front.SetActive(false);
        eyeClosed.SetActive(false);
    }

    void Update()
    {
        Billboarding();
        DetermineState();

        switch(state)
        {
            case bugState.dead:
                if (currCoroutine != null) StopCoroutine(currCoroutine);
                return;
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
        AnimateBug();
    }


    private void DetermineState()
    {
        if (isDead)
        {
            state = bugState.dead;
            return;
        }
        else if (Vector3.Distance(player.position, transform.position) <= detectionRadius)
        {
            state = bugState.hunting;
            currTarget = player;
            animator.SetBool("bugHunting", true);
            SwitchPerspective();
        }
        else
        {
            state = bugState.roaming;
            currTarget = null;
            animator.SetBool("bugHunting", false);
            SwitchPerspective();
        }
    }


    public void BugHunting()
    {
        if (!HasDestination() || currTarget == null)
        {
            state = bugState.roaming;
            animator.SetBool("bugHunting", false);
            SwitchPerspective();
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
            animator.SetBool("bugHunting", false);
            SwitchPerspective();
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
            animator.SetBool("bugHunting", true);
            SwitchPerspective();
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
        ////Don't move while in sleeping animations
        //if (!animator.GetCurrentAnimatorStateInfo(0).IsName("TempEmptyAwakeAnim") && !animator.GetCurrentAnimatorStateInfo(0).IsName("catSleep"))
        //    bug.speed = speed;
        //else
        //    bug.speed = 0f;
        //Animator can set a parameter as a multiplier on speed. Don't need to change animator playback speed.
        bug.speed = speed;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !isDead)
        {
            AttackMouse();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stove"))
        {
            BugTakeDamage(1);
        }
        if (other.CompareTag("Spoon"))
        {
            BugTakeDamage(1);
        }
    }

    void AttackMouse()
    {
        //Adjust for when the bug is underneath the player
        if (Mathf.Abs(bugTransform.position.y - player.transform.position.y) > 0.08f) return;

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }
        lastAttackTime = Time.time;
        animator.SetBool("flutter", true);
        flutterTimer = 0;
        Manager.Manager_.TakeDamage(1);
    }

    void Billboarding()
    {
        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);

        if (healthbar != null)
        {
            healthbar.transform.LookAt(Camera.main.transform, Vector3.up);
            healthbar.transform.rotation = Quaternion.Euler(0, healthbar.transform.rotation.eulerAngles.y, 0);
        }

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

    private void BugTakeDamage(int damageTaken)
    {
        health -= damageTaken;
        if (healthbarSlider != null) healthbarSlider.value = health;

        if (health <= 0)
        {
            Destroy(healthbar);
            isDead = true;
            state = bugState.dead;
            eyeClosed.SetActive(true);
            body.SetActive(true);
            back.SetActive(true);
            legs.SetActive(true);
            front.SetActive(false);
            animator.SetBool("bugMoving", false);
            animator.SetBool("flutter", false);
            bug.isStopped = true;
        }
    }   
    
   

    private void AnimateBug()
    {
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
        //if (stateInfo.IsName("flutter") && stateInfo.normalizedTime >= 1f)
        //{
        //    animator.SetBool("flutter", false);
        //}

        flutterTimer += Time.deltaTime;
        animator.SetFloat("bugMovement", bug.velocity.magnitude / 3f);

        // Make the bug move
        if (bug.velocity.magnitude > 0)
        {
            animator.SetBool("bugMoving", true);
        }
        else
        {
            animator.SetBool("bugMoving", false);
        }

        // Flutter every 15 seconds
        if (flutterTimer >= 15f && body.activeSelf)
        {
            animator.SetTrigger("flutter");
            flutterTimer = 0f;
        }
    }

    private void SwitchPerspective()
    {
        // Switch to front
        if (body.activeSelf && state == bugState.hunting)
        {
            body.SetActive(false);
            back.SetActive(false);
            legs.SetActive(false);
            front.SetActive(true);
        }

        // Switch to side
        else if (front.activeSelf && state == bugState.roaming)
        {
            front.SetActive(false);
            body.SetActive(true);
            back.SetActive(true);
            legs.SetActive(true);
        }
    }
}
