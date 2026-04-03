    using UnityEngine;
    using UnityEngine.InputSystem;


    public class PlayerMovement : MonoBehaviour
    {
    // PUBliC VARIABLES
    public float moveSpeed = 4f;
    public float normalSpeed = 4f;
    public float crawlSpeed = 1.5f;

    public float runSpeed = 7f;

    public float climbCheckDistance = 1.5f;

    public float mouseSensitivity = 100f;
    //force of gravity is 9.81 downwards
    public float gravity = -9.81f;
    public float crawlHeight = .5f;
    public float regHeight = 2f;
    public float jumpHeight = 3f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Transform Playercamera;
    public Transform camOffsets;
    bool isClimbing = false; 
    bool isRunning; 
    RaycastHit climbfound;
/// ////////////

    float xRotation = 0f;
    Vector3 velocity;
    CharacterController controller;
    Animator animator;


    public PlayerInput controlScheme;


    InputAction moveAction;
    InputAction lookAction;
    InputAction pauseAction;
    InputAction crawlAction;
    InputAction climbAction;
    InputAction runAction;
    public InputAction jumpAction;
    bool isCrawling;


    [HideInInspector]
    public static PlayerMovement main;
    public float bobbingIntensity = 1;
    public float bobbingSpeed = .25f;
    //Smooth in and out intensity based on speed.
    public float currBobbingIntensity = 0;
    float bobVel;

    // Used to disable cat idle state
    public float distTraveled = 0;
    private Vector3 prevPosition;
    public GameObject cat;
    private float detectionRadius = 2f;
    private Vector3 climbPoint;

    public MouseHandsHandler mouseHands;

    void Start()
    {
        main = this;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        moveAction = controlScheme.actions["Move"];
        lookAction = controlScheme.actions["Look"];
        jumpAction = controlScheme.actions["Jump"];
        pauseAction = controlScheme.actions["Pause"];
        crawlAction = controlScheme.actions["Crawl"];
        climbAction = controlScheme.actions["Climb"];
        runAction = controlScheme.actions["Run"];
        prevPosition = transform.position;

    }


    void Update()
    {
        //need to split up into calls
        //Do not move when paused. Could replace with a menu check or bool
        if (Time.deltaTime > 0)
        {
            HandleMouseLook();
            HandleMovement();
        }
        HandleMenuInputs();


        // Handle the distance traveled variable
        UpdateDistTraveled();

        mouseHands.enablePawsMovement = controller.isGrounded;
    }


    void HandleMenuInputs()
    {
        if(pauseAction.WasPressedThisFrame())
        {
            PauseMenuManager.main.TogglePause();
        }
    }


    void HandleMouseLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity/1000;


        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);


        //https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Quaternion.html
        Playercamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x);
    }


    void HandleMovement()
    {
        isClimbingCheck();
        if(isClimbing == true)
        {
            HandleClimbing();
            return;
        }
        bool Mouseground = controller.isGrounded;
        
        if(Mouseground && velocity.y < 0){velocity.y = -2f;}


        Vector2 moveInput = moveAction.ReadValue<Vector2>();


        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);


        //Bobbing anim
        float moveMagnitude = (moveInput * moveSpeed).y;
        currBobbingIntensity = Mathf.SmoothDamp(currBobbingIntensity, moveMagnitude * bobbingIntensity, ref bobVel, .2f);
        if (!Mouseground)
            currBobbingIntensity = 0;
        camOffsets.transform.localPosition = new Vector3(camOffsets.transform.localPosition.x,
            (Mathf.Sin(Time.time * bobbingSpeed * moveMagnitude) + 1) / 2f * currBobbingIntensity,
            camOffsets.transform.localPosition.z);
        //End bobbing
        

        //Jumping and gravity
        if (jumpAction.WasPressedThisFrame() && Mouseground)
        {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        //end jumping and gravity

        //crawling 
        isCrawling = crawlAction.IsPressed();
        isRunning = runAction.IsPressed();
        if (isCrawling)
        {
            moveSpeed = crawlSpeed;
            controller.height = crawlHeight;
        }
        else if (isRunning)
        {
            moveSpeed = runSpeed;
        }
        else{
            moveSpeed = normalSpeed;
            controller.height = regHeight;
        }
    
    }


    private void UpdateDistTraveled()
    {
        float distThisFrame = Vector3.Distance(transform.position, prevPosition);
        float catDist = Vector3.Distance(cat.transform.position, transform.position);
        if (catDist <= detectionRadius)
        {
            distTraveled += distThisFrame;
        }
        prevPosition = transform.position;
    }
    /*
    bool isObjectClimbable(Collider other)
    {
        // Check if the object has the climb tag
        //done in Unity editor for now but could do a raycast method
        return other.CompareTag("Climbable");
    }
    */
    void isClimbingCheck()
    {
        if (climbAction.IsPressed())
        {
            Ray path = new Ray(Playercamera.position, Playercamera.forward);
            if(Physics.Raycast(path, out climbfound, climbCheckDistance)){
                if (climbfound.collider.CompareTag("Climbable"))
                {
                    isClimbing = true;
                    velocity.y = 0;
                    transform.forward = -climbfound.normal; //face the norm vector of the wall 
                    return;
                }
            }
        }
        isClimbing = false; 

    }
    //todo: handle climbign lol 
    void HandleClimbing()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float move_vert = moveInput.y;
        float move_horiz = moveInput.x;
        //climb direction for movement- hopefully facing object
        Vector3 climbDire = (Vector3.up * move_vert) + (transform.right *move_horiz);
        controller.Move(climbDire *normalSpeed *Time.deltaTime);

        if (jumpAction.WasPressedThisFrame())
        {
            isClimbing = false;
            //random 2f lol
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

}




