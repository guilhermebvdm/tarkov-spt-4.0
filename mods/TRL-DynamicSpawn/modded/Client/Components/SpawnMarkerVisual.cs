using System;
using UnityEngine;
using TMPro;

namespace TRLDynamicSpawn.Components
{
    public enum SpawnReviewStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class SpawnMarkerVisual : MonoBehaviour
    {
        public string PointId { get; private set; }
        public string Source { get; private set; } // "Native", "MOAR_PMC", "MOAR_SCAV"
        public string ZoneName { get; private set; }
        public Vector3 CurrentPosition { get; private set; }
        public Vector3 OriginalPosition { get; private set; }
        public SpawnReviewStatus Status { get; private set; } = SpawnReviewStatus.Pending;
        public bool WasMoved => (CurrentPosition - OriginalPosition).sqrMagnitude > 0.001f;

        public bool IsHovered { get; private set; }
        public bool IsSelected { get; private set; }

        public bool IsGhost { get; private set; }
        public bool IsCriteriaMet { get; private set; }
        public string ValidationMessage { get; private set; }

        private LineRenderer _poleLine;
        private GameObject _quadObj;
        private MeshRenderer _quadRenderer;
        private GameObject _sphereObj;
        private MeshRenderer _sphereRenderer;
        private GameObject _labelObj;
        private TextMeshPro _labelTmp;
        private CapsuleCollider _selectionCollider;

        // Materiais compartilhados para economizar memória e draw calls
        private static Material _matRed;        // Nativo Pendente
        private static Material _matMagenta;    // MOAR Pendente
        private static Material _matDarkBlue;   // Player Spawn (Azul Escuro para alta distincao macro)
        private static Material _matGreen;      // Aprovado
        private static Material _matBlack;      // Reprovado
        private static Material _matYellow;     // Destaque (Hover/Selected)
        private static Material _matGhostGreen; // Ghost Válido (translúcido)
        private static Material _matGhostGray;  // Ghost Inválido (translúcido)
        private static bool _materialsInitialized = false;

        private static Shader GetReliableShader()
        {
            string[] candidateShaders = new[]
            {
                "Sprites/Default",
                "UI/Default",
                "Legacy Shaders/Particles/Alpha Blended Premultiply",
                "Hidden/Internal-Colored"
            };

            foreach (var name in candidateShaders)
            {
                var s = Shader.Find(name);
                if (s != null) return s;
            }

            try
            {
                var temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var meshRenderer = temp.GetComponent<MeshRenderer>();
                var fallbackShader = meshRenderer != null && meshRenderer.sharedMaterial != null ? meshRenderer.sharedMaterial.shader : null;
                UnityEngine.Object.Destroy(temp);
                if (fallbackShader != null) return fallbackShader;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [GetReliableShader] Primitive Quad fallback exception: {ex}");
            }

            return Shader.Find("Sprites/Default");
        }

        public static void InitMaterials()
        {
            if (_materialsInitialized) return;

            var shader = GetReliableShader();
            if (shader == null)
            {
                Plugin.LogSource?.LogError("[TRL-DynamicSpawn] [InitMaterials] GetReliableShader returned NULL shader!");
            }
            else
            {
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] [InitMaterials] Using shader '{shader.name}'");
            }

            _matRed = CreateZTestAlwaysMaterial(shader, new Color(1.0f, 0.15f, 0.15f, 0.95f));
            _matMagenta = CreateZTestAlwaysMaterial(shader, new Color(1.0f, 0.1f, 1.0f, 0.95f));
            _matDarkBlue = CreateZTestAlwaysMaterial(shader, new Color(0.04f, 0.20f, 0.85f, 0.98f));
            _matGreen = CreateZTestAlwaysMaterial(shader, new Color(0.15f, 1.0f, 0.2f, 0.95f));
            _matBlack = CreateZTestAlwaysMaterial(shader, new Color(0.12f, 0.12f, 0.12f, 0.95f));
            _matYellow = CreateZTestAlwaysMaterial(shader, new Color(1.0f, 0.95f, 0.1f, 1.0f));
            _matGhostGreen = CreateZTestAlwaysMaterial(shader, new Color(0.15f, 1.0f, 0.25f, 0.55f));
            _matGhostGray = CreateZTestAlwaysMaterial(shader, new Color(0.6f, 0.6f, 0.65f, 0.55f));

