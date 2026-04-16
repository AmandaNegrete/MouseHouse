using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    public Material interactableMat;
    public GameObject[] interactables;

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

    public void ApplyShaderToInteractables()
    {
        // Go through game objects in heirarchy and applay shader to everything with interactable tag
        foreach (GameObject interactable in interactables)
        {
            AddShader(interactable);
        }
    }

    public void InitInteractables()
    {
        GameObject[] climbables = GameObject.FindGameObjectsWithTag("Climbable");
        GameObject[] food = GameObject.FindGameObjectsWithTag("Food");
        interactables = new GameObject[climbables.Length + food.Length];
        climbables.CopyTo(interactables, 0);
        food.CopyTo(interactables, climbables.Length);
    }

    public void CheckInteracted()
    {

    }
}
