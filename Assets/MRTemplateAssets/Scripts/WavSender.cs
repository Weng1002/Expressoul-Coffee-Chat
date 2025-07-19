using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class WavSender : MonoBehaviour
{
    public string wavFileName = "test.wav";
    public string serverUrl = "http://127.0.0.1:5000/upload_wav";
    public TMP_Text Text_Q_1_1;
    public TMP_Text Text_Q_1_2;
    public TMP_Text Text_Q_2_1;
    public TMP_Text Text_Q_2_2;
    public TMP_Text Text_Keywords_1;
    public TMP_Text Text_Keywords_2;
    public static string Q11;
    public static string Q12;
    public static string Q21;
    public static string Q22;


    public void SendWavFile()
    {
        StartCoroutine(UploadWavAndGetJson());
    }

    IEnumerator UploadWavAndGetJson()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, wavFileName);
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

            Text_Q_1_1.text = response.Q_1_1;
            Text_Q_1_2.text = response.Q_1_2;
            Text_Q_2_1.text = response.Q_2_1;
            Text_Q_2_2.text = response.Q_2_2;
            Text_Keywords_1.text = response.Keyword_1;
            Text_Keywords_2.text = response.Keyword_2;
        }
        else
        {
            Debug.LogError("Upload failed: " + request.error);

            Text_Q_1_1.text = "❌ 上傳失敗";
            Text_Q_1_2.text = "";
            Text_Q_2_1.text = "";
            Text_Q_2_2.text = "";
            Text_Keywords_1.text = "";
            Text_Keywords_2.text = "";
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
