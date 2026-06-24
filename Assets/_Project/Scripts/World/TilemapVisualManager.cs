using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private Tilemap baseTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase sandTile;
    [SerializeField] private TileBase stoneTile;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase regionTile;
    [SerializeField] private TileBase borderTile;

    [Header("Regions")]
    [SerializeField] private float regionThreshold = 35f;

    private RegionManager regionManager;
    private FactionManager factionManager;

    public void Initialize(TileManager manager)
    {
        tileManager = manager;

        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();

        if (baseTilemap == null) baseTilemap = GetComponent<Tilemap>();

        SimulationManager sm = FindFirstObjectByType<SimulationManager>();

        regionManager = sm?.RegionManager; 

        factionManager = FindFirstObjectByType<FactionManager>();

        baseTilemap.ClearAllTiles();

        RedrawAllTiles();
    }

    void Update()
    {
        RedrawVisibleTiles();
    }

    public void RedrawAllTiles()
    {
        if (tileManager == null) return;

        for (int y = 0; y < tileManager.height; y++)
        {
            for (int x = 0; x < tileManager.width; x++)
            {
                SetTileAt(x, y);
            }
        }
    }

    public void RedrawVisibleTiles()
    {
        if (tileManager == null) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 camPos = cam.transform.position;

        int minX = Mathf.Max(0, Mathf.FloorToInt((camPos.x - halfW) / tileManager.tileSize));
        int maxX = Mathf.Min(tileManager.width - 1, Mathf.CeilToInt((camPos.x + halfW) / tileManager.tileSize));

        int minY = Mathf.Max(0, Mathf.FloorToInt((camPos.y - halfH) / tileManager.tileSize));
        int maxY = Mathf.Min(tileManager.height - 1, Mathf.CeilToInt((camPos.y + halfH) / tileManager.tileSize));
        
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                SetTileAt(x, y);
            }
        }
    }

    private void SetTileAt(int x, int y)
    {
        TileData tile = tileManager.GetTile(x, y);

        Vector3Int tilePos = new Vector3Int(x, y, 0);

        TileBase tileBase;

        Color color;

        bool isRegionMode = cam.orthographicSize >= regionThreshold && regionManager != null;

        if (isRegionMode && tileManager.IsWorldPassable(new Vector2(tilePos.x, tilePos.y)))
        {
            RegionData? region = regionManager.GetRegion(x, y, tileManager);

            int fid = region?.factionId ?? -1;

            bool isBorder = false;

            if (region.HasValue)
            {
                int currentId = region.Value.id;

                if (x > 0 && tileManager.GetTile(x-1, y).regionId != currentId) isBorder = true;
                else if (x < tileManager.width-1 && tileManager.GetTile(x+1, y).regionId != currentId) isBorder = true;
                else if (y > 0 && tileManager.GetTile(x, y-1).regionId != currentId) isBorder = true;
                else if (y < tileManager.height-1 && tileManager.GetTile(x, y+1).regionId != currentId) isBorder = true;

                Color factionColor = factionManager?.GetFaction(fid)?.factionColor ?? Color.gray;

                color = factionColor;
            }
            else
            {
                color = Color.gray;
            }

            tileBase = isBorder ? borderTile : regionTile;
        }
        else
        {
            switch (tile.tileType)
            {
                case TileType.Grass: tileBase = grassTile; break;
                case TileType.Sand:  tileBase = sandTile;  break;
                case TileType.Stone: tileBase = stoneTile; break;
                case TileType.Water: tileBase = waterTile; break;
                default: tileBase = grassTile; break;
            }

            color = Color.white;
        }

        baseTilemap.SetTile(tilePos, tileBase);
        baseTilemap.SetColor(tilePos, color);
    }
}
