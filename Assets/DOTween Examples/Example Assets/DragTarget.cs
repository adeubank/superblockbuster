using UnityEngine;

public class DragTarget : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 offset;
    private Transform t;

    private void Start()
    {
        t = transform;
        mainCam = Camera.main;
    }

    private void OnMouseDown()
    {
        Vector2 mousePos = Input.mousePosition;
        var distance = mainCam.WorldToScreenPoint(t.position).z;
        var worldPos = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
        offset = t.position - worldPos;
    }

    private void OnMouseDrag()
    {
        Vector2 mousePos = Input.mousePosition;
        var distance = mainCam.WorldToScreenPoint(t.position).z;
        t.position = mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance)) + offset;
    }
}