using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public bool enableAI = true;

    private List<int> warFactions = new List<int>();

    private UnitManager unitManager;
    private TileManager tileManager;
    private EconomyManager economyManager;
    private SpawnManager spawnManager;
    private FactionManager factionManager;
    private DiplomacyManager diplomacyManager;

    public void Initialize()
    {
        unitManager = FindFirstObjectByType<UnitManager>();

        tileManager = FindFirstObjectByType<TileManager>();

        economyManager = FindFirstObjectByType<EconomyManager>();

        spawnManager = FindFirstObjectByType<SpawnManager>();

        factionManager = FindFirstObjectByType<FactionManager>();

        diplomacyManager = FindFirstObjectByType<DiplomacyManager>();
    }

    public void ProcessAI(float delta)
    {
        if (!enableAI) return;

        if (unitManager == null || tileManager == null) return;

        warFactions.Clear();

        HumanData[] humans = unitManager.Humans;

        int[] unitCounts = new int[factionManager.FactionCount];
        
        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive) continue;
            unitCounts[humans[i].factionId]++;
        }

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive || factionManager.IsPlayerFaction(humans[i].factionId)) continue;
            
            if (humans[i].hasTarget || humans[i].isExhausted) continue;

            int faction = humans[i].factionId;

            Vector2Int? targetTile = null;
            
            warFactions.Clear();

            if (diplomacyManager != null)
            {
                for (int other = 0; other < factionManager.FactionCount; other++)
                {
                    if (other == faction) continue;
                    if (diplomacyManager.GetRelations(faction, other).atWar) warFactions.Add(other);
                }
            }
            
            if (warFactions.Count > 0)
            {
                foreach (int wf in warFactions)
                {
                    targetTile = tileManager.FindNearestEnemyTile(humans[i].position, humans[i].factionId, 5f, wf);

                    if (targetTile.HasValue) break;
                }
                
                if (!targetTile.HasValue)
                {
                    foreach (int wf in warFactions)
                    {
                        targetTile = tileManager.FindNearestEnemyTile(humans[i].position, humans[i].factionId, 15f, wf);

                        if (targetTile.HasValue) break;
                    }
                }
            }
            
            if (!targetTile.HasValue)
            {
                targetTile = tileManager.FindNearestEnemyTile(humans[i].position, humans[i].factionId, 50f, -1);
            }
            
            if (!targetTile.HasValue)
            {
                float bestDist = float.MaxValue;

                for (int y = 0; y < tileManager.height; y++)
                {
                    for (int x = 0; x < tileManager.width; x++)
                    {
                        if (tileManager.GetTile(x, y).factionId == faction && tileManager.IsPassable(x, y))
                        {
                            float dist = Vector2.Distance(humans[i].position, new Vector2(x * tileManager.tileSize + tileManager.tileSize / 2f, y * tileManager.tileSize + tileManager.tileSize / 2f));

                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                targetTile = new Vector2Int(x, y);
                            }
                        }
                    }
                }
            }
            
            if (targetTile.HasValue)
            {
                Vector2 targetWorld = new Vector2(targetTile.Value.x * tileManager.tileSize + tileManager.tileSize / 2f, targetTile.Value.y * tileManager.tileSize + tileManager.tileSize / 2f);

                unitManager.MoveUnit(i, targetWorld, true);
            }
        }

        if (economyManager != null && spawnManager != null && factionManager != null)
        {
            for (int fid = 0; fid < factionManager.FactionCount; fid++)
            {
                if (factionManager.IsPlayerFaction(fid)) continue;
        
                int currentUnitCount = 0;

                for (int i = 0; i < humans.Length; i++)
                {
                    if (humans[i].isAlive && humans[i].factionId == fid) currentUnitCount++;
                }
        
                if (currentUnitCount < Mathf.Min(economyManager.GetFood(fid) / 500, 150))
                {
                    List<Vector2Int> factionTiles = new List<Vector2Int>();
                    
                    for (int y = 0; y < tileManager.height; y++)
                    {
                        for (int x = 0; x < tileManager.width; x++)
                        {
                            if (tileManager.GetTile(x, y).factionId == fid && tileManager.IsPassable(x, y)) factionTiles.Add(new Vector2Int(x, y));
                        }
                    }
        
                    if (factionTiles.Count > 0)
                    {
                        Vector2Int randomTile = factionTiles[Random.Range(0, factionTiles.Count)];

                        Vector2 worldPos = new Vector2(randomTile.x * tileManager.tileSize + tileManager.tileSize / 2f, randomTile.y * tileManager.tileSize + tileManager.tileSize / 2f);

                        spawnManager.SpawnUnitWithCost(worldPos, fid, 0f);
                    }
                }
            }
        }
        
        if (diplomacyManager != null && factionManager != null && economyManager != null)
        {
            for (int myFaction = 0; myFaction < factionManager.FactionCount; myFaction++)
            {
                if (factionManager.IsPlayerFaction(myFaction)) continue;

                for (int otherFaction = 0; otherFaction < factionManager.FactionCount; otherFaction++)
                {
                    if (otherFaction == myFaction) continue;

                    var rel = diplomacyManager.GetRelations(myFaction, otherFaction);

                    int myUnits = unitCounts[myFaction]; int otherUnits = unitCounts[otherFaction];

                    float myFood = economyManager.GetFood(myFaction);

                    float myLoyalty = 50f;

                    if (rel.atWar)
                    {
                        if (diplomacyManager.EvaluateAction(myFaction, otherFaction, "offerPeace", myUnits, otherUnits, myFood, myLoyalty)) diplomacyManager.MakePeace(myFaction, otherFaction);
                    }
                    else
                    {
                        if (diplomacyManager.EvaluateAction(myFaction, otherFaction, "declareWar", myUnits, otherUnits, myFood, myLoyalty)) diplomacyManager.DeclareWar(myFaction, otherFaction);
                    }
                }
            }
        }
    }
}