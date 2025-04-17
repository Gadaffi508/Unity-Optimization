using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class VerletClothBurst : MonoBehaviour
{
    [Header("Grid")]
    public int width = 15;
    public int height = 15;
    public float spacing = 0.25f;

    [Header("Physics")]
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    public int constraintIterations = 5;
    public float damping = 0.98f;

    [Header("Collision")]
    public Transform sphere;
    public float sphereRadius = 1.0f;

    NativeArray<VerletParticles> particles;
    NativeArray<Spring> springs;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    void Start()
    {
        int particleCount = width * height;
        particles = new NativeArray<VerletParticles>(particleCount, Allocator.Persistent);
        NativeList<Spring> springList = new NativeList<Spring>(Allocator.Temp);

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                Vector3 pos = transform.TransformPoint(new Vector3(x * spacing, -y * spacing, 0));
                particles[index] = new VerletParticles
                {
                    position = pos,
                    previous = pos,
                    isFixed = y == 0 && (x % (width / 4) == 0 || x == width - 1)
                };

                if (x < width - 1)
                    springList.Add(new Spring(index, index + 1, spacing));
                if (y < height - 1)
                    springList.Add(new Spring(index, index + width, spacing));
                if (x < width - 1 && y < height - 1)
                    springList.Add(new Spring(index, index + width + 1, spacing * math.sqrt(2)));
                if (x > 0 && y < height - 1)
                    springList.Add(new Spring(index, index + width - 1, spacing * math.sqrt(2)));
            }

        springs = new NativeArray<Spring>(springList.Length, Allocator.Persistent);
        springs.CopyFrom(springList);
        springList.Dispose();

        CreateMesh();
    }

    void CreateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[particles.Length];
        triangles = new int[(width - 1) * (height - 1) * 6];
        int t = 0;

        for (int y = 0; y < height - 1; y++)
            for (int x = 0; x < width - 1; x++)
            {
                int i = x + y * width;
                triangles[t++] = i;
                triangles[t++] = i + width;
                triangles[t++] = i + 1;
                triangles[t++] = i + 1;
                triangles[t++] = i + width;
                triangles[t++] = i + width + 1;
            }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void Update()
    {
        VerletJob verletJob = new VerletJob
        {
            particles = particles,
            gravity = gravity,
            deltaTime = Time.deltaTime,
            damping = damping,
            sphereCenter = sphere.position,
            sphereRadius = sphereRadius
        };

        JobHandle verletHandle = verletJob.Schedule(particles.Length, 64);
        verletHandle.Complete();

        var springJob = new SpringJob { particles = particles, springs = springs };
        springJob.Schedule().Complete();

        for (int i = 0; i < particles.Length; i++)
            vertices[i] = transform.InverseTransformPoint(particles[i].position);

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    void OnDestroy()
    {
        if (particles.IsCreated) particles.Dispose();
        if (springs.IsCreated) springs.Dispose();
    }
}

public struct Spring
{
    public int a, b;
    public float restLength;

    public Spring(int a, int b, float rest)
    {
        this.a = a;
        this.b = b;
        this.restLength = rest;
    }
}

public struct VerletParticles
{
    public float3 position;
    public float3 previous;
    public bool isFixed;
}

[BurstCompile]
public struct VerletJob : IJobParallelFor
{
    public NativeArray<VerletParticles> particles;
    public float3 gravity;
    public float deltaTime;
    public float damping;
    public float3 sphereCenter;
    public float sphereRadius;

    public void Execute(int i)
    {
        var p = particles[i];
        if (p.isFixed)
            return;

        float3 velocity = (p.position - p.previous) * damping;
        float3 next = p.position + velocity + gravity * deltaTime * deltaTime;

        // Sphere collision
        float3 to = next - sphereCenter;
        float dist = math.length(to);
        if (dist < sphereRadius)
        {
            float3 normal = math.normalize(to);
            next = sphereCenter + normal * sphereRadius;
        }

        p.previous = p.position;
        p.position = next;
        particles[i] = p;
    }
}

[BurstCompile]
public struct SpringJob : IJob
{
    public NativeArray<VerletParticles> particles;
    [ReadOnly] public NativeArray<Spring> springs;

    public void Execute()
    {
        for (int i = 0; i < springs.Length; i++)
        {
            var s = springs[i];
            var p1 = particles[s.a];
            var p2 = particles[s.b];

            float3 delta = p2.position - p1.position;
            float dist = math.length(delta);
            float diff = (dist - s.restLength) / dist;

            if (!p1.isFixed)
                p1.position += delta * 0.5f * diff;
            if (!p2.isFixed)
                p2.position -= delta * 0.5f * diff;

            particles[s.a] = p1;
            particles[s.b] = p2;
        }
    }
}

