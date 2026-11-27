using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public abstract class UIScreen : MonoBehaviour
{
    protected VisualElement Root => GetComponent<UIDocument>().rootVisualElement;

    public void Show() => Root.style.display = DisplayStyle.Flex;
    public void Hide() => Root.style.display = DisplayStyle.None;

    protected static void LoadScene(string sceneName)
    {
        EventManager.TriggerEvent(Event.LEVEL_LOAD_EVENT, new Dictionary<string, object>
        {
            { "sceneName", sceneName }
        });
    }
}
