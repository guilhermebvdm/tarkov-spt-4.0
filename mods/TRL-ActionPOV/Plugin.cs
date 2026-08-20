using System;
using ActionPOV.Core;
using ActionPOV.Patches;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using UnityEngine;

#nullable disable
namespace ActionPOV
{
    [BepInPlugin("com.trl.actionpov", "TRL-ActionPOV", "1.4.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        public static ConfigEntry<bool> EnableMod;
        public static ConfigEntry<bool> EnableDiagnosticOverrides;

        // Física e Inércia
        public static ConfigEntry<float> CameraFollowRatio;
        public static ConfigEntry<float> WeaponWeightTime;
        public static ConfigEntry<float> WalkForwardDelaySeconds;
        public static ConfigEntry<float> WalkForwardRealignSpeed;
        public static ConfigEntry<float> SpringReturnSpeed;
        public static ConfigEntry<float> WeaponRollIntensity;

        // Cinemática CQB (Point-Shooting & Deslizamento de Coronha)
        public static ConfigEntry<float> StockSlideHorizontalMax;
        public static ConfigEntry<float> LeftStockSlideMultiplier;
        public static ConfigEntry<float> StockSlideVerticalMax;
        public static ConfigEntry<float> StockSmoothTimeHorizontal;
        public static ConfigEntry<float> StockSmoothTimeVertical;
        public static ConfigEntry<float> ArmCompressionMultiplier;
        public static ConfigEntry<float> CQBRollMultiplier;
        public static ConfigEntry<float> StrafeWalkMultiplier;

        // Modo ADS Tático (Cheek Weld, Parallax & Sight Alignment)
        public static ConfigEntry<bool> EnableADSTilt;
        public static ConfigEntry<float> ADSTiltHeadRoll;
        public static ConfigEntry<float> ADSDeadzoneHorizontal;
        public static ConfigEntry<float> ADSDeadzoneVertical;
        public static ConfigEntry<float> EyeToSightDistance;
        public static ConfigEntry<float> ADSSightAlignmentFactor;
        public static ConfigEntry<float> StockSlideHorizontalADS;
        public static ConfigEntry<float> StockSlideVerticalADS;
        public static ConfigEntry<float> ADSFrontSightSmoothTime;
        public static ConfigEntry<float> StockSmoothTimeHorizontalADS;
        public static ConfigEntry<float> StockSmoothTimeVerticalADS;

        // Coice Físico do Disparo (Weapon Kickback & Recoil Punch)
        public static ConfigEntry<bool> EnableRecoilKick;
        public static ConfigEntry<float> RecoilKickZ_Hipfire;
        public static ConfigEntry<float> RecoilKickZ_ADS;
        public static ConfigEntry<float> RecoilMuzzleRise_Hipfire;
        public static ConfigEntry<float> RecoilMuzzleRise_ADS;
        public static ConfigEntry<float> RecoilRecoveryTime;
        public static ConfigEntry<float> RecoilHeadPunchIntensity;

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
        public static ConfigEntry<bool> InvertStockHorizontal;
        public static ConfigEntry<bool> InvertStockVertical;
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
            StockSlideHorizontalMax = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Stock Slide Horizontal Max (Meters)",
                0.035f,
                new ConfigDescription(
                    "Deslocamento lateral máximo da coronha no ombro (metros). Ex: 0.035 = 3.5cm.",
                    new AcceptableValueRange<float>(0.0f, 0.400f)
                )
            );

            LeftStockSlideMultiplier = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Left Sway Stock Slide Multiplier",
                0.35f,
                new ConfigDescription(
                    "Multiplicador de amplitude do sway da coronha ao virar para a ESQUERDA (0.35 = 35% do percurso normal).",
                    new AcceptableValueRange<float>(0.0f, 1.0f)
                )
            );

