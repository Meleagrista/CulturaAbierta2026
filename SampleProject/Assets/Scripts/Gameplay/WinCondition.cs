using UnityEngine;

public abstract class WinCondition : MonoBehaviour
{
    protected abstract void OnEnable();

    protected abstract void OnDisable();

    protected void DeclareWin()
    {
        EventManager.TriggerEvent(Event.GAME_WIN_EVENT, null);
    }

    protected void DeclareLose()
    {
        EventManager.TriggerEvent(Event.GAME_LOSE_EVENT, null);
    }
}
