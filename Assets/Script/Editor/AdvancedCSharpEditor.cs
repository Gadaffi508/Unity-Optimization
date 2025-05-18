using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public class AdvancedCSharpEditor : EditorWindow
{
    private Vector2 fileScroll, codeScroll, errorScroll;
    private string[] csFiles;
    private string filePath = "";
    private string editableContent = "";

    [MenuItem("Tools/Advanced C# Editor")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedCSharpEditor>("Unity C# Editor");
    }

    void OnEnable()
    {
        RefreshFileList();
    }

    void RefreshFileList()
    {
        csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetFileName(path)).ToArray();
    }

    void LoadFile(string name)
    {
        string fullPath = Directory.GetFiles(Application.dataPath, name, SearchOption.AllDirectories).FirstOrDefault();
        if (!string.IsNullOrEmpty(fullPath))
        {
            filePath = fullPath;
            editableContent = File.ReadAllText(filePath);
        }
    }

    void SaveFile()
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, editableContent);
            AssetDatabase.ImportAsset(GetRelativeAssetPath(filePath));
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // File list
        EditorGUILayout.BeginVertical(GUILayout.Width(220));
        EditorGUILayout.LabelField("C# Files", EditorStyles.boldLabel);
        fileScroll = EditorGUILayout.BeginScrollView(fileScroll);
        foreach (string file in csFiles)
        {
            if (GUILayout.Button(file))
            {
                LoadFile(file);
            }
        }
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("Refresh"))
            RefreshFileList();
        EditorGUILayout.EndVertical();

        // Editor
        EditorGUILayout.BeginVertical();
        if (!string.IsNullOrEmpty(filePath))
        {
            EditorGUILayout.LabelField("Editing: " + Path.GetFileName(filePath));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save & Compile"))
            {
                SaveFile();
            }
            if (GUILayout.Button("Revert"))
            {
                editableContent = File.ReadAllText(filePath);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Editable Source Code", EditorStyles.boldLabel);
            codeScroll = EditorGUILayout.BeginScrollView(codeScroll);

            // simulate advanced editing (tab and enter handling)
            GUI.SetNextControlName("CodeEditor");
            editableContent = EditorGUILayout.TextArea(editableContent, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            // key handling
            if (Event.current.type == EventType.KeyDown && GUI.GetNameOfFocusedControl() == "CodeEditor")
            {
                if (Event.current.keyCode == KeyCode.Tab)
                {
                    editableContent = InsertAtCursor("    ");
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    editableContent = InsertAtCursor("\n" + GetCurrentIndent());
                    Event.current.Use();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Compile Errors", EditorStyles.boldLabel);
            errorScroll = EditorGUILayout.BeginScrollView(errorScroll, GUILayout.Height(150));
            
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    string GetRelativeAssetPath(string absolutePath)
    {
        return "Assets" + absolutePath.Replace(Application.dataPath, "").Replace("\\", "/");
    }

    string InsertAtCursor(string insert)
    {
        TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        int cursorIndex = te.cursorIndex;
        editableContent = editableContent.Insert(cursorIndex, insert);
        te.cursorIndex += insert.Length;
        te.selectIndex = te.cursorIndex;
        return editableContent;
    }

    string GetCurrentIndent()
    {
        TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
        int lineStart = editableContent.LastIndexOf('\n', te.cursorIndex - 1);
        if (lineStart < 0) return "";
        int indentEnd = 0;
        for (int i = lineStart + 1; i < editableContent.Length; i++)
        {
            if (editableContent[i] != ' ' && editableContent[i] != '\t')
            {
                indentEnd = i;
                break;
            }
        }
        return editableContent.Substring(lineStart + 1, indentEnd - (lineStart + 1));
    }
}
