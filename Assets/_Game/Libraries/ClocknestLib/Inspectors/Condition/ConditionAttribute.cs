using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClocknestGames.Library.Editor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConditionAttribute : PropertyAttribute
    {
        public string ConditionBoolean = "";
        public bool Hidden = false;
        public bool State = true;

        public ConditionAttribute(string conditionBoolean, bool state = true, bool hideInInspector = false)
        {
            this.ConditionBoolean = conditionBoolean;
            this.State = state;
            this.Hidden = hideInInspector;
        }
    }
}
