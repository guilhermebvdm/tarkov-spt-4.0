using System.Collections.Generic;
using Newtonsoft.Json;
using SAIN.Models.Preset.Personalities;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.Personalities;

public class PersonalitySettingsClass : SettingsGroupBase<PersonalitySettingsClass>
{
    [JsonConstructor]
    public PersonalitySettingsClass() { }

    [JsonIgnore]
    public static Dictionary<EPersonality, string> PersonalityDescriptions { get; } =
        new Dictionary<EPersonality, string>
        {
            { EPersonality.Normal, "An Average Tarkov Enjoyer" },
            { EPersonality.GigaChad, "A true alpha threat. Hyper Aggressive and typically wearing high tier equipment." },
            {
                EPersonality.Wreckless,
                "This personality tends to sprint at their enemies, and will very frequently scream at everyone - Usually both at the same time. More Aggressive than Gigachads."
            },
            {
                EPersonality.SnappingTurtle,
                "A player who finds the balance between rat and chad, yin and yang. Will rat you out but can spring out at any moment."
            },
            { EPersonality.Chad, "An aggressive player. Typically wearing high tier equipment, and is more aggressive than usual." },
            { EPersonality.Rat, "Scum of Tarkov. Rarely Seeks out enemies, and when they do - they will crab walk all the way there" },
            { EPersonality.Timmy, "A New Player, terrified of everything." },
            {
                EPersonality.Coward,
                "A player who is more passive and afraid than usual. Will never seek out enemies and will hide in a closet until the scary thing goes away."
            },
        };

    public PersonalitySettingsClass(EPersonality personality)
    {
        Name = personality.ToString();
        Description = PersonalityDescriptions[personality];
    }

    public string Name;
    public string Description;

    public PersonalityAssignmentSettings Assignment = new();
    public PersonalityBehaviorSettings Behavior = new();
    public DifficultySettings Difficulty = new();

    public override void Init()
    {
        InitList();
        CreateDefaults();
        Behavior.Init();
        Update();
    }

    public override void InitList()
    {
        SettingsList.Clear();
        SettingsList.Add(Assignment);
        SettingsList.Add(Difficulty);
    }
}
