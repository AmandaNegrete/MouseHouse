using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Winscreen : MonoBehaviour
{
    public GameObject mainMenuButton;
    public GameObject quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Gamepad.current == null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        StartCoroutine(SelectButtonNextFrame());
    }


    public void OnMainMenuButton()
    {
        CloseWinscreen();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    private void CloseWinscreen()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private IEnumerator SelectButtonNextFrame()
    {
        yield return null;
        // Wait for the next frame
        EventSystem.current.SetSelectedGameObject(mainMenuButton);
    }
}
