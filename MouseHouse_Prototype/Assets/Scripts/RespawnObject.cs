using UnityEngine;

public class RespawnObject : MonoBehaviour
{
    Vector3 respawnPoint;
    Quaternion respawnRotation;

    public Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        respawnPoint = transform.position;
        respawnRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -50)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            transform.position = respawnPoint;
            transform.rotation = respawnRotation;
        }
    }
}
