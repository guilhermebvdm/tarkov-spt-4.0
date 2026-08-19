using UnityEngine;

#nullable disable
namespace ActionPOV.Core
{
    public static class KineticSpringEngine
    {
        // Ponto base da cavidade do ombro direito relativo ao centro focal dos olhos
        public static Vector3 CustomShoulderPivot = new Vector3(0.18f, -0.16f, -0.12f);

        // Estado Angular da Mola da Arma (X = Pitch/Cano, Y = Roll/Torção, Z = Yaw/Direção)
        public static Vector3 TargetWeaponAngle = Vector3.zero;
        public static Vector3 CurrentWeaponAngle = Vector3.zero;
        private static Vector3 _weaponAngleVelocity = Vector3.zero;

        // Estado Posicional da Arma (X = Deslizamento Lateral da Coronha, Z = Compressão de Braço)
        public static Vector3 TargetWeaponPos = Vector3.zero;
        public static Vector3 CurrentWeaponPos = Vector3.zero;
        private static Vector3 _weaponPosVelocity = Vector3.zero;

        // Estado do Head Roll e Inclinação Tática
        public static float CurrentHeadRoll = 0f;
        public static float CurrentHeadPitch = 0f;
        public static float CurrentHeadYaw = 0f;
        private static float _headRollVelocity = 0f;
        private static float _headPitchVelocity = 0f;
        private static float _headYawVelocity = 0f;

        private static float _currentHeadTiltADS = 0f;
        private static float _headTiltVelocity = 0f;

        // Armazenamento do último delta real para cálculo de velocidade
        public static Vector2 LastFrameDelta = Vector2.zero;

        // Parâmetros de Configuração (ajustáveis pelo Plugin / F12)
        public static float CameraFollowRatio = 0.25f;          // Fração do mouse entregue à câmera no hipfire (25% câmera / 75% arma)
        public static float WeaponWeightTime = 0.060f;          // Tempo de resposta da mola em Hipfire
        public static float SpringReturnSpeed = 3.5f;           // Velocidade de retorno ao centro quando o mouse para
        
        // --- CINEMÁTICA CQB (POINT-SHOOTING / WEAPON LEAD) ---
        public static float StockSlideMultiplier = 0.0035f;     // Deslocamento da coronha por grau de mira (metros/grau)
        public static float ArmCompressionMultiplier = 0.0018f; // Compressão de braço para trás ao angular (metros/grau)
        public static float CQBRollMultiplier = 0.18f;          // Torção da arma no eixo Y nas esquinas
        public static float StrafeWalkMultiplier = 0.015f;      // Inércia lateral adicional pelos passos WASD

        // --- MODO ADS TÁTICO ESTILO BODYCAM ---
        public static bool EnableADSTilt = true;                // Ativa a inclinação tática do pescoço ao mirar
        public static float ADSTiltHeadRoll = -2.5f;            // Ângulo de inclinação da cabeça ao colar na coronha (graus)
        public static Vector2 ADSDeadzoneLimits = new Vector2(2.0f, 1.2f); // Micro-deadzone para parallax dinâmico no retículo
        public static float ADSWeightTime = 0.035f;             // Mola mais firme e precisa durante o ADS
        public static float ShotRecoilPunch = 0.8f;             // Sacudida visual no ombro e cabeça por disparo

        public static float HeadRollIntensity = 0.025f;         // Força de inclinação lateral da cabeça
        public static float MaxHeadRoll = 3.5f;                 // Limite máximo do roll em graus
        public static float HeadRecoveryTime = 0.10f;           // Tempo de recuperação do roll

        public static float HeadPitchDelayIntensity = 0.0f;     // Lag vertical da cabeça
        public static float HeadYawDelayIntensity = 0.0f;       // Lag horizontal da cabeça

        // Inversão e Mapeamento de Eixos
        public static bool InvertWeaponYaw = false;
        public static bool InvertWeaponPitch = false;
        public static bool InvertWeaponRoll = false;
        public static bool InvertHeadRoll = false;
        public static bool SwapWeaponPitchYaw = false;

        public static Vector2 DeadzoneLimits = new Vector2(14f, 8f); // Amplitude lateral ampla para CQB (graus)

