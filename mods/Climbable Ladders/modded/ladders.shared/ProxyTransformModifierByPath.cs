using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tarkin.ladders.shared
{
    public class ProxyTransformModifierByPath : MonoBehaviour
    {
        public Mode mode = Mode.MoveTemporary;

        public enum Mode
        {
            MoveTemporary,
            MovePermanent
        }

        private struct TransformData
        {
            public Vector3 localPos;
            public Quaternion localRot;
            public Vector3 localScale;
        }

        private Dictionary<GameObject, TransformData> originalStates = new Dictionary<GameObject, TransformData>();

        void Start()
        {
            ApplyTransform();
        }

        void OnDestroy()
        {
            if (mode == Mode.MoveTemporary)
            {
                foreach (var kvp in originalStates)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.transform.localPosition = kvp.Value.localPos;
                        kvp.Key.transform.localRotation = kvp.Value.localRot;
                        kvp.Key.transform.localScale = kvp.Value.localScale;
                    }
                }
            }
        }

        private string HiearchyPathToString()
        {
            string fullPath = transform.name;

            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.parent == null) // look ahead, do not include the root game object in result
                    break;

                fullPath = parent.name + "/" + fullPath;

                parent = parent.parent;
            }

            return fullPath;
        }

        private void ApplyTransform()
        {
            string targetPath = HiearchyPathToString();
            List<GameObject> targetObjects = GameObjectDisablerByPath.FindObjectsByPath(targetPath);

            if (targetObjects.Count > 0)
            {
                GameObject target = targetObjects[0]; // take only the first match for now
                if (mode == Mode.MoveTemporary && !originalStates.ContainsKey(target))
                {
                    originalStates.Add(target, new TransformData
                    {
                        localPos = target.transform.position,
                        localRot = target.transform.rotation,
                        localScale = target.transform.localScale
                    });
                }

                target.transform.localPosition = this.transform.localPosition;
                target.transform.localRotation = this.transform.localRotation;
                target.transform.localScale = this.transform.localScale;
            }
            else
            {
                Debug.LogWarning($"[{nameof(ProxyTransformModifierByPath)}] Could not find original object to move at path: {targetPath}");
            }
        }
    }
}