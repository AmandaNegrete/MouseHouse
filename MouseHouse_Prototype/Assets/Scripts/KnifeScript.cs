using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int damage = 1;

    [Header("State")]
    public bool isHeld = false;

    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }


    public void OnPickup()
    {
        isHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        //weird collision prevention
        if (col != null)
        {
            col.enabled = true;
        }
    }

    public void OnDrop()
    {
        isHeld = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (col != null)
        {
            col.enabled = true;
        }
    }
}