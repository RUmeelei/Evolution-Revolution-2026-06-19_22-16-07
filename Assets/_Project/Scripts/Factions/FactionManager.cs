using UnityEngine;
using System.Collections.Generic;

public class FactionManager : MonoBehaviour
{
    [SerializeField] private FactionData[] factions;

    void Awake()
    {
        GameManager.RegisterFactionManager(this);
    }

    public void Initialize()
    {
        System.Array.Sort(factions, (a, b) => a.factionId.CompareTo(b.factionId));
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

    private Dictionary<int, Vector2> factionCenters = new Dictionary<int, Vector2>();

    public void AssignFactionsToRegions(RegionManager regionManager, TileManager tileManager)
    {
        var regions = regionManager.AllRegions;

        if (regions == null || regions.Count == 0)
        {
            Debug.LogError("Нет регионов для назначения фракций!"); return;
        }
        
        List<RegionData> shuffled = new List<RegionData>(regions);

        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        
        int factionIndex = 0;

        for (int f = 0; f < FactionCount; f++)
        {
            FactionData faction = GetFaction(f);

            if (faction == null) continue;

            if (factionIndex >= shuffled.Count)
            {
                Debug.LogWarning($"Не хватило регионов для фракции {faction.factionName}"); continue;
            }

            RegionData region = shuffled[factionIndex];

            factionIndex++;
            
            foreach (Vector2Int pos in region.tiles)
            {
                TileData tile = tileManager.GetTile(pos.x, pos.y);

                tile.factionId = faction.factionId;

                tileManager.SetTile(pos.x, pos.y, tile);
            }
            
            Vector2 center = Vector2.zero;

            foreach (Vector2Int pos in region.tiles)
            {
                center += new Vector2(pos.x, pos.y);
            }
            center /= region.tiles.Count;
            
            factionCenters[faction.factionId] = center;
        }
    }

    public Vector2 GetFactionCenter(int factionId)
    {
        if (factionCenters.TryGetValue(factionId, out Vector2 center))
            return center;
        return Vector2.zero;
    }
}