using UnityEngine;

// Cartoon FX  - (c) 2015 Jean Moreno

public class CFX_Demo_RandomDirectionTranslate : MonoBehaviour
{
    public Vector3 axis = Vector3.forward;
    public Vector3 baseDir = Vector3.zero;
    private Vector3 dir;
    public bool gravity;
    public float speed = 30.0f;

    private void Start()
    {
        dir = new Vector3(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f)).normalized;
        dir.Scale(axis);
        dir += baseDir;
    }

    private void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime);

        if (gravity) transform.Translate(Physics.gravity * Time.deltaTime);
    }
}