using EFT;
using EFT.HealthSystem;

namespace TRLImmersiveCombatMedicine.Helpers
{
    /// <summary>
    /// Credita XP de cura ao médico ao tratar um ALIADO, replicando as mesmas chamadas que o
    /// vanilla usa em auto-cura (ref: Assembly-CSharp/GClass2266.cs:309-313, dentro de method_6).
    /// Único ponto de crédito usado pelos dois mecanismos vanilla de XP de cura — HealExperience
    /// por efeito resolvido (sangramento/fratura/intoxicação/dor) e ExpForHeal por HP restaurado —
    /// e pelos dois caminhos do ICM (Caminho A: paciente local; Caminho B: paciente remoto/rede).
    /// Nunca calcula o valor: só recebe um `amount` já derivado de dados 100% vanilla
    /// (globals.json via BackendConfigSettingsClass / IEffect.HealExperience).
    /// </summary>
    internal static class HealXpCredit
    {
        public static void CreditHealXp(Player doctor, int amount)
        {
            if (doctor == null || amount <= 0)
                return;

            // GClass2268 (LocalStatisticManager) é o único IStatisticsManager com lógica real —
            // atribuído apenas ao jogador local (isYourPlayer:true, ref: Assembly-CSharp/EFT/
            // BaseLocalGame-1.cs:743). Bots e peers Fika observados neste cliente usam GClass2265
            // (DumbStatisticsManager) — o cast abaixo falha (retorna false) e nada é creditado,
            // o que é o comportamento correto: só o MÉDICO local pode ser creditado por este helper.
            if (doctor.StatisticsManager is GClass2266 statsManager)
            {
                // ref: Assembly-CSharp/GClass2266.cs:196-198 — ExperienceGained é virtual vazio na
                // base e não é sobrescrito pela implementação usada em raid (GClass2268); a chamada
                // é mantida por paridade com o vanilla mesmo sendo no-op na prática.
                statsManager.ExperienceGained(amount);
                doctor.Profile.EftStats.SessionCounters.AddInt(amount, SessionCounterTypesAbstractClass.ExpHeal);
                statsManager.ShowStatNotification(LocalizationKey.StatsTreatment, LocalizationKey.StatsHealed, amount);
            }
        }
    }
}
