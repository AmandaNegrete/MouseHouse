using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroAnimaticManager : MonoBehaviour
{
    public VideoPlayer animaticPlayer;
    public string sceneName;


    private void Start()
    {
        animaticPlayer.loopPointReached += LoadLevel;
    }

    public void LoadLevel(UnityEngine.Video.VideoPlayer videoPlayer)
    {
        SceneManager.LoadScene(sceneName);
    }
}
