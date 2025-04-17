using UnityEngine;

public struct VerletParticle
{
    public Vector3 position;
    public Vector3 previousPosition;
    public bool isFixed;

    public VerletParticle(Vector3 pos, bool fixedState)
    {
        position = pos;
        previousPosition = pos;
        isFixed = fixedState;
    }
}