using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioSource bgmAudio;    // BGM 音源
    public Slider volumeSlider;     // 音量控制滑动条

    private const string VolumeKey = "BGMVolume"; // 存储音量的 PlayerPrefs key

    void Start()
    {
        // 读取并设置初始音量
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1.0f);
        bgmAudio.volume = savedVolume;
        volumeSlider.value = savedVolume;

        // 监听 Slider 变化，实时调整音量
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float volume)
    {
        bgmAudio.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume); // 保存音量
        PlayerPrefs.Save();
    }
}
