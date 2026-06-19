using UnityEngine;

public struct HumanData
{
    public float maxHp;
    public float maxStamina;
    
    public float hp;
    public float stamina;

    public bool isAlive;
    
    public ArmorType armor;
    public Profession profession;
    public Specialization specialization;

    public int factionId;

    public Vector2 position;
    public Vector2 velocity;
}