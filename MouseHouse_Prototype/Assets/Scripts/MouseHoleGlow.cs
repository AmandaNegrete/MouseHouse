using UnityEngine;

public class TextBehavior : MonoBehaviour
{
    //same thing for catnip ball! :)
    public float glowOffset = 1f;
    public Color hColor = Color.red;
    public float intensity = 2f;
    public float speed = 2f;

    private Material holeM;
    private bool glowing = false;
    
    void Start(){
        holeM = GetComponent<Renderer>().material;
        holeM.DisableKeyword("_EMISSION");
        Invoke(nameof(StartGlow), glowOffset);
    }

    void Update(){
        if(glowing){
           float pulse = Mathf.PingPong(Time.time *4f, intensity);
            holeM.SetColor("_EmissionColor", hColor * pulse);
        }
        
    }
    void StartGlow(){
        glowing = true;
        holeM.EnableKeyword("_EMISSION");
    }
}
