﻿using UnityEngine;
using UnityEditor;

namespace ClocknestGames.Library.Editor
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributeDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // One line of  oxygen free code.
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}