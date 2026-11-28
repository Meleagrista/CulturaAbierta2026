using UnityEngine;

public class GridTiles : MonoBehaviour
{
    public GridManager grid;
    public GridMesh gridMesh;
    public Material tileMaterial;      // material con el shader Custom/GridTileGlow

    [Header("Forma")]
    public float tileHeight = 0.12f;
    public float gap = 0.04f;

    [Header("Colores")]
    public Color validColor = new Color(0.55f, 0.72f, 0.45f, 1f); // verde salvia
    public Color invalidColor = new Color(0.78f, 0.40f, 0.35f, 1f); // terracota
    public Color occupiedColor = new Color(0.38f, 0.58f, 0.68f, 1f); // azul azulejo apagado

    [Header("Altura")]
    public float hoverRise = 0.07f;
    public float occupiedRise = 0.025f;
    public float neighborRise = 0.02f;

    [Header("Suavizado")]
    public float colorSpeed = 9f;     // velocidad de cambio de color
    public float springStiffness = 300f;
    public float springDamping = 28f;  // más bajo = más rebote

    [Header("Vecinos del cursor")]
    public float neighborRadius = 2f;
    [Range(0, 1)] public float neighborGlow = 0.2f;

    [Header("Onda al colocar")]
    public float waveSpeed = 6f;       // casillas por segundo
    public float waveDuration = 0.8f;
    public float waveRise = 0.03f;

    Transform[] _tiles;
    Renderer[] _rends;
    float[] _rise, _vel, _state;
    Color[] _color;
    bool[] _wasOccupied;
    int[] _key;        // grupo visual de cada losa (0 = ninguno)
    float[] _ext;      // 4 por losa: +x, -x, +z, -z (0..1 = unida)

    MaterialPropertyBlock _mpb;
    Vector2Int _waveOrigin;
    float _waveStart = -999f;

    static readonly int StateColorID = Shader.PropertyToID("_StateColor");
    static readonly int StateID = Shader.PropertyToID("_State");
    static readonly int FlashID = Shader.PropertyToID("_Flash");
    static readonly int SidesID = Shader.PropertyToID("_Sides");

    void Start()
    {
        int n = grid.cols * grid.rows;
        _tiles = new Transform[n];
        _rends = new Renderer[n];
        _rise = new float[n];
        _vel = new float[n];
        _state = new float[n];
        _color = new Color[n];
        _wasOccupied = new bool[n];
        _key = new int[n];
        _ext = new float[n * 4];
        _mpb = new MaterialPropertyBlock();

        float s = grid.cellSize;
        for (int z = 0; z < grid.rows; z++)
            for (int x = 0; x < grid.cols; x++)
            {
                int i = z * grid.cols + x;

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(cube.GetComponent<Collider>());
                cube.name = $"Tile_{x}_{z}";
                cube.layer = 0;
                cube.transform.SetParent(transform, false);
                cube.transform.localScale = new Vector3(s - gap, tileHeight, s - gap);

                var r = cube.GetComponent<Renderer>();
                if (tileMaterial != null) r.sharedMaterial = tileMaterial;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;

                _tiles[i] = cube.transform;
                _rends[i] = r;
                _color[i] = occupiedColor;
            }
    }

