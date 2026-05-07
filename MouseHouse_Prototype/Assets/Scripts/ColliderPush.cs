using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float pushForce = 4f;
    private PlayerMovement player;

    private void Start()
    {
        player = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        pushForce = player.moveSpeed;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null)
            return;

        if (rb.isKinematic)
        {
            rb.isKinematic = false;
        }
        if (hit.moveDirection.y < -0.3f)
            return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Use sweeptest to stop if it will go into another object
        RaycastHit rayHit;
        if (rb.SweepTest(transform.forward, out rayHit, 10f))
        {
            pushDir *= rayHit.distance;
            rb.MovePosition(pushDir);
        }

        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
    }
}