using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     082 — 🔻 <b>Medroso</b> (Saqueador): mãos trêmulas SOB FOGO. Porta a lógica do mod <c>UnderFire</c> (rpmwpm),
///     gateada só p/ o Scavenger local. Dois gatilhos: <b>levar tiro</b> (<c>Player.ReceiveDamage</c>) e
///     <b>supressão / near-miss</b> (<c>GClass897.OnShoot</c> = BulletSoundsUtils, bala passando perto). Efeito: um
///     Tremor temporário. ⚠️ O mod UnderFire GLOBAL deve ficar DESATIVADO (senão TODOS ganham o tremor, não só o
///     Scav). Tipos ofuscados reconfirmados no decompile atual — GClass897/898/3008, GInterface361/331 inalterados.
/// </summary>
internal class ScavengerTremor : ActiveHealthController.GClass3008, GInterface361, IEffect, GInterface331
{
    public override float DefaultDelayTime => 0.1f;
    public override float DefaultResidueTime => ActiveHealthController.GClass3008.GClass3019_0.Tremor.DefaultResidueTime;
}

internal static class Medroso
{
    private static float _cooldownUntil;
    private static bool _shootHooked;

    /// <summary>Registra o hook de near-miss (evento ESTÁTICO). Chamado 1× do Plugin; o gate real é no handler.</summary>
    internal static void Init()
    {
        if (_shootHooked)
        {
            return;
        }

        try
        {
            GClass897.OnShoot += OnBulletFlyBy;   // ref: GClass897 (BulletSoundsUtils).OnShoot
            _shootHooked = true;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (082) medroso hook falhou: {ex.Message}");
        }
    }

    /// <summary>Reseta o cooldown entre raids (estado não vaza p/ a próxima).</summary>
    internal static void ResetRaid() => _cooldownUntil = 0f;

    private static MethodInfo _addTremorMethod;

    /// <summary>Aplica o tremor se: perk on + Scav local + fora do cooldown + HC disponível.</summary>
    private static void Trigger()
    {
        try
        {
            if (PerksConfig.MedrosoEnabled?.Value != true || !SkillMultipliers.IsLocalClass("Scavenger"))
            {
                return;
            }

            if (Time.time < _cooldownUntil)
            {
                return;   // anti-spam: não empilha tremor
            }

            var hc = Singleton<GameWorld>.Instance?.MainPlayer?.ActiveHealthController;
            if (hc == null)
            {
                return;
            }

            var dur = PerksConfig.MedrosoDuration?.Value ?? 6f;
            if (_addTremorMethod == null)
            {
                var nativeTremorType = ActiveHealthController.GClass3008.GClass3019_0.Tremor.GetType();
                _addTremorMethod = typeof(ActiveHealthController)
                    .GetMethod(nameof(ActiveHealthController.AddEffect))
                    ?.MakeGenericMethod(nativeTremorType);
            }

            _addTremorMethod?.Invoke(hc, new object[] { EBodyPart.Head, 0.1f, dur, 1.5f, null, null });
            // CR#7 (082): o AddEffect NÃO deduplica efeitos que implementam GInterface331 (cria nova instância).
            // O cooldown é a única trava — então nunca pode ser MENOR que a duração, senão o Tremor EMPILHA.
            _cooldownUntil = Time.time + Mathf.Max(PerksConfig.MedrosoCooldown?.Value ?? 8f, dur);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (082) medroso trigger falhou: {ex.Message}");
        }
    }

    /// <summary>Gatilho 1 — levar TIRO (chamado pelo Postfix de <c>ReceiveDamage</c>).</summary>
    internal static void OnHit(EDamageType type, float damage)
    {
        if (damage <= 0f || (type != EDamageType.Bullet && type != EDamageType.GrenadeFragment))
        {
            return;
        }

        Trigger();
    }

    /// <summary>
    ///     Gatilho 2 — SUPRESSÃO / near-miss (bala passa perto). Portado do <c>UnderFire.CheckFiredBullet</c>: projeta
    ///     a posição do jogador na reta do tiro e mede a menor distância. HOT PATH (cada tiro do mapa) — early-out cedo.
    /// </summary>
    private static void OnBulletFlyBy(SonicBulletSoundPlayer.GClass898 sonic)
    {
        try
        {
            var dist = PerksConfig.MedrosoSuppressDistance?.Value ?? 0f;
            if (dist <= 0f || PerksConfig.MedrosoEnabled?.Value != true || !SkillMultipliers.IsLocalClass("Scavenger"))
            {
                return;   // supressão desligada, perk off, ou não-Scav → sai antes da geometria
            }

            if (sonic == null || sonic.IsOccluded || sonic.Camera == null)
            {
                return;
            }

            Vector3 shotPos = sonic.ShotPosition;
            Vector3 shotDir = sonic.ShotDirection;
            Vector3 playerPos = sonic.Camera.transform.position;
            float denom = Vector3.Dot(shotDir, shotDir);
            if (denom <= 0f)
            {
                return;
            }

            float t = -Vector3.Dot(shotPos - playerPos, shotDir) / denom;
            Vector3 closest = shotPos + t * shotDir;
            if (Vector3.Distance(closest, playerPos) < dist)
            {
                Trigger();
            }
        }
        catch
        {
            // hot path — nunca lançar
        }
    }
}

/// <summary>082 — gatilho de DANO do Medroso: Postfix em <c>Player.ReceiveDamage</c> (Player.cs:26231).</summary>
internal class MedrosoDamagePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ReceiveDamage));
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, float damage, EDamageType type)
    {
        try
        {
            if (__instance != null && __instance.IsYourPlayer)
            {
                Medroso.OnHit(type, damage);
            }
        }
        catch
        {
        }
    }
}
