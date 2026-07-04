using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private UnitManager unitManager;
    private EconomyManager economyManager;
    private TileManager tileManager;
    private FactionManager factionManager;

    void Awake()
    {
        GameManager.RegisterSpawnManager(this);
    }

    public void Initialize()
    {
        unitManager = GameManager.UnitManager;

        economyManager = GameManager.EconomyManager;

        tileManager = GameManager.TileManager;

        factionManager = GameManager.FactionManager;
    }

    void Update()
    {
        //  if (Input.GetKeyDown(KeyCode.N))
        //  {
        //      Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
          
        //      worldPos.z = 0;
          
        //      int playerFaction = -1;
          
        //      for (int i = 0; i < factionManager.FactionCount; i++)
        //      {
        //          if (factionManager.IsPlayerFaction(i)) playerFaction = i;
        //      }
          
        //      if (tileManager != null && tileManager.IsWorldPassable(worldPos) && playerFaction >= 0) SpawnUnitWithCost(worldPos, playerFaction, 50f);
        //  }
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

                SpawnUnitWithCost(pos, fid);
            }
        }
    }

    public void MobilizeUnit(Vector2 pos, int faction)
    {
        SpawnUnitWithCost(pos, faction, foodCost : 50f, regular : false);
    }

    public void RecruitUnit(Vector2 pos, int faction)
    {
        SpawnUnitWithCost(pos, faction, foodCost : 25f, goldCost : 10f, regular : true);
    }

    public bool SpawnUnitWithCost(Vector2 pos, int faction, float foodCost = 0f, float goldCost = 0f, float stoneCost = 0f, float woodCost = 0f, bool regular = false)
    {
        if (economyManager == null || unitManager == null) return false;

        if (economyManager.GetFood(faction) < foodCost) return false;
        if (economyManager.GetGold(faction) < goldCost) return false;
        if (economyManager.GetStone(faction) < stoneCost) return false;
        if (economyManager.GetWood(faction) < woodCost) return false;

        economyManager.SpendFood(faction, foodCost);
        economyManager.SpendGold(faction, goldCost);
        economyManager.SpendStone(faction, stoneCost);
        economyManager.SpendWood(faction, woodCost);

        float mhp = regular ? 150f : 100f;
        float mstamina = regular ? 175f : 100f;
        float adamage = regular ? 15f : 10f;
        float acooldown = regular ? 0.5f : 1f;

        Specialization spec = regular ? Specialization.Infantry : Specialization.None;
        Profession prof = regular ? Profession.Soldier : Profession.Farmer;

        unitManager.CreateHuman(pos, faction, maxhp : mhp, maxstamina : mstamina, attackdamage : adamage, attackcooldown : acooldown, profession : prof, specialization : spec);

        return true;
    }
}