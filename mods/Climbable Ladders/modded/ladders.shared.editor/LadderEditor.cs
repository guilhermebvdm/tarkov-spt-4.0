using UnityEditor;
using UnityEngine;

namespace tarkin.ladders.shared.editor
{
    [CustomEditor(typeof(Ladder))]
    public class LadderEditor : Editor
    {
        private SerializedProperty _rungCountProp;
        private SerializedProperty _rungSpacingProp;
        private SerializedProperty _widthProp;

        private void OnEnable()
        {
            _rungCountProp = serializedObject.FindProperty("<RungCount>k__BackingField");
            _rungSpacingProp = serializedObject.FindProperty("<RungSpacing>k__BackingField");
            _widthProp = serializedObject.FindProperty("<Width>k__BackingField");
        }

        private void OnSceneGUI()
        {
            Ladder ladder = (Ladder)target;
            Transform ladderTransform = ladder.transform;

            Handles.matrix = ladderTransform.localToWorldMatrix;

            float currentWidth = _widthProp.floatValue;
            Vector3 rightHandlePos = new Vector3(currentWidth * 0.5f, ladder.RungSpacing, 0);

            Handles.color = Color.yellow;

            EditorGUI.BeginChangeCheck();

            Vector3 newRightHandlePos = Handles.Slider(rightHandlePos, Vector3.right, HandleUtility.GetHandleSize(rightHandlePos) * 0.1f, Handles.CubeHandleCap, 0);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ladder, "Change Ladder Width");

                _widthProp.floatValue = Mathf.Max(0.1f, newRightHandlePos.x * 2);
            }

            Handles.Label(rightHandlePos + Vector3.right * 0.1f, "Width");


            float currentSpacing = _rungSpacingProp.floatValue;
            Vector3 spacingHandlePos = new Vector3(0, currentSpacing, 0);

            Handles.color = Color.cyan;

            EditorGUI.BeginChangeCheck();

            Vector3 newSpacingHandlePos = Handles.Slider(spacingHandlePos, Vector3.up, HandleUtility.GetHandleSize(spacingHandlePos) * 0.1f, Handles.CubeHandleCap, 0);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ladder, "Change Ladder Spacing");

                _rungSpacingProp.floatValue = Mathf.Max(0.1f, newSpacingHandlePos.y);
            }

            Handles.Label(spacingHandlePos + Vector3.up * 0.1f, "Spacing");


            int currentRungCount = _rungCountProp.intValue;
            Vector3 topHandlePos = new Vector3(0, currentRungCount * ladder.RungSpacing, 0);

            Handles.color = Color.magenta;

            EditorGUI.BeginChangeCheck();

            Vector3 newTopHandlePos = Handles.Slider(topHandlePos, Vector3.up, HandleUtility.GetHandleSize(topHandlePos) * 0.1f, Handles.CubeHandleCap, 0);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(ladder, "Change Ladder Rung Count");

                if (ladder.RungSpacing > 0.01f)
                {
                    _rungCountProp.intValue = Mathf.Max(1, Mathf.RoundToInt(newTopHandlePos.y / ladder.RungSpacing));
                }
            }

            Handles.Label(topHandlePos + Vector3.up * 0.1f, "Height");

            serializedObject.ApplyModifiedProperties();
        }
    }
}