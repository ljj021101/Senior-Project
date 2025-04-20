using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CaveGenerator : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public GameObject[] wallTiles;          // 多个墙体tile的Prefab数组
    private int globalSeed;                 // 全局种子
    public GameObject treasurePrefab;       // 宝箱Prefab
    public GameObject enemyPrefab;          // 敌人Prefab
    public GameObject groundPrefab;         // 地面Prefab
    public GameObject openedTreasurePrefab; // 开启宝箱Prefab
    public GameObject nextLevelPrefab;      // 前往下一层的通道Prefab

    public GameObject floatingIconPrefab;     // 原有的icon
    public GameObject rarityParticlePrefab;

    [Header("UI 显示")]
    public TMP_Text levelText;

    [Header("敌人Prefab")]
    public GameObject slimePrefab;
    public GameObject goblinPrefab;
    public GameObject batPrefab;

    public bool canMove = true;        // 是否允许移动
    public bool canOpenInventory = true;

    public Animator animator; // 请在 Inspector 中将玩家对象上的 Animator 拖入此引用

    public int numberOfWalkers = 20;
    // 矩阵说明：0=通路, 1=墙体, 2=宝箱, 3=敌人, 4=玩家, 5=已开启宝箱, 6=通往下一层通道
    private int[,] map;

    

    // 玩家位置与移动控制
    Vector2Int playerPosition;
    float moveSpeed = 2f;
    private bool isMoving = false;
    public float gridSpacing = 0.32f;

    // 引用
    public InventoryManager inventoryManager;
    public AudioPlayer audioPlayer;
    public Transform playerTransform; // 玩家Transform
    public PlayerStats playerStats;

    // 当前层数计数
    public int currentLevel = 1;

    void Start()
    {
        // 初始随机种子
        globalSeed = Random.Range(0, int.MaxValue);
        GenerateMap();
    }

    void Update()
    {
        MovePlayer();
        if (isMoving)
        {
            
            animator.SetTrigger("IsMoving");
            SmoothMovePlayer();
        }
        else
        {
            animator.SetTrigger("NotMoving");
        }

        // 如果玩家当前位置是 6（通道），并按下 E，则前往下一层
        if (map[playerPosition.x, playerPosition.y] == 6 && Input.GetKeyDown(KeyCode.E))
        {
            NextLevel();
        }
    }

    // 前往下一层：层数+1，重新生成地图
    void NextLevel()
    {
        currentLevel++;
        Debug.Log("前往下一层！当前层数: " + currentLevel);
        globalSeed = Random.Range(0, int.MaxValue);
        GenerateMap();
    }

    void MovePlayer()
    {
        if (!canMove || isMoving) return;
        if (!isMoving)
        {
            Vector2 moveDirection = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
                moveDirection = Vector2.up;
            else if (Input.GetKey(KeyCode.S))
                moveDirection = Vector2.down;
            else if (Input.GetKey(KeyCode.A))
                moveDirection = Vector2.left;
            else if (Input.GetKey(KeyCode.D))
                moveDirection = Vector2.right;

            // 更新动画参数，让 Animator 根据方向播放移动动画
            if (animator != null)
            {
                animator.SetFloat("Horizontal", moveDirection.x);
                animator.SetFloat("Vertical", moveDirection.y);
            }

            // 根据横向输入翻转角色
            if (moveDirection.x < 0)
                playerTransform.localScale = new Vector3(-0.6f, 0.6f, 0);
            else if (moveDirection.x > 0)
                playerTransform.localScale = new Vector3(0.6f, 0.6f, 0);

            if (moveDirection != Vector2.zero)
            {
                Vector2Int gridMove = Vector2Int.RoundToInt(moveDirection);
                Vector2Int newPosition = playerPosition + gridMove;
                if (newPosition.x >= 0 && newPosition.x < width && newPosition.y >= 0 && newPosition.y < height)
                {
                    int target = map[newPosition.x, newPosition.y];
                    // 如果目标格子为通路、敌人或通道，则允许移动
                    if (target == 0 || target == 6 || target == 4)
                    {
                        playerPosition = newPosition;
                        isMoving = true;
                        DrawMap();
                    }
                    if (target == 3)
                    {
                        playerPosition = newPosition;
                        isMoving = true;
                        DrawMap();

                        EnemyStats enemy = EnemyFactory.CreateEnemyAtPosition(newPosition, globalSeed);
                        FindObjectOfType<BattleManager>().StartBattle(enemy, newPosition);
                    }
                    // 如果目标为宝箱，则执行开宝箱逻辑
                    else if (target == 2)
                    {
                        map[newPosition.x, newPosition.y] = 5; // 标记宝箱已开启
                        Debug.Log("宝箱打开，位置：" + newPosition);

                        // 生成装备并添加至背包
                        EquipmentItem newItem = inventoryManager.AddNewItemWithSeed(-1);

                        ShowFloatingIconAboveChest(newPosition, newItem);
                        ShowRarityParticleEffect(newPosition, newItem);

                        // 存档与音效
                        inventoryManager.SaveAll();
                        audioPlayer.PlayChestOpenSound();
                        DrawMap();
                    }
                }
            }
        }
    }

    void ShowFloatingIconAboveChest(Vector2Int chestPos, EquipmentItem item)
    {
        if (floatingIconPrefab == null || item == null) return;

        Vector3 worldPos = new Vector3(chestPos.x * gridSpacing, chestPos.y * gridSpacing + 0.05f, 0f);
        GameObject iconObj = Instantiate(floatingIconPrefab, worldPos, Quaternion.identity);

        var floatingIcon = iconObj.GetComponent<FloatingIcon>();
        if (floatingIcon != null)
        {
            Color tint = Color.white;
            switch (item.rarity)
            {
                case EquipmentRarity.Rare: tint = Color.cyan; break;
                case EquipmentRarity.Legendary: tint = new Color(1f, 0.84f, 0f); break;
            }
            floatingIcon.SetIcon(item.iconImage.sprite, tint);
        }
    }

    private void ShowRarityParticleEffect(Vector2Int chestPos, EquipmentItem item)
    {
        if (rarityParticlePrefab == null || item == null) return;

        // 位置
        Vector3 worldPos = new Vector3(chestPos.x * gridSpacing - 0.03f, chestPos.y * gridSpacing, 0f);

        // 实例化粒子
        GameObject fx = Instantiate(rarityParticlePrefab, worldPos, Quaternion.identity);

        // 设置颜色
        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            switch (item.rarity)
            {
                case EquipmentRarity.Normal:
                    main.startColor = Color.white;
                    break;
                case EquipmentRarity.Rare:
                    main.startColor = Color.cyan;
                    break;
                case EquipmentRarity.Legendary:
                    main.startColor = new Color(1f, 0.84f, 0f); // 金色
                    break;
            }
        }
    }

    void SmoothMovePlayer()
    {
        Vector3 targetPosition = new Vector3(playerPosition.x * gridSpacing, playerPosition.y * gridSpacing, 0);
        if (playerTransform.position != targetPosition)
        {
            playerTransform.position = Vector3.MoveTowards(playerTransform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }
    }

    public void GenerateMap()
    {
        // 重置地图
        InitializeMap();
        // 行走者算法
        SimulateWalkers();
        // 添加宝箱、敌人
        GenerateTreasuresAndEnemies();
        RandomlyPlaceEnemies();

        // 设置玩家起点
        SetPlayerStartPosition();

        // 放置前往下一层的通道
        PlaceNextLevelPassage();

        UpdateLevelText();
        DrawMap();
    }

    void InitializeMap()
    {
        map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = 1; // 全部初始化为墙
            }
        }
    }

    void SimulateWalkers()
    {
        List<Walker> walkers = new List<Walker>();
        // 初始化第一个行走者
        Walker firstWalker = new Walker(Random.Range(0, width), Random.Range(0, height));
        walkers.Add(firstWalker);
        map[firstWalker.position.x, firstWalker.position.y] = 0;

        int totalMoves = 60;
        for (int move = 0; move < totalMoves; move++)
        {
            foreach (var walker in new List<Walker>(walkers))
            {
                walker.Move(width, height, map);
                if (move % (totalMoves / numberOfWalkers) == 0 && move != 0)
                {
                    Vector2Int randomStart = GetRandomPathPosition();
                    Walker newWalker = new Walker(randomStart.x, randomStart.y);
                    walkers.Add(newWalker);
                    map[newWalker.position.x, newWalker.position.y] = 0;
                }
            }
        }
    }

    Vector2Int GetRandomPathPosition()
    {
        List<Vector2Int> possibleStarts = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0) // 通路
                {
                    possibleStarts.Add(new Vector2Int(x, y));
                }
            }
        }
        if (possibleStarts.Count > 0)
        {
            int index = Random.Range(0, possibleStarts.Count);
            return possibleStarts[index];
        }
        return new Vector2Int(-1, -1); // 若无通路返回无效坐标
    }

    void SetPlayerStartPosition()
    {
        Vector2Int startPosition = GetRandomPathPosition();
        if (startPosition.x != -1)
        {
            map[startPosition.x, startPosition.y] = 4;
            playerPosition = startPosition;
            playerTransform.position = new Vector3(startPosition.x * gridSpacing, startPosition.y * gridSpacing, 0);
        }
        else
        {
            Debug.LogError("无法找到有效的玩家起点！");
        }
    }

    void GenerateTreasuresAndEnemies()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == 0 && IsDeadEnd(x, y) && Random.Range(0, 100) > 50)
                {
                    map[x, y] = 2; // 宝箱
                    PlaceEnemyNearTreasureInMap(x, y);
                }
            }
        }
    }

    void PlaceEnemyNearTreasureInMap(int tx, int ty)
    {
        if (map[tx - 1, ty] == 0) map[tx - 1, ty] = 3;
        else if (map[tx + 1, ty] == 0) map[tx + 1, ty] = 3;
        else if (map[tx, ty - 1] == 0) map[tx, ty - 1] = 3;
        else if (map[tx, ty + 1] == 0) map[tx, ty + 1] = 3;
    }

    bool IsDeadEnd(int x, int y)
    {
        int wallCount = 0;
        if (map[x - 1, y] == 1) wallCount++;
        if (map[x + 1, y] == 1) wallCount++;
        if (map[x, y - 1] == 1) wallCount++;
        if (map[x, y + 1] == 1) wallCount++;
        return wallCount == 3;
    }

    void RandomlyPlaceEnemies()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == 0)
                {
                    int pathCount = CountPathsAround(x, y);
                    float chance = GetSpawnChanceForPathCount(pathCount);
                    if (Random.Range(0f, 100f) < chance)
                    {
                        map[x, y] = 3;
                    }
                }
            }
        }
    }

    int CountPathsAround(int x, int y)
    {
        int count = 0;
        if (map[x - 1, y] == 0 || map[x - 1, y] == 3) count++;
        if (map[x + 1, y] == 0 || map[x + 1, y] == 3) count++;
        if (map[x, y - 1] == 0 || map[x, y - 1] == 3) count++;
        if (map[x, y + 1] == 0 || map[x, y + 1] == 3) count++;
        return count;
    }

    float GetSpawnChanceForPathCount(int pathCount)
    {
        switch (pathCount)
        {
            case 2: return 6f;
            case 3: return 40f;
            case 4: return 70f;
            default: return 6f;
        }
    }

    // 在地图生成完成后，选取一个非敌人的通路(=0)格子，设置为6，表示前往下一层的通道
    void PlaceNextLevelPassage()
    {
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == 0) // 仅选取通路(=0)
                {
                    possiblePositions.Add(new Vector2Int(x, y));
                }
            }
        }
        if (possiblePositions.Count > 0)
        {
            int idx = Random.Range(0, possiblePositions.Count);
            Vector2Int passagePos = possiblePositions[idx];
            map[passagePos.x, passagePos.y] = 6;
            Debug.Log("生成下一层通道于: " + passagePos);
        }
    }

    void DrawMap()
    {
        ClearMap();
        int radius = 10;
        int startX = Mathf.Max(0, playerPosition.x - radius);
        int endX = Mathf.Min(width, playerPosition.x + radius + 1);
        int startY = Mathf.Max(0, playerPosition.y - radius);
        int endY = Mathf.Min(height, playerPosition.y + radius + 1);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Vector3 pos = new Vector3(x * gridSpacing, y * gridSpacing, 0);

                // 如果不是墙，则先生成地面
                if (map[x, y] != 1)
                {
                    Instantiate(groundPrefab, pos, Quaternion.identity);
                }

                // 根据地图值生成对应物体
                if (map[x, y] == 1)
                {
                    int index = GetTileIndex(x, y);
                    Instantiate(wallTiles[index], pos, Quaternion.identity);
                }
                else if (map[x, y] == 2)
                {
                    Instantiate(treasurePrefab, pos, Quaternion.identity);
                }
                else if (map[x, y] == 3)
                {
                    // 根据坐标和种子确定敌人类型
                    EnemyType type = EnemyFactory.GetEnemyTypeAtPosition(new Vector2Int(x, y), globalSeed);

                    GameObject prefabToUse = slimePrefab;
                    switch (type)
                    {
                        case EnemyType.Goblin: prefabToUse = goblinPrefab; break;
                        case EnemyType.Bat: prefabToUse = batPrefab; break;
                    }

                    Instantiate(prefabToUse, pos, Quaternion.identity);
                }
                else if (map[x, y] == 5)
                {
                    Instantiate(openedTreasurePrefab, pos, Quaternion.identity);
                }
                // 6 表示通道，需要一个 nextLevelPrefab
                else if (map[x, y] == 6)
                {
                    // 在 Inspector 中为 nextLevelPrefab 指定一个传送门或楼梯外观
                    Instantiate(nextLevelPrefab, pos, Quaternion.identity);
                }
                // 4(玩家) 不在这里绘制，由其他脚本控制玩家对象位置
            }
        }
    }

    void ClearMap()
    {
        GameObject[] oldObjects = GameObject.FindGameObjectsWithTag("map");
        foreach (GameObject obj in oldObjects)
        {
            Destroy(obj);
        }
    }

    public void ClearEnemyTile(Vector2Int pos)
    {
        map[pos.x, pos.y] = 0;
        DrawMap();
    }

    int GetTileIndex(int x, int y)
    {
        // 利用 globalSeed 和坐标计算一个确定性随机值
        int tileSeed = globalSeed ^ (x * 73856093) ^ (y * 19349663);
        if (tileSeed < 0)
            tileSeed = -tileSeed;
        System.Random rng = new System.Random(tileSeed);
        return rng.Next(0, wallTiles.Length);
    }

    class Walker
    {
        public Vector2Int position;

        public Walker(int x, int y)
        {
            position = new Vector2Int(x, y);
        }

        public void Move(int width, int height, int[,] map)
        {
            int direction = Random.Range(0, 4);
            int steps = GetWeightedRandomStep();

            for (int i = 0; i < steps; i++)
            {
                int newX = position.x;
                int newY = position.y;

                switch (direction)
                {
                    case 0: newY++; break;  // 上
                    case 1: newY--; break;  // 下
                    case 2: newX--; break;  // 左
                    case 3: newX++; break;  // 右
                }

                if (newX >= 1 && newX < width - 1 && newY >= 1 && newY < height - 1)
                {
                    position.x = newX;
                    position.y = newY;
                    map[position.x, position.y] = 0; // 开路
                }
                else
                {
                    break;
                }
            }
        }

        private int GetWeightedRandomStep()
        {
            List<int> weightedSteps = new List<int>();
            for (int i = 1; i <= 15; i++)
            {
                if (i <= 3)
                {
                    for (int j = 0; j < 20; j++)
                        weightedSteps.Add(i);
                }
                else
                {
                    weightedSteps.Add(i);
                }
            }
            int index = Random.Range(0, weightedSteps.Count);
            return weightedSteps[index];
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"Floor {currentLevel}";
        }
    }
}
