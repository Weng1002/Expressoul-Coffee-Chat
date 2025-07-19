using UnityEngine;
using UnityEngine.SceneManagement;

public class SwithScene : MonoBehaviour
{
    public string sceneName;
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
    
}
