using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class HandVisualizer : MonoBehaviour
{
    UdpClient client;
    Thread receiveThread;

    public GameObject pointPrefab;

    public float offsetY;

    public float scalably;

    List<List<Vector3>> allHandPoints = new List<List<Vector3>>();
    List<List<GameObject>> allPointObjects = new List<List<GameObject>>();

    void Start()
    {
        client = new UdpClient(5053);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            try
            {
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                HandGroup handGroup = JsonUtility.FromJson<HandGroup>(text);

                lock (allHandPoints)
                {
                    allHandPoints.Clear();
                    foreach (var hand in handGroup.hands)
                    {
                        List<Vector3> points = new List<Vector3>();
                        foreach (var lm in hand.landmarks)
                        {
                            points.Add(new Vector3(
                                lm.x * scalably,
                                -lm.y * scalably + offsetY,
                                lm.z * scalably
                            ));
                        }
                        allHandPoints.Add(points);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Error receiving data: " + e.Message);
            }
        }
    }

    void Update()
    {
        lock (allHandPoints)
        {
            // hand for 21 point
            while (allPointObjects.Count < allHandPoints.Count)
            {
                for (int h = 0; h < 2; h++) // max 2
                {
                    List<GameObject> objList = new List<GameObject>();
                    for (int i = 0; i < 21; i++)
                    {
                        for (int j = 0; j < 7; j++) // center + 6 offset
                        {
                            GameObject obj = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);
                            objList.Add(obj);
                        }
                    }
                    allPointObjects.Add(objList);
                }

            }

            Vector3[] offsets = new Vector3[]
            {
                Vector3.zero,
                new Vector3(0.02f, 0, 0),   // right
                new Vector3(-0.02f, 0, 0),  // left
                new Vector3(0, 0.02f, 0),   // up
                new Vector3(0, -0.02f, 0),  // down
                new Vector3(0, 0, 0.02f),   // forward
                new Vector3(0, 0, -0.02f)   // back
            };

            for (int h = 0; h < allHandPoints.Count; h++)
            {
                var points = allHandPoints[h];
                var objs = allPointObjects[h];
                for (int i = 0; i < points.Count; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        int objIndex = i * 7 + j;
                        Vector3 target = points[i] + offsets[j];
                        Vector3 current = objs[objIndex].transform.position;
                        objs[objIndex].transform.position = Vector3.Lerp(current, target, Time.deltaTime * 10f);
                    }
                }
            }

        }
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        client.Close();
    }

    [System.Serializable]
    public class HandGroup
    {
        public HandDataList[] hands;
    }

    [System.Serializable]
    public class HandDataList
    {
        public HandData[] landmarks;
    }

    [System.Serializable]
    public class HandData
    {
        public float x;
        public float y;
        public float z;
    }
}
