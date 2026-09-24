using EFT;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>
    /// Falas de dor por ENTRADA/AGRAVAMENTO de estado de trauma (item 019).
    ///
    /// Assina o barramento de transições do motor em vez de tocar os consumidores 003/004/005/006 — que já
    /// estão entregues e ainda não validados in-game. Isolar assim significa que o toggle F12 desliga a
    /// feature inteira e que nada aqui pode regredir o comportamento existente.
    ///
    /// Por que na TRANSIÇÃO e não no hit: o gatilho de dano roda por pellet (uma rajada de espingarda que
    /// destrói uma perna dispara várias vezes), e o motor já consolida isso numa única transição de linha por
    /// frame. O mod antigo falava no hit e dependia de um anti-spam de 2s para não metralhar a fala.
    ///
    /// Mapa evento → fala, todo ancorado no que o jogo faz:
    ///
    ///   membro FRATURADO   → LegBroken / HandBroken   (idêntico a BotMemoryClass.cs:640-654)
    ///   membro ZERADO      → OnAgony                  (o jogo não tem gatilho de "membro destruído")
    ///   estômago zerado    → OnAgony                  (idem; `OnPain` do mod antigo NÃO EXISTE no enum)
    ///
    /// Gate de analgésico é o idioma do próprio jogo: Player.cs:31241 só grita no pouso com perna ruim
    /// `if (flag && !OnPainkillers && !IsAI)`, e Player.cs:30302 gateia o OnBeingHurt de hit do mesmo jeito.
    /// </summary>
    internal static class TraumaPainVoice
    {
        private static bool _subscribed;

        internal static void Subscribe()
        {
            if (_subscribed) return;
            _subscribed = true;
            // StateChanged puro, NÃO SubscribeWithSnapshot: o replay de estado existente serviria falas no
            // primeiro frame de quem entrou na raid já ferido — e o critério de spawn ferido (S1b.3) é
            // explícito em não ter voz. O guard de Establishing abaixo cobre o mesmo caso pelo outro lado.
            TraumaEngine.StateChanged += OnTransition;
        }

        private static void OnTransition(TraumaTransition t)
        {
            // ref: review 019 CR-01-01 — try/catch OBRIGATÓRIO aqui, e não é formalidade: o motor publica com
            // `StateChanged?.Invoke(t)` (TraumaEngine.cs:317,570). Um evento C# invoca os handlers em cadeia, e
            // uma exceção em QUALQUER handler aborta a cadeia e propaga para dentro do motor — derrubando a
            // consolidação/reconciliação do frame e, com ela, os consumidores de gameplay. Uma falha de ÁUDIO
            // nunca pode custar o efeito de trauma.
            try { EmitPainVoice(t); }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] pain voice falhou: {ex.Message}");
            }
        }

        private static void EmitPainVoice(TraumaTransition t)
        {
            if (!IsActive()) return;

            // Reconhecimento de estado que já existia (spawn ferido, religar toggle) não é um ferimento novo:
            // sem fala, mesma regra do toast e dos one-shots (comportamento 5 da spec do motor).
            if (t.Establishing) return;

            // Só agravamento. A ordem numérica do enum de linhas É a ordem de severidade (D1), então
            // To > From cobre "entrou" e "piorou" de uma vez; cura (To < From) fica silenciosa.
            if (t.To <= t.From) return;

            // Analgésico cala a dor — idioma do jogo (Player.cs:31241 / :30302).
            if (t.PainkillerActive) return;

            Player p = t.Player;
            if (p is null || p.HealthController == null || !p.HealthController.IsAlive) return;

            // Fratura tem gatilho dedicado no jogo; membro zerado não. Consultado no instante da fala porque a
            // transição carrega a linha, não a natureza do ferimento — e a mesma linha pode vir das duas vias
            // (D4: um membro zerado E quebrado conta nas duas contagens).
            if (t.Region == TraumaRegion.Legs)
            {
                if (IsBroken(p, EBodyPart.LeftLeg) || IsBroken(p, EBodyPart.RightLeg))
                    TraumaVoice.PlayFracture(p, TraumaRegion.Legs);
                else
                    TraumaVoice.PlayZeroed(p);
                return;
            }

            if (t.Region == TraumaRegion.Arms)
            {
                if (IsBroken(p, EBodyPart.LeftArm) || IsBroken(p, EBodyPart.RightArm))
                    TraumaVoice.PlayFracture(p, TraumaRegion.Arms);
                else
                    TraumaVoice.PlayZeroed(p);
                return;
            }

            // Estômago: sem gatilho dedicado no jogo. A voz "Gut" do mod antigo apontava para `OnPain`, que
            // não existe no enum — nunca tocou uma única vez.
            TraumaVoice.PlayZeroed(p);
        }

        private static bool IsBroken(Player p, EBodyPart part)
        {
            try { return p.HealthController.IsBodyPartBroken(part); }
            catch { return false; }
        }

        private static bool IsActive()
        {
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigPainVoice.Value;
        }
    }
}
