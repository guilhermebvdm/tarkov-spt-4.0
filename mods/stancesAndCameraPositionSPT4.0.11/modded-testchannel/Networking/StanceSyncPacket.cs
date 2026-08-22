using System;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace CameraRotationMod.Networking
{
    /// <summary>
    /// Formato LEGADO (≤ 2.10.0), mantido apenas para RECEPÇÃO enquanto convivem versões.
    ///
    /// O identificador de pacote do FIKA é um CRC-16 de `typeof(T).ToString()`
    /// (NetPacketProcessor.GetShortHash): ele deriva do NOME DO TIPO, não da versão do mod. Se o
    /// layout mudasse mantendo o nome, peers antigo e novo continuariam aceitando os pacotes um do
    /// outro e desalinhariam o NetDataReader silenciosamente. Manter este tipo registrado permite
    /// que um cliente novo continue entendendo um peer ainda não atualizado.
    ///
    /// ATENÇÃO — a compatibilidade é de MÃO ÚNICA: 2.11.0 entende 2.10.0, mas um peer 2.10.0 que
    /// receba um V2 não tem handler para esse hash e o NetPacketProcessor lança ParseException,
    /// que sobe até o PollEvents (sem try/catch) e descarta a fila de eventos daquele frame.
    /// Portanto 2.11.0 é release LOCKSTEP: todos os peers e o headless precisam subir juntos.
    /// A alternativa (manter o nome e mudar o layout) seria pior — os dois lados aceitariam o
    /// pacote um do outro e desalinhariam em silêncio, sem erro diagnosticável.
    ///
    /// O envio usa sempre <see cref="StanceSyncPacketV2"/>.
    /// </summary>
    public struct StanceSyncPacket : INetSerializable
    {
        public string ProfileId;
        public int Stance;
        public bool IsAiming;

        public void Serialize(NetDataWriter writer)
        {
            // Simétrico ao formato legado por contrato da interface; o mod nunca envia V1.
            writer.Put(ProfileId ?? string.Empty);
            writer.Put(Stance);
            writer.Put(IsAiming);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            Stance = 0;
            IsAiming = false;

            // Variantes TryGet: não lançam. Uma exceção aqui subiria pelo ReadAllPackets do
            // LiteNetLib e abortaria o lote de rede daquele frame para todos os mods.
            if (!reader.TryGetString(out ProfileId)) return;
            if (!reader.TryGetInt(out Stance)) return;
            if (!reader.TryGetBool(out IsAiming)) return;
        }
    }

    /// <summary>
    /// Formato atual (≥ 2.11.0). O corpo vai dentro de um ENVELOPE de comprimento
    /// (PutBytesWithLength → prefixo ushort).
    ///
    /// Motivo: o NetPacketProcessor do FIKA lê um datagrama inteiro em laço
    /// (`while (reader.AvailableBytes > 0) ReadPacket(reader)`). Um Deserialize que consuma um
    /// número de bytes diferente do que o Serialize escreveu desalinha o NetDataReader e faz a
    /// iteração seguinte interpretar 2 bytes de lixo como hash de pacote — produzindo
    /// `ParseException: Undefined packet in NetDataReader: <lixo>` e derrubando o lote inteiro do
    /// frame, inclusive os pacotes de posição/movimento do FIKA e dos demais mods.
    /// </summary>
    public struct StanceSyncPacketV2 : INetSerializable
    {
        public string ProfileId;
        public int Stance;
        public bool IsAiming;

        // Buffer por thread: SendStance roda na main thread, mas [ThreadStatic] mantém a garantia
        // caso algum caller futuro serialize de outra thread.
        [ThreadStatic] private static NetDataWriter _innerWriter;

        public void Serialize(NetDataWriter writer)
        {
            var inner = _innerWriter ??= new NetDataWriter(true, 128);
            inner.Reset();

            inner.Put(ProfileId ?? string.Empty);
            inner.Put(Stance);
            inner.Put(IsAiming);

            // Sobrecarga com offset/length: evita a alocação do CopyData(). `checked` faz um campo
            // grande demais estourar de forma visível em vez de truncar o comprimento em silêncio
            // (truncar produziria desalinhamento no receptor).
            writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            Stance = 0;
            IsAiming = false;

            // Consome o envelope inteiro quando há dados suficientes; devolve false sem lançar
            // quando não há — o stream principal nunca desalinha.
            if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

            try
            {
                var inner = new NetDataReader(payload);
                if (!inner.TryGetString(out ProfileId)) return;
                if (!inner.TryGetInt(out Stance)) return;
                if (!inner.TryGetBool(out IsAiming)) return;
            }
            catch (Exception ex)
            {
                // Falha contida no envelope: o reader externo já avançou o tamanho declarado.
                FikaSyncManager.LogErrorThrottled("StanceSyncPacketV2.Deserialize", ex);
            }
        }
    }
}
