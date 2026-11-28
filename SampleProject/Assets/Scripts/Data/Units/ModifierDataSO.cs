using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Modifiers/Modifier Data")]
public class ModifierDataSO : ScriptableObject
{
    [SerializeField] private string modifierScriptName;
    public Type GetModifierType()
    {
        return Type.GetType(modifierScriptName);
    }
}
