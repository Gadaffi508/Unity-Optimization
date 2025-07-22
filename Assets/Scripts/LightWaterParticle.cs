using UnityEngine;

public class LightWaterParticle : MonoBehaviour
{
    public Vector3 velocity;
    public float returnSpeed = 4f;
    public float damp = 0.92f;
    public Vector3 targetPosition;

    void Start()
    {
        
    }

    void Update()
    {
        Vector3 toTarget = targetPosition - transform.position;
        velocity += toTarget * returnSpeed * Time.deltaTime;
        velocity *= damp;
        transform.position += velocity * Time.deltaTime;
    }
}