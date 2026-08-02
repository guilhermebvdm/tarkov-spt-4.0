using System;
using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;

namespace TarkovRedLine.PvpMode.Networking
{
    /// <summary>
    /// Avisa os outros clientes que este jogador renasceu em outro ponto, para que o corpo apareça
    /// lá **de corte seco** em vez de interpolar o trajeto inteiro.
    ///
    /// Segue o padrão canônico de [docs/technical/fika-packet-desync-prevention-plan.md] §5.1 (AP-11).
    /// As três regras que não são estilo:
    ///
    ///  1. **Envelope de comprimento.** Uma leitura que falha no meio desalinharia o leitor
    ///     compartilhado do FIKA — e no FIKA uma exceção no caminho do pacote derruba a fila de
    ///     eventos do quadro INTEIRO, para todos os pares e todos os mods (sintoma clássico: jogador
    ///     "patinando"). O envelope garante consumo do bloco inteiro, aconteça o que acontecer.
    ///  2. **Só `TryGet*`.** Os `Get*` simples lançam em corpo truncado.
    ///  3. **Reset de todos os campos na entrada do `Deserialize`.** A instância do pacote é
    ///     REUTILIZADA entre recepções; campo não escrito nesta leitura guardaria o valor da anterior.
    ///
    /// ⚠️ Mudar o layout deste pacote exige **renomear o tipo** (sufixo `V2`): a hash de identificação
    /// vem do NOME do tipo e ignora a versão do mod.
    /// </summary>
    public struct TrlRespawnSyncPacket : INetSerializable
    {
        public int NetId;
        public Vector3 Position;
        public float Yaw;

        /// <summary>NÃO serializado. Falso quando o corpo veio truncado — não processar nem retransmitir.</summary>
        internal bool Valid;

        [ThreadStatic] private static NetDataWriter _inner;

        public void Serialize(NetDataWriter writer)
        {
            var inner = _inner ??= new NetDataWriter(true, 64);
            inner.Reset();

            inner.Put(NetId);
            inner.Put(Position.x);
            inner.Put(Position.y);
            inner.Put(Position.z);
            inner.Put(Yaw);

            // Sempre o overload de 3 argumentos: o de 1 argumento escreve o buffer inteiro,
            // incluindo o padding além de Length (NetDataWriter.cs:381).
            // `checked`: estouro falha visível em vez de truncar o comprimento em silêncio.
            writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
        }

        public void Deserialize(NetDataReader reader)
        {
            NetId = 0;
            Position = Vector3.zero;
            Yaw = 0f;
            Valid = false;

            if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

            var inner = new NetDataReader(payload);

            if (!inner.TryGetInt(out NetId)) return;
            if (!inner.TryGetFloat(out var x)) return;
            if (!inner.TryGetFloat(out var y)) return;
            if (!inner.TryGetFloat(out var z)) return;
            if (!inner.TryGetFloat(out Yaw)) return;

            Position = new Vector3(x, y, z);
            Valid = true;
        }
    }
}
