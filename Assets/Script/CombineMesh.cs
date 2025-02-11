using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineMesh : MonoBehaviour
{
    void Start()
    {
        Dictionary<Material, List<CombineInstance>> materialToCombineInstances = new Dictionary<Material, List<CombineInstance>>();

        foreach (Transform child in transform)
        {
            MeshFilter childFilter = child.GetComponent<MeshFilter>();
            MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();

            if (childFilter != null && childRenderer != null)
            {
                Material childMaterial = childRenderer.sharedMaterial;

                if (!materialToCombineInstances.ContainsKey(childMaterial))
                {
                    materialToCombineInstances[childMaterial] = new List<CombineInstance>();
                }

                CombineInstance combineInstance = new CombineInstance
                {
                    mesh = childFilter.sharedMesh,
                    transform = child.localToWorldMatrix
                };

                materialToCombineInstances[childMaterial].Add(combineInstance);
                child.gameObject.SetActive(false);
            }
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        List<CombineInstance> finalCombine = new List<CombineInstance>();
        List<Material> materials = new List<Material>();

        foreach (var entry in materialToCombineInstances)
        {
            Mesh materialMesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };

            materialMesh.CombineMeshes(entry.Value.ToArray(), true, true);

            CombineInstance combineInstance = new CombineInstance
            {
                mesh = materialMesh,
                transform = Matrix4x4.identity
            };

            finalCombine.Add(combineInstance);
            materials.Add(entry.Key);
        }

        Mesh combinedMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        combinedMesh.CombineMeshes(finalCombine.ToArray(), false, false);

        meshFilter.sharedMesh = combinedMesh;
        meshRenderer.sharedMaterials = materials.ToArray();

        gameObject.SetActive(true);
    }

}
