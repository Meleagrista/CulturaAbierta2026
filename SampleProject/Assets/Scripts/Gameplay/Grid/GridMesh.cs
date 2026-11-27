using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridMesh : MonoBehaviour
{
    public GridManager grid;
    public UnitDataSO selectedUnit;
    public LayerMask groundLayer;

    [Header("Grid lines")]
    public bool showGridLines = true;
    public Color gridLineColor = new Color(1f, 1f, 1f, 0.35f);

    static Material _mat;

    bool _fightStarted;

    void Start()
    {
        if (grid != null && grid.isEnemy)
        {
            enabled = false;
            return;
        }

        EventManager.StartListening(Event.FIGHT_STARTED_EVENT, OnFightStarted);
    }

    void OnDestroy() => EventManager.StopListening(Event.FIGHT_STARTED_EVENT, OnFightStarted);

    void OnFightStarted(Dictionary<string, object> message)
    {
        _fightStarted = true;
        _hasHit = false;
    }

    void OnEnable()
    {
        if (_mat != null) return;
        _mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        _mat.hideFlags = HideFlags.HideAndDontSave;
        _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        _mat.SetInt("_ZWrite", 0);
    }

    Vector2Int _hoverCell;
    bool _hasHit;

    void OnRenderObject()
    {
        if (grid == null || _mat == null)
        {
            return;
        }

        _mat.SetPass(0);
        GL.PushMatrix();

        if (showGridLines)
        {
            DrawGridLines();
        }

        if (_hasHit && selectedUnit != null)
        {
            DrawHoverPreview();
        }

        GL.PopMatrix();
    }

    void DrawGridLines()
    {
        Vector3 o = grid.transform.position + Vector3.up * 0.005f;
        float s = grid.cellSize;
        float width = grid.cols * s;
        float depth = grid.rows * s;

        GL.Begin(GL.LINES);
        GL.Color(gridLineColor);
        for (int x = 0; x <= grid.cols; x++)
        {
            GL.Vertex(o + new Vector3(x * s, 0f, 0f));
            GL.Vertex(o + new Vector3(x * s, 0f, depth));
        }
        for (int z = 0; z <= grid.rows; z++)
        {
            GL.Vertex(o + new Vector3(0f, 0f, z * s));
            GL.Vertex(o + new Vector3(width, 0f, z * s));
        }
        GL.End();
    }

    void DrawHoverPreview()
    {
        bool canPlace = grid.CanAfford(selectedUnit) && grid.CanPlace(selectedUnit, _hoverCell);
        Color fill   = canPlace ? new Color(0f, 1f, 0f, 0.25f) : new Color(1f, 0f, 0f, 0.25f);
        Color border = canPlace ? new Color(0f, 1f, 0f, 0.9f)  : new Color(1f, 0f, 0f, 0.9f);

        foreach (var offset in selectedUnit.footprint)
        {
            Vector2Int c = _hoverCell + offset;
            if (!grid.InBounds(c)) continue;
            Vector3 o = grid.CellToWorld(c);
            float s = grid.cellSize;
            DrawQuad(o, s, fill);
            DrawBorder(o, s, border);
        }
    }

    void DrawQuad(Vector3 o, float s, Color col)
    {
        GL.Begin(GL.QUADS);
        GL.Color(col);
        GL.Vertex3(o.x,     o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z + s);
        GL.Vertex3(o.x,     o.y + 0.01f, o.z + s);
        GL.End();
    }

    void DrawBorder(Vector3 o, float s, Color col)
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(col);
        GL.Vertex3(o.x,     o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z + s);
        GL.Vertex3(o.x,     o.y + 0.01f, o.z + s);
        GL.Vertex3(o.x,     o.y + 0.01f, o.z);
        GL.End();
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (_fightStarted || BattleHUDController.IsPointerOverHUD(mousePos))
        {
            _hasHit = false;
            return;
        }

        _hasHit = Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit hit, 100f, groundLayer);

        if (!_hasHit)
        {
            return;
        }

        _hoverCell = grid.WorldToCell(hit.point);

        if (Mouse.current.leftButton.wasPressedThisFrame && selectedUnit != null)
        {
            grid.TryBuy(selectedUnit, _hoverCell);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            grid.Remove(_hoverCell);
        }
    }
}
