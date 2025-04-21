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

        int level = GameObject.FindObjectOfType<CaveGenerator>().currentLevel;

        switch (type)
        {
            case EnemyType.Slime:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 100 + (level - 1) * 100,
                    attack = 5 + (level - 1) * 3,
                    attackInterval = 1.5f,
                    defense = 0 + (level - 1) * 4
                };
            case EnemyType.Goblin:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 70 + (level - 1) * 50,
                    attack = 10 + (level - 1) * 5,
                    attackInterval = 1f,
                    defense = 3 + (level - 1) * 3
                };
            case EnemyType.Bat:
                return new EnemyStats
                {
                    type = type,
                    maxHP = 50 + (level - 1) * 40,
                    attack = 7 + (level - 1) * 3,
                    attackInterval = 0.7f,
                    defense = 2 + (level - 1) * 2
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
