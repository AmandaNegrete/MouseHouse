using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class SpoonController : MonoBehaviour
{
    public float force;

    public Rigidbody body;

    void OnCollisionEnter(Collision hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            //pointing from the player 2 the ball
            //forward means can only push forward-TEMPORARY FIX!
            Vector3 direction = hit.transform.forward;
            //https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ForceMode.Impulse.html
            body.AddForce(direction * force, ForceMode.Impulse);
        }
        //MoveCat();
    }
}
