using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class UserInfoInput : MonoBehaviour
{
    [Header("UI 元件連接")]
    public InputField inputField;
    public Button submitButton;
    public Text outputText;  // optional: 顯示結果

    // 初始 JSON 結構
    [Serializable]
    public class UserJsonData
    {
        public string user_education = "台大機械學士";
        public string user_experience = "Garmin實習/Ｍake創客松得獎";
        public string user_skill = "控制系統, 3D模型製作";
        public string interviewer_name = "";
        public string interviewer_work = "Apple產品設計工程師";
        public string interviewer_relationship = "台大校友";
        public string communication_target = "科技業, Apple";        
        public string communication_goal = "我想了解他在 Apple 做什麼、當初怎麼進去的，也想問他覺得出國念書對找工作有沒有幫助，我最近正在考慮這件事 。";     
    }

    private UserJsonData jsonData;

    void Start()
    {
        // 初始化 JSON
        jsonData = new UserJsonData();

        // 設定按鈕事件
        submitButton.onClick.AddListener(UpdateJsonAndShow);
    }

    void UpdateJsonAndShow()
    {
        // 取得 InputField 輸入文字，寫入 jsonData
        jsonData.interviewer_name = inputField.text;

        // 序列化為 JSON 字串
        string finalJson = JsonUtility.ToJson(jsonData, true);

        // 顯示結果
        if (outputText != null)
            outputText.text = finalJson;

        Debug.Log("輸出 JSON：\n" + finalJson);
    }

    // 📦 可選：送出 JSON 到 Python Server 的範例
/*    IEnumerator SendJsonToPython(string jsonString)
    {
        string url = "http://localhost:5000/receive";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("成功送出 JSON：" + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("送出失敗：" + request.error);
        }
   }
*/
}
