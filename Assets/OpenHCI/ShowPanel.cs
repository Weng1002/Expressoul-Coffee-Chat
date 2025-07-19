using UnityEngine;

public class PageManager : MonoBehaviour
{
    public GameObject[] panels;  // 所有頁面的引用

    public void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);  // 只顯示目標頁，其餘關掉
        }
    }
}
