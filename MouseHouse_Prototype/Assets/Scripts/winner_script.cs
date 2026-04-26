using UnityEngine;
using UnityEngine.SceneManagement;

public class winner_script : MonoBehaviour
{
    public CanvasGroup winscreen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "Level 2")
            {
                GameObject.FindWithTag("Persistent Data").GetComponent<PersistentData>().levelsCompleted = 2;
                OpenWinscreen();
            }
            else
            {
                GameObject.FindWithTag("Persistent Data").GetComponent<PersistentData>().levelsCompleted = 1;
                SceneManager.LoadScene("VictoryAnimatic");
            }
        }
    }

    private void OpenWinscreen()
    {
        winscreen.alpha = 1f;
        winscreen.interactable = true;
        winscreen.blocksRaycasts = true;
    }

    private void CloseWinscreen()
    {
        winscreen.alpha = 0f;
        winscreen.interactable = false;
        winscreen.blocksRaycasts = false;
    }

    
}