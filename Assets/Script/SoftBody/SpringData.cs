using Unity.Burst;
using Unity.Mathematics;

public struct SpringData
{
    public int a;
    public int b;
    public float restLength;
    public float stiffness;
}


[BurstCompile]
public struct Particle
{
    public float3 position;
    public float3 velocity;
    public float3 force;
    public float mass;
    public bool isFixed;
}