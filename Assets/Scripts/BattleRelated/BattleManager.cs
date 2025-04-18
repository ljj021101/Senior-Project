using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using System;

public class BattleManager : MonoBehaviour
{
    [Header("对象引用")]
    public EnemyBattleController enemyBattleController;
    public GameObject playerModelInstance;
    public GameObject enemyModelInstance;
    public InventoryManager inventoryManager;
    public CaveGenerator cave;
    public CanvasGroup gameOverPanel;

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip playerAttackClip;
    public AudioClip slimeAttackClip;
    public AudioClip goblinAttackClip;
    public AudioClip batAttackClip;
    public AudioClip slimeDeathClip;
    public AudioClip goblinDeathClip;
    public AudioClip batDeathClip;
    public AudioClip playerDeathClip;

    [Header("UI Components")]
    public GameObject battleUI;
    public Slider playerHPBar;
    public Slider enemyHPBar;
    public TMP_Text enemyNameText;
    public Animator playerAnimator;
    public Animator mapPlayerAnimator;

    [Header("光照替代 Fade")]
    public Light2D playerLight;
    public float battleLightRadius = 0f;
    public float lightAdjustSpeed = 10f;

    private float normalLightRadius;

    private Vector2Int enemyTilePosition;
    private Vector3 playerInitialPos;
    private Vector3 enemyInitialPos;

    private EnemyStats currentEnemy;
    private PlayerStats playerStats;

    private float playerTimer = 0f;
    private float enemyTimer = 0f;

    private bool battleInProgress = false;
    private bool isGameOver = false;
    private bool isDeath = false;

    public void StartBattle(EnemyStats enemy, Vector2Int enemyPos)
    {
        currentEnemy = enemy;
        enemyTilePosition = enemyPos;
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found!");
            return;
        }

        // 获取当前光照半径
        normalLightRadius = playerStats.finalLightRadius / 100f;
        playerLight.pointLightOuterRadius = normalLightRadius;

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

        playerHPBar.maxValue = playerStats.finalHP;
        playerHPBar.value = playerStats.currentHP;
        enemyHPBar.maxValue = currentEnemy.maxHP;
        enemyHPBar.value = currentEnemy.maxHP;
        enemyNameText.text = currentEnemy.type.ToString();

        Vector3 camCenter = Camera.main.transform.position;
        playerInitialPos = camCenter + new Vector3(-3f, 0f, 0f);
        enemyInitialPos = camCenter + new Vector3(3f, 0f, 0f);
        playerModelInstance.transform.position = playerInitialPos;
        enemyModelInstance.transform.position = enemyInitialPos;

        yield return StartCoroutine(ShrinkLight());
        yield return StartCoroutine(ShowCharacters());

        float enemyHP = currentEnemy.maxHP;
        playerTimer = 100000f;
        enemyTimer = 0f;

