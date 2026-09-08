using System;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using HarmonyLib;
using JetBrains.Annotations;
using SkillsExtended.Config.Skills;
using SkillsExtended.Skills.LockPicking;
using SkillsExtended.Skills.ProneMovement.Patches;
using SkillsExtended.Skills.WeaponSkills.Patches;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;

namespace SkillsExtended.Skills.Shared.Patches;


internal class OnGameStartedPatch : ModulePatch
{
    private static Type _stimType;
    private static Type _painKillerType;
    private static Type _medEffectType;

    private static WeaponSkillData NatoData => SkillsExtendedPlugin.SkillData.NatoWeapons;
    private static WeaponSkillData EasternData => SkillsExtendedPlugin.SkillData.EasternWeapons;

    // ref: AUD-01-14 — era private; OnGameEndedPatch (mesmo arquivo) precisa acessar pra desinscrever.
    [CanBeNull] internal static Player Player;
    
    protected override MethodBase GetTargetMethod()
    {
        var healthControllerType = PatchConstants.EftTypes.Single(t => t.Name is nameof(ActiveHealthController));
        var nestedTypes = healthControllerType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance);
        _stimType = nestedTypes.First(t => t.Name == "Stimulator");
        _painKillerType = nestedTypes.First(t => t.Name == "PainKiller");
        _medEffectType = nestedTypes.First(t => t.Name == "MedEffect");
        
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }
    
    [PatchPostfix]
    private static void Postfix(GameWorld __instance)
    {
        // Required to not break the headless, we don't need any of this there.
        if (!__instance.MainPlayer || SkillsExtendedInfo.IsFikaHeadless)
        {
            return;
        }
        
        LockPickingHelpers.InitializeLockpickingForLocation(__instance.LocationId);
        Player = __instance.MainPlayer;
        
#if DEBUG
        SkillsExtendedPlugin.Log.LogDebug($"Player map id: {Player.Location}");
#endif
        
        Player.ActiveHealthController.EffectStartedEvent += ApplyMedicalXp;
        
        if (SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled)
        {
            Player!.Skills.OnMasteringExperienceChanged += ApplyNatoRifleXp;
        }
        
        if (SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled)
        {
            Player!.Skills.OnMasteringExperienceChanged += ApplyEasternRifleXp;
        }

        FixDoors();

        // ref: PA-01-01 (review 01) — garante que o bônus de Ergonomia/Recuo já esteja ativo desde o
        // primeiro frame da raid, sem depender do jogador abrir uma tela de menu primeiro
        // (MenuTaskBar.OnScreenChanged não é garantido disparar nesse exato instante).
        UpdateWeaponsPatch.TriggerRaidStart();

#if DEBUG
        LogMissingDoors(__instance);
#endif
    }

    // ref: AUD-01-14 — internal (era private) para OnGameEndedPatch poder desinscrever.
    internal static void ApplyMedicalXp(IEffect effect)
    {
        // ref: PA-01-02 (review 01) — guard de nulidade explícito. Antes, Player! (null-forgiving sem
        // checagem real) era usado incondicionalmente aqui, sem cobrir o cenário de janela de transição
        // de cena (fim de raid) exigido pela spec funcional deste item. Casa com OnGameEndedPatch zerando
        // Player no fim da raid — qualquer invocação tardia/residual do evento vira um no-op seguro.
        if (Player == null)
        {
            return;
        }

        var skillMgrExt = Player.Skills.SkillManagerExtended;

        if (SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled && _stimType.IsInstanceOfType(effect) || _painKillerType.IsInstanceOfType(effect))
        {
            if (Player.Skills.FieldMedicine.IsEliteLevel)
            {
                return;
            }

            var xpGain = SkillsExtendedPlugin.SkillData.FieldMedicine.XpPerAction;

            Player.ExecuteSkill(() => skillMgrExt.FieldMedicineAction.Complete(xpGain));

#if DEBUG
            Logger.LogDebug("APPLYING FIELD MEDICINE XP");
#endif
            return;
        }

        if (SkillsExtendedPlugin.SkillData.FirstAid.Enabled && _medEffectType.IsInstanceOfType(effect))
        {
            // ref: AUD-01-12 — reusa o campo Player já validado em vez de GameUtils.GetPlayer()!
            // (chamada redundante e insegura — Player já é o mesmo MainPlayer que GetPlayer() resolveria).
            if (Player.Skills.FirstAid.IsEliteLevel)
            {
                return;
            }

            var xpGain = SkillsExtendedPlugin.SkillData.FirstAid.XpPerAction;

            Player.ExecuteSkill(() => skillMgrExt.FirstAidAction.Complete(xpGain));

#if DEBUG
            Logger.LogDebug("APPLYING FIRST AID XP");
#endif
        }
    }

    // ref: AUD-01-14 — internal (era private) para OnGameEndedPatch poder desinscrever.
    internal static void ApplyNatoRifleXp(MasterSkillClass skillClass)
    {
        if (Player!.Skills!.UsecArsystems.IsEliteLevel)
        {
            return;
        }
        
        var weaponInHand = Player.HandsController.GetItem();
        if (!NatoData.Weapons.Contains(weaponInHand.TemplateId))
        {
            return;
        }
        
        var skillMgrExt = Player.Skills.SkillManagerExtended;
        
        if (NatoData.SkillShareEnabled)
        {
            var xp = NatoData.XpPerAction * NatoData.SkillShareXpRatio;
            Player.ExecuteSkill(() => skillMgrExt.BearRifleAction.Complete(xp));
#if DEBUG 
            SkillsExtendedPlugin.Log.LogDebug($"APPLYING {xp} EASTERN RIFLE SHARED XP");
#endif
        }
        
        Player.ExecuteSkill(() => skillMgrExt.UsecRifleAction.Complete(NatoData.XpPerAction));
#if DEBUG
        SkillsExtendedPlugin.Log.LogDebug("APPLYING NATO RIFLE XP");
#endif
    }

    // ref: AUD-01-14 — internal (era private) para OnGameEndedPatch poder desinscrever.
    internal static void ApplyEasternRifleXp(MasterSkillClass skillClass)
    {
        if (Player!.Skills!.BearAksystems.IsEliteLevel)
        {
            return;
        }
        
        var weaponInHand = Player!.HandsController.GetItem();
        if (!EasternData.Weapons.Contains(weaponInHand.TemplateId))
        {
            return;
        }
        
        
        var skillMgrExt = Player.Skills.SkillManagerExtended;
        if (EasternData.SkillShareEnabled)
        {
            var xp = EasternData.XpPerAction * EasternData.SkillShareXpRatio;
            Player.ExecuteSkill(() => skillMgrExt.UsecRifleAction.Complete(xp));

#if DEBUG
            SkillsExtendedPlugin.Log.LogDebug($"APPLYING {xp} EASTERN RIFLE SHARED XP");
#endif
        }
        
        Player.ExecuteSkill(() => skillMgrExt.BearRifleAction.Complete(EasternData.XpPerAction));
        
#if DEBUG
        SkillsExtendedPlugin.Log.LogDebug($"APPLYING {EasternData.XpPerAction} EASTERN RIFLE XP");
#endif
    }

    private static void FixDoors()
    {
        var doors = LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<WorldInteractiveObject>();

        foreach (var door in doors)
        {
            // Fix Military checkpoint key because BSG cant assign a key to doors
            if (door.Id != "door_custom_multiScene_00000")
            {
                continue;
            }
                
            door.KeyId = "5913915886f774123603c392";
            break;
        }
    }

    private static void LogMissingDoors(GameWorld gameWorld)
    {
        foreach (var interactableObj in LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<WorldInteractiveObject>())
        {
            if (interactableObj.KeyId is null || interactableObj.KeyId == string.Empty)
            {
                continue;
            }

            var doorLevel = LockPickingHelpers.GetLevelForDoor(gameWorld.LocationId, interactableObj.KeyId);

            if (doorLevel != -1)
            {
                continue;
            }
            
            var logMessage = SkillsExtendedPlugin.Keys.KeyLocale.TryGetValue(interactableObj.KeyId, out var name) 
                ? $"Door ID: {interactableObj.Id} KeyID: {interactableObj.KeyId} Key Name: {name}" 
                : $"Door ID: {interactableObj.Id} KeyID: {interactableObj.KeyId}";
                
            SkillsExtendedPlugin.Log.LogError(logMessage);
        }
    }
}

// ref: AUD-01-14 — EFT/GameWorld.cs:2111 (OnDestroy, virtual; override confirmado em
// EFT/ClientGameWorld.cs:219-222 chama base.OnDestroy()). Desinscreve os 3 eventos de skill assinados por
// OnGameStartedPatch.Postfix e limpa os caches de arma de UpdateWeaponsPatch — sem isso, a referência ao
// Player da raid anterior (e o grafo inteiro que ela segura: inventário, armas, animator) só era liberada
// quando a próxima raid sobrescrevia o campo estático, não quando a raid realmente terminava.
internal class OnGameEndedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        if (OnGameStartedPatch.Player != null)
        {
            OnGameStartedPatch.Player.ActiveHealthController.EffectStartedEvent -= OnGameStartedPatch.ApplyMedicalXp;
            OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyNatoRifleXp;
            OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyEasternRifleXp;
            OnGameStartedPatch.Player = null;
        }

        UpdateWeaponsPatch.ClearRaidState();

        // ref: CR-01-01 (004 code-review 01) — evita reter o Player da raid anterior no cache de
        // delegate de XP de bruços até a próxima raid sobrescrever.
        ProneMoveStatePatch.ClearCachedAction();
    }
}