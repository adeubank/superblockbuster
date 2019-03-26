using UnityEngine;

public class ShakeGameObject : MonoBehaviour
{
    public float decreaseFactor = 1.0f;

    private Vector3 originalPos;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;

    // How long the object should shake for.
    public float shakeDuration;

    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    public Transform shakeObject;

    private void Awake()
    {
        if (shakeObject == null) shakeObject = GetComponent(typeof(Transform)) as Transform;
    }

    private void OnEnable()
    {
        originalPos = shakeObject.localPosition;
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            shakeObject.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
            shakeObject.localPosition = originalPos;
        }
    }
}