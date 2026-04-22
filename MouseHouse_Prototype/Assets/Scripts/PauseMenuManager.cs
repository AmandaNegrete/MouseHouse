using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    public CanvasGroup pauseMenuGroup;

    public static PauseMenuManager main;


    public CanvasGroup bindingsGroup;

    public Button continueButton;

    public Canvas canvas;


    private void Awake()
    {
        main = this;
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    public void PauseGame()
    {
        pauseMenuGroup.alpha = 1;
        pauseMenuGroup.blocksRaycasts = true;
        pauseMenuGroup.interactable = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void UnpauseGame()
    {
        pauseMenuGroup.alpha = 0;
        pauseMenuGroup.blocksRaycasts = false;
        pauseMenuGroup.interactable = false;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void QuitToMenu()
    {
        UnpauseGame();
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void OpenSettings()
    {
        bindingsGroup.interactable = true;
        bindingsGroup.blocksRaycasts = true;
        bindingsGroup.alpha = 1;
    }


    public void TogglePause()
    {
        if(bindingsGroup.alpha > 0)
        {
            CloseBindingsMenu();
            return;
        }

        if (pauseMenuGroup.alpha > 0)
            UnpauseGame();
        else
            PauseGame();
    }
    

    public void CloseBindingsMenu()
    {
        bindingsGroup.alpha = 0;
        bindingsGroup.interactable = false;
        bindingsGroup.blocksRaycasts = false;
    }
}
