using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

public class ErrorLogWindow : EditorWindow
{
    private Vector2 scrollPos;
    private static List<LogEntry> errorLogs = new List<LogEntry>();

    [MenuItem("Tools/Error Log Window")]
    public static void ShowWindow()
    {
        GetWindow<ErrorLogWindow>("Error Log");
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Hata Kay�tlar�:", EditorStyles.boldLabel);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var entry in errorLogs)
        {
            EditorGUILayout.BeginVertical("box");

            if (GUILayout.Button(entry.message, EditorStyles.label))
            {
                if (!string.IsNullOrEmpty(entry.filePath))
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(entry.filePath);
                    if (obj != null)
                    {
                        AssetDatabase.OpenAsset(obj, entry.lineNumber);
                    }
                }
            }

            EditorGUILayout.LabelField(entry.stackTrace, EditorStyles.miniLabel);

            if (entry.contextLines != null && entry.contextLines.Length > 0)
            {
                GUIStyle codeStyle = new GUIStyle(EditorStyles.textField);
                codeStyle.richText = true;
                codeStyle.normal.textColor = Color.cyan;
                codeStyle.font = EditorStyles.textField.font;
                codeStyle.fontSize = 11;

                for (int i = 0; i < entry.contextLines.Length; i++)
                {
                    int currentLine = entry.lineNumber - 2 + i;

                    if (currentLine == entry.lineNumber)
                        EditorGUILayout.TextField($"-> Sat�r {currentLine}: {entry.contextLines[i]}", codeStyle);
                    else
                        EditorGUILayout.TextField($"   Sat�r {currentLine}: {entry.contextLines[i]}", codeStyle);
                }
            }

            // A��klay�c� ipucu
            if (!string.IsNullOrEmpty(entry.tip))
            {
                EditorGUILayout.HelpBox(entry.tip, MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Clear", GUILayout.Height(25)))
        {
            errorLogs.Clear();
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            string filePath = null;
            int lineNumber = 0;
            string[] contextLines = new string[0];
            string tip = "";

            var match = Regex.Match(stackTrace, @"\(at (.+\.cs):(\d+)\)");
            if (match.Success)
            {
                filePath = match.Groups[1].Value;
                lineNumber = int.Parse(match.Groups[2].Value);

                string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), filePath);
                if (File.Exists(fullPath))
                {
                    var allLines = File.ReadAllLines(fullPath);
                    int startLine = Mathf.Max(0, lineNumber - 3); // 2 �st�
                    int endLine = Mathf.Min(allLines.Length, lineNumber + 2); // 2 alt�

                    List<string> lines = new List<string>();
                    for (int i = startLine; i < endLine; i++)
                    {
                        lines.Add(allLines[i]);
                    }

                    contextLines = lines.ToArray();
                }
            }

            // Hata t�r�ne g�re a��klama yaz
            tip = GetHelpfulTip(logString);

            errorLogs.Add(new LogEntry
            {
                message = logString,
                stackTrace = stackTrace,
                filePath = filePath,
                lineNumber = lineNumber,
                contextLines = contextLines,
                tip = tip
            });

            Repaint();
        }
    }

    private string GetHelpfulTip(string logMessage)
    {
        if (logMessage.Contains("NullReferenceException"))
            return "NullReferenceException: Muhtemelen bir nesneye de�er atanmadan eri�ilmeye �al���l�yor. Atama yapmay� kontrol et.";
        if (logMessage.Contains("IndexOutOfRangeException"))
            return "IndexOutOfRangeException: Bir dizi veya liste s�n�rlar�n�n d���na eri�ilmeye �al���lm��.";
        if (logMessage.Contains("MissingReferenceException"))
            return "MissingReferenceException: Muhtemelen sahneden silinmi� bir objeye referans kalm��.";
        if (logMessage.Contains("ArgumentException"))
            return "ArgumentException: Ge�ersiz bir parametre g�nderilmi� olabilir.";

        return "";
    }

    private class LogEntry
    {
        public string message;
        public string stackTrace;
        public string filePath;
        public int lineNumber;
        public string[] contextLines;
        public string tip;
    }
}
