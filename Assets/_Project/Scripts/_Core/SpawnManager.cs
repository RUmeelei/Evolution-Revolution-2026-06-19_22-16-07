using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private UnitManager unitManager;
    private EconomyManager economyManager;
    private TileManager tileManager;
    private FactionManager factionManager;

    public void Initialize()
    {
        unitManager = FindFirstObjectByType<UnitManager>();

        economyManager = FindFirstObjectByType<EconomyManager>();

        tileManager = FindFirstObjectByType<TileManager>();

        factionManager = FindFirstObjectByType<FactionManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            worldPos.z = 0;

            int playerFaction = -1;

            for (int i = 0; i < factionManager.FactionCount; i++)
            {
                if (factionManager.GetFaction(i).isPlayer) playerFaction = i;
            }

            if (tileManager != null && tileManager.IsWorldPassable(worldPos) && playerFaction >= 0) SpawnUnitWithCost(worldPos, playerFaction, 50f);
        }
    }

    public void SpawnStartingUnits()
    {
        if (unitManager == null || economyManager == null || factionManager == null) return;
        
        for (int fid = 0; fid < factionManager.FactionCount; fid++)
        {
            FactionData data = factionManager.GetFaction(fid);
            
            if (data == null) continue;
            
            Vector2 center;

            if (fid == 0) center = new Vector2(5f, 10f); else center = new Vector2(15f, 10f);

            for (int i = 0; i < data.startingUnits; i++)
            {
                Vector2 pos = center + new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));

                SpawnUnitWithCost(pos, fid, 0f);
            }
        }
    }

    public bool SpawnUnitWithCost(Vector2 pos, int faction, float cost = 50f)
    {
        if (economyManager == null || unitManager == null) return false;

        if (economyManager.GetFood(faction) < cost) return false;

        economyManager.SpendFood(faction, cost);

        unitManager.SpawnHuman(pos, faction);

        return true;
    }
}