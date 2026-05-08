namespace CameraRotationMod
{
    public enum EStanceStaminaMode
    {
        None,
        Drain,
        Recovery,
    }

    /// <summary>
    /// Estado leitura-rápida da stance ativa para fins de stamina/velocidade.
    /// Atualizado pelo StanceManager; consultado pelo tick de drain e pelo postfix
    /// de Recovery todo frame. Resetado em OnRaidStart e OnRaidEnd.
    /// </summary>
    public static class StanceStaminaState
    {
        public static EStanceStaminaMode Mode = EStanceStaminaMode.None;
        public static float Intensity = 1f;
        public static bool IsSuspendedByProne = false;

        public static bool ShouldApplyStamina => Mode != EStanceStaminaMode.None && !IsSuspendedByProne;

        public static void Reset()
        {
            Mode = EStanceStaminaMode.None;
            Intensity = 1f;
            IsSuspendedByProne = false;
        }
    }
}
