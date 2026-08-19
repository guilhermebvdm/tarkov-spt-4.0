using System;
using ActionPOV.Core;
using ActionPOV.Patches;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

#nullable disable
namespace ActionPOV
{
    [BepInPlugin("com.trl.actionpov", "TRL-ActionPOV", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> EnableDiagnosticOverrides;

        // Física e Inércia
        public static ConfigEntry<float> CameraFollowRatio;
        public static ConfigEntry<float> WeaponWeightTime;
        public static ConfigEntry<float> SpringReturnSpeed;
        public static ConfigEntry<float> WeaponRollIntensity;

        // Cinemática CQB (Point-Shooting & Deslizamento de Coronha)
        public static ConfigEntry<float> StockSlideMultiplier;
        public static ConfigEntry<float> ArmCompressionMultiplier;
        public static ConfigEntry<float> CQBRollMultiplier;
        public static ConfigEntry<float> StrafeWalkMultiplier;

        // Modo ADS Tático (Cheek Weld & Parallax)
        public static ConfigEntry<bool> EnableADSTilt;
        public static ConfigEntry<float> ADSTiltHeadRoll;
        public static ConfigEntry<float> ADSDeadzoneHorizontal;
        public static ConfigEntry<float> ADSDeadzoneVertical;
        public static ConfigEntry<float> ADSWeightTime;
        public static ConfigEntry<float> ShotRecoilPunch;

        // Pivô do Ombro
        public static ConfigEntry<float> ShoulderPivotX;
        public static ConfigEntry<float> ShoulderPivotY;
        public static ConfigEntry<float> ShoulderPivotZ;
        public static ConfigEntry<float> ShoulderPivotMultiplier;

        // Dinâmica da Cabeça
        public static ConfigEntry<float> HeadRollIntensity;
        public static ConfigEntry<float> HeadRollMaxAngle;
        public static ConfigEntry<float> HeadPitchDelayIntensity;
        public static ConfigEntry<float> HeadYawDelayIntensity;

        // Inversão e Mapeamento de Eixos
        public static ConfigEntry<bool> InvertWeaponYaw;
        public static ConfigEntry<bool> InvertWeaponPitch;
        public static ConfigEntry<bool> InvertWeaponRoll;
        public static ConfigEntry<bool> InvertHeadRoll;
        public static ConfigEntry<bool> SwapWeaponPitchYaw;

        // Limites de Deadzone
        public static ConfigEntry<float> DeadzoneHorizontal;
        public static ConfigEntry<float> DeadzoneVertical;

        // Sliders de Diagnóstico Manual - Arma (Hands/WeaponRoot)
        public static ConfigEntry<float> DebugWeaponPosX;
        public static ConfigEntry<float> DebugWeaponPosY;
        public static ConfigEntry<float> DebugWeaponPosZ;
        public static ConfigEntry<float> DebugWeaponRotX;
        public static ConfigEntry<float> DebugWeaponRotY;
        public static ConfigEntry<float> DebugWeaponRotZ;

        // Sliders de Diagnóstico Manual - Cabeça (Camera/Eyes)
        public static ConfigEntry<float> DebugHeadRotX;
        public static ConfigEntry<float> DebugHeadRotY;
        public static ConfigEntry<float> DebugHeadRotZ;

        private void Awake()
        {
            Instance = this;

            InitConfigs();
            EnablePatches();

            Logger.LogInfo("TRL-ActionPOV (Bodycam Kinetic Physics & Axis Diagnostics) carregado com sucesso!");
        }

        private void InitConfigs()
        {
            // 1. Geral
            EnableMod = Config.Bind(
                "1. General",
                "Enable Mod",
                true,
                "Ativa ou desativa completamente o ActionPOV."
            );

            EnableDiagnosticOverrides = Config.Bind(
                "1. General",
                "Enable Diagnostic Manual Sliders",
                true,
                "Ativa a injeção dos sliders manuais de teste de eixos (Seção 9) para testar no Hideout."
            );

            // 2. Física e Inércia
            CameraFollowRatio = Config.Bind(
                "2. Physics & Inertia",
                "Immediate Camera Follow (%)",
                0.30f,
                new ConfigDescription(
                    "Fração do mouse entregue à câmera imediatamente (0.1 = mais peso na arma, 0.5 = mais direto na câmera).",
                    new AcceptableValueRange<float>(0.05f, 0.70f)
                )
            );

            WeaponWeightTime = Config.Bind(
                "2. Physics & Inertia",
                "Weapon Weight (SmoothTime)",
                0.075f,
                new ConfigDescription(
                    "Tempo de resposta da mola da arma. Valores maiores dão sensação de arma mais pesada.",
                    new AcceptableValueRange<float>(0.01f, 0.25f)
                )
            );

            SpringReturnSpeed = Config.Bind(
                "2. Physics & Inertia",
                "Spring Return Speed",
                3.5f,
                new ConfigDescription(
                    "Velocidade com que a arma reestabiliza ao centro do ombro quando o movimento cessa.",
                    new AcceptableValueRange<float>(0.5f, 15.0f)
                )
            );

            WeaponRollIntensity = Config.Bind(
                "2. Physics & Inertia",
                "Weapon Wrist Roll Intensity",
                0.15f,
                new ConfigDescription(
                    "Intensidade da torção natural da arma no pulso ao virar para os lados.",
                    new AcceptableValueRange<float>(0.0f, 1.5f)
                )
            );

            // 3. Cinemática CQB (Point-Shooting & Deslizamento de Coronha)
            StockSlideMultiplier = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Stock Slide Multiplier (m/deg)",
                0.0035f,
                new ConfigDescription(
                    "O quanto a coronha da arma escorrega para o lado oposto ao virar a mira (metros por grau).",
                    new AcceptableValueRange<float>(0.0f, 0.015f)
                )
            );

            ArmCompressionMultiplier = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Arm Depth Compression (m/deg)",
                0.0018f,
                new ConfigDescription(
                    "O quanto a arma é puxada para trás ao angular muito o cano (simula compressão de braços).",
                    new AcceptableValueRange<float>(0.0f, 0.010f)
                )
            );

            CQBRollMultiplier = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "CQB Weapon Wrist Roll",
                0.18f,
                new ConfigDescription(
                    "Torção natural da arma no próprio eixo (Roll no Y) ao apontar nas esquinas.",
                    new AcceptableValueRange<float>(0.0f, 0.60f)
                )
            );

