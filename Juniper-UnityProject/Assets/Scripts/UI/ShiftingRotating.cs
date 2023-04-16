using System;
using UnityEngine;

public class ShiftingRotating : MonoBehaviour
{
    float rotaNoise;
    public float radius = 70f;
    public float noiseSpeed = 0.7f;
    float noiseLimit = 2 * 3.1415f;

    public void Update()
    {
        rotaNoise = (rotaNoise + noiseSpeed) % noiseLimit;
        transform.localPosition = new Vector3(
            radius * Mathf.Sin(rotaNoise), -radius * Mathf.Cos(rotaNoise)
        );
    }
}