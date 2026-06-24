using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    public int width = 100;
    public int height = 100;

    public float tileSize = 10f;

    private TileData[] tiles;

    public TileData[] Tiles => tiles;

    private int maxFactions;

    private HashSet<int> activeTileIndices = new HashSet<int>();

    private FactionManager factionManager;
    private UnitManager unitManager;

    public void Initialize()
    {
        tiles = new TileData[width * height];

        factionManager = FindFirstObjectByType<FactionManager>();

        unitManager = FindFirstObjectByType<UnitManager>();

        maxFactions = factionManager.FactionCount;

        Debug.Log($"Initialized TileManager with capacity for {width * height} tiles.");

        GenerateWorld();
    }

    public void GenerateWorld(int seed = -1, float scale = 0.05f)
    {
        if (seed == -1) seed = Random.Range(0, 100000);

        float offsetX = seed * 0.1f;
        float offsetY = seed * 0.2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;

                float sample = Mathf.PerlinNoise(x * scale + offsetX, y *  scale + offsetY);

                if (sample < 0.3f)
                {
                    tiles[i].tileType = TileType.Water;
                }            
                else if (sample < 0.4f)
                {
                    tiles[i].tileType = TileType.Sand;
                }      
                else if (sample < 0.8f)
                {
                    tiles[i].tileType = TileType.Grass;
                }
                else
                {
                    tiles[i].tileType = TileType.Stone;
                }

                switch (tiles[i].tileType)
                {
                    case TileType.Grass: tiles[i].foodAmount = 1f; break;
                    case TileType.Sand: tiles[i].foodAmount = 0.2f; break;
                    case TileType.Stone: tiles[i].foodAmount = 0f; break;
                    case TileType.Water: tiles[i].foodAmount = 0f; break;
                }
                
                tiles[i].factionId = -1;

                for (int f = 0; f < factionManager.FactionCount; f++)
                {
                    FactionData fd = factionManager.GetFaction(f);

                    if (fd == null) continue;
                    
                    if (f % 2 == 0 && x < width / 3 && y < height / 3 && IsPassable(x, y)) tiles[i].factionId = f; else if (f % 2 == 1 && x > 2 * width / 3 && y > 2 * height / 3 && IsPassable(x, y)) tiles[i].factionId = f;
                }

                tiles[i].unitsByFaction = new int[maxFactions];
            }
        }

        UpdateTileOwnership();
    }

    public Vector2Int? FindNearestEnemyTile(Vector2 fromPosition, int selfFaction, float searchRadius, int targetFaction = -2)
    {
        bool anyTarget = targetFaction == -2 ? true : false;

        Vector2Int fromTile = WorldToTile(fromPosition);
        Vector2Int? nearest = null;

        float nearestDist = searchRadius;

        int tileRadius = Mathf.CeilToInt(searchRadius / tileSize);
        int minX = Mathf.Max(0, fromTile.x - tileRadius);
        int maxX = Mathf.Min(width - 1, fromTile.x + tileRadius);
        int minY = Mathf.Max(0, fromTile.y - tileRadius);
        int maxY = Mathf.Min(height - 1, fromTile.y + tileRadius);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                TileData tile = GetTile(x, y);

                if ((!anyTarget && tile.factionId == targetFaction || anyTarget && tile.factionId != selfFaction) && IsPassable(x, y))
                {
                    float dist = Vector2.Distance(fromPosition, new Vector2(x * tileSize + tileSize / 2f, y * tileSize + tileSize / 2f));

                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = new Vector2Int(x, y);
                    }
                }
            }
        }

        return nearest;
    }

    public void UpdateTileOwnership()
    {
        if (unitManager == null) return;

        HumanData[] humans = unitManager.Humans;

        if (humans == null) return;
        
        foreach (int idx in activeTileIndices)
        {
            if (idx >= 0 && idx < tiles.Length && tiles[idx].unitsByFaction != null) System.Array.Clear(tiles[idx].unitsByFaction, 0, tiles[idx].unitsByFaction.Length);
        }

        activeTileIndices.Clear();

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive) continue;

            Vector2Int tilePos = WorldToTile(humans[i].position);

            int index = tilePos.y * width + tilePos.x;

            if (index < 0 || index >= tiles.Length || !IsPassable(tilePos.x, tilePos.y)) continue;

            int factionId = humans[i].factionId;

            if (tiles[index].unitsByFaction != null && factionId >= 0 && factionId < tiles[index].unitsByFaction.Length)
            {
                tiles[index].unitsByFaction[factionId]++;
                
                activeTileIndices.Add(index);
            }
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].unitsByFaction == null) continue;

            int dominantFaction = -1;
            int maxUnits = 0;

            bool constested = false;

            for (int f = 0; f < tiles[i].unitsByFaction.Length; f++)
            {
                int count = tiles[i].unitsByFaction[f];

                if (count > maxUnits)
                {
                    maxUnits = count;
                    dominantFaction = f;
                    constested = false;
                }
                else if (count == maxUnits && count > 0)
                {
                    constested = true;
                }
            }

            if (dominantFaction != -1 && !constested)
            {
                tiles[i].factionId = dominantFaction;
            }
        }
    }

    public Vector2Int WorldToTile(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.y / tileSize);

        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);

        return new Vector2Int(x, y);
    }

    public TileData GetTile(int x, int y) => tiles[y * width + x];

    public void SetTile(int x, int y, TileData data) => tiles[y * width + x] = data;

    public bool IsPassable(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;

        TileData tile = GetTile(x, y);

        //return tile.tileType != TileType.Water;

        return true;
    }

    public bool IsWorldPassable(Vector2 worldPos)
    {
        if (worldPos.x < 0 || worldPos.x >= width * tileSize || worldPos.y < 0 || worldPos.y >= height * tileSize) return false;

        Vector2Int tile = WorldToTile(worldPos);
        return IsPassable(tile.x, tile.y);
    }

    public float CalculateMoveCost(int x, int y)
    {
        float moveCost = 1;

        TileData tile = GetTile(x, y);

        if (tile.tileType == TileType.Sand) moveCost += 1.5f;
        if (tile.tileType == TileType.Stone) moveCost += 2f;
        if (tile.tileType == TileType.Water) moveCost += 3f;

        return moveCost;
    }

    public float CalculateWorldMoveCost(Vector2 worldPos)
    {
        Vector2Int tile = WorldToTile(worldPos);

        return CalculateMoveCost(tile.x, tile.y);
    }
}
