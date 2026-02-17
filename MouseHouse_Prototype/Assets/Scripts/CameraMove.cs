using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform Position;
    // Update is called once per frame
    private void Update()
    {
        transform.position = Position.position;
    }
}
