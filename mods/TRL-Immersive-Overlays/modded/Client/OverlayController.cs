using System;
using System.Collections;
using System.IO;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.UI;

namespace TRL_Immersive_Overlays
{
    public class OverlayController : MonoBehaviour
    {
        private Texture2D overlayTexture;
        private const string RaybenchId = "5aa2b9aee5b5b00015693121";

        private GameObject canvasObj;
        private RawImage overlayImage;
        private RectTransform overlayRect;
        
        private bool wasEquipped = false;
        private bool pendingAnimation = false;
        private Coroutine animationCoroutine;

        private void Start()
        {
            string imagePath = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "TRL-Immersive-Overlays", "assets", "raybench.png");
            
            if (File.Exists(imagePath))
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(imagePath);
                    overlayTexture = new Texture2D(2, 2);
                    ImageConversion.LoadImage(overlayTexture, fileData);
                    Plugin.LogSource.LogInfo("TRL-Immersive-Overlays: Textura carregada com sucesso.");

                    SetupCanvas();
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError($"TRL-Immersive-Overlays: Erro ao carregar a imagem - {ex.Message}");
                }
            }
        }

        private void SetupCanvas()
        {
            canvasObj = new GameObject("TRL_OverlayCanvas");
            DontDestroyOnLoad(canvasObj);
            
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -100; // Atrás da UI do Tarkov

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var imageObj = new GameObject("OverlayImage");
            imageObj.transform.SetParent(canvasObj.transform, false);
            
            overlayImage = imageObj.AddComponent<RawImage>();
            overlayImage.texture = overlayTexture;
            overlayImage.raycastTarget = false; 
            overlayImage.color = new Color(1, 1, 1, 0);
            
            overlayRect = overlayImage.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            // Inicia fora da tela (Y positivo = para cima)
            overlayRect.anchoredPosition = new Vector2(0, Screen.height);
        }

        private void Update()
        {
            if (overlayImage == null) return;

            bool isEquipped = CheckIfEquipped();

            if (isEquipped != wasEquipped)
            {
                wasEquipped = isEquipped;
                pendingAnimation = true;
            }

            // Aguarda o inventário ser fechado para disparar a animação visual (Cursor locked = fora de menus)
            if (pendingAnimation && Cursor.lockState == CursorLockMode.Locked)
            {
                pendingAnimation = false;
                HandleStateChange(isEquipped);
            }
        }

        private bool CheckIfEquipped()
        {
            if (!Plugin.EnableMod.Value)
                return false;

            if (!Singleton<GameWorld>.Instantiated || Camera.main == null)
                return false;

            var player = Singleton<GameWorld>.Instance.MainPlayer;
            if (player == null || player.Profile == null || player.Profile.Inventory == null || player.Profile.Inventory.Equipment == null)
                return false;

            if (player.PointOfView != EPointOfView.FirstPerson)
                return false;

            var eyewearSlot = player.Profile.Inventory.Equipment.GetSlot(EquipmentSlot.Eyewear);
            return eyewearSlot != null && eyewearSlot.ContainedItem != null && eyewearSlot.ContainedItem.TemplateId == RaybenchId;
        }

        private void HandleStateChange(bool isEquipping)
        {
            if (isEquipping && canvasObj != null)
            {
                canvasObj.SetActive(true);
            }

            if (Singleton<GameWorld>.Instantiated)
            {
                var player = Singleton<GameWorld>.Instance.MainPlayer;
                if (player != null)
                {
                    // A animação nativa do Visor exige um FaceShieldComponent real e uma operação de rede.
                    // Forçar EInteraction.NightVisionOnGear ou FaceshieldOnGear via HandsAnimator causa
                    // movimentos incorretos dependendo da arma. Removido para manter a imersão visual perfeita.
                }
            }

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateOverlay(isEquipping));
        }

        private IEnumerator AnimateOverlay(bool isEquipping)
        {
            float duration = 0.8f; // Tempo da animação da mão
            float elapsed = 0f;

            // Invertido: starts from bottom (-Screen.height) when equipping, moves to 0
            // When unequipping: starts from 0, moves to bottom (-Screen.height)
            float startY = isEquipping ? -Screen.height : 0f;
            float endY = isEquipping ? 0f : -Screen.height;
            float startAlpha = isEquipping ? 0f : 1f;
            float endAlpha = isEquipping ? 1f : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Easing in-out
                float smoothT = t * t * (3f - 2f * t);

                overlayRect.anchoredPosition = new Vector2(0, Mathf.Lerp(startY, endY, smoothT));
                overlayImage.color = new Color(1, 1, 1, Mathf.Lerp(startAlpha, endAlpha, smoothT));

                yield return null;
            }

            overlayRect.anchoredPosition = new Vector2(0, endY);
            overlayImage.color = new Color(1, 1, 1, endAlpha);

            if (!isEquipping)
            {
                canvasObj.SetActive(false);
            }
        }
    }
}
