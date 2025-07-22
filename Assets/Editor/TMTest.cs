using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class FixAllTMPFonts
{
    [MenuItem("Tools/Fix TMP Fonts In All Scenes & Prefabs")]
    public static void FixFontsEverywhere()
    {
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont == null)
        {
            Debug.LogError("❌ 找不到 LiberationSans SDF，請先從 Window > TextMeshPro > Import TMP Essentials");
            return;
        }

        int fixedSceneCount = 0;
        int fixedPrefabCount = 0;
        string currentScene = SceneManager.GetActiveScene().path;

        // 修復所有場景
        string[] scenePaths = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
        foreach (string path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var tmps = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);
            bool dirty = false;

            foreach (var tmp in tmps)
            {
                if (tmp.font == null)
                {
                    Undo.RecordObject(tmp, "Fix TMP Font in Scene");
                    tmp.font = defaultFont;
                    EditorUtility.SetDirty(tmp);
                    Debug.Log($"✅ 修復場景字型: {scene.name} → {tmp.name}", tmp.gameObject);
                    fixedSceneCount++;
                    dirty = true;
                }
            }

            if (dirty)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        // 回復原本的場景
        if (!string.IsNullOrEmpty(currentScene))
        {
            EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }

        // 修復所有 prefab
        string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var tmps = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            bool changed = false;

            foreach (var tmp in tmps)
            {
                if (tmp != null && tmp.font == null)
                {
                    tmp.font = defaultFont;
                    EditorUtility.SetDirty(tmp);
                    Debug.Log($"✅ 修復 prefab 字型: {prefab.name} → {tmp.name}", tmp.gameObject);
                    changed = true;
                    fixedPrefabCount++;
                }
            }

            if (changed)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"🎉 全部修復完成：共修場景 TMP {fixedSceneCount} 個，prefab TMP {fixedPrefabCount} 個。");
    }
}
