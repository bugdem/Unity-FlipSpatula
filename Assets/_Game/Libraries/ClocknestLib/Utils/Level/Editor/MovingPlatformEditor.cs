#if UNITY_EDITOR

using UnityEditor;

namespace ClocknestGames.Library.Control
{
    /// <summary>
    /// This class adds draws inspector elements of MovingPlatform class.
    /// </summary>
    [CustomEditor(typeof(MovingPlatform), true)]
    [InitializeOnLoad]
    public class MovingPlatformEditor : UnityEditor.Editor
    {
        public MovingPlatform pathMovementTarget
        {
            get
            {
                return (MovingPlatform)target;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (pathMovementTarget.AccelerationType == MovingPlatformAccelerationType.AnimationCurve)
            {
                DrawDefaultInspector();
            }
            else
            {
                UnityEditor.Editor.DrawPropertiesExcluding(serializedObject, new string[] { "_acceleration" });
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}

#endif