using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    public PlayerStats playerStats;

    public TMP_Text healthText;
    public TMP_Text attackText;
    public TMP_Text defenseText;
    public TMP_Text speedText;
    public TMP_Text critRateText;
    public TMP_Text critDamageText;
    public TMP_Text luckText;
    public TMP_Text magicResistText;
    public TMP_Text lightText;
    public TMP_Text attackSpeedText;

    private void Start()
    {
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }
    }

    public void Update()
    {
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (playerStats == null) return;

        healthText.text = $"Health: {playerStats.finalHP}";
        attackText.text = $"Attack: {playerStats.finalAttack}";
        defenseText.text = $"Defense: {playerStats.finalDefense}";
        speedText.text = $"Speed: {playerStats.finalMoveSpeed:F2}";
        critRateText.text = $"CritRate: {playerStats.finalCritRate:F2}%";
        critDamageText.text = $"CritDMG: {playerStats.finalCritDamage:F2}%";
        luckText.text = $"Luck: {playerStats.finalLuck}";
        magicResistText.text = $"MagicResist: {playerStats.finalMagicResist}";
        lightText.text = $"Light: {playerStats.finalLightRadius:F2}%";
        attackSpeedText.text = $"AttackSpeed: {playerStats.finalAttackInterval:F2}";
    }
}
