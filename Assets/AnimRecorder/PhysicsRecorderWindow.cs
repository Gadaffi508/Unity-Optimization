using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;

public class PhysicsRecorderWindow : EditorWindow
{
    private GameObject targetObject;
    private PhysicsRecorder recorder;
    private bool isRecording;

    private bool addToExistingAnimator = true;
    private string lastSavedPath = "";

    [MenuItem("Tools/Physics Recorder")]
    public static void ShowWindow() => GetWindow<PhysicsRecorderWindow>("Physics Recorder");

    void OnGUI()
    {
        GUILayout.Space(4);
        DrawHeader();

        using (new EditorGUILayout.VerticalScope("box"))
        {
            targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
            addToExistingAnimator = EditorGUILayout.ToggleLeft("Add clip to existing Animator Controller (don’t create new)", addToExistingAnimator);
            EditorGUILayout.LabelField("Status", isRecording ? "Recording..." : "Idle", isRecording ? EditorStyles.boldLabel : EditorStyles.label);
            if (!string.IsNullOrEmpty(lastSavedPath))
                EditorGUILayout.LabelField("Last Saved", lastSavedPath);
        }

        GUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = Application.isPlaying && !isRecording;
            if (GUILayout.Button("Start Recording", GUILayout.Height(28)))
                StartRec();
            GUI.enabled = Application.isPlaying && isRecording;
            if (GUILayout.Button("Stop & Save", GUILayout.Height(28)))
                StopAndSave();
            GUI.enabled = true;
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Play Mode’a geç ve sonra kaydı başlat.", MessageType.Info);

        GUILayout.Space(6);
        EditorGUILayout.HelpBox("İpucu: Fizik animasyonu oynatırken Rigidbody ile kavga etmesin diye oynatma sırasında RB’i geçici olarak isKinematic yapmayı düşünebilirsin.", MessageType.None);
    }

    private void DrawHeader()
    {
        var r = EditorGUILayout.GetControlRect(false, 26);
        EditorGUI.DrawRect(new Rect(r.x, r.y + 24, r.width, 1), new Color(0,0,0,0.15f));
        GUI.Label(r, "Physics → Animation Recorder", EditorStyles.boldLabel);
    }

    private void StartRec()
    {
        if (targetObject == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen bir Target Object atayın.", "Tamam");
            return;
        }
        recorder = targetObject.GetComponent<PhysicsRecorder>();
        if (recorder == null) recorder = targetObject.AddComponent<PhysicsRecorder>();
        recorder.target = targetObject.transform;
        recorder.StartRecording();
        isRecording = true;
    }

    private void StopAndSave()
    {
        if (recorder == null) { isRecording = false; return; }
        recorder.StopRecording();
        var posList = new List<Vector3>(recorder.Positions);
        var rotList = new List<Quaternion>(recorder.Rotations);
        var timesList = new List<float>(recorder.Times);

        if (posList.Count == 0)
        {
            Debug.LogWarning("Kayıtta örnek yok.");
            isRecording = false;
            return;
        }

        var clip = CreateClipFromSamples(posList, rotList, timesList);

        string folder = "Assets";
        var rec = targetObject.GetComponent<PhysicsRecorder>();
        if (rec != null)
        {
            MonoScript ms = MonoScript.FromMonoBehaviour(rec);
            string scriptPath = AssetDatabase.GetAssetPath(ms);
            if (!string.IsNullOrEmpty(scriptPath))
            {
                folder = Path.GetDirectoryName(scriptPath).Replace("\\", "/");
                if (string.IsNullOrEmpty(folder)) folder = "Assets";
            }
        }
        if (!AssetDatabase.IsValidFolder(folder)) folder = "Assets";

        string filename = $"{targetObject.name}_PhysicsAnim.anim";
        string savePath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, filename));
        AssetDatabase.CreateAsset(clip, savePath);
        AssetDatabase.SaveAssets();
        lastSavedPath = savePath;
        Debug.Log($"Animation saved to {savePath}");

        if (addToExistingAnimator)
        {
            var animator = targetObject.GetComponent<Animator>();
            var controller = animator != null ? animator.runtimeAnimatorController as AnimatorController : null;

            if (animator == null || controller == null)
            {
                Debug.LogWarning("Mevcut Animator/Controller bulunamadı. Klip kaydedildi, ancak herhangi bir controller’a eklenmedi.");
            }
            else
            {
                var sm = controller.layers[0].stateMachine;
                var state = sm.AddState(clip.name);
                state.motion = clip;

                if (sm.defaultState == null) sm.defaultState = state;

                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }
        }

        isRecording = false;
        Repaint();
    }
    
    private static AnimationClip CreateClipFromSamples(List<Vector3> posList, List<Quaternion> rotList, List<float> timesList)
    {
        var clip = new AnimationClip
        {
            frameRate = 1f / Mathf.Max(Time.fixedDeltaTime, 0.0001f),
            legacy = false
        };

        var px = new AnimationCurve();
        var py = new AnimationCurve();
        var pz = new AnimationCurve();

        var qx = new AnimationCurve();
        var qy = new AnimationCurve();
        var qz = new AnimationCurve();
        var qw = new AnimationCurve();

        float t0 = timesList[0];
        for (int i = 0; i < posList.Count; i++)
        {
            float t = timesList[i] - t0;

            var p = posList[i];
            px.AddKey(t, p.x);
            py.AddKey(t, p.y);
            pz.AddKey(t, p.z);

            var q = rotList[i].normalized;
            qx.AddKey(t, q.x);
            qy.AddKey(t, q.y);
            qz.AddKey(t, q.z);
            qw.AddKey(t, q.w);
        }

        MakeLinear(px); MakeLinear(py); MakeLinear(pz);
        MakeLinear(qx); MakeLinear(qy); MakeLinear(qz); MakeLinear(qw);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), px);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), py);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), pz);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalRotation.x"), qx);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalRotation.y"), qy);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalRotation.z"), qz);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalRotation.w"), qw);

        clip.EnsureQuaternionContinuity();

        var so = AnimationUtility.GetAnimationClipSettings(clip);
        so.loopTime = false;
        AnimationUtility.SetAnimationClipSettings(clip, so);

        return clip;
    }

    private static void MakeLinear(AnimationCurve curve)
    {
        for (int i = 0; i < curve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
    }
}
