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
}

public class GridManager : MonoBehaviour
{
    public int cols = 10;
    public int rows = 6;
    public float cellSize = 1f;

    [Header("Preset units")]
    public List<AutomaticGridBundle> presetUnits = new List<AutomaticGridBundle>();

    [Header("Placed units")]
    [SerializeField] List<GridBundle> placedUnits = new List<GridBundle>();

    GridUnit[,] _cells;

    public IReadOnlyList<GridBundle> PlacedUnits => placedUnits;

    void Awake()
    {
        _cells = new GridUnit[cols, rows];
        placedUnits.Clear();
    }

    void Start()
    {
        foreach (var preset in presetUnits)
        {
            if (preset.data == null)
            {
                continue;
            }

            if (!TryPlace(preset.data, preset.origin))
            {
                Debug.LogWarning($"Could not place '{preset.data.name}' at {preset.origin}.", this);
            }
        }
    }

    public bool InBounds(Vector2Int cell) => cell.x >= 0 && cell.x < cols && cell.y >= 0 && cell.y < rows;

    public bool CanPlace(UnitDataSO unit, Vector2Int origin)
    {
        if (unit == null || unit.footprint == null || unit.footprint.Count == 0)
        {
            return false;
        }

        foreach (var offset in unit.footprint)
        {
            Vector2Int cell = origin + offset;
            if (!InBounds(cell) || _cells[cell.x, cell.y] != null)
            {
                return false;
            }
        }
        return true;
    }

    public bool TryPlace(UnitDataSO unit, Vector2Int origin)
    {
        if (!CanPlace(unit, origin))
        {
            return false;
        }

        var placed = new GridBundle { data = unit, origin = origin };

        foreach (var cells in GroupFootprint(unit, origin))
        {
            var gridUnit = new GridUnit { owner = placed, cells = cells };
            gridUnit.instance = SpawnInstance(unit, cells);
            placed.units.Add(gridUnit);

            foreach (var cell in cells)
            {
                _cells[cell.x, cell.y] = gridUnit;
            }
        }

        placedUnits.Add(placed);
        return true;
    }

    List<List<Vector2Int>> GroupFootprint(UnitDataSO unit, Vector2Int origin)
    {
        var groups = new List<List<Vector2Int>>();

        if (!unit.Is2x2)
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

        placedUnits.Remove(placed);
    }

    /// <summary>The unit standing on this tile.</summary>
    public GridUnit GetUnitAt(Vector2Int cell) => InBounds(cell) ? _cells[cell.x, cell.y] : null;

    /// <summary>The whole bundle this tile belongs to.</summary>
    public GridBundle GetAt(Vector2Int cell) => GetUnitAt(cell)?.owner;

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
}
