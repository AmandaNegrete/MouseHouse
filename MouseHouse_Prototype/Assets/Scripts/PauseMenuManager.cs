using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public CanvasGroup pauseMenuGroup;

    public static PauseMenuManager main;


    public CanvasGroup bindingsGroup;


    private void Awake()
    {
        main = this;
    }

    public void PauseGame()
    {
        pauseMenuGroup.alpha = 1;
        pauseMenuGroup.blocksRaycasts = true;
        pauseMenuGroup.interactable = true;
        Time.timeScale = 0;
        Cursor.visible = true;
    }

    public void UnpauseGame()
    {
        pauseMenuGroup.alpha = 0;
        pauseMenuGroup.blocksRaycasts = false;
        pauseMenuGroup.interactable = false;
        Time.timeScale = 1;
        Cursor.visible = false;
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
