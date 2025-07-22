#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DrawAnimationPath))]
public class DrawAnimationPathEditor : Editor
{
    private int selectedIndex = -1;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DrawAnimationPath script = (DrawAnimationPath)target;

        GUILayout.Space(10);
        if (GUILayout.Button(script.showLabels ? "Close Labels" : "Show Labels"))
        {
            script.showLabels = !script.showLabels;
            EditorUtility.SetDirty(script); // değişikliği kaydet
        }
    }

    void OnSceneGUI()
    {
        DrawAnimationPath pathDrawer = (DrawAnimationPath)target;

        if (!pathDrawer.enabled) return;

        AnimationClip clip = AnimationWindowUtil.GetActiveClip();
        if (clip == null) return;

        int steps = pathDrawer.steps;
        float duration = clip.length;
        float stepTime = duration / steps;
        Vector3 originalPos = pathDrawer.transform.position;
        Vector3[] points = new Vector3[steps + 1];

        for (int i = 0; i <= steps; i++)
        {
            float t = stepTime * i;
            clip.SampleAnimation(pathDrawer.gameObject, t);
            points[i] = pathDrawer.transform.position;
        }

        pathDrawer.transform.position = originalPos;

        Handles.color = Color.white;

        for (int i = 0; i < points.Length; i++)
        {
            float tRatio = (float)i / steps;
            Handles.color = Color.Lerp(pathDrawer.startColor, pathDrawer.endColor, tRatio);

            if (Handles.Button(points[i], Quaternion.identity, 0.05f, 0.05f, Handles.SphereHandleCap))
            {
                selectedIndex = i;
                GUI.changed = true;
            }

            if (i < points.Length - 1)
                Handles.DrawLine(points[i], points[i + 1]);
        }

        if (selectedIndex >= 0 && selectedIndex < points.Length)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(points[selectedIndex], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                AnimationWindowUtil.SetPositionKey(clip, pathDrawer.transform, selectedIndex, steps, newPos);
            }
        }
    }
}
#endif