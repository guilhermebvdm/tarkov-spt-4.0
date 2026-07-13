using System.Linq;
using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;

namespace CameraRotationMod
{
    // Ícone do mount PASSIVO (item 011), canto inferior direito, direcional (left/right/down).
    // MonoBehaviour anexado ao GameObject PERSISTENTE do plugin (nunca `new GameObject` no boot — lição
    // do item 004, em que o MonoBehaviour criado no chainloader não rodava). Base: MountingUI removido
    // (git ebc2312^), adaptado para ler PassiveMountState e revalidar o estado por timeout.
    public class PassiveMountUI : MonoBehaviour
    {
        public static PassiveMountUI Instance { get; private set; }

        public GameObject ActiveUIScreen;
        private GameObject _iconObj;
        private Image _img;
        private RectTransform _rect;
        private readonly Vector2 _iconSize = new Vector2(80, 80);
        private const float DetectTimeout = 0.3f; // PA-01-03: solta o estado se o detector parou (arma guardada)

        private void Awake() { Instance = this; }

        public void DestroyIcon()
        {
            if (_iconObj != null) { Destroy(_iconObj); _iconObj = null; }
        }

        public void CreateIcon(Transform parent)
        {
            _iconObj = new GameObject("PassiveMountIcon");
            _rect = _iconObj.AddComponent<RectTransform>();
            _rect.anchoredPosition = new Vector2(100, 100);
            _img = _iconObj.AddComponent<Image>();
            _iconObj.transform.SetParent(parent);
            if (Plugin.LoadedSprites.TryGetValue("mounting.png", out var s)) _img.sprite = s;
            _img.raycastTarget = false;
            _img.color = Color.clear;
            _rect.sizeDelta = _iconSize;
        }

        public void Update()
        {
            // CR-01 — este MonoBehaviour continua rodando mesmo se o Awake do Plugin abortar, e a linha
            // abaixo lê Plugin._EnablePassiveMount.Value sem rede de proteção. Foi um dos 3 pontos que
            // inundaram o log com NullReferenceException no incidente de 2026-07-12.
            if (!Plugin.ConfigReady) return;

            // PA-01-03: se o detector (method_11) parou de rodar — arma guardada / inventário — solta o estado.
            if (PassiveMountState.IsBracing && Time.time - PassiveMountState.LastDetectTick > DetectTimeout)
                PassiveMountState.ClearBracing();

            if (ActiveUIScreen == null || _img == null) return;

            if (!Plugin._EnablePassiveMount.Value || !Plugin._ShowMountIcon.Value || !PassiveMountState.IsBracing)
            {
                _img.color = Color.clear;
                return;
            }

            // Sprite por direção do apoio.
            string key = PassiveMountState.Direction == EBracingDir.Left ? "mountingleft.png"
                       : PassiveMountState.Direction == EBracingDir.Right ? "mountingright.png"
                       : "mounting.png";
            if (Plugin.LoadedSprites.TryGetValue(key, out var sprite) && sprite != null)
                _img.sprite = sprite;

            // Pulso suave de alpha (passivo).
            float alpha = Mathf.Lerp(0.2f, 1f, Mathf.PingPong(Time.time, 1f));
            _img.color = new Color(1f, 1f, 1f, alpha);
            _rect.sizeDelta = _iconSize;
            _rect.localPosition = new Vector3(650f, -460f, 0f);
        }
    }

    // Parenta o ícone ao HUD de batalha quando ele aparece. ref: BattleUIScreenPatch (item 004, validado).
    public class BattleUIScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => typeof(EftBattleUIScreen).GetMethods(BindingFlags.Instance | BindingFlags.Public)
               .First(x => x.Name == "Show" && x.GetParameters().Length > 0 && x.GetParameters()[0].Name == "owner");

        [PatchPostfix]
        private static void Postfix(EftBattleUIScreen __instance)
        {
            var ui = PassiveMountUI.Instance;
            if (ui == null) return;
            if (ui.ActiveUIScreen == __instance.gameObject) return;
            ui.ActiveUIScreen = __instance.gameObject;
            ui.DestroyIcon();
            ui.CreateIcon(__instance.transform);
        }
    }
}
