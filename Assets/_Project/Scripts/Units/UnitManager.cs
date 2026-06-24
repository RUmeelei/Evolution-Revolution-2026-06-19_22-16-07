using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    private HumanData[] humans;

    public HumanData[] Humans => humans;

    private int aliveCount = 0;

    private TileManager tileManager;
    private UnitVisualManager unitVisualManager;
    private FactionManager factionManager;

    public void Initialize(int maxHumans)
    {
        humans = new HumanData[maxHumans];

        Debug.Log($"Initialized UnitManager with capacity for {maxHumans} humans.");

        if (unitVisualManager == null) unitVisualManager = FindFirstObjectByType<UnitVisualManager>();

        if (factionManager == null) factionManager = FindFirstObjectByType<FactionManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            for (int i = 0; i < humans.Length; i++)
            {
                CreateHuman(new Vector2 (Random.Range(5f, 10f), Random.Range(5f, 10f)));
            } 
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            RandomMoveUnit(GetRandomUnitIndex());
        }
    }

    public void SpawnHuman(Vector2 pos, int faction)
    {
        CreateHuman(pos, faction);
    }

    public void CreateHuman(Vector2 pos, int faction = -1, float maxhp = 100f, float maxstamina = 100f, float basespeed = 0.7f, float attackdamage = 10f, float attackrange = 0.45f, float attackcooldown = 1f, ArmorType armor = ArmorType.None, Profession profession = Profession.None, Specialization specialization = Specialization.None)
    {
        HumanData human = new HumanData()
        {
            maxHp = Mathf.Max(1f, maxhp),
            maxStamina = Mathf.Max(1f, maxstamina),

            hp = maxhp,
            stamina = maxstamina,

            isExhausted = false,
            hadTargetBeforeExhaustion = false,

            baseSpeed = basespeed,

            attackDamage = attackdamage,
            attackRange = attackrange,
            attackCooldown = attackcooldown,
            attackTimer = 0f,

            isAlive = true,
            
            targetPosition = pos,

            armor = armor,
            profession = profession,
            specialization = specialization,

            factionId = faction,
            
            position = pos,
        };

        for (int i = 0; i < humans.Length; i++)
        {
            HumanData _human = humans[i];
         
            if (!_human.isAlive)
            {
                humans[i] = human;
                aliveCount++;
                return;
            }
        }
    }

    public void RandomMoveUnit(int index)
    {
        MoveUnit(index, humans[index].position + new Vector2 (Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
    }

    public void MoveUnit(int index, Vector2 pos, bool forced = false)
    {
        if (!forced && humans[index].forcedTarget) return;

        if (index >= 0 && index < humans.Length && humans[index].isAlive)
        {
            humans[index].targetPosition = pos;

            if (!humans[index].isExhausted)
            {
                humans[index].hasTarget = true;
            }

            if (forced)
            {
                humans[index].forcedTarget = true;
            }
        }
    }

    public void MoveUnits(List<int> indices, Vector2 target, bool forced = false)
    {
        foreach (int idx in indices) MoveUnit(idx, target, forced);
    }

    public int GetRandomUnitIndex()
    {
        if (aliveCount == 0) return -1;

        int target = Random.Range(0, aliveCount);
        int counter = 0;

        for (int i = 0; i < humans.Length; i++)
        {
            if (humans[i].isAlive)
            {
                if (counter == target) return i;
                counter++;
            }
        }

        return -1;
    }

    public List<int> GetUnitsInRect(Rect worldRect)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive) continue;

            if (worldRect.Contains(humans[i].position) && factionManager.IsPlayerFaction(humans[i].factionId)) result.Add(i);
        }

        return result;
    }

    public List<int> GetUnitAtPosition(Vector2 pos)
    {
        List<int> result = new List<int>();

        float pickRadius = 0.1f;

        if (unitVisualManager != null) pickRadius = unitVisualManager.SpriteRadius;

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive) continue;

            if ((humans[i].position == pos || Vector2.Distance(humans[i].position, pos) < pickRadius) && factionManager.IsPlayerFaction(humans[i].factionId)) result.Add(i);
        }

        return result;
    }

    public void RecountAliveHumans()
    {
        aliveCount = 0;

        for (int i = 0; i < humans.Length; i++)
        {
            if (humans[i].isAlive)
            {
                aliveCount++;
            }
        }
    }

    public void HumanDeath(int index)
    {
        if (humans[index].isAlive)
        {
            humans[index].isAlive = false;
            aliveCount--;
        }
    }

    public bool IsPositionBlocked(Vector2 pos, int selfidx, float radius)
    {
        for (int j = 0; j < humans.Length; j++)
        {
            if (j == selfidx) continue;

            if (!humans[j].isAlive) continue;

            if (Vector2.Distance(pos, humans[j].position) < radius) return true;
        }

        return false;
    }

    public void Tick(float delta)
    {
        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();

        for (int i = 0; i < humans.Length; i++)
        {
            HumanData human = humans[i];

            if (human.hp <= 0)
            {
                HumanDeath(i);
                human = humans[i];
            }

            if (!human.isAlive) continue;

            if (human.attackTimer > 0)
            {
                human.attackTimer -= delta;
            }

            if (human.attackTimer <= 0)
            {
                int enemyIndex = -1;

                float closestDist = human.attackRange;

                for (int j = 0; j < humans.Length; j++)
                {
                    if (!humans[j].isAlive) continue;

                    if (humans[j].factionId == human.factionId) continue;

                    float dist = Vector2.Distance(human.position, humans[j].position);

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        enemyIndex = j;
                    }
                }

                if (enemyIndex != -1)
                {
                    humans[enemyIndex].hp -= human.attackDamage;

                    human.attackTimer = human.attackCooldown;

                    Debug.Log($"Unit {i} (faction {human.factionId}) attacks unit {enemyIndex} (faction {humans[enemyIndex].factionId}) for {human.attackDamage} damage. HP left: {humans[enemyIndex].hp}");
                }
            }

            human.previousPosition = human.position;

            float radius = unitVisualManager.SpriteRadius;

            if (human.hasTarget)
            {
                Vector2 toTarget = human.targetPosition - human.position;
            
                float speedMultiplier = Mathf.Lerp(0.3f, 1f, human.stamina / human.maxStamina);

                speedMultiplier /= tileManager.CalculateWorldMoveCost(human.position);

                float currentMaxSpeed = human.baseSpeed * speedMultiplier;
            
                Vector2 desired = toTarget.normalized * currentMaxSpeed;

                human.stamina -= delta * 2f;
                human.velocity = Vector2.Lerp(human.velocity, desired, delta * 2f);

                Vector2 newPos = human.position + human.velocity * delta;

                if (IsPositionBlocked(newPos, i, radius))
                {
                    Vector2 avoidDir = Random.insideUnitCircle.normalized;
                    Vector2 avoidPos = newPos + avoidDir * radius * 4f;

                    if (!IsPositionBlocked(avoidPos, i, radius) && tileManager.IsWorldPassable(avoidPos))
                    {
                        human.isAvoiding = true;
                        human.targetPosition += Random.insideUnitCircle * radius * 2f;
                    }
                    else
                    {
                        human.velocity = Vector2.zero;
                        human.isAvoiding = true;

                        continue;
                    }
                }
                else
                {
                    human.isAvoiding = false;
                }

                float remainingDistance = toTarget.magnitude;

                Vector2 offset = newPos - human.position;
            
                if (offset.magnitude > remainingDistance)
                {
                    offset = offset.normalized * remainingDistance;
                    newPos = human.position + offset;
                }
            
                if (tileManager != null && tileManager.IsWorldPassable(newPos))
                {
                    human.position = newPos;
                }
                else
                {
                    human.velocity = Vector2.zero;
                    human.hasTarget = false;
                    human.forcedTarget = false;
                }
            
                if (Vector2.Distance(human.position, human.targetPosition) < 0.1f)
                {
                    human.velocity = Vector2.zero;
                    human.hasTarget = false;
                    human.forcedTarget = false;
                }
            }
            else
            {
                human.velocity = Vector2.Lerp(human.velocity, Vector2.zero, delta * 2f);

                Vector2 newPos = human.position + human.velocity * delta;

                human.stamina += delta * 3f;

                if (IsPositionBlocked(newPos, i, radius))
                {
                    Vector2 avoidDir = Random.insideUnitCircle.normalized;
                    Vector2 avoidPos = newPos + avoidDir * radius * 4f;

                    if (!IsPositionBlocked(avoidPos, i, radius) && tileManager.IsWorldPassable(avoidPos))
                    {
                        human.hasTarget = true;
                        human.isAvoiding = true;
                        human.targetPosition += Random.insideUnitCircle * radius * 2f;
                    }
                    else
                    {
                        human.velocity = Vector2.zero;

                        continue;
                    }
                }
                
                if (tileManager != null && tileManager.IsWorldPassable(newPos)) human.position = newPos;
                else human.velocity = Vector2.zero;
            }

            if (human.stamina <= 0 && !human.isAvoiding)
            {
                human.hadTargetBeforeExhaustion = human.hasTarget;
                human.isExhausted = true;
                human.hasTarget = false;
            }
            else if (human.stamina > 40f)
            {
                human.isExhausted = false;

                if (human.hadTargetBeforeExhaustion || human.targetPosition != human.position)
                {
                    human.hasTarget = true;
                    human.hadTargetBeforeExhaustion = false;
                }
            }

            human.stamina = Mathf.Clamp(human.stamina, 0, human.maxStamina);
            
            humans[i] = human;
        }
    }
}