        public static void ProcessMouseInput(ref Vector2 deltaRotation, bool isAiming)
        {
            LastFrameDelta = deltaRotation;

            if (isAiming)
            {
                // 1. Em ADS: 60% comanda o giro da visão; 40% gera a micro-inércia/parallax do retículo
                float cameraSplitADS = 0.60f;
                Vector2 weaponInput = deltaRotation * (1f - cameraSplitADS);
                deltaRotation *= cameraSplitADS;

                float inputYaw = weaponInput.x * (InvertWeaponYaw ? -1f : 1f);
                float inputPitch = weaponInput.y * (InvertWeaponPitch ? 1f : -1f);

                if (SwapWeaponPitchYaw)
                {
                    float temp = inputYaw;
                    inputYaw = inputPitch;
                    inputPitch = temp;
                }

                // Micro-ajustes da mira dentro da ótica
                TargetWeaponAngle.z += inputYaw;
                TargetWeaponAngle.x -= inputPitch;

                // Micro-Deadzone para o retículo flutuar suavemente sem perder o alvo
                TargetWeaponAngle.z = Mathf.Clamp(TargetWeaponAngle.z, -ADSDeadzoneLimits.x, ADSDeadzoneLimits.x);
                TargetWeaponAngle.x = Mathf.Clamp(TargetWeaponAngle.x, -ADSDeadzoneLimits.y, ADSDeadzoneLimits.y);

                // Micro-roll sutil
                float rollMult = InvertWeaponRoll ? -0.08f : 0.08f;
                TargetWeaponAngle.y = -TargetWeaponAngle.z * rollMult;

                // Centraliza a coronha lateralmente no ombro durante a visada
                TargetWeaponPos = Vector3.Lerp(TargetWeaponPos, Vector3.zero, Time.deltaTime * 20f);
                return;
            }

            // 1. Hipfire: Divisão Proporcional de Input (25% câmera / 75% projeção da arma)
            float cameraRatio = Mathf.Clamp(CameraFollowRatio, 0.05f, 0.95f);
            Vector2 hipInput = deltaRotation * (1f - cameraRatio);
            deltaRotation *= cameraRatio;

            // 2. Mapeamento dos Eixos do Rig do Tarkov (Z=Yaw, X=Pitch, Y=Roll)
            float hipYaw = hipInput.x * (InvertWeaponYaw ? -1f : 1f);
            float hipPitch = hipInput.y * (InvertWeaponPitch ? 1f : -1f);

            if (SwapWeaponPitchYaw)
            {
                float temp = hipYaw;
                hipYaw = hipPitch;
                hipPitch = temp;
            }

            TargetWeaponAngle.z += hipYaw;
            TargetWeaponAngle.x -= hipPitch;

            // 3. Aplica os limites angulares da Deadzone
            TargetWeaponAngle.z = Mathf.Clamp(TargetWeaponAngle.z, -DeadzoneLimits.x, DeadzoneLimits.x);
            TargetWeaponAngle.x = Mathf.Clamp(TargetWeaponAngle.x, -DeadzoneLimits.y, DeadzoneLimits.y);

            // --- CINEMÁTICA CQB POINT-SHOOTING ---
            TargetWeaponPos.x = -TargetWeaponAngle.z * StockSlideMultiplier;
            TargetWeaponPos.z = -Mathf.Abs(TargetWeaponAngle.z) * ArmCompressionMultiplier;

            float hipRollMult = InvertWeaponRoll ? -CQBRollMultiplier : CQBRollMultiplier;
            TargetWeaponAngle.y = -TargetWeaponAngle.z * hipRollMult;
        }

