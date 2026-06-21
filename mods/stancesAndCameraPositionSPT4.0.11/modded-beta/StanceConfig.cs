using BepInEx.Configuration;

namespace CameraRotationMod
{
    /// <summary>
    /// Conjunto de ConfigEntries de uma stance — stamina, velocidade, prone toggle e snap on fire.
    /// Indexado em Plugin._stanceConfigs por enum Stance (Default/Stance1/Stance2/Stance3).
    /// </summary>
    public sealed class StanceConfig
    {
        // StaminaMultiplier migrou para o grupo "Stamina Management" (item 012) — ver StaminaController.Multipliers.
        public ConfigEntry<bool> ModifiesMovementSpeed;
        public ConfigEntry<int> MovementSpeedMultiplier;
        public ConfigEntry<bool> ApplyWhenProne;

        /// <summary>
        /// F4 (backlog 002) — snap automático para Stance 0 ao atirar.
        ///
        /// ⚠️ **PODE SER NULL** — sentinel indicando que a stance é `Stance.Default` (não há snap em Stance 0).
        /// Callers DEVEM checar `if (cfg.SnapToStance0OnFire == null) return;` antes de acessar `.Value`,
        /// caso contrário NRE ao alternar para Stance 0. O guard padrão em
        /// <see cref="StanceManager.TryInterceptTriggerDown"/> já cobre, mas qualquer novo consumidor
        /// deve replicar o check. (ref: CR-01-05)
        /// </summary>
        public ConfigEntry<bool> SnapToStance0OnFire;
    }
}
