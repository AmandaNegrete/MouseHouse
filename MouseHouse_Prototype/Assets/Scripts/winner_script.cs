using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class winner_script : MonoBehaviour
{
    public CanvasGroup winscreen;
    private GameObject dataObj;
    private PersistentData persistentData;

    private void Awake()
    {
            dataObj = GameObject.FindWithTag("Persistent Data");
            if (dataObj != null) persistentData = dataObj.GetComponent<PersistentData>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "Level 2")
            {
                if (persistentData != null) persistentData.levelsCompleted = 2;
                OpenWinscreen();
            }
            else
            {
                if (persistentData != null) persistentData.levelsCompleted = 1;
                SceneManager.LoadScene("VictoryAnimatic");
            }
        }
    }

    private void OpenWinscreen()
    {
        winscreen.alpha = 1f;
        winscreen.interactable = true;
        winscreen.blocksRaycasts = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CloseWinscreen()
    {
        winscreen.alpha = 0f;
        winscreen.interactable = false;
        winscreen.blocksRaycasts = false;
    }

    
}