using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup fadePanel;
    public GameObject battleUI;
    public Slider playerHPBar;
    public Slider enemyHPBar;
    public TMP_Text enemyNameText;

    public float slideSpeed = 50f;

    private GameObject playerModelInstance;
    private GameObject enemyModelInstance;

    private Vector3 playerInitialPos;
    private Vector3 enemyInitialPos;

    private EnemyStats currentEnemy;
    private PlayerStats playerStats;

    private float playerTimer = 0f;
    private float enemyTimer = 0f;

    private bool battleInProgress = false;

    public void StartBattle(EnemyStats enemy)
    {
        currentEnemy = enemy;
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found!");
            return;
        }

        // 禁止移动与背包操作
        CaveGenerator cave = FindObjectOfType<CaveGenerator>();
        if (cave != null)
        {
            cave.canMove = false;
            cave.canOpenInventory = false;
        }

        battleUI.SetActive(true);
        StartCoroutine(BattleSequence());
    }

    IEnumerator BattleSequence()
    {
        battleInProgress = true;
        // 初始化血条
        playerHPBar.maxValue = playerStats.finalHP;
        playerHPBar.value = playerStats.currentHP;
        enemyHPBar.maxValue = currentEnemy.maxHP;
        enemyHPBar.value = currentEnemy.maxHP;
        enemyNameText.text = currentEnemy.type.ToString();

        yield return StartCoroutine(FadeToBlack());
        yield return StartCoroutine(ShowCharacters());

        float enemyHP = currentEnemy.maxHP;

        while (playerStats.currentHP > 0 && enemyHP > 0)
        {
            playerTimer += Time.deltaTime;
            enemyTimer += Time.deltaTime;

            if (playerTimer >= playerStats.finalAttackInterval)
            {
                playerTimer = 0f;
                enemyHP -= playerStats.finalAttack;
                enemyHPBar.value = enemyHP;
            }

            if (enemyTimer >= currentEnemy.attackInterval)
            {
                enemyTimer = 0f;
                playerStats.currentHP -= currentEnemy.attack;
                playerHPBar.value = playerStats.currentHP;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (playerStats.currentHP <= 0)
            HandleLose();
        else
            HandleWin();

        battleInProgress = false;
    }

    IEnumerator FadeToBlack()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 0;
        while (fadePanel.alpha < 0.8f)
        {
            fadePanel.alpha += Time.deltaTime * 1.5f;
            yield return null;
        }
    }

    IEnumerator ShowCharacters()
    {
        Vector3 camCenter = Camera.main.transform.position;
        camCenter.z = 0f; // 保证在 2D 平面

        // 设置初始位置（远离战斗区域）
        playerInitialPos = camCenter + new Vector3(-6f, 0f, 0f);
        enemyInitialPos = camCenter + new Vector3(6f, 0f, 0f);

        // 设置目标位置（战斗区偏中心）
        Vector3 playerTarget = camCenter + new Vector3(-1.5f, 0f, 0f);
        Vector3 enemyTarget = camCenter + new Vector3(1.5f, 0f, 0f);

        // 找到已放置在场景中的角色对象
        playerModelInstance = GameObject.FindWithTag("PlayerBattle");
        enemyModelInstance = GameObject.FindWithTag("EnemyBattle");

        if (playerModelInstance == null || enemyModelInstance == null)
        {
            Debug.LogError("角色实例未找到，请确认 Tag 设置正确！");
            yield break;
        }

        // 将角色移动到初始位置
        playerModelInstance.transform.position = playerInitialPos;
        enemyModelInstance.transform.position = enemyInitialPos;

        // 角色朝目标位置滑动
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            playerModelInstance.transform.position = Vector3.Lerp(playerInitialPos, playerTarget, t);
            enemyModelInstance.transform.position = Vector3.Lerp(enemyInitialPos, enemyTarget, t);

            yield return null;
        }
        // 最终对齐
        playerModelInstance.transform.position = playerTarget;
        enemyModelInstance.transform.position = enemyTarget;

        yield return new WaitForSeconds(0.2f); // 等一下再进入战斗
    }

    void HandleWin()
    {
        Debug.Log("🎉 玩家胜利！");
        EndBattle();
    }

    void HandleLose()
    {
        Debug.Log("💀 玩家失败！");
        EndBattle();
    }

    void EndBattle()
    {
        // 回到原位
        if (playerModelInstance != null)
            playerModelInstance.transform.position = playerInitialPos;

        if (enemyModelInstance != null)
            enemyModelInstance.transform.position = enemyInitialPos;

        fadePanel.alpha = 0;
        fadePanel.gameObject.SetActive(false);
        battleUI.SetActive(false);

        // 重新允许操作
        CaveGenerator cave = FindObjectOfType<CaveGenerator>();
        if (cave != null)
        {
            cave.canMove = true;
            cave.canOpenInventory = true;
        }
    }

    public bool IsBattleInProgress()
    {
        return battleInProgress;
    }
}
