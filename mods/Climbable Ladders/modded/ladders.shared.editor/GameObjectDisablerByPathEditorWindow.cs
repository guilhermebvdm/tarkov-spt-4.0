using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using tarkin.ladders.shared;

namespace tarkin.ladders.shared.editor
{
    public class GameObjectDisablerByPathEditorWindow : EditorWindow
    {
        private GameObjectDisablerByPath disablerInstance;
        private SerializedObject serializedDisabler;
        private SerializedProperty pathsToDisableProp;

        private Vector2 scrollPosition;

        private int hiddenObjectCount = 0;
        private int totalObjectCount = 0;

        [MenuItem("Mods/Ladders/GameObject DisablerByPath")]
        public static void ShowWindow()
        {
            GetWindow<GameObjectDisablerByPathEditorWindow>("GameObject DisablerByPath");
        }

        private void OnEnable()
        {
            SceneVisibilityManager.visibilityChanged += UpdateHiddenStatus;
            Selection.selectionChanged += Repaint;
            EditorApplication.hierarchyChanged += FindDisablerAndRefresh;
            FindDisablerAndRefresh();
        }

        private void OnDisable()
        {
            SceneVisibilityManager.visibilityChanged -= UpdateHiddenStatus;
            Selection.selectionChanged -= Repaint;
            EditorApplication.hierarchyChanged -= FindDisablerAndRefresh;
        }

        private void FindDisablerAndRefresh()
        {
            var disablers = FindObjectsOfType<GameObjectDisablerByPath>();
            if (disablers.Length > 0)
            {
                disablerInstance = disablers[0];
                if (disablers.Length > 1) Debug.LogWarning($"[{nameof(GameObjectDisablerByPathEditorWindow)}] Multiple instances of SceneObjectDisabler found. Using the first one: {disablerInstance.name}.");

                serializedDisabler = new SerializedObject(disablerInstance);
                pathsToDisableProp = serializedDisabler.FindProperty("pathsToDisable");
            }
            else
            {
                disablerInstance = null;
                serializedDisabler = null;
                pathsToDisableProp = null;
            }
            UpdateHiddenStatus();
            Repaint();
        }

