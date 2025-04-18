using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Slime, Goblin, Bat }

[System.Serializable]
public class EnemyStats
{
    public EnemyType type;
    public int maxHP;
    public int attack;
    public float attackInterval;
    public int defense;
    public Sprite portrait;
}
