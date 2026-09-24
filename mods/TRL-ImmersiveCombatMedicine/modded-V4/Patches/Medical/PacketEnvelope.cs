using System;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    /// Helper de ENVELOPE de comprimento para os pacotes FIKA deste mod.
    ///
    /// Porquê: o <c>NetPacketProcessor</c> do FIKA lê cada datagrama em laço
    /// (<c>while (reader.AvailableBytes > 0) ReadPacket(reader)</c>) e identifica o pacote por uma
    /// hash de 2 bytes. Um <c>Deserialize</c> que consuma um número de bytes diferente do que o
    /// <c>Serialize</c> escreveu deixa o <c>NetDataReader</c> desalinhado, e a iteração seguinte
    /// interpreta 2 bytes de lixo como hash → <c>ParseException: Undefined packet in
    /// NetDataReader</c>. Essa exceção não é tratada em lugar nenhum do caminho: sobe por
    /// <c>OnNetworkReceive</c> até <c>LiteNetManager.PollEvents</c>, que drena a fila de eventos
    /// pendentes SEM try/catch — então o frame inteiro é descartado, para todos os peers, todos os
    /// mods e também os eventos de posição/movimento do próprio FIKA.
    ///
    /// Com o envelope, o leitor externo avança sempre exatamente o tamanho declarado, mesmo que a
    /// leitura interna falhe (payload truncado ou peer com layout divergente).
    /// </summary>
    internal static class PacketEnvelope
    {
        // Buffer por thread: hoje todo Serialize roda na main thread, mas [ThreadStatic] mantém a
        // garantia caso algum caller futuro serialize de outra thread. Reutilizado entre pacotes
        // para não alocar por envio.
        [ThreadStatic] private static NetDataWriter _writer;

        /// <summary>Abre o corpo do pacote. Escreva os campos no writer devolvido e feche com <see cref="Close"/>.</summary>
        internal static NetDataWriter Open()
        {
            var inner = _writer ??= new NetDataWriter(true, 512);
            inner.Reset();
            return inner;
        }

        /// <summary>Grava o corpo no writer de saída, prefixado pelo comprimento.</summary>
        internal static void Close(NetDataWriter outer, NetDataWriter inner)
        {
            // `checked`: um corpo grande demais falha de forma visível em vez de truncar o
            // comprimento em silêncio — truncar produziria desalinhamento no receptor.
            outer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
        }

        /// <summary>
        /// Consome o envelope inteiro e devolve um leitor sobre o corpo. Retorna <c>false</c> sem
        /// lançar quando não há dados suficientes — lançar aqui abortaria a fila de rede do frame.
        /// </summary>
        internal static bool TryOpen(NetDataReader reader, out NetDataReader inner)
        {
            inner = null;
            if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return false;
            inner = new NetDataReader(payload);
            return true;
        }

        /// <summary>
        /// Lê uma string nunca devolvendo <c>null</c>.
        ///
        /// <c>NetDataReader.TryGetString</c> escreve <c>null</c> no parâmetro <c>out</c> nos dois
        /// caminhos de falha, então passar um campo direto como <c>out</c> DESTRÓI o default que o
        /// Deserialize tinha acabado de atribuir — o campo termina null em vez de string.Empty, e
        /// o null vaza para a lógica de negócio. Ler para local e atribuir só no sucesso evita isso.
        /// </summary>
        internal static bool TryReadString(NetDataReader reader, out string value)
        {
            if (!reader.TryGetString(out var read) || read == null)
            {
                value = string.Empty;
                return false;
            }

            value = read;
            return true;
        }
    }
}
