using UnityEngine;
using UnityEngine.SceneManagement;

public class winner_script : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "Level 2")
            {
                GameObject.FindWithTag("Persistent Data").GetComponent<PersistentData>().levelsCompleted = 2;
                SceneManager.LoadScene("MainMenuScene");
            }
            else
            {
                GameObject.FindWithTag("Persistent Data").GetComponent<PersistentData>().levelsCompleted = 1;
                SceneManager.LoadScene("VictoryAnimatic");
            }
        }
    }
}