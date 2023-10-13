using UnityEngine;
using UnityEditor;

namespace ClocknestGames.Library.Editor
{
    public class FindDistanceBetweenEditor : EditorWindow
    {
        private Transform _first;
        private Transform _second;
        private float _result;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Clocknest Games/Utilites/Find Distance")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            FindDistanceBetweenEditor window = (FindDistanceBetweenEditor)EditorWindow.GetWindow(typeof(FindDistanceBetweenEditor), true, "Find Distance");
            window.Show();
        }

        void OnGUI()
        {
            _first = EditorGUILayout.ObjectField("First", _first, typeof(Transform), true) as Transform;
            _second = EditorGUILayout.ObjectField("Second", _second, typeof(Transform), true) as Transform;
            EditorGUILayout.LabelField("Result", _result.ToString());

            if (GUILayout.Button("Calculate"))
            {
                _result = Vector3.Distance(_first.position, _second.position);
            }
        }
    }
}