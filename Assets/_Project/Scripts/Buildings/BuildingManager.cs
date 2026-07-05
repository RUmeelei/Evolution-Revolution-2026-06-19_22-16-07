using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private TileManager tileManager;
    private EconomyManager economyManager;
    private TilemapVisualManager tilemapVisualManager;

    void Awake()
    {
        GameManager.RegisterBuildingManager(this);
    }

    void Start()
    {
        tileManager = GameManager.TileManager;

        economyManager = GameManager.EconomyManager;

        tilemapVisualManager = GameManager.TilemapVisualManager;
    }

    public bool BuildBuilding(Vector2Int tilePos, BuildingType type, int factionId)
    {
        if (tileManager == null || economyManager == null)
        {
            return false;
        }

        TileData tile = tileManager.GetTile(tilePos.x, tilePos.y);
        
        if (tile.factionId != factionId)
        {
            return false;
        }
        
        foreach (var building in tile.buildings)
        {
            if (building.type == type)
            {
                return false;
            }
        }
        
        int woodCost = 50;
        int stoneCost = 30;
        int goldCost = 20;

        if (economyManager.GetWood(factionId) < woodCost || economyManager.GetStone(factionId) < stoneCost || economyManager.GetGold(factionId) < goldCost)
        {
            return false;
        }
        
        economyManager.SpendWood(factionId, woodCost);
        economyManager.SpendStone(factionId, stoneCost);
        economyManager.SpendGold(factionId, goldCost);
        
        tile.buildings.Add(new BuildingData
        {
            type = type,
            level = 1,
            factionId = factionId
        });

        tileManager.AddBuildingCount(factionId, type);
        
        if (tilemapVisualManager != null)
        {
            tilemapVisualManager.UpdateBuildingTile(tilePos.x, tilePos.y, tile);
        }

        return true;
    }
}