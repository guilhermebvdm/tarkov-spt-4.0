using System.Collections;
using System.Collections.Generic;
using EFT.UI;
using EFT.UI.Screens;
using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Models;
using SkillsExtended.Skills.Core;
using SkillsExtended.Utils;

namespace SkillsExtended.Skills.WeaponSkills.Patches;

internal class UpdateWeaponsPatch : ModulePatch
{
    // Store an object containing the weapons original stats.
    internal static readonly Dictionary<string, OrigWeaponValues> UsecOriginalWeaponValues = [];
    internal static readonly Dictionary<string, int> UsecWeaponInstanceIds = [];

    internal static readonly Dictionary<string, OrigWeaponValues> EasternOriginalWeaponValues = [];
    internal static readonly Dictionary<string, int> EasternWeaponInstanceIds = [];

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MenuTaskBar), nameof(MenuTaskBar.OnScreenChanged));
    }

    [PatchPrefix]
    public static void Prefix(EEftScreenType eftScreenType)
    {
        // ref: AUD-01-09 — gate por raid; sem isso o processamento inteiro rodava em qualquer tela de menu,
        // inclusive fora de raid. `.Clear()` removido: o dirty-tracking por nível já existe no corpo da
        // coroutine (skip de instâncias já ajustadas no nível atual), então forçar reprocesso total a cada
        // troca de tela era desnecessário.
        if (!GameUtils.IsInRaid())
        {
            return;
        }

        TriggerRaidStart();
    }

    // ref: PA-01-01 (review 01) — extraído do Prefix pra também poder ser chamado por
    // OnGameStartedPatch.Postfix, garantindo que o bônus já esteja ativo desde o início da raid,
    // sem depender do jogador abrir uma tela de menu primeiro.
    internal static void TriggerRaidStart()
    {
        if (SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled)
        {
            StaticManager.BeginCoroutine(UpdateUsecWeapons());
        }

        if (SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled)
        {
            StaticManager.BeginCoroutine(UpdateEasternWeapons());
        }
    }

    // ref: AUD-01-14 — chamado pelo OnGameEndedPatch (OnGameStarted.cs) no Postfix de GameWorld.OnDestroy.
    // Os dicionários de "já processado nesta raid" não fazem sentido guardados de uma raid pra outra,
    // já que o `.Clear()` incondicional do Prefix foi removido acima.
    internal static void ClearRaidState()
    {
        UsecOriginalWeaponValues.Clear();
        UsecWeaponInstanceIds.Clear();
        EasternOriginalWeaponValues.Clear();
        EasternWeaponInstanceIds.Clear();
    }
    
    private static IEnumerator UpdateUsecWeapons()
    {
        // ref: AUD-01-04 — GetProfile resolvido uma vez (não muda dentro do loop), mas SkillManager
        // é re-checado a CADA iteração abaixo, já que a coroutine atravessa vários frames.
        var profile = GameUtils.GetProfile(GameUtils.IsScav() ? EPlayerSide.Savage : EPlayerSide.Usec);
        if (profile == null)
        {
            yield break;
        }

        var natoWeapons = SkillsExtendedPlugin.SkillData.NatoWeapons;
        var weapons = profile.Inventory.AllRealPlayerItems
            .Where(x => natoWeapons.Weapons.Contains(x.TemplateId));

        foreach (var item in weapons)
        {
            if (item is not Weapon weapon) continue;

            // Re-checa a cada iteração — o jogador pode sair de raid no meio do processo (AUD-01-04).
            var skillManager = GameUtils.GetSkillManager();
            if (skillManager == null)
            {
                yield break;
            }

            // Store the weapons original values
            if (!UsecOriginalWeaponValues.ContainsKey(weapon.TemplateId))
            {
                var origVals = new OrigWeaponValues
                {
                    weaponUp = weapon.Template.RecoilForceUp,
                    weaponBack = weapon.Template.RecoilForceBack
                };

#if DEBUG
                SkillsExtendedPlugin.Log.LogDebug($"original {weapon.LocalizedName()} ergo: {weapon.Template.Ergonomics}, up {weapon.Template.RecoilForceUp}, back {weapon.Template.RecoilForceBack}");
#endif
                UsecOriginalWeaponValues.Add(item.TemplateId, origVals);
            }

            //Skip instances of the weapon that are already adjusted at this level.
            if (UsecWeaponInstanceIds.ContainsKey(item.Id))
            {
                if (UsecWeaponInstanceIds[item.Id] == skillManager.UsecArsystems.Level)
                {
                    continue;
                }

                UsecWeaponInstanceIds.Remove(item.Id);
            }

            var skillMgrExt = skillManager.SkillManagerExtended;

            // ref: AUD-01-08 — Ergonomics não muta mais o Template compartilhado; o bônus agora é aplicado
            // por-instância via ErgonomicsTotalPatch (Postfix em Weapon.ErgonomicsTotal), que consulta
            // UsecWeaponInstanceIds (populado logo abaixo) pra saber quais instâncias recebem o bônus.
            //
            // Recoil (RecoilForceUp/RecoilForceBack) permanece mutando o Template compartilhado — LIMITAÇÃO
            // CONFIRMADA E ACEITA (débito técnico, ver 003-corrigir-achados-altos-auditoria-01-02-spec-tech.md
            // §1.2): ao contrário de Ergonomics, não existe nenhuma property por-instância equivalente pra
            // Recoil no Assembly (Weapon.RecoilForceBack é só um repasse direto de Template.RecoilForceBack,
            // e RecoilForceUp não tem property de instância alguma — todo consumo real lê o Template
            // diretamente). Harmony não intercepta leitura de campo público, só métodos/properties.
            weapon.Template.RecoilForceUp = UsecOriginalWeaponValues[item.TemplateId].weaponUp * (1 - skillMgrExt.UsecArSystemsRecoilBuff);
            weapon.Template.RecoilForceBack = UsecOriginalWeaponValues[item.TemplateId].weaponBack * (1 - skillMgrExt.UsecArSystemsRecoilBuff);

#if DEBUG
            SkillsExtendedPlugin.Log.LogDebug($"New {weapon.LocalizedName()} ergo: {weapon.Template.Ergonomics}, up {weapon.Template.RecoilForceUp}, back {weapon.Template.RecoilForceBack}");
#endif

            UsecWeaponInstanceIds.Add(item.Id, skillManager.UsecArsystems.Level);

            yield return null;
        }
    }

    private static IEnumerator UpdateEasternWeapons()
    {
        // ref: AUD-01-04 — mesma correção de UpdateUsecWeapons acima.
        var profile = GameUtils.GetProfile(GameUtils.IsScav() ? EPlayerSide.Savage : EPlayerSide.Usec);
        if (profile == null)
        {
            yield break;
        }

        var easternWeapons = SkillsExtendedPlugin.SkillData.EasternWeapons;
        var weapons = profile.Inventory.AllRealPlayerItems
            .Where(x => easternWeapons.Weapons.Contains(x.TemplateId));

        foreach (var item in weapons)
        {
            if (item is not Weapon weapon)
            {
                continue;
            }

            // Re-checa a cada iteração — o jogador pode sair de raid no meio do processo (AUD-01-04).
            var skillManager = GameUtils.GetSkillManager();
            if (skillManager == null)
            {
                yield break;
            }

            // Store the weapons original values
            if (!EasternOriginalWeaponValues.ContainsKey(item.TemplateId))
            {
                var origVals = new OrigWeaponValues
                {
                    weaponUp = weapon.Template.RecoilForceUp,
                    weaponBack = weapon.Template.RecoilForceBack
                };

#if DEBUG
                SkillsExtendedPlugin.Log.LogDebug(
                    $"original {weapon.LocalizedName()} ergo: {weapon.Template.Ergonomics}, up {weapon.Template.RecoilForceUp}, back {weapon.Template.RecoilForceBack}");
#endif

                EasternOriginalWeaponValues.Add(item.TemplateId, origVals);
            }

            //Skip instances of the weapon that are already adjusted at this level.
            if (EasternWeaponInstanceIds.ContainsKey(item.Id))
            {
                if (EasternWeaponInstanceIds[item.Id] == skillManager.BearAksystems.Level)
                {
                    continue;
                }

                EasternWeaponInstanceIds.Remove(item.Id);
            }

            var skillMgrExt = skillManager.SkillManagerExtended;

            // ref: AUD-01-08 — mesma correção de UpdateUsecWeapons acima: Ergonomics resolvido por-instância
            // via ErgonomicsTotalPatch; Recoil permanece mutando o Template compartilhado (débito técnico
            // aceito — ver 003-corrigir-achados-altos-auditoria-01-02-spec-tech.md §1.2).
            weapon.Template.RecoilForceUp = EasternOriginalWeaponValues[item.TemplateId].weaponUp * (1 - skillMgrExt.BearAkSystemsRecoilBuff);
            weapon.Template.RecoilForceBack = EasternOriginalWeaponValues[item.TemplateId].weaponBack * (1 - skillMgrExt.BearAkSystemsRecoilBuff);

#if DEBUG
            SkillsExtendedPlugin.Log.LogDebug(
                $"New {weapon.LocalizedName()} ergo: {weapon.Template.Ergonomics}, up {weapon.Template.RecoilForceUp}, back {weapon.Template.RecoilForceBack}");
#endif

            EasternWeaponInstanceIds.Add(item.Id, skillManager.BearAksystems.Level);

            yield return null;
        }
    }
}