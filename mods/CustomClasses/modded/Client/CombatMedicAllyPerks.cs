using System;

namespace CustomClasses.Client;

/// <summary>
///     077 — 🩺 fachada <b>pública</b> (bridge-friendly) dos perks de <b>TEMPO / MOVIMENTO</b> do Médico de Combate
///     para a cura de <b>ALIADO</b> do ICM (TRL-ImmersiveCombatMedicine). O 072 desenhou esses perks assumindo a
///     AUTO-cirurgia (caminho vanilla); na cura de aliado (mecânica do ICM) eles não valiam — o movimento vazava
///     para qualquer operador e a velocidade não valia nem para o Médico.
///     <para>
///     <b>Arquitetura (opção B, espelha o 076):</b> o ICM é dono da cura de aliado e chama esta API por reflection
///     (soft-dep, fail-safe) no seu <c>HealRoutine</c>. CustomClasses ausente = no-op no ICM (tempo padrão + ninguém
///     anda). Espelho: <c>TRL-ImmersiveCombatMedicine/.../CustomClassesBridge.cs</c>.
///     </para>
///     <para>
///     ⚠️ <b>Gate = classe LOCAL.</b> O operador é SEMPRE o <c>MainPlayer</c> (o <c>HealRoutine</c> roda no processo
///     de quem tem o item na mão; ninguém opera remotamente). Logo dispensa o mapa 057 e packet — diferente do 076,
///     onde o efeito (penalidade de HP) é aplicado no processo do PACIENTE. Movimento e velocidade da animação já
///     replicam aos peers pelo Fika nativo.
///     </para>
/// </summary>
public static class CombatMedicAllyPerks
{
    /// <summary>
    ///     Fator de tempo do procedimento quando o operador local é <b>Combat Medic</b>: 🔧 Swift Surgeon (cirurgia)
    ///     ou 🔧 Rapid Care (demais itens). <c>1</c> = sem efeito (não-Médico, perk off, ou 072 desabilitado).
    ///     <b>PÚBLICO</b> — o ICM chama por reflection no seu ApplyTreatment/HealRoutine.
    /// </summary>
    public static float AllyHealTimeMult(bool isSurgery)
    {
        try
        {
            // classe LOCAL (o operador é o MainPlayer). IsLocalClass lê SkillMultipliers.ClassNameEn.
            if (!SkillMultipliers.IsLocalClass("Combat Medic"))
            {
                return 1f;
            }

            return MedicTiming.FactorFor(isSurgery);   // reusa o 072 (respeita o kill-switch _disabled)
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (077) AllyHealTimeMult falhou: {ex.Message}");
            return 1f;
        }
    }

    /// <summary>
    ///     True se o operador local é <b>Combat Medic</b> com 🔧 <b>Mobile Surgery</b> ligado → pode ANDAR durante a
    ///     cirurgia de aliado (sem correr/pular). False (default seguro) faz o ICM imobilizar o operador.
    /// </summary>
    public static bool AllyMobileSurgeon()
    {
        try
        {
            return PerksConfig.MobileSurgeryEnabled?.Value == true
                   && SkillMultipliers.IsLocalClass("Combat Medic");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (077) AllyMobileSurgeon falhou: {ex.Message}");
            return false;
        }
    }
}
