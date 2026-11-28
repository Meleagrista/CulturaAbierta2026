using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class UIScreen : MonoBehaviour
{
    protected VisualElement Root => GetComponent<UIDocument>().rootVisualElement;

    public void Show() => Root.style.display = DisplayStyle.Flex;
    public void Hide() => Root.style.display = DisplayStyle.None;

    /// <summary>Shows the line for the current difficulty tier (element 0 = first tier).
    /// Uses the last line if the list is shorter; leaves the UXML text if the list is empty.</summary>
    protected static void SetTierMessage(Label label, List<string> messages)
    {
        if (label == null || messages == null || messages.Count == 0)
        {
            return;
        }

        int tier = Mathf.Clamp(GameStateSO.Instance.Tier, 0, messages.Count - 1);
        label.text = messages[tier];
    }

    protected static void LoadScene(string sceneName)
    {
        EventManager.TriggerEvent(Event.LEVEL_LOAD_EVENT, new Dictionary<string, object>
        {
            { "sceneName", sceneName }
        });
    }
}
