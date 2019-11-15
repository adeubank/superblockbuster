using UnityEngine;

// Cartoon FX  - (c) 2015 Jean Moreno

public class CFX_Demo_RotateCamera : MonoBehaviour
{
    public static bool rotating = true;
    public Transform rotationCenter;

    public float speed = 30.0f;

    private void Update()
    {
        if (rotating)
            transform.RotateAround(rotationCenter.position, Vector3.up, speed * Time.deltaTime);
    }
}