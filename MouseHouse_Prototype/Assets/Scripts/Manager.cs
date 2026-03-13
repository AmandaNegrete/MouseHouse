using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public static Manager Manager_;
    public CanvasGroup Start;
    public GameObject player;

    private bool gameStart = false;
    int lives = 3;

    public Image[] livesSprites;

    public Sprite fullLife;
    public Sprite EmptyLife;

    public CanvasGroup lose;
    public CanvasGroup win;

    Vector3 playerStartpos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        Manager_ = this;
        Time.timeScale = 0f;
        playerStartpos = player.transform.position;
        Return();
    }
    void Update(){
        if(!gameStart && PlayerMovement.main.jumpAction.WasPressedThisFrame()){
            StartGame();
        }
    }
    public void StartGame(){
        gameStart = true;
        CloseAllScreens();
        Time.timeScale =1f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        UpdateLivesDisplay();
        player.transform.position = playerStartpos;
    }

    public void Return(){
        Start.interactable = true;
        Start.blocksRaycasts = true;
        Start.alpha = 1;

        Time.timeScale = 0f;
        lives =3;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        player.transform.position = playerStartpos;
        gameStart = false;
    }
    public void LoseLife(){
        lives--;
        UpdateLivesDisplay();
        if (lives <=0){
            GameOver();
        }
    }
    void GameOver(){
        Return();
        OpenLoseScreen();

        lose.alpha = 1;
        lose.blocksRaycasts = true;
        lose.interactable = true;
        lives = 3;
    }

    void OpenLoseScreen()
    {
        lose.alpha = 1;
        lose.blocksRaycasts = true;
        lose.interactable = true;
    }

    void OpenWinScreen()
    {
        win.alpha = 1;
        win.blocksRaycasts = true;
        win.interactable = true;
    }

    void CloseAllScreens()
    {
        lose.alpha = 0;
        lose.blocksRaycasts = false;
        lose.interactable = false;

        win.alpha = 0;
        win.blocksRaycasts = false;
        win.interactable = false;

        Start.alpha = 0;
        Start.blocksRaycasts = false;
        Start.interactable = false;

    }


    void UpdateLivesDisplay()
    {
        for(int i = 0; i < livesSprites.Length; i++)
        {
            if (i < lives)
            {
                livesSprites[i].sprite = fullLife;
            }
            else
                livesSprites[i].sprite = EmptyLife;
        }
    }
}
