using BepInEx;
using BepInEx.Logging;

namespace CustomClasses.Client;

/// <summary>
///     Item 005 (client) — escala o ganho de XP de skill conforme os multiplicadores da classe do perfil.
///     Busca os fatores do server (rota /customclasses/skill-multipliers) e faz Prefix em
///     AbstractSkillClass.OnTrigger. UI (linha+tooltip) vem na Fatia 2.
/// </summary>
[BepInPlugin("customclasses.mdj.client", "CustomClasses", "0.1.0")]
[BepInDependency("com.SPT.core", "4.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal enum Language { English, Portugues }   // item 008

    internal static ManualLogSource? Log;
    internal static bool Enabled = true;
    internal static bool ShowOnUi = true;
    internal static Language Lang = Language.English;   // item 008: idioma dos textos do mod na tela

    private void Awake()
    {
        Log = Logger;
        Enabled = Config.Bind(
            "General",
            "EnableSkillMultipliers",
            true,
            "Liga/desliga a escala de ganho de XP de skill por classe (CustomClasses).").Value;
        ShowOnUi = Config.Bind(
            "General",
            "ShowMultiplierOnSkills",
            true,
            "Mostra o destaque do multiplicador nas skills (borda colorida no ícone + seta ±X% ao lado do nome + tooltip da classe).").Value;
        Lang = Config.Bind(
            "General",
            "Language",
            Language.English,
            "Idioma dos textos do mod na tela (tooltip dos multiplicadores). / Language of the mod's in-game texts.").Value;

        new OnTriggerPatch().Enable();
        new WorkoutBehaviourPatch().Enable();   // (a) gym
        new SkillPanelPatch().Enable();         // (010) UI — marcador ±X% + tooltip dedicado da classe
        new SkillIconBorderPatch().Enable();    // (010) UI — borda colorida no ícone
        Log.LogInfo("[CustomClasses] client carregado (multiplicadores de skill).");
    }
}
