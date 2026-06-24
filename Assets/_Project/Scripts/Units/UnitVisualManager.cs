using UnityEngine;

public class UnitVisualManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer humanPrefab;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private int poolSize = 4000;

    private float spriteRadius = 1f;

    public float SpriteRadius => spriteRadius;

    private SpriteRenderer[] pool;

    private SimulationManager simulationManager;
    private SelectionManager selectionManager;
    private FactionManager factionManager;

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

        spriteRadius = humanPrefab.sprite.bounds.size.x / 2f;

        Debug.Log($"Initialized UnitVisualManager with capacity for {poolSize} humans.");
    }

    void Update()
    {
        RenderUnits();
    }

    private void RenderUnits()
    {
        if (unitManager == null)
        {
            Debug.Log("UnitVisualManager: No UnitManager found.");
            return;
        }

        HumanData[] humans = unitManager.Humans;

        if (humans == null) 
        {
            Debug.Log("UnitVisualManager: No humans found in UnitManager.");
            return;
        }

        int poolIndex = 0;

        for (int i = 0; i < humans.Length; i++)
        {
            if (!humans[i].isAlive) continue;

            Vector3 screenPoint = cam.WorldToViewportPoint(humans[i].position);
            bool isVisible = screenPoint.x > -0.1f && screenPoint.x < 1.1f && screenPoint.y > -0.1f && screenPoint.y < 1.1f;

            if (selectionManager == null) selectionManager = FindFirstObjectByType<SelectionManager>();

            if (factionManager == null) factionManager = FindFirstObjectByType<FactionManager>();

            FactionData factionData = factionManager.GetFaction(humans[i].factionId);

            if (factionData != null && factionData.unitSprite != null) pool[poolIndex].sprite = factionData.unitSprite;

            if (selectionManager != null && selectionManager.IsSelected(i)) pool[poolIndex].color = factionData?.selectionColor ?? Color.green; else pool[poolIndex].color = factionData?.factionColor ?? Color.white;

            if (isVisible && poolIndex < pool.Length)
            {
                pool[poolIndex].enabled = true;

                if (simulationManager == null) simulationManager = FindFirstObjectByType<SimulationManager>();

                float lastTickTime = simulationManager?.LastTickTime ?? Time.time;
                float tickInterval = simulationManager?.TickInterval ?? 0.1f;

                float t = Mathf.Clamp01((Time.time - lastTickTime) / tickInterval);

                pool[poolIndex].transform.position = Vector2.Lerp(humans[i].previousPosition, humans[i].position, t);
                poolIndex++;
            }
        }

        for (int i = poolIndex; i < pool.Length; i++)
        {
            pool[i].enabled = false;
        }
    }
}
