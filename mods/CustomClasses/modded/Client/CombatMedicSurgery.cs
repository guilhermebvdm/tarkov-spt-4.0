using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     076 — 🩺 <b>Restorative Surgery</b> (Médico de Combate): a cirurgia NÃO reduz o HP MÁXIMO do membro curado.
///     O vanilla deixa uma "cicatriz" permanente na raid — <c>ActiveHealthController.RestoreBodyPart(bodyPart,
///     healthPenalty)</c> aplica um penalty ao máximo do membro. O perk multiplica esse penalty pelo mult do F12
///     (default 0 = sem penalidade) QUANDO O OPERADOR é Médico de Combate.
///     <para>
///     <b>API pública p/ o ICM (TRL-ImmersiveCombatMedicine — mod oficial de cura de ALIADOS).</b> Decisão de
///     arquitetura (opção B, 2026-07-19): o CustomClasses é dono do "operador é Médico?" e expõe <see cref="Adjust"/>
///     + <see cref="SetExternalHandling"/>; o ICM (dono da cirurgia de aliado) chama a API no seu <c>ApplySurgery</c>
///     (envio) — o penalty ajustado viaja no <c>BandAidHealPacket</c> até o cliente do paciente. A AUTO-cirurgia
///     (caminho nativo, paciente local) é coberta pelo <see cref="SurgeryPenaltyPatch"/> aqui.
///     </para>
///     <para>
///     Gate = classe do OPERADOR via <see cref="ClassIdentities.ClassNameEnOf"/> (resolve local + peer pela rota 057;
///     retorna null p/ <c>IsAI</c>). Os 3 cenários coop (médico host→client · client→host · client→client) caem
///     naturalmente: o operador é o <c>doctor</c> do ApplySurgery, resolvido em qualquer processo pelo ProfileId.
///     </para>
/// </summary>
public static class CombatMedicSurgery
{
    // ICM seta isto (via SetExternalHandling) ao aplicar a cirurgia do PACKET no ActiveHealthController do paciente
    // (#3, cliente do paciente), pra o patch nativo NÃO re-ajustar pela classe do PACIENTE — o penalty do packet já
    // veio ajustado pela classe do OPERADOR no envio (#2). Sem isto, um paciente-Médico operado por um NÃO-Médico
    // teria a penalidade zerada por engano (o gate nativo veria o paciente, não o operador).
    private static bool _externalHandling;

    /// <summary>Chamado pelo ICM (reflection) ao redor do RestoreBodyPart do packet de cirurgia. Ver o campo acima.</summary>
    public static void SetExternalHandling(bool value) => _externalHandling = value;

    internal static bool ExternalHandling => _externalHandling;

    /// <summary>
    ///     Penalty ajustado da cirurgia: se o perk está ligado e o <paramref name="doctor"/> é <b>Combat Medic</b>,
    ///     eleva a fração de HP máximo retida ao PISO do F12 (default 0.90 = o membro volta com 90%), sem nunca
    ///     piorar o vanilla; senão devolve intacto. <b>PÚBLICO</b> — o ICM chama por reflection no seu ApplySurgery.
    /// </summary>
    public static float Adjust(Player? doctor, float penalty)
    {
        try
        {
            if (PerksConfig.RestorativeSurgeryEnabled?.Value != true || doctor == null)
            {
                return penalty;
            }

            // classe do OPERADOR (não do paciente). ClassNameEnOf: local via SkillMultipliers, peer via rota 057,
            // null p/ IsAI (bots barrados — regra 075). Compare case-insensitive, alinhado ao resto do mod.
            var cls = ClassIdentities.ClassNameEnOf(doctor);
            if (!string.Equals(cls, "Combat Medic", StringComparison.OrdinalIgnoreCase))
            {
                return penalty;
            }

            // ⚠️ SEMÂNTICA (review 076): o `penalty`/`healthPenalty` é a FRAÇÃO de HP MÁXIMO RETIDA
            // (Maximum × healthPenalty, ActiveHealthController:3903), NÃO a penalidade — 1.0 = sem cicatriz,
            // 0.0 = membro volta com 1 de máximo. O vanilla retém pouco: CMS 0.25–0.45, Surv12 0.60–0.72.
            // Restorative Surgery eleva a retenção a um PISO configurável (F12, default 0.90 = 90%): Max(vanilla, piso)
            //   → nunca pior que o piso, nunca pior que o vanilla. A skill Surgery vanilla ainda empurra ALÉM do piso
            //   (linha 3902, aplicada DEPOIS deste Prefix) → 90% é o MÍNIMO garantido, não o teto (decisão do usuário).
            var retention = PerksConfig.RestorativeSurgeryRetention?.Value ?? 0.90f;
            return Math.Max(penalty, retention);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (076) surgery adjust falhou: {ex.Message}");
            return penalty;
        }
    }
}

/// <summary>
///     076 — patch do caminho NATIVO (auto-cirurgia). O overload <c>RestoreBodyPart(EBodyPart, float healthPenalty)</c>
///     é chamado pela cirurgia (MedEffect DestroyedPart, ActiveHealthController:1907) — o caso do perk — e também
///     por uma REGENERAÇÃO periódica de membro destruído (ActiveHealthController:2256, com <c>healthPenalty = 1f</c>).
///     Como <c>Adjust</c> devolve <c>Max(penalty, piso)</c> e o piso ≤ 1, um <c>healthPenalty = 1f</c> fica intacto
///     (<c>Max(1, 0.90) = 1</c>) → a regeneração não é afetada. Roda no health controller do PACIENTE. Na
///     auto-cirurgia o operador = paciente = dono deste <c>ActiveHealthController</c> → o gate lê a classe dele.
///     No caso de ALIADO (o ICM aplica o packet no ActiveHC do paciente), o <c>SetExternalHandling(true)</c> do ICM
///     faz este patch PULAR — o penalty já veio ajustado pela classe do OPERADOR no envio (#2).
///     <para>Bots: se um bot fizesse cirurgia, <c>ClassNameEnOf</c> retorna null (IsAI) → não é Médico → penalty
///     intacto. Regra 075 (gate por classe do operador, que barra IsAI).</para>
/// </summary>
internal class SurgeryPenaltyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(ActiveHealthController), nameof(ActiveHealthController.RestoreBodyPart),
            new[] { typeof(EBodyPart), typeof(float) });
    }

    [PatchPrefix]
    private static void Prefix(ActiveHealthController __instance, ref float healthPenalty)
    {
        try
        {
            if (CombatMedicSurgery.ExternalHandling)
            {
                return;   // ICM está aplicando a cirurgia de ALIADO (packet) — já ajustou pela classe do OPERADOR.
            }

            // auto-cirurgia: operador = paciente = dono deste ActiveHealthController.
            healthPenalty = CombatMedicSurgery.Adjust(__instance?.Player, healthPenalty);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (076) surgery penalty (native) falhou: {ex.Message}");
        }
    }
}
