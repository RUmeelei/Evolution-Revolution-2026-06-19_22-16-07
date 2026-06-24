using UnityEngine;

public class TileVisualManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer tilePrefab;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private int poolSize = 4000;
    [SerializeField] private float regionThreshold = 35;
    [SerializeField] private Color[] factionColors = new Color[] {Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan};

    [Header("Sprites")]
    [SerializeField] private Sprite grassTile;
    [SerializeField] private Sprite sandTile;
    [SerializeField] private Sprite stoneTile;
    [SerializeField] private Sprite waterTile;
    [SerializeField] private Sprite regionTile;
    [SerializeField] private Sprite borderTile;

    private SpriteRenderer[] pool;

    private SimulationManager simulationManager;
    private CameraManager cameraManager;
    private RegionManager regionManager;

    public void Initialize(TileManager manager)
    {
        tileManager = manager;

        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();

        if (cameraManager == null) cameraManager = FindFirstObjectByType<CameraManager>();

        SimulationManager sm = FindFirstObjectByType<SimulationManager>();
        regionManager = sm?.RegionManager;

        int visibleTilesX = Mathf.CeilToInt(2f * cameraManager.MaxZoom * cam.aspect / tileManager.tileSize);
        int visibleTilesY = Mathf.CeilToInt(2f * cameraManager.MaxZoom / tileManager.tileSize);

        poolSize = visibleTilesX * visibleTilesY + 100;

        pool = new SpriteRenderer[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(tilePrefab, transform);
            pool[i].enabled = false;
            pool[i].transform.localScale = Vector3.one * tileManager.tileSize;
            pool[i].GetComponent<SpriteRenderer>().sortingLayerName = "Tiles";
        }

        if (simulationManager == null) simulationManager = FindFirstObjectByType<SimulationManager>();
    }

    void Update()
    {
        RenderTiles();
    }

    private void RenderTiles()
    {
        if (tileManager == null)
        {
            Debug.Log("TileVisualManager: No TileManager found.");
            return;
        }

        TileData[] tiles = tileManager.Tiles;

        if (tiles == null) 
        {
            Debug.Log("TileVisualManager: No tiles found in TileManager.");
            return;
        }

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 camPos = cam.transform.position;
        
        int minX = Mathf.Max(0, Mathf.FloorToInt((camPos.x - halfW) / tileManager.tileSize));
        int maxX = Mathf.Min(tileManager.width - 1, Mathf.CeilToInt((camPos.x + halfW) / tileManager.tileSize));
        int minY = Mathf.Max(0, Mathf.FloorToInt((camPos.y - halfH) / tileManager.tileSize));
        int maxY = Mathf.Min(tileManager.height - 1, Mathf.CeilToInt((camPos.y + halfH) / tileManager.tileSize));
        
        int poolIndex = 0;
        
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int i = y * tileManager.width + x;

                float worldX = x * tileManager.tileSize + tileManager.tileSize / 2f;
                float worldY = y * tileManager.tileSize + tileManager.tileSize / 2f;

                Vector3 worldPos = new Vector3(worldX, worldY, 0);

                Vector3 screenPoint = cam.WorldToViewportPoint(worldPos);

                bool isVisible = screenPoint.x > -0.3f && screenPoint.x < 1.3f && screenPoint.y > -0.3f && screenPoint.y < 1.3f;

                if (cam.orthographicSize >= regionThreshold && regionManager != null)
                {
                    RegionData? region = regionManager.GetRegion(x, y, tileManager);

                    bool isBorder = false;

                    if (region.HasValue)
                    {
                        if (poolIndex >= pool.Length) continue;

                        int currentId = region.Value.id;
                        int fid = region.Value.factionId;
                        
                        if (x > 0 && tileManager.GetTile(x-1, y).regionId != currentId) isBorder = true;
                        else if (x < tileManager.width-1 && tileManager.GetTile(x+1, y).regionId != currentId) isBorder = true;
                        else if (y > 0 && tileManager.GetTile(x, y-1).regionId != currentId) isBorder = true;
                        else if (y < tileManager.height-1 && tileManager.GetTile(x, y+1).regionId != currentId) isBorder = true;

                        Color tileColor = (fid >= 0 && fid < factionColors.Length) ? factionColors[fid] : Color.gray;

                        pool[poolIndex].color = tileColor;
                        pool[poolIndex].sprite =  isBorder ? borderTile : regionTile;

                        pool[poolIndex].enabled = true;
                        pool[poolIndex].transform.position = worldPos;
                        poolIndex++;
                    }
                    else
                    {
                        if (poolIndex >= pool.Length) continue;

                        pool[poolIndex].color = Color.gray;
                        pool[poolIndex].sprite = regionTile;

                        pool[poolIndex].enabled = true;
                        pool[poolIndex].transform.position = worldPos;
                        poolIndex++;
                    }
                }
                else if (isVisible && poolIndex < pool.Length)
                {

                    TileData tile = tiles[i];

                    switch (tile.tileType)
                    {
                        case TileType.Grass: pool[poolIndex].sprite = grassTile; break;
                        case TileType.Sand: pool[poolIndex].sprite = sandTile; break;
                        case TileType.Stone: pool[poolIndex].sprite = stoneTile; break;
                        case TileType.Water: pool[poolIndex].sprite = waterTile; break;
                        default: pool[poolIndex].sprite = grassTile; break;
                    }
                    
                    pool[poolIndex].color = Color.white;

                    pool[poolIndex].enabled = true;
                    pool[poolIndex].transform.position = worldPos;
                    poolIndex++;
                }
            }
        }
        for (int i = poolIndex; i < pool.Length; i++)
        {
            pool[i].enabled = false;
        }
    }
}