using EFT;

namespace CameraRotationMod
{
    /// <summary>
    /// Modo ÚNICO que controla a stamina de braço (HandsStamina) neste frame — item 011 / fix-06-01.
    /// Resolve o cabo-de-guerra entre o tick de stance, a restauração vanilla, o mount ativo/passivo e o
    /// hold-breath: exatamente UMA fonte age por frame, por prioridade decrescente. Todos os consumidores
    /// (StanceStaminaRecoveryPatch, HandsStaminaConsumePatch, TickStanceStamina, ApplyComplexRotationPatch)
    /// consultam <see cref="ArmStamina.Resolve"/> e só agem no seu modo.
    /// </summary>
    public enum ArmStaminaMode
    {
        Vanilla,      // EFT controla (ADS sem mount, hipfire neutro)
        Prone,        // deitado suspenso — vanilla congela (Consume/Process patches)
        MountActive,  // mount vanilla (IsMountedState)
        MountPassive, // mount passivo encostado (item 011), com Passive Stamina Save ligado
        HoldBreath,   // segurando a respiração sem mount — drain extra
        StanceDrain   // multiplier de stance em hipfire — o tick aplica o delta
    }

    public static class ArmStamina
    {
        // Prioridade: Prone > MountActive > MountPassive > HoldBreath > ADS(vanilla) > StanceDrain > Vanilla.
        public static ArmStaminaMode Resolve(Player p)
        {
            if (p == null) return ArmStaminaMode.Vanilla;

            // 1. Prone suspenso (ApplyWhenProne off) — vanilla congela
            if (p.IsInPronePose && StanceStaminaState.IsSuspendedByProne)
                return ArmStaminaMode.Prone;

            var pwa = p.ProceduralWeaponAnimation;

            // 2. Mount ativo (vanilla)
            if (pwa != null && pwa.IsMountedState)
                return ArmStaminaMode.MountActive;

            // 3. Mount passivo (item 011) — só captura a stamina se o save estiver ligado
            if (PassiveMountState.IsBracing
                && Plugin._EnablePassiveMount.Value
                && Plugin._PassiveStaminaSave.Value)
                return ArmStaminaMode.MountPassive;

            // 4. Hold breath sem mount — drain extra (a respiração cansa)
            if (p.Physical != null && p.Physical.HoldingBreath)
                return ArmStaminaMode.HoldBreath;

            // 5. ADS sem mount — vanilla (EFT controla o aim-drain)
            if (pwa != null && pwa.IsAiming)
                return ArmStaminaMode.Vanilla;

            // 6. Multiplier de stance em hipfire — o tick aplica o delta
            if (StanceStaminaState.ShouldApplyStamina)
                return ArmStaminaMode.StanceDrain;

            return ArmStaminaMode.Vanilla;
        }
    }
}
