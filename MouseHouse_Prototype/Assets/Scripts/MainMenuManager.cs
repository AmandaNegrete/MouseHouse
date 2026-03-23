using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class MainMenuManager : MonoBehaviour
{
    public List<string> levelSceneNames = new List<string>();

    public PlayerInput inputs;

    InputAction startAction;

    public CanvasGroup levelSelect;

    public Transform levelsContainer;

    public GameObject LevelSelectListingPrefab;

    private void Start()
    {
        
        startAction = inputs.actions["jump"];

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        PopulateLevelSelect();
    }

    void PopulateLevelSelect()
    {
        for(int i = 0; i < levelSceneNames.Count; i++)
        {
            GameObject addedListing = Instantiate(LevelSelectListingPrefab, levelsContainer);
            LevelListingButton listing = addedListing.GetComponent<LevelListingButton>();
            listing.text.text = "" + (i + 1);
            listing.LevelName = levelSceneNames[i];
        }
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(levelSceneNames[0]);
    }

    public void OpenLevelSelect()
    {
        levelSelect.alpha = 1;
        levelSelect.blocksRaycasts = true;
        levelSelect.interactable = true;
    }

    public void CloseLevelSelect()
    {
        levelSelect.alpha = 0;
        levelSelect.blocksRaycasts = false;
        levelSelect.interactable = false;
    }

    private void Update()
    {
        //Change this to be keyboard navigation. Should default selection to start.
        //Also should have level select at some point?
        //if(startAction.WasPressedThisFrame())
        //{
        //    LoadNextLevel();
        //}
        
    }
}
