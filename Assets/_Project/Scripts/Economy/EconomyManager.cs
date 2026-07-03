using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    private int maxFactions;

    private float[] factionFood;

    private TileManager tileManager;
    private UnitManager unitManager;
    private FactionManager factionManager;

    void Awake()
    {
        GameManager.RegisterEconomyManager(this);
    }

    public void Initialize()
    {
        tileManager = GameManager.TileManager;
        
        unitManager = GameManager.UnitManager;

        factionManager = GameManager.FactionManager;

        maxFactions = factionManager.FactionCount;

        factionFood = new float[maxFactions];

        for (int i = 0; i < maxFactions; i++)
        {
            FactionData data = factionManager.GetFaction(i);

            factionFood[i] = data != null ? data.startingFood : 250f;
        }
    }

    public void ProcessEconomy(float delta)
    {
        if (tileManager == null || unitManager == null) return;
        
        TileData[] tiles = tileManager.Tiles;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].factionId >= 0 && tiles[i].factionId < maxFactions) factionFood[tiles[i].factionId] += tiles[i].foodAmount * delta;
        }
        
        HumanData[] humans = unitManager.Humans;

        float upkeepPerUnit = 15f;

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive) continue;

            int fid = humans[i].factionId;

            if (fid < 0 || fid >= maxFactions) continue;

            factionFood[fid] -= upkeepPerUnit * delta;
        }
        
        for (int f = 0; f < maxFactions; f++)
        {
            if (factionFood[f] < 0)
            {
                for (int i = 0; i < humans.Length; i++)
                {
                    if (!humans[i].isAlive || humans[i].factionId != f) continue;

                    humans[i].hp -= 5f * delta;
                }
                factionFood[f] = 0;
            }
        }
    }

    public float GetFood(int factionId)
    {
        if (factionId < 0 || factionId >= maxFactions) return 0;

        return factionFood[factionId];
    }
    
    public void SpendFood(int factionId, float amount)
    {
        if (factionId >= 0 && factionId < factionFood.Length) factionFood[factionId] -= amount;
    }
}