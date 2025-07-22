using System;
using UnityEngine;
using System.Collections.Generic;

public class WaterRenderer : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public List<Transform> particleTransforms;
    
    private bool initialized = false;

    void LateUpdate()
    {
        if (!initialized)
        {
            var generator = FindObjectOfType<WaterBodyGenerator>();
            if (generator != null)
            {
                particleTransforms = generator.GetParticleTransforms();
                initialized = true;
            }
        }
        
        if (particleTransforms == null || particleTransforms.Count == 0)
            return;

        List<Matrix4x4> matrices = new List<Matrix4x4>();

        foreach (var t in particleTransforms)
        {
            if (t != null)
                matrices.Add(Matrix4x4.TRS(t.position, t.rotation, t.localScale));
        }

        for (int i = 0; i < matrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, matrices.Count - i);
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices.GetRange(i, count));
        }
    }
}