        while (playerStats.currentHP > 0 && enemyHP > 0)
        {
            playerTimer += Time.deltaTime;
            enemyTimer += Time.deltaTime;

            if (playerTimer >= playerStats.finalAttackInterval)
            {
                playerTimer = 0f;

                // 判断是否暴击
                bool isCrit = UnityEngine.Random.value < (playerStats.finalCritRate / 100f);
                
                // 基础伤害
                float baseDamage = playerStats.finalAttack;

                if (isCrit)
                {
                    baseDamage *= (playerStats.finalCritDamage / 100f);
                }

                // 计算敌人最终受到的伤害（线性减伤，最少为1）
                float damageDealt = Mathf.Max(1f, baseDamage);  // 默认无防御
                if (currentEnemy.defense > 0)
                {
                    damageDealt = Mathf.Max(1f, baseDamage - currentEnemy.defense);  // 线性减伤
                }

                enemyHP -= damageDealt;
                enemyHPBar.value = enemyHP;

                // 播放玩家攻击动画与音效
                playerAnimator.SetTrigger("Slash");
                audioSource?.PlayOneShot(playerAttackClip);

                // 闪白
                var enemyHit = enemyModelInstance.GetComponentInChildren<HitEffect>();
                if (enemyHit != null) enemyHit.Flash();

                // 调试输出
                Debug.Log(isCrit
                    ? $"暴击！对敌人造成了 {damageDealt:F1} 点伤害！"
                    : $"对敌人造成了 {damageDealt:F1} 点伤害");
            }


            if (enemyTimer >= currentEnemy.attackInterval)
            {
                enemyTimer = 0f;
                playerStats.currentHP -= currentEnemy.attack;
                playerHPBar.value = playerStats.currentHP;

                enemyBattleController.animator.SetTrigger("Attack");
                switch (currentEnemy.type)
                {
                    case EnemyType.Slime: audioSource?.PlayOneShot(slimeAttackClip); break;
                    case EnemyType.Goblin: audioSource?.PlayOneShot(goblinAttackClip); break;
                    case EnemyType.Bat: audioSource?.PlayOneShot(batAttackClip); break;
                }
                playerModelInstance.GetComponentInChildren<HitEffect>()?.Flash();
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

    IEnumerator ShrinkLight()
    {
        while (playerLight.pointLightOuterRadius > 0)
        {
            playerLight.pointLightOuterRadius -= Time.deltaTime * lightAdjustSpeed;
            yield return null;
        }
        playerLight.pointLightOuterRadius = battleLightRadius;
    }

    IEnumerator RestoreLight(Action onComplete = null)
    {
        while (playerLight.pointLightOuterRadius < normalLightRadius)
        {
            playerLight.pointLightOuterRadius += Time.deltaTime * lightAdjustSpeed;
            yield return null;
        }

        playerLight.pointLightOuterRadius = normalLightRadius;
    }

    void ResetHitColor(GameObject model)
    {
        var hitEffect = model.GetComponentInChildren<HitEffect>();
        if (hitEffect != null && hitEffect.spriteRenderer != null)
        {
            hitEffect.spriteRenderer.color = hitEffect.spriteRenderer.color = Color.white;
        }
    }

    IEnumerator ShowCharacters()
    {
        Vector3 camCenter = Camera.main.transform.position;
        camCenter.z = 0f;

        enemyBattleController.Setup(currentEnemy.type);
        ResetHitColor(playerModelInstance);
        ResetHitColor(enemyModelInstance);

        playerInitialPos = camCenter + new Vector3(-3f, -0.3f, 0f);
        enemyInitialPos = camCenter + new Vector3(3f, 0f, 0f);
        Vector3 playerTarget = camCenter + new Vector3(-0.2f, -0.3f, 0f);
        Vector3 enemyTarget = camCenter + new Vector3(0.2f, 0f, 0f);

        if (playerModelInstance == null || enemyModelInstance == null)
        {
            Debug.LogError("角色实例未找到，请确认绑定！");
            yield break;
        }

        playerModelInstance.transform.position = playerInitialPos;
        enemyModelInstance.transform.position = enemyInitialPos;

        Vector3 scale = enemyModelInstance.transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        enemyModelInstance.transform.localScale = scale;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            playerModelInstance.transform.position = Vector3.Lerp(playerInitialPos, playerTarget, t);
            enemyModelInstance.transform.position = Vector3.Lerp(enemyInitialPos, enemyTarget, t);

            yield return null;
        }

        playerModelInstance.transform.position = playerTarget;
        enemyModelInstance.transform.position = enemyTarget;
    }

    void HandleWin()
    {
        Debug.Log("玩家胜利！");
        FindObjectOfType<CaveGenerator>()?.ClearEnemyTile(enemyTilePosition);

        switch (currentEnemy.type)
        {
            case EnemyType.Slime: audioSource?.PlayOneShot(slimeDeathClip); break;
            case EnemyType.Goblin: audioSource?.PlayOneShot(goblinDeathClip); break;
            case EnemyType.Bat: audioSource?.PlayOneShot(batDeathClip); break;
        }

        EndBattle();
    }

    void HandleLose()
    {
        Debug.Log("玩家失败！");
        isDeath = true;
        playerAnimator.SetTrigger("Death");
        audioSource?.PlayOneShot(playerDeathClip);
        StartCoroutine(HandleDeathThenShowGameOver());
    }

    IEnumerator HandleDeathSequence()
    {
        yield return new WaitForSeconds(2f);
        EndBattle();
    }

    IEnumerator WaitAndEnableMove(float delay = 0.4f)
    {
        yield return new WaitForSeconds(delay);
        if (cave != null && !isGameOver)
        {
            cave.canMove = true;
            cave.canOpenInventory = true;
        }
    }

    void EndBattle()
    {
        if (playerModelInstance != null)
            playerModelInstance.transform.position = playerInitialPos;
        if (enemyModelInstance != null)
            enemyModelInstance.transform.position = enemyInitialPos;

        battleUI.SetActive(false);
        
        if (!isDeath)
        {
            StartCoroutine(RestoreLight()); // 光照恢复不管它
            StartCoroutine(WaitAndEnableMove()); // 但移动延迟恢复
        }

        var cave = FindObjectOfType<CaveGenerator>();

        inventoryManager.SaveAll();
        Debug.Log("战斗结束存档");
    }

    IEnumerator HandleDeathThenShowGameOver()
    {
        yield return new WaitForSeconds(2f);
        battleUI.SetActive(false);
        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.alpha = 1;
        isGameOver = true;
    }

    void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartFromLevelOne();
        }
    }

    void RestartFromLevelOne()
    {
        isGameOver = false;
        isDeath = false;

        gameOverPanel.alpha = 0;
        gameOverPanel.gameObject.SetActive(false);

        playerStats.currentHP = playerStats.finalHP;
        playerAnimator.SetTrigger("Idle");

        var cave = FindObjectOfType<CaveGenerator>();
        if (cave != null)
        {
            cave.currentLevel = 1;
            cave.GenerateMap();
            cave.canMove = true;
            cave.canOpenInventory = true;
        }

        StartCoroutine(RestoreLight());
        battleUI.SetActive(false);

        Debug.Log("玩家重生并回到第一层！");
    }
}
