using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

public class ShaderPreviewWindow : EditorWindow
{
    private enum MeshType { Sphere, Cube, Plane, Custom }
    private enum DebugView { None, UV, Normals, WorldPos }

    // UI/Preview
    private Vector2 scrollPos;
    private MeshType meshType = MeshType.Sphere;
    private Mesh previewMesh;
    private Mesh customMesh;
    private Material previewMaterial;
    private PreviewRenderUtility previewRenderer;

    // camera/orbit
    private Vector2 orbit = new Vector2(20f, -15f);
    private float distance = 3.0f;
    private Vector2 lastMouse;
    private bool dragging;

    // lighting & bg
    private float lightIntensity = 1.25f;
    private Color lightColor = Color.white;
    private Color bgColor = new Color(0.18f, 0.18f, 0.18f);

    // skybox / environment
    private bool useSkybox = false;
    private Material skyboxMaterial;

    // hot-reload
    private bool autoHotReload = true;
    private double lastReloadTime;

    // scene apply
    private GameObject targetObject;
    private int applySlotIndex = -1; // -1 = tüm slotlar
    private bool restoreOnClose = true;
    private bool isApplied = false;
    private readonly List<AppliedRenderer> applied = new();

    // debug views
    private DebugView debugView = DebugView.None;
    private readonly Dictionary<DebugView, Material> debugMats = new();

    [MenuItem("Tools/Shader Preview Debugger")]
    public static void ShowWindow()
    {
        var window = GetWindow<ShaderPreviewWindow>();
        window.titleContent = new GUIContent("Shader Preview");
        window.Show();
    }

    private void OnEnable()
    {
        previewRenderer = new PreviewRenderUtility();
        previewRenderer.cameraFieldOfView = 30f;
        previewRenderer.ambientColor = new Color(0.15f, 0.15f, 0.15f);

        previewMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        previewMaterial = new Material(Shader.Find("Standard"));

        // watcher
        ShaderPreviewAssetWatcher.OnRelevantAssetsChanged += OnAssetsChanged;

        // hazır değilse debug shaderlarını üret
        EnsureAllDebugShaders();
        LoadDebugMaterials();
    }

    private void OnDisable()
    {
        ShaderPreviewAssetWatcher.OnRelevantAssetsChanged -= OnAssetsChanged;

        if (restoreOnClose) RestoreApplied();
        if (previewRenderer != null) { previewRenderer.Cleanup(); previewRenderer = null; }
        if (!Application.isPlaying && previewMaterial != null) DestroyImmediate(previewMaterial);
        foreach (var kv in debugMats) { if (kv.Value) DestroyImmediate(kv.Value); }
        debugMats.Clear();
    }

    // --------- GUI ---------
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("Shader Preview Debugger", EditorStyles.boldLabel);

        // material + actions
        EditorGUILayout.BeginHorizontal();
        previewMaterial = (Material)EditorGUILayout.ObjectField("Material", previewMaterial, typeof(Material), false);
        using (new EditorGUI.DisabledScope(previewMaterial == null || previewMaterial.shader == null))
        {
            if (GUILayout.Button("Reimport Shader", GUILayout.Width(120)))
            {
                string sp = AssetDatabase.GetAssetPath(previewMaterial.shader);
                if (!string.IsNullOrEmpty(sp)) AssetDatabase.ImportAsset(sp);
            }
        }
        EditorGUILayout.EndHorizontal();
        autoHotReload = EditorGUILayout.ToggleLeft("Auto Hot-Reload", autoHotReload);

        // mesh picker
        meshType = (MeshType)EditorGUILayout.EnumPopup("Mesh", meshType);
        if (meshType == MeshType.Custom)
            customMesh = (Mesh)EditorGUILayout.ObjectField("Custom Mesh", customMesh, typeof(Mesh), false);

        // lighting / env
        lightIntensity = EditorGUILayout.Slider("Light Intensity", lightIntensity, 0f, 5f);
        lightColor = EditorGUILayout.ColorField("Light Color", lightColor);

        useSkybox = EditorGUILayout.Toggle("Use Skybox", useSkybox);
        if (useSkybox)
            skyboxMaterial = (Material)EditorGUILayout.ObjectField("Skybox Material", skyboxMaterial, typeof(Material), false);
        else
            bgColor = EditorGUILayout.ColorField("Background", bgColor);

        // debug view
        debugView = (DebugView)EditorGUILayout.EnumPopup("Debug View", debugView);

