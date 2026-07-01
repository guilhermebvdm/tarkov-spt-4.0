using System;
using System.Text;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     Item 052 (validação) — valores "ao vivo" capturados pelos patches de EVENTO (recuo/som/jam),
///     que não dá pra ler direto de uma propriedade contínua. Cada patch escreve aqui quando diag está on.
/// </summary>
internal static class PerkDiag
{
    internal static bool Enabled => PerksConfig.DiagnosticsEnabled?.Value == true;

    // Gap 11 fix: valores CRUS gravados nos hot-paths (sem alocar string) — formatados só no Draw (1×/frame).
    internal static float RecoilBefore = 1f, RecoilAfter = 1f;
    internal static float AudioBefore, AudioAfter;
    internal static float AiPowerBefore, AiPowerAfter;
    internal static float MalfChance = -1f;
}

/// <summary>
///     Item 052 — "super espião": overlay F12 (toggle <c>Perk Diagnostics</c>) que lê AO VIVO as
///     propriedades afetadas pelos perks do MainPlayer. Troque o toggle de um perk no F12 e veja o número
///     pular — prova que o patch dispara + o gate casa + o valor muda, mesmo sem "sentir" in-game.
///     Cobre os pontos obfuscados/injetados de risco (TotalErgonomics, AimingSpeed) sem depender da sensação.
/// </summary>
internal static class PerkDiagnostics
{
    private static GUIStyle? _style;

    internal static void Draw()
    {
        if (PerksConfig.DiagnosticsEnabled?.Value != true)
        {
            return;
        }

        // (perf) OnGUI dispara 2×/frame (Layout+Repaint); com Rect fixo só o Repaint desenha → constrói a
        // string 1×/frame em vez de 2× (metade da alocação do overlay, e só quando o diag está ligado).
        if (Event.current != null && Event.current.type != EventType.Repaint)
        {
            return;
        }

        var p = Singleton<GameWorld>.Instance?.MainPlayer;
        if (p == null)
        {
            return;
        }

        _style ??= new GUIStyle
        {
            fontSize = 14,
            richText = true,
            normal = { textColor = Color.white },
            padding = new RectOffset(8, 8, 8, 8),
        };

        var sb = new StringBuilder();
        sb.AppendLine("<b><color=#7fd4ff>CustomClasses — Perk Diagnostics</color></b>");
        sb.AppendLine($"Class (EN): <b>{SkillMultipliers.ClassNameEn ?? "?"}</b>");

        Line(sb, "Char speed (REAL — walk driver)", () => $"{p.MovementContext.CharacterMovementSpeed:F3}");
        Line(sb, "Sprint speed (REAL — run driver)", () => $"{p.MovementContext.SprintSpeed:F3}");
        Line(sb, "MaxSpeed (base ceiling — NOT the driver)", () => $"{p.MovementContext.MaxSpeed:F2}");
        Line(sb, "Inertia", () => $"{p.Physical.Inertia:F3}");
        Line(sb, "Carry mod", () => $"{p.Skills.CarryingWeightRelativeModifier:F3}");
        Line(sb, "Ergo (weapon)", () => p.HandsController is Player.FirearmController fc && fc.Item is Weapon w
            ? $"{fc.TotalErgonomics:F1}  [{w.WeapClass}]"
            : "(no firearm)");
        Line(sb, "Aim speed", () => $"{p.ProceduralWeaponAnimation.AimingSpeed:F3}");
        Line(sb, "Holding breath (Iron Lungs)", () => p.Physical.HoldingBreath ? "<color=#7fff7f>YES</color>" : "no");
        Line(sb, "Cond: aim / melee / heavy-wpn", () =>
            $"{Flag(p.HandsController is Player.FirearmController fa && fa.IsAiming, "AIM")}"
            + $" / {Flag(p.HandsController is Player.KnifeController, "MELEE")}"
            + $" / {Flag(HeavyWeapon.InHand(p), "HEAVY")}");
        sb.AppendLine($"Adrenaline: <b>{AdrenalineLabel()}</b>");
        sb.AppendLine($"Recoil str (last shot): <b>{FmtBA(PerkDiag.RecoilBefore, PerkDiag.RecoilAfter, "F2")}</b>");
        sb.AppendLine($"Audio radius — you hear: <b>{FmtBA(PerkDiag.AudioBefore, PerkDiag.AudioAfter, "F1")}</b>");
        sb.AppendLine($"AI hear power — bots: <b>{FmtBA(PerkDiag.AiPowerBefore, PerkDiag.AiPowerAfter, "F1")}</b>");
        sb.AppendLine($"Malfunction%: <b>{(PerkDiag.MalfChance < 0f ? "-" : (PerkDiag.MalfChance * 100f).ToString("F2") + "%")}</b>");

        AppendPerkList(sb);

        GUI.Label(new Rect(12f, 90f, 580f, 470f), sb.ToString(), _style);
    }

    private static void Line(StringBuilder sb, string label, Func<string> read)
    {
        string val;
        try
        {
            val = read();
        }
        catch (Exception ex)
        {
            val = $"<color=#ff7f7f>ERR {ex.GetType().Name}</color>";
        }

        sb.AppendLine($"{label}: <b>{val}</b>");
    }

    // Verde quando a condição está ativa (Rooted/Execution/Bunker dependem disso).
    private static string Flag(bool on, string label)
    {
        return on ? $"<color=#7fff7f>{label}</color>" : "-";
    }

    // "antes→depois" formatado no Draw (1×/frame); verde quando um perk MODIFICOU o valor.
    private static string FmtBA(float before, float after, string fmt)
    {
        if (before <= 0f && after <= 0f)
        {
            return "-";
        }

        var s = $"{before.ToString(fmt)}→{after.ToString(fmt)}";
        return Math.Abs(after - before) > 0.001f ? $"<color=#7fff7f>{s}</color>" : s;
    }

    // Lista os perks/drawbacks da classe local (reusa o PerksCatalog) — "o que esperar".
    private static void AppendPerkList(StringBuilder sb)
    {
        try
        {
            var entries = PerksCatalog.LocalEntries();
            if (entries == null || entries.Length == 0)
            {
                return;
            }

            sb.AppendLine("<b><color=#7fd4ff>Class perks / drawbacks</color></b>");
            foreach (var e in entries)
            {
                sb.AppendLine(e.IsPerk ? $"  <color=#7fff7f>+</color> {e.En}" : $"  <color=#ff7f7f>-</color> {e.En}");
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"<color=#ff7f7f>perk list ERR {ex.GetType().Name}</color>");
        }
    }

    private static string AdrenalineLabel()
    {
        if (AdrenalineState.IsActive)
        {
            return $"<color=#7fff7f>ACTIVE {AdrenalineState.SecondsLeft:F0}s</color>";
        }

        return AdrenalineState.OnCooldown ? "cooldown" : "ready";
    }
}
