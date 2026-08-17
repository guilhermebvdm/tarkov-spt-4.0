using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    /// Formatos LEGADOS (≤ 1.10.0), mantidos registrados **apenas para consumir o payload**.
    ///
    /// Não restauram funcionalidade: todos os fluxos do mod são handshakes bidirecionais, e um peer
    /// antigo não entende as respostas V2. O motivo de existirem é outro e mais grave: se o tipo
    /// simplesmente sumisse da tabela de callbacks, um pacote vindo de um peer 1.10.0 cairia no
    /// `throw new ParseException` de `NetPacketProcessor.GetCallbackFromData`. Essa exceção não é
    /// tratada em nenhum ponto do caminho — sobe por `OnNetworkReceive` até
    /// `LiteNetManager.PollEvents`, que desanexa a fila de eventos pendentes e a percorre SEM
    /// try/catch. O frame inteiro é descartado: todos os peers, todos os OUTROS mods e os eventos
    /// de posição/movimento do próprio FIKA.
    ///
    /// Ou seja: consumir o pacote antigo em silêncio custa a funcionalidade daquela interação;
    /// deixá-lo sem handler custa um engasgo de rede global. Estes stubs compram o primeiro.
    ///
    /// Podem ser removidos quando todos os peers e o headless estiverem em ≥ 1.11.0.
    /// </summary>
    internal static class LegacyPacketCompat
    {
        private static bool _warned;

        /// <summary>
        /// Registra os stubs na instância ativa. Chamado por
        /// <see cref="BandAidNetworkHandler.EnsurePacketsRegistered"/>.
        /// </summary>
        internal static void Register(IFikaNetworkManager manager)
        {
            manager.RegisterPacket<BandAidHealPacket>(_ => WarnOnce("BandAidHealPacket"));
            manager.RegisterPacket<BandAidShoulderTapPacket>(_ => WarnOnce("BandAidShoulderTapPacket"));
            manager.RegisterPacket<BandAidHealCheckPacket>(_ => WarnOnce("BandAidHealCheckPacket"));
            manager.RegisterPacket<BandAidHealCheckResponsePacket>(_ => WarnOnce("BandAidHealCheckResponsePacket"));
            manager.RegisterPacket<TraumaFaintPacket>(_ => WarnOnce("TraumaFaintPacket"));
            manager.RegisterPacket<BandAidTreatmentReportPacket>(_ => WarnOnce("BandAidTreatmentReportPacket"));
        }

        private static void WarnOnce(string packetName)
        {
            if (_warned) return;
            _warned = true;
            BandAidNetworkHandler.LogVersionMismatch(packetName);
        }
    }

    // Os structs abaixo replicam BYTE A BYTE o formato ≤1.10.0 (sem envelope), para que a leitura
    // consuma exatamente o que o peer antigo escreveu. Leem com as variantes TryGet, que não lançam.
    // Nenhum deles é enviado — Serialize existe só por contrato da interface.

    public struct BandAidHealPacket : INetSerializable
    {
        public void Serialize(NetDataWriter writer) { }

        public void Deserialize(NetDataReader reader)
        {
            if (!reader.TryGetString(out _)) return;   // DoctorProfileId
            if (!reader.TryGetString(out _)) return;   // PatientProfileId
            if (!reader.TryGetString(out _)) return;   // ItemTemplateId
            if (!reader.TryGetByte(out _)) return;     // BodyPart
            if (!reader.TryGetFloat(out _)) return;    // HealAmount
            if (!reader.TryGetBool(out _)) return;     // IsSurgery
            if (!reader.TryGetFloat(out _)) return;    // SurgeryPenalty
            if (!reader.TryGetBool(out _)) return;     // RemovedHeavyBleed
            if (!reader.TryGetBool(out _)) return;     // RemovedLightBleed
            if (!reader.TryGetBool(out _)) return;     // RemovedFracture
            if (!reader.TryGetBool(out _)) return;     // ApplyFullTreatment
        }
    }

    public struct BandAidShoulderTapPacket : INetSerializable
    {
        public void Serialize(NetDataWriter writer) { }

        public void Deserialize(NetDataReader reader)
        {
            if (!reader.TryGetString(out _)) return;   // SenderProfileId
            if (!reader.TryGetString(out _)) return;   // SenderNickname
            if (!reader.TryGetString(out _)) return;   // TargetProfileId
        }
    }

    public struct BandAidHealCheckPacket : INetSerializable
    {
        public void Serialize(NetDataWriter writer) { }

        public void Deserialize(NetDataReader reader)
        {
            if (!reader.TryGetString(out _)) return;   // DoctorProfileId
            if (!reader.TryGetString(out _)) return;   // PatientProfileId
            if (!reader.TryGetString(out _)) return;   // ItemTemplateId
        }
    }

    public struct BandAidHealCheckResponsePacket : INetSerializable
    {
        public void Serialize(NetDataWriter writer) { }

        public void Deserialize(NetDataReader reader)
        {
            if (!reader.TryGetString(out _)) return;   // DoctorProfileId
            if (!reader.TryGetString(out _)) return;   // PatientProfileId
            if (!reader.TryGetString(out _)) return;   // ItemTemplateId
            if (!reader.TryGetBool(out _)) return;     // Approved
            if (!reader.TryGetByte(out _)) return;     // DenyReasonId
            if (!reader.TryGetByte(out _)) return;     // ExpectedBodyPart
        }
    }

    public struct TraumaFaintPacket : INetSerializable
    {
        public void Serialize(NetDataWriter writer) { }

        public void Deserialize(NetDataReader reader)
        {
            if (!reader.TryGetString(out _)) return;   // ProfileId
            if (!reader.TryGetBool(out _)) return;     // IsFainted
            if (!reader.TryGetFloat(out _)) return;    // DurationSeconds
            if (!reader.TryGetFloat(out _)) return;    // GraceSeconds
        }
    }

    public struct BandAidTreatmentReportPacket : INetSerializable
    {
        public void Serialize(NetDataWriter writer) { }

        public void Deserialize(NetDataReader reader)
        {
            if (!reader.TryGetString(out _)) return;   // DoctorProfileId
            if (!reader.TryGetString(out _)) return;   // PatientProfileId
            if (!reader.TryGetString(out _)) return;   // ItemTemplateId
            if (!reader.TryGetByte(out _)) return;     // BodyPart
            if (!reader.TryGetFloat(out _)) return;    // HealedAmount
            if (!reader.TryGetFloat(out _)) return;    // CostAmount
        }
    }
}
