using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    public Shader interactableShader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyShader(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {

        }
    }

    public void RemoveShader(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {

        }
    }

    public void ApplyShaderToInteractables()
    {

    }
}
