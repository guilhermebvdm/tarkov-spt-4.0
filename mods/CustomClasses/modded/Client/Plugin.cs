using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

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
    internal static Plugin? Instance;   // item 012: host de coroutine (BaseUnityPlugin é MonoBehaviour)
    internal static ManualLogSource? Log;
    internal static bool Enabled = true;
    internal static bool ShowOnUi = true;
    internal static bool ShowClassIdentity;   // item 012: selo separado no menu/Skills — OFF por padrão (item 015: usar ChatSpecialIcon)
    internal static bool ShowClassOnPlayerName = true;   // item 015: identidade da classe no nome do jogador (deploy/character/online)
    internal static bool ShowSkillsButton = true;   // item 013: botão SKILLS no menu
    internal static bool ShowLevelUpFlavor = true;   // item 014: notificação de level-up (EASILY/FINALLY)
    // item 012: posição (offset X/Y) do selo da classe na tela de Skills — ConfigEntry (lido em tempo real).
    internal static ConfigEntry<float>? SkillsClassPosX;     // 0 = centralizado horizontalmente na tela de Skills
    internal static ConfigEntry<float>? SkillsClassPosY;
    internal static ConfigEntry<float>? ClassIconRatio;     // item 015/006-fix: tamanho do ícone = fontSize do nome × ratio (proporção consistente)
    internal static ConfigEntry<float>? DeployNameScale;    // item 015: escala do ícone+nome na tela de deploy

    private void Awake()
    {
        Log = Logger;
        Instance = this;   // item 012: para hospedar coroutines de UI do menu
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
        ShowClassOnPlayerName = Config.Bind(
            "General",
            "ShowClassOnPlayerName",
            true,
            "Aplica ícone + nome da classe no nome do jogador (deploy, character, lista online). / Apply class icon + name on the player's name.").Value;
        ShowClassIdentity = Config.Bind(
            "General",
            "ShowClassIdentity",
            false,
            "Selo separado da classe no menu e no topo da tela de Skills (off por padrão — o nome do jogador já mostra). / Separate class seal in the menu and Skills screen (off by default).").Value;
        ShowSkillsButton = Config.Bind(
            "General",
            "ShowSkillsButton",
            true,
            "Adiciona um botão SKILLS no menu (abaixo de CHARACTER) que abre a tela de Skills. / Add a SKILLS button to the menu.").Value;
        ShowLevelUpFlavor = Config.Bind(
            "General",
            "ShowLevelUpFlavor",
            true,
            "Customiza a notificação de level-up (EASILY/FINALLY) das skills com multiplicador da classe. / Customize the skill level-up notification.").Value;
        // AcceptableValueRange → o ConfigurationManager (F12) renderiza como SLIDER (barra de arrastar), não input numérico.
        SkillsClassPosX = Config.Bind("Class identity position", "SkillsClassPosX", 0f,
            new ConfigDescription("Skills screen class seal — horizontal offset (px) from center. 0 = centered.",
                new AcceptableValueRange<float>(-1000f, 1000f)));
        SkillsClassPosY = Config.Bind("Class identity position", "SkillsClassPosY", -20f,
            new ConfigDescription("Skills screen class seal — vertical offset (px) from top. Negative = down.",
                new AcceptableValueRange<float>(-1000f, 1000f)));
        // 006-fix (calibragem): tamanho do ícone = fontSize do nome de CADA tela × ratio → proporção ícone:fonte
        // idêntica em todas as telas (menu, OVERALL, deploy, confirmation), independente do tamanho da fonte.
        ClassIconRatio = Config.Bind("Class identity position", "ClassIconRatio", 1.35f,
            new ConfigDescription("Class icon size as a multiple of each screen's name font size (icon = nameFontSize × ratio). Keeps the icon:font proportion consistent across screens.",
                new AcceptableValueRange<float>(0.8f, 2.5f)));
        DeployNameScale = Config.Bind("Class identity position", "DeployNameScale", 3.0f,
            new ConfigDescription("Scale of the player icon+name on the raid loading (deploy) screen (1.0 = original). Icon and name grow together (same proportion).",
                new AcceptableValueRange<float>(1.0f, 4.0f)));
        // Real-time reposition when the F12 value changes (same pattern as Menu-Overhaul: SettingChanged event).
        SkillsClassPosX.SettingChanged += (_, _) => RepositionSeals();
        SkillsClassPosY.SettingChanged += (_, _) => RepositionSeals();

        new OnTriggerPatch().Enable();
        new WorkoutBehaviourPatch().Enable();   // (a) gym
        new SkillPanelPatch().Enable();         // (010) UI — marcador ±X% + tooltip dedicado da classe
        new SkillIconBorderPatch().Enable();    // (010) UI — borda colorida no ícone
        new MenuClassIdentityPatch().Enable();              // (015) identidade no nome do jogador no menu (Menu-Overhaul)
        new SkillsScreenIdentityPatch().Enable();           // (012) selo da classe no topo da tela de Skills
        new ChatSpecialIconPatch().Enable();                // (015) identidade no nome — deploy/chat/grupo (ChatSpecialIcon)
        new PlayerModelWithStatsIdentityPatch().Enable();   // (015) identidade no nome — tela de character (OVERALL)
        new PlayerNamePanelPatch().Enable();                // (015) identidade no nome — confirmation (PlayerNamePanel)
        new RaidReadyPlayerPanelPatch().Enable();           // (015) aumenta ícone+nome na tela de deploy
        new SkillsNavButtonPatch().Enable();                // (013) botão SKILLS no menu → abre a aba Skills
        if (SkillLevelUpNotificationPatch.CanEnable)        // (014) notificação de level-up (EASILY/FINALLY)
        {
            new SkillLevelUpNotificationPatch().Enable();
        }
        else
        {
            Log.LogWarning("[CustomClasses] (014) tipo da notificação de skill não resolvido — flavor desativado.");
        }
        Log.LogInfo("[CustomClasses] client carregado (multiplicadores de skill).");
    }

    private void OnDestroy()
    {
        ClassIconCache.Dispose();   // item 011: libera sprites/texturas dos ícones (evita leak de VRAM)
        MenuOverhaulBridge.RestoreAccent();   // item 015: devolve a cor original do Menu-Overhaul
    }

    /// <summary>
    ///     Item 012: reposiciona o selo da classe na tela de Skills em tempo real quando o offset X/Y muda no F12.
    ///     Acha o selo pelo nome (só existe quando a tela está aberta) e reaplica a anchoredPosition.
    /// </summary>
    private static void RepositionSeals()
    {
        var skills = GameObject.Find("CC_ClassSeal_Skills");
        if (skills != null)
        {
            ((RectTransform)skills.transform).anchoredPosition = new Vector2(SkillsClassPosX?.Value ?? 0f, SkillsClassPosY?.Value ?? 0f);
        }
    }
}
