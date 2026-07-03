using System.Collections.Generic;
using UnityEngine;

public enum DiplomacyEventType
{
    DeclaredWar,
    SignedPeace,
    BrokeCeasefire,
    SignedArmistice,
    StartedBorderClash,
}

public class DiplomacyEvent
{
    public DiplomacyEventType type;
    public int tick;
    public int initiator;
    public int target;
    public float magnitude;
}

public class DiplomacyManager : MonoBehaviour
{
    public class DiplomacyData
    {
        public float tension;
        public bool atWar;
    }
    
    private Dictionary<(int, int), DiplomacyData> allRelations = new();
    
    private FactionManager factionManager;

    private List<DiplomacyEvent> eventHistory = new();

    public void Initialize()
    {
        factionManager = FindFirstObjectByType<FactionManager>();
    }
    
    public DiplomacyData GetRelations(int factionA, int factionB)
    {
        if (factionA < 0 || factionB < 0) return null;

        if (factionA == factionB) return null;

        int min = Mathf.Min(factionA, factionB);
        int max = Mathf.Max(factionA, factionB);

        (int, int) key = (min, max);

        if (!allRelations.ContainsKey(key))
        {
            allRelations[key] = new DiplomacyData
            {
                tension = 0f,
                atWar = false
            };
        }

        return allRelations[key];
    }

    public void DeclareWar(int a, int b)
    {
        var rel = GetRelations(a, b);

        if (rel == null) return;

        rel.atWar = true;
        rel.tension = 100f;

        AddEvent(DiplomacyEventType.DeclaredWar, a, b, 100f);

        if (factionManager.IsPlayerFaction(a) || factionManager.IsPlayerFaction(b))
        {
            AudioManager.Instance?.PlayRandomMusic();
        }
    }

    public void MakePeace(int a, int b)
    {
        var rel = GetRelations(a, b);

        if (rel == null) return;

        rel.atWar = false;
        rel.tension = 0f;

        AddEvent(DiplomacyEventType.SignedPeace, a, b, 60f);
    }

    public bool EvaluateAction(int myFaction, int otherFaction, string actionType, int myUnits, int otherUnits, float myFood, float myLoyalty)
    {
        FactionData myData = factionManager.GetFaction(myFaction);
        FactionData otherData = factionManager.GetFaction(otherFaction);

        if (myData == null) return false;

        var rel = GetRelations(myFaction, otherFaction);

        if (rel == null) return false;

        if (actionType == "declareWar")
        {
            bool strongEnough = myUnits > otherUnits * 1.5f && myFood > 50f;
            bool tensionHigh = rel.tension >= 0f;
        
            float memoryBonus = 0f;

            foreach (var evt in eventHistory)
            {
                if (evt.initiator == myFaction && evt.target == otherFaction && evt.type == DiplomacyEventType.DeclaredWar)
                {
                    memoryBonus += 0.2f;
                }
            }

            float warChance = 0.5f + memoryBonus;

            if (Random.value > warChance) return false;

            if (!strongEnough || !tensionHigh) return false;

            return myData.rulerCharacter switch
            {
                RulerCharacter.Aggressive => Random.value < 0.7f,
                RulerCharacter.Treacherous => Random.value < 0.4f,
                RulerCharacter.Diplomatic => otherData.rulerCharacter == RulerCharacter.Aggressive && Random.value < 0.1f,
                RulerCharacter.Honorable => rel.tension > 80f || otherData.rulerCharacter == RulerCharacter.Aggressive && Random.value < 0.4f,
                _ => otherData.rulerCharacter == RulerCharacter.Aggressive && Random.value < 0.1f
            };
        }
        else if (actionType == "offerPeace")
        {
            bool losing = false; // myUnits < otherUnits * 0.5f || myFood < 30f;
        
            float memoryBonus = 0f;
    
            foreach (var evt in eventHistory)
            {
                if (evt.initiator == myFaction && evt.target == otherFaction && evt.type == DiplomacyEventType.DeclaredWar)
                {
                    memoryBonus -= 0.2f;
                }
            }
            
            float peaceChance = 0.5f + memoryBonus;

            if (Random.value > peaceChance) return false;

            if (!losing) return false;

            return myData.rulerCharacter switch
            {
                RulerCharacter.Aggressive => Random.value < 0.15f,
                RulerCharacter.Honorable => Random.value < 0.4f,
                RulerCharacter.Diplomatic => Random.value < 0.5f,
                RulerCharacter.Treacherous => Random.value < 0.3f,
                _ => false
            };
        }
        else if (actionType == "acceptPeace")
        {
            bool losing = myUnits < 2 || myFood < 20f || myLoyalty < 20f;

            if (!losing) return false;

            return myData.rulerCharacter switch
            {
                RulerCharacter.Honorable => true,
                RulerCharacter.Aggressive => myUnits == 0,
                RulerCharacter.Diplomatic => true,
                RulerCharacter.Treacherous => true,
                _ => false
            };
        }

        return false;
    }

    private void AddEvent(DiplomacyEventType type, int initiator, int target, float magnitude)
    {
        var evt = new DiplomacyEvent
        {
            type = type,
            tick = Time.frameCount,
            initiator = initiator,
            target = target,
            magnitude = magnitude
        };
        eventHistory.Add(evt);
    }

    public void CleanupEvents(int currentTick)
    {
        eventHistory.RemoveAll(e => currentTick - e.tick > 1000);
    }
}