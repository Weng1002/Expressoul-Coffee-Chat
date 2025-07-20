using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPageController : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene("MainScene");
    }
}
