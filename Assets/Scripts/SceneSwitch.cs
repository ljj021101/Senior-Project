using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    public string sceneName = "HomePage"; // 这里填入你要跳转的场景名称

    void Start()
    {
        // 获取按钮组件，并添加监听器
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(LoadGameScene);
        }
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
