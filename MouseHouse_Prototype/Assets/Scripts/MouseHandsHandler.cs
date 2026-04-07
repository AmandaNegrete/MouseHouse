using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MouseHandsHandler : MonoBehaviour
{
    public List<Transform> hands = new List<Transform>();

    List<Vector3> handPos = new List<Vector3>();
    List<Vector3> relHandPivPos = new List<Vector3>();

    List<Vector3> handVelocities = new List<Vector3>();

    public float handMaxDistance = .2f;

    public bool enablePawsMovement = true;

    public bool alternator = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(Transform hand in hands)
        {
            handPos.Add(hand.position);
            relHandPivPos.Add(hand.localPosition);
            handVelocities.Add(default);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (enablePawsMovement)
        {
            for (int i = 0; i < hands.Count; i++)
            {
                UpdateHand(i);
            }
        }
        DebugVisuals();
    }

    void UpdateHand(int handId)
    {

        if (Vector3.Distance(handPos[handId], transform.position + transform.rotation * relHandPivPos[handId]) > handMaxDistance
            && ((alternator ^ (handId % 2 == 0)) || 
            Vector3.Distance(handPos[handId], transform.position + transform.rotation * relHandPivPos[handId]) > handMaxDistance * 2))
        {
            Vector3 overshootDir = (transform.position + transform.rotation * relHandPivPos[handId]) - handPos[handId];
            overshootDir = new Vector3(overshootDir.x, 0, overshootDir.z).normalized;
            //handPos[handId] = transform.position + transform.rotation * relHandPivPos[handId] + transform.forward * handMaxDistance;
            handPos[handId] = transform.position + transform.rotation * relHandPivPos[handId] + overshootDir * (Mathf.Abs(Vector3.Dot(overshootDir, transform.forward)) + .25f)/1.25f * handMaxDistance;
        }

        //hands[handId].transform.position = handPos[handId];


        hands[handId].transform.position = Vector3.Slerp(hands[handId].transform.position, handPos[handId], Time.deltaTime * 50);

        if((alternator ^ (handId % 2 == 0))
            //&& Vector3.Distance(handPos[handId], hands[handId].transform.position) < 0.1f)
            && Vector3.Distance(handPos[handId], transform.position + transform.rotation * relHandPivPos[handId]) < .1f)
        {
            alternator = !alternator;
        }
    }

    void DebugVisuals()
    {
        for(int i = 0; i < hands.Count; i++)
        {
            Debug.DrawLine(handPos[i], handPos[i] + Vector3.up * 2);
            Debug.DrawLine(transform.position + transform.rotation * relHandPivPos[i], handPos[i], Color.red);
        }
    }

    int getOtherPaw(int inID)
    {
        if (inID % 2 == 0)
            return (inID / 2) * 2 + 1;
        else
            return (inID / 2) * 2;
    }
}
