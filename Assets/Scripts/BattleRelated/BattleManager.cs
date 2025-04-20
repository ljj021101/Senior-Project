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
    public GameObject damageTextPrefab;
    public Transform battleCanvas;

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
                    baseDamage *= (playerStats.finalCritDamage / 100f);

                // 减去敌人防御
                float reducedDamage = Mathf.Max(1f, baseDamage - currentEnemy.defense);

                // 添加伤害浮动（80%-120%）
                float finalDamage = reducedDamage * UnityEngine.Random.Range(0.8f, 1.2f);
                finalDamage = Mathf.Max(1f, finalDamage); // 最低为1

                enemyHP -= finalDamage;
                enemyHPBar.value = enemyHP;

                // 播放动画和音效
                playerAnimator.SetTrigger("Slash");
                audioSource?.PlayOneShot(playerAttackClip);
                enemyModelInstance.GetComponentInChildren<HitEffect>()?.Flash();

                // 显示敌人受到伤害的飘字
                ShowDamageText(finalDamage, enemyHPBar.GetComponent<RectTransform>(), isCrit);

                // Debug log
                Debug.Log(isCrit
                    ? $"暴击！对敌人造成了 {finalDamage:F1} 点伤害！"
                    : $"对敌人造成了 {finalDamage:F1} 点伤害");
            }


            if (enemyTimer >= currentEnemy.attackInterval)
            {
                enemyTimer = 0f;

                float baseDamage = currentEnemy.attack;
                float reducedDamage = Mathf.Max(1f, baseDamage - playerStats.finalDefense);

                float finalDamage = reducedDamage * UnityEngine.Random.Range(0.8f, 1.2f);
                finalDamage = Mathf.Max(1f, finalDamage);

                playerStats.currentHP -= finalDamage;
                playerHPBar.value = playerStats.currentHP;

                // 动画和音效
                enemyBattleController.animator.SetTrigger("Attack");
                switch (currentEnemy.type)
                {
                    case EnemyType.Slime: audioSource?.PlayOneShot(slimeAttackClip); break;
                    case EnemyType.Goblin: audioSource?.PlayOneShot(goblinAttackClip); break;
                    case EnemyType.Bat: audioSource?.PlayOneShot(batAttackClip); break;
                }
                playerModelInstance.GetComponentInChildren<HitEffect>()?.Flash();

                // 显示玩家受到伤害的飘字
                ShowDamageText(finalDamage, playerHPBar.GetComponent<RectTransform>(), false);
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

    public void ShowDamageText(float amount, RectTransform anchor, bool isCrit = false)
    {
        if (damageTextPrefab == null || anchor == null || battleCanvas == null) return;

        GameObject popup = Instantiate(damageTextPrefab, battleCanvas);
        RectTransform popupRect = popup.GetComponent<RectTransform>();

        popupRect.position = anchor.position + new Vector3(0, 40f, 0);
        popupRect.localScale = Vector3.one;

        TMP_Text text = popup.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.text = isCrit ? $"<color=red>{Mathf.RoundToInt(amount)}!</color>" : Mathf.RoundToInt(amount).ToString();
            StartCoroutine(AnimateDamageText(popupRect, text));
        }
    }

    IEnumerator AnimateDamageText(RectTransform rect, TMP_Text text)
    {
        float duration = 1f;
        float elapsed = 0f;

        Vector3 startPos = rect.position;
        Vector3 endPos = startPos + new Vector3(0, 100f, 0);

        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 透明

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rect.position = Vector3.Lerp(startPos, endPos, t);
            text.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        Destroy(rect.gameObject);
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

    public void RestartFromLevelOne()
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
