using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unit spawned on the grid and the tiles it occupies.
/// </summary>
[Serializable]
public class GridUnit
{
    public GameObject instance;
    public List<Vector2Int> cells = new List<Vector2Int>();

    [NonSerialized] public GridBundle owner;
}

/// <summary>
/// Holds every unit spawned in a bundle.
/// </summary>
[Serializable]
public class GridBundle
{
    public UnitDataSO data;
    public Vector2Int origin;
    public int paid;
    public int rotation;
    public List<GridUnit> units = new List<GridUnit>();
}

/// <summary>
/// Bundles automatically added on Start.
/// </summary>
[Serializable]
public class AutomaticGridBundle
{
    public UnitDataSO data;
    public Vector2Int origin;
    [Tooltip("Quarter turns clockwise (0-3). Ignored for commanders.")]
    public int rotation;
}

public class GridManager : MonoBehaviour
{
    public int cols = 10;
    public int rows = 6;
    public float cellSize = 1f;

    [Header("Team")]
    [Tooltip("Enemy grid: only spawns its presets, the player can't place or remove anything and the grid mesh is hidden.")]
    public bool isEnemy;

    [Header("Money")]
    [Tooltip("Used when the current scene isn't in LevelListSO.")]
    public int fallbackStartingMoney = 15;
    [SerializeField] int money;

    [Header("Preset units")]
    public List<AutomaticGridBundle> presetUnits = new List<AutomaticGridBundle>();

    [Header("Placed units")]
    [SerializeField] List<GridBundle> placedUnits = new List<GridBundle>();

    private List<UnitBunddle> placedBunddles = new List<UnitBunddle>();

    GridUnit[,] _cells;

    public IReadOnlyList<GridBundle> PlacedUnits => placedUnits;

    public int Money => money;
    public event Action<int> OnMoneyChanged;

    [SerializeField] GameObject UnitBundlePrefab;

    void Awake()
    {
        _cells = new GridUnit[cols, rows];
        placedUnits.Clear();

        var level = LevelListSO.Instance.Find(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        int surplus = GameStateSO.Instance.GetSurplus();
        int money = isEnemy ? 0 : level?.startingMoney ?? fallbackStartingMoney;

        SetMoney(money + surplus);
    }

    void SetMoney(int value)
    {
        money = value;
        OnMoneyChanged?.Invoke(money);
    }

    public bool CanAfford(UnitDataSO unit) => unit != null && unit.Price <= money;

    void Start()
    {
        foreach (var preset in presetUnits)
        {
            if (preset.data == null)
            {
                continue;
            }

            if (!Place(preset.data, preset.origin, 0, preset.rotation))
            {
                Debug.LogWarning($"Could not place '{preset.data.name}' at {preset.origin}.", this);
            }
        }
    }

    public bool InBounds(Vector2Int cell) => cell.x >= 0 && cell.x < cols && cell.y >= 0 && cell.y < rows;

    public bool CanPlace(UnitDataSO unit, Vector2Int origin, int rotation = 0)
    {
        if (unit == null || unit.footprint == null || unit.footprint.Count == 0)
        {
            return false;
        }

        foreach (var offset in Footprint(unit, rotation))
        {
            Vector2Int cell = origin + offset;
            if (!InBounds(cell) || _cells[cell.x, cell.y] != null)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Player placement: costs the unit's Price.</summary>
    public bool TryBuy(UnitDataSO unit, Vector2Int origin, int rotation = 0)
    {
        if (isEnemy || !CanAfford(unit) || !Place(unit, origin, unit.Price, rotation))
        {
            return false;
        }

        SetMoney(money - unit.Price);
        return true;
    }

    /// <summary>Places a bundle without touching money (presets, scripted spawns).</summary>
    public bool Place(UnitDataSO unit, Vector2Int origin, int paid = 0, int rotation = 0)
    {
        if (!CanPlace(unit, origin, rotation))
        {
            return false;
        }

        GameObject unitBunddleObject = Instantiate(UnitBundlePrefab, CellCenterToWorld(origin), Quaternion.identity);

        UnitBunddle bundle = unitBunddleObject.GetComponent<UnitBunddle>();

        bundle.SetUp(unit,this);

        List<Vector3> unitPositions = new List<Vector3>();

        var placed = new GridBundle { data = unit, origin = origin, paid = paid, rotation = RotationFor(unit, rotation) };

        foreach (var cells in GroupFootprint(unit, origin, rotation))
        {
            foreach(Vector2Int cell in cells)
            {
                unitPositions.Add(CellCenterToWorld(cell));
            }

            var gridUnit = new GridUnit { owner = placed, cells = cells };
            placed.units.Add(gridUnit);

            foreach (var cell in cells)
            {
                _cells[cell.x, cell.y] = gridUnit;
            }
        }

        bundle.SpawnUnits(unitPositions);
        placedBunddles.Add(bundle);
        placedUnits.Add(placed);
        return true;
    }

    List<List<Vector2Int>> GroupFootprint(UnitDataSO unit, Vector2Int origin)
    {
        var groups = new List<List<Vector2Int>>();

        if (unit is CommanderDataSO comanderData)
        {
            foreach (var offset in unit.footprint)
            {
                groups.Add(new List<Vector2Int> { origin + offset });
            }
            return groups;
        }

        foreach (var offset in unit.footprint)
        {
            if (offset.x % 2 != 0 || offset.y % 2 != 0)
            {
                continue;
            }

            Vector2Int corner = origin + offset;
            groups.Add(new List<Vector2Int>
            {
                corner,
                corner + Vector2Int.right,
                corner + Vector2Int.up,
                corner + Vector2Int.one,
            });
        }

        return groups;
    }

    GameObject SpawnInstance(UnitDataSO unit, List<Vector2Int> cells)
    {
        if (unit.prefab == null)
        {
            Debug.LogWarning($"[Grid] '{unit.name}' has no prefab assigned.", unit);
            return null;
        }

        Vector3 center = Vector3.zero;
        foreach (var cell in cells)
        {
            center += CellCenterToWorld(cell);
        }
        center /= cells.Count;

        var go = Instantiate(unit.prefab, center, Quaternion.identity, transform);
        go.name = $"{unit.prefab.name} ({string.Join(" ", cells)})";
        return go;
    }

    public void Remove(Vector2Int cell)
    {
        if (!InBounds(cell))
        {
            return;
        }

        var gridUnit = _cells[cell.x, cell.y];
        if (gridUnit == null)
        {
            return;
        }

        UnitBunddle placedBundle = GetAt(cell);

        var placed = gridUnit.owner;



        foreach (var unit in placed.units)
        {
            foreach (var c in unit.cells)
            {
                _cells[c.x, c.y] = null;
            }

            if (unit.instance != null)
            {
                Destroy(unit.instance);
            }
        }

        Destroy(placedBundle.gameObject);
        placedBunddles.Remove(placedBundle);
        placedUnits.Remove(placed);

        // Refunds what was paid (presets were free, so they give nothing back).
        if (placed.paid > 0)
        {
            SetMoney(money + placed.paid);
        }
    }

    /// <summary>The unit standing on this tile.</summary>
    public GridUnit GetUnitAt(Vector2Int cell) => InBounds(cell) ? _cells[cell.x, cell.y] : null;

    /// <summary>The whole bundle this tile belongs to.</summary>
    public UnitBunddle GetAt(Vector2Int cell)
    {
        var gridUnit = _cells[cell.x, cell.y];
        var placed = gridUnit.owner;
        UnitBunddle placedBundle = null;
        foreach (UnitBunddle bundle in placedBunddles)
        {
            if (placed.origin == WorldToCell(bundle.transform.position))
            {
                placedBundle = bundle;
                return placedBundle;
            }
        }
        return null;
    }
        

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.z / cellSize)
        );
    }

