using UnityEngine;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        // EventManager must exist before GameManager subscribes to it.
        Spawn("Managers/EventManager");
        Spawn("Managers/GameManager");
    }

    private static void Spawn(string path)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"Bootstrapper: missing prefab at Resources/{path}. Run Tools > Template > Build Sample Content.");
            return;
        }

        Object.Instantiate(prefab).name = prefab.name;
    }
}
