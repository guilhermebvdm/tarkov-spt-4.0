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
    internal static string LastRecoil = "-";
    internal static string LastSound = "-";
    internal static string LastMalfunction = "-";
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

        Line(sb, "MaxSpeed / Sprint", () => $"{p.MovementContext.MaxSpeed:F2} / {p.MovementContext.SprintSpeed:F2}");
        Line(sb, "Inertia", () => $"{p.Physical.Inertia:F3}");
        Line(sb, "Carry mod", () => $"{p.Skills.CarryingWeightRelativeModifier:F3}");
        Line(sb, "Ergo (weapon)", () => p.HandsController is Player.FirearmController fc && fc.Item is Weapon w
            ? $"{fc.TotalErgonomics:F1}  [{w.WeapClass}]"
            : "(no firearm)");
        Line(sb, "Aim speed", () => $"{p.ProceduralWeaponAnimation.AimingSpeed:F3}");
        sb.AppendLine($"Adrenaline: <b>{AdrenalineLabel()}</b>");
        sb.AppendLine($"Recoil str (last shot): <b>{PerkDiag.LastRecoil}</b>");
        sb.AppendLine($"Sound radius (last): <b>{PerkDiag.LastSound}</b>");
        sb.AppendLine($"Malfunction% (last): <b>{PerkDiag.LastMalfunction}</b>");

        GUI.Label(new Rect(12f, 90f, 560f, 340f), sb.ToString(), _style);
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

    private static string AdrenalineLabel()
    {
        if (AdrenalineState.IsActive)
        {
            return $"<color=#7fff7f>ACTIVE {AdrenalineState.SecondsLeft:F0}s</color>";
        }

        return AdrenalineState.OnCooldown ? "cooldown" : "ready";
    }
}
