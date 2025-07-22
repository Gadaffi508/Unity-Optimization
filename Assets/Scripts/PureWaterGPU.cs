using UnityEngine;
using System.Collections.Generic;

public class PureWaterGPU : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    public Vector3Int gridSize = new Vector3Int(10, 10, 10);
    public float spacing = 0.3f;

    private List<Matrix4x4> matrices = new List<Matrix4x4>();
    private List<Vector3> positions = new List<Vector3>();
    private List<Vector3> velocities = new List<Vector3>();

    private List<Vector3> startPositions = new List<Vector3>();

    private float groundLevelY;

    void Start()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    Vector3 localPos = new Vector3(x, y, z) * spacing;
                    Vector3 worldPos = transform.position + localPos;
                    positions.Add(worldPos);
                    velocities.Add(Vector3.zero);
                    startPositions.Add(worldPos);
                }
            }
        }
    }

    void Update()
    {
        matrices.Clear();

        for (int i = 0; i < positions.Count; i++)
        {
            // gravity uygula
            velocities[i] += new Vector3(0, -9.81f, 0) * Time.deltaTime;

            // hedef pozisyona doğru çek
            Vector3 toTarget = startPositions[i] - positions[i];
            velocities[i] += toTarget * 4f * Time.deltaTime;
            
            // damping
            velocities[i] *= 0.92f;

            // pozisyon güncelle
            positions[i] += velocities[i] * Time.deltaTime;

            Vector3 pos = positions[i];
            if (pos.y < groundLevelY)
            {
                pos.y = groundLevelY;
                Vector3 vel = velocities[i];
                vel.y *= -0.5f;
                velocities[i] = vel;
            }
            positions[i] = pos;


            // çizim
            matrices.Add(Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one * 0.3f));
        }


        for (int i = 0; i < matrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, matrices.Count - i);
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices.GetRange(i, count));
        }

        
    }
}