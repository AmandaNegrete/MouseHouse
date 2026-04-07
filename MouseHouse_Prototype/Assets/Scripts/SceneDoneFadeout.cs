using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoneFadeout : MonoBehaviour
{
    public string queuedNewScene;

    public Animator anim;

    public void LoadQueuedScene()
    {
        SceneManager.LoadScene(queuedNewScene);
    }

    public void QueueAndPlaySceneChange(string newScene)
    {
        queuedNewScene = newScene;
        anim.Play("SceneFadeOut");
    }

}
