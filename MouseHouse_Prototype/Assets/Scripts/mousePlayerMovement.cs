using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float mouseSensitivity = 100f;
    //force of gravity is 9.81 downwards 
    public float gravity = -9.81f;

    public float jumpHeight = 2f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Transform Playercamera;
    public Transform camOffsets;

    float xRotation = 0f;
    Vector3 velocity;
    CharacterController controller;
    Animator animator;

    public PlayerInput controlScheme;

    InputAction moveAction;
    InputAction lookAction;
    [HideInInspector]
    public InputAction jumpAction;

    public static PlayerMovement main;

    public float bobbingIntensity = 1;
    public float bobbingSpeed = .25f;

    //Smooth in and out intensity based on speed.
    public float currBobbingIntensity = 0;
    float bobVel;

    // Used to disable cat idle state
    public float distTraveled = 0;
    private Vector3 prevPosition;

    void Start()
    {
        main = this;
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        moveAction = controlScheme.actions["Move"];
        lookAction = controlScheme.actions["Look"];
        jumpAction = controlScheme.actions["Jump"];
        prevPosition = transform.position;
    }

    void Update()
    {
        //need to split up into calls
        HandleMouseLook();
        HandleMovement();

        // Handle the distance traveled variable
        UpdateDistTraveled();
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
        

        if (jumpAction.WasPressedThisFrame() && Mouseground)
        {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateDistTraveled()
    {
        float distThisFrame = Vector3.Distance(transform.position, prevPosition);
        distTraveled += distThisFrame;
        prevPosition = transform.position;
    }
}

