using System.Collections.Generic;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.Personalities;

public interface ISettingsGroup
{
    void Init();
    void Update();
    List<ISAINSettings> SettingsList { get; }
    void InitList();
    void CreateDefaults();
    void UpdateDefaults(ISettingsGroup replacementValues = null);
}
