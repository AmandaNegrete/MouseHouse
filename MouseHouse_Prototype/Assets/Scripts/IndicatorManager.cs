using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class IndicatorManager : MonoBehaviour
{
    public Material interactableMat;
    public GameObject[] interactables;
    public GameObject[] climbables;
    public GameObject[] food;
    public GameObject[] grabables;

    public PlayerInput controlScheme;
    public Transform playerCamera;
    public PlayerMovement playerMovement;
    public Transform player;
    public DialogueManager dialogueManager;

    private GameObject currObject;
    public Coroutine currCoroutine;
    private const float hintDist = 2f;
    public bool stopHint = false;
    private const float timeBeforeHint = 12f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitInteractables();
        ApplyShaderToInteractables();
    }


    // Update is called once per frame
    void Update()
    {
        CheckEnteredArea();
        CheckLeftArea();
        if ((InteractedWithFood() || Climbed() || Grabbed()) && currCoroutine != null)
        {
            StopCoroutine(currCoroutine);
            currCoroutine = null;
            currObject = null;
        }
    }


    //**********Initialization*******************
    public void InitInteractables()
    {
        climbables = GameObject.FindGameObjectsWithTag("Climbable");
        food = GameObject.FindGameObjectsWithTag("Food");
        grabables = GameObject.FindGameObjectsWithTag("Spoon");
        interactables = new GameObject[climbables.Length + food.Length + grabables.Length];
        climbables.CopyTo(interactables, 0);
        food.CopyTo(interactables, climbables.Length);
        grabables.CopyTo(interactables, climbables.Length + food.Length);
    }


    public void ApplyShaderToInteractables()
    {
        // Go through game objects in heirarchy and applay shader to everything with interactable tag
        foreach (GameObject interactable in interactables)
        {
            AddShader(interactable);
        }
    }


    public void AddShader(GameObject obj)
    {
        // Add interactable material to material list of all renderers in game object and children
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length + 1];
            materials.CopyTo(newMaterials, 0);
            newMaterials[materials.Length] = interactableMat;
            renderer.materials = newMaterials;
        }
    }


    //**********************Check enter and exit interactables area*****************
    public void CheckEnteredArea()
    {
        if (currCoroutine != null || currObject != null) return;

        foreach (GameObject interactable in interactables)
        {
            if (interactable == null) continue;
            else if (Vector3.Distance(player.position, interactable.transform.position) <= hintDist)
            {
                if (currCoroutine == null && currObject == null)
                {
                    currObject = interactable;
                    currCoroutine = StartCoroutine(EnteredInteractableArea(interactable.tag));
                    stopHint = false;
                    return;
                }
            }
        }
    }


    public void CheckLeftArea()
    {
        if (currObject != null && Vector3.Distance(player.position, currObject.transform.position) > hintDist)
        {
            if (currCoroutine != null && currObject != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
                currObject = null;
                stopHint = true;
                return;
            }
        }
    }


    public void RemoveShader(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Remove the last material in the array
            Material[] materials = renderer.materials;
            if (materials.Length > 1)
            {
                Material[] newMaterials = new Material[materials.Length - 1];
                for (int i = 0; i < materials.Length - 1; i++)
                {
                    newMaterials[i] = materials[i];
                }
                renderer.materials = newMaterials;
            }
        }
    }


    IEnumerator EnteredInteractableArea(string tag)
    {
        Debug.Log("Entered " + tag + " area");
        yield return new WaitForSeconds(timeBeforeHint);
        yield return new WaitUntil(() => dialogueManager.currCoroutine == null);
        DisplayHint(tag);
    }


    //********************Check for interaction********************
    public bool InteractedWithFood()
    {
        dialogueManager.triggerHintDialogue = false;
        // Determine whther the food has been eaten or not
        if (currObject != null && playerMovement.isEating)
        {
            RemoveCurrentObject();
            stopHint = true;
            return true;
        }
        return false;
    }


    public bool Climbed()
    {
        dialogueManager.triggerHintDialogue = false;
        if (playerMovement.isClimbing && controlScheme.actions["Climb"].triggered)
        {
            RemoveCurrentObject();
            stopHint = true;
            return true;
        }
        return false;
    }


    public bool Grabbed()
    {
        dialogueManager.triggerHintDialogue = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            // Hits the spoon and left mouse was clicked using new input system
            if (hit.collider.CompareTag("Spoon") && Input.GetMouseButtonDown(0))
            {
                RemoveCurrentObject();
                stopHint = true;
                return true;
            }
        }
        return false;
    }


    //*******************Display and cleanup*****************
    private void DisplayHint(string tag)
    {
        string hintText = "";
        if (tag == "Food")
        {
             hintText = "Press [" + controlScheme.actions["Eat"].GetBindingDisplayString() + "] to eat!";
        }
        else if (tag == "Climbable")
        {
            hintText = "Press [" + controlScheme.actions["Climb"].GetBindingDisplayString() + "] to climb!";
        }
        else if (tag == "Spoon")
        {
            hintText = "Click the object to grab it!";
        }

        if (hintText != "")
        {
            dialogueManager.currCoroutine = StartCoroutine(dialogueManager.WriteHint(hintText));
        }
    }


    private void RemoveCurrentObject()
    {
        if (currObject != null)
        {
            Debug.Log("Interacted with " + currObject.name);
            // Check for whether the object still exists
            if (currObject != null) RemoveShader(currObject);
            currObject = null;
        }
    }
}