            StockSlideVerticalMax = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Stock Slide Vertical Max (Meters)",
                0.020f,
                new ConfigDescription(
                    "Deslocamento vertical máximo da coronha no ombro ao mirar para cima/baixo (metros).",
                    new AcceptableValueRange<float>(0.0f, 0.300f)
                )
            );

            StockSmoothTimeHorizontal = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Stock Smooth Time Horizontal (Inertia)",
                0.25f,
                new ConfigDescription(
                    "Tempo de acomodação lateral da coronha no ombro (segundos). Maior = Mais suave e pesado.",
                    new AcceptableValueRange<float>(0.02f, 1.00f)
                )
            );

            StockSmoothTimeVertical = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Stock Smooth Time Vertical (Inertia)",
                0.38f,
                new ConfigDescription(
                    "Tempo de acomodação vertical da coronha no ombro (segundos). Maior = Mais firme e pesado.",
                    new AcceptableValueRange<float>(0.02f, 1.00f)
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
                0.22f,
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

            WalkForwardDelaySeconds = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Walk Forward Delay (Seconds)",
                1.0f,
                new ConfigDescription(
                    "Tempo contínuo andando para frente (W) antes de iniciar a centralização (segundos).",
                    new AcceptableValueRange<float>(0.2f, 5.0f)
                )
            );

            WalkForwardRealignSpeed = Config.Bind(
                "3. CQB Point-Shooting (Stock Slide)",
                "Walk Forward Realign Speed",
                6.0f,
                new ConfigDescription(
                    "Velocidade de centralização suave da arma e da visão quando o jogador avança para frente (W).",
                    new AcceptableValueRange<float>(1.0f, 25.0f)
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
                2.5f,
                new ConfigDescription(
                    "Ângulo de inclinação lateral do pescoço ao mirar (graus).",
                    new AcceptableValueRange<float>(-8.0f, 8.0f)
                )
            );

            ADSDeadzoneHorizontal = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Deadzone Horizontal (Degrees)",
                2.5f,
                new ConfigDescription(
                    "Micro-deadzone horizontal para o retículo flutuar suavemente na ótica (graus).",
                    new AcceptableValueRange<float>(0.2f, 6.0f)
                )
            );

            ADSDeadzoneVertical = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Deadzone Vertical (Degrees)",
                1.5f,
                new ConfigDescription(
                    "Micro-deadzone vertical para o retículo flutuar suavemente na ótica (graus).",
                    new AcceptableValueRange<float>(0.2f, 5.0f)
                )
            );

            EyeToSightDistance = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "Eye to Sight Distance (Meters)",
                0.35f,
                new ConfigDescription(
                    "Distância focal média do olho até a alça/massa de mira (metros). Usado no cálculo de alinhamento óptico concêntrico.",
                    new AcceptableValueRange<float>(0.15f, 0.60f)
                )
            );

            ADSSightAlignmentFactor = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Sight Alignment Factor",
                0.85f,
                new ConfigDescription(
                    "Fator de alinhamento óptico da alça e massa no ADS: 0.0 = sem correção (livre) | 1.0 = alinhamento geométrico estrito.",
                    new AcceptableValueRange<float>(0.0f, 1.50f)
                )
            );

            StockSlideHorizontalADS = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Stock Slide Horizontal Max",
                0.010f,
                new ConfigDescription(
                    "Deslocamento lateral da coronha no ADS para criar parallax óptico na lente (metros).",
                    new AcceptableValueRange<float>(0.0f, 0.150f)
                )
            );

            StockSlideVerticalADS = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Stock Slide Vertical Max",
                0.006f,
                new ConfigDescription(
                    "Deslocamento vertical da coronha no ADS para criar parallax óptico na lente (metros).",
                    new AcceptableValueRange<float>(0.0f, 0.100f)
                )
            );

            ADSFrontSightSmoothTime = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Front Sight Smooth Time (Cano)",
                0.035f,
                new ConfigDescription(
                    "Tempo de resposta do cano/massa de mira no ADS (segundos). Rápido para guiar o tiro.",
                    new AcceptableValueRange<float>(0.010f, 0.10f)
                )
            );

            StockSmoothTimeHorizontalADS = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Rear Sight Smooth Time Horizontal (Alça)",
                0.12f,
                new ConfigDescription(
                    "Tempo de resposta lateral da coronha/alça de mira no ADS (segundos). Gera o atraso de catch-up.",
                    new AcceptableValueRange<float>(0.02f, 0.50f)
                )
            );

            StockSmoothTimeVerticalADS = Config.Bind(
                "4. Tactical ADS (Cheek Weld & Parallax)",
                "ADS Rear Sight Smooth Time Vertical (Alça)",
                0.20f,
                new ConfigDescription(
                    "Tempo de resposta vertical da coronha/alça de mira no ADS (segundos). Gera o atraso de catch-up.",
                    new AcceptableValueRange<float>(0.02f, 0.50f)
                )
            );

            // 6. Coice Físico do Disparo (Weapon Kickback & Recoil Punch)
            EnableRecoilKick = Config.Bind(
                "6. Weapon Shot Recoil & Kickback",
                "Enable Weapon Kickback",
                true,
                "Ativa o coice e impacto mecânico do disparo na arma e na visão."
            );

            RecoilKickZ_Hipfire = Config.Bind(
                "6. Weapon Shot Recoil & Kickback",
                "Recoil Kick Z Hipfire (Meters)",
                0.030f,
                new ConfigDescription(
                    "Distância que a arma recua para trás contra o ombro/peito no Hipfire (metros).",
                    new AcceptableValueRange<float>(0.0f, 0.100f)
                )
            );

            RecoilKickZ_ADS = Config.Bind(
                "6. Weapon Shot Recoil & Kickback",
                "Recoil Kick Z ADS (Meters)",
                0.012f,
                new ConfigDescription(
                    "Distância que a arma recua para trás contra o ombro no ADS (metros). Mais firme pelo apoio.",
                    new AcceptableValueRange<float>(0.0f, 0.050f)
                )
            );

            RecoilMuzzleRise_Hipfire = Config.Bind(
                "6. Weapon Shot Recoil & Kickback",
                "Recoil Muzzle Rise Hipfire (Degrees)",
                1.8f,
                new ConfigDescription(
                    "Elevação angular instantânea do cano no disparo em Hipfire (graus).",
                    new AcceptableValueRange<float>(0.0f, 8.0f)
                )
            );

            RecoilMuzzleRise_ADS = Config.Bind(
                "6. Weapon Shot Recoil & Kickback",
                "Recoil Muzzle Rise ADS (Degrees)",
                0.8f,
                new ConfigDescription(
                    "Elevação angular instantânea do cano no disparo em ADS (graus).",
                    new AcceptableValueRange<float>(0.0f, 4.0f)
                )
            );

            RecoilRecoveryTime = Config.Bind(
                "6. Weapon Shot Recoil & Kickback",
                "Recoil Recovery Time (Seconds)",
                0.08f,
                new ConfigDescription(
                    "Tempo de recuperação elástica da mola após o coice do disparo (segundos).",
                    new AcceptableValueRange<float>(0.02f, 0.30f)
                )
            );

            RecoilHeadPunchIntensity = Config.Bind(
                "6. Weapon Shot Recoil & Kickback",
                "Recoil Head Punch Intensity",
                0.8f,
                new ConfigDescription(
                    "Intensidade do solavanco visual na cabeça do operador a cada disparo.",
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
                "5. Axis Inversion & Direction Mapping",
                "Invert Weapon Pitch (Vertical)",
                false,
                "Inverte o sentido de elevação vertical da arma em relação ao mouse."
            );

            InvertWeaponRoll = Config.Bind(
                "5. Axis Inversion & Direction Mapping",
                "Invert Weapon Roll (Wrist Twist)",
                false,
                "Inverte o sentido da torção da arma ao virar o mouse."
            );

            InvertHeadRoll = Config.Bind(
                "5. Axis Inversion & Direction Mapping",
                "Invert Head Roll",
                false,
                "Inverte a inclinação lateral da cabeça ao girar a câmera."
            );

            InvertStockHorizontal = Config.Bind(
                "5. Axis Inversion & Direction Mapping",
                "Invert Stock Horizontal Shift",
                false,
                "Inverte a direção do deslizamento lateral da coronha."
            );

            InvertStockVertical = Config.Bind(
                "5. Axis Inversion & Direction Mapping",
                "Invert Stock Vertical Shift",
                false,
                "Inverte a direção do deslizamento vertical da coronha."
            );

            SwapWeaponPitchYaw = Config.Bind(
                "5. Axis Inversion & Direction Mapping",
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

            ShowLaserSpyHUD = Config.Bind(
                "9. Diagnostics",
                "Show Laser Spy HUD",
                true,
                new ConfigDescription("Exibe HUD de telemetria em tempo real do Laser na tela (Pressione F10 para alternar).")
            );

            SyncEngineConfigs();
        }

        public static ConfigEntry<bool> ShowLaserSpyHUD;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                ShowLaserSpyHUD.Value = !ShowLaserSpyHUD.Value;
            }
            SyncEngineConfigs();
        }

        private void OnGUI()
        {
            if (!EnableMod.Value || !ShowLaserSpyHUD.Value) return;

            var myPlayer = GamePlayerOwner.MyPlayer;
            if (myPlayer == null) return;

            GUI.color = Color.white;
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.88f);

            GUILayout.BeginArea(new Rect(20, 130, 520, 270), GUI.skin.box);
            GUILayout.Label("<color=#00FFAA><b>=== TRL-ActionPOV — LASER TELEMETRY SPY (F10) ===</b></color>");

            string hookStatus = (LaserSpy.CallsPerSecond > 0) ? $"<color=#00FF00>ATIVO ({LaserSpy.CallsPerSecond} fps - {LaserSpy.DistinctInstancesCount} instâncias ativas)</color>" : "<color=#FF3333>INATIVO (0 fps - Laser Desligado)</color>";
            GUILayout.Label($"<b>Status do Hook:</b> {hookStatus}");
            GUILayout.Label($"<b>Objeto / Hierarquia:</b> <color=#00E5FF>{LaserSpy.LastHierarchyPath}</color>");
            GUILayout.Label($"<b>Dono do Laser (isYourPlayer):</b> {(LaserSpy.IsYourPlayer ? "<color=#00FF00>SIM (Jogador Local)</color>" : "<color=#FFFF00>NÃO</color>")}");

            GUILayout.Space(4);
            GUILayout.Label($"<b>Direção Balística do Cano (WeaponDir):</b> {LaserSpy.WeaponDirection.x:F3}, {LaserSpy.WeaponDirection.y:F3}, {LaserSpy.WeaponDirection.z:F3}");
            GUILayout.Label($"<b>Direção Nativa da Lente (Transform.fwd):</b> {LaserSpy.TransformForward.x:F3}, {LaserSpy.TransformForward.y:F3}, {LaserSpy.TransformForward.z:F3}");
            GUILayout.Label($"<b>Desvio Angular Real:</b> <color=#FFCC00><b>{LaserSpy.DeltaAngle:F2}°</b></color>");

            GUILayout.Space(4);
            GUILayout.Label($"<b>Distância até a Parede/Alvo:</b> {LaserSpy.HitDistance:F2}m");
            GUILayout.Label($"<b>Ponto de Impacto (Hit Point):</b> ({LaserSpy.HitPoint.x:F2}, {LaserSpy.HitPoint.y:F2}, {LaserSpy.HitPoint.z:F2})");

            GUILayout.EndArea();
        }

        private void SyncEngineConfigs()
        {
            KineticSpringEngine.CameraFollowRatio = CameraFollowRatio.Value;
            KineticSpringEngine.WeaponWeightTime = WeaponWeightTime.Value;
            KineticSpringEngine.WalkForwardDelaySeconds = WalkForwardDelaySeconds.Value;
            KineticSpringEngine.WalkForwardRealignSpeed = WalkForwardRealignSpeed.Value;

            KineticSpringEngine.StockSlideHorizontalMax = StockSlideHorizontalMax.Value;
            KineticSpringEngine.LeftStockSlideMultiplier = LeftStockSlideMultiplier.Value;
            KineticSpringEngine.StockSlideVerticalMax = StockSlideVerticalMax.Value;
            KineticSpringEngine.StockSmoothTimeHorizontal = StockSmoothTimeHorizontal.Value;
            KineticSpringEngine.StockSmoothTimeVertical = StockSmoothTimeVertical.Value;
            KineticSpringEngine.ArmCompressionMultiplier = ArmCompressionMultiplier.Value;
            KineticSpringEngine.CQBRollMultiplier = CQBRollMultiplier.Value;
            KineticSpringEngine.StrafeWalkMultiplier = StrafeWalkMultiplier.Value;

            KineticSpringEngine.EnableADSTilt = EnableADSTilt.Value;
            KineticSpringEngine.ADSTiltHeadRoll = ADSTiltHeadRoll.Value;
            KineticSpringEngine.ADSDeadzoneLimits = new Vector2(ADSDeadzoneHorizontal.Value, ADSDeadzoneVertical.Value);
            KineticSpringEngine.EyeToSightDistance = EyeToSightDistance.Value;
            KineticSpringEngine.ADSSightAlignmentFactor = ADSSightAlignmentFactor.Value;
            KineticSpringEngine.StockSlideHorizontalADS = StockSlideHorizontalADS.Value;
            KineticSpringEngine.StockSlideVerticalADS = StockSlideVerticalADS.Value;
            KineticSpringEngine.ADSFrontSightSmoothTime = ADSFrontSightSmoothTime.Value;
            KineticSpringEngine.StockSmoothTimeHorizontalADS = StockSmoothTimeHorizontalADS.Value;
            KineticSpringEngine.StockSmoothTimeVerticalADS = StockSmoothTimeVerticalADS.Value;
            KineticSpringEngine.EnableRecoilKick = EnableRecoilKick.Value;
            KineticSpringEngine.RecoilKickZ_Hipfire = RecoilKickZ_Hipfire.Value;
            KineticSpringEngine.RecoilKickZ_ADS = RecoilKickZ_ADS.Value;
            KineticSpringEngine.RecoilMuzzleRise_Hipfire = RecoilMuzzleRise_Hipfire.Value;
            KineticSpringEngine.RecoilMuzzleRise_ADS = RecoilMuzzleRise_ADS.Value;
            KineticSpringEngine.RecoilRecoveryTime = RecoilRecoveryTime.Value;
            KineticSpringEngine.RecoilHeadPunchIntensity = RecoilHeadPunchIntensity.Value;

            KineticSpringEngine.CustomShoulderPivot = new Vector3(ShoulderPivotX.Value, ShoulderPivotY.Value, ShoulderPivotZ.Value);

            KineticSpringEngine.HeadRollIntensity = HeadRollIntensity.Value;
            KineticSpringEngine.MaxHeadRoll = HeadRollMaxAngle.Value;
            KineticSpringEngine.HeadPitchDelayIntensity = HeadPitchDelayIntensity.Value;
            KineticSpringEngine.HeadYawDelayIntensity = HeadYawDelayIntensity.Value;

            KineticSpringEngine.InvertWeaponYaw = InvertWeaponYaw.Value;
            KineticSpringEngine.InvertWeaponPitch = InvertWeaponPitch.Value;
            KineticSpringEngine.InvertWeaponRoll = InvertWeaponRoll.Value;
            KineticSpringEngine.InvertHeadRoll = InvertHeadRoll.Value;
            KineticSpringEngine.InvertStockHorizontal = InvertStockHorizontal.Value;
            KineticSpringEngine.InvertStockVertical = InvertStockVertical.Value;
            KineticSpringEngine.SwapWeaponPitchYaw = SwapWeaponPitchYaw.Value;

            KineticSpringEngine.DeadzoneLimits = new Vector2(DeadzoneHorizontal.Value, DeadzoneVertical.Value);
        }

        private void EnablePatches()
        {
            try
            {
                new Patch_PlayerRotate().Enable();
                new Patch_SetHeadRotation().Enable();
                new Patch_WeaponRootAnimTransform().Enable();
                new Patch_UpdateSwayFactors().Enable();
                new Patch_OnMakingShot().Enable();
                new Patch_LaserBeam_FireportSync().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro ao ativar patches do ActionPOV: {ex}");
            }
        }
    }
}
