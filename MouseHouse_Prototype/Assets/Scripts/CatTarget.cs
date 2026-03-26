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

    private void Update()
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
}
