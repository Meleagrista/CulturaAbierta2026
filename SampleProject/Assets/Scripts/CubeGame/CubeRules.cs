using System.Collections.Generic;
using UnityEngine;

public class CubeRules : WinCondition
{
    [SerializeField] private Transform goal;

    protected override void OnEnable() => EventManager.StartListening(Event.PLAYER_MOVED_EVENT, OnPlayerMoved);
    protected override void OnDisable() => EventManager.StopListening(Event.PLAYER_MOVED_EVENT, OnPlayerMoved);

    private void OnPlayerMoved(Dictionary<string, object> message)
    {
        Vector3 position = (Vector3)message["position"];

        if (!Physics.Raycast(position, Vector3.down, 1f))
        {
            DeclareLose();
            return;
        }

        if (Mathf.Approximately(position.x, goal.position.x) && Mathf.Approximately(position.z, goal.position.z))
            DeclareWin();
    }
}
