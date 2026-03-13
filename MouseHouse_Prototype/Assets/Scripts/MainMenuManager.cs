using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class MainMenuManager : MonoBehaviour
{
    public List<string> levelSceneNames = new List<string>();

    public PlayerInput inputs;

    InputAction startAction;

    private void Start()
    {
        startAction = inputs.actions["jump"];

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(levelSceneNames[0]);
    }

    private void Update()
    {
        //Change this to be keyboard navigation. Should default selection to start.
        //Also should have level select at some point?
        if(startAction.WasPressedThisFrame())
        {
            LoadNextLevel();
        }
        
    }
}
