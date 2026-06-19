using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private HumanData[] humans;

    public void Initialize(int maxHumans)
    {
        humans = new HumanData[maxHumans];
    }

    public void CreateHuman(Vector2 pos, int faction = -1, float maxhp = 100f, float maxstamina = 100f, ArmorType armor = ArmorType.None, Profession profession = Profession.None, Specialization specialization = Specialization.None)
    {
        HumanData human = new HumanData()
        {
            maxHp = Mathf.Max(1f, maxhp),
            maxStamina = Mathf.Max(1f, maxstamina),

            hp = maxhp,
            stamina = maxstamina,

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
                return;
            }
        }
    }

    public void Tick(float delta)
    {
        for (int i = 0; i < humans.Length; i++)
        {
            HumanData human = humans[i];

            if (human.hp <= 0)
            {
                human.isAlive = false;
            }

            if (!human.isAlive) continue;
            
            human.position += human.velocity * delta * Mathf.Max(0.3f, human.stamina / human.maxStamina);
            
            human.velocity *= 0.95f;

            human.stamina -= delta * 0.1f;
            
            humans[i] = human;
        }
    }
}
