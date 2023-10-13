using UnityEngine;
using UnityEditor;

namespace ClocknestGames.Library.Editor
{
    public class GetChildrenCount : EditorWindow
    {
        private Transform _parent;
        private float _result;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Clocknest Games/Utilites/Get Children Count")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            GetChildrenCount window = (GetChildrenCount)EditorWindow.GetWindow(typeof(GetChildrenCount), true, "Get Children Count");
            window.Show();
        }

        void OnGUI()
        {
            _parent = EditorGUILayout.ObjectField("Parent", _parent, typeof(Transform), true) as Transform;
            EditorGUILayout.LabelField("Result", _result.ToString());

            if (GUILayout.Button("Calculate"))
            {
                _result = _parent.childCount;
            }
        }
    }
}