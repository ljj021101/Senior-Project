using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // 拖入你的 Pause Menu UI 对象
    private bool isPaused = false; // 记录当前是否处于暂停状态

    void Start()
    {
        pauseMenuUI.SetActive(false); // 游戏开始时隐藏 Pause Menu
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // 按下 ESC 键
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
        pauseMenuUI.SetActive(true); // 显示菜单
        Time.timeScale = 0f; // 暂停游戏
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // 隐藏菜单
        Time.timeScale = 1f; // 恢复游戏
        isPaused = false;
    }
}
