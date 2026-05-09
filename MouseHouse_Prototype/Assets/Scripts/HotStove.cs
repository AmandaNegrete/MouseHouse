using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    public Color baseColor = Color.black;   
    public Color glowColor = Color.red;     
    public float pulseSpeed = 1.0f;         

    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        Color currentColor = Color.Lerp(baseColor, glowColor, t);

        mat.color = currentColor;
    }
}