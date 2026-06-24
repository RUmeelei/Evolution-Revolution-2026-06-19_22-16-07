using UnityEngine;
using System.Collections.Generic;

public class RegionManager
{
    private List<RegionData> regions;

    private Dictionary<int, RegionData> regionsById;

    private int maxFactions;

    public IReadOnlyList<RegionData> AllRegions => regions;

    public void Initialize(int maxFactionsCount)
    {
        maxFactions = maxFactionsCount;
        regions = new List<RegionData>();
        regionsById = new Dictionary<int, RegionData>();
    }

    public void UpdateRegions(TileManager manager)
    {
        for (int i = 0; i < manager.Tiles.Length; i++)
        {
            manager.Tiles[i].regionId = -1;
        }

        regions.Clear();
        regionsById.Clear();

        int nextRegionId = 0;

        for (int y = 0; y < manager.height; y++)
        {
            for (int x = 0; x < manager.width; x++)
            {
                TileData tile = manager.GetTile(x, y);

                if (tile.factionId == -1 || tile.regionId != -1) continue;

                RegionData region = new RegionData();

                region.id = nextRegionId;
                region.factionId = tile.factionId;
                region.tiles = new List<Vector2Int>();

                FloodFill(manager, x, y, tile.factionId, nextRegionId, region.tiles);

                regions.Add(region);
                regionsById[nextRegionId] = region;

                nextRegionId++;
            }
        }

        for (int i = 0; i < regions.Count; i++)
        {
            RegionData reg = regions[i];

            bool constested = false;

            foreach (Vector2Int tilePos in reg.tiles)
            {
                TileData tile = manager.GetTile(tilePos.x, tilePos.y);

                if (tile.unitsByFaction != null)
                {
                    for (int f = 0; f < tile.unitsByFaction.Length; f++)
                    {
                        if (f != reg.factionId && tile.unitsByFaction[f] > 0)
                        {
                            constested = true;
                            break;
                        }
                    }
                }
                if (constested) break;
            }

            reg.isConstested = constested;
            regions[i] = reg;
            regionsById[reg.id] = reg;
        }
    }

    private void FloodFill(TileManager manager, int startX, int startY, int factionId, int regionId, List<Vector2Int> tilesList)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            int x = current.x;
            int y = current.y;

            if (x < 0 || x >= manager.width || y < 0 || y >= manager.height) continue;

            int index = y * manager.width + x;

            TileData tile = manager.Tiles[index];

            if (tile.factionId == factionId && tile.regionId == -1)
            {
                manager.Tiles[index].regionId = regionId;

                tilesList.Add(new Vector2Int(x, y));

                for (int dy = -1; dy <= 1 ; dy++)
                {
                    for (int dx = -1; dx <= 1 ; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        queue.Enqueue(new Vector2Int(x + dx, y + dy));
                    }
                }
            }
        }
    }

    public RegionData? GetRegion(int tileX, int tileY, TileManager tileManager)
    {
        int regionId = tileManager.GetTile(tileX, tileY).regionId;

        if (regionId == -1) return null;

        regionsById.TryGetValue(regionId, out RegionData region);
        
        return region;
    }
}