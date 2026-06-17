namespace CameraRotationMod
{
    /// <summary>
    /// Estado leitura-rápida da stance ativa para fins de stamina/velocidade.
    /// Atualizado pelo StanceManager; consultado pelo tick e pelo postfix de stamina todo frame.
    /// Resetado em OnRaidStart e OnRaidEnd.
    /// Multiplier: &lt;1.0 = drain, 1.0 = vanilla, &gt;1.0 = recovery.
    /// ref: fix-01 — StaminaMode+Intensity unificados em StaminaMultiplier.
    /// </summary>
    public static class StanceStaminaState
    {
        public static float Multiplier = 1f;
        public static bool IsSuspendedByProne = false;

        // 1.0 = vanilla (nenhuma intervenção do mod)
        public static bool ShouldApplyStamina =>
            System.Math.Abs(Multiplier - 1.0f) > 1e-5f && !IsSuspendedByProne;

        public static void Reset()
        {
            Multiplier = 1f;
            IsSuspendedByProne = false;
        }
    }
}
