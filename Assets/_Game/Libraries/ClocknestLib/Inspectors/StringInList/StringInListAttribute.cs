using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClocknestGames.Library.Editor
{
    public class StringInListAttribute : PropertyAttribute
    {
        public delegate string[] GetStringList();

        public StringInListAttribute(params string[] list)
        {
            List = list;
        }

        public StringInListAttribute(Type type, string methodName, object[] parameters = null)
        {
            var method = type.GetMethod(methodName);
            if (method != null)
            {
                List = method.Invoke(null, parameters) as string[];
            }
            else
            {
                Debug.LogError("NO SUCH METHOD " + methodName + " FOR " + type);
            }
        }

        public string[] List
        {
            get;
            private set;
        }
    }
}