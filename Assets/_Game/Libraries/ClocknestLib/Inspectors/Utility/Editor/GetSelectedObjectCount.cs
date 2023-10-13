using UnityEngine;
using UnityEditor;

namespace ClocknestGames.Library.Editor
{
    public class GetSelectedObjectCount : EditorWindow
    {
        private float _result;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Clocknest Games/Utilites/Get Selected Count")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            GetSelectedObjectCount window = (GetSelectedObjectCount)EditorWindow.GetWindow(typeof(GetSelectedObjectCount), true, "Get Selected Count");
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Result", _result.ToString());

            if (GUILayout.Button("Calculate"))
            {
                _result = Selection.objects.Length;
            }
        }
    }
}