using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Manager : MonoBehaviour
{
    public static Manager Manager_;
    public CanvasGroup start;
    public GameObject player;

    public int lives = 3;
    public int hitCount = 0;
    private bool isImmune = false;
    private float ImmuneTimer = 0f;
    public Image[] livesSprites;

    public Sprite fullLife;
    public Sprite EmptyLife;

    public CanvasGroup lose;
    public CanvasGroup win;

    Vector3 playerStartpos;

    public int levelNum = 1;

    public bool playOnStart = true;

    //damage/health screen UI
    public Image redScreen;
    public float redScreenDuration = 0.3f;
    private float redScreenTimer = 0f;
    //end
    public AudioSource sfxSource;
    public AudioClip buttonClickSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake(){
        Manager_ = this;
        Time.timeScale = 0f;
        playerStartpos = player.transform.position;

        if(redScreen == null){
            GameObject DamageScreen = GameObject.Find("DamageScreen");
            redScreen = DamageScreen.GetComponent<Image>();
        }

    }

    private void Start()
    {
        UpdateLives();
        StartGame();
    }


    public void StartGame(){
        CloseAllScreens();
        Time.timeScale =1f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        UpdateLivesDisplay();
        player.transform.position = playerStartpos;
    }

    public void Update()
    {
        if (isImmune)
        {
            ImmuneTimer -= Time.deltaTime;
            if (ImmuneTimer <= 0f)
            {
                isImmune = false;
            }
        }

        if(redScreenTimer > 0f)
        {
            redScreenTimer -= Time.deltaTime;
                Color redColor = redScreen.color;
                redColor.a = redScreenTimer / redScreenDuration;
                redScreen.color = redColor;
        }
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
        Color screen_color = redScreen.color;
        //alpha value back to 0
        screen_color.a = 0;
        redScreen.color = screen_color;
        OpenLoseScreen();

        lose.alpha = 1;
        lose.blocksRaycasts = true;
        lose.interactable = true;
        redScreenTimer =0; 
        
    }

    void OpenLoseScreen()
    {
        OnMenuOpen();
        lose.alpha = 1;
        lose.blocksRaycasts = true;
        lose.interactable = true;
        EventSystem.current.SetSelectedGameObject(lose.transform.GetChild(1).gameObject);   
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

    public void TakeDamage(int damage)
    {
        if (isImmune)
            return;
        isImmune = true;
        ImmuneTimer = 1f;

        hitCount += damage;
        if (hitCount >= 1)
        {
            redScreenTimer = redScreenDuration;
            Color redColor = redScreen.color;
            redColor.a = 1f;
            redScreen.color = redColor;
            hitCount = 0;
            LoseLife();
        }

    }
    
public void PlayButtonClick()
{
    if (sfxSource != null && buttonClickSound != null)
    {
        sfxSource.PlayOneShot(buttonClickSound);
    }
}

public void GainLife()
{
    if (lives < livesSprites.Length)
    {
        lives++;
        UpdateLivesDisplay();
    }
}

private void UpdateLives()
    {
        GameObject dataObj = GameObject.FindWithTag("Persistent Data");
        if (dataObj != null)
        { 
            PersistentData data = dataObj.GetComponent<PersistentData>();
            lives = data.lives;
            UpdateLivesDisplay();
        }
    }
}
