using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CatAIFollow : MonoBehaviour
{
    // Radius/Sensing variables
    private const float detectionRadius = 6f;
    private const float catnipRadius = 3f;
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

    public enum CatState
    {
        idling,
        sleeping,
        hunting,
        wandering
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

        UpdateTargetsList();
        Billboarding();

        // Cat sleep state, check idle triggers
        if (state == CatState.sleeping)
        {
            SleepBehavior();
        }
        else
        {
            CatMovement();
        }
    }


    void UpdateTargetsList()
    {
        if(true || currTarget == null || GetTargetWeight(currTarget) < neededAggro)
        {
            CatTarget highestAggro = null;
            for(int i = 0; i < targetsInScene.Count; i++)
            {
                if(highestAggro == null || GetTargetWeight(highestAggro) < GetTargetWeight(targetsInScene[i]))
                {
                    highestAggro = targetsInScene[i];
                }
                Debug.Log(GetTargetWeight(targetsInScene[i]));
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


    public void CatMovement()
    {
        animator.SetFloat("catmovement", cat.velocity.magnitude);

        // Chase player
        float targDist = Vector3.Distance(transform.position, currTarget.transform.position);
        if (targDist <= detectionRadius)
        {
            // record last known position
            lastKnownPlayerPos = currTarget.transform.position;
        }
        // Player just left detection radius
        else if (state == CatState.hunting)
        {

            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
            }

            // Update speed
            UpdateSpeed(chaseSpeed);

            // Set new destination
            cat.SetDestination(lastKnownPlayerPos);
            lookingDir = cat.nextPosition - transform.position;
        }
        // Roam
        else if (state == CatState.wandering)
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
        else if (state == CatState.idling)
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
        while (state == CatState.wandering)
        {
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
            yield return new WaitForSeconds(waitTime);
        }
        currCoroutine = null;
    }

    private void UpdateSpeed(float speed)
    {
        cat.speed = speed;
        //Animator can set a parameter as a multiplier on speed. Don't need to change animator playback speed.
    }

    IEnumerator Investigate(float invTime)
    {
        state = CatState.idling;
        yield return new WaitForSeconds(invTime);
        currCoroutine = null;
        state = CatState.wandering;
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
            state = CatState.wandering;
            calledCatnipRoutine = false;
        }

        // Launch the catnip in a random direction
        else
        {
            Vector3 direction = Random.onUnitSphere * 4f;
            //Debug.Log("Launching Catnip: " + direction);
            catnip.GetComponent<Rigidbody>().AddForce(direction * launchForce);
            currCoroutine = null;
        }
    }




    public void target_Set(Transform new_target)
    {
        target = new_target;
        //TODO 
        //make go back to player after amount of time
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

        Vector2 camProjSpace = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z);
        Vector2 catLookProjSpace = new Vector2(lookingDir.x, lookingDir.z);

        float angleToCam = Vector2.SignedAngle(Vector2.right, camProjSpace) + Vector2.SignedAngle(Vector2.right, catLookProjSpace);
        angleToCam = (angleToCam + 360) % 360;

        animator.SetFloat("angToCam", angleToCam);

    }

    void SleepBehavior()
    {
        //Up needed noise/distraction to wake up
        neededAggro = 2.5f;
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
}