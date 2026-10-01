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
    internal static float SainBefore, SainAfter;
    internal static float MalfChance = -1f;

    /// <summary>
    ///     <b>B20 — observabilidade de PEER.</b> O overlay 052 descreve só o SEU player (todos os writes acima são
    ///     gateados em <c>IsYourPlayer</c>), então os efeitos de som EM PEERS (B14: o que a IA ouve deles; B20: o que
    ///     VOCÊ ouve deles) não tinham COMO ser verificados — o teste viraria "achei que soou mais baixo", que não é
    ///     evidência. Isto emite um log por emissor remoto, com a classe resolvida e o antes→depois.
    ///     <para>
    ///     Só roda com <c>Diagnostics</c> LIGADO no F12 (default off) → custo zero no jogo normal. Throttle por
    ///     (canal + nickname) para não inundar o log: 1 linha a cada <see cref="ThrottleSeconds"/>, e só quando o
    ///     multiplicador de fato mudou o valor. Como o mult é constante por classe, uma linha já prova o efeito.
    ///     </para>
    /// </summary>
    private const float ThrottleSeconds = 3f;
    private static readonly System.Collections.Generic.Dictionary<string, float> LastLog = new(StringComparer.Ordinal);

    internal static void LogPeer(string channel, string nickname, string classNameEn, float before, float after)
    {
        var key = channel + "" + nickname;
        var now = Time.time;

        if (LastLog.TryGetValue(key, out var last) && now - last < ThrottleSeconds)
        {
            return;
        }

        LastLog[key] = now;
        var mult = before > 0f ? after / before : 0f;
        Plugin.Log?.LogInfo(
            $"[CustomClasses][diag/peer] {channel}: '{nickname}' [{classNameEn}] {before:F1} → {after:F1} (×{mult:F2})");
    }

    /// <summary>Limpa o throttle entre raids (Time.time é monotônico no processo, mas o roster muda).</summary>
    internal static void ResetPeerLog() => LastLog.Clear();
}

/// <summary>
///     PERF-INSTR AUD-01-02/03 — temporary, remove after validation.
///     <para>
///     Censo das superfícies mais quentes. Responde as duas perguntas que a leitura estática não fecha:
///     <b>qual o N real</b> (bots × frames) que essas superfícies pagam numa raid, e <b>qual fração passa
///     do gate</b> (deve ficar ~1/N — se subir, o gate afrouxou).
///     </para>
///     <para>
///     ⚠️ PA-02-05 — a POSIÇÃO de cada incremento é o que torna a razão mensurável: <c>*Calls</c> vem ANTES
///     do gate, <c>*Passed</c> DEPOIS. Se os dois ficarem depois, a razão dá sempre 1 e o critério de aceite
///     não mede nada. <c>*Gates</c> conta execuções de gate por evento — é o que prova a meta 4 → 2.
///     </para>
///     <para>Contadores primitivos, sem alocação; só incrementados sob <c>PerkDiag.Enabled</c>.</para>
/// </summary>
internal static class PerfCount
{
    internal static long MoveSpeedCalls, MoveSpeedPassed;
    internal static long StepAiCalls, StepAiPassed;
    internal static long RolloffCalls, RolloffPassed;
    internal static long DamageCalls, DamageGates;
    internal static long ShootCalls, ShootGates;
    internal static long ErgoGates;

    internal static void Reset()
    {
        MoveSpeedCalls = MoveSpeedPassed = 0;
        StepAiCalls = StepAiPassed = 0;
        RolloffCalls = RolloffPassed = 0;
        DamageCalls = DamageGates = 0;
        ShootCalls = ShootGates = 0;
        ErgoGates = 0;
    }

    /// <summary>Linha agregada. ⚠️ O PRIMEIRO dump após ligar o diagnóstico é parcial — descartar (PA-02-05).</summary>
    internal static string Dump() =>
        $"moveSpeed={MoveSpeedCalls}/{MoveSpeedPassed} stepAI={StepAiCalls}/{StepAiPassed} "
        + $"rolloff={RolloffCalls}/{RolloffPassed} damage={DamageCalls} (gates={DamageGates}) "
        + $"shoot={ShootCalls} (gates={ShootGates}) ergoGates={ErgoGates}";
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

