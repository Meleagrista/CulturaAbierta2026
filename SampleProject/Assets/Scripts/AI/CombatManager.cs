using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatManager : MonoBehaviour
{
    List<UnitBunddle> playerBundles = new List<UnitBunddle>();
    List<UnitBunddle> enemyBundles = new List<UnitBunddle>();

    bool bHasStarted = false;
    bool bHasEnded = false;

    public bool HasStarted => bHasStarted;
    public bool HasEnded => bHasEnded;

    void Update()
    {
        // De momento el combate arranca con espacio. Cuando la UI tenga su boton, que llame a StartCombat().
        if (!bHasStarted && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCombat();
        }
    }

    public void StartCombat()
    {
        if (bHasStarted) return;

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

        // Puede pasar porque aquí ya se han calculado los golpes
        if (bPlayerDead && bEnemyDead)
        {
            Debug.Log("[Combat] Tie");
        }
        else if (bEnemyDead)
        {
            Debug.Log("[Combat] Player Wins!");
        }
        else
        {
            Debug.Log("[Combat] Enemy Wins!");
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
}
