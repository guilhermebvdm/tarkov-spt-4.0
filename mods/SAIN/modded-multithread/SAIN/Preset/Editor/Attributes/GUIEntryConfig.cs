using SAIN.Plugin;
using UnityEngine;

namespace SAIN.Attributes;

public class GUIEntryConfig
{
    private const float TARGT_WIDTH_SCALE = 1920;

    public float EntryHeight
    {
        get { return PresetHandler.EditorDefaults.ConfigEntryHeight; }
    }

    public float SliderWidth
    {
        get { return PresetHandler.EditorDefaults.ConfigSliderWidth; }
    }

    public float ResultWidth
    {
        get { return PresetHandler.EditorDefaults.ConfigResultsWidth; }
    }

    public float ResetWidth
    {
        get { return PresetHandler.EditorDefaults.ConfigResetWidth; }
    }

    public float SubList_Indent_Vertical
    {
        get { return PresetHandler.EditorDefaults.SubList_Indent_Vertical; }
    }

    public float SubList_Indent_Horizontal
    {
        get { return PresetHandler.EditorDefaults.SubList_Indent_Horizontal; }
    }

    public GUILayoutOption[] Toggle
    {
        get { return Params(SliderWidth); }
    }

    public GUILayoutOption[] Result
    {
        get { return Params(ResultWidth); }
    }

    public GUILayoutOption[] Reset
    {
        get { return Params(ResetWidth); }
    }

    private GUILayoutOption[] Params(float width0to1)
    {
        return [GUILayout.Width(width0to1 * TARGT_WIDTH_SCALE), GUILayout.Height(EntryHeight)];
    }
}
