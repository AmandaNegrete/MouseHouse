using UnityEngine;
using UnityEngineAI;

public class CatAIFollow : MonoBehaviour
{
    public float radius = 2f;
    public Transform player;

    private UnityEngine.AI.NavMeshAgent cat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cat = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        int distance = Vector3.Distance(transform.position,player.position);
        if(distance <= radius){cat.SetDestination(player.position);}
    }

    void DrawRad(){
        Gizmos.color = color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
