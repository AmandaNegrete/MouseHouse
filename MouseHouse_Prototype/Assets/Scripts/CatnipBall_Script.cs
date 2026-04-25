using UnityEngine;

public class CatnipBall_Script : CatTarget
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

    public float launchForce = 20f;

    private Vector3 startPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody>();
        startPosition = transform.position;
        ballM = GetComponent<Renderer>().material;
        ballM.DisableKeyword("_EMISSION");
        Invoke(nameof(StartGlow), glowOffset);
    }

    protected override void Update(){
        base.Update();
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

        //Move ball back to play area if out of bounds
        if(transform.position.y < -50)
        {
            transform.position = startPosition;
            rb.linearVelocity = new Vector3();
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
        //MoveCat();
    }

    public override void OnInteract(CatAIFollow cat)
    {
        base.OnInteract(cat);

        if (lastAct + actCooldown > Time.time)
            return;

        lastAct = Time.time;
        //Prevent sliding while being interacted with.
        rb.linearVelocity = new Vector3();
        // Randomly choose to either launch the catnip or move away from it
        bool move = Random.Range(0, 2) == 1;
        // Move away from the catnip
        if (move)
        {
            //Leave catnip
            cat.state = CatAIFollow.CatState.wandering;

        }

        // Launch the catnip in a random direction
        else
        {
            Vector3 direction = Random.onUnitSphere * 4f;
            rb?.AddForce(direction * launchForce);
            cat.state = CatAIFollow.CatState.wandering;
        }
    }
}
