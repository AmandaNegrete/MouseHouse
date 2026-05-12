using UnityEngine;
using UnityEngine.InputSystem;

public class ClickPickup : MonoBehaviour
{
    private Vector3 startPosition;
    public Transform cameraTransform;
    public float holdDistance = 3f;
    public float holdHeight = 2f;
    public float speed = 10f;
    public float pickUpRange = 2f;

    private GameObject heldObject;
    private Collider heldCollider;
    private Rigidbody heldRigidbody;
    private Transform bridgeAnchor;

    public IndicatorManager indicatormanager;

    public InputActionReference grabAction;

    // Colliders to ignore 
    public GameObject sinkBlockLeft;
    public GameObject sinkBlockRight;

    void Start()
    {
        startPosition = transform.position;
        if (indicatormanager == null)
        {
            indicatormanager = FindFirstObjectByType<IndicatorManager>();
        }
    }

    void Update()
    {
        if (grabAction.action.triggered || Input.GetMouseButtonDown(0))
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
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, pickUpRange))
        {
            if (hit.collider.CompareTag("Spoon"))
            {
                indicatormanager.grabbed = true;

                heldObject = hit.collider.gameObject;

                //while (heldObject?.transform?.parent?.gameObject != null)
                //    heldObject = heldObject.transform.parent.gameObject;

                heldCollider = heldObject.GetComponent<Collider>();
                heldRigidbody = hit.rigidbody;

                // Find snap anchor on object
                bridgeAnchor = heldObject.transform.Find("BridgeAnchor");

                if (heldRigidbody != null)
                {
                    heldRigidbody.isKinematic = true;
                    heldRigidbody.useGravity = false;
                    heldRigidbody.linearVelocity = Vector3.zero;
                    heldRigidbody.angularVelocity = Vector3.zero;
                }

                if (heldCollider != null)
                {
                    //heldCollider.enabled = false;
                    //Physics.IgnoreCollision(heldCollider, this.GetComponent<Collider>(), true);
                    Physics.IgnoreCollision(heldCollider, sinkBlockLeft.GetComponent<Collider>(), true);
                    Physics.IgnoreCollision(heldCollider, sinkBlockRight.GetComponent<Collider>(), true);
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
            if (hit.collider.CompareTag("Sink"))
            {
                //Remove for now
                //PlaceOnSink(hit.collider.transform);
                //return;
            }
        }
        if (heldCollider != null && this.TryGetComponent<Collider>(out Collider playerCollider))
        {
            Physics.IgnoreCollision(heldCollider, this.GetComponent<Collider>(), false);
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
        Transform snapPoint = sink.Find("BridgeSnapPoint");

        if (snapPoint != null && bridgeAnchor != null)
        {
            // Calculate offset from anchor to object root
            Vector3 offset = heldObject.transform.position - bridgeAnchor.position;

            // Rotate offset to match snap rotation
            Vector3 rotatedOffset =
                snapPoint.rotation *
                Quaternion.Inverse(heldObject.transform.rotation) *
                offset;

            // Apply final placement
            heldObject.transform.rotation = snapPoint.rotation;
            heldObject.transform.position = snapPoint.position + rotatedOffset;
        }
        else
        {
            heldObject.transform.position = sink.position;
            heldObject.transform.rotation = sink.rotation;
        }

        // Lock physics in place
        if (heldRigidbody != null)
        {
            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;

            heldRigidbody.isKinematic = true;
            heldRigidbody.useGravity = false;
        }

        if (heldCollider != null)
        {
            heldCollider.enabled = true;
        }

        // Optional scaling tweak for bridge placement
        heldObject.transform.localScale = new Vector3(1.2f, 0.2f, 1.2f);

        // Clear references
        heldObject = null;
        heldCollider = null;
        heldRigidbody = null;
        bridgeAnchor = null;
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
        bridgeAnchor = null;
        indicatormanager.grabbed = false;
    }
    public GameObject GetHeldObject()
    {
        return heldObject;
    }
}