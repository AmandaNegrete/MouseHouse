using UnityEngine;
using UnityEngine.SceneManagement;
public class RetryLevel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
  public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
