using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    public string sceneName = "HomePage"; // ����������Ҫ��ת�ĳ�������

    void Start()
    {
        // ��ȡ��ť���������Ӽ�����
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
