using UnityEngine;
using System.Collections.Generic;

public class UnitVisualManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer humanPrefab;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private int poolSize = 4000;

    [Header("Bars")]
    [SerializeField] private SpriteRenderer healthBarPrefab;
    [SerializeField] private SpriteRenderer staminaBarPrefab;
    [SerializeField] private float barWidth = 1f;
    [SerializeField] private float barHeight = 0.1f;

    private SpriteRenderer[] healthBarPool;
    private SpriteRenderer[] staminaBarPool;

    [Header("Status Icons")]
    [SerializeField] private SpriteRenderer statusIconPrefab;
    [SerializeField] private Sprite tiredIcon;
    [SerializeField] private Sprite exhaustedIcon;
    [SerializeField] private Sprite desertingIcon;
    [SerializeField] private Sprite lowLoyaltyIcon;
    [SerializeField] private Sprite avoidingIcon;

    [Header("Combat Feedback")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.2f;

    private Dictionary<int, float> damageFlashTimers = new();

    private SpriteRenderer[] statusIconPool;

    private float spriteRadius = 1f;

    public float SpriteRadius => spriteRadius;

    private SpriteRenderer[] pool;

    private Dictionary<int, Sprite> cachedUnitSprites = new();
    private Dictionary<int, Color> cachedFactionColors = new();
    private Dictionary<int, Color> cachedSelectionColors = new();

    private int lastCacheFrame = -1;

    private float cachedPlayerLoyalty = 50f;
    private int lastLoyaltyFrame = -1;

    private int playerFactionId = -2;

    private SimulationManager simulationManager;
    private SelectionManager selectionManager;
    private FactionManager factionManager;
    private PoliticsManager politicsManager;
    private TileManager tileManager;

    public void Initialize(UnitManager manager)
    {
        unitManager = manager;

        if (unitManager == null) unitManager = FindFirstObjectByType<UnitManager>();

        pool = new SpriteRenderer[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(humanPrefab, transform);
            pool[i].enabled = false;
            pool[i].GetComponent<SpriteRenderer>().sortingLayerName = "Units";
        }

        if (simulationManager == null) simulationManager = FindFirstObjectByType<SimulationManager>();

        if (factionManager == null) factionManager = FindFirstObjectByType<FactionManager>();

        if (politicsManager == null) politicsManager = FindFirstObjectByType<PoliticsManager>();

        if (tileManager == null) tileManager = FindFirstObjectByType<TileManager>();

        spriteRadius = humanPrefab.sprite.bounds.size.x / 1.2f;

        healthBarPool = new SpriteRenderer[poolSize];

        staminaBarPool = new SpriteRenderer[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            healthBarPool[i] = Instantiate(healthBarPrefab, transform);

            healthBarPool[i].enabled = false;
            healthBarPool[i].sortingLayerName = "Units";
            healthBarPool[i].sortingOrder = 1;

            staminaBarPool[i] = Instantiate(staminaBarPrefab, transform);

            staminaBarPool[i].enabled = false;
            staminaBarPool[i].sortingLayerName = "Units";
            staminaBarPool[i].sortingOrder = 1;
        }

        statusIconPool = new SpriteRenderer[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            statusIconPool[i] = Instantiate(statusIconPrefab, transform);

            statusIconPool[i].enabled = false;
            statusIconPool[i].sortingLayerName = "Units";
            statusIconPool[i].sortingOrder = 2;
        }
    }

    void Update()
    {
        RenderUnits();
    }

    public void TriggerDamageFlash(int unitIndex)
    {
        if (unitIndex >= 0) damageFlashTimers[unitIndex] = damageFlashDuration;
    }

    private void RenderUnits()
    {
        if (unitManager == null) return;

        HumanData[] humans = unitManager.Humans;

        if (humans == null) return;
        
        if (simulationManager == null) simulationManager = FindFirstObjectByType<SimulationManager>();

        if (selectionManager == null) selectionManager = FindFirstObjectByType<SelectionManager>();

        if (factionManager == null) factionManager = FindFirstObjectByType<FactionManager>();

        if (politicsManager == null) politicsManager = FindFirstObjectByType<PoliticsManager>();

        if (lastLoyaltyFrame != Time.frameCount)
        {
            lastLoyaltyFrame = Time.frameCount;

            if (playerFactionId == -2)
            {
                for (int i = 0; i < factionManager.FactionCount; i++)
                {
                    if (factionManager.GetFaction(i).isPlayer) playerFactionId = i;
                }
            }

            cachedPlayerLoyalty = politicsManager.GetFactionLoyalty(playerFactionId);
        }

        if (lastCacheFrame != Time.frameCount)
        {
            lastCacheFrame = Time.frameCount;

            cachedUnitSprites.Clear();

            cachedFactionColors.Clear();

            cachedSelectionColors.Clear();
        }

        float lastTickTime = simulationManager?.LastTickTime ?? Time.time;
        float tickInterval = simulationManager?.TickInterval ?? 0.1f;

        int mainIndex = 0;
        int barIndex = 0;
        int iconIndex = 0;
        
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 camPos = cam.transform.position;

        int minX = Mathf.Max(0, Mathf.FloorToInt((camPos.x - halfW) / tileManager.tileSize));
        int maxX = Mathf.Min(tileManager.width - 1, Mathf.CeilToInt((camPos.x + halfW) / tileManager.tileSize));
        int minY = Mathf.Max(0, Mathf.FloorToInt((camPos.y - halfH) / tileManager.tileSize));
        int maxY = Mathf.Min(tileManager.height - 1, Mathf.CeilToInt((camPos.y + halfH) / tileManager.tileSize));

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive) continue;

            Vector2Int unitTile = tileManager.WorldToTile(humans[i].position);
            
            if (unitTile.x < minX || unitTile.x > maxX || unitTile.y < minY || unitTile.y > maxY) continue;
                
            float t = Mathf.Clamp01((Time.time - lastTickTime) / tickInterval);

            Vector2 displayPos = Vector2.Lerp(humans[i].previousPosition, humans[i].position, t);
                    
            if (mainIndex < pool.Length)
            {
                int fid = humans[i].factionId;
    
                if (!cachedUnitSprites.ContainsKey(fid))
                {
                    FactionData fd = factionManager.GetFaction(fid);
    
                    cachedUnitSprites[fid] = fd?.unitSprite;
                    cachedFactionColors[fid] = fd?.factionColor ?? Color.white;
                    cachedSelectionColors[fid] = fd?.selectionColor ?? Color.green;
                }
                Sprite sprite = cachedUnitSprites[fid];
    
                if (sprite != null) pool[mainIndex].sprite = sprite;
                
                if (damageFlashTimers.TryGetValue(i, out float flashTime) && flashTime > 0f)
                {
                    pool[mainIndex].color = damageColor;

                    damageFlashTimers[i] -= Time.deltaTime;

                    if (damageFlashTimers[i] <= 0f) damageFlashTimers.Remove(i);
                }
                else
                {
                    pool[mainIndex].color = selectionManager != null && selectionManager.IsSelected(i) ? cachedSelectionColors[fid] : cachedFactionColors[fid];
                }
                
                pool[mainIndex].enabled = true;
                pool[mainIndex].transform.position = displayPos;
    
                mainIndex++;
            }
                     
            if (barIndex < healthBarPool.Length)
            {
                float hpRatio = humans[i].hp / humans[i].maxHp;
    
                healthBarPool[barIndex].enabled = true;
    
                healthBarPool[barIndex].transform.position = (Vector3)(displayPos + Vector2.up * 0.45f);
                healthBarPool[barIndex].transform.localScale = new Vector3(barWidth * hpRatio, barHeight, 1f);
                healthBarPool[barIndex].color = Color.Lerp(Color.red, Color.green, hpRatio);
    
                float staminaRatio = humans[i].stamina / humans[i].maxStamina;
    
                staminaBarPool[barIndex].enabled = true;
    
                staminaBarPool[barIndex].transform.position = (Vector3)(displayPos + Vector2.up * 0.4f);
                staminaBarPool[barIndex].transform.localScale = new Vector3(barWidth * staminaRatio, barHeight, 1f);
                staminaBarPool[barIndex].color = Color.Lerp(Color.yellow, Color.cyan, staminaRatio);
                barIndex++;
            }
                     
            if (iconIndex < statusIconPool.Length)
            {
                SpriteRenderer icon = statusIconPool[iconIndex];
    
                icon.enabled = false;
                         
                icon.transform.position = (Vector3)(displayPos + Vector2.up * 0.6f);

                if (humans[i].isExhausted) { icon.sprite = exhaustedIcon; icon.enabled = true; }
                else if (humans[i].stamina < 50f) { icon.sprite = tiredIcon; icon.enabled = true; }
                else if (humans[i].isAvoiding) { icon.sprite = avoidingIcon; icon.enabled = true; }
                else if (cachedPlayerLoyalty < 30f) { icon.sprite = lowLoyaltyIcon; icon.enabled = true; }
                else if (cachedPlayerLoyalty < 10f) { icon.sprite = desertingIcon; icon.enabled = true; }

                iconIndex++;
            }
        }
        
        for (int i = mainIndex; i < pool.Length; i++) pool[i].enabled = false;

        for (int i = barIndex; i < healthBarPool.Length; i++)
        {
            healthBarPool[i].enabled = false;
            staminaBarPool[i].enabled = false;
        }

        for (int i = iconIndex; i < statusIconPool.Length; i++) statusIconPool[i].enabled = false;
    }
}