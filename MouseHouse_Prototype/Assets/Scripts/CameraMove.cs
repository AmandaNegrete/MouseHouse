using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform Position;
    // Update is called once per frame
    private void Update()
    {
        transform.position = Position.position;
    }
}
