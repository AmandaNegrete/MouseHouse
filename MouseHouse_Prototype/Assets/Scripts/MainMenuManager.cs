using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;      
using System.Collections;


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

    public AudioSource audioSource; 
    public AudioClip clickSound;
    public VideoPlayer videoPlayer; 

    public GameObject videoUI;

    public SceneDoneFadeout onDoneFadeout;

    public CanvasGroup settingsGroup;

    private void Start()
    {
        if (audioSource == null) 
            audioSource = GetComponent<AudioSource>();
        
        startAction = inputs.actions["jump"];

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OpenSettingsMenu();
    }
    private void OnHoverButton()
    {
        if (audioSource != null)
        {
            audioSource.volume = .1f;
            audioSource.PlayOneShot(clickSound);
        }
    }

    public void LoadNextLevel()
    {
        OnHoverButton();
       StartCoroutine(PlayVideoAndLoad());
    }
    
    public void OpenLevelSelect()
    {
        OnHoverButton();
        levelSelect.alpha = 1;
        levelSelect.blocksRaycasts = true;
        levelSelect.interactable = true;
    }

    public void CloseLevelSelect()
    {
         OnHoverButton();
        levelSelect.alpha = 0;
        levelSelect.blocksRaycasts = false;
        levelSelect.interactable = false;
    }

    public void LoadCredits()
    {
        OnHoverButton();
        SceneManager.LoadScene("Credits Scene");
    }private IEnumerator PlayVideoAndLoad()
    {
        videoUI.SetActive(true);
        
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null; 
        }

        videoPlayer.Play();
        yield return new WaitForSeconds(0.1f);
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        SceneManager.LoadScene(levelSceneNames[0]);
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
        ImageAnim.Play("ImageFadeIn", -1, 0);
    }

    public void ClickedLevel(LevelListingButton level)
    {
        //SceneManager.LoadScene(level.levelSceneName);
        onDoneFadeout.QueueAndPlaySceneChange(level.levelSceneName);
    }

    public void OpenSettingsMenu()
    {
        settingsGroup.alpha = 1;
        settingsGroup.interactable = true;
        settingsGroup.blocksRaycasts = true;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
