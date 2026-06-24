using UnityEngine;

public class FactionManager : MonoBehaviour
{
    [SerializeField] private FactionData[] factions;

    public void Initialize()
    {
        System.Array.Sort(factions, (a, b) => a.factionId.CompareTo(b.factionId));

        for (int i = 0; i < factions.Length; i++)
        {
            Debug.Log($"Faction {factions[i].factionName} color : {factions[i].factionColor}");
        }
    }

    public FactionData GetFaction(int factionId)
    {
        foreach (var f in factions)
        {
            if (f.factionId == factionId) return f;
        }
        
        return null;
    }

    public int FactionCount => factions.Length;

    public bool IsPlayerFaction(int factionId)
    {
        FactionData data = GetFaction(factionId);
        return data != null && data.isPlayer;
    }
    
    public Sprite GetUnitSprite(int factionId)
    {
        FactionData data = GetFaction(factionId);
        return data != null ? data.unitSprite : null;
    }
    
    public Color GetSelectionColor(int factionId)
    {
        FactionData data = GetFaction(factionId);
        return data != null ? data.selectionColor : Color.green;
    }
}