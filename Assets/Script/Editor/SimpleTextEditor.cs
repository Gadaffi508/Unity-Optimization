using UnityEditor;
using UnityEngine;
using System.IO;

public class SimpleTextEditor : EditorWindow
{
    string filePath = "";
    string fileContent = "";
    Vector2 scrollPos;

    [MenuItem("Tools/Simple Text Editor")]
    public static void ShowWindow()
    {
        GetWindow<SimpleTextEditor>("Text Editor");
    }

    void OnGUI()
    {
        GUILayout.Label("Text File Editor", EditorStyles.boldLabel);

        if (GUILayout.Button("Open .txt File"))
        {
            string path = EditorUtility.OpenFilePanel("Open Text File", "", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;
                fileContent = File.ReadAllText(path);
            }
        }

        if (!string.IsNullOrEmpty(filePath))
        {
            GUILayout.Label("Editing: " + Path.GetFileName(filePath), EditorStyles.label);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            fileContent = EditorGUILayout.TextArea(fileContent, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Save"))
            {
                File.WriteAllText(filePath, fileContent);
                Debug.Log("File saved: " + filePath);
            }
        }
    }
}