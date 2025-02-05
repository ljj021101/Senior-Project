using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // ������� Pause Menu UI ����
    private bool isPaused = false; // ��¼��ǰ�Ƿ�����ͣ״̬

    void Start()
    {
        pauseMenuUI.SetActive(false); // ��Ϸ��ʼʱ���� Pause Menu
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // ���� ESC ��
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // ��ʾ�˵�
        Time.timeScale = 0f; // ��ͣ��Ϸ
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // ���ز˵�
        Time.timeScale = 1f; // �ָ���Ϸ
        isPaused = false;
    }
}
