using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentData : MonoBehaviour
{

    public int lives = 3;
    public bool cheeseHintDisplayed = false;
    public int levelsCompleted = 0;

    [SerializeField] private string currScene;
    [SerializeField] private Manager gameManager;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        currScene = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateScene();
        UpdateLives();
    }

    private void UpdateScene()
    {
        if (currScene != SceneManager.GetActiveScene().name)
        {
            currScene = SceneManager.GetActiveScene().name;
            if (currScene != "Level 1" && currScene != "Level 2") return;
            gameManager = GameObject.FindWithTag("Game Manager").GetComponent<Manager>();
        }
    }

    private void UpdateLives()
    {
        if (gameManager != null)
        {
            lives = gameManager.lives;
        }
        if (currScene == "MainMenuScene")
        {
            lives = 3;
        }
    }
}
