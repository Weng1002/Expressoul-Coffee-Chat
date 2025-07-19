using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json; // 建議用 Newtonsoft.Json

public class TopicUIManager : MonoBehaviour
{
    [System.Serializable]
    public class Topic
    {
        public string keyword;
        public List<string> subtopics;
    }

    [System.Serializable]
    public class TopicData
    {
        public List<Topic> topics;
    }

    public GameObject keywordItemPrefab;
    public GameObject subtopicItemPrefab;

    public Transform keywordMenu;
    public Transform subtopicMenu;

    private Dictionary<string, List<GameObject>> subtopicObjectsMap = new Dictionary<string, List<GameObject>>();

    void Start()
    {
        // 假設你從 LLM 或檔案取得以下 JSON 字串
        string json = @"
        {
            ""topics"": [
                {
                    ""keyword"": ""人工智慧"",
                    ""subtopics"": [""你會建議我看機器學習的哪些課程?""]
                },
                {
                    ""keyword"": ""虛擬實境"",
                    ""subtopics"": [""推薦使用哪些頭戴裝置?""]
                }
            ]
        }";

        var data = JsonConvert.DeserializeObject<TopicData>(json);
        GenerateUI(data);
    }

    void GenerateUI(TopicData data)
    {
        foreach (var topic in data.topics)
        {
            // 建立關鍵字
            GameObject keywordItem = Instantiate(keywordItemPrefab, keywordMenu);
            keywordItem.GetComponentInChildren<TMP_Text>().text = topic.keyword;

            // 建立延伸話題並隱藏
            List<GameObject> subtopicItems = new List<GameObject>();
            foreach (string sub in topic.subtopics)
            {
                GameObject subItem = Instantiate(subtopicItemPrefab, subtopicMenu);
                subItem.GetComponentInChildren<TMP_Text>().text = sub;
                subItem.SetActive(false);
                subtopicItems.Add(subItem);
            }

            subtopicObjectsMap[topic.keyword] = subtopicItems;

            // 設定按鈕點擊事件
            Button btn = keywordItem.GetComponentInChildren<Button>();
            string keywordCopy = topic.keyword; // 避免閉包問題
            btn.onClick.AddListener(() => ToggleSubtopics(keywordCopy));
        }
    }

    void ToggleSubtopics(string keyword)
    {
        if (!subtopicObjectsMap.ContainsKey(keyword)) return;

        foreach (var obj in subtopicObjectsMap[keyword])
        {
            obj.SetActive(!obj.activeSelf);
        }
    }
}
