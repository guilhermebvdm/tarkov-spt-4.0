using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace tarkin.ladders.shared
{
    public class GameObjectDisablerByPath : MonoBehaviour
    {
        public Mode mode;

        public enum Mode
        {
            DisableTemporary,
            DisablePermanent,
            Destroy
        }

        public List<string> pathsToDisable;

        public float delay;

        Dictionary<GameObject, bool> originalStates = new Dictionary<GameObject, bool>();

        void Start()
        {
            StartCoroutine(DisableObjectsRoutine());
        }

        void OnDestroy()
        {
            if (mode == Mode.DisableTemporary)
            {
                foreach (var kvp in originalStates)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.SetActive(kvp.Value);
                    }
                }
            }
        }

        private IEnumerator DisableObjectsRoutine()
        {
            // just in case
            yield return new WaitForEndOfFrame();

            if (delay > 0)
                yield return new WaitForSecondsRealtime(delay);

            Debug.Log($"[{nameof(GameObjectDisablerByPath)}] Processing {pathsToDisable.Count} paths...");
            int processedPathCount = 0;
            int totalObjectsAffected = 0;

            foreach (string path in pathsToDisable)
            {
                List<GameObject> targetObjects = FindObjectsByPath(path);

                if (targetObjects.Count > 0)
                {
                    processedPathCount++;
                    foreach (GameObject targetObject in targetObjects)
                    {
                        if (targetObject != null)
                        {
                            switch (mode)
                            {
                                case Mode.Destroy:
                                    Destroy(targetObject);
                                    break;
                                case Mode.DisableTemporary:
                                    if (!originalStates.ContainsKey(targetObject))
                                        originalStates.Add(targetObject, targetObject.activeSelf);
                                    targetObject.SetActive(false);
                                    break;
                                case Mode.DisablePermanent:
                                    targetObject.SetActive(false);
                                    break;
                            }
                            totalObjectsAffected++;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[{nameof(GameObjectDisablerByPath)}] Could not find any objects at path: {path}");
                }
            }

            Debug.Log($"[{nameof(GameObjectDisablerByPath)}] Finished processing. Successfully processed {processedPathCount}/{pathsToDisable.Count} paths, affecting a total of {totalObjectsAffected} objects.");
        }

        public static List<GameObject> FindObjectsByPath(string path)
        {
            var foundObjects = new List<GameObject>();
            if (string.IsNullOrEmpty(path)) return foundObjects;

            // Standardize the path by removing a leading slash if it exists, just to be safe
            if (path.StartsWith("/"))
                path = path.Substring(1);

            string[] names = path.Split('/');

            var rootObjects = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.name == names[0])
                    {
                        rootObjects.Add(go);
                    }
                }
            }

            if (names.Length == 1)
            {
                return rootObjects;
            }

            foreach (var root in rootObjects)
            {
                FindChildrenRecursive(root.transform, names, 1, foundObjects);
            }

            return foundObjects;
        }

        private static void FindChildrenRecursive(Transform currentParent, string[] names, int index, List<GameObject> foundObjects)
        {
            if (index >= names.Length) return;

            string targetName = names[index];

            for (int i = 0; i < currentParent.childCount; i++)
            {
                Transform child = currentParent.GetChild(i);
                if (child.name == targetName)
                {
                    if (index == names.Length - 1)
                    {
                        foundObjects.Add(child.gameObject);
                    }
                    else
                    {
                        FindChildrenRecursive(child, names, index + 1, foundObjects);
                    }
                }
            }
        }
    }
}