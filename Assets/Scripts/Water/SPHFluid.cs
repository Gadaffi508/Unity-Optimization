using System.Collections.Generic;
using UnityEngine;

public class SPHFluid : MonoBehaviour
{
    [Header("SPH Params")]
    public int particleCount = 1000;
    public float mass = 0.02f;
    public float restDensity = 1000f;
    public float stiffness = 2000f;
    public float viscosity = 0.1f;
    public float smoothingRadius = 0.1f;
    public Vector3 gravity = new Vector3(0, -9.81f, 0);

    public int substeps = 3;

    [Header("Rendering")]
    public GameObject particlePrefab;
    public Transform fluidContainer; // parçalıcıları içine yerleştirecek transform

    struct Particle
    {
        public Vector3 pos;
        public Vector3 vel;
        public float density;
        public float pressure;
    }

    List<Particle> particles;
    List<GameObject> visuals;

    // spatial hashing
    Dictionary<Vector3Int, List<int>> grid = new Dictionary<Vector3Int, List<int>>();
    float cellSize;

    void Start()
    {
        cellSize = smoothingRadius;
        particles = new List<Particle>(particleCount);
        visuals  = new List<GameObject>(particleCount);

        // Parçacıkları başlat
        for (int i = 0; i < particleCount; i++)
        {
            Particle p = new Particle();
            p.pos = transform.position + new Vector3(
                Random.Range(-0.5f,0.5f),
                Random.Range(0.0f,0.5f),
                Random.Range(-0.5f,0.5f)
            );
            p.vel = Vector3.zero;
            particles.Add(p);

            // Görsel objeyi instantiate et
            var go = Instantiate(particlePrefab, p.pos, Quaternion.identity, fluidContainer);
            visuals.Add(go);
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime / substeps;
        for (int s = 0; s < substeps; s++)
        {
            BuildGrid();
            ComputeDensityPressure();
            ComputeForces(out Vector3[] forces);
            Integrate(forces, dt);
        }
        UpdateVisuals();
    }


    void BuildGrid()
    {
        grid.Clear();
        for (int i = 0; i < particles.Count; i++)
        {
            Vector3Int cell = WorldToCell(particles[i].pos);
            if (!grid.ContainsKey(cell)) grid[cell] = new List<int>();
            grid[cell].Add(i);
        }
    }

    Vector3Int WorldToCell(Vector3 p)
    {
        return new Vector3Int(
            Mathf.FloorToInt(p.x / cellSize),
            Mathf.FloorToInt(p.y / cellSize),
            Mathf.FloorToInt(p.z / cellSize)
        );
    }

    void ComputeDensityPressure()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            Particle pi = particles[i];
            pi.density = 0f;

            foreach (var j in GetNeighbors(i))
            {
                Particle pj = particles[j];
                float r = Vector3.Distance(pi.pos, pj.pos);
                if (r < smoothingRadius)
                    pi.density += mass * Poly6(r);
            }

            pi.pressure = stiffness * (pi.density - restDensity);
            particles[i] = pi;
        }
    }

    void ComputeForces(out Vector3[] forces)
    {
        forces = new Vector3[particles.Count];
        for (int i = 0; i < particles.Count; i++)
        {
            Particle pi = particles[i];
            Vector3 fPressure = Vector3.zero;
            Vector3 fVisc    = Vector3.zero;

            foreach (var j in GetNeighbors(i))
            {
                if (j == i) continue;
                Particle pj = particles[j];
                Vector3 rij = pi.pos - pj.pos;
                float r = rij.magnitude;

                if (r < smoothingRadius)
                {
                    // basınç
                    fPressure += -rij.normalized * mass * 
                        (pi.pressure + pj.pressure) / (2f * pj.density) * SpikyGrad(r);

                    // viskozite
                    fVisc += viscosity * mass *
                        (pj.vel - pi.vel) / pj.density * ViscLaplacian(r);
                }
            }

            Vector3 fGravity = gravity * pi.density;
            forces[i] = fPressure + fVisc + fGravity;
        }
    }

    void Integrate(Vector3[] forces, float dt)
    {
        for (int i = 0; i < particles.Count; i++)
        {
            Particle p = particles[i];
            p.vel += dt * forces[i] / p.density;
            p.pos += dt * p.vel;
            particles[i] = p;
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < visuals.Count; i++)
            visuals[i].transform.position = particles[i].pos;
    }

    IEnumerable<int> GetNeighbors(int idx)
    {
        Vector3Int cell = WorldToCell(particles[idx].pos);
        for (int x=-1; x<=1; x++)
        for (int y=-1; y<=1; y++)
        for (int z=-1; z<=1; z++)
        {
            var key = new Vector3Int(cell.x + x, cell.y + y, cell.z + z);
            if (!grid.ContainsKey(key)) continue;
            foreach (var j in grid[key]) yield return j;
        }
    }

    // Kernel fonksiyonları
    float Poly6(float r)
    {
        float h2 = smoothingRadius * smoothingRadius;
        float r2 = r * r;
        if (r2 >= 0 && r2 <= h2)
        {
            float x = (h2 - r2);
            return 315f / (64f * Mathf.PI * Mathf.Pow(smoothingRadius, 9)) * x * x * x;
        }
        return 0f;
    }

    float SpikyGrad(float r)
    {
        if (r <= smoothingRadius && r > 0)
        {
            float x = smoothingRadius - r;
            return 45f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6)) * x * x;
        }
        return 0f;
    }

    float ViscLaplacian(float r)
    {
        if (r <= smoothingRadius)
            return 45f / (Mathf.PI * Mathf.Pow(smoothingRadius, 6)) * (smoothingRadius - r);
        return 0f;
    }
}
