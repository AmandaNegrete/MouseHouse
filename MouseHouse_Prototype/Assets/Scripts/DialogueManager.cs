using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    private CanvasGroup dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Dialogue levelOneDialogue;
    public bool[] levelOneFlags;
    public Dialogue levelTwoDialogue;
    public bool[] levelTwoFlags;
    public GameObject player;
    private PlayerMovement playerInfo;
    public GameObject cat;
    public GameObject catnip;
    public GameObject bed;
    public GameObject box;

    private float textDelay = 0.03f;
    private float timePerWord = 0.5f;
    public Coroutine currCoroutine;
    public Coroutine typeWriterCoroutine;
    //public bool cheeseDialogueTriggered = false;
    public bool triggerHintDialogue = true;
    private bool startDialogueFinished = false;
    //private bool isShowingHint = false; 

    public IndicatorManager indicatorManager;
    public PersistentData persistentData;
    public InputActionReference skip;
    public TextMeshProUGUI skipText;

    public GameObject leftBlock;
    public GameObject rightBlock;
    public GameObject spoonPuzzleCheese;

    public GameObject bug;
    public GameObject stove;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject dataObj = GameObject.FindWithTag("Persistent Data");
        if (dataObj != null) persistentData = dataObj.GetComponent<PersistentData>();
        InitFlags(SceneManager.GetActiveScene().name);
        dialoguePanel = GetComponent<CanvasGroup>();
        playerInfo = player.GetComponent<PlayerMovement>();
    }


    // Update is called once per frame
    void Update()
    {
        // Run the corresponding levels dialogue
        string currScene = SceneManager.GetActiveScene().name;
        switch (currScene)
        {
            case "Level 1":
                RunLevelOneDialogue();
                break;
            case "Level 2":
                RunLevelTwoDialogue();
                break;
        }
    }

    //***************************************Run dialogue********************************************
    private void RunLevelOneDialogue()
    {
        if (currCoroutine != null) return;

        if (levelOneFlags[0] && !startDialogueFinished)
        {
            UnfreezePlayer();
            startDialogueFinished = true;
        }

        // Index 0
        else if (!levelOneFlags[0])
        {
            FreezePlayer();
            RunLine(0, 1);
        }

        // Index 1
        else if (!levelOneFlags[1])
        {
            RunLine(1, 1);
        }

        // Index 2
        else if (TriggerCatDialogue() && !levelOneFlags[2])
        {
            RunLine(2, 1);
        }

        // Index 3
        else if (TriggerBallDialogue() && !levelOneFlags[3])
        {
            RunLine(3, 1);
        }

        // Index 4
        else if (TriggerBoxDialogue() && !levelOneFlags[4])
        {
            RunLine(4, 1);
        }

        // Index 5
        else if (TriggerCheeseDialogue() && !levelOneFlags[5])
        {
            RunLine(5, 1);
            if (persistentData != null) persistentData.cheeseHintDisplayed = true;
        }

        // Index 6
        else if (TriggerHintDialogue() && !levelOneFlags[6])
        {
            RunLine(6, 1);
            if (persistentData != null) persistentData.sparklingHintDisplayed = true;
        }

        // Index 7
        else if (TriggerClimbDialogue())
        {
            RunLine(7, 1);
        }
    }


    private void RunLevelTwoDialogue()
    {
        if (currCoroutine != null) return;

        if (player.GetComponent<PlayerMovement>().isTrapped) return;

        if (levelTwoFlags[0] && !startDialogueFinished)
        {
            UnfreezePlayer();
            startDialogueFinished = true;
        }

        // Index 0
        else if (!levelTwoFlags[0])
        {
            FreezePlayer();
            RunLine(0, 2);
        }

        // Index 1
        else if (TriggerCheeseDialogue() && !levelTwoFlags[1])
        {
            RunLine(1, 2);
            if (persistentData != null) persistentData.cheeseHintDisplayed = true;
        }

        // Index 2
        else if (TriggerBlockDialogue())
        {
            RunLine(2, 2);
        }

        // Index 3
        else if (TriggerClimbDialogue())
        {
            RunLine(3, 2);
        }

        // Index 4
        else if (TriggerSpoonPuzzleDialogue() && !levelTwoFlags[4])
        {
            RunLine(4, 2);
        }

        // Index 5
        else if (TriggerStoveDialogue() && !levelTwoFlags[5])
        {
            RunLine(5, 2);
        }

        // Index 6
        else if (TriggerBugDialogue() && !levelTwoFlags[6])
        {
            RunLine(6, 2);
        }

        // Index 7
        else if (TriggerHintDialogue() && !levelTwoFlags[7])
        {
            RunLine(7, 2);
            if (persistentData != null) persistentData.sparklingHintDisplayed = true;
        }
    }


    private void RunLine(int index, int level)
    {
        switch (level)
        {
            case 1:
                currCoroutine = StartCoroutine(WriteDialogue(levelOneDialogue.lines[index], DisplayTime(levelOneDialogue.lines[index])));
                levelOneFlags[index] = true;
                break;
            case 2:
                currCoroutine = StartCoroutine(WriteDialogue(levelTwoDialogue.lines[index], DisplayTime(levelTwoDialogue.lines[index])));
                levelTwoFlags[index] = true;
                break;
        }
    }


    //**********************************************Dialogue triggers*************************************************
    private bool TriggerCatDialogue()
    {
        if (Vector3.Distance(player.transform.position, cat.transform.position) <= 5f && cat.GetComponent<CatAIFollow>().state == CatAIFollow.CatState.sleeping)
        {
            if (cat.GetComponentInChildren<SpriteRenderer>().isVisible) return true;
        }
        return false;
    }


    private bool TriggerBallDialogue()
    {
        if (Vector3.Distance(player.transform.position, catnip.transform.position) <= 1.5f)
        {
            if (catnip.GetComponent<Renderer>().isVisible) return true;
        }
        return false;
    }


    private bool TriggerBedDialogue()
    {
        if (Vector3.Distance(player.transform.position, bed.transform.position) <= 3f)
        {
            return true;
        }
        return false;
    }


    private bool TriggerBoxDialogue()
    {
        // Only triggers if the player has already seen the cat
        if (Vector3.Distance(player.transform.position, box.transform.position) <= 4f && (levelOneFlags[1] || cat.GetComponent<CatAIFollow>().state != CatAIFollow.CatState.sleeping))
        {
            if (box.GetComponent<Renderer>().isVisible) return true;
        }
        return false;
    }

    //*********Level two triggers************
    private bool TriggerCheeseDialogue()
    {
        if (persistentData != null && persistentData.cheeseHintDisplayed) return false;

        if (indicatorManager == null || indicatorManager.interactables == null) return false;

        foreach (GameObject interactable in indicatorManager.interactables)
        {
            if (interactable == null) continue;
            if (!interactable.CompareTag("Food")) continue;

            if (Vector3.Distance(player.transform.position, interactable.transform.position) <= 1.5f)
            {
                if (interactable.GetComponent<Renderer>().isVisible) return true;
            }
        }
        return false;
    }

    private bool TriggerHintDialogue()
    {
        if (persistentData != null && persistentData.sparklingHintDisplayed) return false;
        // Get time since level started
        if (Time.timeSinceLevelLoad >= 30f)
        {
            return true;
        }
        return false;
    }


    private bool TriggerClimbDialogue()
    {
        if (playerInfo.attemptingClimb)
        {
            return true;
        }
        return false;
    }


    private bool TriggerBlockDialogue()
    {
        if ((Vector3.Distance(player.transform.position, leftBlock.GetComponent<BoxCollider>().ClosestPoint(player.transform.position)) <= 0.25f) || 
            (Vector3.Distance(player.transform.position, rightBlock.GetComponent<BoxCollider>().ClosestPoint(player.transform.position)) <= 0.2f))
        {
            return true;
        }
        return false;
    }


    private bool TriggerSpoonPuzzleDialogue()
    {
        if (spoonPuzzleCheese == null) return false;
        if (Vector3.Distance(player.transform.position, spoonPuzzleCheese.transform.position) <= 3f)
        {
            return true;
        }
        return false;
    }


    private bool TriggerStoveDialogue()
    {
        if (Vector3.Distance(player.transform.position, stove.transform.position) <= 4f)
        {
            if (stove.GetComponent<Renderer>().isVisible) return true;
        }
        return false;
    }


    private bool TriggerBugDialogue()
    {
        if (Vector3.Distance(player.transform.position, bug.transform.position) <= 4f)
        {
            if (bug.GetComponentInChildren<SpriteRenderer>().isVisible) return true;
        }
        return false;
    }


    //***************************************Functions for writing dialogue to UI*****************************************
    private IEnumerator WriteDialogue(string text, float displayTime)
    {
        float timeStart = Time.time;
        UpdateSkipText();
        dialoguePanel.alpha = 1f;
        DisplayText(text);

        // Return when either the time is up or the player presses the skip button
        //yield return new WaitForSeconds(DisplayTime(text));
        yield return new WaitUntil(() => Time.time - timeStart >= displayTime || skip.action.WasPressedThisFrame());
        dialoguePanel.alpha = 0f;
        currCoroutine = null;
    }


    // For the IndicatorManager class, doesn't use a defined time
    public IEnumerator WriteHint(string text)
    {
        dialoguePanel.alpha = 1f;
        DisplayText(text);
        yield return new WaitUntil(() => indicatorManager.stopHint || skip.action.WasPressedThisFrame());
        dialoguePanel.alpha = 0f;
        currCoroutine = null;
    }


    public float DisplayTime(string text)
    {
        int spaces = 0;
        foreach (char character in text)
        {
            if (character == ' ') spaces++;
        }
        return (spaces + 1) * timePerWord;
    }


    private void DisplayText(string text)
    {
        if (typeWriterCoroutine != null)
        {
            StopCoroutine(typeWriterCoroutine);
        }
        typeWriterCoroutine = StartCoroutine(TypeWriterEffect(text));
    }

    IEnumerator TypeWriterEffect(string fullText)
    {
        dialogueText.text = fullText;

        for (int i = 0; i < fullText.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(textDelay);
        }
    }


    private void InitFlags(string level)
    {
        switch (level)
        {
            case "Level 1":
                levelOneFlags = new bool[levelOneDialogue.lines.Length];
                break;
            case "Level 2":
                levelTwoFlags = new bool[levelTwoDialogue.lines.Length];
                break;
        }
    }

    private void FreezePlayer()
    {
        // Disable player movement script
        playerInfo.moveAction.Disable();
        playerInfo.crawlAction.Disable();
        playerInfo.climbAction.Disable();
        playerInfo.runAction.Disable();
        playerInfo.eatAction.Disable();
        playerInfo.jumpAction.Disable();
    }

    private void UnfreezePlayer()
    {
        // Enable player movement script
        playerInfo.moveAction.Enable();
        playerInfo.crawlAction.Enable();
        playerInfo.climbAction.Enable();
        playerInfo.runAction.Enable();
        playerInfo.eatAction.Enable();
        playerInfo.jumpAction.Enable();
    }

    private void UpdateSkipText()
    {
        if (Gamepad.current != null)
        {
            skipText.text = "Skip: " + skip.action.GetBindingDisplayString(1);
        }
        else
        {
            skipText.text = "Skip: " + skip.action.GetBindingDisplayString(0);
        }
    }
}