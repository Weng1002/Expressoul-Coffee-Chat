using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // 輸入框用TextMeshPro
using UnityEngine.UI;

public class XRDebugMover : MonoBehaviour
{
    [Header("移動速度")]
    public float moveSpeed = 1.0f;

    [Header("文字輸入框（TMP_InputField）")]
    public TMP_InputField inputField;

    [Header("滑鼠點擊模擬目標UI物件")]
    public GameObject targetUIElement;

    void Update()
    {
        // 1. 鍵盤WASDQE移動
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += transform.forward;
        if (Input.GetKey(KeyCode.S)) move -= transform.forward;
        if (Input.GetKey(KeyCode.A)) move -= transform.right;
        if (Input.GetKey(KeyCode.D)) move += transform.right;
        if (Input.GetKey(KeyCode.Q)) move -= transform.up;
        if (Input.GetKey(KeyCode.E)) move += transform.up;

        transform.position += move * moveSpeed * Time.deltaTime;

        // 2. 按Tab聚焦輸入框，啟用鍵盤輸入
        if (inputField != null && Input.GetKeyDown(KeyCode.Tab))
        {
            inputField.ActivateInputField();
        }

        // 3. 按Esc取消輸入框焦點
        if (inputField != null && Input.GetKeyDown(KeyCode.Escape))
        {
            EventSystem.current.SetSelectedGameObject(null);
            inputField.DeactivateInputField();
        }

        // 4. 滑鼠左鍵模擬點擊UI目標
        if (Input.GetMouseButtonDown(0) && targetUIElement != null)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            ExecuteEvents.Execute(targetUIElement, pointer, ExecuteEvents.pointerClickHandler);
        }
    }
}
