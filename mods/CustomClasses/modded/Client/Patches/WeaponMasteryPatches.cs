using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Animations;       // ProceduralWeaponAnimation
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     Item 058 · Perna 1 — XP da maestria **Underbarrel Launchers por DISPARO** do GP-25/M203 (+ efeito por
///     nível no coice do próprio lançador). Validação in-game (2026-07-04) provou que o funil vanilla de XP
///     nunca credita o underbarrel (o Item do acerto da explosão é a MUNIÇÃO) — por isso o hook é o método
///     dedicado do disparo: <c>FirearmController.method_57(LauncherItemClass, AmmoItemClass)</c>
///     (ref: Player.cs:14231; chamado 1×/tiro pelo OnFireEvent do estado do lançador, Player.cs:6500-6512).
///     Anti-XP-duplo: SMG/LMG/Launcher sobem vanilla e NÃO recebem XP do mod (§10 da spec).
/// </summary>
internal class UnderbarrelMasteryXpPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: Player.cs:14231 — assinatura estável por tipos explícitos (método ofuscado → AP-03: não-virtual,
        // classe concreta; overload único com (LauncherItemClass, AmmoItemClass)).
        return AccessTools.Method(typeof(Player.FirearmController), "method_57",
            new[] { typeof(LauncherItemClass), typeof(AmmoItemClass) });
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, ref float ___float_5)
    {
        try
        {
            if (PerksConfig.WeaponMasteryEnabled?.Value != true)
            {
                return;
            }

            // AP-02: bots também usam FirearmController → só o controller do MainPlayer local.
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.HandsController))
            {
                return;
            }

            var skill = p.Skills?.AttachedLauncher;   // ref: SkillManager.cs:1320
            if (skill == null)
            {
                return;
            }

            // ref: CR-01-04 (ajustado R2-01) — o shooting range do hideout NÃO dá XP de weapon skill no
            // vanilla (HideoutPlayer.ExecuteShotSkill é override VAZIO — HideoutPlayer.cs:653) → o gate
            // bloqueia SÓ o XP; o EFEITO por nível (abaixo) vale no range também, como nos patches irmãos
            // de recuo/ergo (o range é onde o jogador testa recuo — não pode mentir sobre o in-raid).
            var inHideout = p.GetType().Name.IndexOf("Hideout", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!inHideout)
            {
                // XP por disparo × fator de XP da classe (consistência com o OnTriggerPatch — PA-01-02).
                // silent: true → sem LevelChanged por tiro (PA-01-03; ref: AbstractSkillClass.cs:115).
                var xp = PerksConfig.MasteryXpPerShot?.Value ?? 0f;
                SkillMultipliers.EnsureLoaded();   // ref: CR-01-02 — todo call-site de TryGet vem pareado com EnsureLoaded
                if (Plugin.Enabled && SkillMultipliers.TryGet(ESkillId.AttachedLauncher, out var f))   // ref: CR-01-06 — respeita o master switch
                {
                    xp *= Mathf.Max(0f, f);   // ref: CR-01-07 — clamp explícito de fator negativo (como no OnTriggerPatch)
                }

                if (xp > 0f)
                {
                    // ref: CR-01-01 — PARIDADE nos primeiros níveis: o funil vanilla amplifica o XP cru em
                    // Level<9 (×10/(nível+1) — SkillClass.cs:228-241/108). Sem isso, nível 0→1 = ~1000 disparos
                    // (10× mais lento que a paridade prometida). Fadiga/BonusController: skip documentado (PA-01-02).
                    if (skill.Level < 9)
                    {
                        xp = skill.CalculateExpOnFirstLevels(xp);   // público — SkillClass.cs:108
                    }

                    skill.SetCurrent(skill.Current + xp, true);
                }
            }

            // Efeito por nível do PRÓPRIO underbarrel: method_57 seta float_5 = 1 + ammoRec/100 (força do
            // recuo deste tiro, consumo único em ManualLateUpdate — Player.cs:14479) — escala só o EXCESSO
            // acima de 1. ref: CR-01-05 — mesmo piso 0.5 da perna de recuo (teto do F12 × nível alto zeraria).
            var rec = PerksConfig.MasteryRecoilPerLevel?.Value ?? 0f;
            var lvl = skill.Level;
            if (rec > 0f && lvl > 0 && ___float_5 > 1f)
            {
                ___float_5 = 1f + (___float_5 - 1f) * Mathf.Max(0.5f, 1f - rec * lvl);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (058) underbarrel mastery falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     Item 058 · Perna 2 — recuo × (1 − rec/nível·Level) pela maestria da ARMA EM MÃOS (SMG/LMG/Launcher).
///     Mesmo alvo do <see cref="ShootRecoilPatch"/> (050) — Prefixes independentes compõem (multiplicadores
///     comutam; coexiste com Shaky Hands/Adrenaline/Bunker). O PerkDiag captura o baseline no Prefix do 050,
///     então o efeito de maestria aparece no `Recoil str` do overlay (052) — é o instrumento de validação.
/// </summary>
internal class WeaponMasteryRecoilPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.Shoot));
    }

    // ref: CR-01-03 — prioridade EXPLÍCITA: este Prefix precisa rodar ANTES do ShootRecoilPatch (050) pra
    // maestria entrar no baseline `str0` que o PerkDiag (052) captura — senão o overlay não mostra o efeito.
    // Hoje a ordem dos Enable() no Plugin.cs já garante isso; o atributo torna a intenção à prova de refactor.
    [HarmonyPriority(Priority.High)]
    [PatchPrefix]
    private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
    {
        try
        {
            if (PerksConfig.WeaponMasteryEnabled?.Value != true)
            {
                return;
            }

            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.ProceduralWeaponAnimation))
            {
                return;   // só a arma do player local (padrão do ShootRecoilPatch)
            }

            var skill = WeaponMastery.SkillForHeld(p.Skills, (p.HandsController as Player.FirearmController)?.Item);
            var lvl = skill?.Level ?? 0;
            var rec = PerksConfig.MasteryRecoilPerLevel?.Value ?? 0f;
            if (lvl > 0 && rec > 0f)
            {
                str *= Mathf.Max(0.5f, 1f - rec * lvl);   // clamp: nunca corta mais que 50% via maestria
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (058) mastery recoil falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     Item 058 · Perna 2 — ergonomia × (1 + ergo/nível·Level) pela maestria da arma em mãos. Mesmo alvo do
///     <see cref="HeavyWeaponErgoPatch"/> (050) — Postfixes compõem; gate idêntico (controller do MainPlayer).
/// </summary>
internal class WeaponMasteryErgoPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Player.FirearmController), nameof(Player.FirearmController.TotalErgonomics));
    }

    [PatchPostfix]
    private static void Postfix(Player.FirearmController __instance, ref float __result)
    {
        try
        {
            if (PerksConfig.WeaponMasteryEnabled?.Value != true)
            {
                return;
            }

            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.HandsController))
            {
                return;   // só a arma do player local (padrão do HeavyWeaponErgoPatch)
            }

            var skill = WeaponMastery.SkillForHeld(p.Skills, __instance.Item);
            var lvl = skill?.Level ?? 0;
            var ergo = PerksConfig.MasteryErgoPerLevel?.Value ?? 0f;
            if (lvl > 0 && ergo > 0f)
            {
                __result *= 1f + ergo * lvl;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (058) mastery ergo falhou: {ex.Message}");
        }
    }
}
