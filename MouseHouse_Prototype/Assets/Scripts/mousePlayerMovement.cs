    using UnityEngine;
    using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


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
    ///
    
    public bool isClimbing = false; 
    public bool isRunning = false; 
    public bool isEating = false;
    RaycastHit climbfound;
    RaycastHit foodfound;
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
    InputAction eatAction;
    public InputAction jumpAction;
    bool isCrawling;
    bool SpeedBoost;


    [HideInInspector]
    public static PlayerMovement main;
    public float bobbingIntensity = 1;
    public float bobbingSpeed = .25f;
    //Smooth in and out intensity based on speed.
    public float currBobbingIntensity = 0;

    public float timerBoost = 0f;
    float bobVel;

    // Used to disable cat idle state
    public float distTraveled = 0;
    private Vector3 prevPosition;
    public GameObject cat;
    private float detectionRadius = 2f;
    private Vector3 climbPoint;

    public MouseHandsHandler mouseHands;

    private float fallStartHorz;

    private bool wasGrounded;
    bool climbInterrupted = false;
    private float damageTimer = 0f;
    private IndicatorManager indicatorManager;

    void Awake()
    {
        indicatorManager = Object.FindFirstObjectByType<IndicatorManager>();
        if (indicatorManager == null)
        {
            Debug.LogWarning("Indicator manager not found. ");
        }

        main = this;
    }
    void Start()
    {
        
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        //
        moveAction = controlScheme.actions["Move"];
        lookAction = controlScheme.actions["Look"];
        jumpAction = controlScheme.actions["Jump"];
        pauseAction = controlScheme.actions["Pause"];
        ///
        crawlAction = controlScheme.actions["Crawl"];
        climbAction = controlScheme.actions["Climb"];
        runAction = controlScheme.actions["Run"];
        ///
        eatAction = controlScheme.actions["Eat"];
        prevPosition = transform.position;

    }
    

    void Update()
    {
        //need to split up into calls
        //Do not move when paused. Could replace with a menu check or bool
        if (Time.deltaTime > 0)
        {
            HandleMovement();
            HandleMouseLook();
        }
        HandleMenuInputs();


        // Handle the distance traveled variable
        UpdateDistTraveled();
    
      if (mouseHands != null)
        {
        mouseHands.enablePawsMovement = controller.isGrounded || isClimbing;
        }
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
        if(isClimbing)
        {
            if(Vector3.SignedAngle(transform.forward, Vector3.up, transform.up) < 30 + lookInput.x
                && Vector3.SignedAngle(transform.forward, Vector3.up, transform.up) > -30 + lookInput.x)
            {
                transform.Rotate(Vector3.up * lookInput.x);
            }
        }
        else
            transform.Rotate(Vector3.up * lookInput.x);
    }


    void HandleMovement()
    {
        isClimbingCheck();

        //Handle fall damage.
        bool Mouseground = controller.isGrounded || isClimbing;
        if (Mouseground && !wasGrounded)
        {
            float distance_fallen = fallStartHorz - transform.position.y;

            if (distance_fallen > 2f)
            {
                Manager.Manager_.TakeDamage(1);
            }
        }

        //leaving
        if (!Mouseground && wasGrounded)
        {
            fallStartHorz = transform.position.y;
        }

        wasGrounded = Mouseground;


        if (isClimbing == true)
        {
            HandleClimbing();
            return;
        }

        if (Mouseground && velocity.y < 0){velocity.y = -2f;}


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
            //comment
        }
        else{
            moveSpeed = normalSpeed;
            controller.height = regHeight;
        }
        isEating = eatAction.IsPressed();
        if (isEating)
        {
            EatControl();

        }
        wasGrounded = Mouseground;
    } 


    private void UpdateDistTraveled()
    {
        float distThisFrame = Vector3.Distance(transform.position, prevPosition);
        
        
        if (cat != null) 
        {
            float catDist = Vector3.Distance(cat.transform.position, transform.position);
            if (catDist <= detectionRadius)
            {
                distTraveled += distThisFrame;
            }
        }
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
            if (climbInterrupted)
            {
                isClimbing = false;
                return;
            }

            Ray path;
            if (isClimbing)
                path = new Ray(transform.position, -transform.up);
            else
                path = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(path, out climbfound, climbCheckDistance))
            {
                if (climbfound.collider.CompareTag("Climbable"))
                {
                    if (!isClimbing)
                    {
                        //On transition to climbing
                        transform.up = climbfound.normal;
                        xRotation += 90;
                    }

                    isClimbing = true;
                    if (indicatorManager != null)
                    {
                        indicatorManager.climbed = true;
                    }
                    velocity.y = 0;
                    //transform.forward = -climbfound.normal; //face the norm vector of the wall 
                    return;
                }
            }

            if (isClimbing)
                climbInterrupted = true;
        }
        else
            climbInterrupted = false;

        //On transition from climbing
        if (isClimbing)
        {
            transform.up = Vector3.up;
            xRotation -= 90;
        }
        //transform.up = Vector3.up;

        isClimbing = false; 

    }
    //todo: handle climbign lol 
    void HandleClimbing()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float move_vert = moveInput.y;
        float move_horiz = moveInput.x;
        //climb direction for movement- hopefully facing object
        Vector3 climbDire = (transform.forward * move_vert) + (transform.right *move_horiz);
        controller.Move(climbDire *normalSpeed *Time.deltaTime);

        if (jumpAction.WasPressedThisFrame())
        {
            //isClimbing = false;
            ////random 2f lol
            //velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            //controller.velo transform.up * jumpHeight;
            //controller.Move((transform.up + transform.forward) * jumpHeight);
            controller.SimpleMove((transform.up + transform.forward) * jumpHeight);
        }
    }

    void EatControl(){
        if (indicatorManager != null)
        {
            indicatorManager.ateFood = true;
        }
        Ray foodRay = new Ray(Playercamera.position, Playercamera.forward);
        if (Physics.Raycast(foodRay, out foodfound, 1))
        {
            if (foodfound.collider.CompareTag("Food"))
            {
                GameObject food = foodfound.collider.gameObject;

                Manager.Manager_.GainLife();
                Destroy(food);
                
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Sink"))
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= 0.5f)
            {
                Manager.Manager_.TakeDamage(1);
                damageTimer = 0f; 
            }
        }
    }



 }
