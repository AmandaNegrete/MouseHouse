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
    public PlayerInput controlScheme;
    public Transform playerCamera;
    public PlayerMovement playerMovement;
    private Coroutine currCoroutine;
    public Transform player;
    public TextMeshProUGUI hintText;
    private GameObject currObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitInteractables();
        ApplyShaderToInteractables();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //**********Initialization*******************
    public void InitInteractables()
    {
        climbables = GameObject.FindGameObjectsWithTag("Climbable");
        food = GameObject.FindGameObjectsWithTag("Food");
        interactables = new GameObject[climbables.Length + food.Length];
        climbables.CopyTo(interactables, 0);
        food.CopyTo(interactables, climbables.Length);
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
        foreach (GameObject interactable in interactables)
        {
            if (Vector3.Distance(player.position, interactable.transform.position) <= 1f)
            {
                if (currCoroutine == null && currObject == null)
                {
                    currObject = interactable;
                    currCoroutine = StartCoroutine(EnteredInteractableArea(interactable.tag));
                }
            }
        }
    }

    public void CheckLeftArea()
    {
        if (currObject != null && Vector3.Distance(player.position, currObject.transform.position) > 1f)
        {
            if (currCoroutine != null)
            {
                StopCoroutine(currCoroutine);
                currCoroutine = null;
                hintText.text = "";
                currObject = null;
            }
        }
    }


    public void RemoveShader(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            Material[] newMaterials = new Material[materials.Length - 1];
            int index = 0;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != interactableMat)
                {
                    newMaterials[index] = materials[i];
                    index++;
                }
            }
            renderer.materials = newMaterials;
        }
    }

    public bool InteractedWithFood()
    {
        RaycastHit foodfound;
        Ray foodRay = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(foodRay, out foodfound, 1))
        {
            if (foodfound.collider.CompareTag("Food") && controlScheme.actions["Eat"].triggered)
            {
                RemoveCurrentObject();
                return true;
            }
        }
        return false; 
    }

    public bool Climbed()
    {
        RaycastHit climbfound;
        Ray climbRay = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(climbRay, out climbfound, 1))
        {
            if (climbfound.collider.CompareTag("Climbable") && controlScheme.actions["Climb"].triggered)
            {
                RemoveCurrentObject();
                return true;
            }
        }
        return false;
    }

    IEnumerator EnteredInteractableArea(string tag)
    {
        yield return new WaitForSeconds(20f);
        DisplayHint(tag);
    }

    private void DisplayHint(string tag)
    {
        if (tag == "Food")
        {
            hintText.text = "Press [" + controlScheme.actions["Eat"].GetBindingDisplayString() + "] to eat!";
        }
        else if (tag == "Climbable")
        {
            hintText.text = "Press [" + controlScheme.actions["Climb"].GetBindingDisplayString() + "] to climb!";
        }
    }

    private void RemoveCurrentObject()
    {
        if (currObject != null)
        {
            RemoveShader(currObject);
            currObject = null;
        }
    }
}
