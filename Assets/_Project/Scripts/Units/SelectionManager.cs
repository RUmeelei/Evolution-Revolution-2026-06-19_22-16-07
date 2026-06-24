using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Camera cam;

    private Vector2 startMousePos;

    private bool isDragging;

    private List<int> selectedUnits = new List<int>();

    private UnitManager unitManager;

    private Vector2Int? lastClickedTile = null;

    void Awake()
    {
        cam = Camera.main;

        unitManager = FindFirstObjectByType<UnitManager>();
    }

    void Update()
    {
        TileManager tm = FindFirstObjectByType<TileManager>();

        if (Input.GetMouseButtonDown(0))
        {
            startMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 p1 = startMousePos;
            Vector2 p2 = Input.mousePosition;

            Vector2 p3 = p1 - p2;

            if (p3.sqrMagnitude > 200)
            {
                isDragging = true;
            }
            else
            {
                isDragging = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;

                lastClickedTile = null;

                Rect worldRect = GetWorldRect(startMousePos, Input.mousePosition);

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (var unit in unitManager.GetUnitsInRect(worldRect))
                    {
                        if (!selectedUnits.Contains(unit)) selectedUnits.Add(unit);
                    }
                }
                else
                {
                    selectedUnits = unitManager.GetUnitsInRect(worldRect);
                }
            }
            else
            {
                Vector2 world = cam.ScreenToWorldPoint(startMousePos);

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    foreach (var unit in unitManager.GetUnitAtPosition(world))
                    {
                        if (!selectedUnits.Contains(unit)) selectedUnits.Add(unit);
                    }
                }
                else
                {
                    selectedUnits = unitManager.GetUnitAtPosition(world);
                }
            
                Vector2 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);

                bool clickedOnUnit = unitManager.GetUnitAtPosition(worldPoint).Count > 0;
                bool clickedOnTile = tm != null && tm.WorldToTile(worldPoint) != null;
            
                if (!clickedOnUnit && selectedUnits.Count > 0)
                {
                    selectedUnits.Clear();
                    lastClickedTile = null;
                }
            
                Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

                if (tm != null)
                {
                    if (worldPos.x >= 0 && worldPos.x < tm.width * tm.tileSize && worldPos.y >= 0 && worldPos.y < tm.height * tm.tileSize)
                    {
                        lastClickedTile = tm.WorldToTile(worldPos);
                    }
                    else
                    {
                        lastClickedTile = null;
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            unitManager.MoveUnits(selectedUnits, worldPos, true);
        }
    }

    Rect GetWorldRect(Vector2 screenPos1, Vector2 screenPos2)
    {
        Vector2 world1 = cam.ScreenToWorldPoint(new Vector3(screenPos1.x, screenPos1.y, -cam.transform.position.z));
        Vector2 world2 = cam.ScreenToWorldPoint(new Vector3(screenPos2.x, screenPos2.y, -cam.transform.position.z));

        float xMin = Mathf.Min(world1.x, world2.x);
        float yMin = Mathf.Min(world1.y, world2.y);
        float width = Mathf.Abs(world1.x - world2.x);
        float height = Mathf.Abs(world1.y - world2.y);

        return new Rect(xMin, yMin, width, height);
    }

    void OnGUI()
    {
        if (isDragging)
        {
            Rect screenRect = GetScreenRect(startMousePos, Input.mousePosition);

            GUI.Box(screenRect, "");
        }
    }

    Rect GetScreenRect(Vector2 p1, Vector2 p2)
    {
        float x = Mathf.Min(p1.x, p2.x);
        float y = Screen.height - Mathf.Max(p1.y, p2.y);
        float w = Mathf.Abs(p1.x - p2.x);
        float h = Mathf.Abs(p1.y - p2.y);

        return new Rect(x, y, w, h);
    }

    public bool IsSelected(int unitIndex)
    {
        return selectedUnits.Contains(unitIndex);
    }

    public int GetFirstSelectedUnit()
    {
        return selectedUnits.Count > 0 ? selectedUnits[0] : -1;
    }

    public Vector2Int? GetLastClickedTile() => lastClickedTile;
}