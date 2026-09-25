using System;
using System.Collections.Generic;
using EFT;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Core
{
    /// <summary>
    /// Componente de depuração visual do sistema de camuflagem zonal.
    /// Renderiza cápsulas translúcidas na cor vermelha (50% transparente) exatamente
    /// nas partes do corpo onde a camuflagem física contra IA estiver ativa no momento,
    /// exibindo o contorno de segurança de 30cm que envolve o soldado e a mochila.
    /// </summary>
    public class ConcealmentVisualizer : MonoBehaviour
    {
        private class BodyMeshGhost
        {
            public string Name;
            public GameObject GhostObject;
            public SkinnedMeshRenderer GhostRenderer;
            public int AssociatedZoneIndex; // 0=Head, 1=Chest, 2=Pelvis, 3=LeftLeg, 4=RightLeg
        }

        private readonly List<BodyMeshGhost> _meshGhosts = new List<BodyMeshGhost>();
        private Material _redTransparentMaterial;
        private NaturalConcealmentManager _manager;
        private Player _player;
        private GameObject _ghostRoot;
        private bool _isInitialized;

        public void Initialize(NaturalConcealmentManager manager, Player player)
        {
            _manager = manager;
            _player = player;
            if (player == null || player.PlayerBones == null)
            {
                return;
            }

            CreateSharedMaterial();
            SetupBodyMeshGhosts(player);
            _isInitialized = true;
        }

        private void CreateSharedMaterial()
        {
            if (_redTransparentMaterial != null) return;

            // Busca shader transparente compatível com a Unity do EFT
            Shader shader = Shader.Find("Transparent/Diffuse") ??
                            Shader.Find("Sprites/Default") ??
                            Shader.Find("Standard") ??
                            Shader.Find("Unlit/Color");

            _redTransparentMaterial = new Material(shader);

            // Cor vermelha pura com 50% de opacidade (Alpha = 0.5f)
            Color translucentRed = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            _redTransparentMaterial.color = translucentRed;

            if (_redTransparentMaterial.HasProperty("_Color"))
            {
                _redTransparentMaterial.SetColor("_Color", translucentRed);
            }

            // Modo transparente para shaders que usam blend mode
            if (_redTransparentMaterial.HasProperty("_Mode"))
            {
                _redTransparentMaterial.SetFloat("_Mode", 3); // 3 = Transparent
                _redTransparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _redTransparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _redTransparentMaterial.SetInt("_ZWrite", 0);
                _redTransparentMaterial.DisableKeyword("_ALPHATEST_ON");
                _redTransparentMaterial.EnableKeyword("_ALPHABLEND_ON");
                _redTransparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                _redTransparentMaterial.renderQueue = 3000;
            }
        }

        private void SetupBodyMeshGhosts(Player player)
        {
            CleanupMeshGhosts();

            if (player == null || player.gameObject == null) return;

            _ghostRoot = new GameObject("TRL_BodyMeshGhosts");
            _ghostRoot.transform.SetParent(transform, false);

            // Coleta os SkinnedMeshRenderers do soldado (corpo, cabeça, calça, botas)
            SkinnedMeshRenderer[] sourceRenderers = player.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (sourceRenderers == null) return;

            for (int i = 0; i < sourceRenderers.Length; i++)
            {
                SkinnedMeshRenderer source = sourceRenderers[i];
                if (source == null || source.sharedMesh == null) continue;

                string lowerName = source.name.ToLower();

                // Determina qual das 5 zonas este renderer representa no corpo
                int zoneIdx = 1; // Default = Peito/Tronco
                if (lowerName.Contains("head") || lowerName.Contains("face") || lowerName.Contains("cap") || lowerName.Contains("helmet") || lowerName.Contains("eyes") || lowerName.Contains("hair"))
                {
                    zoneIdx = 0; // Cabeça
                }
                else if (lowerName.Contains("pant") || lowerName.Contains("feet") || lowerName.Contains("boot") || lowerName.Contains("leg") || lowerName.Contains("lower"))
                {
                    zoneIdx = 3; // Pernas
                }
                else if (lowerName.Contains("pelvis") || lowerName.Contains("hip") || lowerName.Contains("belt"))
                {
                    zoneIdx = 2; // Pelve
                }

                GameObject ghostGo = new GameObject("Ghost_" + source.name);
                ghostGo.transform.SetParent(_ghostRoot.transform, false);

                SkinnedMeshRenderer ghost = ghostGo.AddComponent<SkinnedMeshRenderer>();
                ghost.sharedMesh = source.sharedMesh;
                ghost.bones = source.bones;
                ghost.rootBone = source.rootBone;

                Material[] mats = new Material[source.sharedMaterials.Length > 0 ? source.sharedMaterials.Length : 1];
                for (int m = 0; m < mats.Length; m++)
                {
                    mats[m] = _redTransparentMaterial;
                }
                ghost.sharedMaterials = mats;
                ghost.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ghost.receiveShadows = false;
                ghost.enabled = false;

                _meshGhosts.Add(new BodyMeshGhost
                {
                    Name = source.name,
                    GhostObject = ghostGo,
                    GhostRenderer = ghost,
                    AssociatedZoneIndex = zoneIdx
                });
            }

            Plugin.LogSource?.LogInfo($"[TRL-CoreSight] ConcealmentVisualizer: {_meshGhosts.Count} malhas corporais holográficas vinculadas ao esqueleto do soldado (primitivas removidas).");
        }

        private void LateUpdate()
        {
            if (!_isInitialized || _manager == null) return;

            bool isDebugEnabled = ModConfig.ShowConcealmentVisualizer != null && 
                                  ModConfig.ShowConcealmentVisualizer.Value && 
                                  ModConfig.ModEnabled.Value && 
                                  ModConfig.EnableNaturalConcealment.Value;

            // Atualização exclusiva das malhas corporais reais (Silhueta 3D do Soldado em Vermelho Translúcido)
            for (int j = 0; j < _meshGhosts.Count; j++)
            {
                BodyMeshGhost ghost = _meshGhosts[j];
                if (ghost.GhostRenderer == null) continue;

                bool isCovered = _manager.IsZoneCovered(ghost.AssociatedZoneIndex);
                if (ghost.AssociatedZoneIndex == 3 && _manager.IsZoneCovered(4))
                {
                    isCovered = true; // Se qualquer uma das pernas estiver coberta
                }

                ghost.GhostRenderer.enabled = isDebugEnabled && isCovered;
            }
        }

        public void UpdateZoneScales()
        {
            // Primitivas removidas — malhas usam a escala anatômica real do soldado
        }

        private void CleanupMeshGhosts()
        {
            if (_ghostRoot != null)
            {
                Destroy(_ghostRoot);
                _ghostRoot = null;
            }
            _meshGhosts.Clear();
        }

        private void CleanupVisuals()
        {
            CleanupMeshGhosts();
        }

        private void OnDestroy()
        {
            CleanupVisuals();
            if (_redTransparentMaterial != null)
            {
                Destroy(_redTransparentMaterial);
                _redTransparentMaterial = null;
            }
        }
    }
}
