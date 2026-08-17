using Fika.Core.Networking.LiteNetLib.Utils;
using Fika.Core.Networking;

namespace Band_Aid
{
    /// <summary>
    /// Handshake Step 1: Médico pergunta ao paciente se pode usar item X.
    /// Médico → (rede) → Paciente
    ///
    /// Corpo dentro de um envelope de comprimento (ver <see cref="PacketEnvelope"/>).
    /// </summary>
    public struct BandAidHealCheckPacketV2 : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;

        /// <summary>NÃO serializado. Falso quando o corpo veio truncado — ver BandAidHealPacketV2.Valid.</summary>
        internal bool Valid;

        public void Serialize(NetDataWriter writer)
        {
            var inner = PacketEnvelope.Open();
            inner.Put(DoctorProfileId ?? string.Empty);
            inner.Put(PatientProfileId ?? string.Empty);
            inner.Put(ItemTemplateId ?? string.Empty);
            PacketEnvelope.Close(writer, inner);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = string.Empty;
            PatientProfileId = string.Empty;
            ItemTemplateId = string.Empty;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;

            if (!PacketEnvelope.TryReadString(inner, out DoctorProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out PatientProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out ItemTemplateId)) return;

            Valid = true;
        }
    }

    /// <summary>
    /// Handshake Step 2: Paciente responde se o item pode ser usado.
    /// Paciente → (rede) → Médico
    ///
    /// Corpo dentro de um envelope de comprimento (ver <see cref="PacketEnvelope"/>).
    /// </summary>
    public struct BandAidHealCheckResponsePacketV2 : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;   // reusado por MedicLocale.GetDenyReasonText p/ resolver o
                                         // nome do item localmente no médico — nenhum campo NOVO
        public bool Approved;
        // ref: item 010 — DESVIO da spec técnica (stub 5): o campo precisou virar `internal` (não
        // `public` como escrito no stub) porque `MedicDenyReasonId` é `internal` (encapsulamento
        // intencional, mesmo padrão de MedicLocale) — um campo PÚBLICO de um struct PÚBLICO com tipo
        // INTERNO é CS0052 ("Inconsistent accessibility"), confirmado por build isolado (dotnet build,
        // repro mínimo). `internal` preserva o mesmo acesso de fato (Band_Aid/TRLImmersiveCombatMedicine
        // são a mesma assembly) sem expor o enum para fora do mod. Tradução
        // acontece no médico (exibidor), nunca serializada como texto.
        internal TRLImmersiveCombatMedicine.MedicDenyReasonId DenyReasonId;
        // Membro que o smart-target do paciente PRETENDE tratar (mesma lógica da
        // aplicação) — permite ao médico ver o alvo ANTES da animação começar.
        public byte ExpectedBodyPart;

        /// <summary>NÃO serializado. Falso quando o corpo veio truncado — ver BandAidHealPacketV2.Valid.</summary>
        internal bool Valid;

        public void Serialize(NetDataWriter writer)
        {
            var inner = PacketEnvelope.Open();
            inner.Put(DoctorProfileId ?? string.Empty);
            inner.Put(PatientProfileId ?? string.Empty);
            inner.Put(ItemTemplateId ?? string.Empty);
            inner.Put(Approved);
            inner.Put((byte)DenyReasonId);
            inner.Put(ExpectedBodyPart);
            PacketEnvelope.Close(writer, inner);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = string.Empty;
            PatientProfileId = string.Empty;
            ItemTemplateId = string.Empty;
            Approved = false;
            DenyReasonId = default;
            ExpectedBodyPart = 0;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;

            if (!PacketEnvelope.TryReadString(inner, out DoctorProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out PatientProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out ItemTemplateId)) return;
            if (!inner.TryGetBool(out Approved)) return;
            if (!inner.TryGetByte(out var denyReason)) return;
            DenyReasonId = (TRLImmersiveCombatMedicine.MedicDenyReasonId)denyReason;
            if (!inner.TryGetByte(out ExpectedBodyPart)) return;

            Valid = true;
        }
    }
}
