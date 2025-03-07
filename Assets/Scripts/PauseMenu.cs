using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // 暂停菜单UI对象
    private bool isPaused = false; // 记录当前是否处于暂停状态

    void Start()
    {
        pauseMenuUI.SetActive(false); // 游戏开始时隐藏暂停菜单
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
        pauseMenuUI.SetActive(true); // 显示暂停菜单
        Time.timeScale = 0f;         // 暂停游戏
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // 隐藏暂停菜单
        Time.timeScale = 1f;          // 恢复游戏
        isPaused = false;
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏...");
        Application.Quit();
    }
}