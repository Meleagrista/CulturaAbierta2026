using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatManager : WinCondition
{
    List<UnitBunddle> playerBundles = new List<UnitBunddle>();
    List<UnitBunddle> enemyBundles = new List<UnitBunddle>();

    bool bHasStarted = false;
    bool bHasEnded = false;

    public bool HasStarted => bHasStarted;
    public bool HasEnded => bHasEnded;
    [SerializeField] bool debug = false;
    [SerializeField] float debugTimeScale = 4f;

    void Update()
    {
        // Solo en debug: arrancar con espacio sin pasar por el HUD.
        if (debug && !bHasStarted && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCombat();
        }
    }

    public void StartCombat()
    {
        if (bHasStarted) return;

        if (debug) Time.timeScale = debugTimeScale;

        UnitBunddle[] bundles = FindObjectsByType<UnitBunddle>(FindObjectsSortMode.None);

        foreach (UnitBunddle bundle in bundles)
        {
            if (bundle.GetTeam() == UnitBehaviour.Team.PlayerTeam)
            {
                playerBundles.Add(bundle);
            }
            else
            {
                enemyBundles.Add(bundle);
            }
        }

        foreach (UnitBunddle bundle in playerBundles)
        {
            AddSquadAI(bundle, enemyBundles);
        }

        foreach (UnitBunddle bundle in enemyBundles)
        {
            AddSquadAI(bundle, playerBundles);
        }

        bHasStarted = true;
        Debug.Log($"[Combat] Empieza: {playerBundles.Count} bundles del jugador contra {enemyBundles.Count} enemigos");
    }

    void AddSquadAI(UnitBunddle bundle, List<UnitBunddle> enemies)
    {
        SquadAI squad = bundle.GetComponent<SquadAI>();

        if (squad == null)
        {
            squad = bundle.gameObject.AddComponent<SquadAI>();
        }

        squad.Init(enemies);
    }

    void LateUpdate()
    {
        if (!bHasStarted || bHasEnded) return;

        bool bPlayerDead = AllDead(playerBundles);
        bool bEnemyDead = AllDead(enemyBundles);

        if (!bPlayerDead && !bEnemyDead) return;

        bHasEnded = true;

        Time.timeScale = 1f;

        // Puede haber empate, pero se considera como victoria para el jugador.
        if (bEnemyDead)
        {
            DeclareWin();
        }
        else
        {
            DeclareLose();
        }
    }

    static bool AllDead(List<UnitBunddle> bundles)
    {
        foreach (UnitBunddle bundle in bundles)
        {
            if (bundle != null && bundle.IsAlive()) return false;
        }

        return true;
    }

    protected override void OnEnable()
    {
        EventManager.StartListening(Event.FIGHT_STARTED_EVENT, OnFightStarted);
    }

    protected override void OnDisable()
    {
        EventManager.StopListening(Event.FIGHT_STARTED_EVENT, OnFightStarted);
        Time.timeScale = 1f;
    }

    private void OnFightStarted(Dictionary<string, object> data) => StartCombat();
}
