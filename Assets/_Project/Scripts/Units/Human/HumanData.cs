using UnityEngine;

public struct HumanData
{
    public float maxHp;
    public float maxStamina;
    
    public float hp;
    public float stamina;

    public bool isExhausted;
    public bool hadTargetBeforeExhaustion;

    public float baseSpeed;

    public float attackDamage;
    public float attackRange;
    public float attackCooldown;
    public float attackTimer;

    public bool isAlive;

    public Vector2 targetPosition;
    public Vector2 avoidTargetPosition;
    public bool hasTarget;
    public bool forcedTarget;
    public bool isAvoiding;
    
    public ArmorType armor;
    public Profession profession;
    public Specialization specialization;

    public int factionId;

    public Vector2 position;
    public Vector2 previousPosition;
    public Vector2 velocity;
}