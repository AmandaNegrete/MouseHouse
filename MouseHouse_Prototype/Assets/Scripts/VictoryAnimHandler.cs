using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VictoryAnimHandler : MonoBehaviour
{
    public VideoPlayer animaticPlayer;
    public SceneDoneFadeout fadeout;

    private void Start()
    {
        animaticPlayer.loopPointReached += PlayFadeOut;
    }

    public void PlayFadeOut(UnityEngine.Video.VideoPlayer videoPlayer)
    {
        fadeout.QueueAndPlaySceneChange("Level 2");
    }
}
