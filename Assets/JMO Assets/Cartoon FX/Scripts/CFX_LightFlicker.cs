using UnityEngine;

// Cartoon FX  - (c) 2015 Jean Moreno

// Randomly changes a light's intensity over time.

[RequireComponent(typeof(Light))]
public class CFX_LightFlicker : MonoBehaviour
{
    /// Max intensity will be: baseIntensity + addIntensity
    public float addIntensity = 1.0f;

    private float baseIntensity;

    // Loop flicker effect
    public bool loop;
    private float maxIntensity;

    private float minIntensity;

    // Perlin scale: makes the flicker more or less smooth
    public float smoothFactor = 1f;

    private void Awake()
    {
        baseIntensity = GetComponent<Light>().intensity;
    }

    private void OnEnable()
    {
        minIntensity = baseIntensity;
        maxIntensity = minIntensity + addIntensity;
    }

    private void Update()
    {
        GetComponent<Light>().intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(Time.time * smoothFactor, 0f));
    }
}