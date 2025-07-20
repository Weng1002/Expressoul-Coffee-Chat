using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using System.IO;

public class TMPSceneChecker
{
    [MenuItem("Tools/Check TMP Fonts in All Scenes")]
    public static void CheckAllScenes()
    {
        string[] scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
        int totalMissing = 0;

        string currentScene = EditorSceneManager.GetActiveScene().path;

        foreach (string path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var tmps = GameObject.FindObjectsOfType<TextMeshProUGUI>();
            int sceneMissing = 0;

            foreach (var tmp in tmps)
            {
                if (tmp.font == null)
                {
                    Debug.LogWarning($"⚠️ [Scene: {scene.name}] TMP Missing Font: {tmp.name}", tmp.gameObject);
                    sceneMissing++;
                }
            }

            Debug.Log($"🔎 檢查場景：{scene.name}，共找到 {sceneMissing} 個缺字型 TMP 元件");
            totalMissing += sceneMissing;
        }

        // 回到原本的場景
        if (!string.IsNullOrEmpty(currentScene))
        {
            EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }

        Debug.Log($"✅ 全部場景檢查完畢，共 {scenePaths.Length} 個場景，缺字型 TMP 數量：{totalMissing}");
    }
}
