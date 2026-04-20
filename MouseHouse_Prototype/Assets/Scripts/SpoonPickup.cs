using UnityEngine;
using UnityEngine.InputSystem;

public class ClickPickup : MonoBehaviour
{
    public Transform cameraTransform;
    public float holdDistance = 1.5f;
    public float holdHeight = -0.2f;
    public float speed = 10f;
    public float pickUpRange = 3f;

    private GameObject heldObject;
    private Collider heldCollider;
    private Rigidbody heldRigidbody;

    public IndicatorManager indicatormanager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null)
                PickUp();
            else
                PlaceOrDrop();
        }

        if (heldObject != null)
        {
            HoldObjectInFront();
        }
    }

    void PickUp()
    {
        indicatormanager.grabbed = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickUpRange))
        {
            if (hit.collider.CompareTag("Spoon"))
            {
                heldObject = hit.collider.gameObject;

                heldCollider = heldObject.GetComponent<Collider>();
                heldRigidbody = heldObject.GetComponent<Rigidbody>();

                if (heldRigidbody != null)
                {
                    heldRigidbody.isKinematic = true;
                    heldRigidbody.useGravity = false;
                    heldRigidbody.linearVelocity = Vector3.zero;
                    heldRigidbody.angularVelocity = Vector3.zero;
                }

                if (heldCollider != null)
                {
                    heldCollider.enabled = false;
                }
            }
        }
    }

    void PlaceOrDrop()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickUpRange))
        {
            //snap to the collider on the sink
            if (hit.collider.CompareTag("Sink"))
            {
                PlaceOnSink(hit.collider.transform);
                return;
            }
        }

        DropObject();
    }

    void HoldObjectInFront()
    {
        Vector3 targetPos =
            cameraTransform.position +
            cameraTransform.forward * holdDistance +
            cameraTransform.up * holdHeight;

        heldObject.transform.position =
            Vector3.Lerp(heldObject.transform.position, targetPos, Time.deltaTime * speed);

        heldObject.transform.rotation =
            Quaternion.Lerp(heldObject.transform.rotation, cameraTransform.rotation, Time.deltaTime * speed);
    }

    void PlaceOnSink(Transform sink)
    {
        //can be named anything
        Transform snapPoint = sink.Find("BridgeSnapPoint");

        if (snapPoint != null)
        {
            heldObject.transform.position = snapPoint.position;
            heldObject.transform.rotation = snapPoint.rotation;
        }
        else
        {
            heldObject.transform.position = sink.position;
            heldObject.transform.rotation = sink.rotation;
        }
        /*
         }

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.linearVelocity = Vector2.zero;
            heldRigidbody.angularVelocity = Vector2.zero;
            heldRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        */
        //RIGID body exists
        if (heldRigidbody != null)
        {
            //no collisions!!!!
            //locking everything because we were flying lol
            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
            heldRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        //aftee placment, collider for walking across bridge
        if (heldCollider != null)
        {
            heldCollider.enabled = true;
        }

        heldObject.transform.localScale = new Vector3(1.2f, 0.2f, 1.2f);

        heldObject = null;
        heldCollider = null;
        heldRigidbody = null;
    }

    void DropObject()
    {
        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = false;
            heldRigidbody.useGravity = true;
        }

        if (heldCollider != null)
        {
            heldCollider.enabled = true;
        }

        heldObject = null;
        heldCollider = null;
        heldRigidbody = null;
    }
}
