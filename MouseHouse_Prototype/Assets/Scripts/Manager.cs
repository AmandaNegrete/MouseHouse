using UnityEngine;

public class Manager : MonoBehaviour
{
    public static Manager Manager_;
    public GameObject Start;
    private bool gameStart = false;
    int lives = 3;
    public GameObject lose;
    public GameObject win;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        Manager_ = this;
        Time.timeScale = 0f;
    }
    void Update(){
        if(!gameStart && Input.GetKeyDown(KeyCode.Space)){
            StartGame();
        }
    }
    public void StartGame(){
        gameStart = true;
        Start.SetActive(false);
        Time.timeScale =1f;
    }

    public void Return(){
        Start.SetActive(true);
        Time.timeScale = 0f;
        lives =3;
    }
    public void LoseLife(){
        lives--;
        if(lives <=0){
            GameOver();
        }
    }
    void GameOver(){
        Return();
        lives = 3;
    }
    
}
