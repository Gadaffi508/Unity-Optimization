using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class WebcamReceiver : MonoBehaviour
{
    UdpClient videoClient;
    Thread videoThread;
    Texture2D webcamTexture;
    public UnityEngine.UI.RawImage targetImage;

    void Start()
    {
        webcamTexture = new Texture2D(2, 2);
        UnityMainThreadDispatcher.Instance(); 
        videoClient = new UdpClient(5054);
        videoThread = new Thread(VideoReceiveLoop);
        videoThread.IsBackground = true;
        videoThread.Start();
    }

    void VideoReceiveLoop()
    {
        IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            try
            {
                byte[] data = videoClient.Receive(ref ip);
                if (data != null && data.Length > 0)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        webcamTexture.LoadImage(data);
                        targetImage.texture = webcamTexture;
                    });
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Video receive error: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        videoThread?.Abort();
        videoClient?.Close();
    }
}