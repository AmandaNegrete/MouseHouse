using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public static Manager Manager_;
    public GameObject Start;
    public GameObject player;

    private bool gameStart = false;
    int lives = 3;

    public Image[] livesSprites;

    public Sprite fullLife;
    public Sprite EmptyLife;

    public GameObject lose;
    public GameObject win;

    Vector3 playerStartpos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        Manager_ = this;
        Time.timeScale = 0f;
        playerStartpos = player.transform.position;
    }
    void Update(){
        if(!gameStart && PlayerMovement.main.jumpAction.WasPressedThisFrame()){
            StartGame();
        }
    }
    public void StartGame(){
        gameStart = true;
        Start.SetActive(false);
        Time.timeScale =1f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        UpdateLivesDisplay();
    }

    public void Return(){
        Start.SetActive(true);
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
        lives = 3;
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
