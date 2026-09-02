using SAIN.Helpers;
using SAIN.Models.Preset.Personalities;

namespace SAIN.Preset.Personalities;

public class PersonalityManagerClass : BasePreset
{
    public PersonalityDictionary PersonalityDictionary = new();

    public PersonalityManagerClass(SAINPresetClass preset)
        : base(preset)
    {
        import();
        PersonalityDefaultsClass.InitDefaults(PersonalityDictionary, Preset);
    }

    public void Init()
    {
        foreach (var settings in PersonalityDictionary.Values)
        {
            settings.Init();
        }
    }

    // ref: AUD-18-01 - Proteção contra KeyNotFoundException usando TryGetValue
    public void UpdateDefaults(PersonalityManagerClass replacementClass = null)
    {
        foreach (var settings in PersonalityDictionary)
        {
            PersonalitySettingsClass replacementSettings = null;
            replacementClass?.PersonalityDictionary.TryGetValue(settings.Key, out replacementSettings);
            settings.Value.UpdateDefaults(replacementSettings);
        }
    }

    public void Update()
    {
        foreach (var settings in PersonalityDictionary.Values)
        {
            settings.Update();
        }
    }

    private void import()
    {
        if (!Preset.Info.IsCustom)
        {
            return;
        }

        foreach (var item in EnumValues.Personalities)
        {
            if (SAINPresetClass.Import(out PersonalitySettingsClass personality, Preset.Info.Name, item.ToString(), nameof(Personalities)))
            {
                PersonalityDictionary.Add(item, personality);
            }
        }
    }

    public void ResetAllToDefaults()
    {
        // ref: AUD-06-04 - Limpeza completa e dinamica de todas as personalidades
        PersonalityDictionary.Clear();
        PersonalityDefaultsClass.InitDefaults(PersonalityDictionary, Preset);
    }

    public void ResetToDefault(EPersonality personality)
    {
        PersonalityDictionary.Remove(personality);
        PersonalityDefaultsClass.InitDefaults(PersonalityDictionary, Preset);
    }
}
