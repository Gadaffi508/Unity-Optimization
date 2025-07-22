using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
#endif

[ExecuteAlways]
public class DrawAnimationPath : MonoBehaviour
{
    public Color startColor = Color.red;
    public Color endColor = Color.green;
    public int steps = 14;
    public float pointSize = 0.08f;
    public float arrowLength = 0.1f;
    
    public bool showLabels { get;set; }

#if UNITY_EDITOR
    private int selectedPointIndex = -1; // Seçili keyframe index'i
#endif

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        AnimationClip clip = GetCurrentAnimationWindowClip();
        if (clip == null) return;

        float clipLength = clip.length;
        Vector3 originalPosition = transform.position;
        Vector3[] positions = new Vector3[steps + 1];
        float[] times = new float[steps + 1];

        for (int i = 0; i <= steps; i++)
        {
            float t = (clipLength / steps) * i;
            clip.SampleAnimation(gameObject, t);
            positions[i] = transform.position;
            times[i] = t;
        }

        transform.position = originalPosition;

        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
        labelStyle.normal.textColor = Color.black;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 10;

        for (int i = 0; i < steps; i++)
        {
            Vector3 start = positions[i];
            Vector3 end = positions[i + 1];
            Vector3 dir = (end - start).normalized;

            float tRatio = (float)i / steps;
            Gizmos.color = Color.Lerp(startColor, endColor, tRatio);
            Handles.color = Gizmos.color;

            // Nokta
            if (Handles.Button(start, Quaternion.identity, pointSize * 2, pointSize * 2, Handles.SphereHandleCap))
            {
                selectedPointIndex = i;
            }

            // Çizgi ve ok
            Gizmos.DrawLine(start, end);
            Handles.ArrowHandleCap(0, start, Quaternion.LookRotation(dir), arrowLength, EventType.Repaint);

            if (showLabels)
            {
                float time = times[i];
                string arrow = GetAsciiArrow(dir);
                string label = $"{arrow}  t={time:F2}s";
                Handles.Label(start + Vector3.up * 0.1f, label, labelStyle);
            }
        }

        // Seçili noktayı hareket ettirme
        if (selectedPointIndex >= 0 && selectedPointIndex <= steps)
        {
            Vector3 handlePos = positions[selectedPointIndex];
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(handlePos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateClipPositionKey(clip, times[selectedPointIndex], newPos);
            }
        }

        Gizmos.color = endColor;
        Gizmos.DrawSphere(positions[steps], pointSize * 1.5f);
#endif
    }

#if UNITY_EDITOR
    void UpdateClipPositionKey(AnimationClip clip, float time, Vector3 newWorldPos)
    {
        Vector3 localPos = transform.parent ? transform.parent.InverseTransformPoint(newWorldPos) : newWorldPos;

        AnimationCurve curveX = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x")) ?? new AnimationCurve();
        AnimationCurve curveY = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y")) ?? new AnimationCurve();
        AnimationCurve curveZ = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z")) ?? new AnimationCurve();

        AddOrUpdateKey(curveX, time, localPos.x);
        AddOrUpdateKey(curveY, time, localPos.y);
        AddOrUpdateKey(curveZ, time, localPos.z);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), curveX);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), curveY);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), curveZ);

        Debug.Log($"Keyframe güncellendi → time: {time:F2}  pos: {localPos}");
    }

    void AddOrUpdateKey(AnimationCurve curve, float time, float value)
    {
        int index = curve.keys.Length > 0 ? Array.FindIndex(curve.keys, k => Mathf.Approximately(k.time, time)) : -1;
        if (index >= 0)
            curve.MoveKey(index, new Keyframe(time, value));
        else
            curve.AddKey(new Keyframe(time, value));
    }

    AnimationClip GetCurrentAnimationWindowClip()
    {
        Type animWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
        if (animWindowType == null) return null;

        var allWindows = Resources.FindObjectsOfTypeAll(animWindowType);
        if (allWindows.Length == 0) return null;

        var animWindow = allWindows[0];
        FieldInfo animEditorField = animWindowType.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance);
        if (animEditorField == null) return null;

        object animEditor = animEditorField.GetValue(animWindow);
        if (animEditor == null) return null;

        Type animEditorType = animEditor.GetType();
        PropertyInfo stateProp = animEditorType.GetProperty("state", BindingFlags.Public | BindingFlags.Instance);
        if (stateProp == null) return null;

        object state = stateProp.GetValue(animEditor);
        if (state == null) return null;

        PropertyInfo clipProp = state.GetType().GetProperty("activeAnimationClip", BindingFlags.Public | BindingFlags.Instance);
        return clipProp?.GetValue(state) as AnimationClip;
    }

    string GetAsciiArrow(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

        if (angle >= -22.5f && angle < 22.5f) return "→";
        if (angle >= 22.5f && angle < 67.5f) return "↗";
        if (angle >= 67.5f && angle < 112.5f) return "↑";
        if (angle >= 112.5f && angle < 157.5f) return "↖";
        if (angle >= 157.5f || angle < -157.5f) return "←";
        if (angle >= -157.5f && angle < -112.5f) return "↙";
        if (angle >= -112.5f && angle < -67.5f) return "↓";
        if (angle >= -67.5f && angle < -22.5f) return "↘";
        return "•";
    }
#endif
}