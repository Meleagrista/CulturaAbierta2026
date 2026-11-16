using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public static class TemplateSetup
{
    private const string ScenesDir = "Assets/Scenes";
    private const string UIDir = "Assets/UI";
    private const string ResourcesDir = "Assets/Resources";
    private const string ManagersDir = "Assets/Resources/Managers";
    private const string MaterialsDir = "Assets/Materials";

    // '.' floor, 'S' start, 'G' goal, ' ' hole. Rows go away from the camera.
    private static readonly string[][] LevelMaps =
    {
        new[]
        {
            "S...G",
        },
        new[]
        {
            "S..  ",
            "  .  ",
            "  ..G",
        },
        new[]
        {
            "S..  ",
            ". .  ",
            ".....",
            "   .G",
        },
    };

    private struct Materials
    {
        public Material tile;
        public Material goal;
        public Material player;
    }

    [MenuItem("Tools/Template/Build Sample Content")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder(ScenesDir);
        EnsureFolder(ManagersDir);
        EnsureFolder(MaterialsDir);

        CreateManagerPrefab<EventManager>("EventManager");
        CreateManagerPrefab<GameManager>("GameManager");

        PanelSettings panel = CreatePanelSettings();
        Materials materials = new Materials
        {
            tile = CreateMaterial("Tile", new Color(0.75f, 0.77f, 0.82f)),
            goal = CreateMaterial("Goal", new Color(0.30f, 0.80f, 0.40f)),
            player = CreateMaterial("Player", new Color(0.90f, 0.45f, 0.20f)),
        };

        var buildScenes = new List<EditorBuildSettingsScene>
        {
            CreateMenuScene<TitleMenuController>("Title", panel),
            CreateMenuScene<SelectionScreenController>("Selection", panel),
        };

        var levelNames = new List<string>();
        for (int i = 0; i < LevelMaps.Length; i++)
        {
            string name = $"Level{i + 1}";
            buildScenes.Add(CreateLevelScene(name, LevelMaps[i], panel, materials));
            levelNames.Add(name);
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        CreateDataAssets(levelNames);

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene($"{ScenesDir}/Title.unity");
        Debug.Log("Template sample content built. Press Play in the Title scene.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    private static void CreateManagerPrefab<T>(string name) where T : Component
    {
        var go = new GameObject(name);
        go.AddComponent<T>();
        PrefabUtility.SaveAsPrefabAsset(go, $"{ManagersDir}/{name}.prefab");
        Object.DestroyImmediate(go);
    }

    private static PanelSettings CreatePanelSettings()
    {
        var panel = ScriptableObject.CreateInstance<PanelSettings>();
        panel.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>($"{UIDir}/Theme.tss");
        panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panel.referenceResolution = new Vector2Int(1920, 1080);
        panel.match = 0.5f;
        AssetDatabase.CreateAsset(panel, $"{UIDir}/PanelSettings.asset");
        return panel;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, $"{MaterialsDir}/{name}.mat");
        return material;
    }

    private static void CreateDataAssets(List<string> levelNames)
    {
        var levelList = ScriptableObject.CreateInstance<LevelListSO>();
        levelList.titleScene = "Title";
        levelList.selectionScene = "Selection";
        levelList.levels = levelNames.Select(n => new LevelEntry { sceneName = n }).ToList();
        AssetDatabase.CreateAsset(levelList, $"{ResourcesDir}/LevelListSO.asset");

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GameStateSO>(), $"{ResourcesDir}/GameStateSO.asset");
    }

    private static EditorBuildSettingsScene CreateMenuScene<T>(string name, PanelSettings panel) where T : UIScreen
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        AddCamera(new Vector3(0f, 1f, -10f), Vector3.zero);
        AddScreen<T>(name + "Menu", name, panel, 0);

        return SaveScene(scene, name);
    }

    private static EditorBuildSettingsScene CreateLevelScene(string name, string[] map, PanelSettings panel, Materials materials)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var level = new GameObject("Level").transform;
        Transform goal = null;
        Vector3 start = Vector3.zero;
        Vector3 sum = Vector3.zero;
        int tileCount = 0;

        for (int row = 0; row < map.Length; row++)
        {
            for (int col = 0; col < map[row].Length; col++)
            {
                char cell = map[row][col];
                if (cell == ' ')
                    continue;

                Vector3 position = new Vector3(col, 0f, -row);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile_{col}_{row}";
                tile.transform.SetParent(level);
                tile.transform.position = position + Vector3.down * 0.1f;
                tile.transform.localScale = new Vector3(0.95f, 0.2f, 0.95f);
                tile.GetComponent<Renderer>().sharedMaterial = cell == 'G' ? materials.goal : materials.tile;

                if (cell == 'G') goal = tile.transform;
                if (cell == 'S') start = position;

                sum += position;
                tileCount++;
            }
        }

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.transform.position = start + Vector3.up * 0.5f;
        player.GetComponent<Renderer>().sharedMaterial = materials.player;
        player.AddComponent<Rigidbody>().isKinematic = true;
        player.AddComponent<CubePlayer>();

        var rules = new GameObject("Rules").AddComponent<CubeRules>();
        var serializedRules = new SerializedObject(rules);
        serializedRules.FindProperty("goal").objectReferenceValue = goal;
        serializedRules.ApplyModifiedPropertiesWithoutUndo();

        Vector3 center = sum / tileCount;
        AddCamera(center + new Vector3(0f, 7f, -6f), center);

        AddScreen<GameOverlayController>("HUD", "HUD", panel, 0);
        AddScreen<PauseMenuController>("PauseMenu", "Pause", panel, 10);
        AddScreen<WinScreenController>("WinScreen", "Win", panel, 20);
        AddScreen<LoseScreenController>("LoseScreen", "Lose", panel, 20);

        return SaveScene(scene, name);
    }

    private static void AddCamera(Vector3 position, Vector3 lookAt)
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = go.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.13f, 0.17f);
        go.AddComponent<AudioListener>();
        go.transform.position = position;
        go.transform.LookAt(lookAt);
    }

    private static void AddScreen<T>(string objectName, string uxml, PanelSettings panel, int sortingOrder) where T : UIScreen
    {
        var go = new GameObject(objectName);
        var document = go.AddComponent<UIDocument>();
        document.panelSettings = panel;
        document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UIDir}/{uxml}.uxml");
        document.sortingOrder = sortingOrder;
        go.AddComponent<T>();
    }

    private static EditorBuildSettingsScene SaveScene(Scene scene, string name)
    {
        string path = $"{ScenesDir}/{name}.unity";
        EditorSceneManager.SaveScene(scene, path);
        return new EditorBuildSettingsScene(path, true);
    }
}
