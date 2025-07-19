using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private static PersistentObject instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 跨場景保留
        }
        else
        {
            Destroy(gameObject); // 避免場景重複創建A
        }
    }
}
