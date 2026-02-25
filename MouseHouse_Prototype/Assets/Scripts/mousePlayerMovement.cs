using UnityEngine;

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //need to split up into calls
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Quaternion.html
        Playercamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        //for jumping--> check if the mouse is on the grounded
        bool Mouseground = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if(Mouseground && velocity.y < 0){velocity.y = -2f;}

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);


        if (Input.GetButtonDown("Jump") && Mouseground)
        {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}

