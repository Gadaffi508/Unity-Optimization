#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

public static class AnimationWindowUtil
{
    public static AnimationClip GetActiveClip()
    {
        Type animWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
        var windows = Resources.FindObjectsOfTypeAll(animWindowType);
        if (windows.Length == 0) return null;

        var animWindow = windows[0];
        var animEditorField = animWindowType.GetField("m_AnimEditor", BindingFlags.NonPublic | BindingFlags.Instance);
        var animEditor = animEditorField.GetValue(animWindow);
        var stateProp = animEditor.GetType().GetProperty("state", BindingFlags.Public | BindingFlags.Instance);
        var state = stateProp.GetValue(animEditor);
        var clipProp = state.GetType().GetProperty("activeAnimationClip", BindingFlags.Public | BindingFlags.Instance);
        return clipProp.GetValue(state) as AnimationClip;
    }

    public static void SetPositionKey(AnimationClip clip, Transform target, int index, int totalSteps, Vector3 worldPos)
    {
        float time = (clip.length / totalSteps) * index;
        Vector3 localPos = target.parent ? target.parent.InverseTransformPoint(worldPos) : worldPos;

        AnimationCurve curveX = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x")) ?? new AnimationCurve();
        AnimationCurve curveY = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y")) ?? new AnimationCurve();
        AnimationCurve curveZ = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z")) ?? new AnimationCurve();

        AddOrUpdateKey(curveX, time, localPos.x);
        AddOrUpdateKey(curveY, time, localPos.y);
        AddOrUpdateKey(curveZ, time, localPos.z);

        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), curveX);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), curveY);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), curveZ);
    }

    static void AddOrUpdateKey(AnimationCurve curve, float time, float value)
    {
        int index = Array.FindIndex(curve.keys, k => Mathf.Approximately(k.time, time));
        if (index >= 0)
            curve.MoveKey(index, new Keyframe(time, value));
        else
            curve.AddKey(new Keyframe(time, value));
    }
}
#endif
