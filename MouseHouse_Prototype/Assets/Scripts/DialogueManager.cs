using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
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
    public GameObject cheeseOne;
    public GameObject cheeseTwo;

    private float textDelay = 0.05f;
    private float timePerWord = 0.4f;
    private Coroutine currCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitFlags(SceneManager.GetActiveScene().name);
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

        // Index 0
        else if (!levelOneFlags[0])
        {
            RunLine(0, 1);
        }

        // Index 1
        else if (TriggerCatDialogue() && !levelOneFlags[1])
        {
            RunLine(1, 1);
        }

        // Index 2
        else if (TriggerBallDialogue() && !levelOneFlags[2])
        {
            RunLine(2, 1);
        }

        // Index 3 (in collision function)
        else if (TriggerBedDialogue() && !levelOneFlags[3])
        {
            RunLine(3, 1);
        }

        // Index 4
        else if (TriggerBoxDialogue() && !levelOneFlags[4])
        {
            RunLine(4, 1);
        }
    }


    private void RunLevelTwoDialogue()
    {
        if (currCoroutine != null) return;

        // Index 0
        else if (!levelTwoFlags[0])
        {
            RunLine(0, 2);
        }

        // Index 1
        else if (TriggerCheeseDialogue() && !levelTwoFlags[1])
        {
            RunLine(1, 2);
        }

        // Index 2
        else if (TriggerHintDialogue() && !levelTwoFlags[2])
        {
            RunLine(2, 2);
        }
    }


    private void RunLine(int index, int level)
    {
        switch(level)
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
        if (Vector3.Distance(player.transform.position, cat.transform.position) <= 4f)
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
        if (Vector3.Distance(player.transform.position, cheeseOne.transform.position) <= 1f || Vector3.Distance(player.transform.position, cheeseTwo.transform.position) <= 1f)
        {
            return true;
        }
        return false;
    }

    private bool TriggerHintDialogue()
    {
        // Get time since level started
        if (Time.timeSinceLevelLoad >= 60f)
        {
            return true;
        }
        return false;
    }


    //***************************************Functions for writing dialogue to UI*****************************************
    private IEnumerator WriteDialogue(string text, float displayTime)
    {
        dialoguePanel.SetActive(true);
        DisplayText(text);
        yield return new WaitForSeconds(DisplayTime(text));
        dialoguePanel.SetActive(false);
        currCoroutine = null;
    }


    private float DisplayTime(string text)
    {
        int spaces = 0;
        foreach(char character in text)
        {
            if (character == ' ') spaces++;
        }
        return (spaces + 1) * timePerWord + 3f;
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
}
