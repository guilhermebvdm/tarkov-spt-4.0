using System;
using System.Linq;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using UnityEngine.UI;

namespace CameraRotationMod
{
    public class MountingUI : MonoBehaviour
    {
        public GameObject ActiveUIScreen;
        private GameObject _mountingUIGameObject;
        private Image _mountingUIImage;
        private RectTransform _mountingUIRect;
        private Vector2 _iconSize = new Vector2(80, 80);

        public void DestroyGameObject()
        {
            if (_mountingUIGameObject != null)
            {
                Destroy(_mountingUIGameObject);
                _mountingUIGameObject = null;
            }
        }

        public void CreateGameObject(UnityEngine.Transform parent)
        {
            _mountingUIGameObject = new GameObject("MountingUI");
            _mountingUIRect = _mountingUIGameObject.AddComponent<RectTransform>();
            _mountingUIRect.anchoredPosition = new Vector2(100, 100);
            _mountingUIImage = _mountingUIGameObject.AddComponent<Image>();
            _mountingUIGameObject.transform.SetParent(parent);

            if (Plugin.LoadedSprites.TryGetValue("mounting.png", out var sprite))
            {
                _mountingUIImage.sprite = sprite;
            }
            _mountingUIImage.raycastTarget = false;
            _mountingUIImage.color = Color.clear;
            _mountingUIRect.sizeDelta = _iconSize;
        }

        public void Update()
        {
            if (ActiveUIScreen != null && Plugin._EnableWeaponMounting.Value)
            {
                if (_mountingUIImage == null) return;

                Sprite newSprite = null;
                if (MountingManager.BracingDirection == EBracingDirection.Left)
                {
                    Plugin.LoadedSprites.TryGetValue("mountingleft.png", out newSprite);
                }
                else if (MountingManager.BracingDirection == EBracingDirection.Right)
                {
                    Plugin.LoadedSprites.TryGetValue("mountingright.png", out newSprite);
                }
                else
                {
                    Plugin.LoadedSprites.TryGetValue("mounting.png", out newSprite);
                }

                if (newSprite != null)
                {
                    _mountingUIImage.sprite = newSprite;
                }

                var gameWorld = StanceManager.GetCachedGameWorld();
                var player = gameWorld?.MainPlayer;
                bool isSprinting = player != null && player.IsSprintEnabled;

                if (MountingManager.IsMounting)
                {
                    _mountingUIImage.color = Color.white;
                    float scaleAmount = Mathf.Lerp(1f, 1.15f, Mathf.PingPong(Time.time * 0.9f, 1f));
                    _mountingUIRect.sizeDelta = _iconSize * scaleAmount;
                }
                else if (MountingManager.IsBracing && !isSprinting)
                {
                    _mountingUIRect.sizeDelta = _iconSize;
                    float alpha = Mathf.Lerp(0.2f, 1f, Mathf.PingPong(Time.time, 1f));
                    Color lerpedColor = new Color(1f, 1f, 1f, alpha);
                    _mountingUIImage.color = lerpedColor;
                }
                else
                {
                    _mountingUIImage.color = Color.clear;
                }

                _mountingUIRect.localPosition = new Vector3(650f, -460f, 0f);
            }
        }
    }

    public class BattleUIScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EFT.UI.EftBattleUIScreen).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(x => x.Name == "Show" && x.GetParameters()[0].Name == "owner");
        }

        [PatchPostfix]
        private static void PatchPostFix(EFT.UI.EftBattleUIScreen __instance)
        {
            if (Plugin.MountingUIGameObject == null) return;

            MountingUI mountingUI = Plugin.MountingUIGameObject.GetComponent<MountingUI>();
            if (mountingUI != null) 
            {
                if (mountingUI.ActiveUIScreen == __instance.gameObject) return;
                mountingUI.ActiveUIScreen = __instance.gameObject;
                mountingUI.DestroyGameObject();
                mountingUI.CreateGameObject(__instance.transform);
            }
        }
    }
}
