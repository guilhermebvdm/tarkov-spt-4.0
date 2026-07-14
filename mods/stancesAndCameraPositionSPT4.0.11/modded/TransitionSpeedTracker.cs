namespace CameraRotationMod
{
    /// <summary>
    /// Decide QUAL velocidade de transição usar na mola: a de troca de postura ou a de mira (ADS).
    ///
    /// Por que é preciso rastrear: a mola é uma só — ela interpola a arma até o alvo, e o alvo muda tanto ao
    /// trocar de postura quanto ao mirar. A mola não sabe o "porquê" do movimento. Então olhamos o que mudou:
    /// se foi o `isAiming`, a transição em curso é de MIRA; se foi a stance, é de POSTURA. O modo escolhido
    /// vale até a próxima mudança — é isso que faz a velocidade de mira valer também na SAÍDA da mira, não só
    /// na entrada.
    ///
    /// Uma instância por "corpo animado": o jogador local e CADA jogador observado no Fika têm a sua, porque
    /// cada um está numa transição diferente ao mesmo tempo.
    /// </summary>
    public class TransitionSpeedTracker
    {
        private bool _lastAiming;
        private int _lastStance;
        private bool _initialized;
        private bool _inAdsTransition;

        /// <summary>Multiplicador de velocidade para o frame atual, dado o estado do jogador.</summary>
        public float SpeedMult(bool isAiming, int stance)
        {
            if (!_initialized)
            {
                // Primeiro frame: sem transição anterior para comparar — assume postura (o neutro).
                _initialized = true;
            }
            else if (isAiming != _lastAiming)
            {
                _inAdsTransition = true;   // levantou ou baixou a mira
            }
            else if (stance != _lastStance)
            {
                _inAdsTransition = false;  // trocou de postura
            }
            // Se nada mudou, mantém o modo — a mola ainda está a caminho do alvo anterior.

            _lastAiming = isAiming;
            _lastStance = stance;

            return _inAdsTransition
                ? (Plugin._ADSTransitionSpeed?.Value ?? 1f)
                : (Plugin._StanceTransitionSpeed?.Value ?? 1f);
        }

        /// <summary>Troca de raid / recriação do player: esquece o estado, senão o 1º frame da raid nova
        /// compara com o da anterior e escolhe o modo errado.</summary>
        public void Reset()
        {
            _initialized = false;
            _inAdsTransition = false;
            _lastAiming = false;
            _lastStance = 0;
        }
    }
}
