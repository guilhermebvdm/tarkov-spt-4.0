using BepInEx.Configuration;

namespace CameraRotationMod
{
    /// <summary>
    /// Conjunto de ConfigEntries de uma stance — stamina, velocidade e prone toggle.
    /// Indexado em Plugin._stanceConfigs por enum Stance (Default/Stance1/Stance2/Stance3).
    /// </summary>
    public sealed class StanceConfig
    {
        public ConfigEntry<EStanceStaminaMode> StaminaMode;
        public ConfigEntry<float> StaminaIntensity;
        public ConfigEntry<bool> ModifiesMovementSpeed;
        public ConfigEntry<int> MovementSpeedMultiplier;
        public ConfigEntry<bool> ApplyWhenProne;
    }
}
