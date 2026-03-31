using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTrap : MonoBehaviour
{
    public GameObject player;
    public PlayerInput controlScheme;

    private InputAction moveAction;
    private InputAction crawlAction;
    private InputAction climbAction;
    private InputAction jumpAction;

    private bool isTrapped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            isTrapped = true;
        }
    }


    private void FreezePlayer()
    {
        // Disable all player movements
        moveAction.Disable();
        crawlAction.Disable();
        climbAction.Disable();
        jumpAction.Disable();
    }


    private void UnfreezePlayer()
    {
        // Enable all player movements
        moveAction.Enable();
        crawlAction.Enable();
        climbAction.Enable();
        jumpAction.Enable();
    }


    private void Escape()
    {
        while ()
    }
}
