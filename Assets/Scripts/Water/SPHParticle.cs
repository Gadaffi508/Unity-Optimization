using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SPHParticle : MonoBehaviour
{
    [HideInInspector] public Vector3 Velocity;
    [HideInInspector] public Vector3 Force;
    [HideInInspector] public float Density;
    [HideInInspector] public float Pressure;

    public float Mass = 1f;

    void Awake()
    {
        Velocity = Vector3.zero;
        Force = Vector3.zero;
    }

    public void UpdateVisual()
    {
        // İsteğe göre parçacık boyutu, renk vs. ekleyebilirsin
        transform.position += Velocity * Time.fixedDeltaTime;
    }
}