        public static void UpdatePhysics(EFT.Player player, float dt)
        {
            if (dt <= 0.0001f) return;

            bool isAiming = player != null && player.HandsController != null && player.HandsController.IsAiming;
            float currentWeightTime = isAiming ? ADSWeightTime : WeaponWeightTime;

            // 1. Retorno Elástico Contínuo ao Centro
            float returnSpeed = isAiming ? SpringReturnSpeed * 1.5f : SpringReturnSpeed;
            TargetWeaponAngle = Vector3.Lerp(TargetWeaponAngle, Vector3.zero, dt * returnSpeed);
            TargetWeaponPos = Vector3.Lerp(TargetWeaponPos, Vector3.zero, dt * returnSpeed);

            // 2. Inércia Dinâmica pelos Passos WASD (apenas em Hipfire)
            Vector3 posTarget = TargetWeaponPos;
            if (!isAiming && player != null && player.MovementContext != null)
            {
                Vector2 inputDir = (Vector2)player.InputDirection;
                float walkSpeed = player.Speed; // 0.0 a 1.0
                posTarget.x -= inputDir.x * walkSpeed * StrafeWalkMultiplier;
            }

            // 3. Amortecimento Angular Unificado (SmoothDamp)
            CurrentWeaponAngle = Vector3.SmoothDamp(
                CurrentWeaponAngle,
                TargetWeaponAngle,
                ref _weaponAngleVelocity,
                Mathf.Max(currentWeightTime, 0.001f),
                Mathf.Infinity,
                dt
            );

            // 4. Amortecimento Posicional
            CurrentWeaponPos = Vector3.SmoothDamp(
                CurrentWeaponPos,
                posTarget,
                ref _weaponPosVelocity,
                Mathf.Max(currentWeightTime, 0.001f),
                Mathf.Infinity,
                dt
            );

            // 5. Inclinação Tática de Cabeça em ADS (Cheek Weld)
            float targetTilt = (isAiming && EnableADSTilt) ? ADSTiltHeadRoll : 0f;
            _currentHeadTiltADS = Mathf.SmoothDamp(_currentHeadTiltADS, targetTilt, ref _headTiltVelocity, 0.10f, Mathf.Infinity, dt);

            // 6. Dinâmica da Cabeça (Head Roll Dinâmico + Tilt do ADS)
            float degPerSecX = (LastFrameDelta.x / dt) * (InvertHeadRoll ? -1f : 1f);
            float degPerSecY = LastFrameDelta.y / dt;

            float dynamicRoll = Mathf.Clamp(-degPerSecX * HeadRollIntensity, -MaxHeadRoll, MaxHeadRoll);
            float totalTargetRoll = dynamicRoll + _currentHeadTiltADS;

            CurrentHeadRoll = Mathf.SmoothDamp(
                CurrentHeadRoll,
                totalTargetRoll,
                ref _headRollVelocity,
                Mathf.Max(HeadRecoveryTime, 0.001f),
                Mathf.Infinity,
                dt
            );

            // Head Pitch Lag (X)
            if (HeadPitchDelayIntensity > 0.0001f)
            {
                float targetPitch = Mathf.Clamp(degPerSecY * HeadPitchDelayIntensity, -5f, 5f);
                CurrentHeadPitch = Mathf.SmoothDamp(CurrentHeadPitch, targetPitch, ref _headPitchVelocity, 0.12f, Mathf.Infinity, dt);
            }
            else
            {
                CurrentHeadPitch = 0f;
            }

            // Head Yaw Lag (Y)
            if (HeadYawDelayIntensity > 0.0001f)
            {
                float targetYaw = Mathf.Clamp(-degPerSecX * HeadYawDelayIntensity, -5f, 5f);
                CurrentHeadYaw = Mathf.SmoothDamp(CurrentHeadYaw, targetYaw, ref _headYawVelocity, 0.12f, Mathf.Infinity, dt);
            }
            else
            {
                CurrentHeadYaw = 0f;
            }

            // Atenua o delta acumulado para o próximo frame
            LastFrameDelta = Vector2.Lerp(LastFrameDelta, Vector2.zero, dt * 10f);
        }

        // Aplica impacto físico e sacudida visual ao disparar a arma
        public static void ApplyShotPunch(bool isAiming)
        {
            if (ShotRecoilPunch <= 0.001f) return;

            float punchMultiplier = isAiming ? 0.7f : 1.0f;
            float punch = ShotRecoilPunch * punchMultiplier;

            // Solavanco aleatório de roll na cabeça / ombro
            CurrentHeadRoll += UnityEngine.Random.Range(-punch * 1.2f, punch * 1.2f);

            // Elevação instantânea no pitch da arma
            TargetWeaponAngle.x -= punch * 1.5f;

            // Leve coice no recuo da coronha
            TargetWeaponPos.z -= punch * 0.01f;
        }

        public static void CalculateArmOffsets(out Vector3 positionOffset, out Quaternion rotationOffset)
        {
            rotationOffset = Quaternion.Euler(CurrentWeaponAngle.x, CurrentWeaponAngle.y, CurrentWeaponAngle.z);
            positionOffset = CurrentWeaponPos;
        }

        public static void Reset()
        {
            TargetWeaponAngle = Vector3.zero;
            CurrentWeaponAngle = Vector3.zero;
            _weaponAngleVelocity = Vector3.zero;

            TargetWeaponPos = Vector3.zero;
            CurrentWeaponPos = Vector3.zero;
            _weaponPosVelocity = Vector3.zero;

            CurrentHeadRoll = 0f;
            CurrentHeadPitch = 0f;
            CurrentHeadYaw = 0f;
            _headRollVelocity = 0f;
            _headPitchVelocity = 0f;
            _headYawVelocity = 0f;
            _currentHeadTiltADS = 0f;
            _headTiltVelocity = 0f;
            LastFrameDelta = Vector2.zero;
        }
    }
}
