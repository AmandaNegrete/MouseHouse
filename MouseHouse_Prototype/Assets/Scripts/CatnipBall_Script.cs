using UnityEngine;

public class CatnipBall_Script : MonoBehaviour
{
    public float force = .3f;
    public float Cat_Move_Radius = 1f;
    Rigidbody body;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    void OnCollisionEnter(Collision hit){
        if(hit.gameObject.CompareTag("Player")){
            //pointing from the player 2 the ball
            //forward means can only push forward-TEMPORARY FIX!
            Vector3 direction = hit.transform.forward;
            //https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ForceMode.Impulse.html
            body.AddForce(direction * force, ForceMode.Impulse);
        }
        MoveCat();
    }
    void MoveCat(){
        GameObject cat = GameObject.FindGameObjectWithTag("Finish");
        if(cat == null){ return;}
        else{
            CatAIFollow catAI = cat.GetComponent<CatAIFollow>();
            //make target the ball
            catAI.target_Set(transform );
        }
    }
}
