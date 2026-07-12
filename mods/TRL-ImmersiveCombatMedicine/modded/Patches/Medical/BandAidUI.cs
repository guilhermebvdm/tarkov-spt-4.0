using UnityEngine;
using TRLImmersiveCombatMedicine;
using UnityEngine.UI;
using EFT;
using EFT.HealthSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace Band_Aid
{
    public class BandAidUI : MonoBehaviour
    {
        public static BandAidUI Instance;
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BandAid_UI");

        private GameObject _canvasObj;
        private Player _targetPlayer;

        // === Cache de Tipos de Efeitos ===
        private static Type _heavyBleedType, _lightBleedType, _fractureType;
        private static Type _painType, _contusionType, _tremorType, _intoxicationType;
        private static MethodInfo _findEffectMethod;
        private static bool _typesCached = false;

        // === Sprites Nativos de Efeitos ===
        private Sprite _sprHeavyBleed, _sprLightBleed, _sprFracture;
        private Sprite _sprPain, _sprContusion, _sprTremor, _sprIntoxication;
        private Sprite _sprTourniquet;
        private bool _spritesCached = false;

        // === Fonte ===
        private Font _tarkovFont;

        // === UI por membro ===
        private Dictionary<EBodyPart, LimbUI> _limbViews = new Dictionary<EBodyPart, LimbUI>();

        // === Imagens da silhueta ===
        private Dictionary<EBodyPart, Image> _silhouetteImages = new Dictionary<EBodyPart, Image>();
        private bool _hasCustomImages = false;

        // === UI global ===
        private Text _globalHpText;
        private Image _globalBarFill;
        private Image _globalBarBg;
        private Text _titleText;
        private Text _subtitleText;
        private Text _footerText;

        // === ECG ===
        private Texture2D _ecgTexture;
        private RawImage _ecgRawImage;
        private Text _ecgLabel;
        private float[] _ecgBuffer;
        private int _ecgWritePos = 0;
        private float _ecgTimer = 0f;
        private float _ecgHpRatio = 1f;
        private const int ECG_WIDTH = 360;
        private const int ECG_HEIGHT = 35;
        private Color32 _ecgBgColor = new Color32(4, 8, 12, 160);

        // Radar sweep
        private float _radarTimer = 0f;
        private const float RADAR_SWEEP_DURATION = 3f;  // 3s para percorrer
        private const float RADAR_DELAY = 1f;           // 1s de pausa
        private const int RADAR_WIDTH = 160;             // Largura do retângulo

        // Padrão PQRST (onda cardíaca normalizada 0-1, apenas o complexo)
        private static readonly float[] ECG_WAVE = new float[]
        {
            // P wave (onda atrial)
            0.52f, 0.55f, 0.58f, 0.60f, 0.58f, 0.55f, 0.52f,
            // PR segment
            0.50f, 0.50f, 0.50f,
            // Q dip
            0.47f, 0.43f,
            // R peak (pico ventricular)
            0.55f, 0.72f, 0.90f, 0.98f, 0.88f, 0.68f,
            // S trough
            0.38f, 0.28f, 0.32f, 0.40f,
            // ST segment
            0.47f, 0.49f, 0.50f, 0.50f,
            // T wave (repolarização)
            0.52f, 0.55f, 0.59f, 0.62f, 0.63f, 0.62f, 0.59f, 0.55f, 0.52f, 0.50f
        };

        // === Mapeamento EBodyPart â†’ prefixo de imagem ===
        private static readonly Dictionary<EBodyPart, string> BodyPartImagePrefix = new Dictionary<EBodyPart, string>
        {
            { EBodyPart.Head, "cabeca" },
            { EBodyPart.Chest, "torax" },
            { EBodyPart.Stomach, "estomago" },
            { EBodyPart.LeftArm, "bracoe" },
            { EBodyPart.RightArm, "bracod" },
            { EBodyPart.LeftLeg, "pernae" },
            { EBodyPart.RightLeg, "pernad" }
        };

        // === Âncoras da silhueta (para linhas de conexão) ===
        private static readonly Dictionary<EBodyPart, Vector2> SilhouetteAnchor = new Dictionary<EBodyPart, Vector2>
        {
            { EBodyPart.Head,     new Vector2(0, 175) },
            { EBodyPart.Chest,    new Vector2(0, 115) },
            { EBodyPart.Stomach,  new Vector2(0, 40) },
            { EBodyPart.LeftArm,  new Vector2(80, 80) },
            { EBodyPart.RightArm, new Vector2(-80, 80) },
            { EBodyPart.LeftLeg,  new Vector2(40, -85) },
            { EBodyPart.RightLeg, new Vector2(-40, -85) }
        };

        private class LimbUI
        {
            public GameObject Root;
            public Image BarFill;
            public Image BarBg;
            public Outline BarOutline;
            public Text HpText;
            public Text DestroyedX;
            public Text NameText;
            public Image[] EffectSlots;
        }

        // === DIMENSÕES ===
        private const float BAR_WIDTH = 95f;
        private const float BAR_HEIGHT = 12f;
        private const float ICON_SIZE = 18f;    // Aumentado de 13 para 18
        private const int MAX_EFFECTS = 6;
        private const float SILHOUETTE_WIDTH = 230f;
        private const float SILHOUETTE_HEIGHT = 360f;

        // Barra global vertical
        private const float GLOBAL_BAR_W = 16f;
        private const float GLOBAL_BAR_H = 280f;

        // === PALETA DE CORES ===
        private static readonly Color COL_BG_PANEL   = new Color(0.01f, 0.02f, 0.04f, 0.30f);
        private static readonly Color COL_BG_BAR     = new Color(0.04f, 0.06f, 0.08f, 0.80f);
        private static readonly Color COL_BG_LABEL   = new Color(0.03f, 0.05f, 0.07f, 0.70f);
        private static readonly Color COL_BORDER     = new Color(0.12f, 0.25f, 0.30f, 0.35f);
        private static readonly Color COL_LINE_CONN  = new Color(0.15f, 0.28f, 0.32f, 0.25f);
        private static readonly Color COL_TITLE      = new Color(0.55f, 0.80f, 0.85f, 1f);
        private static readonly Color COL_SUBTITLE   = new Color(0.55f, 0.72f, 0.78f, 0.95f); // Mais brilhante
        private static readonly Color COL_LABEL      = new Color(0.70f, 0.75f, 0.78f, 1f);
        private static readonly Color COL_TEXT_HP     = new Color(0.88f, 0.90f, 0.92f, 1f);
        private static readonly Color COL_FOOTER     = new Color(0.35f, 0.40f, 0.30f, 0.75f);
        private static readonly Color COL_DESTROYED  = new Color(0.65f, 0.18f, 0.18f, 1f);

        // Cores gradiente
        private static readonly Color COL_GREEN  = new Color(0.20f, 0.55f, 0.25f, 1f);
        private static readonly Color COL_YELLOW = new Color(0.65f, 0.60f, 0.15f, 1f);
        private static readonly Color COL_ORANGE = new Color(0.70f, 0.40f, 0.10f, 1f);
        private static readonly Color COL_RED    = new Color(0.65f, 0.15f, 0.10f, 1f);
        private static readonly Color COL_BLACK  = new Color(0.05f, 0.05f, 0.05f, 1f);

        public void Awake()
        {
            Instance = this;
            CacheTypes();
            LoadFont();
            LoadCustomImages();
            CreateCanvas();
            Logger.LogInfo("BandAidUI V7 inicializado.");
        }

        // === FONTE (prioridade: Tarkov â†’ condensadas â†’ Arial) ===
        private void LoadFont()
        {
            try
            {
                Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
                // Lista de prioridade
                string[] preferred = { "bender", "tarkov", "din condensed", "bahnschrift", "agency fb", "roboto condensed", "roboto" };

                foreach (string pref in preferred)
                {
                    foreach (var f in allFonts)
                    {
                        if (f.name.ToLowerInvariant().Contains(pref))
                        {
                            _tarkovFont = f;
                            Logger.LogInfo($"Fonte: {f.name}");
                            return;
                        }
                    }
                }

                // Fallback: tenta fontes do sistema
                string[] systemFonts = Font.GetOSInstalledFontNames();
                string[] sysPref = { "DIN Condensed", "Bahnschrift", "Agency FB", "Roboto Condensed" };
                foreach (string sp in sysPref)
                {
                    foreach (string sf in systemFonts)
                    {
                        if (sf.IndexOf(sp, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            _tarkovFont = Font.CreateDynamicFontFromOSFont(sf, 14);
                            Logger.LogInfo($"Fonte sistema: {sf}");
                            return;
                        }
                    }
                }
                Logger.LogWarning("Nenhuma fonte preferida encontrada, usando Arial.");
            }
            catch (Exception ex) { Logger.LogWarning($"Erro fonte: {ex.Message}"); }
        }

        public Font GetFont()
        {
            return _tarkovFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // === CACHE DE TIPOS (1x) ===
        private static void CacheTypes()
        {
            if (_typesCached) return;

            // ref: G-2 (coop-heal-matrix) — detecção por INTERFACE de marcador, não por
            // tipo concreto: os tipos nested do ActiveHC nunca casam com os efeitos do
            // NetworkHealthController (players remotos), deixando o médico "cego" a
            // bleeds/fraturas do paciente. FindActiveEffect<GInterfaceNNN>() funciona
            // nas DUAS famílias — mesmo padrão do vanilla, que consulta fratura em HC
            // de rede com FindActiveEffect<GInterface342> (NetworkHealthControllerAbstractClass).
            // Mapa (idêntico em ActiveHC e NetworkHC, conferido no DLL real):
            _heavyBleedType = typeof(GInterface340);   // HeavyBleeding
            _lightBleedType = typeof(GInterface339);   // LightBleeding
            _fractureType = typeof(GInterface342);     // Fracture
            _contusionType = typeof(GInterface352);    // Contusion
            _tremorType = typeof(GInterface361);       // Tremor
            _painType = typeof(GInterface357);         // Pain (existe no NetworkHC também)
            _intoxicationType = typeof(GInterface346); // Intoxication

            // FindActiveEffect<T> está na INTERFACE IHealthController — funciona em qualquer HC
            var ihcType = typeof(IHealthController);
            var methods = ihcType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var m in methods)
            {
                if (m.Name == "FindActiveEffect" && m.IsGenericMethod && m.GetParameters().Length == 1)
                { _findEffectMethod = m; break; }
            }
            _typesCached = true;
        }

        private void LoadCustomImages()
        {
            foreach (var kvp in BodyPartImagePrefix)
            {
                if (ImageLoader.Exists(kvp.Value + "_verde"))
                { _hasCustomImages = true; break; }
            }
            _sprTourniquet = ImageLoader.Load("ico_tourniquet");
            if (_hasCustomImages) Logger.LogInfo("Imagens customizadas detectadas.");
        }

        private void CacheSprites()
        {
            if (_spritesCached) return;
            try
            {
                var effectIcons = EFTHardSettings.Instance?.StaticIcons?.EffectIcons;
                if (effectIcons == null) return;
                _sprHeavyBleed = effectIcons.HeavyBleeding;
                _sprLightBleed = effectIcons.LightBleeding;
                _sprFracture = effectIcons.Fracture;
                _sprPain = effectIcons.Pain;
                _sprContusion = effectIcons.Contusion;
                _sprTremor = effectIcons.Tremor;
                _sprIntoxication = effectIcons.Intoxication;
                _spritesCached = true;
            }
            catch { }
        }

        private static bool HasEffect(IHealthController hc, EBodyPart bodyPart, Type effectType)
        {
            if (effectType == null || _findEffectMethod == null || hc == null) return false;
            try
            {
                // FindActiveEffect<T> está na interface IHealthController — funciona em qualquer HC
                var genericFind = _findEffectMethod.MakeGenericMethod(effectType);
                return genericFind.Invoke(hc, new object[] { bodyPart }) != null;
            }
            catch { }
            return false;
        }

        // =====================================
        //         CRIAÇÃO DO CANVAS V7
        // =====================================
        private void CreateCanvas()
        {
            _canvasObj = new GameObject("BandAidCanvas_V7");
            _canvasObj.transform.SetParent(transform);
            Canvas canvas = _canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = _canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            _canvasObj.AddComponent<GraphicRaycaster>();

            // === PAINEL PRINCIPAL ===
            GameObject panel = MakePanel(_canvasObj.transform, "MainPanel",
                new Vector2(420, 0), new Vector2(480, 600), COL_BG_PANEL);

            // === TÃTULO ===
            _titleText = MakeText(panel.transform, "Title",
                new Vector2(0, 275), new Vector2(460, 20),
                "SITUAÇÃO DO OPERADOR", 14, TextAnchor.MiddleCenter,
                COL_TITLE, FontStyle.Bold);

            // === SUBTÃTULO (nome do operador, mais brilhante) ===
            _subtitleText = MakeText(panel.transform, "Subtitle",
                new Vector2(0, 258), new Vector2(460, 16),
                "", 11, TextAnchor.MiddleCenter,
                COL_SUBTITLE, FontStyle.Normal);

            // Linha separadora fina
            MakeLine(panel.transform, new Vector2(0, 248), new Vector2(420, 1),
                new Color(COL_BORDER.r, COL_BORDER.g, COL_BORDER.b, 0.20f));

            // === BARRA GLOBAL HP (vertical, à esquerda) ===
            CreateGlobalHpBarVertical(panel.transform);

            // === SILHUETA ===
            if (_hasCustomImages)
                CreateSilhouetteImages(panel.transform);

            // === CONTAINER DE LINHAS (criado ANTES dos blocos â†’ renderiza ATRÃS) ===
            GameObject linesContainer = new GameObject("ConnectionLines");
            linesContainer.transform.SetParent(panel.transform, false);
            RectTransform linesRect = linesContainer.AddComponent<RectTransform>();
            linesRect.anchorMin = linesRect.anchorMax = new Vector2(0.5f, 0.5f);
            linesRect.sizeDelta = Vector2.zero;
            linesRect.anchoredPosition = Vector2.zero;

            // === BLOCOS DE MEMBROS (criados DEPOIS â†’ renderizam NA FRENTE) ===
            CreateLimbBlock(panel.transform, EBodyPart.Head,     "CABEÇA",     new Vector2(115, 162));
            CreateLimbBlock(panel.transform, EBodyPart.Chest,    "TÓRAX",      new Vector2(0,    88));
            CreateLimbBlock(panel.transform, EBodyPart.LeftArm,  "BRAÇO ESQ.", new Vector2(120,   50));
            CreateLimbBlock(panel.transform, EBodyPart.RightArm, "BRAÇO DIR.", new Vector2(-120,  50));
            CreateLimbBlock(panel.transform, EBodyPart.Stomach,  "ESTÔMAGO",   new Vector2(0,    -10));
            CreateLimbBlock(panel.transform, EBodyPart.LeftLeg,  "PERNA ESQ.", new Vector2(110,  -130));
            CreateLimbBlock(panel.transform, EBodyPart.RightLeg, "PERNA DIR.", new Vector2(-110, -130));

            // === LINHAS DE CONEXÃO removidas (feitas manualmente via base.png) ===

            // Linha separadora inferior
            MakeLine(panel.transform, new Vector2(0, -195), new Vector2(420, 1),
                new Color(COL_BORDER.r, COL_BORDER.g, COL_BORDER.b, 0.20f));

            // === ECG (abaixo do operador, largo) ===
            CreateEcg(panel.transform);

            // === RODAPÉ ===
            _footerText = MakeText(panel.transform, "Footer",
                new Vector2(0, -268), new Vector2(440, 32),
                "Utilize as suas teclas de atalhos para curar\n[Pressione F] Fechar Examinador", 12, TextAnchor.MiddleCenter,
                COL_FOOTER, FontStyle.Normal);

            _canvasObj.SetActive(false);
        }

        // === BARRA GLOBAL HP VERTICAL ===
        private void CreateGlobalHpBarVertical(Transform parent)
        {
            // Container posicionado à esquerda do boneco
            GameObject container = new GameObject("GlobalBarContainer");
            container.transform.SetParent(parent, false);
            RectTransform contRect = container.AddComponent<RectTransform>();
            contRect.anchorMin = contRect.anchorMax = new Vector2(0.5f, 0.5f);
            contRect.sizeDelta = new Vector2(GLOBAL_BAR_W + 30, GLOBAL_BAR_H + 28);
            contRect.anchoredPosition = new Vector2(-195, 15);

            // Label "HP" no topo
            MakeText(container.transform, "HpLabel",
                new Vector2(0, GLOBAL_BAR_H / 2 + 10), new Vector2(40, 14),
                "HP", 9, TextAnchor.MiddleCenter, COL_SUBTITLE, FontStyle.Normal);

            // Background da barra
            _globalBarBg = MakeImage(container.transform, "GlobalBarBg",
                Vector2.zero, new Vector2(GLOBAL_BAR_W, GLOBAL_BAR_H), COL_BG_BAR);

            // Fill (vertical: cresce de baixo para cima)
            GameObject fillObj = new GameObject("GlobalBarFill");
            fillObj.transform.SetParent(_globalBarBg.transform, false);
            _globalBarFill = fillObj.AddComponent<Image>();
            _globalBarFill.color = COL_GREEN;
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);      // Baixo
            fillRect.anchorMax = new Vector2(1, 1);       // Cima (ajustado no update)
            fillRect.offsetMin = new Vector2(1, 1);
            fillRect.offsetMax = new Vector2(-1, -1);
            fillRect.pivot = new Vector2(0.5f, 0);        // Pivô em baixo

            // Texto HP
            _globalHpText = MakeText(container.transform, "GlobalHpText",
                new Vector2(0, -(GLOBAL_BAR_H / 2 + 10)), new Vector2(50, 14),
                "440", 9, TextAnchor.MiddleCenter, COL_TEXT_HP, FontStyle.Normal);
            var hpOutline = _globalHpText.gameObject.AddComponent<Outline>();
            hpOutline.effectColor = new Color(0, 0, 0, 0.7f);
            hpOutline.effectDistance = new Vector2(0.5f, -0.5f);
        }

        // === ECG (abaixo do operador, largo) ===
        private void CreateEcg(Transform parent)
        {
            float ecgY = -220f;

            // Bg ECG
            GameObject ecgBg = new GameObject("EcgBg");
            ecgBg.transform.SetParent(parent, false);
            Image ecgBgImg = ecgBg.AddComponent<Image>();
            ecgBgImg.color = new Color(0.02f, 0.03f, 0.05f, 0.55f);
            ecgBgImg.raycastTarget = false;
            RectTransform ecgBgRect = ecgBg.GetComponent<RectTransform>();
            ecgBgRect.anchorMin = ecgBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            ecgBgRect.sizeDelta = new Vector2(420, ECG_HEIGHT + 16);
            ecgBgRect.anchoredPosition = new Vector2(0, ecgY);

            // Label
            _ecgLabel = MakeText(ecgBg.transform, "EcgLabel",
                new Vector2(-195, 0), new Vector2(40, ECG_HEIGHT),
                "â™¥", 17, TextAnchor.MiddleCenter, COL_SUBTITLE, FontStyle.Normal);

            // Texture2D
            _ecgTexture = new Texture2D(ECG_WIDTH, ECG_HEIGHT, TextureFormat.RGBA32, false);
            _ecgTexture.filterMode = FilterMode.Point;
            _ecgTexture.wrapMode = TextureWrapMode.Clamp;

            GameObject ecgImgObj = new GameObject("EcgImage");
            ecgImgObj.transform.SetParent(ecgBg.transform, false);
            _ecgRawImage = ecgImgObj.AddComponent<RawImage>();
            _ecgRawImage.texture = _ecgTexture;
            _ecgRawImage.raycastTarget = false;
            RectTransform ecgImgRect = ecgImgObj.GetComponent<RectTransform>();
            ecgImgRect.anchorMin = ecgImgRect.anchorMax = new Vector2(0.5f, 0.5f);
            ecgImgRect.sizeDelta = new Vector2(385, ECG_HEIGHT);
            ecgImgRect.anchoredPosition = new Vector2(8, 0);

            _ecgBuffer = new float[ECG_WIDTH];
            for (int i = 0; i < ECG_WIDTH; i++) _ecgBuffer[i] = 0.5f;
            ClearEcgTexture();
        }

        private void ClearEcgTexture()
        {
            Color32[] pixels = _ecgTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = _ecgBgColor;
            _ecgTexture.SetPixels32(pixels);
            _ecgTexture.Apply();
        }

        // === SILHUETA ===
        private void CreateSilhouetteImages(Transform parent)
        {
            GameObject silContainer = new GameObject("SilhouetteContainer");
            silContainer.transform.SetParent(parent, false);
            RectTransform contRect = silContainer.AddComponent<RectTransform>();
            contRect.anchorMin = contRect.anchorMax = new Vector2(0.5f, 0.5f);
            contRect.sizeDelta = new Vector2(SILHOUETTE_WIDTH, SILHOUETTE_HEIGHT);
            contRect.anchoredPosition = new Vector2(0, 15);

            Sprite baseSprite = ImageLoader.Load("base");
            if (baseSprite != null)
            {
                GameObject baseObj = new GameObject("Sil_Base");
                baseObj.transform.SetParent(silContainer.transform, false);
                Image baseImg = baseObj.AddComponent<Image>();
                baseImg.sprite = baseSprite;
                baseImg.color = Color.white;
                baseImg.preserveAspect = true;
                baseImg.raycastTarget = false;
                RectTransform baseRect = baseObj.GetComponent<RectTransform>();
                baseRect.anchorMin = Vector2.zero;
                baseRect.anchorMax = Vector2.one;
                baseRect.offsetMin = Vector2.zero;
                baseRect.offsetMax = Vector2.zero;
            }

            foreach (var kvp in BodyPartImagePrefix)
            {
                Sprite defaultSprite = ImageLoader.Load(kvp.Value + "_verde");
                if (defaultSprite == null) continue;

                GameObject imgObj = new GameObject($"Sil_{kvp.Key}");
                imgObj.transform.SetParent(silContainer.transform, false);
                Image img = imgObj.AddComponent<Image>();
                img.sprite = defaultSprite;
                img.color = Color.white;
                img.preserveAspect = true;
                img.raycastTarget = false;
                RectTransform imgRect = imgObj.GetComponent<RectTransform>();
                imgRect.anchorMin = Vector2.zero;
                imgRect.anchorMax = Vector2.one;
                imgRect.offsetMin = Vector2.zero;
                imgRect.offsetMax = Vector2.zero;

                _silhouetteImages[kvp.Key] = img;
            }
        }

        // === LINHAS DE CONEXÃO ===
        private void CreateConnectionLines(Transform parent)
        {
            foreach (var kvp in _limbViews)
            {
                if (!SilhouetteAnchor.TryGetValue(kvp.Key, out Vector2 silPos)) continue;

                RectTransform limbRect = kvp.Value.Root.GetComponent<RectTransform>();
                Vector2 limbPos = limbRect.anchoredPosition;
                Vector2 barCenter = limbPos + new Vector2(0, 2);
                Vector2 silCenter = silPos;

                Vector2 diff = silCenter - barCenter;
                float distance = diff.magnitude;
                float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

                if (distance < 15f) continue;

                GameObject lineObj = new GameObject($"ConnLine_{kvp.Key}");
                lineObj.transform.SetParent(parent, false);
                Image lineImg = lineObj.AddComponent<Image>();
                lineImg.color = new Color(1f, 1f, 1f, 0.35f);
                lineImg.raycastTarget = false;

                RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                lineRect.anchorMin = lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                lineRect.sizeDelta = new Vector2(distance, 2.5f);
                lineRect.anchoredPosition = (barCenter + silCenter) / 2f;
                lineRect.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // === BLOCO DE MEMBRO ===
        private void CreateLimbBlock(Transform parent, EBodyPart part, string label, Vector2 position)
        {
            var limb = new LimbUI();

            limb.Root = new GameObject($"Limb_{part}");
            limb.Root.transform.SetParent(parent, false);
            RectTransform rootRect = limb.Root.AddComponent<RectTransform>();
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(BAR_WIDTH + 4, 54);
            rootRect.anchoredPosition = position;

            // === Fundo do label (mesma largura da barra) ===
            GameObject labelBg = new GameObject("LabelBg");
            labelBg.transform.SetParent(limb.Root.transform, false);
            Image labelBgImg = labelBg.AddComponent<Image>();
            labelBgImg.color = COL_BG_LABEL;
            labelBgImg.raycastTarget = false;
            RectTransform labelBgRect = labelBg.GetComponent<RectTransform>();
            labelBgRect.anchorMin = labelBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelBgRect.sizeDelta = new Vector2(BAR_WIDTH, 14);   // Mesma largura da barra
            labelBgRect.anchoredPosition = new Vector2(0, 16);

            // Nome do membro
            limb.NameText = MakeText(limb.Root.transform, "Name",
                new Vector2(0, 16), new Vector2(BAR_WIDTH, 14),
                label, 10, TextAnchor.MiddleCenter,
                COL_LABEL, FontStyle.Normal);
            var nameOutline = limb.NameText.gameObject.AddComponent<Outline>();
            nameOutline.effectColor = new Color(0, 0, 0, 0.6f);
            nameOutline.effectDistance = new Vector2(0.5f, -0.5f);

            // Barra HP Background
            GameObject barBg = new GameObject("BarBg");
            barBg.transform.SetParent(limb.Root.transform, false);
            limb.BarBg = barBg.AddComponent<Image>();
            limb.BarBg.color = COL_BG_BAR;
            RectTransform barBgRect = barBg.GetComponent<RectTransform>();
            barBgRect.anchorMin = barBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            barBgRect.sizeDelta = new Vector2(BAR_WIDTH, BAR_HEIGHT);
            barBgRect.anchoredPosition = new Vector2(0, 3);
            // Outline para contorno vermelho quando 0HP
            limb.BarOutline = barBg.AddComponent<Outline>();
            limb.BarOutline.effectColor = Color.clear;
            limb.BarOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Barra HP Fill
            GameObject barFill = new GameObject("BarFill");
            barFill.transform.SetParent(barBg.transform, false);
            limb.BarFill = barFill.AddComponent<Image>();
            limb.BarFill.color = COL_GREEN;
            RectTransform fillRect = barFill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = new Vector2(1, 1);
            fillRect.offsetMax = new Vector2(-1, -1);
            fillRect.pivot = new Vector2(0, 0.5f);

            // Texto HP atual/max
            limb.HpText = MakeText(barBg.transform, "HpValue",
                new Vector2(0, 0), new Vector2(BAR_WIDTH - 16, BAR_HEIGHT),
                "", 9, TextAnchor.MiddleCenter, COL_TEXT_HP, FontStyle.Normal);
            var hpOutline = limb.HpText.gameObject.AddComponent<Outline>();
            hpOutline.effectColor = new Color(0, 0, 0, 0.85f);
            hpOutline.effectDistance = new Vector2(0.5f, -0.5f);

            // "X" destruído (alinhado à direita da barra)
            limb.DestroyedX = MakeText(barBg.transform, "DestroyedX",
                new Vector2(BAR_WIDTH / 2 - 7, 0), new Vector2(12, BAR_HEIGHT),
                "âœ•", 9, TextAnchor.MiddleCenter, COL_DESTROYED, FontStyle.Normal);
            var xOutline = limb.DestroyedX.gameObject.AddComponent<Outline>();
            xOutline.effectColor = new Color(0, 0, 0, 0.7f);
            xOutline.effectDistance = new Vector2(0.5f, -0.5f);
            limb.DestroyedX.gameObject.SetActive(false);

            // Slots de ícones de efeitos (tamanho aumentado)
            limb.EffectSlots = new Image[MAX_EFFECTS];
            for (int i = 0; i < MAX_EFFECTS; i++)
            {
                GameObject slot = new GameObject($"Eff_{i}");
                slot.transform.SetParent(limb.Root.transform, false);
                limb.EffectSlots[i] = slot.AddComponent<Image>();
                limb.EffectSlots[i].color = Color.white;
                limb.EffectSlots[i].preserveAspect = true;
                RectTransform slotRect = slot.GetComponent<RectTransform>();
                slotRect.anchorMin = slotRect.anchorMax = new Vector2(0.5f, 0.5f);
                slotRect.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE);
                float leftEdge = -(BAR_WIDTH / 2f) + (ICON_SIZE / 2f);
                slotRect.anchoredPosition = new Vector2(leftEdge + i * (ICON_SIZE + 2), -14);
                slot.SetActive(false);
            }

            _limbViews[part] = limb;
        }

        // =====================================
        //         SHOW / HIDE
        // =====================================
        public void ShowUI(Player target)
        {
            _targetPlayer = target;
            CacheSprites();
            if (_canvasObj != null)
            {
                _canvasObj.SetActive(true);
                _lastUpdateTime = 0f;
            }
        }

        public void HideUI()
        {
            _targetPlayer = null;
            if (_canvasObj != null) _canvasObj.SetActive(false);
        }

        // =====================================
        //       UPDATE
        // =====================================
        private float _lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.25f;

        public void Update()
        {
            if (_targetPlayer == null || _canvasObj == null || !_canvasObj.activeSelf) return;

            UpdateEcg();

            if (Time.time - _lastUpdateTime < UPDATE_INTERVAL) return;
            _lastUpdateTime = Time.time;

            try
            {
                var hc = _targetPlayer.HealthController;
                var profile = _targetPlayer.Profile;

                if (hc == null || profile == null)
                { _subtitleText.text = "INDISPONÃVEL"; return; }

                // === AUTO-CLOSE: fechar HUD se médico se afastou do paciente ===
                var mainPlayer = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer != null && _targetPlayer != null)
                {
                    // ref: CR-01-01 — regra ÚNICA de distância (config + 1 m de margem
                    // feet-to-feet, igual ao BandAidController). O valor antigo (1,0 m)
                    // fechava o HUD imediatamente: lado a lado já são ~1,5 m.
                    float maxDist = TRLImmersiveCombatMedicinePlugin.MedicInteractDistance.Value + 1f;
                    float dist = Vector3.Distance(mainPlayer.Position, _targetPlayer.Position);
                    if (dist > maxDist)
                    {
                        Logger.LogInfo($"Paciente distante ({dist:F1}m > {maxDist:F1}m), fechando HUD.");
                        HideUI();
                        // Notificar BandAidPlugin para desativar modo médico
                        BandAidController.Instance.DeactivateMedicModeExternal();
                        return;
                    }
                }

                _subtitleText.text = (profile.Nickname ?? "???").ToUpper();

                float totalCur = 0, totalMax = 0;

                foreach (var kvp in _limbViews)
                {
                    var hp = hc.GetBodyPartHealth(kvp.Key);
                    totalCur += hp.Current;
                    totalMax += hp.Maximum;
                    UpdateLimb(hc, kvp.Key, kvp.Value, hp);

                    if (_hasCustomImages)
                        UpdateSilhouetteImage(kvp.Key, hp);
                }

                // Global HP vertical
                float totalRatio = totalMax > 0 ? totalCur / totalMax : 0f;
                RectTransform gFill = _globalBarFill.GetComponent<RectTransform>();
                gFill.anchorMin = new Vector2(0, 0);
                gFill.anchorMax = new Vector2(1, Mathf.Clamp01(totalRatio));
                gFill.offsetMin = new Vector2(1, 1);
                gFill.offsetMax = new Vector2(-1, -1);
                Color hpColor = GetBarColor(totalRatio, totalCur <= 0);
                _globalBarFill.color = hpColor;
                _globalHpText.text = $"{Mathf.Round(totalCur)}";
                _ecgHpRatio = totalRatio;

                // Item 2: Cor do HP no título e no nome do paciente
                _titleText.color = hpColor;
                _subtitleText.color = hpColor;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Erro UI V7: {ex.Message}");
            }
        }

        // === ECG UPDATE (pulsos contínuos, ritmo baseado no BPM/HP) ===
        private int _ecgWavePos = 0;
        private float _ecgBeatTimer = 0f;
        private bool _ecgPulseActive = false;

        private void UpdateEcg()
        {
            if (_ecgTexture == null || _ecgBuffer == null) return;

            // Radar sweep timer
            _radarTimer += Time.deltaTime;

            // BPM dinâmico: HP alto = 65bpm (calmo), HP baixo = 140bpm (taquicardia)
            float bpm = Mathf.Lerp(200f, 100f, _ecgHpRatio);
            float beatInterval = 60f / bpm;

            // Velocidade de scroll proporcional ao BPM
            // BPM alto = scroll mais rápido para manter proporção visual correta
            float basePixelsPerSec = ECG_WIDTH / 4f;
            float bpmFactor = bpm / 80f; // Normalizado para 80bpm
            float pixelsPerSec = basePixelsPerSec * Mathf.Lerp(0.8f, 1.4f, (bpmFactor - 0.8f) / 0.95f);

            _ecgTimer += Time.deltaTime * pixelsPerSec;
            _ecgBeatTimer += Time.deltaTime;

            int steps = Mathf.FloorToInt(_ecgTimer);
            if (steps <= 0) return;
            _ecgTimer -= steps;
            steps = Mathf.Min(steps, 5);

            for (int s = 0; s < steps; s++)
            {
                if (_ecgPulseActive)
                {
                    // Escrevendo amostras do complexo PQRST
                    _ecgBuffer[_ecgWritePos] = ECG_WAVE[_ecgWavePos];
                    _ecgWavePos++;
                    if (_ecgWavePos >= ECG_WAVE.Length)
                    {
                        _ecgWavePos = 0;
                        _ecgPulseActive = false;
                        _ecgBeatTimer = 0f; // Reset timer para próximo batimento
                    }
                }
                else
                {
                    // Baseline plana entre batimentos
                    _ecgBuffer[_ecgWritePos] = 0.5f;

                    // Inicia novo pulso quando intervalo BPM é atingido
                    if (_ecgBeatTimer >= beatInterval && _ecgHpRatio > 0f)
                    {
                        _ecgPulseActive = true;
                        _ecgWavePos = 0;
                    }
                }
                _ecgWritePos = (_ecgWritePos + 1) % ECG_WIDTH;
            }

            RenderEcgTexture();

            // === PULSAR O CORAÇÃO sincronizado com o ECG ===
            if (_ecgLabel != null)
            {
                int lastIdx = (_ecgWritePos + ECG_WIDTH - 1) % ECG_WIDTH;
                float currentVal = _ecgBuffer[lastIdx];
                // Desvio da baseline (0.5) — quanto maior, mais o coração pulsa
                float deviation = Mathf.Abs(currentVal - 0.5f) * 2f; // 0.0 a ~1.0
                // Escala: 1.0 (baseline) até 2.0 (pico R)
                float scale = 1f + deviation * 1.5f;
                _ecgLabel.transform.localScale = Vector3.Lerp(
                    _ecgLabel.transform.localScale,
                    new Vector3(scale, scale, 1f),
                    Time.deltaTime * 15f); // Suaviza a transição
            }
        }

        // ref: CR-01-12 — buffer reutilizado: alocar Color32[12600] (~50KB) POR FRAME
        // com a HUD aberta gerava pressão de GC contínua.
        private Color32[] _ecgPixels;

        private void RenderEcgTexture()
        {
            if (_ecgPixels == null) _ecgPixels = new Color32[ECG_WIDTH * ECG_HEIGHT];
            Color32[] pixels = _ecgPixels;
            Color32 bg = _ecgBgColor;

            Color lineColor = GetBarColor(_ecgHpRatio, _ecgHpRatio <= 0f);
            Color32 lineCol32 = lineColor;
            Color glowColor = new Color(lineColor.r * 0.3f, lineColor.g * 0.3f, lineColor.b * 0.3f, 0.35f);
            Color32 glowCol32 = glowColor;

            for (int i = 0; i < pixels.Length; i++) pixels[i] = bg;

            // Grid sutil
            Color32 gridCol = new Color32(10, 18, 22, 30);
            for (int gridY = 0; gridY < ECG_HEIGHT; gridY += (ECG_HEIGHT / 4))
            {
                for (int x = 0; x < ECG_WIDTH; x++)
                    pixels[gridY * ECG_WIDTH + x] = gridCol;
            }

            for (int x = 0; x < ECG_WIDTH; x++)
            {
                int bufIdx = (_ecgWritePos + x) % ECG_WIDTH;
                float val = _ecgBuffer[bufIdx];
                int y = Mathf.Clamp(Mathf.RoundToInt(val * (ECG_HEIGHT - 1)), 0, ECG_HEIGHT - 1);

                pixels[y * ECG_WIDTH + x] = lineCol32;
                if (y > 0) pixels[(y - 1) * ECG_WIDTH + x] = glowCol32;
                if (y < ECG_HEIGHT - 1) pixels[(y + 1) * ECG_WIDTH + x] = glowCol32;

                if (x > 0)
                {
                    int prevBufIdx = (_ecgWritePos + x - 1) % ECG_WIDTH;
                    int prevY = Mathf.Clamp(Mathf.RoundToInt(_ecgBuffer[prevBufIdx] * (ECG_HEIGHT - 1)), 0, ECG_HEIGHT - 1);
                    int minY = Mathf.Min(y, prevY);
                    int maxY = Mathf.Max(y, prevY);
                    for (int fillY = minY; fillY <= maxY; fillY++)
                        pixels[fillY * ECG_WIDTH + x] = lineCol32;
                }

                if (x < 15)
                {
                    float fade = x / 15f;
                    int idx = y * ECG_WIDTH + x;
                    Color32 c = pixels[idx];
                    pixels[idx] = new Color32(
                        (byte)(c.r * fade), (byte)(c.g * fade), (byte)(c.b * fade), (byte)(c.a * fade));
                }
            }

            // === PULSO DE RADAR (sweep da esquerda para direita) ===
            {
                float totalCycle = RADAR_SWEEP_DURATION + RADAR_DELAY;
                float phase = _radarTimer % totalCycle;
                if (phase < RADAR_SWEEP_DURATION)
                {
                    float progress = phase / RADAR_SWEEP_DURATION; // 0.0 a 1.0
                    int sweepRight = Mathf.RoundToInt(progress * (ECG_WIDTH - 1));
                    int sweepLeft = Mathf.Max(0, sweepRight - RADAR_WIDTH);

                    for (int x = sweepLeft; x <= sweepRight; x++)
                    {
                        // Gradiente: esquerda = totalmente transparente, direita = 20% opacidade
                        float t = (float)(x - sweepLeft) / Mathf.Max(1, sweepRight - sweepLeft);
                        float alpha = t * 0.80f; // 0% â†’ 80%

                        for (int y = 0; y < ECG_HEIGHT; y++)
                        {
                            int idx = y * ECG_WIDTH + x;
                            Color32 existing = pixels[idx];
                            // Blend aditivo com cor do HP
                            byte r = (byte)Mathf.Min(255, existing.r + (int)(lineColor.r * 255 * alpha));
                            byte g = (byte)Mathf.Min(255, existing.g + (int)(lineColor.g * 255 * alpha));
                            byte b = (byte)Mathf.Min(255, existing.b + (int)(lineColor.b * 255 * alpha));
                            byte a = (byte)Mathf.Max(existing.a, (byte)(alpha * 255));
                            pixels[idx] = new Color32(r, g, b, a);
                        }
                    }

                    // Borda sólida no lado direito (2px de largura)
                    for (int bx = sweepRight; bx >= Mathf.Max(0, sweepRight - 1); bx--)
                    {
                        for (int y = 0; y < ECG_HEIGHT; y++)
                        {
                            pixels[y * ECG_WIDTH + bx] = lineCol32;
                        }
                    }
                }
            }

            // === BOLINHA BRILHANTE no final direito do ECG ===
            // Posição Y = último valor do buffer (ponta da linha)
            {
                int lastBufIdx = (_ecgWritePos + ECG_WIDTH - 1) % ECG_WIDTH;
                float lastVal = _ecgBuffer[lastBufIdx];
                int dotY = Mathf.Clamp(Mathf.RoundToInt(lastVal * (ECG_HEIGHT - 1)), 0, ECG_HEIGHT - 1);
                int dotX = ECG_WIDTH - 1;

                // Raio do glow externo e interno (aumentado para mais brilho)
                int glowRadius = 16;
                int coreRadius = 2;

                for (int dy = -glowRadius; dy <= glowRadius; dy++)
                {
                    for (int dx = -glowRadius; dx <= glowRadius; dx++)
                    {
                        int px = dotX + dx;
                        int py = dotY + dy;
                        if (px < 0 || px >= ECG_WIDTH || py < 0 || py >= ECG_HEIGHT) continue;

                        float dist = Mathf.Sqrt(dx * dx + dy * dy);

                        if (dist <= coreRadius)
                        {
                            // Núcleo branco puro
                            pixels[py * ECG_WIDTH + px] = new Color32(255, 255, 255, 255);
                        }
                        else if (dist <= glowRadius)
                        {
                            // Glow intenso (cor do HP com fade mais forte)
                            float fade = 1f - (dist - coreRadius) / (glowRadius - coreRadius);
                            fade = fade * fade * 2.5f; // Brilho intenso
                            fade = Mathf.Clamp01(fade);
                            Color32 existing = pixels[py * ECG_WIDTH + px];
                            // Blend aditivo forte
                            pixels[py * ECG_WIDTH + px] = new Color32(
                                (byte)Mathf.Min(255, existing.r + (int)(lineColor.r * 255 * fade)),
                                (byte)Mathf.Min(255, existing.g + (int)(lineColor.g * 255 * fade)),
                                (byte)Mathf.Min(255, existing.b + (int)(lineColor.b * 255 * fade)),
                                (byte)Mathf.Max(existing.a, (byte)(fade * 255)));
                        }
                    }
                }
            }

            _ecgTexture.SetPixels32(pixels);
            _ecgTexture.Apply();
        }

        // === UPDATE MEMBRO ===
        private void UpdateLimb(IHealthController hc, EBodyPart part, LimbUI limb, ValueStruct hp)
        {
            float current = hp.Current;
            float max = hp.Maximum;
            float ratio = max > 0 ? current / max : 0f;
            bool destroyed = current <= 0;

            limb.HpText.text = $"{Mathf.Round(current)}/{max}";
            limb.HpText.color = destroyed ? new Color(0.5f, 0.5f, 0.5f, 0.6f) : COL_TEXT_HP;

            limb.DestroyedX.gameObject.SetActive(destroyed);

            RectTransform fillRect = limb.BarFill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1);
            fillRect.offsetMin = new Vector2(1, 1);
            fillRect.offsetMax = new Vector2(-1, -1);
            limb.BarFill.color = GetBarColor(ratio, destroyed);

            // Contorno vermelho quando 0HP
            limb.BarOutline.effectColor = destroyed ? COL_DESTROYED : Color.clear;

            limb.NameText.color = destroyed ? COL_DESTROYED : COL_LABEL;

            UpdateEffects(hc, part, limb);
        }

        // === SILHUETA ===
        private void UpdateSilhouetteImage(EBodyPart part, ValueStruct hp)
        {
            if (!_silhouetteImages.TryGetValue(part, out Image img)) return;
            if (!BodyPartImagePrefix.TryGetValue(part, out string prefix)) return;

            float ratio = hp.Maximum > 0 ? hp.Current / hp.Maximum : 0f;
            string suffix = GetColorSuffix(ratio);

            if (_targetPlayer != null && HasEffect(_targetPlayer.HealthController, part, _fractureType))
            {
                Sprite fracSprite = ImageLoader.Load(prefix + "_fratura");
                if (fracSprite != null) { img.sprite = fracSprite; return; }
            }

            Sprite sprite = ImageLoader.Load(prefix + suffix);
            if (sprite != null) img.sprite = sprite;
        }

        private string GetColorSuffix(float ratio)
        {
            if (ratio <= 0f) return "_preto";
            if (ratio < 0.25f) return "_vermelho";
            if (ratio < 0.50f) return "_laranja";
            if (ratio < 0.75f) return "_amarelo";
            return "_verde";
        }

        // === EFEITOS ===
        private void UpdateEffects(IHealthController hc, EBodyPart part, LimbUI limb)
        {
            int idx = 0;
            // TORNIQUETE DESATIVADO — manter vanilla por enquanto
            // if (TourniquetManager.HasTourniquet(part))
            //     TryShowEffectDirect(_sprTourniquet, COL_ORANGE, limb, ref idx);

            TryShowEffect(hc, part, _heavyBleedType, _sprHeavyBleed, new Color(0.8f, 0.2f, 0.15f), limb, ref idx);
            TryShowEffect(hc, part, _lightBleedType, _sprLightBleed, new Color(0.8f, 0.45f, 0.15f), limb, ref idx);
            TryShowEffect(hc, part, _fractureType, _sprFracture, new Color(0.8f, 0.8f, 0.2f), limb, ref idx);
            TryShowEffect(hc, part, _painType, _sprPain, new Color(0.6f, 0.25f, 0.8f), limb, ref idx);
            TryShowEffect(hc, part, _contusionType, _sprContusion, new Color(0.25f, 0.4f, 0.8f), limb, ref idx);
            TryShowEffect(hc, part, _tremorType, _sprTremor, new Color(0.1f, 0.55f, 0.55f), limb, ref idx);

            for (int i = idx; i < MAX_EFFECTS; i++)
                limb.EffectSlots[i].gameObject.SetActive(false);
        }

        private void TryShowEffect(IHealthController hc, EBodyPart part, Type effectType, Sprite sprite, Color fallback, LimbUI limb, ref int idx)
        {
            if (idx >= MAX_EFFECTS || !HasEffect(hc, part, effectType)) return;
            TryShowEffectDirect(sprite, fallback, limb, ref idx);
        }

        private void TryShowEffectDirect(Sprite sprite, Color fallback, LimbUI limb, ref int idx)
        {
            if (idx >= MAX_EFFECTS) return;
            var slot = limb.EffectSlots[idx];
            slot.gameObject.SetActive(true);
            if (sprite != null) { slot.sprite = sprite; slot.color = Color.white; }
            else { slot.sprite = null; slot.color = fallback; }
            idx++;
        }

        // === COR DO GRADIENTE ===
        private Color GetBarColor(float ratio, bool destroyed)
        {
            if (destroyed || ratio <= 0f) return COL_BLACK;
            if (ratio >= 0.75f) return Color.Lerp(COL_YELLOW, COL_GREEN, (ratio - 0.75f) / 0.25f);
            if (ratio >= 0.50f) return Color.Lerp(COL_ORANGE, COL_YELLOW, (ratio - 0.50f) / 0.25f);
            if (ratio >= 0.25f) return Color.Lerp(COL_RED, COL_ORANGE, (ratio - 0.25f) / 0.25f);
            return COL_RED;
        }

        // =====================================
        //         HELPERS
        // =====================================
        private GameObject MakePanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            return obj;
        }

        private Text MakeText(Transform parent, string name, Vector2 pos, Vector2 size,
            string content, int fontSize, TextAnchor align, Color color, FontStyle style)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Text text = obj.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            text.supportRichText = true;
            text.alignment = align;
            text.text = content;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            return text;
        }

        private Image MakeImage(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            return img;
        }

        private void MakeLine(Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject("TechLine");
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
        }
    }
}

