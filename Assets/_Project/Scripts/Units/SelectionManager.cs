using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Camera cam;

    private Vector2 startMousePos;

    private bool isDragging;

    private List<int> selectedUnits = new List<int>();

    public List<int> SelectedUnits => selectedUnits;

    private UnitManager unitManager;

    private ContextMenuManager contextMenuManager;

    private TilemapVisualManager tilemapVisualManager;

    private Vector2Int? lastClickedTile = null;

    private bool forcedMapModeChange = false;

    private bool wasPointerOverUI = false;

    void Awake()
    {
        GameManager.RegisterSelectionManager(this);

        cam = Camera.main;
    }

    void Start()
    {
        unitManager = GameManager.UnitManager;

        contextMenuManager = GameManager.ContextMenuManager;

        tilemapVisualManager = GameManager.TilemapVisualManager;
    }

    void Update()
    {
        TileManager tm = GameManager.TileManager;

        if (Input.GetMouseButtonUp(0) && wasPointerOverUI) wasPointerOverUI = false;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !wasPointerOverUI)
        {
            startMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonDown(0)) wasPointerOverUI = true;

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && !wasPointerOverUI)
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

        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject() && !wasPointerOverUI)
        {
            if (isDragging)
            {
                isDragging = false;

                lastClickedTile = null;

                if (tilemapVisualManager != null && forcedMapModeChange)
                {
                    tilemapVisualManager.ChangeMapMode(0);

                    forcedMapModeChange = false;
                }

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
            
                if (!clickedOnUnit && selectedUnits.Count > 0)
                {
                    selectedUnits.Clear();
                }

                lastClickedTile = null;
            
                Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

                if (tm != null)
                {
                    if (clickedOnUnit) return;

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
        else if (EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = false;
        }

        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0 && !EventSystem.current.IsPointerOverGameObject() && !wasPointerOverUI)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            unitManager.MoveUnits(selectedUnits, worldPos, true);
        }
        else if (Input.GetMouseButtonDown(1) && contextMenuManager != null)
        {
            Vector2 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);

            bool clickedOnUnit = unitManager.GetUnitAtPosition(worldPoint).Count > 0;
            
            if (tm != null)
            {
                if (clickedOnUnit) return;

                if (worldPos.x >= 0 && worldPos.x < tm.width * tm.tileSize && worldPos.y >= 0 && worldPos.y < tm.height * tm.tileSize)
                {
                    lastClickedTile = tm.WorldToTile(worldPos);

                    if (tilemapVisualManager != null && tilemapVisualManager.MapMode != 2)
                    {
                        tilemapVisualManager.ChangeMapMode(2);

                        forcedMapModeChange = true;
                    }
                }
                else
                {
                    lastClickedTile = null;

                    if (tilemapVisualManager != null && forcedMapModeChange)
                    {
                        tilemapVisualManager.ChangeMapMode(0);

                        forcedMapModeChange = false;
                    }
                }

                contextMenuManager.Check(lastClickedTile);
            }
        }

        if (Input.GetKeyUp(KeyCode.V) && selectedUnits.Count > 0)
        {
            unitManager.SwitchAutonomy(selectedUnits);

            Debug.Log($"SelectionManager switch autonomy");
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

    public void SetLastClickedTile(Vector2Int tilePos)
    {
        lastClickedTile = tilePos;
    }

    public Vector2Int? GetLastClickedTile() => lastClickedTile;
}