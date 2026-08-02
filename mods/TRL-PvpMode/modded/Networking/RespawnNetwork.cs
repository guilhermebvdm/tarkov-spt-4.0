using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Players;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using UnityEngine;

namespace TarkovRedLine.PvpMode.Networking
{
    /// <summary>
    /// Registro e tráfego do aviso de renascimento.
    ///
    /// Padrão obrigatório do repo (AP-11, guia de rede §4):
    ///  - registro rastreado por **instância** do gerenciador, nunca por um `bool` — o FIKA destrói e
    ///    recria o gerenciador a cada troca de sessão, e o novo já nasce sem os nossos registros;
    ///  - `EnsurePacketsRegistered()` chamado no `Update` do plugin **e** antes de todo envio;
    ///  - **nunca** chamar `UnregisterPacket`; fora de raid se resolve com guarda no callback;
    ///  - airbag (`try/catch`) na raiz do callback: exceção que escapa daqui derruba a fila de eventos
    ///    do quadro para **todos os pares e todos os mods**, não só para este mod.
    /// </summary>
    internal static class RespawnNetwork
    {
        /// <summary>Por quanto tempo defendemos a posição cravada contra estados atrasados.</summary>
        private const float SNAP_GUARD_SECONDS = 1.5f;

        /// <summary>Distância a partir da qual um estado é considerado "pré-teleporte" e rejeitado.</summary>
        private const float SNAP_GUARD_TOLERANCE_SQR = 400f;   // 20 m

        /// <summary>Além disto não é mapa nenhum — payload lixo (2 km da origem).</summary>
        private const float SANITY_MAX_SQR = 4_000_000f;

        private static IFikaNetworkManager _lastRegisteredManager;
        private static IFikaNetworkManager _lastFailedManager;
        private static int _failCount;

        private readonly struct SnapGuard
        {
            public readonly Vector3 Position;
            public readonly float Deadline;
            public SnapGuard(Vector3 position, float deadline) { Position = position; Deadline = deadline; }
        }

        private static readonly Dictionary<int, SnapGuard> _guards = new();
        private static readonly List<int> _expiredBuffer = new();

        public static void Reset()
        {
            _guards.Clear();
            _expiredBuffer.Clear();
            _failCount = 0;
            _lastFailedManager = null;
        }

        public static void EnsurePacketsRegistered()
        {
            if (!Singleton<IFikaNetworkManager>.Instantiated) return;

            var manager = Singleton<IFikaNetworkManager>.Instance;
            if (ReferenceEquals(_lastRegisteredManager, manager)) return;

            // Desistir depois de algumas tentativas na MESMA instância: sem isto, uma falha de
            // registro vira LogError a 60-144 Hz pela sessão inteira, e o próprio volume de log
            // causa engasgo (code review 003, E-04).
            if (ReferenceEquals(_lastFailedManager, manager) && _failCount >= 5) return;

            try
            {
                manager.RegisterPacket<TrlRespawnSyncPacket>(OnRespawnSyncReceived);
                _lastRegisteredManager = manager;
                _lastFailedManager = null;
                _failCount = 0;
                Plugin.Log.LogInfo("[TRL-PvpMode] Pacote de respawn registrado na instancia atual do FIKA.");
            }
            catch (Exception ex)
            {
                _lastFailedManager = manager;
                _failCount++;
                if (_failCount == 1)
                    Plugin.Log.LogError($"[TRL-PvpMode] Falha ao registrar o pacote de respawn: {ex}");
                else if (_failCount == 5)
                    Plugin.Log.LogError("[TRL-PvpMode] Registro do pacote falhou 5x — desistindo desta instancia.");
            }
        }

