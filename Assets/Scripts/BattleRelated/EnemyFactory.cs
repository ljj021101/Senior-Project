using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyFactory
{
    public static EnemyStats CreateEnemyAtPosition(Vector2Int pos, int seed)
    {
        int hash = seed ^ (pos.x * 73856093) ^ (pos.y * 19349663);
        System.Random rng = new System.Random(hash);
        EnemyType type = (EnemyType)(rng.Next(0, System.Enum.GetNames(typeof(EnemyType)).Length));

        // 读取当前层数
        int level = GameObject.FindObjectOfType<CaveGenerator>().currentLevel;

        // 强度加成
        int hpBoost = (level - 1) * 15;
        int atkBoost = (level - 1);

        switch (type)
        {
            case EnemyType.Slime:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 50 + hpBoost,
                    attack = 5 + atkBoost,
                    attackInterval = 1.5f,
                    defense = 1
                };
            case EnemyType.Goblin:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 80 + hpBoost,
                    attack = 10 + atkBoost,
                    attackInterval = 1f,
                    defense = 3
                };
            case EnemyType.Bat:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 30 + hpBoost,
                    attack = 7 + atkBoost,
                    attackInterval = 0.7f,
                    defense = 2
                };
            default:
                return null;
        }
    }

    public static EnemyType GetEnemyTypeAtPosition(Vector2Int pos, int seed)
    {
        int hash = seed ^ (pos.x * 73856093) ^ (pos.y * 19349663);
        System.Random rng = new System.Random(hash);
        return (EnemyType)(rng.Next(0, System.Enum.GetNames(typeof(EnemyType)).Length));
    }
}
