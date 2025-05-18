using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class SimpleCSEditor : EditorWindow
{
    private Vector2 fileListScroll, codeScroll;
    private string[] csFiles;
    private string filePath = "";
    private string editableContent = "";
    private string originalContent = "";

    [MenuItem("Tools/Unity C# Editor")]
    public static void ShowWindow()
    {
        GetWindow<SimpleCSEditor>("Unity C# Editor");
    }

    void OnEnable()
    {
        RefreshFileList();
    }

    void RefreshFileList()
    {
        csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetFileName(path))
            .ToArray();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // File List
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("Unity C# File Editor", EditorStyles.boldLabel);
        fileListScroll = EditorGUILayout.BeginScrollView(fileListScroll);
        foreach (string file in csFiles)
        {
            if (GUILayout.Button(file))
            {
                string fullPath = Directory.GetFiles(Application.dataPath, file, SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrEmpty(fullPath))
                {
                    filePath = fullPath;
                    editableContent = File.ReadAllText(filePath);
                    originalContent = editableContent;
                }
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Editor area
        EditorGUILayout.BeginVertical();

        if (!string.IsNullOrEmpty(filePath))
        {
            EditorGUILayout.LabelField("Editing: " + Path.GetFileName(filePath));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                File.WriteAllText(filePath, editableContent);
                AssetDatabase.ImportAsset(GetRelativeAssetPath(filePath));
                originalContent = editableContent;
                Debug.Log("File saved: " + filePath);
            }
            if (GUILayout.Button("Revert"))
            {
                editableContent = originalContent;
            }
            EditorGUILayout.EndHorizontal();

            // Preview (read-only syntax highlight simulation)
            EditorGUILayout.LabelField("Syntax Highlight Preview (Read-only)", EditorStyles.boldLabel);
            codeScroll = EditorGUILayout.BeginScrollView(codeScroll, GUILayout.Height(150));
            DrawSyntaxHighlightedCode(originalContent);
            EditorGUILayout.EndScrollView();

            // Editable Code Area
            EditorGUILayout.LabelField("Editable Source Code", EditorStyles.boldLabel);
            codeScroll = EditorGUILayout.BeginScrollView(codeScroll);
            editableContent = EditorGUILayout.TextArea(editableContent, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            // Live sync check
            if (File.Exists(filePath))
            {
                string diskContent = File.ReadAllText(filePath);
                if (diskContent != editableContent && diskContent != originalContent)
                {
                    editableContent = diskContent;
                    originalContent = diskContent;
                }
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    void DrawSyntaxHighlightedCode(string content)
    {
        string[] lines = content.Split('\n');
        foreach (var line in lines)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);

            if (line.TrimStart().StartsWith("using"))
                style.normal.textColor = Color.cyan;
            else if (line.Contains("class"))
                style.normal.textColor = Color.green;
            else if (line.Contains("private") || line.Contains("public"))
                style.normal.textColor = Color.magenta;
            else
                style.normal.textColor = Color.white;

            EditorGUILayout.LabelField(line, style);
        }
    }

    string GetRelativeAssetPath(string absolutePath)
    {
        return "Assets" + absolutePath.Replace(Application.dataPath, "").Replace("\\", "/");
    }

}
