using System.Collections;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class DissolveControll : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;

    public float refreshRate = 0.025f;

    public float dissolveRate = 0.0125f;

    private Material[] skinnedMats;

    private void Start()
    {
        if (skinnedMesh != null)
        {
            skinnedMats = skinnedMesh.sharedMaterials;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DissolveCo());
        }
    }

    IEnumerator DissolveCo()
    {
        if (skinnedMats.Length > 0)
        {
            float counter = 0;

            while (skinnedMats[0].GetFloat("_DissolveAmount") < 1)
            {
                counter += dissolveRate;
                for (int i = 1; i < skinnedMats.Length; i++)
                {
                    skinnedMats[i].SetFloat("_DissolveAmount", counter);
                }
                yield return new WaitForSeconds(refreshRate);
            }
        }
    }

    private void OnDisable()
    {
        for (int i = 1; i < skinnedMats.Length; i++)
        {
            skinnedMats[i].SetFloat("_DissolveAmount", 0);
        }
    }
}
