using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private UnitManager unitManager;
    private TileManager tileManager;
    private EconomyManager economyManager;
    private SpawnManager spawnManager;
    private FactionManager factionManager;

    public void Initialize()
    {
        unitManager = FindFirstObjectByType<UnitManager>();

        tileManager = FindFirstObjectByType<TileManager>();

        economyManager = FindFirstObjectByType<EconomyManager>();

        spawnManager = FindFirstObjectByType<SpawnManager>();

        factionManager = FindFirstObjectByType<FactionManager>();
    }

    public void ProcessAI(float delta)
    {
        if (unitManager == null || tileManager == null) return;

        HumanData[] humans = unitManager.Humans;

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive || factionManager.IsPlayerFaction(humans[i].factionId)) continue;

            if (humans[i].hasTarget) continue;

            Vector2Int? targetTile = tileManager.FindNearestEnemyTile(humans[i].position, humans[i].factionId, 50f, -1);

            if (!targetTile.HasValue) targetTile = tileManager.FindNearestEnemyTile(humans[i].position, humans[i].factionId, 50f);

            if (targetTile.HasValue)
            {
                Vector2 targetWorld = new Vector2(targetTile.Value.x * tileManager.tileSize + tileManager.tileSize / 2f, targetTile.Value.y * tileManager.tileSize + tileManager.tileSize / 2f);
                
                unitManager.MoveUnit(i, targetWorld);
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
        
                if (currentUnitCount < economyManager.GetFood(fid) / 150)
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
    }
}