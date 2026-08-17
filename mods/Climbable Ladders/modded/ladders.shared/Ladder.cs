#if EFT_RUNTIME
using EFT.Interactive;
#endif

using System.Collections.Generic;
using UnityEngine;

namespace tarkin.ladders.shared
{
    [RequireComponent(typeof(BoxCollider))]
    public class Ladder :
#if EFT_RUNTIME
        InteractableObject
#else
        MonoBehaviour
#endif
    {
        public static bool TryGetLadderInstanceByNetId(string netId, out Ladder ladder) => registry.TryGetValue(netId, out ladder);
        private static Dictionary<string, Ladder> registry = new Dictionary<string, Ladder>();

        public string NetId => gameObject.name;
        [field: SerializeField, Min(1)] public int RungCount { get; private set; } = 10;
        [field: SerializeField] public float RungSpacing { get; private set; } = 0.5f;
        [field: SerializeField] public float Width { get; private set; } = 0.8f;

        public float MaxHeight => RungSpacing * RungCount;

        void Awake()
        {
            registry[NetId] = this;
        }

        void OnDestroy()
        {
            registry.Remove(NetId);
        }

#if EFT_RUNTIME
        private static readonly Collider[] overlapCols = new Collider[1];
        public bool IsSurfaceSoundIdentified { get; private set; }
        public void TryIdentifySurfaceSound(Vector3 contactPoint)
        {
            if (Physics.OverlapSphereNonAlloc(contactPoint, 0.1f, overlapCols, 1 << 12) > 0)
            {
                if (overlapCols[0].TryGetComponent(out EFT.Ballistics.BallisticCollider ballistic))
                {
                    IsSurfaceSoundIdentified = true;
                    SurfaceSound = ballistic.SurfaceSound;
                }
            }
        }
        public BaseBallistic.ESurfaceSound SurfaceSound { get; private set; } = BaseBallistic.ESurfaceSound.MetalThin;
#endif


#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.layer = 22; //Interactive

            UnityEditor.EditorApplication.delayCall -= UpdateInteractiveCollider;
            UnityEditor.EditorApplication.delayCall += UpdateInteractiveCollider;

            CheckUniqueId();
        }

        [ContextMenu(nameof(GenerateUniqueName))]
        private void GenerateUniqueName()
        {
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            Ladder[] allLadders = FindObjectsOfType<Ladder>();
            string prefix = BuildHierarchyPrefix(gameObject);

            HashSet<string> usedNames = new HashSet<string>();
            foreach (Ladder l in allLadders)
            {
                if (l != this)
                    usedNames.Add(l.gameObject.name);
            }

            for (int i = 0; i < 100000; i++)
            {
                string candidate = prefix + i.ToString("D5");
                if (!usedNames.Contains(candidate))
                {
                    UnityEditor.Undo.RecordObject(gameObject, "Generate Unique Ladder Name");

                    gameObject.name = candidate;
                    break;
                }
            }
        }

        [ContextMenu("Force check for unique name")]
        void CheckUniqueId()
        {
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            Ladder[] allLadders = FindObjectsOfType<Ladder>();

            foreach (Ladder other in allLadders)
            {
                if (other != this && other.gameObject.name == gameObject.name)
                {
                    Debug.LogError($"Duplicate ladder name found: {gameObject.name}", this);
                    break;
                }
            }
        }

        static string BuildHierarchyPrefix(GameObject go)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // Scene name
            if (!string.IsNullOrEmpty(go.scene.name))
            {
                sb.Append(go.scene.name.Replace(" ", "_"));
                sb.Append("_");
            }

            // Collect parents all the way to the root
            List<string> parents = new List<string>();
            Transform curr = go.transform.parent;
            while (curr != null)
            {
                parents.Add(curr.name.Replace(" ", "_"));
                curr = curr.parent;
            }

            // Reverse so it goes Root -> Child order
            parents.Reverse();

            foreach (string p in parents)
            {
                sb.Append(p);
                sb.Append("_");
            }

            // Clean up any double underscores in case objects have weird names
            string result = sb.ToString();
            result = System.Text.RegularExpressions.Regex.Replace(result, @"_+", "_");

            // Guarantee it ends with an underscore so the appended index format is correct
            if (!result.EndsWith("_"))
                result += "_";

            return result;
        }

        private void UpdateInteractiveCollider()
        {
            if (this == null || gameObject == null) return;

            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null) return;

            float requiredHeight = MaxHeight;
            float requiredWidth = Width;
            const float depth = 0.3f;
            const float frontOffset = 0.2f;

            Vector3 requiredSize = new Vector3(requiredWidth, requiredHeight, depth);
            Vector3 requiredCenter = new Vector3(0, requiredHeight * 0.5f, frontOffset);

            boxCollider.size = new Vector3(
                Mathf.Max(boxCollider.size.x, requiredSize.x),
                Mathf.Max(boxCollider.size.y, requiredSize.y),
                Mathf.Max(boxCollider.size.z, requiredSize.z)
            );

            boxCollider.center = new Vector3(
                requiredCenter.x,
                Mathf.Max(boxCollider.center.y, requiredCenter.y),
                Mathf.Max(boxCollider.center.z, requiredCenter.z)
            );

            boxCollider.isTrigger = true;
        }

        private void OnDrawGizmos()
        {
            Vector3 origin = transform.position;
            Quaternion rotation = transform.rotation;
            Vector3 localRight = rotation * Vector3.right;

            // Draw rungs
            Gizmos.color = Color.blue;
            for (int i = 0; i < RungCount; i++)
            {
                Vector3 center = origin + rotation * new Vector3(0, RungSpacing, 0) * i;
                Vector3 left = center - localRight * (Width * 0.5f);
                Vector3 right = center + localRight * (Width * 0.5f);

                Gizmos.DrawLine(left, right);
            }

            // Draw side rails
            if (RungCount > 1)
            {
                Vector3 firstCenter = origin;
                Vector3 lastCenter = origin + rotation * (new Vector3(0, RungSpacing, 0) * (RungCount - 1));

                Vector3 firstLeft = firstCenter - localRight * (Width * 0.5f);
                Vector3 firstRight = firstCenter + localRight * (Width * 0.5f);
                Vector3 lastLeft = lastCenter - localRight * (Width * 0.5f);
                Vector3 lastRight = lastCenter + localRight * (Width * 0.5f);

                Gizmos.DrawLine(firstLeft, lastLeft);
                Gizmos.DrawLine(firstRight, lastRight);
            }
        }
#endif
    }
}
