using UnityEngine;

namespace CameraRotationMod
{
    /// <summary>
    /// Item 017 (F1) — "waypoint por Stance 0" ao mirar. Enquanto ATIVO (os primeiros `ADS Waypoint Time` ms
    /// após apertar mirar a partir de uma stance), o alvo da mola deve ir para Stance 0 (pose neutra), e — no
    /// jogador LOCAL — o ADS nativo do EFT é segurado por um gate de aim-speed (feito no
    /// ApplyComplexRotationPatch, que tem o PWA). Assim a arma assenta no neutro ANTES de subir para a mira,
    /// matando o "super loop" vertical do High/Low Ready → ADS.
    ///
    /// Este helper cuida SÓ do timer e da borda de entrada em ADS — uma instância por corpo (local + cada
    /// observado Fika). O gate de aim-speed é responsabilidade do caminho local (o observado sobe a arma pela
    /// animação de ADS nativa do modelo, não pela nossa câmera).
    ///
    /// NÃO toca CurrentStance: "voltar à stance ao sair do ADS" é automático (o estado nunca mudou).
    /// </summary>
    public class AdsWaypoint
    {
        private bool _active;
        private bool _wasAiming;
        private float _msRemaining;

        /// <summary>Enquanto true, o alvo da mola deve ser sobrescrito para Vector3.zero (Stance 0).</summary>
        public bool Active => _active;

        /// <summary>Chamar todo frame ANTES de calcular o alvo da mola. Detecta a borda de entrada em ADS
        /// internamente e decrementa o timer. Lê a config (enable + ms) da <paramref name="stance"/> de origem
        /// — item 017 F1: cada stance calibra o seu waypoint. Retorna <see cref="Active"/>.</summary>
        public bool Update(bool isAiming, bool isInStance, float dt, Stance stance)
        {
            bool resetOnAds = Plugin._ResetOnADS?.Value ?? false;

            // Borda de entrada em ADS estando em stance (e com ResetOnADS — sem ele a pose já continua em ADS,
            // então não há salto a suavizar). Config por-stance: enable + ms da stance de origem.
            if (isAiming && !_wasAiming && isInStance && resetOnAds
                && Plugin._stanceConfigs.TryGetValue(stance, out var cfg)
                && (cfg.AdsWaypoint?.Value ?? false))
            {
                _msRemaining = Mathf.Max(0f, cfg.AdsWaypointTime?.Value ?? 0);
                _active = _msRemaining > 0f;
            }
            _wasAiming = isAiming;

            // Soltou o ADS antes de expirar → desarma na hora (a pose volta para a stance).
            if (!isAiming) _active = false;

            if (_active)
            {
                _msRemaining -= dt * 1000f;
                if (_msRemaining <= 0f) _active = false;
            }
            return _active;
        }

        /// <summary>Troca de raid / recriação do corpo: esquece a borda e o timer.</summary>
        public void Reset()
        {
            _active = false;
            _wasAiming = false;
            _msRemaining = 0f;
        }
    }
}
