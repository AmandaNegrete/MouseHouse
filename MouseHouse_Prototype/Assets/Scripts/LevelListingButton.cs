using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LevelListingButton : MonoBehaviour, IPointerEnterHandler
{
    public string LevelName;
    public string levelSceneName;
    public Sprite image;
    public string levelDescription;

    public bool unlocked = false;

    public MainMenuManager mainMenuMan;
    public Button button;

    private void Start()
    {
        if (!unlocked)
            button.interactable = false;
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(LevelName);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        mainMenuMan.SetHoveredLevel(this);
    }
}
