using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private HumanData[] humans;

    public HumanData[] Humans => humans;

    private int aliveCount = 0;

    public void Initialize(int maxHumans)
    {
        humans = new HumanData[maxHumans];

        Debug.Log($"Initialized UnitManager with capacity for {maxHumans} humans.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            CreateHuman(new Vector2 (Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            RandomMoveUnit(GetRandomUnitIndex());
        }
    }

    public void CreateHuman(Vector2 pos, int faction = -1, float maxhp = 100f, float maxstamina = 100f, float basespeed = 0.7f, ArmorType armor = ArmorType.None, Profession profession = Profession.None, Specialization specialization = Specialization.None)
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

            isAlive = true,

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
        MoveUnit(index, new Vector2 (Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
    }

    public void MoveUnit(int index, Vector2 pos)
    {
        if (index >= 0 && index < humans.Length && humans[index].isAlive)
        {
            humans[index].targetPosition = pos;

            if (!humans[index].isExhausted)
            {
                humans[index].hasTarget = true;
            }
        }
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

    public void Tick(float delta)
    {
        for (int i = 0; i < humans.Length; i++)
        {
            HumanData human = humans[i];

            if (human.hp <= 0)
            {
                HumanDeath(i);
            }

            if (!human.isAlive) continue;

            if (human.hasTarget)
            {
                Vector2 toTarget = human.targetPosition - human.position;

                if (toTarget.sqrMagnitude < 0.01f)
                {
                    human.velocity = Vector2.zero;
        
                    human.hasTarget = false;
                }
                else
                {
                    float speedMultiplier = Mathf.Lerp(0.3f, 1f, human.stamina / human.maxStamina);
                    float currentMaxSpeed = human.baseSpeed * speedMultiplier;
        
                    Vector2 desired = toTarget.normalized * currentMaxSpeed;

                    human.stamina -= delta * 3f;

                    human.velocity = Vector2.Lerp(human.velocity, desired, delta * 2f);
                }
            }
            else
            {
                human.velocity = Vector2.Lerp(human.velocity, Vector2.zero, delta * 2f);

                human.stamina += delta * 1f;
            }

            if (human.stamina <= 0)
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

            human.previousPosition = human.position;

            human.position += human.velocity * delta;
            
            humans[i] = human;
        }
    }
}
