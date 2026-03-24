using UnityEngine;

public class CatnipBall_Script : MonoBehaviour
{
    public float force = .3f;
    public float Cat_Move_Radius = 1f;
    Rigidbody body;

    //emission gloww//
    public float glowOffset = 20f;
    public Color ballColor = Color.yellow;
    public float intensity = 100f;

    private Material ballM;
    private bool glowing = false;

    private bool playerFoundBall = false; 
    private float timer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody>();
        ballM = GetComponent<Renderer>().material;
        ballM.DisableKeyword("_EMISSION");
        Invoke(nameof(StartGlow), glowOffset);
    }

    void Update(){
        if(!playerFoundBall && !glowing){
            timer += Time.deltaTime;
            if(timer >= glowOffset){
                StartGlow();
            }
        }

        if(glowing){
            float pulse = Mathf.PingPong(Time.time *4f, intensity);
            ballM.SetColor("_EmissionColor", ballColor * pulse);
        }
    }
    void StartGlow(){
        glowing = true;
        ballM.EnableKeyword("_EMISSION");
        ballM.SetColor("_EmissionColor", ballColor * intensity *10f);
    }
    void OnCollisionEnter(Collision hit){
        if(hit.gameObject.CompareTag("Player")){
            playerFoundBall= true;
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
