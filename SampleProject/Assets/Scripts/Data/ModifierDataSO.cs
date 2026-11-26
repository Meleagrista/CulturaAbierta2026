using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Modifiers/Modifier Data")]
public class ModifierDataSO : MonoBehaviour
{
    [SerializeField] private string modifierScriptName;
    public Type GetModifierType()
    {
        return Type.GetType(modifierScriptName);
    }
}
