using System.Collections.Generic;
using Comfort.Common;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class BotPerformanceLimiter : MonoBehaviour
    {
        private Player _mainPlayer;
        private Camera _mainCamera;
        private float _lastCheckTime;
        private readonly Plane[] _cameraPlanes = new Plane[6];

        public int OccludedBotsCount { get; private set; }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            _mainCamera = GetActiveCamera();
        }

        private Camera GetActiveCamera()
        {
            if (_mainCamera == null || !_mainCamera.isActiveAndEnabled)
            {
                _mainCamera = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                    ? CameraClass.Instance.Camera
                    : Camera.main;
            }
            return _mainCamera;
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableBotAnimationLOD.Value)
            {
                RestoreAll();
                return;
            }

            Camera activeCam = GetActiveCamera();
            if (_mainPlayer == null || !_mainPlayer.HealthController.IsAlive || activeCam == null)
            {
                return;
            }

            if (Time.time - _lastCheckTime < ModConfig.BotOcclusionCheckInterval.Value)
            {
                return;
            }

            _lastCheckTime = Time.time;
            ProcessBots();
        }

        private void ProcessBots()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.AllAlivePlayersList == null)
            {
                return;
            }

            Camera activeCam = GetActiveCamera();
            if (activeCam == null)
            {
                return;
            }

            GeometryUtility.CalculateFrustumPlanes(activeCam, _cameraPlanes);
            Vector3 playerPos = _mainPlayer.Position;
            int occludedCount = 0;

            var alivePlayers = gameWorld.AllAlivePlayersList;
            int count = alivePlayers.Count;

            for (int i = 0; i < count; i++)
            {
                Player bot = alivePlayers[i];
                if (bot == null || bot.IsYourPlayer || !bot.HealthController.IsAlive)
                {
                    continue;
                }

                if (bot.CharacterController == null)
                {
                    continue;
                }

                Animator animator = bot.PlayerBones?.PlayableAnimator?.outputAnimator ?? bot.GetComponent<Animator>();
                if (animator == null)
                {
                    continue;
                }

                float sqrDist = (bot.Position - playerPos).sqrMagnitude;
                bool inFrustum = GeometryUtility.TestPlanesAABB(_cameraPlanes, bot.CharacterController.bounds);

                // Se o bot estiver fora do cone de visão OU muito longe (>75m)
                if (!inFrustum || sqrDist > 5625f) // 75m ao quadrado = 5625
                {
                    // Reduz taxa de atualização do Animator para o esqueleto não consumir ciclos de transformação
                    if (animator.cullingMode != AnimatorCullingMode.CullUpdateTransforms)
                    {
                        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    }
                    occludedCount++;
                }
                else
                {
                    // Dentro do campo de visão próximo: restaura animação com fidelidade total
                    if (animator.cullingMode != AnimatorCullingMode.AlwaysAnimate)
                    {
                        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    }
                }
            }

            OccludedBotsCount = occludedCount;
        }

        public void RestoreAll()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.AllAlivePlayersList == null)
            {
                return;
            }

            var alivePlayers = gameWorld.AllAlivePlayersList;
            for (int i = 0; i < alivePlayers.Count; i++)
            {
                Player bot = alivePlayers[i];
                if (bot != null && !bot.IsYourPlayer)
                {
                    Animator animator = bot.PlayerBones?.PlayableAnimator?.outputAnimator ?? bot.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    }
                }
            }

            OccludedBotsCount = 0;
        }

        private void OnDestroy()
        {
            RestoreAll();
        }
    }
}
