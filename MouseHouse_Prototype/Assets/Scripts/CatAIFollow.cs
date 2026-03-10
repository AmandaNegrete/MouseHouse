using UnityEngine;
using UnityEngine.AI;

public class CatAIFollow : MonoBehaviour
{
    public float radius = 1f;
    public Transform player;
    public Transform target;

    private NavMeshAgent cat;
    private SpriteRenderer art;
    public  Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cat = GetComponent<NavMeshAgent>();
        art = GetComponent<SpriteRenderer>();
        target = player;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(transform.position,target.position);
        if(distance <= radius){cat.SetDestination(target.position);}

        bool catmovement = cat.velocity.magnitude > .1f;
        animator.SetBool("catmovement", catmovement);

        animator.transform.LookAt(Camera.main.transform, Vector3.up);
        animator.transform.rotation = Quaternion.Euler(0, animator.transform.rotation.eulerAngles.y, 0);
    }

    public void target_Set(Transform new_target){
        target = new_target;
        //TODO 
        //make go back to player after amount of time
    }
}
