using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentData : MonoBehaviour
{
    public static PersistentData Instance { get; private set; }

    public int lives = 3;
    public bool cheeseHintDisplayed = false;
    public bool sparklingHintDisplayed = false;
    public int levelsCompleted = 0;

    public GameObject floorplan;
    private DialogueManager dialogueManager;    
    [SerializeField] private string currScene;
    [SerializeField] private Manager gameManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        currScene = SceneManager.GetActiveScene().name;
        if (currScene == "MainMenuScene") TryUnlockLevelsInMainMenu();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currScene = scene.name;
        if (currScene == "MainMenuScene")
        {
            TryUnlockLevelsInMainMenu();
            return;
        }

        if (currScene == "Level 1" || currScene == "Level 2")
        {
            GameObject gameobject = GameObject.FindWithTag("Game Manager");
            if (gameobject != null)
            {
                gameManager = gameobject.GetComponent<Manager>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLives();
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

    private void TryUnlockLevelsInMainMenu()
    {
        if (levelsCompleted <= 0) return;

        if (floorplan == null) floorplan = GameObject.Find("Floorplan");

        if (floorplan == null) return;

        
        int maxChildren = 2;
        int toUnlock = Mathf.Clamp(levelsCompleted, 0, maxChildren - 1);

        for (int i = 1; i <= toUnlock; i++)
        {
            if (i >= maxChildren) break;

            Transform child = floorplan.transform.GetChild(i);
            if (child == null) continue;

            LevelListingButton listing = child.GetComponent<LevelListingButton>();
            if (listing == null)
            {
                listing = child.GetComponentInChildren<LevelListingButton>();
            }

            if (listing != null)
            {
                listing.unlocked = true;
                if (listing.button != null) listing.button.interactable = true;
            }
        }
    }
}
