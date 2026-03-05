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

    float xRotation = 0f;
    Vector3 velocity;
    CharacterController controller;
    Animator animator;

    public PlayerInput controlScheme;

    InputAction moveAction;
    InputAction lookAction;
    public InputAction jumpAction;

    public static PlayerMovement main;

    void Start()
    {
        main = this;
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        moveAction = controlScheme.actions["Move"];
        lookAction = controlScheme.actions["Look"];
        jumpAction = controlScheme.actions["Jump"];
    }

    void Update()
    {
        //need to split up into calls
        HandleMouseLook();
        HandleMovement();
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


        if (jumpAction.WasPressedThisFrame() && Mouseground)
        {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

