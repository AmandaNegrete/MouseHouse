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

    public GameObject currObject;
    public Coroutine currCoroutine;
    private const float foodDist = 1.1f;
    private const float climbDist = 3f;
    private const float grabDist = 3f;
    public bool stopHint = false;
    private const float timeBeforeHint = 6f;
    public bool printCheeseHint = true;

    public bool ateFood = false;
    public bool climbed = false;
    public bool grabbed = false;

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
        if (((InteractedWithFood() && dialogueManager.cheeseDialogueTriggered) || Climbed() || Grabbed()) && currCoroutine != null)
        {
            StopCoroutine(currCoroutine);
            currCoroutine = null;
            currObject = null;
        }
        ResetBools();
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
            else if (IsWithinDistance(interactable))
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
        if (currObject != null && !IsWithinDistance(currObject))
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
        //Debug.Log("Entered " + tag + " area");
        yield return new WaitForSeconds(timeBeforeHint);
        yield return new WaitUntil(() => dialogueManager.currCoroutine == null);
        DisplayHint(tag);
    }


    //********************Check for interaction********************
    public bool InteractedWithFood()
    {
        if (ateFood)
        {
            printCheeseHint = false;
            stopHint = true;
            ateFood = false;
            return true;
        }

        printCheeseHint = false;

        RaycastHit foodfound;
        Ray foodRay = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(foodRay, out foodfound, 1))
        {
            if (foodfound.collider.CompareTag("Food") && controlScheme.actions["Eat"].triggered)
            {
                GameObject hitObj = foodfound.collider.gameObject;
                RemoveShader(hitObj);

                if (currObject == hitObj) RemoveCurrentObject();

                stopHint = true;
                return true;
            }
        }
        return false;
    }


    public bool Climbed()
    {
        if (climbed)
        {
            RemoveCurrentObject();
            stopHint = true;
            return true;
        }
        return false;
    }


    public bool Grabbed()
    {
        if (grabbed)
        {
            RemoveCurrentObject();
            stopHint = true;
            return true;
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
            //Debug.Log("Interacted with " + currObject.name);
            // Check for whether the object still exists
            if (currObject != null) RemoveShader(currObject);
            currObject = null;
        }
    }

    private void ResetBools()
    {
        ateFood = false;
        climbed = false;
        grabbed = false;
    }

    
    private bool IsWithinDistance(GameObject obj)
    {
        string tag = obj.tag;
        if (tag == "Food")
        {
            return Vector3.Distance(player.position, obj.transform.position) <= foodDist;
        }
        else if (tag == "Climbable")
        {
            return Vector3.Distance(player.position, obj.transform.position) <= climbDist;
        }
        else if (tag == "Spoon")
        {
            return Vector3.Distance(player.position, obj.transform.position) <= grabDist;
        }
        return false;
    }
}