        /// <summary>
        /// Anuncia que o jogador local renasceu na posição dada.
        ///
        /// ⚠️ Precisa ser chamado **antes** do teleporte. O estado de posição do FIKA trafega em
        /// canal não-confiável e o aviso em canal confiável — não há ordem garantida entre canais
        /// diferentes. Avisando primeiro, o receptor já está defendendo a posição nova quando os
        /// primeiros estados pós-teleporte chegarem (code review 003, E-01).
        /// </summary>
        public static void BroadcastRespawn(FikaPlayer player, Vector3 position)
        {
            EnsurePacketsRegistered();
            if (!Singleton<IFikaNetworkManager>.Instantiated) return;

            try
            {
                var packet = new TrlRespawnSyncPacket
                {
                    NetId = player.NetId,
                    Position = position,
                };

                // Envio só a partir da main thread: o FIKA compartilha um único NetDataWriter sem
                // trava entre todos os envios. Este caminho vem de UpdateTick, então está na main.
                //
                // Nota: `broadcast: true` só tem efeito no caminho cliente; o host ignora o
                // parâmetro e sempre manda para todos (FikaServer.cs:606-613). O resultado é o
                // desejado nos dois casos, mas não é este argumento que faz isso acontecer.
                Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, broadcast: true);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"[TRL-PvpMode] Erro ao anunciar respawn: {ex.Message}");
            }
        }

        private static void OnRespawnSyncReceived(TrlRespawnSyncPacket packet)
        {
            try
            {
                // Guarda de contexto no lugar de UnregisterPacket.
                if (!Singleton<GameWorld>.Instantiated) return;

                // Corpo truncado: não aplicar. (O relay do host acontece em bytes crus ANTES do
                // Deserialize — FikaServer.cs:932-936 — e não pode ser impedido daqui.)
                if (!packet.Valid) return;

                // O envelope garante contagem de bytes, não valor plausível: um payload do tamanho
                // certo vindo de uma colisão de identificador ou de um par com build diferente
                // decodifica "válido" com NaN dentro. Escrever NaN num transform o deixa inválido
                // pelo resto da raid (E-02).
                if (!IsSane(packet.Position)) return;

                if (!Singleton<IFikaNetworkManager>.Instantiated) return;

                var observed = FindObserved(packet.NetId);

                // Jogador desconhecido (entrou depois, já saiu): ignorar em silêncio é o correto.
                if (observed == null) return;

                ApplySnap(observed, packet.Position);

                // Passa a defender esta posição por um tempo — ver TickGuards.
                _guards[packet.NetId] = new SnapGuard(packet.Position, Time.time + SNAP_GUARD_SECONDS);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] Excecao no callback de respawn: {ex}");
            }
        }

        /// <summary>
        /// Reafirma a posição cravada enquanto a janela estiver aberta.
        ///
        /// **Por que isto é necessário:** limpar o histórico de estados também apaga a única
        /// informação que o FIKA usa para rejeitar estado atrasado — ele só descarta um pacote
        /// fora de ordem se já houver algo no buffer. Depois do `Clear()`, o primeiro estado a
        /// chegar é aceito sem questionamento, mesmo que tenha sido produzido **antes** do
        /// teleporte; ele forma par com o seguinte e o corpo volta a percorrer o trajeto inteiro —
        /// exatamente o defeito que este item existe para eliminar (E-01).
        ///
        /// Chamado do `Update` do plugin.
        /// </summary>
        public static void TickGuards()
        {
            if (_guards.Count == 0) return;

            try
            {
                var now = Time.time;
                _expiredBuffer.Clear();

                foreach (var entry in _guards)
                {
                    if (now >= entry.Value.Deadline)
                    {
                        _expiredBuffer.Add(entry.Key);
                        continue;
                    }

                    var observed = FindObserved(entry.Key);
                    if (observed == null)
                    {
                        _expiredBuffer.Add(entry.Key);
                        continue;
                    }

                    // Só reage a desvio grande: movimento normal do jogador recém-nascido é
                    // legítimo e não deve ser desfeito.
                    if ((observed.Position - entry.Value.Position).sqrMagnitude <= SNAP_GUARD_TOLERANCE_SQR)
                        continue;

                    ApplySnap(observed, entry.Value.Position);
                }

                for (var i = 0; i < _expiredBuffer.Count; i++)
                    _guards.Remove(_expiredBuffer[i]);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] TickGuards: {ex.Message}");
                _guards.Clear();
            }
        }

        private static bool IsSane(Vector3 v)
        {
            if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)) return false;
            if (float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z)) return false;
            return v.sqrMagnitude <= SANITY_MAX_SQR;
        }

        private static ObservedPlayer FindObserved(int netId)
        {
            var players = Singleton<IFikaNetworkManager>.Instance.ObservedPlayers;
            if (players == null) return null;

            // Laço simples em vez de LINQ: o callback roda no caminho de rede.
            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                if (candidate != null && candidate.NetId == netId) return candidate;
            }

            return null;
        }

        /// <summary>
        /// Corte seco na posição. Sem isto, o corpo interpola entre a posição antiga e a nova
        /// (ObservedPlayer.cs:906, Vector3.LerpUnclamped) — sem nenhuma detecção de teleporte — e
        /// atravessa o mapa em linha reta.
        ///
        /// Limpar o histórico e a barra de efeitos é o que o próprio FIKA faz no cenário
        /// equivalente de reconexão (FikaClient.Callbacks.cs:39-49) — inclusive a limpeza dos
        /// ícones de efeito, que de outra forma ficariam pendurados na plaquinha do peer mesmo
        /// depois de a cura ter removido os efeitos de verdade (E-03).
        /// </summary>
        private static void ApplySnap(ObservedPlayer observed, Vector3 position)
        {
            observed.Snapshotter?.Clear();
            FikaBridge.TryClearHealthBarEffects(observed.HealthBar);

            // `Player.Transform` é o mesmo transform que `Teleport` escreveria; chamar `Teleport`
            // num observado ainda traria reclassificação de ambiente e reset de queda por cima,
            // que não fazem sentido para um boneco que não simula localmente (E-08).
            observed.Transform.position = position;
        }
    }
}
