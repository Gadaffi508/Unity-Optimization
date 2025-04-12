using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ParticleSim : MonoBehaviour
{
    [SerializeField] int gridWidth = 10;
    [SerializeField] int gridHeight = 10;
    [SerializeField] float spacing = 0.5f;

    private NativeArray<Particle> particles;

    private NativeArray<SpringData> springs;

    private NativeArray<Particle> nextParticles;

    private NativeArray<Particle> currentDrawArray;

    void Start()
    {
        particles = new NativeArray<Particle>(gridWidth * gridHeight, Allocator.Persistent);

        nextParticles = new NativeArray<Particle>(particles.Length, Allocator.Persistent);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int i = x + y * gridWidth;
                Particle p = new Particle
                {
                    position = new float3(x * spacing, y * spacing, 0),
                    velocity = float3.zero,
                    force = float3.zero,
                    mass = 1f,
                    isFixed = (y == gridHeight - 1) // üst sýradakiler sabit
                };
                particles[i] = p;
            }
        }

        var springList = new NativeList<SpringData>(Allocator.Temp);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int i = x + y * gridWidth;

                if (x < gridWidth - 1)
                {
                    int j = (x + 1) + y * gridWidth;
                    springList.Add(new SpringData
                    {
                        a = i,
                        b = j,
                        restLength = spacing,
                        stiffness = 20f
                    });
                }

                if (y < gridHeight - 1)
                {
                    int j = x + (y + 1) * gridWidth;
                    springList.Add(new SpringData
                    {
                        a = i,
                        b = j,
                        restLength = spacing,
                        stiffness = 20f
                    });
                }
            }
        }

        springs = new NativeArray<SpringData>(springList.Length, Allocator.Persistent);
        for (int i = 0; i < springList.Length; i++)
        {
            springs[i] = springList[i];
        }

        springList.Dispose();
    }

    void Update()
    {
        var job = new PhysicsJob
        {
            particles = particles,
            nextParticles = nextParticles,
            springs = springs,
            deltaTime = math.min(Time.deltaTime, 1f / 60f),
            damping = 0.95f,
            gravity = 9.81f
        };

        var handle = job.Schedule(particles.Length, 64);
        handle.Complete();

        // Swap arrays
        var temp = particles;
        particles = nextParticles;
        nextParticles = temp;

        currentDrawArray = particles;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !currentDrawArray.IsCreated)
            return;

        NativeArray<Particle> drawArray = Application.isPlaying ? particles : default;

        if (!drawArray.IsCreated || !springs.IsCreated)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < drawArray.Length; i++)
        {
            Gizmos.DrawSphere(drawArray[i].position, 0.05f);
        }

        Gizmos.color = Color.white;
        for (int i = 0; i < springs.Length; i++)
        {
            var a = drawArray[springs[i].a].position;
            var b = drawArray[springs[i].b].position;
            Gizmos.DrawLine(a, b);
        }
    }

    void OnDestroy()
    {
        if (particles.IsCreated)
            particles.Dispose();

        if (nextParticles.IsCreated)
            nextParticles.Dispose();
    }
}

[BurstCompile]
public struct PhysicsJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Particle> particles;// sadece oku
    public NativeArray<Particle> nextParticles; // buraya yaz

    [ReadOnly] public NativeArray<SpringData> springs;
    public float deltaTime;
    public float damping;
    public float gravity;

    public void Execute(int index)
    {
        Particle p = particles[index];

        if (p.isFixed)
        {
            nextParticles[index] = p;
            return;
        }

        float3 force = float3.zero;
        force += new float3(0, -gravity * p.mass, 0); // Yerçekimi

        // Yay kuvvetleri
        for (int i = 0; i < springs.Length; i++)
        {
            SpringData spring = springs[i];
            if (spring.a == index || spring.b == index)
            {
                int otherIndex = spring.a == index ? spring.b : spring.a;
                Particle other = particles[otherIndex];

                float3 dir = p.position - other.position;
                float dist = math.length(dir);
                float3 n = math.normalize(dir);
                float stretch = dist - spring.restLength;

                force += -spring.stiffness * stretch * n;
            }
        }

        // Hýz ve pozisyon güncelle
        // Hýz ve pozisyon güncelle
        p.velocity += (force / p.mass) * deltaTime;
        p.velocity *= damping;
        p.position += p.velocity * deltaTime;

        // Basit zemin çarpýþmasý   
        if (p.position.y < 2f)
        {
            p.position.y = 2f;
            p.velocity.y *= -0.3f;
        }
        nextParticles[index] = p;
    }
}

