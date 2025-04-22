using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyFactory
{
    public static EnemyStats CreateEnemyAtPosition(Vector2Int pos, int seed)
    {
        CaveGenerator cave = GameObject.FindObjectOfType<CaveGenerator>();
        int level = cave != null ? cave.currentLevel : 1;

        int hash = seed ^ (pos.x * 73856093) ^ (pos.y * 19349663);
        System.Random rng = new System.Random(hash);

        // 如果是死胡同，就生成 Mimic
        if (cave != null && cave.IsDeadEnd(pos.x, pos.y))
        {
            return new EnemyStats
            {
                type = EnemyType.Mimic,
                maxHP = 120 + (level - 1) * 120,
                attack = 20 + (level - 1) * 8,
                attackInterval = 3f,
                defense = 0
            };
        }

        // 普通敌人类型（不包含 Mimic）
        EnemyType[] allowedTypes = new EnemyType[] { EnemyType.Slime, EnemyType.Goblin, EnemyType.Bat };
        EnemyType type = allowedTypes[rng.Next(allowedTypes.Length)];

        switch (type)
        {
            case EnemyType.Slime:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 100 + (level - 1) * 100,
                    attack = 5 + (level - 1) * 3,
                    attackInterval = 1.5f,
                    defense = 0 + (level - 1) * 5
                };
            case EnemyType.Goblin:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 70 + (level - 1) * 50,
                    attack = 7 + (level - 1) * 5,
                    attackInterval = 1f,
                    defense = 3 + (level - 1) * 4
                };
            case EnemyType.Bat:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 50 + (level - 1) * 40,
                    attack = 6 + (level - 1) * 3,
                    attackInterval = 0.7f,
                    defense = 2 + (level - 1) * 3
                };
            default:
                return null;
        }
    }

    public static EnemyType GetEnemyTypeAtPosition(Vector2Int pos, int seed)
    {
        int hash = seed ^ (pos.x * 73856093) ^ (pos.y * 19349663);
        System.Random rng = new System.Random(hash);

        CaveGenerator cave = GameObject.FindObjectOfType<CaveGenerator>();
        if (cave != null && cave.IsDeadEnd(pos.x, pos.y))
            return EnemyType.Mimic;

        EnemyType[] allowedTypes = new EnemyType[] { EnemyType.Slime, EnemyType.Goblin, EnemyType.Bat };
        return allowedTypes[rng.Next(allowedTypes.Length)];
    }
}