            _materialsInitialized = true;
        }

        private static Material CreateZTestAlwaysMaterial(Shader shader, Color color)
        {
            Shader chosenShader = shader ?? Shader.Find("Sprites/Default");
            if (chosenShader == null)
            {
                Plugin.LogSource?.LogError("[TRL-DynamicSpawn] [CreateZTestAlwaysMaterial] Critical: shader is NULL!");
            }

            var mat = new Material(chosenShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            // Gera uma textura 2x2 sólida com a cor para garantir visibilidade mesmo em shaders sem property _Color
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Color[] cols = new Color[4] { color, color, color, color };
            tex.SetPixels(cols);
            tex.Apply();

            if (mat.HasProperty("_MainTex")) mat.mainTexture = tex;
            if (mat.HasProperty("_Color")) mat.color = color;
            if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", color);

            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            mat.renderQueue = 4500;
            return mat;
        }

        public void Initialize(string id, string source, string zone, Vector3 pos, SpawnReviewStatus initialStatus)
        {
            try
            {
                InitMaterials();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [Initialize:{id}] Step 1 InitMaterials failed: {ex}");
                throw;
            }

            PointId = id;
            Source = source;
            ZoneName = zone;
            CurrentPosition = pos;
            OriginalPosition = pos;
            Status = initialStatus;

            transform.position = pos;

            try
            {
                // 1. Poste vertical de 24m com LineRenderer (de -12m a +12m)
                _poleLine = gameObject.AddComponent<LineRenderer>();
                _poleLine.useWorldSpace = true;
                _poleLine.positionCount = 2;
                _poleLine.startWidth = 0.20f;
                _poleLine.endWidth = 0.20f;
                _poleLine.alignment = LineAlignment.View;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [Initialize:{id}] Step 2 LineRenderer failed: {ex}");
                throw;
            }

            try
            {
                // 2. Colisor vertical trigger para seleção via raycast
                _selectionCollider = gameObject.AddComponent<CapsuleCollider>();
                _selectionCollider.radius = 0.6f;
                _selectionCollider.height = 24f;
                _selectionCollider.center = Vector3.zero;
                _selectionCollider.isTrigger = true;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [Initialize:{id}] Step 3 CapsuleCollider failed: {ex}");
                throw;
            }

            try
            {
                // 3. Quad horizontal 1.5x1.5m no plano do piso
                _quadObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                _quadObj.name = "LevelQuad";
                _quadObj.transform.SetParent(transform, false);
                _quadObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                _quadObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                _quadObj.transform.localPosition = new Vector3(0f, 0.05f, 0f);

                var oldQuadCol = _quadObj.GetComponent<Collider>();
                if (oldQuadCol != null) UnityEngine.Object.Destroy(oldQuadCol);
                _quadRenderer = _quadObj.GetComponent<MeshRenderer>();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [Initialize:{id}] Step 4 LevelQuad failed: {ex}");
                throw;
            }

            try
            {
                // 4. Marcador esférico central 3D (0.5m)
                _sphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _sphereObj.name = "CenterSphere";
                _sphereObj.transform.SetParent(transform, false);
                _sphereObj.transform.localPosition = Vector3.zero;
                _sphereObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                var oldSphereCol = _sphereObj.GetComponent<Collider>();
                if (oldSphereCol != null) UnityEngine.Object.Destroy(oldSphereCol);
                _sphereRenderer = _sphereObj.GetComponent<MeshRenderer>();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [Initialize:{id}] Step 5 CenterSphere failed: {ex}");
                throw;
            }

            try
            {
                // 5. Texto em TextMeshPro
                _labelObj = new GameObject("Label_TMP");
                _labelObj.transform.SetParent(transform, false);
                _labelObj.transform.localPosition = new Vector3(0f, 1.4f, 0f);

                _labelTmp = _labelObj.AddComponent<TextMeshPro>();
                try
                {
                    var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                    if (fonts != null && fonts.Length > 0)
                    {
                        _labelTmp.font = fonts[0];
                    }
                }
                catch (Exception fontEx)
                {
                    Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] [Initialize:{id}] Font search warning: {fontEx}");
                }

                string initialZone = !string.IsNullOrEmpty(ZoneName) ? $"\n<size=75%><color=#8be9fd>[{ZoneName}]</color></size>" : "";
                _labelTmp.text = $"{PointId}{initialZone}";
                _labelTmp.fontSize = 3.5f;
                _labelTmp.alignment = TextAlignmentOptions.Center;
                _labelTmp.color = Color.white;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [Initialize:{id}] Step 6 TextMeshPro failed: {ex}");
                throw;
            }

            try
            {
                UpdatePositions();
                UpdateVisuals();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] [Initialize:{id}] Step 7 UpdatePositions/Visuals failed: {ex}");
                throw;
            }
        }

        public void SetPosition(Vector3 newPos)
        {
            CurrentPosition = newPos;
            transform.position = newPos;
            UpdatePositions();
        }

        public void SetZoneName(string newZone)
        {
            ZoneName = newZone;
            UpdateVisuals();
        }

        public void SetSource(string newSource)
        {
            Source = newSource;
            UpdateVisuals();
        }

        public void SetGhostState(bool isGhost, bool criteriaMet, string validationMessage = null)
        {
            IsGhost = isGhost;
            IsCriteriaMet = criteriaMet;
            ValidationMessage = validationMessage;
            UpdateVisuals();
        }

        public void ConfirmGhost()
        {
            IsGhost = false;
            Status = SpawnReviewStatus.Approved;
            UpdateVisuals();
        }

        public void SetStatus(SpawnReviewStatus newStatus)
        {
            Status = newStatus;
            UpdateVisuals();
        }

        public void SetHighlight(bool isHovered, bool isSelected)
        {
            if (IsHovered == isHovered && IsSelected == isSelected) return;

            IsHovered = isHovered;
            IsSelected = isSelected;
            UpdateVisuals();
        }

        private void UpdatePositions()
        {
            if (_poleLine != null)
            {
                _poleLine.SetPosition(0, CurrentPosition - Vector3.up * 12f);
                _poleLine.SetPosition(1, CurrentPosition + Vector3.up * 12f);
            }
        }

        private void UpdateVisuals()
        {
            Material targetMat;

            // Se for Ghost (Holograma móvel)
            if (IsGhost)
            {
                targetMat = IsCriteriaMet ? _matGhostGreen : _matGhostGray;
            }
            // Se estiver Selecionado/Travado -> Amarelo brilhante
            else if (IsSelected)
            {
                targetMat = _matYellow;
            }
            // Se estiver Aprovado -> Verde
            else if (Status == SpawnReviewStatus.Approved)
            {
                targetMat = _matGreen;
            }
            // Se estiver Reprovado -> Preto
            else if (Status == SpawnReviewStatus.Rejected)
            {
                targetMat = _matBlack;
            }
            // Se for Player -> Azul Escuro (alta distinção contra verde aprovado em visão macro)
            else if (!string.IsNullOrEmpty(Source) && Source.IndexOf("PLAYER", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                targetMat = _matDarkBlue;
            }
            // Se for Nativo Pendente -> Vermelho
            else if (Source == "Native")
            {
                targetMat = _matRed;
            }
            // Se for MOAR Pendente -> Magenta
            else
            {
                targetMat = _matMagenta;
            }

            if (_poleLine != null)
            {
                _poleLine.material = targetMat;
                Color col = targetMat != null ? targetMat.color : Color.white;
                _poleLine.startColor = col;
                _poleLine.endColor = col;
            }
            if (_quadRenderer != null) _quadRenderer.material = targetMat;
            if (_sphereRenderer != null) _sphereRenderer.material = targetMat;

            if (_labelTmp != null)
            {
                string zoneLine = !string.IsNullOrEmpty(ZoneName) ? $"\n<size=75%><color=#8be9fd>[{ZoneName}]</color></size>" : "";

                if (IsGhost)
                {
                    if (IsCriteriaMet)
                    {
                        _labelTmp.text = $"{PointId} [NOVO VÁLIDO]{zoneLine}";
                        _labelTmp.color = Color.green;
                        _labelTmp.fontSize = 4.0f;
                    }
                    else
                    {
                        string reason = !string.IsNullOrEmpty(ValidationMessage) ? $" [INVÁLIDO: {ValidationMessage}]" : " [INVÁLIDO]";
                        _labelTmp.text = $"{PointId}{reason}{zoneLine}";
                        _labelTmp.color = new Color(0.85f, 0.85f, 0.85f, 0.95f);
                        _labelTmp.fontSize = 3.6f;
                    }
                }
                else
                {
                    string statusText = Status == SpawnReviewStatus.Approved ? " [OK]" : (Status == SpawnReviewStatus.Rejected ? " [X]" : "");
                    string movedText = WasMoved ? " *" : "";

                    string typeTag = "";
                    if (!string.IsNullOrEmpty(Source))
                    {
                        if (Source.IndexOf("SNIPER", StringComparison.OrdinalIgnoreCase) >= 0 && PointId.IndexOf("SNIPER", StringComparison.OrdinalIgnoreCase) < 0)
                            typeTag = " <color=#ff79c6>[SNIPER]</color>";
                        else if (Source.IndexOf("BOSS", StringComparison.OrdinalIgnoreCase) >= 0 && PointId.IndexOf("BOSS", StringComparison.OrdinalIgnoreCase) < 0)
                            typeTag = " <color=#ff5555>[BOSS]</color>";
                        else if (Source.IndexOf("ROGUE", StringComparison.OrdinalIgnoreCase) >= 0 && PointId.IndexOf("ROGUE", StringComparison.OrdinalIgnoreCase) < 0)
                            typeTag = " <color=#bd93f9>[ROGUE]</color>";
                        else if (Source.IndexOf("RAIDER", StringComparison.OrdinalIgnoreCase) >= 0 && PointId.IndexOf("RAIDER", StringComparison.OrdinalIgnoreCase) < 0)
                            typeTag = " <color=#ffb86c>[RAIDER]</color>";
                        else if (Source.IndexOf("SCAV", StringComparison.OrdinalIgnoreCase) >= 0 && PointId.IndexOf("SCAV", StringComparison.OrdinalIgnoreCase) < 0)
                            typeTag = " <color=#f1fa8c>[SCAV]</color>";
                        else if (Source.IndexOf("PMC", StringComparison.OrdinalIgnoreCase) >= 0 && PointId.IndexOf("PMC", StringComparison.OrdinalIgnoreCase) < 0)
                            typeTag = " <color=#50fa7b>[PMC]</color>";
                        else if (Source.IndexOf("PLAYER", StringComparison.OrdinalIgnoreCase) >= 0 && PointId.IndexOf("PLAYER", StringComparison.OrdinalIgnoreCase) < 0)
                            typeTag = " <color=#2563eb>[PLAYER]</color>";
                    }

                    _labelTmp.text = $"{PointId}{typeTag}{statusText}{movedText}{zoneLine}";

                    if (IsSelected)
                    {
                        _labelTmp.color = Color.yellow;
                        _labelTmp.fontSize = 4.2f;
                    }
                    else if (IsHovered)
                    {
                        _labelTmp.color = Color.cyan;
                        _labelTmp.fontSize = 4.0f;
                    }
                    else
                    {
                        if (Status == SpawnReviewStatus.Approved)
                            _labelTmp.color = Color.green;
                        else if (Status == SpawnReviewStatus.Rejected)
                            _labelTmp.color = Color.gray;
                        else if (!string.IsNullOrEmpty(Source) && Source.IndexOf("PLAYER", StringComparison.OrdinalIgnoreCase) >= 0)
                            _labelTmp.color = new Color(0.35f, 0.65f, 1.0f);
                        else
                            _labelTmp.color = Color.white;

                        _labelTmp.fontSize = 3.5f;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            // Billboard contínuo: alinha a orientação do texto com a câmera ativa
            if (_labelObj != null)
            {
                var cam = SpawnPointReviewerManager.GetActiveCamera();
                if (cam != null)
                {
                    _labelObj.transform.rotation = cam.transform.rotation;
                }
            }
        }
    }
}
