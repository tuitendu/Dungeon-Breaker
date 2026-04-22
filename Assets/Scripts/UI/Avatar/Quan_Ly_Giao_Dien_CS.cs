using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject characterPanel;

    // Hàm MỞ bảng
    public void OpenCharacterPanel()
    {
        characterPanel.SetActive(true);
        Time.timeScale = 0f; //  Đóng băng thời gian
    }

    // Hàm ĐÓNG bảng
    public void CloseCharacterPanel()
    {
        characterPanel.SetActive(false);
        Time.timeScale = 1f; //  Cho game chạy lại bình thường
    }

    public void TogglePanel()
    {
        bool isActive = characterPanel.activeSelf;
        if (isActive)
        {
            CloseCharacterPanel();
        }
        else
        {
            OpenCharacterPanel();
        }
    }
}