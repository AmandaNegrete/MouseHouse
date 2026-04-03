using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
    public List<string> levelSceneNames = new List<string>();

    public PlayerInput inputs;

    InputAction startAction;

    public CanvasGroup levelSelect;

    public Transform levelsContainer;

    public GameObject LevelSelectListingPrefab;

    public Image levelImage;

    public Animator ImageAnim;

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

    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits Scene");
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

    public void SetHoveredLevel(LevelListingButton listing)
    {
        levelImage.sprite = listing.image;
        ImageAnim.Play("ImageFadeIn");
    }

    public void ClickedLevel(LevelListingButton level)
    {
        SceneManager.LoadScene(level.levelSceneName);
    }
}
