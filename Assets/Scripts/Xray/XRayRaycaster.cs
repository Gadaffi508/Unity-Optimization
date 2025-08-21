using System.Collections.Generic;
using UnityEngine;

public class XRaySwitcher : MonoBehaviour
{
    [Header("Ray Source")]
    public Camera cam;
    public float defaultRayLength = 15f;
    public float coneRadiusEnd = 0.7f;
    public float softness = 0.25f;
    public LayerMask hitMask;

    [Header("Materials")]
    public Material xrayMaterial; // sadece SEÇİLİYKEN kullanılacak

    Renderer _current;                        // o an seçili renderer
    readonly Dictionary<Renderer, Material[]> _originalMats = new(); // geri yüklemek için

    float _lastLen;

    void Update()
    {
        if (!cam) return;

        Vector3 ro = cam.transform.position;
        Vector3 rd = cam.transform.forward;

        float len = defaultRayLength;
        Renderer hitRenderer = null;

        if (Physics.Raycast(ro, rd, out var hit, defaultRayLength, hitMask, QueryTriggerInteraction.Ignore))
        {
            len = hit.distance;
            hitRenderer = hit.collider.GetComponentInParent<Renderer>();
        }
        _lastLen = len;

        // shader global’leri (XRay materyali bunları okuyacak)
        Shader.SetGlobalVector("_XR_RayOrigin", ro);
        Shader.SetGlobalVector("_XR_RayDir", rd.normalized);
        Shader.SetGlobalFloat("_XR_RayLength", len);
        Shader.SetGlobalFloat("_XR_ConeRadiusEnd", coneRadiusEnd);
        Shader.SetGlobalFloat("_XR_Softness", softness);

        // materyal swap
        if (_current != hitRenderer)
        {
            Restore(_current);
            ApplyXRay(hitRenderer);
            _current = hitRenderer;
        }
    }

    void ApplyXRay(Renderer r)
    {
        if (!r || !xrayMaterial) return;
        if (!_originalMats.ContainsKey(r))
            _originalMats[r] = r.sharedMaterials; // orijinali sakla

        // tüm submesh’leri XRay’e al (istersen sadece 0. indexi değiştir)
        var arr = r.sharedMaterials;
        for (int i = 0; i < arr.Length; i++) arr[i] = xrayMaterial;
        r.sharedMaterials = arr;
    }

    void Restore(Renderer r)
    {
        if (!r) return;
        if (_originalMats.TryGetValue(r, out var orig))
        {
            r.sharedMaterials = orig;
            _originalMats.Remove(r);
        }
    }

    void OnDrawGizmos()
    {
        if (!cam) return;
        Gizmos.color = Color.white;
        Vector3 ro = cam.transform.position;
        Vector3 rd = cam.transform.forward;
        float len = (_lastLen > 0f) ? _lastLen : defaultRayLength;
        Gizmos.DrawLine(ro, ro + rd * len);
        Gizmos.DrawWireSphere(ro + rd * len, 0.1f);
    }
}
