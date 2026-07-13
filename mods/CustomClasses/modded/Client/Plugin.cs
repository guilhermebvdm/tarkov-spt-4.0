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
[BepInPlugin("customclasses.mdj.client", "CustomClasses", "0.2.1")]
[BepInDependency("com.SPT.core", "4.0.0")]
[BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]   // (050.4 SAIN) carrega após o SAIN se presente
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
            PerksConfig.SecGeneral,
            "EnableSkillMultipliers",
            true,
            "Liga/desliga a escala de ganho de XP de skill por classe. / Enable per-class skill XP-gain scaling.").Value;
        ShowOnUi = Config.Bind(
            PerksConfig.SecGeneral,
            "ShowMultiplierOnSkills",
            true,
            "Mostra o destaque do multiplicador nas skills (borda colorida + seta ±X% + tooltip da classe). / Show the multiplier highlight on skills (colored border + ±X% arrow + class tooltip).").Value;
        ShowClassOnPlayerName = Config.Bind(
            PerksConfig.SecGeneral,
            "ShowClassOnPlayerName",
            true,
            "Aplica ícone + nome da classe no nome do jogador (deploy, character, lista online). / Apply class icon + name on the player's name.").Value;
        ShowClassIdentity = Config.Bind(
            PerksConfig.SecGeneral,
            "ShowClassIdentity",
            false,
            "Selo separado da classe no menu e no topo da tela de Skills (off por padrão — o nome do jogador já mostra). / Separate class seal in the menu and Skills screen (off by default).").Value;
        ShowSkillsButton = Config.Bind(
            PerksConfig.SecGeneral,
            "ShowSkillsButton",
            true,
            "Adiciona um botão SKILLS no menu (abaixo de CHARACTER) que abre a tela de Skills. / Add a SKILLS button to the menu.").Value;
        ShowLevelUpFlavor = Config.Bind(
            PerksConfig.SecGeneral,
            "ShowLevelUpFlavor",
            true,
            "Customiza a notificação de level-up (EASILY/FINALLY) das skills com multiplicador da classe. / Customize the skill level-up notification.").Value;
        // AcceptableValueRange → o ConfigurationManager (F12) renderiza como SLIDER (barra de arrastar), não input numérico.
        SkillsClassPosX = Config.Bind(PerksConfig.SecInterface, "SkillsClassPosX", 0f,
            new ConfigDescription("Selo da classe na tela de Skills — offset horizontal (px) do centro. 0 = centrado. / Skills screen class seal — horizontal offset (px) from center. 0 = centered.",
                new AcceptableValueRange<float>(-1000f, 1000f)));
        SkillsClassPosY = Config.Bind(PerksConfig.SecInterface, "SkillsClassPosY", -20f,
            new ConfigDescription("Selo da classe na tela de Skills — offset vertical (px) do topo. Negativo = para baixo. / Skills screen class seal — vertical offset (px) from top. Negative = down.",
                new AcceptableValueRange<float>(-1000f, 1000f)));
        // 006-fix (calibragem): tamanho do ícone = fontSize do nome de CADA tela × ratio → proporção ícone:fonte
        // idêntica em todas as telas (menu, OVERALL, deploy, confirmation), independente do tamanho da fonte.
        ClassIconRatio = Config.Bind(PerksConfig.SecInterface, "ClassIconRatio", 1.35f,
            new ConfigDescription("Tamanho do ícone da classe como múltiplo da fonte do nome de cada tela (mantém a proporção ícone:fonte). / Class icon size as a multiple of each screen's name font size (keeps the icon:font proportion consistent).",
                new AcceptableValueRange<float>(0.8f, 2.5f)));
        // (review CR-057F3-01) default era 3.0 calibrado ÀS CEGAS (o host antigo nunca disparava — a escala
        // nunca foi aplicada in-game). Agora que o host real (PartyPlayerItem) funciona, 1.2 = leve e seguro.
        DeployNameScale = Config.Bind(PerksConfig.SecInterface, "DeployNameScale", 1.2f,
            new ConfigDescription("Escala do ícone+nome do jogador na tela de deploy/loading (1.0 = original; ícone e nome crescem juntos). / Scale of the player icon+name on the raid loading (deploy) screen (1.0 = original).",
                new AcceptableValueRange<float>(1.0f, 4.0f)));
        // Real-time reposition when the F12 value changes (same pattern as Menu-Overhaul: SettingChanged event).
        SkillsClassPosX.SettingChanged += (_, _) => RepositionSeals();
        SkillsClassPosY.SettingChanged += (_, _) => RepositionSeals();

        PerksConfig.Bind(Config);   // item 050: F12 dos perks/drawbacks (fatia 050.0: Bulwark + Pack Mule)

        new OnTriggerPatch().Enable();
        new WorkoutBehaviourPatch().Enable();   // (a) gym
        new SkillPanelPatch().Enable();         // (010) UI — marcador ±X% + tooltip dedicado da classe
        new SkillIconBorderPatch().Enable();    // (010) UI — borda colorida no ícone
        new MenuClassIdentityPatch().Enable();              // (015) identidade no nome do jogador no menu (Menu-Overhaul)
        new SkillsScreenIdentityPatch().Enable();           // (012) selo da classe no topo da tela de Skills
        // (059) o overlay legado (SkillsPerksPanelPatch) foi removido — substituído pela aba CLASS nativa (abaixo).
        try
        {
            new SkillsClassTabPatch().Enable();             // (053) sub-aba CLASS (CLASS | SKILLS | MASTERING)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (053) aba CLASS não aplicada: {ex.Message}");
        }
        // (057 06-fix-03) O patch das ROWS do FIKA (ClassDetailLoadingPatch) foi DESATIVADO por decisão do
        // usuário: a lista inferior (progresso de carregamento do FIKA) não deve ser tocada. O host do
        // popover/identidade no deploy é a listagem de grupo SUPERIOR-esquerda (PartyPlayerItemPatch —
        // review CR-057F3-01: o RaidReadyPlayerPanel era código morto no SPT).
        new ChatSpecialIconPatch().Enable();                // (015/057) identidade no nome — deploy/chat/grupo (local + remotos)
        new PlayerModelWithStatsIdentityPatch().Enable();   // (015) identidade no nome — tela de character (OVERALL)
        new PlayerNamePanelPatch().Enable();                // (015) identidade no nome — confirmation (PlayerNamePanel)
        new PartyPlayerItemPatch().Enable();                // (015/057) escala + popover no cursor — host REAL do deploy
        new PartyInfoPanelPrefetchPatch().Enable();         // (057) caches frescos por tela de deploy (classe local + mapa)
        new SkillsNavButtonPatch().Enable();                // (013) botão SKILLS no menu → abre a aba Skills
        new BulwarkPatch().Enable();                        // (050.0/B6) 🛡️ Tanque — dano recebido ×0.85 SÓ com colete pesado
        new PackMulePatch().Enable();                       // (050.0) 🎒🛡️ Pack Mule — +30% limite de carga (piso, stash+raid)
        new QuickHandsPatch().Enable();                     // (061) 🎒 Saqueador — revista 2 contêineres (bônus elite da Search, antecipado)
        new WeightMarkerPatch().Enable();                   // (056) marcador "▲ +X%" no peso (aba Health) — atribui ao Pack Mule
        new RaidPerksNotificationPatch().Enable();          // (050) notificação de perks/drawbacks no início da raid
        new NotificationDurationPatch().Enable();           // (fix 2026-07-03) notificação de perks por 10s (Infinite + hide agendado)
        StancesArmStaminaBridge.TryAttach();                // (051) hook de stamina de braço no stances (re-try no raid-start)
        new UnderbarrelMasteryXpPatch().Enable();           // (058) XP de Underbarrel Launchers por disparo do GP-25/M203
        new WeaponMasteryRecoilPatch().Enable();            // (058) recuo × (1 − rec/nível) — ANTES do ShootRecoilPatch (ordem intencional
                                                            //       + HarmonyPriority.High: maestria entra no baseline do PerkDiag — CR-01-03)
        new WeaponMasteryErgoPatch().Enable();              // (058) ergo × (1 + ergo/nível) pela maestria da arma em mãos
        // (050.1 fix 2026-06-24) MaxSpeedPatch/SprintSpeedPatch (getters) REMOVIDOS:
        //   - MaxSpeed é só TETO/denominador da razão RelativeSpeed = CharSpeed/MaxSpeed → aplicar no getter E no
        //     SetCharacterMovementSpeed CANCELAVA o efeito na razão. Agora só o driver real é patchado (abaixo).
        //   - SprintSpeed getter é DEAD (sprint usa StateSprintSpeedLimit/Physical.SprintSpeed, caminho separado).
        new OverladenInertiaPatch().Enable();               // (050.1) 🔻 Saqueador — inércia ∝ peso
        try
        {
            new SetCharacterMovementSpeedPatch().Enable();  // (050.1 fix) driver REAL da velocidade de ANDAR
            new SprintAccelerationPatch().Enable();         // (050.1 fix) driver da velocidade de CORRER (sprint)
            new AiSoundPatch().Enable();                    // (050.4 fix) audibilidade PARA A IA (BotEventHandler.PlaySound)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (fix vel/som) patches falharam ao aplicar: {ex.Message}");
        }
        // (050.4 SAIN) Ghost Step/Loud Operator/Silent Looter pelo pipeline REAL que os bots ouvem — só se SAIN presente.
        if (HarmonyLib.AccessTools.TypeByName("SAIN.Components.PlayerComponentSpace.PlayerComponent") != null)
        {
            try
            {
                new SainSoundPatch().Enable();
                Log.LogInfo("[CustomClasses] SAIN detectado — sons de perk pelo pipeline do SAIN.");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[CustomClasses] (050.4 SAIN) SainSoundPatch falhou ao aplicar: {ex.Message}");
            }
        }
        // B15 — piso COMBINADO de recuo: os 2 patches CERCAM a cadeia (Capture = Priority.First, ANTES da
        // maestria; Apply = Priority.Last, DEPOIS de todos os multiplicadores). A ordem real é imposta pelos
        // [HarmonyPriority], não pela ordem destes Enable().
        new RecoilFloorCapturePatch().Enable();             // (B15) guarda o `str` original do tiro
        new RecoilFloorApplyPatch().Enable();               // (B15) clampa o produto maestria × perks no piso
        new ShootRecoilPatch().Enable();                    // (050.2) 🔻 Médico — recuo ×1.25 (Shaky Hands) + Adrenaline ×0.7
        new AimPunchPatch().Enable();                       // (050.2) 🔻 Furtivo — aim-punch ×1.5 (Rattled)
        new LocalHitTypePatch().Enable();                   // (review) captura tipo do dano local (barra aim-punch em queda)
        try
        {
            new AdrenalineTriggerPatch().Enable();          // (050.2) 🔧 Fuzileiro — gatilho da Adrenaline (dano dado/recebido)
            new ReloadSpeedPatch().Enable();                // (050.2) 🔧 Adrenaline — recarga mais rápida na janela
            new AdsSpeedPatch().Enable();                   // (050.2) 🔧 Adrenaline — ADS mais rápido (injeção de campo _aimingSpeed)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (050.2) Adrenaline patches falharam ao aplicar: {ex.Message}");
        }
        new ExecutionMeleePatch().Enable();                 // (050.3) 🔧 Furtivo — dano de melee ×3.5 (B7)
        new MalfunctionChancePatch().Enable();              // (050.3) 🔧 Fuzileiro — anti-jam ×0.5 (Cool Under Fire)
        new InteractionSoundPatch().Enable();               // (050.4) 🔧 Saqueador — loot mais silencioso (Silent Looter)
        try
        {
            new SoundRadiusPatch().Enable();                // (050.4) 🔧🔻 Ghost Step / Loud Operator — raio de som (method_67 obfuscado)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (050.4) SoundRadiusPatch falhou ao aplicar: {ex.Message}");
        }
        // 050.4b — Bunker recuo (branch no ShootRecoilPatch já ligado) + Sharpshooter ADS (branch no AdsSpeedPatch já ligado)
        try
        {
            new HeavyWeaponErgoPatch().Enable();            // (050.4b) 🔧 Tanque — +ergo arma pesada (Bunker)
            new IronLungsPatch().Enable();                  // (050.4b, lever corrigido) 🔧 Caçador — fôlego +longo (BaseHoldBreathConsumption)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (050.4b) Bunker ergo / Iron Lungs falharam ao aplicar: {ex.Message}");
        }
        // (072) Perks deferidos no 050 — try/catch PRÓPRIO: os alvos do Médico moram em tipos aninhados/obfuscados
        // (Player.MedsController.ObservedMedsControllerClass) e em EFT.HealthSystem; se um deles sumir numa build
        // futura do EFT, o mod perde SÓ estes perks, em vez de derrubar toda a cadeia de patches.
        try
        {
            new CalmSightsPatch().Enable();                 // (072) 🔧 Caçador — sway ×0.7 (UpdateSwayFactors)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (072) Calm Sights não aplicado: {ex.Message}");
        }
        // ⚠️ CR-F6 — o TRIO de tempo do Médico (escopo + efeito + animação) vive ou morre JUNTO. Num try/catch
        // compartilhado com os outros, se o MedUseTimePatch entrasse e o MedAnimSpeedPatch falhasse, o jogador
        // ficaria com o efeito encurtado e a animação em velocidade vanilla — o pior dos dois mundos. Aqui, se
        // qualquer um dos três falhar, DESLIGAMOS o perk inteiro (o escopo nunca arma → caminho vanilla intacto).
        try
        {
            new MedsOperationScopePatch().Enable();         // (072) 🩺 arma o escopo do method_5 do SEU player
            new MedUseTimePatch().Enable();                 // (072) 🔧 Rapid Care / Swift Surgeon — o EFEITO
            new MedAnimSpeedPatch().Enable();               // (072) 🔧 ...e a ANIMAÇÃO, casada com ele
        }
        catch (System.Exception ex)
        {
            MedicTiming.ForceDisable();                     // efeito sem animação (ou vice-versa) é pior que nada
            Log.LogError($"[CustomClasses] (072) Rapid Care / Swift Surgeon DESLIGADOS (patch incompleto): {ex.Message}");
        }
        try
        {
            new MobileSurgeryPatch().Enable();              // (072) 🔧 Médico — cirurgia andando (HealingLegs off)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (072) Mobile Surgery não aplicado: {ex.Message}");
        }
        try
        {
            new ChangeEnergyPatch().Enable();               // (050.3) 🔻 Tanque — fome drena ×1.3 (Heavy Frame)
            new ChangeHydrationPatch().Enable();            // (050.3) 🔻 Tanque — sede drena ×1.3 (Heavy Frame)
        }
        catch (System.Exception ex)
        {
            Log.LogError($"[CustomClasses] (050.3) Heavy Frame metabolism patches falharam ao aplicar: {ex.Message}");
        }
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

    private void OnGUI()
    {
        PerkDiagnostics.Draw();   // (052) overlay "super espião" — só desenha se o toggle F12 estiver on
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
