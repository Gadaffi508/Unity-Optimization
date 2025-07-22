using UnityEngine;
using System.Collections.Generic;

public class WaterBodyGenerator : MonoBehaviour
{
    public GameObject particlePrefab;
    public Vector3 gridSize = new Vector3(4, 6, 2);
    public float spacing = 0.4f;

    private List<GameObject> particles = new List<GameObject>();

    void Start()
    {
        //GenerateBody();
        GenerateHumanoidBody();
    }
    
    void Update()
    {
        CalculateCenterAndApplyForce();

        
    }
    
    void CalculateCenterAndApplyForce()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var obj in particles)
        {
            if (obj != null)
            {
                center += obj.transform.position;
                count++;
            }
        }

        if (count == 0) return;

        center /= count;

        foreach (var obj in particles)
        {
            var wp = obj.GetComponent<WaterParticle>();
            if (wp != null)
            {
                wp.centerForceTarget = center;
            }
        }
    }
    
    public List<Transform> GetParticleTransforms()
    {
        List<Transform> transforms = new List<Transform>();
        foreach (var obj in particles)
            if (obj != null)
                transforms.Add(obj.transform);
        return transforms;
    }

    void GenerateBody()
    {
        for (float x = 0; x < gridSize.x; x++)
        {
            for (float y = 0; y < gridSize.y; y++)
            {
                for (float z = 0; z < gridSize.z; z++)
                {
                    Vector3 position = transform.position + new Vector3(x, y, z) * spacing;
                    GameObject p = Instantiate(particlePrefab, position, Quaternion.identity);
                    p.transform.parent = transform;
                    var wp = p.AddComponent<WaterParticle>();
                    
                    foreach (GameObject otherObj in particles)
                    {
                        float dist = Vector3.Distance(p.transform.position, otherObj.transform.position);
                        if (dist < spacing * 1.1f)
                        {
                            WaterParticle otherWP = otherObj.GetComponent<WaterParticle>();
                            if (otherWP != null)
                                wp.ConnectTo(otherWP);
                        }
                    }
                    particles.Add(p);
                }
            }
        }
    }
    
    void GenerateHumanoidBody()
    {
        particles.Clear();

        CreateSphereCluster(center: transform.position + Vector3.up * 2.5f, radius: 0.3f, count: 5);

        CreateLineCluster(start: transform.position + Vector3.up * 2f, end: transform.position + Vector3.up * 1f, step: 0.3f);

        CreateLineCluster(start: transform.position + new Vector3(0.3f, 2f, 0), end: transform.position + new Vector3(1f, 1.5f, 0), step: 0.3f);

        CreateLineCluster(start: transform.position + new Vector3(-0.3f, 2f, 0), end: transform.position + new Vector3(-1f, 1.5f, 0), step: 0.3f);

        CreateLineCluster(start: transform.position + new Vector3(0.2f, 1f, 0), end: transform.position + new Vector3(0.2f, 0f, 0), step: 0.3f);

        CreateLineCluster(start: transform.position + new Vector3(-0.2f, 1f, 0), end: transform.position + new Vector3(-0.2f, 0f, 0), step: 0.3f);
    }

    
    void CreateLineCluster(Vector3 start, Vector3 end, float step)
    {
        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        int count = Mathf.CeilToInt(distance / step);

        for (int i = 0; i <= count; i++)
        {
            Vector3 pos = start + dir * step * i;
            CreateParticleAt(pos);
        }
    }

    void CreateSphereCluster(Vector3 center, float radius, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Random.onUnitSphere * radius;
            Vector3 pos = center + offset;
            CreateParticleAt(pos);
        }
    }

    void CreateParticleAt(Vector3 position)
    {
        GameObject p = Instantiate(particlePrefab, position, Quaternion.identity);
        p.transform.parent = transform;

        var wp = p.AddComponent<WaterParticle>();

        foreach (GameObject otherObj in particles)
        {
            if (Vector3.Distance(p.transform.position, otherObj.transform.position) < spacing * 1.1f)
            {
                WaterParticle otherWP = otherObj.GetComponent<WaterParticle>();
                if (otherWP != null)
                    wp.ConnectTo(otherWP);
            }
        }

        particles.Add(p);
    }

}