            StrafeWalkMultiplier = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "WASD Footstep Stock Shift",
                0.015f,
                new ConfigDescription(
                    "Deslocamento lateral sutil da coronha decorrente dos passos de caminhada WASD.",
                    new AcceptableValueRange<float>(0.0f, 0.08f)
                )
            );

            // 4. Modo ADS Tático (Cheek Weld, Parallax & Recoil Punch)
            EnableADSTilt = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "Enable Tactical ADS Tilt",
                true,
                "Ativa a inclinação tática do pescoço ao colar na coronha (Cheek Weld)."
            );

            ADSTiltHeadRoll = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Head Tilt Angle (Degrees)",
                -2.5f,
                new ConfigDescription(
                    "Ângulo de inclinação lateral do pescoço ao mirar (graus).",
                    new AcceptableValueRange<float>(-8.0f, 0.0f)
                )
            );

            ADSDeadzoneHorizontal = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Deadzone Horizontal (Degrees)",
                2.0f,
                new ConfigDescription(
                    "Micro-deadzone horizontal para o retículo flutuar suavemente na ótica (graus).",
                    new AcceptableValueRange<float>(0.2f, 6.0f)
                )
            );

            ADSDeadzoneVertical = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Deadzone Vertical (Degrees)",
                1.2f,
                new ConfigDescription(
                    "Micro-deadzone vertical para o retículo flutuar suavemente na ótica (graus).",
                    new AcceptableValueRange<float>(0.2f, 5.0f)
                )
            );

            ADSWeightTime = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Weapon Weight Time",
                0.035f,
                new ConfigDescription(
                    "Tempo de resposta da mola em ADS (mais firme e responsivo para precisão).",
                    new AcceptableValueRange<float>(0.010f, 0.10f)
                )
            );

            ShotRecoilPunch = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "Shot Recoil Punch Intensity",
                0.8f,
                new ConfigDescription(
                    "Intensidade da sacudida visual na cabeça/ombro a cada disparo.",
                    new AcceptableValueRange<float>(0.0f, 3.0f)
                )
            );

            // 5. Mapeamento e Inversão de Eixos
            InvertWeaponYaw = Config.Bind(
                "5. Axis Inversion & Direction Mapping",
                "Invert Weapon Yaw (Horizontal)",
                false,
                "Inverte o sentido de giro horizontal da arma em relação ao mouse."
            );

            InvertWeaponPitch = Config.Bind(
                "4. Axis Inversion & Direction Mapping",
                "Invert Weapon Pitch (Vertical)",
                false,
                "Inverte o sentido de elevação vertical da arma em relação ao mouse."
            );

            InvertWeaponRoll = Config.Bind(
                "4. Axis Inversion & Direction Mapping",
                "Invert Weapon Roll (Wrist Twist)",
                false,
                "Inverte o sentido da torção da arma ao virar o mouse."
            );

            InvertHeadRoll = Config.Bind(
                "4. Axis Inversion & Direction Mapping",
                "Invert Head Roll",
                false,
                "Inverte a inclinação lateral da cabeça ao girar a câmera."
            );

            SwapWeaponPitchYaw = Config.Bind(
                "4. Axis Inversion & Direction Mapping",
                "Swap Weapon Pitch and Yaw",
                false,
                "Troca os eixos X e Y da arma (útil se o rig de animação do jogo estiver com eixos invertidos)."
            );

            // 5. Ancoragem do Ombro
            ShoulderPivotX = Config.Bind(
                "5. Shoulder Pivot Anchor (Calibration)",
                "Shoulder Anchor X (Lateral)",
                0.18f,
                new ConfigDescription(
                    "Posição lateral do ombro direito em relação aos olhos (metros).",
                    new AcceptableValueRange<float>(-0.5f, 0.5f)
                )
            );

            ShoulderPivotY = Config.Bind(
                "5. Shoulder Pivot Anchor (Calibration)",
                "Shoulder Anchor Y (Vertical)",
                -0.16f,
                new ConfigDescription(
                    "Posição vertical do ombro direito em relação aos olhos (metros).",
                    new AcceptableValueRange<float>(-0.5f, 0.5f)
                )
            );

            ShoulderPivotZ = Config.Bind(
                "5. Shoulder Pivot Anchor (Calibration)",
                "Shoulder Anchor Z (Depth/Forward)",
                -0.12f,
                new ConfigDescription(
                    "Profundidade do ombro direito em relação aos olhos (metros).",
                    new AcceptableValueRange<float>(-0.5f, 0.5f)
                )
            );

            ShoulderPivotMultiplier = Config.Bind(
                "5. Shoulder Pivot Anchor (Calibration)",
                "Shoulder Pivot Multiplier",
                1.0f,
                new ConfigDescription(
                    "Multiplicador geral do arco esférico do ombro.",
                    new AcceptableValueRange<float>(0.0f, 2.5f)
                )
            );

            // 6. Dinâmica da Cabeça
            HeadRollIntensity = Config.Bind(
                "6. Head Dynamics (Camera/Eyes)",
                "Head Roll Intensity",
                0.025f,
                new ConfigDescription(
                    "Intensidade de inclinação lateral da cabeça/visão durante giros rápidos do mouse.",
                    new AcceptableValueRange<float>(0.0f, 0.15f)
                )
            );

            HeadRollMaxAngle = Config.Bind(
                "6. Head Dynamics (Camera/Eyes)",
                "Head Roll Max Angle (Degrees)",
                3.5f,
                new ConfigDescription(
                    "Limite angular máximo da inclinação lateral da cabeça.",
                    new AcceptableValueRange<float>(0.0f, 15.0f)
                )
            );

            HeadPitchDelayIntensity = Config.Bind(
                "6. Head Dynamics (Camera/Eyes)",
                "Head Pitch Lag Intensity",
                0.0f,
                new ConfigDescription(
                    "Atraso/lag vertical da cabeça ao olhar para cima/baixo (0 = desativado).",
                    new AcceptableValueRange<float>(0.0f, 0.10f)
                )
            );

            HeadYawDelayIntensity = Config.Bind(
                "6. Head Dynamics (Camera/Eyes)",
                "Head Yaw Lag Intensity",
                0.0f,
                new ConfigDescription(
                    "Atraso/lag horizontal da cabeça ao olhar para os lados (0 = desativado).",
                    new AcceptableValueRange<float>(0.0f, 0.10f)
                )
            );

            // 7. Deadzone Box
            DeadzoneHorizontal = Config.Bind(
                "7. Deadzone Box Limits",
                "Deadzone Horizontal (Degrees)",
                12.0f,
                new ConfigDescription(
                    "Amplitude horizontal máxima do cone de mira livre em graus.",
                    new AcceptableValueRange<float>(1.0f, 25.0f)
                )
            );

            DeadzoneVertical = Config.Bind(
                "7. Deadzone Box Limits",
                "Deadzone Vertical (Degrees)",
                8.0f,
                new ConfigDescription(
                    "Amplitude vertical máxima do cone de mira livre em graus.",
                    new AcceptableValueRange<float>(1.0f, 20.0f)
                )
            );

            // 9. Diagnósticos Manuais - Posição da Arma
            DebugWeaponPosX = Config.Bind(
                "9. Diagnostics - Weapon Position (Hands)",
                "Weapon Pos X (Lateral)",
                0.0f,
                new ConfigDescription("Mova para testar o que o eixo X faz na posição da arma.", new AcceptableValueRange<float>(-0.5f, 0.5f))
            );

            DebugWeaponPosY = Config.Bind(
                "9. Diagnostics - Weapon Position (Hands)",
                "Weapon Pos Y (Vertical)",
                0.0f,
                new ConfigDescription("Mova para testar o que o eixo Y faz na posição da arma.", new AcceptableValueRange<float>(-0.5f, 0.5f))
            );

            DebugWeaponPosZ = Config.Bind(
                "9. Diagnostics - Weapon Position (Hands)",
                "Weapon Pos Z (Depth)",
                0.0f,
                new ConfigDescription("Mova para testar o que o eixo Z faz na posição da arma.", new AcceptableValueRange<float>(-0.5f, 0.5f))
            );

            // 9. Diagnósticos Manuais - Rotação da Arma
            DebugWeaponRotX = Config.Bind(
                "9. Diagnostics - Weapon Rotation (Hands)",
                "Weapon Rot X (Euler Pitch?)",
                0.0f,
                new ConfigDescription("Gire para testar qual eixo roda a arma no Euler X.", new AcceptableValueRange<float>(-45f, 45f))
            );

            DebugWeaponRotY = Config.Bind(
                "9. Diagnostics - Weapon Rotation (Hands)",
                "Weapon Rot Y (Euler Yaw?)",
                0.0f,
                new ConfigDescription("Gire para testar qual eixo roda a arma no Euler Y.", new AcceptableValueRange<float>(-45f, 45f))
            );

            DebugWeaponRotZ = Config.Bind(
                "9. Diagnostics - Weapon Rotation (Hands)",
                "Weapon Rot Z (Euler Roll?)",
                0.0f,
                new ConfigDescription("Gire para testar qual eixo roda a arma no Euler Z.", new AcceptableValueRange<float>(-45f, 45f))
            );

            // 9. Diagnósticos Manuais - Rotação da Cabeça
            DebugHeadRotX = Config.Bind(
                "9. Diagnostics - Head Rotation (Camera)",
                "Head Rot X (Pitch?)",
                0.0f,
                new ConfigDescription("Gire para testar o que o eixo X faz na cabeça do player.", new AcceptableValueRange<float>(-45f, 45f))
            );

            DebugHeadRotY = Config.Bind(
                "9. Diagnostics - Head Rotation (Camera)",
                "Head Rot Y (Yaw?)",
                0.0f,
                new ConfigDescription("Gire para testar o que o eixo Y faz na cabeça do player.", new AcceptableValueRange<float>(-45f, 45f))
            );

            DebugHeadRotZ = Config.Bind(
                "9. Diagnostics - Head Rotation (Camera)",
                "Head Rot Z (Roll?)",
                0.0f,
                new ConfigDescription("Gire para testar o que o eixo Z faz na cabeça do player.", new AcceptableValueRange<float>(-45f, 45f))
            );

            SyncEngineConfigs();
        }

        private void Update()
        {
            SyncEngineConfigs();
        }

        private void SyncEngineConfigs()
        {
            KineticSpringEngine.CameraFollowRatio = CameraFollowRatio.Value;
            KineticSpringEngine.WeaponWeightTime = WeaponWeightTime.Value;
            KineticSpringEngine.SpringReturnSpeed = SpringReturnSpeed.Value;

            KineticSpringEngine.StockSlideMultiplier = StockSlideMultiplier.Value;
            KineticSpringEngine.ArmCompressionMultiplier = ArmCompressionMultiplier.Value;
            KineticSpringEngine.CQBRollMultiplier = CQBRollMultiplier.Value;
            KineticSpringEngine.StrafeWalkMultiplier = StrafeWalkMultiplier.Value;

            KineticSpringEngine.EnableADSTilt = EnableADSTilt.Value;
            KineticSpringEngine.ADSTiltHeadRoll = ADSTiltHeadRoll.Value;
            KineticSpringEngine.ADSDeadzoneLimits = new Vector2(ADSDeadzoneHorizontal.Value, ADSDeadzoneVertical.Value);
            KineticSpringEngine.ADSWeightTime = ADSWeightTime.Value;
            KineticSpringEngine.ShotRecoilPunch = ShotRecoilPunch.Value;

            KineticSpringEngine.CustomShoulderPivot = new Vector3(ShoulderPivotX.Value, ShoulderPivotY.Value, ShoulderPivotZ.Value);

            KineticSpringEngine.HeadRollIntensity = HeadRollIntensity.Value;
            KineticSpringEngine.MaxHeadRoll = HeadRollMaxAngle.Value;
            KineticSpringEngine.HeadPitchDelayIntensity = HeadPitchDelayIntensity.Value;
            KineticSpringEngine.HeadYawDelayIntensity = HeadYawDelayIntensity.Value;

            KineticSpringEngine.InvertWeaponYaw = InvertWeaponYaw.Value;
            KineticSpringEngine.InvertWeaponPitch = InvertWeaponPitch.Value;
            KineticSpringEngine.InvertWeaponRoll = InvertWeaponRoll.Value;
            KineticSpringEngine.InvertHeadRoll = InvertHeadRoll.Value;
            KineticSpringEngine.SwapWeaponPitchYaw = SwapWeaponPitchYaw.Value;

            KineticSpringEngine.DeadzoneLimits = new Vector2(DeadzoneHorizontal.Value, DeadzoneVertical.Value);
        }

        private void EnablePatches()
        {
            try
            {
                new Patch_PlayerRotate().Enable();
                new Patch_SetHeadRotation().Enable();
                new Patch_CalculateCameraPosition().Enable();
                new Patch_UpdateSwayFactors().Enable();
                new Patch_OnMakingShot().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro ao ativar patches do ActionPOV: {ex}");
            }
        }
    }
}
