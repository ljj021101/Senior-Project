using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }

    [Header("音效剪辑")]
    public AudioClip chestOpenSound;  // 箱子开启音效
    public AudioClip pickupSound;     // 装备拾取音效
    public AudioClip dropSound;       // 装备放下音效

    private AudioSource audioSource;

    void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 获取 AudioSource 组件，如没有则添加一个
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 播放箱子开启音效
    /// </summary>
    public void PlayChestOpenSound()
    {
        if (chestOpenSound != null)
        {
            audioSource.PlayOneShot(chestOpenSound);
        }
    }

    /// <summary>
    /// 播放装备拾取音效
    /// </summary>
    public void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    /// <summary>
    /// 播放装备放下音效
    /// </summary>
    public void PlayDropSound()
    {
        if (dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
    }
}
