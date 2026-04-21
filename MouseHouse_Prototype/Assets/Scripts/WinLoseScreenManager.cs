using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseScreenManager : MonoBehaviour
{
    public CanvasGroup winGroup;
    public Animator winAnimator;
    public string nextLevel;
    public GameObject nextLevelButton;

    public void OpenWinGameScreen()
    {
        winGroup.alpha = 1;
        winGroup.interactable = true;
        winGroup.blocksRaycasts = true;

        //Hide next level if there is none
        if (nextLevel == "")
            nextLevelButton.SetActive(false);
        else
            nextLevelButton.SetActive(true);

        //Open win screen instead
        winAnimator.Play("WinAnim");

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
        
    }

    public void ReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
