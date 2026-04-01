using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseTrap : MonoBehaviour
{
    public GameObject player;
    public PlayerInput controlScheme;

    private InputAction moveAction;
    private InputAction crawlAction;
    private InputAction climbAction;
    private InputAction jumpAction;
    public InputActionReference escapeButton;

    private bool isTrapped = false;

    public Slider slider;
    public CanvasGroup mouseTrapScreen;
    public float progress;

    private float mashingPower = 4f;
    private float decayRate = 10f;
    private float escapeThreshold = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouseTrapScreen.alpha = 0;
        moveAction = controlScheme.actions["Move"];
        crawlAction = controlScheme.actions["Crawl"];
        climbAction = controlScheme.actions["Climb"];
        jumpAction = controlScheme.actions["Jump"];
    }


    // Update is called once per frame
    void Update()
    {
        if (isTrapped)
        {
            Escape();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            isTrapped = true;
            FreezePlayer();
        }
    }


    private void FreezePlayer()
    {
        // Disable all player movements
        moveAction.Disable();
        crawlAction.Disable();
        climbAction.Disable();
        jumpAction.Disable();
        mouseTrapScreen.alpha = 1;
    }


    private void UnfreezePlayer()
    {
        // Enable all player movements
        moveAction.Enable();
        crawlAction.Enable();
        climbAction.Enable();
        jumpAction.Enable();
        mouseTrapScreen.alpha = 0;
        Reset();
    }


    private void Escape()
    {
        progress -= decayRate * Time.deltaTime;
        if (escapeButton.action.WasPressedThisFrame())
        {
            progress += mashingPower;
        }

        progress = Mathf.Clamp(progress, 0, escapeThreshold);
        slider.value = progress;

        if (progress >= escapeThreshold)
        {
            isTrapped = false;
            UnfreezePlayer();
        }
    }

    private void Reset()
    {
        progress = 0;
        slider.value = progress;
    }
}
