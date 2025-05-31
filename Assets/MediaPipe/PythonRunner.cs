using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonRunner : MonoBehaviour
{
    public string pythonFileName = "app.py"; // script file name
    public string pythonInterpreterPath = "D:/UnityProjects/Unity-Optimization/Assets/MediaPipe/.venv/Scripts/python.exe"; // example: "python" or full path:

    Process pythonProcess;

    void Start()
    {
        string fullPath = Path.Combine(Application.dataPath, "MediaPipe", pythonFileName);
        if (File.Exists(fullPath))
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonInterpreterPath;
            start.Arguments = $"\"{fullPath}\"";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;

            pythonProcess = new Process();
            pythonProcess.StartInfo = start;
            pythonProcess.OutputDataReceived += (s, e) => { if (e.Data != null) UnityEngine.Debug.Log("[PY] " + e.Data); };
            pythonProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) UnityEngine.Debug.LogError("[PY-ERR] " + e.Data); };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();
        }
        else
        {
            UnityEngine.Debug.LogError("Python script not found at: " + fullPath);
        }
    }

    void OnApplicationQuit()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
        }
    }
}