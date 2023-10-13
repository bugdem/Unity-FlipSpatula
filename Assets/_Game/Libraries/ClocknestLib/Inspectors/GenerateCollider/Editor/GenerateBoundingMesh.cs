using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class GenerateBoundingMesh : EditorWindow
{
    [SerializeField] private List<MeshGroup> _renderers;

    private Vector2 _scrollPosition;

    [MenuItem("Clocknest Games/Tools/Generate Bounding Mesh")]
    static void CreateMeshBoundingGeneration()
    {
        EditorWindow.GetWindow<GenerateBoundingMesh>();
    }

    private void OnGUI()
    {
        if (_renderers == null)
            _renderers = new List<MeshGroup>();

        var selection = Selection.gameObjects;
        if (selection.Length <= 0)
        {
            EditorGUILayout.LabelField("Select an object.");
            return;
        }

        if (selection.Length > 1)
        {
            EditorGUILayout.LabelField("You can select only 1 object.");
            return;
        }

        var selected = selection[0];

        EditorGUILayout.LabelField("Selected Object: " + selected.name);

        if (GUILayout.Button("Find Submeshes"))
        {
            _renderers.Clear();

            var renderers = selected.gameObject.GetComponentsInChildren<MeshFilter>();
            for (var i = renderers.Length - 1; i >= 0; --i)
            {
                var renderer = renderers[i];

                MeshGroup newGroup = new MeshGroup
                {
                    Filter = renderer
                };

                _renderers.Add(newGroup);

                /*
                Undo.RegisterCreatedObjectUndo(newObject, "Generate Mesh Collider");
                newObject.transform.parent = selected.transform.parent;
                newObject.transform.localPosition = selected.transform.localPosition;
                newObject.transform.localRotation = selected.transform.localRotation;
                newObject.transform.localScale = selected.transform.localScale;
                newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
                Undo.DestroyObjectImmediate(selected);
                */
            }
        }

        if (_renderers.Count > 0)
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
            for (int index = 0; index < _renderers.Count; index++)
            {
                var renderer = _renderers[index];
                GUILayout.BeginHorizontal();
                renderer.IsSelected = EditorGUILayout.Toggle(renderer.IsSelected);
                EditorGUILayout.LabelField("Selected Object: " + renderer.Filter.gameObject.name);
                GUI.enabled = false;
                renderer.Filter = (MeshFilter)EditorGUILayout.ObjectField(renderer.Filter, typeof(MeshFilter));
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        if (_renderers.Count > 0)
        {
            if (GUILayout.Button("Create Bounding Box Collider"))
            {
                var collider = selected.GetComponent<BoxCollider>();
                if (collider == null)
                    collider = selected.AddComponent<BoxCollider>();

                var selectedRenderers = _renderers.Where(x => x.IsSelected);
                if (selectedRenderers.Count() <= 0)
                {
                    EditorGUILayout.LabelField("You have not selected any meshes!");
                }
                else
                {
                    CombineInstance[] combine = new CombineInstance[selectedRenderers.Count()];
                    int combineIndex = 0;
                    for (int index = 0; index < _renderers.Count; index++)
                    {
                        var renderer = _renderers[index];
                        if (!renderer.IsSelected) continue;

                        combine[combineIndex].mesh = renderer.Filter.sharedMesh;
                        combine[combineIndex].transform = selected.transform.worldToLocalMatrix * renderer.Filter.transform.localToWorldMatrix;

                        combineIndex++;
                    }

                    var mesh = new Mesh();
                    mesh.CombineMeshes(combine);

                    var bounds = new Bounds(mesh.bounds.center, mesh.bounds.size);
                    collider.center = bounds.center;
                    collider.size = bounds.size;

                    EditorGUILayout.LabelField("Collider created!");
                }

                _renderers.Clear();
            }
        }
    }

    [System.Serializable]
    public class MeshGroup
    {
        public bool IsSelected = true;
        public MeshFilter Filter;
    }
}