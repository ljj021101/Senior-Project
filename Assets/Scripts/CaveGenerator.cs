using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    public GameObject wallPrefab; // 需要在Unity编辑器中指定这个Prefab
    public GameObject treasurePrefab; // 宝箱Prefab
    public GameObject enemyPrefab; // 敌人Prefab
    public int numberOfWalkers = 20;
    private int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        do
        {
            InitializeMap();
            SimulateWalkers();
            GenerateTreasuresAndEnemies();
            RandomlyPlaceEnemies();
        } while (!IsValidMap());

        DrawMap(); // 只有验证通过后才绘制地图
    }


    void InitializeMap()
    {
        map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = 1; // 初始化地图全为墙（1代表墙）
            }
        }
    }

    void SimulateWalkers()
    {
        List<Walker> walkers = new List<Walker>();
        for (int i = 0; i < numberOfWalkers; i++)
        {
            walkers.Add(new Walker(Random.Range(0, width), Random.Range(0, height)));
        }

        int totalMoves = 300;
        for (int move = 0; move < totalMoves; move++)
        {
            foreach (var walker in walkers)
            {
                walker.Move(width, height, map); // 在Move方法中更新地图
            }
        }
    }

    bool IsValidMap()
    {
        int[,] quadrantCount = new int[2, 2];
        int totalPaths = 0;

        // 计算每个象限的通路数和总通路数
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0)
                {
                    int qx = x < width / 2 ? 0 : 1;
                    int qy = y < height / 2 ? 0 : 1;
                    quadrantCount[qx, qy]++;
                    totalPaths++;
                }
            }
        }

        // 检查每个象限和总通路数是否符合条件
        return totalPaths >= 400 &&
            quadrantCount[0,0] >= 100 &&
            quadrantCount[0,1] >= 100 &&
            quadrantCount[1,0] >= 100 &&
            quadrantCount[1,1] >= 100;
    }

    void GenerateTreasuresAndEnemies()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == 0 && IsDeadEnd(x, y) && Random.Range(0, 100) > 70)
                {
                    map[x, y] = 2; // 将宝箱位置设置为2
                    PlaceEnemyNearTreasureInMap(x, y); // 在地图中标记敌人位置为3
                }
            }
        }
    }

    void PlaceEnemyNearTreasureInMap(int tx, int ty)
    {
        // 检查四个方向寻找可放置敌人的位置
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

        return wallCount == 3; // 如果三面是墙则为死胡同
    }

    void RandomlyPlaceEnemies()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == 0) // 检查是不是通路
                {
                    int pathCount = CountPathsAround(x, y);
                    float chance = GetSpawnChanceForPathCount(pathCount);

                    if (Random.Range(0f, 100f) < chance)
                    {
                        map[x, y] = 3; // 在地图上标记为敌人
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

    void DrawMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                if (map[x, y] == 1)
                {
                    Instantiate(wallPrefab, position, Quaternion.identity);
                }
                else if (map[x, y] == 2)
                {
                    Instantiate(treasurePrefab, position, Quaternion.identity);
                }
                else if (map[x, y] == 3)
                {
                    Instantiate(enemyPrefab, position, Quaternion.identity);
                }
            }
        }
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
            int direction = Random.Range(0, 4); // 随机选择一个方向
            int steps = GetWeightedRandomStep(); // 使用加权随机获取步数

            for (int i = 0; i < steps; i++)
            {
                int newX = position.x;
                int newY = position.y;

                switch (direction)
                {
                    case 0: // 向上
                        newY++;
                        break;
                    case 1: // 向下
                        newY--;
                        break;
                    case 2: // 向左
                        newX--;
                        break;
                    case 3: // 向右
                        newX++;
                        break;
                }

                // 检查是否超出边界
                if (newX >= 1 && newX < width - 1 && newY >= 1 && newY < height - 1)
                {
                    position.x = newX;
                    position.y = newY;
                    map[position.x, position.y] = 0; // 更新地图
                }
                else
                {
                    // 如果超出边界，取消此次移动
                    break;
                }
            }
        }

        // 加权随机步数生成函数
        private int GetWeightedRandomStep()
        {
            List<int> weightedSteps = new List<int>();
            for (int i = 1; i <= 15; i++)
            {
                if (i <= 3)
                {
                    // 将1至5步各添加5次到列表中
                    for (int j = 0; j < 20; j++)
                    {
                        weightedSteps.Add(i);
                    }
                }
                else
                {
                    // 将6至20步各添加1次到列表中
                    weightedSteps.Add(i);
                }
            }

            // 从加权列表中随机选择一个元素
            int index = Random.Range(0, weightedSteps.Count);
            return weightedSteps[index];
        }
    }
}
