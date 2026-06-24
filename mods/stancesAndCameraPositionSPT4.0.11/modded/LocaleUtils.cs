using System.Collections.Generic;

namespace CameraRotationMod
{
    public static class LocaleUtils
    {
        public static string CurrentLanguage = "English";

        // Simple Dictionary holding English to Portuguese translations
        private static readonly Dictionary<string, string> _translations = new Dictionary<string, string>
        {
            { "Enable Camera Rotation", "Habilitar Rotação da Câmera" },
            { "Toggle key", "Tecla para Alternar" },
            { "Action Stance key", "Tecla da Stance de Ação" },
            { "Return to Stance 0", "Retornar à Stance 0" },
            { "Base Pitch", "Inclinação (Pitch) Base" },
            { "Sprint Pitch", "Inclinação (Pitch) Correndo" },
            { "Prone Pitch", "Inclinação (Pitch) Deitado" },
            { "Transition Speed", "Velocidade de Transição" },
            { "Advanced ADS Transitions", "Transições Avançadas de Mira (ADS)" },
            { "ADS Translation Z", "Avanço da Arma (Eixo Z)" },
            { "Hipfire FOV Scale", "Escala de FOV s/ Mirar" },
            { "ADS FOV Scale", "Escala de FOV Mirando" },
            { "FOV Transition Speed", "Velocidade de Transição do FOV" },
            { "Left Hand Target Override", "Substituição do Alvo da Mão Esquerda" },
            { "Action Stance (Item 008)", "Action Stance (Animação)" },
            { "Stance Animations (Cinematic Curves)", "Animações de Stance (Curvas Cinemáticas)" },
            { "Animation Curve Duration", "Duração da Curva de Animação" },
            { "Pitch Multiplier (Cano sobe/desce)", "Multiplicador de Pitch (Cano)" },
            { "Yaw Multiplier (Luneta tomba)", "Multiplicador de Yaw (Luneta)" },
            { "Roll Multiplier (Apontar Esq/Dir)", "Multiplicador de Roll (Giro Lateral)" },
            { "Position Multiplier (Coronha no peito)", "Multiplicador de Posição (Impacto da Coronha)" },
            { "Manual Chambering", "Ação de Câmara Manual" },
            { "Enable Manual Chambering", "Habilitar Ação Manual da Câmara" },
            { "Base Yaw", "Giro Lateral (Yaw) Base" },
            { "Base Roll", "Giro Diagonal (Roll) Base" },
            { "Camera Bobbing Multiplier", "Multiplicador do Balanço de Câmera" }
        };

        /// <summary>
        /// Localizes the English string to Portuguese if the current language is Portuguese.
        /// </summary>
        public static string Localize(string englishText)
        {
            if (CurrentLanguage == "Português" && _translations.TryGetValue(englishText, out string translated))
            {
                return translated;
            }
            return englishText;
        }

        public static void SetLanguageFromEFTLocale(string eftLocale)
        {
            if (eftLocale.ToLower().Contains("pt"))
            {
                CurrentLanguage = "Português";
            }
            else
            {
                CurrentLanguage = "English";
            }
        }
    }
}
