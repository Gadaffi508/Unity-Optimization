using UnityEngine;

public class WaterParticle : MonoBehaviour
{
    public Rigidbody rb;
    
    public Vector3 centerForceTarget;
    public float centerForceStrength = 25f;

    void Awake()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.02f;
        rb.linearDamping = 2f;
        rb.angularDamping = 2f;
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
    }
    
    void FixedUpdate()
    {
        Vector3 toCenter = (centerForceTarget - transform.position).normalized;
        rb.AddForce(toCenter * centerForceStrength);
    }

    public void ConnectTo(WaterParticle other, float springForce = 50f, float damping = 10f)
    {
        SpringJoint joint = gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = other.rb;
        joint.spring = springForce;
        joint.damper = damping;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;
        joint.enableCollision = false;
    }
}