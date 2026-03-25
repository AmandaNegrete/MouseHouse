using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public LevelOneDialogue lvlOneDialogue;
    private float textDelay = 0.05f;
    private float textDisplayTime = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lvlOneDialogue = ScriptableObject.CreateInstance<LevelOneDialogue>();
        StartCoroutine(LevelStartDialogue());
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }


    private IEnumerator LevelStartDialogue()
    {
        dialoguePanel.SetActive(true);
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("Writing to dialoguebox: " + currentScene + "|");
        switch (currentScene)
        {
            case "Level 1":
                DisplayText(lvlOneDialogue.start);
                break;
        }
        yield return new WaitForSeconds(textDisplayTime);
        dialoguePanel.SetActive(false);
    }


    private void DisplayText(string text)
    {
        StartCoroutine(TypeWriterEffect(text));
    }

    IEnumerator TypeWriterEffect(string fullText)
    {
        dialogueText.text = "";

        foreach (char character in fullText.ToCharArray())
        {
            dialogueText.text += character;
            yield return new WaitForSeconds(textDelay);
        }
    }
}
