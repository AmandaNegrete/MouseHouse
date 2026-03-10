using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    public CanvasGroup pauseMenuGroup;

    public static PauseMenuManager main;


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
        Manager.Manager_.Return();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        
#else
        Application.Quit();
#endif
    }

    public void OpenSettings()
    {
        //NYI
    }


    public void TogglePause()
    {
        if (pauseMenuGroup.alpha > 0)
            UnpauseGame();
        else
            PauseGame();
    }
    
}
