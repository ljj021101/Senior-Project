using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    public string sceneName = "HomePage"; // 要跳转的场景名称
    public AudioClip clickSound; // 只需要点击音效

    void Start()
    {
        // 获取按钮组件，并添加监听器
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSoundAndLoadScene);
        }
    }

    void PlayClickSoundAndLoadScene()
    {
        if (clickSound != null)
        {
            // 直接在按钮位置播放音效
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
        }

        // 延迟加载场景，等待音效播放（如果音效为空，则立即跳转）
        Invoke("LoadGameScene", clickSound != null ? clickSound.length : 0);
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
