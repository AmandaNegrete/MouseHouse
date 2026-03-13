using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager Manager_;
    public CanvasGroup start;
    public GameObject player;

    private bool gameStart = false;
    int lives = 3;

    public Image[] livesSprites;

    public Sprite fullLife;
    public Sprite EmptyLife;

    public CanvasGroup lose;
    public CanvasGroup win;

    Vector3 playerStartpos;

    public int levelNum = 1;

    public bool playOnStart = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        Manager_ = this;
        Time.timeScale = 0f;
        playerStartpos = player.transform.position;
    }

    private void Start()
    {
        StartGame();
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

        SceneManager.LoadScene("MainMenuScene");
    }

    public void OnMenuOpen()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoseLife(){
        lives--;
        UpdateLivesDisplay();
        if (lives <=0){
            GameOver();
        }
    }
    void GameOver(){
        OpenLoseScreen();

        lose.alpha = 1;
        lose.blocksRaycasts = true;
        lose.interactable = true;
        lives = 3;
    }

    void OpenLoseScreen()
    {
        OnMenuOpen();
        lose.alpha = 1;
        lose.blocksRaycasts = true;
        lose.interactable = true;
    }

    void OpenWinScreen()
    {
        OnMenuOpen();
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

        start.alpha = 0;
        start.blocksRaycasts = false;
        start.interactable = false;

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

    public void ReloadStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
