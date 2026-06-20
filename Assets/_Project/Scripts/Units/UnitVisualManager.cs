using UnityEngine;

public class UnitVisualManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer humanPrefab;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private int poolSize = 100;

    private SpriteRenderer[] pool;

    public void Initialize(UnitManager manager)
    {
        unitManager = manager;

        if (unitManager == null)
        {
            unitManager = FindFirstObjectByType<UnitManager>();
        }

        pool = new SpriteRenderer[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(humanPrefab, transform);
            pool[i].enabled = false;
        }

        Debug.Log($"Initialized UnitVisualManager with capacity for {poolSize} humans.");
    }

    void Update()
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

            if (isVisible && poolIndex < pool.Length)
            {
                pool[poolIndex].enabled = true;

                float lastTickTime = FindFirstObjectByType<SimulationManager>()?.LastTickTime ?? Time.time;
                float tickInterval = FindFirstObjectByType<SimulationManager>()?.TickInterval ?? 0.1f;

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
