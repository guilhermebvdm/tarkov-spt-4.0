using System.Collections.Generic;
using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class LookSettings : SAINSettingsBase<LookSettings>, ISAINSettings
{
    [Category("Core Settings")]
    [Name("Vision Speed Settings")]
    public VisionSpeedSettings VisionSpeed = new();

    [Category("Core Settings")]
    [Name("Vision Distance Settings")]
    public VisionDistanceSettings VisionDistance = new();

    [Category("Core Settings")]
    [Name("Time Settings")]
    public TimeSettings Time = new();

    [Category("Core Settings")]
    [Name("Flashlights and NVGs Settings")]
    public LightNVGSettings Light = new();

    [Category("Extra")]
    [Name("Not Looking At Bot Settings")]
    public NotLookingSettings NotLooking = new();

    public override void Init(List<ISAINSettings> list)
    {
        VisionSpeed.Init(list);
        list.Add(VisionDistance);
        list.Add(NotLooking);
        list.Add(Time);
        list.Add(Light);
    }
}
