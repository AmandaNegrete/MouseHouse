using UnityEngine;
using UnityEngine.AI;

public class CatAIFollow : MonoBehaviour
{
    public float radius = 1f;
    public Transform player;

    private NavMeshAgent cat;
    private SpriteRenderer art;
    private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cat = GetComponent<NavMeshAgent>();
        art = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
         float distance = Vector3.Distance(transform.position,player.position);
        if(distance <= radius){cat.SetDestination(player.position);}

        bool catmovement = cat.velocity.magnitude > .1f;
        animator.SetBool("catmovement", catmovement);
   

    }


}
