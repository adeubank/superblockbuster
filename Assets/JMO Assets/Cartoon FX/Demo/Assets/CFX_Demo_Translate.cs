using UnityEngine;

// Cartoon FX  - (c) 2015, Jean Moreno

public class CFX_Demo_Translate : MonoBehaviour
{
    public Vector3 axis = Vector3.forward;
    private Vector3 dir;
    public bool gravity;
    public Vector3 rotation = Vector3.forward;
    public float speed = 30.0f;

    private void Start()
    {
        dir = new Vector3(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f));
        dir.Scale(rotation);
        transform.localEulerAngles = dir;
    }

    private void Update()
    {
        transform.Translate(axis * speed * Time.deltaTime, Space.Self);
    }
}