        private void OnGUI()
        {
            if (disablerInstance == null)
            {
                EditorGUILayout.HelpBox("No SceneObjectDisabler found in the current scene. Please add the component to a GameObject.", MessageType.Warning);
                if (GUILayout.Button("Create Disabler Object"))
                {
                    var go = new GameObject("SceneObjectDisabler_Manager");
                    go.AddComponent<GameObjectDisablerByPath>();
                    FindDisablerAndRefresh();
                }
                return;
            }

            serializedDisabler.Update();

            EditorGUILayout.LabelField("Managing Disabler:", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(disablerInstance, typeof(GameObjectDisablerByPath), true);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedDisabler.FindProperty("mode"));
            EditorGUILayout.PropertyField(serializedDisabler.FindProperty("delay"));


            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Scene Visibility Sync", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int pathCount = pathsToDisableProp.arraySize;
            EditorGUILayout.LabelField($"Status: {hiddenObjectCount} / {totalObjectCount} objects from {pathCount} paths are hidden.");

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
            if (GUILayout.Button("Hide All Targeted Objects"))
            {
                SyncVisibility(true);
            }
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            if (GUILayout.Button("Show All Targeted Objects"))
            {
                SyncVisibility(false);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            GUI.backgroundColor = new Color(0.8f, 0.9f, 1f);
            EditorGUI.BeginDisabledGroup(Selection.gameObjects.Length == 0);
            if (GUILayout.Button($"Add {Selection.gameObjects.Length} Selected Object(s)", GUILayout.Height(30)))
            {
                AddSelectedObjects();
            }
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Objects to Disable", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int indexToRemove = -1;
            for (int i = 0; i < pathsToDisableProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                SerializedProperty pathProp = pathsToDisableProp.GetArrayElementAtIndex(i);
                EditorGUILayout.LabelField(pathProp.stringValue);

                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    List<GameObject> objects = GameObjectDisablerByPath.FindObjectsByPath(pathProp.stringValue);
                    if (objects.Count > 0)
                    {
                        foreach (var obj in objects)
                        {
                            if (SceneVisibilityManager.instance.IsHidden(obj))
                            {
                                SceneVisibilityManager.instance.Show(obj, true);
                            }
                            EditorGUIUtility.PingObject(obj);
                        }
                        Selection.objects = objects.ToArray();
                    }
                }

                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    indexToRemove = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (indexToRemove != -1)
            {
                string pathToRemove = pathsToDisableProp.GetArrayElementAtIndex(indexToRemove).stringValue;
                List<GameObject> objects = GameObjectDisablerByPath.FindObjectsByPath(pathToRemove);
                foreach (var obj in objects)
                {
                    if (obj != null)
                    {
                        SceneVisibilityManager.instance.Show(obj, true);
                    }
                }
                pathsToDisableProp.DeleteArrayElementAtIndex(indexToRemove);
            }

            EditorGUILayout.EndScrollView();

            if (serializedDisabler.ApplyModifiedProperties())
            {
                UpdateHiddenStatus();
            }
        }

        private void SyncVisibility(bool hide)
        {
            if (pathsToDisableProp == null) return;

            Undo.RecordObject(SceneVisibilityManager.instance, hide ? "Hide Targeted Objects" : "Show Targeted Objects");

            for (int i = 0; i < pathsToDisableProp.arraySize; i++)
            {
                string path = pathsToDisableProp.GetArrayElementAtIndex(i).stringValue;
                List<GameObject> objects = GameObjectDisablerByPath.FindObjectsByPath(path);
                foreach (var obj in objects)
                {
                    if (obj != null)
                    {
                        if (hide)
                        {
                            SceneVisibilityManager.instance.Hide(obj, true);
                        }
                        else
                        {
                            SceneVisibilityManager.instance.Show(obj, true);
                        }
                    }
                }
            }
            UpdateHiddenStatus();
        }

        private void UpdateHiddenStatus()
        {
            if (disablerInstance == null || pathsToDisableProp == null)
            {
                hiddenObjectCount = 0;
                totalObjectCount = 0;
                return;
            }

            int hidden = 0;
            int total = 0;
            for (int i = 0; i < pathsToDisableProp.arraySize; i++)
            {
                string path = pathsToDisableProp.GetArrayElementAtIndex(i).stringValue;
                List<GameObject> objects = GameObjectDisablerByPath.FindObjectsByPath(path);
                total += objects.Count;
                foreach (var obj in objects)
                {
                    if (obj != null && SceneVisibilityManager.instance.IsHidden(obj))
                    {
                        hidden++;
                    }
                }
            }
            hiddenObjectCount = hidden;
            totalObjectCount = total;
            Repaint();
        }

        private void AddSelectedObjects()
        {
            Undo.RecordObject(disablerInstance, "Add objects to disabler list");

            int addedCount = 0;
            foreach (var go in Selection.gameObjects)
            {
                string path = GenerateHierarchyPath(go);
                if (!PathExists(path))
                {
                    int newIndex = pathsToDisableProp.arraySize;
                    pathsToDisableProp.InsertArrayElementAtIndex(newIndex);
                    pathsToDisableProp.GetArrayElementAtIndex(newIndex).stringValue = path;
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                Debug.Log($"Added {addedCount} new object path(s) to the disabler list.");
                EditorSceneManager.MarkSceneDirty(disablerInstance.gameObject.scene);
            }

            serializedDisabler.ApplyModifiedProperties();
            SyncVisibility(true);
        }

        private bool PathExists(string path)
        {
            for (int i = 0; i < pathsToDisableProp.arraySize; i++)
            {
                if (pathsToDisableProp.GetArrayElementAtIndex(i).stringValue == path) return true;
            }
            return false;
        }

        private string GenerateHierarchyPath(GameObject obj)
        {
            if (obj == null) return string.Empty;
            string path = string.Join("/", obj.GetComponentsInParent<Transform>(true).Reverse().Select(t => t.name));
            return path;
        }
    }
}