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
        // item 012: Multiplier/ShouldApplyStamina aposentados (o StaminaController lê os multiplicadores
        // direto de StaminaController.Multipliers). Sobra IsSuspendedByProne para o speed-limit em prone.
        public static bool IsSuspendedByProne = false;

        public static void Reset()
        {
            IsSuspendedByProne = false;
        }
    }
}
