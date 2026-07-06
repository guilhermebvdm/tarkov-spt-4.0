using System;

namespace SAIN.Components;

[Flags]
public enum DeviceMode
{
    None = 0,
    WhiteLight = 1,
    VisibleLaser = 2,
    IRLight = 4,
    IRLaser = 8,
}
