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

    [Header("Map Modes")]
    [SerializeField] private int mapMode = 0;

    [Header("Buildings")]
    [SerializeField] private Tilemap buildingsTilemap;
    [SerializeField] private TileBase farmTile;
    [SerializeField] private TileBase marketTile;
    [SerializeField] private TileBase templeTile;
    [SerializeField] private TileBase wallTile;

    private RegionManager regionManager;
    private FactionManager factionManager;
    private SelectionManager selectionManager;
    private DiplomacyManager diplomacyManager;

    void Awake()
    {
        GameManager.RegisterTilemapVisualManager(this);
    }

    public void Initialize(TileManager manager)
    {
        tileManager = manager;

        if (tileManager == null) tileManager = GameManager.TileManager;

        if (baseTilemap == null) baseTilemap = GetComponent<Tilemap>();

        SimulationManager sm = GameManager.SimulationManager;

        selectionManager = FindFirstObjectByType<SelectionManager>();

        diplomacyManager = GameManager.DiplomacyManager;

        regionManager = sm?.RegionManager; 

        factionManager = GameManager.FactionManager;

        baseTilemap.ClearAllTiles();

        RedrawAllTiles();
        
        RedrawAllBuildings();
    }

    void Update()
    {
        RedrawVisibleTiles();

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (mapMode < 2)
            {
                mapMode++;
            }
            else mapMode = 0;
        }
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

        TileBase tileBase = grassTile;

        Color color = Color.gray;

        bool isRegionMode = cam.orthographicSize >= regionThreshold && regionManager != null || mapMode == 1;

        bool isDiplomaticMode = mapMode == 2;

        if ((isRegionMode || isDiplomaticMode && (selectionManager == null || selectionManager.GetLastClickedTile() == null || tileManager.GetTile(selectionManager.GetLastClickedTile().Value.x, selectionManager.GetLastClickedTile().Value.y).factionId == -1)) && tileManager.IsWorldPassable(new Vector2(tilePos.x, tilePos.y)) && tile.tileType != TileType.Water)
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
        else if (isDiplomaticMode && selectionManager != null && tile.tileType != TileType.Water)
        {
            Vector2Int? selectedTile = selectionManager.GetLastClickedTile();

            int selectedId = tileManager.GetTile(selectedTile.Value.x, selectedTile.Value.y).factionId;

            bool isBorder = false;

            int currentId = tile.regionId;

            if (x > 0 && tileManager.GetTile(x-1, y).regionId != currentId) isBorder = true;
            else if (x < tileManager.width-1 && tileManager.GetTile(x+1, y).regionId != currentId) isBorder = true;
            else if (y > 0 && tileManager.GetTile(x, y-1).regionId != currentId) isBorder = true;
            else if (y < tileManager.height-1 && tileManager.GetTile(x, y+1).regionId != currentId) isBorder = true;

            if (tile.factionId == selectedId)
            {
                color = Color.cyan;
            }
            else if (tile.factionId != -1)
            {
                for (int f = 0; f < factionManager.FactionCount; f++)
                {
                    if (f == selectedId) continue;
                    
                    if (tile.factionId != f) continue;

                    bool atWar = diplomacyManager.GetRelations(selectedId, f).atWar;

                    color = atWar ? Color.softRed : Color.green;

                    break;
                }
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

        bool isSelected = selectionManager != null && selectionManager.GetLastClickedTile().HasValue && selectionManager.GetLastClickedTile().Value.x == x && selectionManager.GetLastClickedTile().Value.y == y;

        if (isSelected)
        {
            color = Color.yellow;
        }

        baseTilemap.SetTile(tilePos, tileBase);
        baseTilemap.SetColor(tilePos, color);
    }

    public void UpdateBuildingTile(int x, int y, TileData tile)
    {
        if (buildingsTilemap == null) return;

        Vector3Int pos = new Vector3Int(x, y, 0);
        
        buildingsTilemap.SetTile(pos, null);

        if (tile.buildings == null || tile.buildings.Count == 0) return;
        
        TileBase icon = tile.buildings[0].type switch {BuildingType.Farm => farmTile, BuildingType.Market => marketTile, BuildingType.Temple => templeTile, BuildingType.Wall => wallTile, _ => null};

        if (icon != null)
        {
            buildingsTilemap.SetTile(pos, icon);
            buildingsTilemap.SetColor(pos, Color.white);
        }
    }

    public void RedrawAllBuildings()
    {
        if (tileManager == null || buildingsTilemap == null) return;
        
        for (int y = 0; y < tileManager.height; y++)
        {
            for (int x = 0; x < tileManager.width; x++)
            {
                UpdateBuildingTile(x, y, tileManager.GetTile(x, y));
            }
        }
    }
}
