using System;
using EFT;
using UnityEngine;

namespace CameraRotationMod.UI
{
    public class OxygenUI : MonoBehaviour
    {
        private Texture2D _whiteTexture;
        private GUIStyle _bgStyle;
        private GUIStyle _fgStyle;
        
        private float _currentAlpha = 0f;
        private const float FADE_SPEED = 3f;

        private void Awake()
        {
            _whiteTexture = new Texture2D(1, 1);
            _whiteTexture.SetPixel(0, 0, Color.white);
            _whiteTexture.Apply();

            _bgStyle = new GUIStyle();
            _fgStyle = new GUIStyle();
        }

        private void OnGUI()
        {
            if (Plugin._EnableOxygenUI == null || !Plugin._EnableOxygenUI.Value)
                return;

            // Encontra o player atual
            Player player = Comfort.Common.Singleton<EFT.GameWorld>.Instantiated && Comfort.Common.Singleton<EFT.GameWorld>.Instance.MainPlayer != null
                ? Comfort.Common.Singleton<EFT.GameWorld>.Instance.MainPlayer
                : null;

            if (player == null || player.Physical == null || player.Physical.Oxygen == null)
            {
                _currentAlpha = 0f;
                return;
            }

            // Define se deve aparecer
            bool shouldShow = player.Physical.HoldingBreath;
            
            // Controle suave de Alpha (Fade In / Fade Out)
            float dt = Time.deltaTime;
            if (shouldShow)
                _currentAlpha = Mathf.MoveTowards(_currentAlpha, 1f, dt * FADE_SPEED);
            else
                _currentAlpha = Mathf.MoveTowards(_currentAlpha, 0f, dt * FADE_SPEED);

            if (_currentAlpha <= 0.01f)
                return;

            // Valores de config
            float xPos = Plugin._OxygenUIPosX.Value;
            float yPos = Screen.height - Plugin._OxygenUIPosY.Value; // Y from bottom
            float width = Plugin._OxygenUIWidth.Value;
            float height = Plugin._OxygenUIHeight.Value;

            // Porcentagem atual do oxigênio
            float fillAmount = player.Physical.Oxygen.NormalValue;
            float currentWidth = width * fillAmount;

            // Fundo escuro semitransparente (opcional, para dar contraste)
            Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.5f * _currentAlpha);
            _whiteTexture.SetPixel(0, 0, bgColor);
            _whiteTexture.Apply();
            _bgStyle.normal.background = _whiteTexture;
            GUI.Box(new Rect(xPos, yPos, width, height), GUIContent.none, _bgStyle);

            // Barra branca principal
            Color fgColor = new Color(1f, 1f, 1f, 1f * _currentAlpha);
            _whiteTexture.SetPixel(0, 0, fgColor);
            _whiteTexture.Apply();
            _fgStyle.normal.background = _whiteTexture;
            GUI.Box(new Rect(xPos, yPos, currentWidth, height), GUIContent.none, _fgStyle);
        }

        private void OnDestroy()
        {
            if (_whiteTexture != null)
                Destroy(_whiteTexture);
        }
    }
}
