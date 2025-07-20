using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using UnityEngine.UI;
using UnityEngine.XR.Templates.MR;

public class WavSender : MonoBehaviour
{
    string wavFileName;
    public string serverUrl = "http://127.0.0.1:5000/upload_wav";
    // public TMP_Text Text_Q_1_1;
    // public TMP_Text Text_Q_1_2;
    // public TMP_Text Text_Q_2_1;
    // public TMP_Text Text_Q_2_2;
    // public TMP_Text Text_Keywords_1;
    // public TMP_Text Text_Keywords_2;
    public static string Q11;
    public static string Q12;
    public static string Q21;
    public static string Q22;
    public static string K1;
    public static string K2;
    float duration = 20f;
    float time;
    public static bool record_already;
    string filePath;

    void Start()
    {
        time = duration;
    }
    void Update()
    {
        if (time >= 0)
        {
            time -= Time.deltaTime;
        }
        else
        {
            if (record_already)
            {
                filePath = AudioRecorder.filepath;
                Debug.Log(filePath);
                StartCoroutine(UploadWavAndGetJson());
                time = duration;
                record_already = false;
            }
        }
    }

    // public void SendWavFile()
    // {
    //     StartCoroutine(UploadWavAndGetJson());
    // }

    IEnumerator UploadWavAndGetJson()
    {
        // string filePath = Path.Combine(Application.streamingAssetsPath, wavFileName);
        byte[] wavBytes = File.ReadAllBytes(filePath);

        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(wavBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "audio/wav");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Upload success: " + request.downloadHandler.text);

            string json = request.downloadHandler.text;
            ServerResponse response = JsonUtility.FromJson<ServerResponse>(json);
            Debug.Log("這是json"+json);
            Debug.Log("這是response"+response.Q_1_1);
            // Text_Q_1_1.text = response.Q_1_1;
            // Text_Q_1_2.text = response.Q_1_2;
            // Text_Q_2_1.text = response.Q_2_1;
            // Text_Q_2_2.text = response.Q_2_2;
            // Text_Keywords_1.text = response.Keyword_1;
            // Text_Keywords_2.text = response.Keyword_2;
            Q11 = response.Q_1_1;
            Q12 = response.Q_1_2;
            Q21 = response.Q_2_1;
            Q22 = response.Q_2_2;
            K1 = response.Keyword_1;
            K2 = response.Keyword_2;
            Debug.Log("Get Text");
            GoalManager.text_already = true;
        }
        else
        {
            Debug.LogError("Upload failed: " + request.error);

            Q11 = "❌ 上傳失敗";
            Q12 = "";
            Q21 = "";
            Q22 = "";
            K1 = "";
            K2 = "";
        }

    }

    [System.Serializable]
    public class ServerResponse
    {
        public string Q_1_1;
        public string Q_1_2;
        public string Q_2_1;
        public string Q_2_2;
        public string Keyword_1;
        public string Keyword_2;
    }
}