    /// <summary>
    ///     ref: AUD-01-07d — com o overlay ligado, o <c>AppendPerkList</c> chamava
    ///     <c>PerksCatalog.LocalGroups()</c> (LINQ + <c>ToArray</c>) a CADA Repaint. Cacheado.
    ///     <para>
    ///     Seguro: <c>PerkGroup</c>/<c>PerkLine</c> do <c>Library</c> são SINGLETONS e
    ///     <c>PerkLine.Multiplier</c> resolve <c>Live?.Invoke()</c> a cada acesso (PerksCatalog.cs:39) —
    ///     cachear o ARRAY não congela os valores, o F12 continua vivo (B4).
    ///     </para>
    ///     <para>ref: PA-04-03 — invalidado por <c>SkillMultipliers.ClassChanged</c>, assinado no Awake.</para>
    /// </summary>
    private static PerksCatalog.PerkGroup[]? _cachedGroups;
    private static bool _groupsCached;

    /// <summary>ref: PA-04-03 — assinado a <c>SkillMultipliers.ClassChanged</c> no <c>Plugin.Awake</c>.</summary>
    internal static void ClearGroupCache()
    {
        _cachedGroups = null;
        _groupsCached = false;
    }

    private static PerksCatalog.PerkGroup[]? CachedLocalGroups()
    {
        if (_groupsCached)
        {
            return _cachedGroups;
        }

        _cachedGroups = PerksCatalog.LocalGroups();
        _groupsCached = true;   // cacheia inclusive null (classe vanilla) — não re-tentar por Repaint
        return _cachedGroups;
    }

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
        // (fix 2026-07-15) prova do bug de timing do Pack Mule: 'Carry mod' lê o getter LIVE (já +30%), mas o
        // limiar de dreno ao andar vem dos limites CACHEADOS. Com Pack Mule +30%, corrigido → ≈ 45×1.30 = 58.5;
        // bug (cache vanilla) → ~45. Compare com o peso que o inventário do jogo mostra: se você drena ao andar
        // com peso ABAIXO deste limite, algo está errado.
        Line(sb, "Walk overweight limit (kg)", () => $"{p.Physical.WalkOverweightLimits.x:F1}");
        Line(sb, "Ergo (weapon)", () => p.HandsController is Player.FirearmController fc && fc.Item is Weapon w
            ? $"{fc.TotalErgonomics:F1}  [{w.WeapClass}]"
            : "(no firearm)");
        Line(sb, "Aim speed", () => $"{p.ProceduralWeaponAnimation.AimingSpeed:F3}");
        Line(sb, "Holding breath (Iron Lungs)", () => p.Physical.HoldingBreath ? "<color=#7fff7f>YES</color>" : "no");
        // B6 (2026-07-11): ARMOR = condição nova da Couraça (armadura de TRONCO classe >= o mínimo do F12).
        Line(sb, "Cond: aim / melee / heavy-wpn / armor", () =>
            $"{Flag(p.HandsController is Player.FirearmController fa && fa.IsAiming, "AIM")}"
            + $" / {Flag(p.HandsController is Player.KnifeController, "MELEE")}"
            + $" / {Flag(HeavyWeapon.InHand(p), "HEAVY")}"
            + $" / {Flag(BulwarkArmor.HasHeavyArmor(p), "ARMOR")}");
        sb.AppendLine($"Adrenaline: <b>{AdrenalineLabel()}</b>");
        sb.AppendLine($"Recoil str (last shot): <b>{FmtBA(PerkDiag.RecoilBefore, PerkDiag.RecoilAfter, "F2")}</b>");
        sb.AppendLine($"Audio radius — you hear: <b>{FmtBA(PerkDiag.AudioBefore, PerkDiag.AudioAfter, "F1")}</b>");
        sb.AppendLine($"AI hear power — bots (base): <b>{FmtBA(PerkDiag.AiPowerBefore, PerkDiag.AiPowerAfter, "F1")}</b>");
        sb.AppendLine($"SAIN hear range — bots: <b>{FmtBA(PerkDiag.SainBefore, PerkDiag.SainAfter, "F1")}</b>");
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
            var groups = CachedLocalGroups();   // ref: AUD-01-07d
            if (groups == null || groups.Length == 0)
            {
                return;
            }

            sb.AppendLine("<b><color=#7fd4ff>Class perks / drawbacks</color></b>");
            foreach (var g in groups)
            {
                sb.AppendLine($"<color=#c8c8c8>{g.NameEn}</color>");
                foreach (var l in g.Lines)
                {
                    var mark = l.IsPerk ? "<color=#7fff7f>+</color>" : "<color=#ff7f7f>-</color>";
                    var tok = l.ValueToken.Length > 0 ? l.ValueToken + " " : "";
                    var soon = l.Pending ? " (soon)" : "";
                    sb.AppendLine($"  {mark} {tok}{l.LabelEn}{soon}");
                }
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
