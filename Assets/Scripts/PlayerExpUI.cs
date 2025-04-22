using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerExpUI : MonoBehaviour
{
    public PlayerStats playerStats;

    [Header("UI组件")]
    public Slider expSlider;               // 用于显示经验条进度（如果用Slider）
    public Image expFillImage;             // 如果用Image的 Filled 模式
    public TMP_Text levelText;             // 显示“Lv. x”
    public Slider HPSlider;               // 用于显示经验条进度（如果用Slider）
    public Image HPFillImage;             // 如果用Image的 Filled 模式

    void Update()
    {
        if (playerStats == null) return;

        // 设置等级文本
        levelText.text = $"Lv. {playerStats.playerLevel}";

        // 显示当前经验比例
        float ratio = (float)playerStats.currentExp / playerStats.expToNextLevel;

        if (expSlider != null)
        {
            expSlider.value = ratio;
        }

        if (expFillImage != null)
        {
            expFillImage.fillAmount = ratio;
        }

        float HPratio = (float)playerStats.currentHP / playerStats.finalHP;

        if (expSlider != null)
        {
            HPSlider.value = HPratio;
        }

        if (expFillImage != null)
        {
            HPFillImage.fillAmount = HPratio;
        }
    }
}