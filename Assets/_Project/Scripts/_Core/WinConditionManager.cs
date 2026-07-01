using System;
using UnityEngine;
using System.Collections.Generic;

public class WinConditionManager : MonoBehaviour
{
    private RegionManager regionManager;

    private bool gameEnded = false;

    public event Action<int> OnVictory; // с параметром int

    private List<TileData> waterTiles = new List<TileData>();

    public void Initialize()
    {
        SimulationManager sm = FindFirstObjectByType<SimulationManager>();

        regionManager = sm?.RegionManager;
    }

    public void CheckVictory()
    {
        if (gameEnded) return;

        TileManager tm = FindFirstObjectByType<TileManager>();

        if (tm == null) return;

        int totalTiles = tm.width * tm.height;

        Dictionary<int, int> factionTileCount = new Dictionary<int, int>();

        waterTiles.Clear();

        for (int i = 0; i < totalTiles; i++)
        {
            if (tm.Tiles[i].tileType == TileType.Water) waterTiles.Add(tm.Tiles[i]);

            int fid = tm.Tiles[i].factionId;

            if (!factionTileCount.ContainsKey(fid)) factionTileCount[fid] = 0; factionTileCount[fid]++;
        }

        for (int i = 0; i < waterTiles.Count; i++)
        {
            totalTiles--;
        }

        int maxFaction = -1;
        int maxTiles = 0;

        foreach (var kvp in factionTileCount)
        {
            if (kvp.Key == -1) continue;

            if (kvp.Value > maxTiles)
            {
                maxTiles = kvp.Value;
                maxFaction = kvp.Key;
            }
        }

        if (maxTiles >= totalTiles * 0.9f)
        {
            gameEnded = true;

            FactionManager fm = FindFirstObjectByType<FactionManager>();

            string factionName = fm?.GetFaction(maxFaction)?.factionName ?? maxFaction.ToString();

            SimulationManager sm = FindFirstObjectByType<SimulationManager>();

            sm?.StopSimulation();

            OnVictory?.Invoke(maxFaction);
        }
    }
}