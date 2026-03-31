using UnityEngine;

public class CatTarget : MonoBehaviour
{

    public Rigidbody rb;
    //Use if on mouse
    public CharacterController charCtrl;

    public float BaseDistractionWeight;
    public float VelDistractionMult;

    public float FinDistractionAmount;

    public float DetectionDistance = 4;

    //Will the cat thwack it when it gets near?
    public bool battable = false;

    public float actCooldown = 2;
    public float lastAct;

    protected virtual void Update()
    {
        if (rb != null)
        {
            FinDistractionAmount = BaseDistractionWeight + rb.linearVelocity.magnitude * VelDistractionMult;
        }
        else if (charCtrl != null)
        {
            FinDistractionAmount = BaseDistractionWeight + charCtrl.velocity.magnitude * VelDistractionMult;
        }
        else
            FinDistractionAmount = BaseDistractionWeight;
    }

    //What does the cat do when interacting with this?
    public virtual void OnInteract(CatAIFollow cat)
    {
        
    }
}
