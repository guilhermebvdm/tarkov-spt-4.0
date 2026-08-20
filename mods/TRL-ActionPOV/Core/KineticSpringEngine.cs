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
        public static float WeaponWeightTime = 0.055f;          // Tempo de resposta da mola em Hipfire
        
        // --- CINEMÁTICA CQB (POINT-SHOOTING / WEAPON LEAD) ---
        public static float StockSlideHorizontalMax = 0.035f;   // Deslocamento lateral máximo da coronha no ombro em metros (ex: 0.035 = 3.5cm)
        public static float LeftStockSlideMultiplier = 0.35f;   // Multiplicador de percurso do sway ao virar para a ESQUERDA (0.35 = 35% do percurso)
        public static float StockSlideVerticalMax = 0.020f;     // Deslocamento vertical máximo da coronha no ombro em metros (ex: 0.020 = 2.0cm)
        public static float StockSmoothTimeHorizontal = 0.25f;  // Tempo de amortecimento lateral da coronha no Hipfire (segundos)
        public static float StockSmoothTimeVertical = 0.38f;    // Tempo de amortecimento vertical da coronha no Hipfire (segundos)
        public static float ArmCompressionMultiplier = 0.0018f; // Compressão de braço para trás ao angular (metros/grau)
        public static float CQBRollMultiplier = 0.22f;          // Torção da arma no eixo Y nas esquinas
        public static float StrafeWalkMultiplier = 0.015f;      // Inércia lateral adicional pelos passos WASD

        // --- REALINHAMENTO POR CAMINHADA FRONTAL (W) ---
        public static float WalkForwardDelaySeconds = 1.0f;     // Tempo contínuo andando para frente antes de decidir centralizar (segundos)
        public static float WalkForwardRealignSpeed = 6.0f;     // Velocidade de centralização suave após o delay
        private static float _forwardWalkTimer = 0f;

        // --- MODO ADS TÁTICO ESTILO BODYCAM (SIGHT ALIGNMENT & CATCH-UP) ---
        public static bool EnableADSTilt = true;                // Ativa a inclinação tática do pescoço ao colar na coronha (Cheek Weld)
        public static float ADSTiltHeadRoll = 2.5f;             // Ângulo de inclinação da cabeça ao colar na coronha (graus)
        public static Vector2 ADSDeadzoneLimits = new Vector2(2.5f, 1.5f); // Micro-deadzone para parallax dinâmico no retículo
        public static float EyeToSightDistance = 0.35f;         // Distância focal média do olho até a alça/massa em metros (35cm)
        public static float ADSSightAlignmentFactor = 0.85f;    // Fator de correção óptica: 0 = livre | 1.0 = geométrico estrito
        public static float StockSlideHorizontalADS = 0.010f;   // Deslocamento lateral da coronha no ADS (metros)
        public static float StockSlideVerticalADS = 0.006f;     // Deslocamento vertical da coronha no ADS (metros)
        public static float ADSFrontSightSmoothTime = 0.035f;   // Tempo de resposta ágil do cano/massa de mira no ADS (segundos)
        public static float StockSmoothTimeHorizontalADS = 0.12f; // Tempo de resposta lateral da coronha/alça no ADS (segundos)
        public static float StockSmoothTimeVerticalADS = 0.20f;   // Tempo de resposta vertical da coronha/alça no ADS (segundos)

        // --- COICE FÍSICO DO DISPARO (WEAPON KICKBACK & RECOIL PUNCH NO EIXO Z E PITCH) ---
        public static bool EnableRecoilKick = true;             // Ativa o coice mecânico de disparo
        public static float RecoilKickZ_Hipfire = 0.030f;       // Recuo para trás no eixo Z em Hipfire (metros, ex: 0.030 = 3.0cm)
        public static float RecoilKickZ_ADS = 0.012f;           // Recuo para trás no eixo Z em ADS (metros, ex: 0.012 = 1.2cm)
        public static float RecoilMuzzleRise_Hipfire = 1.8f;    // Empinada vertical angular do cano em Hipfire (graus)
        public static float RecoilMuzzleRise_ADS = 0.8f;        // Empinada vertical angular do cano em ADS (graus)
        public static float RecoilRecoveryTime = 0.08f;         // Tempo de recuperação da mola após o coice (segundos)
        public static float RecoilHeadPunchIntensity = 0.8f;    // Sacudida visual na cabeça a cada disparo
        private static int _lastShotFrame = -1;

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
        public static bool InvertStockHorizontal = false;
        public static bool InvertStockVertical = false;
        public static bool SwapWeaponPitchYaw = false;
        public static Vector2 DeadzoneLimits = new Vector2(14f, 8f); // Amplitude lateral ampla para CQB (graus)

        // Transição Orgânica Contínua para ADS (0.0f = Hipfire, 1.0f = ADS pleno)
        public static float ADSTransitionBlend = 0f;
        private static float _adsBlendVelocity = 0f;

        public static void ProcessMouseInput(ref Vector2 deltaRotation, bool isAiming, EFT.Player player)
        {
            LastFrameDelta = deltaRotation;
            
            // 1. LIMITES DE DEADZONE E SENSIBILIDADE INTERPOLADOS SUAVEMENTE
            Vector2 bounds = Vector2.Lerp(DeadzoneLimits, ADSDeadzoneLimits, ADSTransitionBlend);
            float sensitivity = Mathf.Lerp(1.0f, 0.75f, ADSTransitionBlend);

            // ABERTURA GRADUAL COM CURVA DE RESISTÊNCIA NÃO-LINEAR
            float progressY = Mathf.Abs(TargetWeaponAngle.y) / Mathf.Max(bounds.x, 0.001f);
            float progressX = Mathf.Abs(TargetWeaponAngle.x) / Mathf.Max(bounds.y, 0.001f);
            
            float resistanceY = Mathf.Clamp01(1.0f - (progressY * 0.35f));
            float resistanceX = Mathf.Clamp01(1.0f - (progressX * 0.35f));

            float inputYaw = deltaRotation.x * sensitivity * resistanceY * (InvertWeaponYaw ? -1f : 1f);
            float inputPitch = deltaRotation.y * sensitivity * resistanceX * (InvertWeaponPitch ? 1f : -1f);

            if (SwapWeaponPitchYaw)
            {
                float temp = inputYaw;
                inputYaw = inputPitch;
                inputPitch = temp;
            }

            // 2. Acumula a intenção do mouse na Deadzone da arma (X = Pitch, Y = Yaw)
            Vector2 candidate = new Vector2(
                TargetWeaponAngle.y + inputYaw,
                TargetWeaponAngle.x - inputPitch
            );

            // 3. Aplica o Clamp estrito da caixa de Deadzone dinâmica
            Vector2 clamped = new Vector2(
                Mathf.Clamp(candidate.x, -bounds.x, bounds.x),
                Mathf.Clamp(candidate.y, -bounds.y, bounds.y)
            );

            // 4. Absorção da Deadzone: O que couber na deadzone é consumido; o excesso gira a câmera
            Vector2 consumed = new Vector2(
                clamped.x - TargetWeaponAngle.y,
                -(clamped.y - TargetWeaponAngle.x)
            );

            TargetWeaponAngle.y = clamped.x; // Yaw (Horizontal)
            TargetWeaponAngle.x = clamped.y; // Pitch (Vertical)

            // Subtrai da rotação da câmera (se estiver na deadzone, a câmera fica imóvel)
            deltaRotation.x -= (consumed.x / sensitivity);
            deltaRotation.y -= (consumed.y / sensitivity);

            // 5. REALINHAMENTO CONDICIONAL APÓS 1 SEGUNDO DE CAMINHADA CONTÍNUA PARA FRENTE (W)
            if (player != null && player.MovementContext != null)
            {
                Vector2 inputDir = (Vector2)player.InputDirection;
                float walkSpeed = player.Speed;
                
                if (inputDir.y > 0.3f && walkSpeed > 0.1f)
                {
                    _forwardWalkTimer += Time.deltaTime;
                    if (_forwardWalkTimer >= WalkForwardDelaySeconds)
                    {
                        float forwardRealignFactor = Time.deltaTime * WalkForwardRealignSpeed;
                        
                        Vector2 pull = new Vector2(
                            TargetWeaponAngle.y * forwardRealignFactor,
                            TargetWeaponAngle.x * forwardRealignFactor
                        );

                        deltaRotation.x += pull.x;
                        deltaRotation.y -= pull.y;

                        TargetWeaponAngle.y -= pull.x;
                        TargetWeaponAngle.x -= pull.y;
                    }
                }
                else
                {
                    _forwardWalkTimer = 0f;
                }
            }

            // 6. POSIÇÃO DA CORONHA COM TRANSIÇÃO ORGÂNICA ENTRE HIPFIRE E ADS
            float curSlideMaxH = Mathf.Lerp(StockSlideHorizontalMax, StockSlideHorizontalADS, ADSTransitionBlend);
            float curSlideMaxV = Mathf.Lerp(StockSlideVerticalMax, StockSlideVerticalADS, ADSTransitionBlend);

            float normYaw = TargetWeaponAngle.y / Mathf.Max(bounds.x, 0.001f);
            float curveYaw = Mathf.Sign(normYaw) * Mathf.Pow(Mathf.Abs(normYaw), Mathf.Lerp(1.25f, 1.15f, ADSTransitionBlend));
            float multH = InvertStockHorizontal ? 1f : -1f;
            float maxSlideH = curSlideMaxH;

            // Atenuação de percurso para a ESQUERDA (TargetWeaponAngle.y < 0)
            if (TargetWeaponAngle.y < 0f)
            {
                maxSlideH *= Mathf.Clamp01(LeftStockSlideMultiplier);
            }

            TargetWeaponPos.x = multH * curveYaw * maxSlideH;

            // Vertical (Pitch): Ao mirar para CIMA (Pitch negativo), cano sobe e coronha desce
            float normPitch = TargetWeaponAngle.x / Mathf.Max(bounds.y, 0.001f);
            float curvePitch = Mathf.Sign(normPitch) * Mathf.Pow(Mathf.Abs(normPitch), Mathf.Lerp(1.25f, 1.15f, ADSTransitionBlend));
            float multV = InvertStockVertical ? -1f : 1f;
            TargetWeaponPos.y = multV * curvePitch * curSlideMaxV;

            // Profundidade (Z) e Torção (Roll) suaves
            TargetWeaponPos.z = Mathf.Lerp(-Mathf.Abs(curveYaw) * ArmCompressionMultiplier, 0f, ADSTransitionBlend);
            float rollMult = InvertWeaponRoll ? -CQBRollMultiplier : CQBRollMultiplier;
            TargetWeaponAngle.z = Mathf.Lerp(-TargetWeaponAngle.y * rollMult, 0f, ADSTransitionBlend);
        }

        public static void UpdatePhysics(EFT.Player player, float dt)
        {
            if (dt <= 0.0001f) return;

            bool isAiming = player != null && player.HandsController != null && player.HandsController.IsAiming;

            // Mede a distância/deslocamento atual da arma em relação ao centro focal
            float angleDist = CurrentWeaponAngle.magnitude; // magnitude angular em graus
            float posDist = CurrentWeaponPos.magnitude;     // magnitude de deslocamento em metros
            
            // Fator de deslocamento: quanto mais longe a arma estava, mais peso e inércia para puxar para o olho
            float displacementFactor = (angleDist / 8f) + (posDist / 0.030f);
            float dynamicADSSmoothTime = Mathf.Lerp(0.18f, 0.45f, Mathf.Clamp01(displacementFactor * 0.5f));

            float targetBlend = isAiming ? 1.0f : 0.0f;
            ADSTransitionBlend = Mathf.SmoothDamp(
                ADSTransitionBlend,
                targetBlend,
                ref _adsBlendVelocity,
                Mathf.Max(dynamicADSSmoothTime, 0.04f),
                Mathf.Infinity,
                dt
            );

            // Tempos de amortecimento adaptativos
            float baseAngularTime = Mathf.Lerp(WeaponWeightTime, ADSFrontSightSmoothTime, ADSTransitionBlend);
            float currentAngularSmoothTime = baseAngularTime;
            if (isAiming && ADSTransitionBlend < 0.95f)
            {
                currentAngularSmoothTime *= Mathf.Lerp(1.0f, 1.4f, Mathf.Clamp01(displacementFactor * 0.5f) * (1f - ADSTransitionBlend));
            }

            float smoothH = Mathf.Lerp(StockSmoothTimeHorizontal, StockSmoothTimeHorizontalADS, ADSTransitionBlend);
            float smoothV = Mathf.Lerp(StockSmoothTimeVertical, StockSmoothTimeVerticalADS, ADSTransitionBlend);

            // Inércia Dinâmica pelos Passos WASD (apenas em Hipfire)
            Vector3 posTarget = TargetWeaponPos;
            if (!isAiming && player != null && player.MovementContext != null)
            {
                Vector2 inputDir = (Vector2)player.InputDirection;
                float walkSpeed = player.Speed; // 0.0 a 1.0
                posTarget.x -= inputDir.x * walkSpeed * StrafeWalkMultiplier;
            }

            // 1. Amortecimento Angular da Arma
            CurrentWeaponAngle = Vector3.SmoothDamp(
                CurrentWeaponAngle,
                TargetWeaponAngle,
                ref _weaponAngleVelocity,
                Mathf.Max(currentAngularSmoothTime, 0.001f),
                Mathf.Infinity,
                dt
            );

            // 2. Amortecimento Dedicado da Coronha (Separado por Eixo Horizontal e Vertical)
            CurrentWeaponPos.x = Mathf.SmoothDamp(
                CurrentWeaponPos.x,
                posTarget.x,
                ref _weaponPosVelocity.x,
                Mathf.Max(smoothH, 0.001f),
                Mathf.Infinity,
                dt
            );

            CurrentWeaponPos.y = Mathf.SmoothDamp(
                CurrentWeaponPos.y,
                posTarget.y,
                ref _weaponPosVelocity.y,
                Mathf.Max(smoothV, 0.001f),
                Mathf.Infinity,
                dt
            );

            // Restrição Estrita de Quadrante Vertical (A coronha não passa do centro até o cano cruzar o centro)
            // - Cano para CIMA (Pitch negativo, TargetWeaponAngle.x < 0): Coronha deve ficar em baixo (<= 0f)
            // - Cano para BAIXO (Pitch positivo, TargetWeaponAngle.x > 0): Coronha deve ficar em cima (>= 0f)
            float multVCur = InvertStockVertical ? -1f : 1f;
            if (TargetWeaponAngle.x < -0.01f) // Cano apontado para CIMA
            {
                if (multVCur > 0)
                    CurrentWeaponPos.y = Mathf.Min(0f, CurrentWeaponPos.y);
                else
                    CurrentWeaponPos.y = Mathf.Max(0f, CurrentWeaponPos.y);
            }
            else if (TargetWeaponAngle.x > 0.01f) // Cano apontado para BAIXO
            {
                if (multVCur > 0)
                    CurrentWeaponPos.y = Mathf.Max(0f, CurrentWeaponPos.y);
                else
                    CurrentWeaponPos.y = Mathf.Min(0f, CurrentWeaponPos.y);
            }

            float zRecoveryTime = isAiming ? (RecoilRecoveryTime * 0.75f) : RecoilRecoveryTime;
            CurrentWeaponPos.z = Mathf.SmoothDamp(
                CurrentWeaponPos.z,
                posTarget.z,
                ref _weaponPosVelocity.z,
                Mathf.Max(zRecoveryTime, 0.001f),
                Mathf.Infinity,
                dt
            );

            // 3. Inclinação Tática de Cabeça em ADS (Cheek Weld com sinal corrigido)
            float targetTilt = (isAiming && EnableADSTilt) ? ADSTiltHeadRoll : 0f;
            _currentHeadTiltADS = Mathf.SmoothDamp(_currentHeadTiltADS, targetTilt, ref _headTiltVelocity, 0.10f, Mathf.Infinity, dt);

            // 4. Dinâmica da Cabeça (Head Roll Dinâmico + Tilt do ADS)
            float degPerSecX = (LastFrameDelta.x / dt) * (InvertHeadRoll ? -1f : 1f);
            float degPerSecY = LastFrameDelta.y / dt;

            // Sentido natural de roll ao virar
            float dynamicRoll = Mathf.Clamp(degPerSecX * HeadRollIntensity, -MaxHeadRoll, MaxHeadRoll);
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

        // Aplica coice físico e sacudida mecânica ao disparar a arma (Weapon Kickback & Recoil Punch)
        public static void ApplyRecoilKick(bool isAiming)
        {
            if (Time.frameCount == _lastShotFrame) return;
            _lastShotFrame = Time.frameCount;

            if (!EnableRecoilKick) return;

            float kickDepth = isAiming ? RecoilKickZ_ADS : RecoilKickZ_Hipfire;
            float muzzleRise = isAiming ? RecoilMuzzleRise_ADS : RecoilMuzzleRise_Hipfire;
            float headPunch = isAiming ? (RecoilHeadPunchIntensity * 0.6f) : RecoilHeadPunchIntensity;

            // 1. Recuo imediato de profundidade no eixo Z (para trás contra o ombro/peito)
            CurrentWeaponPos.z -= kickDepth;

            // 2. Empinada angular instantânea do cano no Pitch (X)
            CurrentWeaponAngle.x -= muzzleRise;

            // 3. Solavanco de Head Roll aleatório na cabeça/visão
            if (headPunch > 0.001f)
            {
                CurrentHeadRoll += UnityEngine.Random.Range(-headPunch, headPunch);
            }
        }

        // Compatibilidade com chamadas legadas
        public static void ApplyShotPunch(bool isAiming)
        {
            ApplyRecoilKick(isAiming);
        }

        public static void CalculateArmOffsets(EFT.Animations.ProceduralWeaponAnimation pwa, out Vector3 positionOffset, out Quaternion rotationOffset)
        {
            bool isAiming = pwa != null && pwa.IsAiming;
            rotationOffset = Quaternion.Euler(CurrentWeaponAngle.x, CurrentWeaponAngle.y, CurrentWeaponAngle.z);

            if (!isAiming)
            {
                // NO HIPFIRE: Translação lateral livre de CQB e deslizamento da coronha
                positionOffset = CurrentWeaponPos;
            }
            else
            {
                // NO ADS: PIVÔ FOCAL NO OSSO DA MIRA (mod_aim_camera)
                // A rotação ocorre ao redor do centro óptico da mira, mantendo alça e massa 100% concêntricas
                if (pwa != null && pwa.CurrentScope != null && pwa.CurrentScope.Bone != null && pwa.HandsContainer != null && pwa.HandsContainer.WeaponRoot != null)
                {
                    // Posição local do osso da mira em relação ao WeaponRoot
                    Vector3 scopeLocalPos = pwa.HandsContainer.WeaponRoot.InverseTransformPoint(pwa.CurrentScope.Bone.position);

                    // Translação compensatória: ΔP = P_scope - (ΔR * P_scope)
                    positionOffset = scopeLocalPos - (rotationOffset * scopeLocalPos);

                    // Adiciona a inércia amortecida da coronha
                    positionOffset += CurrentWeaponPos;
                }
                else
                {
                    positionOffset = CurrentWeaponPos;
                }
            }
        }

        public static void CalculateArmOffsets(out Vector3 positionOffset, out Quaternion rotationOffset)
        {
            CalculateArmOffsets(null, out positionOffset, out rotationOffset);
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
