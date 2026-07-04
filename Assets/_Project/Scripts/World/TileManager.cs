using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] public int width = 100;
    [SerializeField] public int height = 100;
    [SerializeField] public float tileSize = 10f;

    [Header("Noise Parameters")]
    [SerializeField] private float continentalFrequency = 1.0f;
    [SerializeField] private float mountainFrequency = 3.0f;
    [SerializeField] private float moistureFrequency = 3.0f;
    [SerializeField] private float detailFrequency = 10.0f;
    [SerializeField] private float waterThreshold = 0.4f;
    [SerializeField] private float mountainThreshold = 0.7f;

    public List<Vector2Int> factionCenters = new List<Vector2Int>();

    private TileData[] tiles;

    public TileData[] Tiles => tiles;

    private int[,] buildingCounts;

    private int maxFactions;

    private HashSet<int> activeTileIndices = new HashSet<int>();

    private FactionManager factionManager;
    private UnitManager unitManager;

    void Awake()
    {
        GameManager.RegisterTileManager(this);
    }

    public void Initialize()
    {
        tiles = new TileData[width * height];

        factionManager = GameManager.FactionManager;

        unitManager = GameManager.UnitManager;

        maxFactions = factionManager.FactionCount;

        GenerateWorld();

        buildingCounts = new int[factionManager.FactionCount, System.Enum.GetValues(typeof(BuildingType)).Length];
    }

    public void GenerateWorld(int seed = -1)
    {
        if (seed == -1) seed = Random.Range(0, 100000);

        Random.InitState(seed);
        
        float continentalOffsetX = Random.Range(0f, 1000f);
        float continentalOffsetY = Random.Range(0f, 1000f);
        float mountainOffsetX = Random.Range(0f, 1000f);
        float mountainOffsetY = Random.Range(0f, 1000f);
        float moistureOffsetX = Random.Range(0f, 1000f);
        float moistureOffsetY = Random.Range(0f, 1000f);
        float detailOffsetX = Random.Range(0f, 1000f);
        float detailOffsetY = Random.Range(0f, 1000f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                
                float continentNoise = Mathf.PerlinNoise((float)x / width * continentalFrequency + continentalOffsetX, (float)y / height * continentalFrequency + continentalOffsetY);

                bool isWater = continentNoise < waterThreshold;

                if (isWater)
                {
                    tiles[i].tileType = TileType.Water;
                    tiles[i].elevation = 0f;
                    tiles[i].foodAmount = 0f;
                }
                else
                {
                    float mountainNoise = Mathf.PerlinNoise((float)x / width * mountainFrequency + mountainOffsetX, (float)y / height * mountainFrequency + mountainOffsetY);

                    tiles[i].elevation = mountainNoise;
                    
                    float moistureNoise = Mathf.PerlinNoise((float)x / width * moistureFrequency + moistureOffsetX, (float)y / height * moistureFrequency + moistureOffsetY);
                    
                    float detailNoise = Mathf.PerlinNoise((float)x / width * detailFrequency + detailOffsetX, (float)y / height * detailFrequency + detailOffsetY);
                    
                    if (mountainNoise > mountainThreshold)
                    {
                        tiles[i].tileType = TileType.Stone;
                        tiles[i].foodAmount = 0f;
                        tiles[i].goldAmount = Random.Range(0, 2);
                        tiles[i].stoneAmount = Random.Range(1, 2);
                        tiles[i].woodAmount = 0f;
                    }
                    else if (moistureNoise < 0.3f + detailNoise * 0.1f)
                    {
                        tiles[i].tileType = TileType.Sand;
                        tiles[i].foodAmount = 0.2f;
                        tiles[i].goldAmount = Random.Range(0, 1);
                        tiles[i].stoneAmount = Random.Range(0, 1);
                        tiles[i].woodAmount = 0f;
                    }
                    else if (moistureNoise > 0.6f - detailNoise * 0.1f && detailNoise > 0.5f)
                    {
                        tiles[i].tileType = TileType.Grass;
                        tiles[i].foodAmount = 2f;
                        tiles[i].goldAmount = 0f;
                        tiles[i].stoneAmount = 0f;
                        tiles[i].woodAmount = Random.Range(0, 2);
                    }
                    else
                    {
                        tiles[i].tileType = TileType.Grass;
                        tiles[i].foodAmount = 1f;
                        tiles[i].goldAmount = 0f;
                        tiles[i].stoneAmount = 0f;
                        tiles[i].woodAmount = Random.Range(0, 2);
                    }
                }

                tiles[i].factionId = -1;

                //for (int f = 0; f < factionManager.FactionCount; f++)
                //{
                //    FactionData fd = factionManager.GetFaction(f);
                
                //    if (fd == null) continue;
                //
                //    int hexSize = 16;
                //    int startTileX = x;
                //    int startTileY = y;
                
                //    if (x >= startTileX && x < startTileX + hexSize && y >= startTileY && y < startTileY + hexSize && tiles[i].tileType != TileType.Water && tiles[i].factionId == -1)
                //    {
                //        tiles[i].factionId = f;
                
                //        break;
                //    }
                //}
                
                float riverNoise = Mathf.PerlinNoise((float)x / width * 0.15f + detailOffsetX, (float)y / height * 0.15f + detailOffsetY);

                if (!isWater && riverNoise > 0.49f && riverNoise < 0.5f)
                {
                    tiles[i].tileType = TileType.Water;
                    tiles[i].foodAmount = 0f;
                    tiles[i].goldAmount = 0f;
                    tiles[i].stoneAmount = 0f;
                    tiles[i].woodAmount = 0f;
                }

                tiles[i].unitsByFaction = new int[maxFactions];

                tiles[i].buildings = new List<BuildingData>();
            }
        }
        
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tiles[y * width + x].tileType != TileType.Water)
                {
                    availableTiles.Add(new Vector2Int(x, y));
                }
            }
        }
        
        for (int i = availableTiles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            (availableTiles[i], availableTiles[j]) = (availableTiles[j], availableTiles[i]);
        }
        
        factionCenters.Clear();

        for (int f = 0; f < factionManager.FactionCount; f++)
        {
            if (f >= availableTiles.Count) break;

            factionCenters.Add(availableTiles[f]);
        }
    }

    public void AddBuildingCount(int factionId, BuildingType type)
    {
        if (factionId >= 0 && factionId < buildingCounts.GetLength(0)) buildingCounts[factionId, (int)type]++;
    }

    public int GetBuildingCount(int factionId, BuildingType type)
    {
        if (factionId >= 0 && factionId < buildingCounts.GetLength(0)) return buildingCounts[factionId, (int)type];
        
        return 0;
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

                if ((!anyTarget && tile.factionId == targetFaction || anyTarget && tile.factionId != selfFaction) && IsPassable(x, y) && tile.tileType != TileType.Water)
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

            if (index < 0 || index >= tiles.Length || !IsPassable(tilePos.x, tilePos.y) || tiles[index].tileType == TileType.Water) continue;

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