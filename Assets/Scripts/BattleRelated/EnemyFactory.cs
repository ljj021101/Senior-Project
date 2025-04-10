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

        switch (type)
        {
            case EnemyType.Slime:
                return new EnemyStats { type = type, maxHP = 50, attack = 5, attackInterval = 1.5f };
            case EnemyType.Goblin:
                return new EnemyStats { type = type, maxHP = 80, attack = 10, attackInterval = 1f };
            case EnemyType.Bat:
                return new EnemyStats { type = type, maxHP = 30, attack = 7, attackInterval = 0.7f };
            default:
                return null;
        }
    }
}
