using UnityEngine;
using System.Collections.Generic;

public class VerletSim : MonoBehaviour
{
    [Header("Grid")]
    public int width = 10;
    public int height = 10;
    public float spacing = 0.5f;

    [Header("Physics")]
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    public int constraintIterations = 6;
    public float damping = 0.98f;

    private VerletParticle[,] particles;
    private List<(int x1, int y1, int x2, int y2, float restLength)> constraints = new();

    private Mesh mesh;
    private Vector3[] meshVertices;
    private int[] meshTriangles;

    private List<CapsuleCollider> colliders = new();

    void Start()
    {
        InitializeGrid();
        GenerateMesh();
        FindColliders();
    }

    void Update()
    {
        Simulate(Time.deltaTime);
        UpdateMesh();
    }

    void FindColliders()
    {
        colliders.Clear();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("ClothCollider");
        foreach (var obj in objs)
        {
            if (obj.TryGetComponent(out CapsuleCollider col))
                colliders.Add(col);
        }
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshVertices = new Vector3[width * height];
        meshTriangles = new int[(width - 1) * (height - 1) * 6];

        int triIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = x + y * width;
                meshVertices[i] = transform.InverseTransformPoint(particles[x, y].position);

                if (x < width - 1 && y < height - 1)
                {
                    int i00 = i;
                    int i10 = i + 1;
                    int i01 = i + width;
                    int i11 = i + width + 1;

                    meshTriangles[triIndex++] = i00;
                    meshTriangles[triIndex++] = i01;
                    meshTriangles[triIndex++] = i10;

                    meshTriangles[triIndex++] = i10;
                    meshTriangles[triIndex++] = i01;
                    meshTriangles[triIndex++] = i11;
                }
            }
        }

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }

    void UpdateMesh()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = x + y * width;
                meshVertices[i] = transform.InverseTransformPoint(particles[x, y].position);
            }
        }

        mesh.vertices = meshVertices;
        mesh.RecalculateNormals();
    }

    void InitializeGrid()
    {
        particles = new VerletParticle[width, height];
        constraints.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 local = new Vector3(x * spacing, -y * spacing, 0);
                Vector3 pos = transform.TransformPoint(local);

                bool isFixed = (y == 0 && (x % (width / 4) == 0 || x == width - 1));
                particles[x, y] = new VerletParticle(pos, isFixed);
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < width - 1) AddConstraint(x, y, x + 1, y);
                if (y < height - 1) AddConstraint(x, y, x, y + 1);
                if (x < width - 1 && y < height - 1) AddConstraint(x, y, x + 1, y + 1);
                if (x > 0 && y < height - 1) AddConstraint(x, y, x - 1, y + 1); // diagonal
            }
        }
    }

    void AddConstraint(int x1, int y1, int x2, int y2)
    {
        float restLength = Vector3.Distance(particles[x1, y1].position, particles[x2, y2].position);
        constraints.Add((x1, y1, x2, y2, restLength));
    }

    void Simulate(float deltaTime)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (particles[x, y].isFixed) continue;

                VerletParticle p = particles[x, y];

                Vector3 velocity = (p.position - p.previousPosition) * damping;
                Vector3 newPosition = p.position + velocity + gravity * (deltaTime * deltaTime);

                p.previousPosition = p.position;
                p.position = newPosition;

                particles[x, y] = p;
            }
        }

        for (int i = 0; i < constraintIterations; i++)
        {
            foreach (var (x1, y1, x2, y2, restLength) in constraints)
            {
                VerletParticle p1 = particles[x1, y1];
                VerletParticle p2 = particles[x2, y2];

                Vector3 delta = p2.position - p1.position;
                float dist = delta.magnitude;
                float diff = (dist - restLength) / dist;
                Vector3 correction = delta * 0.5f * diff;

                if (!p1.isFixed) p1.position += correction;
                if (!p2.isFixed) p2.position -= correction;

                particles[x1, y1] = p1;
                particles[x2, y2] = p2;
            }

            ApplyCapsuleCollisions();
        }
    }

    void ApplyCollisions()
    {
        foreach (var col in colliders)
        {
            Vector3 center = col.transform.position;
            float radius = col.radius * col.transform.lossyScale.x;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (particles[x, y].isFixed) continue;

                    VerletParticle p = particles[x, y];
                    Vector3 toParticle = p.position - center;
                    float dist = toParticle.magnitude;

                    if (dist < radius)
                    {
                        Vector3 normal = toParticle.normalized;
                        float pushOut = radius - dist;
                        p.position += normal * pushOut * 1.05f;
                        p.previousPosition = p.position;
                    }

                    particles[x, y] = p;
                }
            }
        }
    }

    void ApplyCapsuleCollisions()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                VerletParticle p = particles[x, y];
                if (p.isFixed) continue;

                foreach (var col in colliders)
                {
                    Vector3 point1 = col.transform.TransformPoint(col.center + Vector3.up * (col.height / 2 - col.radius));
                    Vector3 point2 = col.transform.TransformPoint(col.center - Vector3.up * (col.height / 2 - col.radius));
                    float radius = col.radius * col.transform.lossyScale.x;

                    Vector3 closest = ClosestPointOnSegment(point1, point2, p.position);
                    Vector3 toParticle = p.position - closest;
                    float dist = toParticle.magnitude;

                    if (dist < radius)
                    {
                        Vector3 normal = toParticle.normalized;
                        float pushOut = radius - dist;
                        p.position += normal * pushOut * 1.2f;
                    }
                }

                particles[x, y] = p;
            }
        }
    }

    Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }



    void OnDrawGizmos()
    {
        if (particles == null) return;

        Gizmos.color = Color.red;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                Gizmos.DrawSphere(particles[x, y].position, 0.02f);

        Gizmos.color = Color.white;
        foreach (var (x1, y1, x2, y2, _) in constraints)
        {
            Gizmos.DrawLine(particles[x1, y1].position, particles[x2, y2].position);
        }
    }
}