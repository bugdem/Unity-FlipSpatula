#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace ClocknestGames.Library.Editor
{
    [CustomPropertyDrawer(typeof(GUIDAttribute))]
    /// <summary>
    /// This class allows specified attribute to have a unique id. Useful for automated id in scriptable objects.
    /// </summary>
    public class GUIDAttributeDrawer : PropertyDrawer
    {
        /// <summary>
        /// OnGUI, displays the property and the textbox in the specified order
        /// </summary>
        /// <param name="position">Rect.</param>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String
                    && string.IsNullOrWhiteSpace(property.stringValue))
            {
                property.stringValue = System.Guid.NewGuid().ToString();
            }

            Rect textFieldPosition = position;
            textFieldPosition.height = GetPropertyHeight(property, label);

            GUI.enabled = false; // Disable fields
            EditorGUI.PropertyField(textFieldPosition, property, label, true);
            GUI.enabled = true; // Disable fields
        }
    }
}

#endif
