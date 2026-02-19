using UnityEngine;
using UnityEngine.AI;

public class CatAIFollow : MonoBehaviour
{
    public float radius = 2f;
    public Transform player;

    private NavMeshAgent cat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cat = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
         float distance = Vector3.Distance(transform.position,player.position);
        if(distance <= radius){cat.SetDestination(player.position);}
    }

    void DrawRad(){
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
