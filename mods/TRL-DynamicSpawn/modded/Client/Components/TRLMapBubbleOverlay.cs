using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using TRLDynamicSpawn.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace TRLDynamicSpawn.Components
{
    public class TRLMapBubbleOverlay : MonoBehaviour
    {
        private static TRLMapBubbleOverlay _instance;

        private GameObject _overlayContainer;
        private Image _safeZoneImage;
        private Image _spawnBubbleImage;
        private Image _losConeImage;

        private GameObject _cachedMapViewGO;
        private Component _activeMapView;
        private Sprite _safeZoneSprite;
        private Sprite _spawnBubbleSprite;
        private Sprite _losConeSprite;

        private float _lastMapCheckTime = 0f;

        public static void Enable()
        {
            if (_instance != null) return;
            var go = new GameObject("TRL_MapBubbleOverlay");
            _instance = go.AddComponent<TRLMapBubbleOverlay>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            _instance = this;
            GenerateProceduralSprites();
        }

        private void GenerateProceduralSprites()
        {
            try
            {
                _safeZoneSprite = CreateRingSprite(256, new Color(1f, 0.25f, 0.25f, 0.8f), 4f);
                _spawnBubbleSprite = CreateRingSprite(256, new Color(0f, 0.85f, 1f, 0.8f), 3f);
                _losConeSprite = CreateConeSprite(256, 70f, new Color(1f, 0.92f, 0.2f, 0.35f));
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL] Error generating procedural overlay sprites: {ex.Message}");
            }
        }

        private void LateUpdate()
        {
            // 1. Se desativado no F12, esconde e sai
            if (Settings.enableMapOverlay == null || !Settings.enableMapOverlay.Value)
            {
                if (_overlayContainer != null && _overlayContainer.activeSelf)
                {
                    _overlayContainer.SetActive(false);
                }
                return;
            }

            // 2. Se já temos o GameObject do MapView em cache e o mapa está FECHADO, sai instantaneamente (0 alocações GC)
            if (_cachedMapViewGO != null && !_cachedMapViewGO.activeInHierarchy)
            {
                if (_overlayContainer != null && _overlayContainer.activeSelf)
                {
                    _overlayContainer.SetActive(false);
                }
                return;
            }

            // 3. Tentar localizar a MapView sem alocar arrays pesados no GC se ainda não tiver em cache (a cada 2.0s)
            if (_cachedMapViewGO == null && Time.time - _lastMapCheckTime > 2.0f)
            {
                _lastMapCheckTime = Time.time;
                FindActiveMapView();
            }

            // 4. Se a MapView não existe ou está inativa, sai
            if (_cachedMapViewGO == null || !_cachedMapViewGO.activeInHierarchy || _activeMapView == null)
            {
                if (_overlayContainer != null && _overlayContainer.activeSelf)
                {
                    _overlayContainer.SetActive(false);
                }
                return;
            }

            UpdateOverlay();
        }

        private void FindActiveMapView()
        {
            try
            {
                // Busca por nome de GameObject nativo primeiro (0 alocações GC se inativo)
                var go = GameObject.Find("MapView");
                if (go != null && go.activeInHierarchy)
                {
                    var comp = go.GetComponent("MapView");
                    if (comp != null)
                    {
                        _cachedMapViewGO = go;
                        if (_activeMapView != comp)
                        {
                            _activeMapView = comp;
                            if (_overlayContainer != null)
                            {
                                Destroy(_overlayContainer);
                                _overlayContainer = null;
                            }
                        }
                        return;
                    }
                }
            }
            catch
            {
                _cachedMapViewGO = null;
                _activeMapView = null;
            }
        }

        private void EnsureOverlayStructure(Transform parentTransform)
        {
            if (_overlayContainer != null) return;

            try
            {
                _overlayContainer = new GameObject("TRL_OverlayContainer", typeof(RectTransform));
                _overlayContainer.transform.SetParent(parentTransform, false);
                _overlayContainer.transform.SetAsFirstSibling();

                var containerRect = _overlayContainer.GetComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.pivot = new Vector2(0.5f, 0.5f);

                // 1. Cone de Visão (LoS / FOV)
                var coneGO = new GameObject("TRL_LoSCone", typeof(RectTransform), typeof(Image));
                coneGO.transform.SetParent(_overlayContainer.transform, false);
                _losConeImage = coneGO.GetComponent<Image>();
                _losConeImage.sprite = _losConeSprite;
                _losConeImage.raycastTarget = false;

                // 2. Círculo Externo (Bolha de Spawn)
                var bubbleGO = new GameObject("TRL_SpawnBubble", typeof(RectTransform), typeof(Image));
                bubbleGO.transform.SetParent(_overlayContainer.transform, false);
                _spawnBubbleImage = bubbleGO.GetComponent<Image>();
                _spawnBubbleImage.sprite = _spawnBubbleSprite;
                _spawnBubbleImage.raycastTarget = false;

                // 3. Círculo Interno (Zona Segura)
                var safeGO = new GameObject("TRL_SafeZone", typeof(RectTransform), typeof(Image));
                safeGO.transform.SetParent(_overlayContainer.transform, false);
                _safeZoneImage = safeGO.GetComponent<Image>();
                _safeZoneImage.sprite = _safeZoneSprite;
                _safeZoneImage.raycastTarget = false;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL] Error creating overlay UI structure: {ex.Message}");
            }
        }

        private void UpdateOverlay()
        {
            if (_activeMapView == null) return;

            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null || gameWorld.MainPlayer == null) return;

                var mainPlayer = gameWorld.MainPlayer;

                // Tentar obter o MapMarkerContainer ou transform do MapView
                Transform parentTransform = _activeMapView.transform;
                var markerContainerProp = _activeMapView.GetType().GetProperty("MapMarkerContainer");
                if (markerContainerProp != null)
                {
                    var containerObj = markerContainerProp.GetValue(_activeMapView) as GameObject;
                    if (containerObj != null) parentTransform = containerObj.transform;
                }

                EnsureOverlayStructure(parentTransform);
                if (_overlayContainer == null) return;

                _overlayContainer.SetActive(true);

                // Converter a posição do jogador para as coordenadas 2D do mapa
                Vector3 playerPos = mainPlayer.Position;
                Vector2 mapPos = new Vector2(playerPos.x, playerPos.z); // X world -> X UI, Z world -> Y UI

                var containerRect = _overlayContainer.GetComponent<RectTransform>();
                containerRect.anchoredPosition = mapPos;

                // Obter distâncias configuradas para o mapa atual
                string mapName = mainPlayer.Location?.ToLower() ?? "";
                double minSafeDist = 100.0;
                double maxBubbleDist = 300.0;

                var mapSettings = MapNameHelper.GetMapSettings(ServerConfigProvider.Config, mapName);
                if (mapSettings != null)
                {
                    minSafeDist = mapSettings.SafeZoneDistance;
                    maxBubbleDist = mapSettings.SpawnBubbleDistance;
                }

                float losDist = TRLDynamicSpawn.Helpers.Settings.losCullingDistance.Value;

                // Ajustar tamanhos (diâmetro = raio * 2)
                float safeDiam = (float)minSafeDist * 2f;
                float bubbleDiam = (float)maxBubbleDist * 2f;
                float losDiam = losDist * 2f;

                if (_safeZoneImage != null)
                {
                    bool showSafe = Settings.showSafeZoneCircle != null && Settings.showSafeZoneCircle.Value;
                    _safeZoneImage.gameObject.SetActive(showSafe);
                    _safeZoneImage.rectTransform.sizeDelta = new Vector2(safeDiam, safeDiam);
                }

                if (_spawnBubbleImage != null)
                {
                    bool showBubble = Settings.showSpawnBubbleCircle != null && Settings.showSpawnBubbleCircle.Value;
                    _spawnBubbleImage.gameObject.SetActive(showBubble);
                    _spawnBubbleImage.rectTransform.sizeDelta = new Vector2(bubbleDiam, bubbleDiam);
                }

                if (_losConeImage != null)
                {
                    bool showCone = Settings.showLoSCone != null && Settings.showLoSCone.Value;
                    _losConeImage.gameObject.SetActive(showCone);
                    _losConeImage.rectTransform.sizeDelta = new Vector2(losDiam, losDiam);

                    // Orientar o cone de visão com o olhar do jogador no mapa 2D
                    float lookAngle = -mainPlayer.Rotation.x;
                    _losConeImage.rectTransform.localRotation = Quaternion.Euler(0, 0, lookAngle);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[TRL] Exception during map overlay update: {ex.Message}");
            }
        }

        private static Sprite CreateRingSprite(int size, Color color, float thickness)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float center = size / 2f;
            float radius = center - 4f;

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    float delta = Mathf.Abs(dist - radius);
                    if (delta <= thickness)
                    {
                        alpha = 1f - (delta / thickness);
                    }

                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateConeSprite(int size, float fovDegrees, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            float center = size / 2f;
            float radius = center - 4f;
            float halfFov = fovDegrees / 2f;

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    if (dist <= radius && dist > 2f)
                    {
                        // Ângulo relativo ao vetor superior (Up/Forward)
                        float angle = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
                        float absAngle = Mathf.Abs(angle);
                        if (absAngle <= halfFov)
                        {
                            float edgeFactor = 1f - Mathf.Pow(absAngle / halfFov, 4f);
                            float distFactor = 1f - (dist / radius) * 0.4f;
                            alpha = Mathf.Clamp01(edgeFactor * distFactor);
                        }
                    }

                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
    }
}
