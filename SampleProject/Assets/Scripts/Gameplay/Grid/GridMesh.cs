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

    [Header("Shader visual")]
    public Material gridMaterial;
    public float heightOffset = 0.01f;

    static Material _mat;

    bool _fightStarted;

    Texture2D _stateTex;
    Color32[] _states;
    Material _matInstance;

    Dictionary<Vector2Int, int> _occupied = new Dictionary<Vector2Int, int>();
    int _nextId = 1;

    struct PlacedInfo { public UnitDataSO unit; public Vector2Int origin; }
    struct AbilityCell { public Color color; public bool preview; }
    Dictionary<int, PlacedInfo> _placed = new Dictionary<int, PlacedInfo>();
    Dictionary<Vector2Int, AbilityCell> _ability = new Dictionary<Vector2Int, AbilityCell>();

    void Start()
    {
        if (grid != null && grid.isEnemy)
        {
            enabled = false;
            return;
        }

        if (grid != null && gridMaterial != null)
        {
            BuildQuad();
            CreateStateTexture();
        }

        EventManager.StartListening(Event.FIGHT_STARTED_EVENT, OnFightStarted);
    }

    void OnDestroy()
    {
        EventManager.StopListening(Event.FIGHT_STARTED_EVENT, OnFightStarted);

        if (_matInstance != null) Destroy(_matInstance);
        if (_stateTex != null) Destroy(_stateTex);
    }

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
        bool canPlace = grid.CanAfford(selectedUnit) && grid.CanPlace(selectedUnit, _hoverCell, _rotation);
        Color fill = canPlace ? new Color(0f, 1f, 0f, 0.25f) : new Color(1f, 0f, 0f, 0.25f);
        Color border = canPlace ? new Color(0f, 1f, 0f, 0.9f) : new Color(1f, 0f, 0f, 0.9f);

        foreach (var offset in CurrentFootprint)
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
        GL.Vertex3(o.x, o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z + s);
        GL.Vertex3(o.x, o.y + 0.01f, o.z + s);
        GL.End();
    }

    void DrawBorder(Vector3 o, float s, Color col)
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(col);
        GL.Vertex3(o.x, o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z);
        GL.Vertex3(o.x + s, o.y + 0.01f, o.z + s);
        GL.Vertex3(o.x, o.y + 0.01f, o.z + s);
        GL.Vertex3(o.x, o.y + 0.01f, o.z);
        GL.End();
    }

    void BuildQuad()
    {
        float w = grid.cols * grid.cellSize;
        float d = grid.rows * grid.cellSize;

        var mesh = new Mesh { name = "GridQuad" };
        mesh.vertices = new[]
        {
            new Vector3(0, 0, 0), new Vector3(w, 0, 0),
            new Vector3(w, 0, d), new Vector3(0, 0, d)
        };
        mesh.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateBounds();

        var go = new GameObject("GridVisual");
        go.layer = 0; 
        go.transform.SetParent(transform, false);
        go.transform.position = grid.transform.position + Vector3.up * heightOffset;

        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        _matInstance = new Material(gridMaterial);
        mr.sharedMaterial = _matInstance;
    }

    void CreateStateTexture()
    {
        _stateTex = new Texture2D(grid.cols, grid.rows, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        _states = new Color32[grid.cols * grid.rows];

        _matInstance.SetTexture("_StateTex", _stateTex);
        _matInstance.SetVector("_GridSize", new Vector4(grid.cols, grid.rows, 0, 0));
    }

    void UpdateStates()
    {
        RefreshAbility(); 

        if (_stateTex == null) return;

        for (int i = 0; i < _states.Length; i++)
            _states[i] = new Color32(0, 0, 0, 255);

        foreach (var kv in _occupied)
        {
            int idx = kv.Key.y * grid.cols + kv.Key.x;
            _states[idx].r = 255;
        }

        if (_hasHit && selectedUnit != null)
        {
            bool ok = grid.CanAfford(selectedUnit) && grid.CanPlace(selectedUnit, _hoverCell, _rotation);

            foreach (var off in CurrentFootprint)
            {
                Vector2Int c = _hoverCell + off;
                if (!grid.InBounds(c)) continue;

                int idx = c.y * grid.cols + c.x;
                var px = _states[idx];
                if (ok) px.g = 255; else px.b = 255;
                _states[idx] = px;
            }
        }

        _stateTex.SetPixels32(_states);
        _stateTex.Apply(false);
    }

    public bool IsOccupiedCell(Vector2Int c) => _occupied.ContainsKey(c);

    public bool HasHover => _hasHit;
    public Vector2Int HoverCell => _hoverCell;
    public bool HoverValid => _hasHit && selectedUnit != null && grid.CanAfford(selectedUnit) && grid.CanPlace(selectedUnit, _hoverCell, _rotation);

    public bool TryGetAbility(Vector2Int c, out Color color, out bool preview)
    {
        if (_ability.TryGetValue(c, out var a))
        {
            color = a.color;
            preview = a.preview;
            return true;
        }
        color = default;
        preview = false;
        return false;
    }

    void RefreshAbility()
    {
        _ability.Clear();
    }

    public bool IsHoveredCell(Vector2Int c)
    {
        if (!_hasHit || selectedUnit == null) return false;
        foreach (var off in CurrentFootprint)
            if (_hoverCell + off == c) return true;
        return false;
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (!_fightStarted)
        {
            HandleRotationInput();
        }

        if (_fightStarted || BattleHUDController.IsPointerOverHUD(mousePos))
        {
            _hasHit = false;
            UpdateStates();
            return;
        }

        _hasHit = Physics.Raycast(Camera.main.ScreenPointToRay(mousePos), out RaycastHit hit, 100f, groundLayer);

        if (!_hasHit)
        {
            UpdateStates();
            return;
        }

        _hoverCell = grid.WorldToCell(hit.point);

        if (Mouse.current.leftButton.wasPressedThisFrame && selectedUnit != null)
        {
            
            bool couldPlace = grid.CanAfford(selectedUnit) && grid.CanPlace(selectedUnit, _hoverCell, _rotation);

            grid.TryBuy(selectedUnit, _hoverCell, _rotation);

            
            if (couldPlace && !grid.CanPlace(selectedUnit, _hoverCell, _rotation))
            {
                int id = _nextId++;
                _placed[id] = new PlacedInfo { unit = selectedUnit, origin = _hoverCell };
                foreach (var off in CurrentFootprint)
                {
                    Vector2Int c = _hoverCell + off;
                    if (grid.InBounds(c)) _occupied[c] = id;
                }
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            grid.Remove(_hoverCell);

            
            if (_occupied.TryGetValue(_hoverCell, out int removedId))
            {
                var toRemove = new List<Vector2Int>();
                foreach (var kv in _occupied)
                    if (kv.Value == removedId) toRemove.Add(kv.Key);
                foreach (var k in toRemove) _occupied.Remove(k);
                _placed.Remove(removedId);
            }
        }

        UpdateStates(); 
    }

    // ---------- Rotation ----------
    // R or the mouse wheel turns the selected unit 90°. Commanders ignore it (GridManager.RotationFor).
    // The rotation resets to 0 whenever a different unit is selected.

    int _rotation;
    UnitDataSO _rotationUnit;

    /// <summary>Current rotation in quarter turns (0-3), always 0 for commanders.</summary>
    public int Rotation => GridManager.RotationFor(selectedUnit, _rotation);

    /// <summary>Footprint of the selected unit with the current rotation applied.</summary>
    List<Vector2Int> CurrentFootprint => grid.Footprint(selectedUnit, _rotation);

    /// <summary>Turns the selected unit 90° (clockwise = true). Can be called from a HUD button.</summary>
    public void Rotate(bool clockwise = true)
    {
        if (selectedUnit == null || selectedUnit is CommanderDataSO)
        {
            return;
        }
        _rotation = (_rotation + (clockwise ? 1 : 3)) % 4;
    }

    void HandleRotationInput()
    {
        if (selectedUnit != _rotationUnit)
        {
            _rotationUnit = selectedUnit;
            _rotation = 0;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Rotate(true);
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0f)
        {
            Rotate(true);
        }
        else if (scroll < 0f)
        {
            Rotate(false);
        }
    }
}