    int KeyAt(int x, int z)
    {
        if (x < 0 || z < 0 || x >= grid.cols || z >= grid.rows) return 0;
        return _key[z * grid.cols + x];
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float t = Time.time;
        float s = grid.cellSize;
        Vector3 o = grid.transform.position;

        bool hasHover = gridMesh.HasHover;
        Vector2Int hc = gridMesh.HoverCell;
        bool valid = gridMesh.HoverValid;
        Color hoverCol = valid ? validColor : invalidColor;

        float colorK = 1f - Mathf.Exp(-colorSpeed * dt);

        // 1) Detectar si se acaba de colocar algo para lanzar la onda
        for (int z = 0; z < grid.rows; z++)
            for (int x = 0; x < grid.cols; x++)
            {
                int i = z * grid.cols + x;
                bool occ = gridMesh.IsOccupiedCell(new Vector2Int(x, z));
                if (occ && !_wasOccupied[i])
                {
                    _waveOrigin = new Vector2Int(x, z);
                    _waveStart = t;
                }
                _wasOccupied[i] = occ;
                _key[i] = gridMesh.IsHoveredCell(new Vector2Int(x, z)) ? 2 : (occ ? 1 : 0);
            }

        float waveT = (t - _waveStart) / waveDuration; // 0..1 mientras dura

        // 2) Actualizar cada losa
        for (int z = 0; z < grid.rows; z++)
            for (int x = 0; x < grid.cols; x++)
            {
                int i = z * grid.cols + x;
                var cell = new Vector2Int(x, z);

                float targetState = 0f;
                float targetRise = 0f;
                Color targetCol = _color[i]; // si está en reposo, mantiene el color para apagarse suave

                bool occ = _wasOccupied[i];
                bool hovered = gridMesh.IsHoveredCell(cell);

                if (occ)
                {
                    targetCol = occupiedColor;
                    targetState = 0.6f + 0.05f * Mathf.Sin(t * 1.5f + (x + z) * 0.6f);
                    targetRise = occupiedRise;
                }
                else if (hasHover)
                {
                    float dist = Vector2.Distance(cell, hc);
                    float f = Mathf.Clamp01(1f - dist / neighborRadius);
                    f *= f;
                    targetCol = hoverCol;
                    targetState = f * neighborGlow;
                    targetRise = f * neighborRise;
                }

                if (hovered)
                {
                    targetCol = hoverCol;
                    targetState = 0.9f + 0.1f * Mathf.Sin(t * 3f);
                    targetRise = hoverRise;
                }

                // Onda al colocar
                float flash = 0f;
                if (waveT >= 0f && waveT < 1f)
                {
                    float dist = Vector2.Distance(cell, _waveOrigin);
                    float front = waveT * waveDuration * waveSpeed;
                    float ring = Mathf.Exp(-Mathf.Pow(dist - front, 2f) / 0.6f);
                    flash = ring * (1f - waveT);
                    targetRise += flash * waveRise;
                }

                // Suavizado de color y estado
                _color[i] = Color.Lerp(_color[i], targetCol, colorK);
                _state[i] = Mathf.Lerp(_state[i], targetState, colorK);

                // Altura con muelle (pequeño rebote)
                _vel[i] += (targetRise - _rise[i]) * springStiffness * dt;
                _vel[i] *= Mathf.Exp(-springDamping * dt);
                _rise[i] += _vel[i] * dt;

                // Unión con vecinas del mismo grupo (se funden y solo queda el perímetro)
                int key = _key[i];
                int bi = i * 4;
                _ext[bi] = Mathf.Lerp(_ext[bi], (key != 0 && KeyAt(x + 1, z) == key) ? 1f : 0f, colorK);
                _ext[bi + 1] = Mathf.Lerp(_ext[bi + 1], (key != 0 && KeyAt(x - 1, z) == key) ? 1f : 0f, colorK);
                _ext[bi + 2] = Mathf.Lerp(_ext[bi + 2], (key != 0 && KeyAt(x, z + 1) == key) ? 1f : 0f, colorK);
                _ext[bi + 3] = Mathf.Lerp(_ext[bi + 3], (key != 0 && KeyAt(x, z - 1) == key) ? 1f : 0f, colorK);

                float sx = (s - gap) + gap * 0.5f * (_ext[bi] + _ext[bi + 1]);
                float sz = (s - gap) + gap * 0.5f * (_ext[bi + 2] + _ext[bi + 3]);
                float ox = gap * 0.25f * (_ext[bi] - _ext[bi + 1]);
                float oz = gap * 0.25f * (_ext[bi + 2] - _ext[bi + 3]);

                _tiles[i].localScale = new Vector3(sx, tileHeight, sz);

                float y = tileHeight * 0.5f + _rise[i];
                _tiles[i].position = o + new Vector3((x + 0.5f) * s + ox, y, (z + 0.5f) * s + oz);

                _mpb.SetVector(SidesID, new Vector4(_ext[bi], _ext[bi + 1], _ext[bi + 2], _ext[bi + 3]));

                _mpb.SetColor(StateColorID, _color[i]);
                _mpb.SetFloat(StateID, _state[i]);
                _mpb.SetFloat(FlashID, flash);
                _rends[i].SetPropertyBlock(_mpb);
            }
    }
}