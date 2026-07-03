using UnityEngine;

public class PoliticsManager : MonoBehaviour
{
    public enum SocialGroupArchetype
    {
        Elite,
        Producers,
        Traders,
        Ideologists,
        Military,
    }
    
    [System.Serializable]
    public struct SocialGroupState
    {
        public SocialGroupArchetype archetype;
        public float loyalty; // 0-100
        public float foodSatisfaction; // 0-1
        public float securitySatisfaction; // 0-1
        public float ideologySatisfaction; // 0-1
        public float tradeSatisfaction; // 0-1
    }

    // [factionId][groupIndex]
    private SocialGroupState[][] factionGroups;

    private FactionManager factionManager;
    private DiplomacyManager diplomacyManager;
    private TileManager tileManager;
    private EconomyManager economyManager;

    public void Initialize()
    {
        factionManager = FindFirstObjectByType<FactionManager>();

        diplomacyManager = FindFirstObjectByType<DiplomacyManager>();

        tileManager = FindFirstObjectByType<TileManager>();

        economyManager = FindFirstObjectByType<EconomyManager>();

        int factionCount = factionManager.FactionCount;

        factionGroups = new SocialGroupState[factionCount][];
        
        for (int f = 0; f < factionCount; f++)
        {
            factionGroups[f] = new SocialGroupState[5];
            for (int g = 0; g < 5; g++)
            {
                factionGroups[f][g] = new SocialGroupState
                {
                    archetype = (SocialGroupArchetype)g,
                    loyalty = 50f,
                    foodSatisfaction = 0.5f,
                    securitySatisfaction = 0.5f,
                    ideologySatisfaction = 0.5f,
                    tradeSatisfaction = 0.5f
                };
            }
        }
    }
    
    public SocialGroupState GetGroupState(int factionId, SocialGroupArchetype archetype)
    {
        if (factionId < 0 || factionId >= factionGroups.Length) return default;

        foreach (var state in factionGroups[factionId])
        {
            if (state.archetype == archetype) return state;
        }

        return default;
    }

    public SocialGroupState[] GetFactionGroups(int factionId)
    {
        if (factionId >= 0 && factionId < factionGroups.Length) return factionGroups[factionId];

        return null;
    }

    public float GetFactionLoyalty(int factionId)
    {
        if (factionId < 0 || factionId >= factionGroups.Length) return 50f;

        float sum = 0f;

        foreach (var state in factionGroups[factionId]) sum += state.loyalty;
        
        return sum / factionGroups[factionId].Length;
    }

    public void UpdatePolitics(float delta)
    {
        EconomyManager economy = FindFirstObjectByType<EconomyManager>();

        if (diplomacyManager == null)
        {
            diplomacyManager = FindFirstObjectByType<DiplomacyManager>();
        }

        for (int f = 0; f < factionGroups.Length; f++)
        {
            for (int g = 0; g < factionGroups[f].Length; g++)
            {
                SocialGroupState state = factionGroups[f][g];
                
                float food = economy != null ? economy.GetFood(f) : 0f;
                float foodNeed = 10000f;

                state.foodSatisfaction = Mathf.Clamp01(food / foodNeed);

                bool atWar = false;

                if (diplomacyManager != null)
                {
                    for (int other = 0; other < factionManager.FactionCount; other++)
                    {
                        if (other == f) continue;

                        var rel = diplomacyManager.GetRelations(f, other);

                        if (rel.atWar)
                        {
                            atWar = true;

                            break;
                        }
                    }
                }
                
                if (state.archetype == SocialGroupArchetype.Producers || state.archetype == SocialGroupArchetype.Traders)
                {
                    state.securitySatisfaction = atWar ? 0.2f : 0.8f;
                }
                else if (state.archetype == SocialGroupArchetype.Military)
                {
                    state.securitySatisfaction = atWar ? 0.9f : 0.5f;
                }
                else
                {
                    state.securitySatisfaction = 0.5f;
                }
                
                if (state.archetype == SocialGroupArchetype.Traders)
                {
                    state.tradeSatisfaction = atWar ? 0.2f : 0.8f;
                }
                else
                {
                    state.tradeSatisfaction = 0.5f;
                }

                state.ideologySatisfaction = 0.5f;
                
                float buildingBonus = 0f;
                if (tileManager != null)
                {
                    int farmCount = tileManager.GetBuildingCount(f, BuildingType.Farm);
                    int marketCount = tileManager.GetBuildingCount(f, BuildingType.Market);
                    int templeCount = tileManager.GetBuildingCount(f, BuildingType.Temple);
                    int wallCount = tileManager.GetBuildingCount(f, BuildingType.Wall);

                    switch (state.archetype)
                    {
                        case SocialGroupArchetype.Producers: buildingBonus = farmCount * 0.1f; break;
                        case SocialGroupArchetype.Traders: buildingBonus = marketCount * 0.1f; break;
                        case SocialGroupArchetype.Ideologists: buildingBonus = templeCount * 0.1f; break;
                        case SocialGroupArchetype.Military: buildingBonus = wallCount * 0.1f; break;
                    }
                }
                
                float avgSatisfaction = (state.foodSatisfaction + state.securitySatisfaction + state.tradeSatisfaction + state.ideologySatisfaction) / 4f + buildingBonus;

                float targetLoyalty = Mathf.Min(avgSatisfaction * 100f, 100f);

                state.loyalty = Mathf.MoveTowards(state.loyalty, targetLoyalty, delta * 5f);
                
                factionGroups[f][g] = state;
            }
        }
    }

    public void ProcessConstruction()
    {
        if (tileManager == null) return;

        for (int f = 0; f < factionGroups.Length; f++)
        {
            for (int g = 0; g < factionGroups[f].Length; g++)
            {
                SocialGroupState state = factionGroups[f][g];
                
                BuildingType neededBuilding = BuildingType.None;

                if (state.foodSatisfaction < 0.5f) neededBuilding = BuildingType.Farm;
                else if (state.tradeSatisfaction < 0.5f) neededBuilding = BuildingType.Market;
                else if (state.ideologySatisfaction < 0.5f) neededBuilding = BuildingType.Temple;
                else if (state.securitySatisfaction < 0.5f) neededBuilding = BuildingType.Wall;

                if (neededBuilding == BuildingType.None) continue;
                
                bool found = false;

                TileData bestTile = default;

                Vector2Int bestPos = Vector2Int.zero;

                for (int y = 0; y < tileManager.height && !found; y++)
                {
                    for (int x = 0; x < tileManager.width && !found; x++)
                    {
                        TileData tile = tileManager.GetTile(x, y);

                        if (tile.factionId != f) continue;

                        if (tile.tileType != TileType.Grass) continue;

                        if (tile.buildings.Exists(b => b.type == neededBuilding)) continue;

                        bestTile = tile;
                        bestPos = new Vector2Int(x, y);
                        found = true;
                    }
                }

                if (found)
                {
                    bestTile.buildings.Add(new BuildingData
                    {
                        type = neededBuilding,
                        level = 1,
                        factionId = f
                    });

                    tileManager.AddBuildingCount(f, neededBuilding);

                    TilemapVisualManager tvm = FindFirstObjectByType<TilemapVisualManager>();

                    tvm?.UpdateBuildingTile(bestPos.x, bestPos.y, bestTile);

                    break;
                }
            }
        }
    }
}