    public Vector3 CellToWorld(Vector2Int cell) => transform.position + new Vector3(cell.x * cellSize, 0, cell.y * cellSize);

    public Vector3 CellCenterToWorld(Vector2Int cell) => CellToWorld(cell) + new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);

    // ---------- Rotation ----------
    // Rotation is a number of 90° clockwise quarter turns (0-3). Commanders are never rotated.
    // The tile groups (the 2x2 blocks) are built unrotated and then turned as a whole, so every
    // unit keeps its block; the result is shifted back so its lowest x/y are 0 again, which keeps
    // the shape anchored at the hovered tile.

    /// <summary>Rotation actually used for this unit: 0 for commanders, otherwise 0-3.</summary>
    public static int RotationFor(UnitDataSO unit, int rotation)
    {
        if (unit is CommanderDataSO)
        {
            return 0;
        }
        return ((rotation % 4) + 4) % 4;
    }

    /// <summary>Footprint offsets after rotating (what CanPlace and the hover preview use).</summary>
    public List<Vector2Int> Footprint(UnitDataSO unit, int rotation)
    {
        int turns = RotationFor(unit, rotation);
        var result = new List<Vector2Int>();
        if (turns == 0)
        {
            result.AddRange(unit.footprint);
            return result;
        }

        Vector2Int shift = RotationShift(unit, turns);
        foreach (var offset in unit.footprint)
        {
            result.Add(RotateCell(offset, turns) + shift);
        }
        return result;
    }

    /// <summary>Tile groups (one per spawned unit) after rotating, placed at origin.</summary>
    List<List<Vector2Int>> GroupFootprint(UnitDataSO unit, Vector2Int origin, int rotation)
    {
        int turns = RotationFor(unit, rotation);
        if (turns == 0)
        {
            return GroupFootprint(unit, origin);
        }

        Vector2Int shift = RotationShift(unit, turns);
        var groups = GroupFootprint(unit, Vector2Int.zero);
        foreach (var group in groups)
        {
            for (int i = 0; i < group.Count; i++)
            {
                group[i] = origin + RotateCell(group[i], turns) + shift;
            }
        }
        return groups;
    }

    // Offset that moves the rotated tiles back so the lowest x and y are 0. It is measured on every
    // tile the unit claims (whole 2x2 blocks), so the blocks never end up below or left of the origin.
    Vector2Int RotationShift(UnitDataSO unit, int turns)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var group in GroupFootprint(unit, Vector2Int.zero))
        {
            foreach (var cell in group)
            {
                Vector2Int r = RotateCell(cell, turns);
                minX = Mathf.Min(minX, r.x);
                minY = Mathf.Min(minY, r.y);
            }
        }
        if (minX == int.MaxValue)
        {
            return Vector2Int.zero;
        }
        return new Vector2Int(-minX, -minY);
    }

    // 90° clockwise seen from above (x = right, y = forward), applied 'turns' times.
    static Vector2Int RotateCell(Vector2Int cell, int turns)
    {
        for (int i = 0; i < turns; i++)
        {
            cell = new Vector2Int(cell.y, -cell.x);
        }
        return cell;
    }
}
