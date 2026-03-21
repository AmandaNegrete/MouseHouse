using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelListingButton : MonoBehaviour
{
    public string LevelName;
    public TextMeshProUGUI text;


    public void LoadScene()
    {
        SceneManager.LoadScene(LevelName);
    }

}
