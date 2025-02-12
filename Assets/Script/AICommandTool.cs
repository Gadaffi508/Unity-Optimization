using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using Unity.EditorCoroutines.Editor;

public class AICommandTool : EditorWindow
{
    private string commandText = "";
    private string apiKey = "Api Key";

    [MenuItem("Tools/AI Command Tool")]
    public static void ShowWindow()
    {
        GetWindow<AICommandTool>("AI Command Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Yapay Zeka Komut Aracı", EditorStyles.boldLabel);
        GUILayout.Label("Komut Girin:", EditorStyles.label);

        // Kullanıcının metin gireceği yer
        commandText = EditorGUILayout.TextField(commandText);

        // Generate Butonu
        if (GUILayout.Button("Generate"))
        {
            // OpenAI API'ye bağlan ve komutu işle
            SendCommandToAI(commandText);
        }

        // Kullanım Dökümanı
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Komut örnekleri:\n" +
            "- '10 tane küp oluştur ve rastgele yerleştir.'\n" +
            "- 'Bir AIBehavior script oluştur.'\n" +
            "Yapay zeka bu komutları yorumlayıp işleyecektir.",
            MessageType.Info
        );
    }

    private void SendCommandToAI(string userInput)
    {
        if (string.IsNullOrEmpty(userInput))
        {
            Debug.LogError("Lütfen bir komut girin.");
            return;
        }

        Debug.Log($"OpenAI'ye gönderiliyor: {userInput}");
        string apiUrl = "https://api.openai.com/v1/completions";

        // OpenAI API İsteği için Coroutine çağır
        EditorCoroutineUtility.StartCoroutine(SendRequest(apiUrl, userInput), this);
    }

    private IEnumerator SendRequest(string apiUrl, string prompt)
    {
        // OpenAI API'ye JSON isteği gönder
        var requestBody = new
        {
            model = "text-davinci-003", // Model ismi
            prompt = prompt,
            max_tokens = 100,
            temperature = 0.7
        };

        string json = "";//JsonConvert.SerializeObject(requestBody); // JSON'u serialize et

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // OpenAI API'den gelen cevabı işle
            string result = request.downloadHandler.text;
            Debug.Log($"AI Cevabı: {result}");
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"OpenAI API Hatası: {request.error}");
            Debug.LogError($"Detaylı Yanıt: {request.downloadHandler.text}");
        }

    }
}
