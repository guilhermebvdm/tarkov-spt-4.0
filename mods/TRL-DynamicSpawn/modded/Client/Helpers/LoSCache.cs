using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace TRLDynamicSpawn.Helpers
{
    public static class LoSCache
    {
        private struct CacheEntry
        {
            public float Timestamp;
            public bool HasLoS;
        }

        private static readonly Dictionary<long, CacheEntry> _cache = new Dictionary<long, CacheEntry>();
        private const float CACHE_EXPIRATION_SECONDS = 1.5f;
        public const float MAX_LOS_DISTANCE = 150f;
        private const float MAX_LOS_DISTANCE_SQ = MAX_LOS_DISTANCE * MAX_LOS_DISTANCE;
        private const int MAX_CACHE_ENTRIES = 1000;
        private static float _lastPurgeTime = 0f;

        public static void Clear()
        {
            _cache.Clear();
            _lastPurgeTime = 0f;
        }

        private static void TryPurgeStaleEntries(float currentTime)
        {
            if (_cache.Count < MAX_CACHE_ENTRIES || currentTime - _lastPurgeTime < 5.0f)
                return;

            _lastPurgeTime = currentTime;
            var staleKeys = new List<long>();
            foreach (var kvp in _cache)
            {
                if (currentTime - kvp.Value.Timestamp >= CACHE_EXPIRATION_SECONDS)
                {
                    staleKeys.Add(kvp.Key);
                }
            }

            for (int i = 0; i < staleKeys.Count; i++)
            {
                _cache.Remove(staleKeys[i]);
            }

            // Se mesmo após a purga continuar acima do teto de segurança, reseta completamente
            if (_cache.Count > MAX_CACHE_ENTRIES)
            {
                _cache.Clear();
            }
        }

        /// <summary>
        /// Verifica se algum dos jogadores da lista possui linha de visão (LoS) desobstruída para a posição-alvo.
        /// Aplica corte estrito de 150m, teste de cone de visão e cache com expiração de 1.5s para erradicar stutterings de raycast.
        /// </summary>
        public static bool CheckLoSToPlayers(Vector3 targetPos, List<Player> players, float configLosDist)
        {
            if (players == null || players.Count == 0) return false;

            float currentTime = Time.time;
            TryPurgeStaleEntries(currentTime);

            float effectiveMaxDist = Mathf.Min(configLosDist, MAX_LOS_DISTANCE);
            float effectiveMaxDistSq = effectiveMaxDist * effectiveMaxDist;

            // Quantização de posição em grade de 1.5m para maximizar reuso de cache em pontos próximos
            int gridX = Mathf.RoundToInt(targetPos.x / 1.5f);
            int gridY = Mathf.RoundToInt(targetPos.y / 1.5f);
            int gridZ = Mathf.RoundToInt(targetPos.z / 1.5f);

            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || player.HealthController == null || !player.HealthController.IsAlive) continue;

                Vector3 playerPos = player.Position;
                Vector3 diff = targetPos - playerPos;
                float sqrDist = diff.sqrMagnitude;

                // 1. Filtro estrito de corte de raio de 150m: além disso, é impossível ter LoS relevante
                if (sqrDist > effectiveMaxDistSq) continue;

                // Chave composta única por jogador e coordenada quantizada
                int playerKey = player.Id;
                long cacheKey = ((long)playerKey << 32) ^ ((long)gridX * 73856093L ^ (long)gridY * 19349663L ^ (long)gridZ * 83492791L);

                if (_cache.TryGetValue(cacheKey, out var entry))
                {
                    if (currentTime - entry.Timestamp < CACHE_EXPIRATION_SECONDS)
                    {
                        if (entry.HasLoS) return true;
                        continue;
                    }
                }

                // 2. Teste rápido de cone de visão do jogador antes de chamar física
                bool inView = false;
                if (player.IsYourPlayer && Camera.main != null)
                {
                    Vector3 screenPoint = Camera.main.WorldToViewportPoint(targetPos + Vector3.up * 1f);
                    if (screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1)
                    {
                        inView = true;
                    }
                }
                else
                {
                    Vector3 lookDir = player.LookDirection;
                    Vector3 dirToTarget = diff.normalized;
                    if (Vector3.Dot(lookDir, dirToTarget) > 0.35f)
                    {
                        inView = true;
                    }
                }

                bool hasLoS = false;
                if (inView)
                {
                    Vector3 headPos = (player.MainParts != null && player.MainParts.ContainsKey(BodyPartType.head)) 
                        ? player.MainParts[BodyPartType.head].Position 
                        : playerPos + Vector3.up * 1.5f;

                    if (!Physics.Linecast(headPos, targetPos + Vector3.up * 1f, LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.PlayerStaticCollisionsMask))
                    {
                        hasLoS = true;
                    }
                }

                _cache[cacheKey] = new CacheEntry { Timestamp = currentTime, HasLoS = hasLoS };
                if (hasLoS) return true;
            }

            return false;
        }
    }
}
