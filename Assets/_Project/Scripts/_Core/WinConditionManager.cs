using System;
using UnityEngine;
using System.Collections.Generic;

public class WinConditionManager : MonoBehaviour
{
    private RegionManager regionManager;

    private bool gameEnded = false;

    public event Action<int> OnVictory; // с параметром int

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

        for (int i = 0; i < totalTiles; i++)
        {
            int fid = tm.Tiles[i].factionId;

            if (!factionTileCount.ContainsKey(fid)) factionTileCount[fid] = 0;

            factionTileCount[fid]++;
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

            Debug.Log($"ФРАКЦИЯ {factionName} ПОБЕДИЛА! Контролирует {maxTiles} из {totalTiles} тайлов.");

            SimulationManager sm = FindFirstObjectByType<SimulationManager>();

            sm?.StopSimulation();

            OnVictory?.Invoke(maxFaction);
        }
    }
}