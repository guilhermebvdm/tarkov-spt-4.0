using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    public class LootCullingManager : MonoBehaviour
    {
        public static LootCullingManager Instance { get; private set; }

        public int CulledLootCount { get; private set; }

        private readonly List<TrackedLoot> _trackedItems = new List<TrackedLoot>(2048);
        private readonly HashSet<LootItem> _registeredSet = new HashSet<LootItem>();
        private readonly Plane[] _cameraPlanes = new Plane[6];
        private Player _mainPlayer;
        private Camera _mainCamera;
        private int _currentIndex;
        private float _lastSyncTime;
        private const int BATCH_SIZE = 64;
        private const float SYNC_INTERVAL = 5.0f;

        private struct TrackedLoot
        {
            public LootItem Loot;
            public Transform Transform;
            public float MaxDistanceSqr;
            public bool IsCulled;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(Player mainPlayer)
        {
            _mainPlayer = mainPlayer;
            _mainCamera = GetActiveCamera();
            ScanExistingLoot();
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

        public void ScanExistingLoot()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.LootList == null)
            {
                return;
            }

            var list = gameWorld.LootList;
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                if (list[i] is LootItem lootItem)
                {
                    RegisterLootItem(lootItem);
                }
            }
        }

        public void RegisterLootItem(LootItem lootItem)
        {
            if (lootItem == null || lootItem.Item == null || _registeredSet.Contains(lootItem))
            {
                return;
            }

            XYCellSizeStruct size = lootItem.Item.CalculateCellSize();
            int slots = size.X * size.Y;

            // Armas longas, coletes e mochilas volumosas (> 6 slots) permanecem sempre visíveis
            if (slots > 6)
            {
                return;
            }

            float maxDist = (slots <= 2)
                ? ModConfig.SmallLootDistance.Value
                : ModConfig.MediumLootDistance.Value;

            _registeredSet.Add(lootItem);
            _trackedItems.Add(new TrackedLoot
            {
                Loot = lootItem,
                Transform = lootItem.transform,
                MaxDistanceSqr = maxDist * maxDist,
                IsCulled = false
            });
        }

        public void OnUpdate()
        {
            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableLootCulling.Value)
            {
                if (CulledLootCount > 0)
                {
                    RestoreVisibilityOnly();
                }
                return;
            }

            // Se o jogador estiver morto ou nulo, restaura visibilidade para espectadores/FIKA
            if (_mainPlayer == null || !_mainPlayer.HealthController.IsAlive)
            {
                if (CulledLootCount > 0)
                {
                    RestoreVisibilityOnly();
                }
                return;
            }

            Camera activeCam = GetActiveCamera();
            if (activeCam == null)
            {
                return;
            }

            // Sincronização periódica para capturar itens dropados recentemente
            if (Time.time - _lastSyncTime > SYNC_INTERVAL)
            {
                _lastSyncTime = Time.time;
                ScanExistingLoot();
            }

            int total = _trackedItems.Count;
            if (total == 0)
            {
                return;
            }

            bool isAiming = _mainPlayer.ProceduralWeaponAnimation != null && _mainPlayer.ProceduralWeaponAnimation.IsAiming;
            bool adsBypass = isAiming && ModConfig.EnableADSLootBypass.Value;

            if (adsBypass)
            {
                GeometryUtility.CalculateFrustumPlanes(activeCam, _cameraPlanes);
            }

            Vector3 camPos = activeCam.transform.position;
            int processed = 0;

            while (processed < BATCH_SIZE && _currentIndex < total)
            {
                TrackedLoot item = _trackedItems[_currentIndex];
                if (item.Loot == null || item.Transform == null)
                {
                    if (item.IsCulled)
                    {
                        CulledLootCount = Math.Max(0, CulledLootCount - 1);
                    }
                    _registeredSet.Remove(item.Loot);
                    _trackedItems.RemoveAt(_currentIndex);
                    total--;
                    continue;
                }

                float sqrDist = (item.Transform.position - camPos).sqrMagnitude;
                bool shouldCull = sqrDist > item.MaxDistanceSqr;

                // ADS Bypass com filtragem de Frustum
                if (adsBypass && shouldCull)
                {
                    // Testa se a posição do item está dentro do cone de visão da mira
                    if (GeometryUtility.TestPlanesAABB(_cameraPlanes, new Bounds(item.Transform.position, Vector3.one * 0.4f)))
                    {
                        shouldCull = false;
                    }
                }

                if (shouldCull != item.IsCulled)
                {
                    item.IsCulled = shouldCull;
                    item.Loot.method_10(!shouldCull);
                    _trackedItems[_currentIndex] = item;

                    if (shouldCull)
                    {
                        CulledLootCount++;
                    }
                    else
                    {
                        CulledLootCount = Math.Max(0, CulledLootCount - 1);
                    }
                }

                _currentIndex++;
                processed++;
            }

            if (_currentIndex >= total)
            {
                _currentIndex = 0;
            }
        }

        private void RestoreVisibilityOnly()
        {
            int count = _trackedItems.Count;
            for (int i = 0; i < count; i++)
            {
                TrackedLoot item = _trackedItems[i];
                if (item.Loot != null && item.IsCulled)
                {
                    item.IsCulled = false;
                    item.Loot.method_10(true);
                    _trackedItems[i] = item;
                }
            }
            CulledLootCount = 0;
        }

        public void RestoreAll()
        {
            RestoreVisibilityOnly();
            _trackedItems.Clear();
            _registeredSet.Clear();
            Instance = null;
        }

        private void OnDestroy()
        {
            RestoreAll();
        }
    }
}
