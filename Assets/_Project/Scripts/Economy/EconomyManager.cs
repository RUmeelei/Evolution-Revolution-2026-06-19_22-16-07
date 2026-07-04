using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    private int maxFactions;

    private float[] factionFood;
    private float[] factionGold;
    private float[] factionStone;
    private float[] factionWood;

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
        factionGold = new float[maxFactions];
        factionStone = new float[maxFactions];
        factionWood = new float[maxFactions];

        for (int i = 0; i < maxFactions; i++)
        {
            FactionData data = factionManager.GetFaction(i);

            factionFood[i] = data != null ? data.startingFood : 250f;
            factionGold[i] = data != null ? data.startingGold : 50f;
            factionStone[i] = data != null ? data.startingStone : 150f;
            factionWood[i] = data != null ? data.startingWood : 200f;
        }
    }

    public void ProcessEconomy(float delta)
    {
        if (tileManager == null || unitManager == null) return;
        
        TileData[] tiles = tileManager.Tiles;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].factionId >= 0 && tiles[i].factionId < maxFactions)
            {
                factionFood[tiles[i].factionId] += tiles[i].foodAmount * delta;
                factionGold[tiles[i].factionId] += tiles[i].goldAmount * delta;
                factionStone[tiles[i].factionId] += tiles[i].stoneAmount * delta;
                factionWood[tiles[i].factionId] += tiles[i].woodAmount * delta;
            } 
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

    public float GetGold(int factionId)
    {
        if (factionId < 0 || factionId >= maxFactions) return 0;

        return factionGold[factionId];
    }

    public void SpendGold(int factionId, float amount)
    {
        if (factionId >= 0 && factionId < factionGold.Length) factionGold[factionId] -= amount;
    }

    public float GetStone(int factionId)
    {
        if (factionId < 0 || factionId >= maxFactions) return 0;

        return factionStone[factionId];
    }

    public void SpendStone(int factionId, float amount)
    {
        if (factionId >= 0 && factionId < factionStone.Length) factionStone[factionId] -= amount;
    }

    public float GetWood(int factionId)
    {
        if (factionId < 0 || factionId >= maxFactions) return 0;

        return factionWood[factionId];
    }

    public void SpendWood(int factionId, float amount)
    {
        if (factionId >= 0 && factionId < factionWood.Length) factionWood[factionId] -= amount;
    }
}