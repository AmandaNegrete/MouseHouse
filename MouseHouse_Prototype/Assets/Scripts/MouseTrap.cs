using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseTrap : MonoBehaviour
{
    public GameObject player;
    public TextMeshProUGUI escapeText;
    public PlayerInput controlScheme;
    public Manager gameManager;

    private InputAction moveAction;
    private InputAction crawlAction;
    private InputAction climbAction;
    private InputAction jumpAction;
    public InputActionReference escapeButton;

    private bool isTrapped = false;
    private bool canTrap = true;

    public Slider slider;
    public CanvasGroup mouseTrapScreen;
    public float progress;

    private float mashingPower = 5f;
    private float decayRate = 8f;
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
            player.GetComponent<PlayerMovement>().isTrapped = true;
            Escape();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player && canTrap)
        {
            if (gameManager.lives <= 1)
            {
                gameManager.TakeDamage(1);
            }
            else
            {
                gameManager.TakeDamage(1);
                isTrapped = true;
                FreezePlayer();
            }
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
        SetEscapeText();
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
            canTrap = false;
            player.GetComponent<PlayerMovement>().isTrapped = false;
            UnfreezePlayer();
        }
    }

    private void Reset()
    {
        progress = 0;
        slider.value = progress;
        player.GetComponent<PlayerMovement>().isTrapped = false;
    }

    public void SetEscapeText()
    {
        // Get the current control for escapeTrap
        string currentBinding = escapeButton.action.GetBindingDisplayString();
        escapeText.text = $"Mash [{currentBinding}] to escape!";
    }
}
