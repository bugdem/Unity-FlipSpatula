#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace ClocknestGames.Library.Control
{
    /// <summary>
    /// This class adds names for each LevelMapPathElement next to it on the scene view, for easier setup
    /// </summary>
    [CustomEditor(typeof(PathMovement), true)]
    [InitializeOnLoad]
    public class PathMovementEditor : UnityEditor.Editor
    {
        public PathMovement pathMovementTarget
        {
            get
            {
                return (PathMovement)target;
            }
        }

        /// <summary>
        /// OnSceneGUI, draws repositionable handles at every point in the path, for easier setup
        /// </summary>
        protected virtual void OnSceneGUI()
        {
            Handles.color = Color.green;
            PathMovement t = (target as PathMovement);

            if (t.IsOriginalPositionSet == false)
            {
                return;
            }

            for (int i = 0; i < t.Waypoints.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 oldPoint = t.OriginalPosition + t.Waypoints[i].Position;
                GUIStyle style = new GUIStyle();

                // draws the path item number
                style.normal.textColor = Color.yellow;
                Handles.Label(t.OriginalPosition + t.Waypoints[i].Position + (Vector3.down * 0.4f) + (Vector3.right * 0.4f), "" + i, style);

                // draws a movable handle
                Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, Quaternion.identity, .5f, new Vector3(.25f, .25f, .25f), Handles.CircleHandleCap);

                // records changes
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Free Move Handle");
                    t.Waypoints[i].Position = newPoint - t.OriginalPosition;
                }
            }
        }
    }
}

#endif