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

    public bool BuildBuilding(Vector2Int tilePos, BuildingType type, int factionId, bool isGovernment = false, float woodcost = 0f, float stonecost = 0f, float goldcost = 0f)
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

        if (!isGovernment) {woodcost /= 2; stonecost /= 2; goldcost /= 2;}
        else
        {
            if (type == BuildingType.Farm) woodcost = 50f;
            else if (type == BuildingType.Wall) stonecost = 50f;
            else if (type == BuildingType.Temple) {woodcost = 50f; stonecost = 100f;}
            else if (type == BuildingType.Market) {woodcost = 150f; stonecost = 100f; goldcost = 50f;}
        }

        if (economyManager.GetWood(factionId) < woodcost || economyManager.GetStone(factionId) < stonecost || economyManager.GetGold(factionId) < goldcost)
        {
            return false;
        }
        
        economyManager.SpendWood(factionId, woodcost);
        economyManager.SpendStone(factionId, stonecost);
        economyManager.SpendGold(factionId, goldcost);
        
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