        // shader properties
        if (previewMaterial != null)
        {
            EditorGUILayout.Space(6);
            DrawMaterialProperties(previewMaterial);
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Preview")) Repaint();
        EditorGUILayout.EndHorizontal();

        // scene apply
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Apply to Scene Object", EditorStyles.boldLabel);
        targetObject = (GameObject)EditorGUILayout.ObjectField("Target", targetObject, typeof(GameObject), true);
        if (targetObject == null && Selection.activeGameObject != null)
        {
            if (GUILayout.Button("Use Current Selection")) targetObject = Selection.activeGameObject;
        }
        applySlotIndex = EditorGUILayout.IntField(new GUIContent("Material Slot", "-1 = all slots"), applySlotIndex);
        restoreOnClose = EditorGUILayout.ToggleLeft("Restore on Window Close", restoreOnClose);

        using (new EditorGUI.DisabledScope(previewMaterial == null || targetObject == null))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(false, "Apply", "ButtonLeft")) { ApplyToTarget(targetObject, applySlotIndex); }
            using (new EditorGUI.DisabledScope(!isApplied))
            {
                if (GUILayout.Toggle(false, "Restore", "ButtonRight")) { RestoreApplied(); }
            }
            EditorGUILayout.EndHorizontal();
        }

        // preview rect
        Rect rect = GUILayoutUtility.GetRect(200, Mathf.Max(240, position.height - 320));
        HandleMouse(rect);
        DrawPreview(rect);

        EditorGUILayout.EndScrollView();
    }

    // --------- Mouse orbit/zoom ---------
    private void HandleMouse(Rect rect)
    {
        var e = Event.current;
        if (!rect.Contains(e.mousePosition)) return;

        if (e.type == EventType.ScrollWheel)
        {
            distance = Mathf.Clamp(distance + e.delta.y * 0.05f, 1.0f, 10f);
            e.Use(); Repaint();
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        { dragging = true; lastMouse = e.mousePosition; e.Use(); }
        if (e.type == EventType.MouseUp && e.button == 0) dragging = false;

        if (dragging && e.type == EventType.MouseDrag)
        {
            Vector2 d = e.mousePosition - lastMouse; lastMouse = e.mousePosition;
            orbit.x += d.x * 0.4f;
            orbit.y = Mathf.Clamp(orbit.y - d.y * 0.4f, -85f, 85f);
            e.Use(); Repaint();
        }
    }

    // --------- Draw Preview ---------
    private void DrawPreview(Rect rect)
    {
        var mesh = GetMesh();
        if (mesh == null) return;

        var matToUse = GetActivePreviewMaterial();
        if (matToUse == null) return;

        previewRenderer.BeginPreview(rect, GUIStyle.none);

        var cam = previewRenderer.camera;
        cam.transform.position = Quaternion.Euler(orbit.y, orbit.x, 0f) * (Vector3.back * distance);
        cam.transform.LookAt(Vector3.zero);
        cam.nearClipPlane = 0.01f; cam.farClipPlane = 100f;

        SetupLights();

        // background
        if (useSkybox && skyboxMaterial != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            var sky = cam.gameObject.GetComponent<Skybox>() ?? cam.gameObject.AddComponent<Skybox>();
            sky.material = skyboxMaterial;
        }
        else
        {
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = bgColor;
            var sky = cam.gameObject.GetComponent<Skybox>();
            if (sky) DestroyImmediate(sky);
        }

        // draw
        previewRenderer.DrawMesh(mesh, Matrix4x4.identity, matToUse, 0);
        cam.Render();

        var tex = previewRenderer.EndPreview();
        GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);
    }

    private void SetupLights()
    {
        var l0 = previewRenderer.lights[0];
        l0.color = lightColor;
        l0.intensity = lightIntensity;
        l0.transform.rotation = Quaternion.Euler(40f, 40f, 0f);

        var l1 = previewRenderer.lights[1];
        l1.color = Color.white;
        l1.intensity = lightIntensity * 0.5f;
        l1.transform.rotation = Quaternion.Euler(340f, 220f, 0f);

        previewRenderer.ambientColor = new Color(0.15f, 0.15f, 0.15f);
    }

    private Mesh GetMesh()
    {
        switch (meshType)
        {
            case MeshType.Cube: return Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            case MeshType.Plane: return Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            case MeshType.Custom: return customMesh != null ? customMesh : Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            default: return Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        }
    }

    // --------- Shader Properties UI ---------
    private void DrawMaterialProperties(Material mat)
    {
        if (mat.shader == null) return;
        int count = GetShaderPropertyCount(mat.shader);
        if (count <= 0) return;

        EditorGUILayout.LabelField("Shader Properties", EditorStyles.boldLabel);

        for (int i = 0; i < count; i++)
        {
            var type = GetShaderPropertyType(mat.shader, i);
            string name = GetShaderPropertyName(mat.shader, i);

            switch (type)
            {
                case ShaderPropertyType.Color:
                    { var v = mat.GetColor(name); var nv = EditorGUILayout.ColorField(ObjectNames.NicifyVariableName(name), v); if (nv != v) mat.SetColor(name, nv); }
                    break;
                case ShaderPropertyType.Vector:
                    { var v = mat.GetVector(name); var nv = EditorGUILayout.Vector4Field(ObjectNames.NicifyVariableName(name), v); if (nv != v) mat.SetVector(name, nv); }
                    break;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    {
                        float v = mat.GetFloat(name);
                        Vector2 range = GetRangeLimits(mat.shader, i);
                        float nv = (range != Vector2.zero)
                            ? EditorGUILayout.Slider(ObjectNames.NicifyVariableName(name), v, range.x, range.y)
                            : EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(name), v);
                        if (!Mathf.Approximately(nv, v)) mat.SetFloat(name, nv);
                    }
                    break;
                case ShaderPropertyType.TexEnv:
                    {
                        Texture t = mat.GetTexture(name);
                        Texture nt = (Texture)EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(name), t, typeof(Texture), false);
                        if (nt != t) mat.SetTexture(name, nt);

                        var st = mat.GetTextureScale(name);
                        var of = mat.GetTextureOffset(name);
                        var nst = EditorGUILayout.Vector2Field("  Tiling", st);
                        var nof = EditorGUILayout.Vector2Field("  Offset", of);
                        if (nst != st) mat.SetTextureScale(name, nst);
                        if (nof != of) mat.SetTextureOffset(name, nof);
                    }
                    break;
            }
        }
    }

    // --------- Hot Reload Watcher ---------
    private void OnAssetsChanged(string[] imported)
    {
        if (!autoHotReload || previewMaterial == null || previewMaterial.shader == null) return;

        string shaderPath = AssetDatabase.GetAssetPath(previewMaterial.shader);
        bool affected = false;

        if (!string.IsNullOrEmpty(shaderPath))
        {
            foreach (var p in imported)
            {
                if (string.Equals(p, shaderPath, StringComparison.OrdinalIgnoreCase) ||
                    p.EndsWith(".hlsl", StringComparison.OrdinalIgnoreCase) ||
                    p.EndsWith(".cginc", StringComparison.OrdinalIgnoreCase) ||
                    p.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                { affected = true; break; }
            }
        }
        if (!affected) return;

        if (EditorApplication.timeSinceStartup - lastReloadTime < 0.2f) return;
        lastReloadTime = EditorApplication.timeSinceStartup;

        EditorApplication.delayCall += () =>
        {
            if (!string.IsNullOrEmpty(shaderPath))
                AssetDatabase.ImportAsset(shaderPath, ImportAssetOptions.ForceUpdate);
            Repaint();
        };
    }

    // --------- Scene Apply / Restore ---------
    private void ApplyToTarget(GameObject go, int slotIndex)
    {
        if (previewMaterial == null || go == null) return;

        RestoreApplied(); // önce temizle
        applied.Clear();

        var renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            var originals = r.sharedMaterials;
            var newMats = (Material[])originals.Clone();

            if (slotIndex < 0)
            {
                for (int i = 0; i < newMats.Length; i++) newMats[i] = previewMaterial;
            }
            else if (slotIndex < newMats.Length)
            {
                newMats[slotIndex] = previewMaterial;
            }

            applied.Add(new AppliedRenderer(r, originals));
            r.sharedMaterials = newMats;
            EditorUtility.SetDirty(r);
        }

        isApplied = applied.Count > 0;
    }

    private void RestoreApplied()
    {
        if (!isApplied) return;
        foreach (var a in applied)
        {
            if (a.renderer) { a.renderer.sharedMaterials = a.originals; EditorUtility.SetDirty(a.renderer); }
        }
        applied.Clear();
        isApplied = false;
    }

    private struct AppliedRenderer
    {
        public Renderer renderer;
        public Material[] originals;
        public AppliedRenderer(Renderer r, Material[] o) { renderer = r; originals = o; }
    }

    // --------- Debug Views ---------
    private Material GetActivePreviewMaterial()
    {
        if (debugView == DebugView.None || !debugMats.ContainsKey(debugView) || debugMats[debugView] == null)
            return previewMaterial;
        return debugMats[debugView];
    }

    private void LoadDebugMaterials()
    {
        debugMats.Clear();
        debugMats[DebugView.UV]      = CreateMat("Hidden/SPD/DebugUV");
        debugMats[DebugView.Normals] = CreateMat("Hidden/SPD/DebugNormals");
        debugMats[DebugView.WorldPos]= CreateMat("Hidden/SPD/DebugWorldPos");
    }

    private static Material CreateMat(string shaderName)
    {
        var sh = Shader.Find(shaderName);
        return sh ? new Material(sh) { hideFlags = HideFlags.HideAndDontSave } : null;
    }

    private void EnsureAllDebugShaders()
    {
        EnsureDebugShader("DebugUV", kSrcDebugUV);
        EnsureDebugShader("DebugNormals", kSrcDebugNormals);
        EnsureDebugShader("DebugWorldPos", kSrcDebugWorldPos);
    }

    private static void EnsureDebugShader(string shortName, string source)
    {
        string dir = "Assets/ShaderPreview/Generated";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string path = $"{dir}/{shortName}.shader";

        if (!File.Exists(path))
        {
            File.WriteAllText(path, source);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }
    }

    // --------- ShaderUtil reflection helpers ---------
    private enum ShaderPropertyType { Color, Vector, Float, Range, TexEnv, Unknown }

    private static int GetShaderPropertyCount(Shader s)
    {
        var t = typeof(ShaderUtil);
        var m = t.GetMethod("GetPropertyCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        return (int)m.Invoke(null, new object[] { s });
    }

    private static string GetShaderPropertyName(Shader s, int index)
    {
        var t = typeof(ShaderUtil);
        var m = t.GetMethod("GetPropertyName", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        return (string)m.Invoke(null, new object[] { s, index });
    }

    private static ShaderPropertyType GetShaderPropertyType(Shader s, int index)
    {
        var t = typeof(ShaderUtil);
        var m = t.GetMethod("GetPropertyType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var val = m.Invoke(null, new object[] { s, index });
        switch (val.ToString())
        {
            case "Color": return ShaderPropertyType.Color;
            case "Vector": return ShaderPropertyType.Vector;
            case "Float":  return ShaderPropertyType.Float;
            case "Range":  return ShaderPropertyType.Range;
            case "TexEnv": return ShaderPropertyType.TexEnv;
            default:       return ShaderPropertyType.Unknown;
        }
    }

    private static Vector2 GetRangeLimits(Shader s, int index)
    {
        try
        {
            var t = typeof(ShaderUtil);
            var m = t.GetMethod("GetRangeLimits", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            object[] call = new object[] { s, index, null, null };
            m.Invoke(null, call);
            return new Vector2((float)call[2], (float)call[3]);
        }
        catch { return Vector2.zero; }
    }

    // --------- Embedded Debug Shaders (Built-in RP) ---------
    private const string kSrcDebugUV =
@"Shader ""Hidden/SPD/DebugUV""{
SubShader{ Tags{ ""RenderType""=""Opaque"" } ZWrite On Cull Back
Pass{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include ""UnityCG.cginc""
struct appdata{ float4 vertex:POSITION; float2 uv:TEXCOORD0; };
struct v2f{ float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
fixed4 frag(v2f i):SV_Target{ return fixed4(frac(i.uv),0,1); }
ENDCG
}}}";

    private const string kSrcDebugNormals =
@"Shader ""Hidden/SPD/DebugNormals""{
SubShader{ Tags{ ""RenderType""=""Opaque"" } ZWrite On Cull Back
Pass{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include ""UnityCG.cginc""
struct appdata{ float4 vertex:POSITION; float3 normal:NORMAL; };
struct v2f{ float4 pos:SV_POSITION; float3 n: TEXCOORD0; };
v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.n = UnityObjectToWorldNormal(v.normal); return o; }
fixed4 frag(v2f i):SV_Target{ return fixed4(0.5 + 0.5*normalize(i.n), 1); }
ENDCG
}}}";

    private const string kSrcDebugWorldPos =
@"Shader ""Hidden/SPD/DebugWorldPos""{
SubShader{ Tags{ ""RenderType""=""Opaque"" } ZWrite On Cull Back
Pass{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include ""UnityCG.cginc""
struct appdata{ float4 vertex:POSITION; };
struct v2f{ float4 pos:SV_POSITION; float3 wp: TEXCOORD0; };
v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.wp = mul(unity_ObjectToWorld, v.vertex).xyz; return o; }
fixed4 frag(v2f i):SV_Target{
    float3 c = frac(i.wp * 0.1); // 10 dünya biriminde sarmal renk
    return fixed4(c,1);
}
ENDCG
}}}";
}