
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(Rigidbody))]
public class WaterDropController : MonoBehaviour
{
    [Header("Drop Settings")]
    public float maxSpreadRadius = 1.5f;
    public float dropDuration = 0.5f;
    public float settleDuration = 1.0f;

    private Material mat;
    private Rigidbody rb;
    private float dropProgress = 0f;
    private bool hasDropped = false;
    private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mat = GetComponent<MeshRenderer>().material;
        mat.SetFloat("_DropProgress", 0f);
    }

    void Update()
    {
        if (!hasDropped && rb.linearVelocity.y < -0.1f && IsGrounded())
        {
            hasDropped = true;
            timer = 0f;
        }

        if (hasDropped && dropProgress < 1f)
        {
            // Animate drop spread
            timer += Time.deltaTime;
            dropProgress = Mathf.Clamp01(timer / dropDuration);
            mat.SetFloat("_DropProgress", dropProgress);
        }
        else if (hasDropped && dropProgress >= 1f && timer < dropDuration + settleDuration)
        {
            // Animate settle back
            timer += Time.deltaTime;
            float t = Mathf.Clamp01((timer - dropDuration) / settleDuration);
            float settle = 1f - t;
            mat.SetFloat("_DropProgress", settle);
        }
    }

    bool IsGrounded()
    {
        // simple raycast
        Ray ray = new Ray(transform.position, Vector3.down);
        return Physics.Raycast(ray, 0.51f);
    }
}
