using System;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRL_SpeakFromTarkov
{
    /// <summary>
    /// Formato LEGADO (≤ 1.3.0), mantido apenas para RECEPÇÃO durante a convivência de versões.
    ///
    /// O identificador de pacote do FIKA é um CRC-16 de `typeof(T).ToString()`
    /// (NetPacketProcessor.GetShortHash) — ele deriva do NOME DO TIPO, não da versão do mod.
    /// Por isso bumpar a versão não separa formatos: se o layout mudasse mantendo o nome, um peer
    /// antigo e um novo continuariam aceitando os pacotes um do outro e desalinhariam o
    /// NetDataReader silenciosamente. Manter este tipo registrado permite que um cliente novo
    /// continue OUVINDO um peer antigo enquanto o parque não está todo atualizado.
    ///
    /// ATENÇÃO — a compatibilidade é de MÃO ÚNICA: 1.4.0 ouve 1.3.0, mas um peer 1.3.0 que receba
    /// um V2 não tem handler para esse hash e o NetPacketProcessor lança ParseException, que sobe
    /// até o PollEvents (sem try/catch) e descarta a fila de eventos daquele frame. Portanto 1.4.0
    /// é release LOCKSTEP: todos os peers e o headless precisam subir juntos. A alternativa
    /// (manter o nome e mudar o layout) seria pior — os dois lados aceitariam o pacote um do outro
    /// e desalinhariam em silêncio, sem erro diagnosticável.
    ///
    /// O envio usa sempre <see cref="SftAudioPacketV2"/>. Este tipo pode ser removido quando todos
    /// os peers (incluindo o headless) estiverem em ≥ 1.4.0.
    /// </summary>
    public struct SftAudioPacket : INetSerializable
    {
        public string ProfileId;
        public byte Channel;
        public byte[] AudioData;
        public float VoiceLevel;

        public void Serialize(NetDataWriter writer)
        {
            // Mantido simétrico ao formato legado por contrato da interface; o mod nunca envia V1.
            writer.Put(ProfileId ?? string.Empty);
            writer.Put(Channel);
            writer.PutBytesWithLength(AudioData ?? Array.Empty<byte>());
            writer.Put(VoiceLevel);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            Channel = 0;
            AudioData = null!;
            VoiceLevel = 0f;

            // Sem envelope no formato legado: lemos campo a campo com as variantes TryGet, que não
            // lançam. Uma exceção aqui subiria pelo ReadAllPackets e abortaria o lote de rede.
            if (!reader.TryGetString(out ProfileId)) return;
            if (!reader.TryGetByte(out Channel)) return;
            if (!reader.TryGetBytesWithLength(out AudioData)) return;
            if (!reader.TryGetFloat(out VoiceLevel)) return;
        }
    }

    /// <summary>
    /// Formato atual (≥ 1.4.0). O corpo vai dentro de um ENVELOPE de comprimento
    /// (PutBytesWithLength → prefixo ushort).
    ///
    /// Motivo: o NetPacketProcessor do FIKA lê um datagrama inteiro em laço
    /// (`while (reader.AvailableBytes > 0) ReadPacket(reader)`), então um Deserialize que consuma
    /// um número de bytes diferente do que o Serialize escreveu desalinha o NetDataReader e faz a
    /// iteração seguinte interpretar 2 bytes de lixo como hash de pacote — produzindo
    /// `ParseException: Undefined packet in NetDataReader: <lixo>` e derrubando o lote inteiro do
    /// frame, inclusive os pacotes de posição/movimento do FIKA e dos demais mods.
    ///
    /// Com o envelope, o reader externo avança sempre exatamente o tamanho declarado, mesmo que a
    /// leitura interna falhe.
    /// </summary>
    public struct SftAudioPacketV2 : INetSerializable
    {
        /// <summary>Teto defensivo do payload de áudio. O encoder Opus local nunca passa de 1275
        /// bytes (VoipProcessor.opusBuffer); acima disso o pacote veio de peer bugado/hostil.</summary>
        internal const int MaxAudioBytes = 1500;

#pragma warning disable CS8618
        public string ProfileId;
        public byte Channel;
        public byte[]? AudioData;
        public float VoiceLevel;

        // Buffer por thread: hoje Serialize roda na main thread (drain da fila do SftNetwork),
        // mas [ThreadStatic] mantém a garantia se algum caller futuro serializar de outra thread.
        [ThreadStatic] private static NetDataWriter? _innerWriter;
#pragma warning restore CS8618

        public void Serialize(NetDataWriter writer)
        {
            var inner = _innerWriter ??= new NetDataWriter(true, 2048);
            inner.Reset();

            inner.Put(ProfileId ?? string.Empty);
            inner.Put(Channel);
            inner.PutBytesWithLength(AudioData ?? Array.Empty<byte>());
            inner.Put(VoiceLevel);

            // Sobrecarga com offset/length: escreve os mesmos bytes sem a cópia do CopyData().
            // O corpo é limitado por MaxAudioBytes + ProfileId, muito abaixo do teto ushort do
            // prefixo; `checked` faz um aumento futuro de bitrate estourar de forma visível em vez
            // de truncar o comprimento em silêncio (que produziria desalinhamento no receptor).
            writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            Channel = 0;
            AudioData = null;
            VoiceLevel = 0f;

            // TryGet: consome o envelope inteiro quando há dados suficientes e devolve false sem
            // lançar quando não há. Lançar aqui abortaria o lote de rede de todos os mods, porque
            // o Deserialize roda dentro do laço do NetPacketProcessor, antes do callback.
            if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

            try
            {
                var inner = new NetDataReader(payload);
                if (!inner.TryGetString(out ProfileId)) return;
                if (!inner.TryGetByte(out Channel)) return;
                if (!inner.TryGetBytesWithLength(out AudioData)) return;
                if (!inner.TryGetFloat(out VoiceLevel)) return;
            }
            catch (Exception ex)
            {
                // Falha contida no envelope: o stream principal já avançou o tamanho declarado.
                AudioData = null!;
                Network.SftNetwork.LogErrorThrottled("SftAudioPacketV2.Deserialize", ex);
            }
        }
    }
}
