using System;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class InteriorOcclusionWatcher : MonoBehaviour
    {
        private Player _mainPlayer;
        private float _originalShadowDistance;
        private float _currentShadowDistance;
        private float _targetShadowDistance;
        private float _lastCheckTime;
        private bool _isInside;

        private const float CheckInterval = 0.5f;
        private const float TransitionSpeed = 80.0f; // metros por segundo de interpolação suave

        public bool IsInside => _isInside;

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            _originalShadowDistance = QualitySettings.shadowDistance;
            _currentShadowDistance = _originalShadowDistance;
            _targetShadowDistance = _originalShadowDistance;
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableInteriorShadowCulling.Value)
            {
                if (Math.Abs(QualitySettings.shadowDistance - _originalShadowDistance) > 0.1f)
                {
                    QualitySettings.shadowDistance = _originalShadowDistance;
                }
                return;
            }

            if (_mainPlayer == null || !_mainPlayer.HealthController.IsAlive)
            {
                if (Math.Abs(QualitySettings.shadowDistance - _originalShadowDistance) > 0.1f)
                {
                    Restore();
                }
                return;
            }

            // Checagem periódica de interior
            if (ModConfig.EnableInteriorShadowCulling.Value && Time.time - _lastCheckTime >= CheckInterval)
            {
                _lastCheckTime = Time.time;
                _isInside = CheckIfInside();
            }

            _targetShadowDistance = _isInside
                ? ModConfig.InteriorShadowDistance.Value
                : ModConfig.ExteriorShadowDistance.Value;

            // Interpolação suave para evitar pop-in brusco de sombras
            if (Math.Abs(_currentShadowDistance - _targetShadowDistance) > 0.05f)
            {
                _currentShadowDistance = Mathf.MoveTowards(_currentShadowDistance, _targetShadowDistance, TransitionSpeed * Time.deltaTime);
                QualitySettings.shadowDistance = _currentShadowDistance;
            }
        }

        private bool CheckIfInside()
        {
            Vector3 origin = _mainPlayer.Position + Vector3.up * 1.5f;

            // Raio vertical para cima para detectar teto / laje
            if (Physics.Raycast(origin, Vector3.up, out RaycastHit hitUp, 20f, LayerMaskClass.HighPolyWithTerrainMask))
            {
                // Verifica se há também paredes ao redor em pelo menos 2 direções cardeais
                int wallHits = 0;
                Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

                for (int i = 0; i < directions.Length; i++)
                {
                    if (Physics.Raycast(origin, directions[i], 15f, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        wallHits++;
                    }
                }

                return wallHits >= 2;
            }

            return false;
        }

        public void Restore()
        {
            QualitySettings.shadowDistance = _originalShadowDistance;
        }

        private void OnDestroy()
        {
            Restore();
        }
    }
}
