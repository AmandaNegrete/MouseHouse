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
    public GameObject cat;
    public GameObject catnip;
    public GameObject bed;
    public GameObject box;

    private float textDelay = 0.03f;
    private float timePerWord = 0.4f;
    public Coroutine currCoroutine;
    //public bool cheeseDialogueTriggered = false;
    public bool triggerHintDialogue = true;
    private bool startDialogueFinished = false;
    //private bool isShowingHint = false; 

    public IndicatorManager indicatorManager;
    public PersistentData persistentData;
    public InputActionReference skip;
    public TextMeshProUGUI skipText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject dataObj = GameObject.FindWithTag("Persistent Data");
        if (dataObj != null) persistentData = dataObj.GetComponent<PersistentData>();
        InitFlags(SceneManager.GetActiveScene().name);
        dialoguePanel = GetComponent<CanvasGroup>();
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

        // Index 4 (in collision function)
        else if (TriggerBedDialogue() && !levelOneFlags[4])
        {
            RunLine(4, 1);
        }

        // Index 5
        else if (TriggerBoxDialogue() && !levelOneFlags[5])
        {
            RunLine(5, 1);
        }

        else if (TriggerCheeseDialogue() && !levelOneFlags[6])
        {
            RunLine(6, 1);
            if (persistentData != null) persistentData.cheeseHintDisplayed = true;
        }
    }


    private void RunLevelTwoDialogue()
    {
        if (currCoroutine != null) return;

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
        else if (TriggerHintDialogue() && !levelTwoFlags[2])
        {
            RunLine(2, 2);
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
            return true;
        }
        return false;
    }


    private bool TriggerBallDialogue()
    {
        if (Vector3.Distance(player.transform.position, catnip.transform.position) <= 1.5f)
        {
            return true;
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
            return true;
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
                return true;
            }
        }
        return false;
    }

    private bool TriggerHintDialogue()
    {
        // Get time since level started
        if (Time.timeSinceLevelLoad >= 90f)
        {
            return true;
        }
        return false;
    }


    //***************************************Functions for writing dialogue to UI*****************************************
    private IEnumerator WriteDialogue(string text, float displayTime)
    {
        UpdateSkipText();
        dialoguePanel.alpha = 1f;
        DisplayText(text);

        // Return when either the time is up or the player presses the skip button
        float timer = 0f;
        while (timer < displayTime)
        {
            if (skip.action.WasPressedThisFrame())
            {
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        //yield return new WaitForSeconds(DisplayTime(text));
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
        StartCoroutine(TypeWriterEffect(text));
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
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        movement.moveAction.Disable();
        movement.crawlAction.Disable();
        movement.climbAction.Disable();
        movement.runAction.Disable();
        movement.eatAction.Disable();
        movement.jumpAction.Disable();
    }

    private void UnfreezePlayer()
    {
        // Enable player movement script
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        movement.moveAction.Enable();
        movement.crawlAction.Enable();
        movement.climbAction.Enable();
        movement.runAction.Enable();
        movement.eatAction.Enable();
        movement.jumpAction.Enable();
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