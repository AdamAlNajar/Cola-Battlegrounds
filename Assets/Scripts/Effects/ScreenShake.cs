using UnityEngine;

public class ScreenShake : MonoBehaviour
{
   public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.3f;
    public float dampingSpeed = 1.0f;

    Vector3 initialPosition;
    float currentShakeDuration = 0f;

    void OnEnable()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (currentShakeDuration > 0)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            currentShakeDuration = 0f;
            transform.localPosition = initialPosition;
        }
    }

    public void TriggerShake(float duration = -1f)
    {
        currentShakeDuration = duration > 0 ? duration : shakeDuration;
    }
}
