using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CatAIFollow : MonoBehaviour
{
    // Radius/Sensing variables
    private const float detectionRadius = 3f;
    private const float catnipRadius = 2f;
    private const float roamRadius = 30f;
    private const float flipThreshold = 0.05f;
    private Vector3 lastKnownPlayerPos;

    // Speed variables
    private const float chaseSpeed = 1.5f;
    private const float roamSpeed = 0.6f;
    private const float roamInterval = 15f;
    private const float investigateInterval = 4f;
    private const float launchForce = 20f;

    // Delimeters
    private float playerTraveledAwake = 1f; // How far the player can travel before the cat wakes up

    // Boolean flags
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

    private float lastAttackTime;
    private float attackCooldown = 1f; // Time in seconds between attacks

    public CatState state;

    //Used for billboarding
    public Vector3 lookingDir;

    //Amount of aggro needed to make cat change target.
    public float neededAggro = 3;

    public List<CatTarget> targetsInScene = new List<CatTarget>();
    public CatTarget currTarget;

    public float wanderCooldown = 10;
    float lastWanderStart;


    public enum CatState
    {
        idling,
        sleeping,
        hunting,
        wandering,
        acting
    }

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
        animator.SetFloat("catmovement", cat.velocity.magnitude / 5f);
        UpdateTargetsList();
        Billboarding();

        switch (state)
        {
            case CatState.sleeping:
                SleepBehavior();
                break;
            case CatState.hunting:
                {
                    CatHunting();
                    if (currTarget != null && Vector3.Distance(currTarget.transform.position, transform.position) < 1 
                        && (currTarget.rb != null && currTarget.rb.linearVelocity.magnitude < 2))
                    {
                        currTarget.lastAct = Time.time;
                        state = CatState.acting;
                    }
                }
                break;
            case CatState.wandering:
                CatWander();
                break;

            case CatState.acting:
                ActOnTarget();
                break;

            case CatState.idling:
                CatIdle();
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


    void UpdateTargetsList()
    {
        //Currently ignore this, but this is meant to make targeting "Sticky" and prevent switching between targets too often.
        if(true || GetTargetWeight(currTarget) < neededAggro)
        {
            CatTarget highestAggro = null;
            for(int i = 0; i < targetsInScene.Count; i++)
            {
                if (Vector3.Distance(targetsInScene[i].transform.position, transform.position) > detectionRadius)
                    continue;

                if(highestAggro == null || GetTargetWeight(highestAggro) < GetTargetWeight(targetsInScene[i]))
                {
                    highestAggro = targetsInScene[i];
                }
                //Debug.Log(GetTargetWeight(targetsInScene[i]));
            }
            if (highestAggro == null || GetTargetWeight(highestAggro) < neededAggro)
                return;
            currTarget = highestAggro;
            target = currTarget.transform;
            cat.SetDestination(target.position);
        }
    }

    float GetTargetWeight(CatTarget targ)
    {
        if (targ == null)
            return 0;

        float returnAggro = targ.FinDistractionAmount * Mathf.Clamp01(((targ.DetectionDistance - Vector3.Distance(targ.transform.position, transform.position)))/targ.DetectionDistance);
        if (targ == currTarget)
            returnAggro += 1;
        return returnAggro;
    }

    private bool HasDestination()
    {
        if (cat == null) return false;
        if (cat.pathPending) return true;
        if (!cat.hasPath) return false;
        return cat.remainingDistance > cat.stoppingDistance + 0.01f;
    }


    public void CatHunting()
    {
        if(!HasDestination() || currTarget == null)
        {
            state = CatState.wandering;
            return;
        }

        // Chase player
        float targDist = Vector3.Distance(transform.position, currTarget.transform.position);
        if (targDist <= detectionRadius)
        {
            // record last known position
            lastKnownPlayerPos = currTarget.transform.position;
        }
        else if(Vector3.Distance(lastKnownPlayerPos, transform.position) < 2 || !HasDestination()
            ||Vector3.Distance(cat.pathEndPosition, lastKnownPlayerPos) > 3)
        {
            state = CatState.wandering;
            currTarget = null;
        }

        // Update speed
        UpdateSpeed(chaseSpeed);

        // Set new destination
        cat.SetDestination(lastKnownPlayerPos);
        lookingDir = (cat.steeringTarget - transform.position).normalized;
    }

    void CatWander()
    {

        if (currTarget != null)
        {
            state = CatState.hunting;
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
        if ((newDest - cat.destination).sqrMagnitude > 0.01f)
        {
            cat.SetDestination(newDest);
            //Debug.Log("New Cat Destination Set: " + newDest);
        }
        else
        {
            Debug.Log("GeneratePoint returned fallback origin; retrying next tick.");
        }
        lookingDir = (cat.steeringTarget - transform.position).normalized;

    }
    void SleepBehavior()
    {
        //Up needed noise/distraction to wake up
        neededAggro = 7f;
        //Play sleep anim
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != "catSleep")
            animator.Play("catSleep");

        if (currTarget != null)
        {
            //wake up
            animator.SetTrigger("catWake");
            state = CatState.hunting;
            neededAggro = 0;
        }
    }

    void CatIdle()
    {
        // Update speed
        UpdateSpeed(0f);
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


    private void UpdateSpeed(float speed)
    {
        cat.speed = speed;
        //Animator can set a parameter as a multiplier on speed. Don't need to change animator playback speed.
    }

    void ActOnTarget()
    {
        UpdateSpeed(0);
        if (Vector3.Distance(currTarget.transform.position, transform.position) > 2.5f)
        {
            state = CatState.hunting;
            return;
        }
        

        if(currTarget != null)
        {
            currTarget.OnInteract(this);
        }

    }

    public void target_Set(Transform new_target)
    {
        target = new_target;
        //Left in case of needing additional behavior
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && state != CatState.sleeping)
        {
            AttackMouse();
        }
    }

    void AttackMouse()
    {
        if(Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }
        lastAttackTime = Time.time;
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

}