using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Editor Tool", typeof(MissionInfo))]
public class EditorToolScript : EditorTool
{
    public override void OnActivated()
    {
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Entering Platform Tool"), .1f);
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView sceneView))
            return;

        Handles.BeginGUI();

        Handles.Label(Vector3.forward,"Mission");

        Handles.EndGUI();
    }

    void OnEnable()
    {
        // Allocate unmanaged resources or perform one-time set up functions here
    }

    void OnDisable()
    {
        // Free unmanaged resources, state teardown.
    }
}