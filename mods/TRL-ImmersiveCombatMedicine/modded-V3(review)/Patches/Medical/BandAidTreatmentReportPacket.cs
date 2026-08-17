using EFT;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    /// Report do PACIENTE → MÉDICO com o resultado real do tratamento remoto:
    /// qual membro foi escolhido pelo smart-target, quanto HP curou de fato e o
    /// CUSTO REAL de recurso (HP curado + custos por efeito removido, tabela do
    /// ItemDatabase). Alimenta o feedback visual do HUD do médico E o consumo
    /// autoritativo do item (ref: CR-05 — o médico debita o que o PACIENTE
    /// efetivamente aplicou, não uma estimativa da saúde observada).
    ///
    /// Corpo dentro de um envelope de comprimento (ver <see cref="PacketEnvelope"/>).
    /// </summary>
    public struct BandAidTreatmentReportPacketV2 : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;
        public byte BodyPart;      // EBodyPart
        public float HealedAmount;
        public float CostAmount;   // recurso a debitar do item do médico (CR-05)

        /// <summary>NÃO serializado. Falso quando o corpo veio truncado — ver BandAidHealPacketV2.Valid.</summary>
        internal bool Valid;

        public void Serialize(NetDataWriter writer)
        {
            var inner = PacketEnvelope.Open();
            inner.Put(DoctorProfileId ?? string.Empty);
            inner.Put(PatientProfileId ?? string.Empty);
            inner.Put(ItemTemplateId ?? string.Empty);
            inner.Put(BodyPart);
            inner.Put(HealedAmount);
            inner.Put(CostAmount);
            PacketEnvelope.Close(writer, inner);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = string.Empty;
            PatientProfileId = string.Empty;
            ItemTemplateId = string.Empty;
            BodyPart = 0;
            HealedAmount = 0f;
            CostAmount = 0f;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;

            if (!PacketEnvelope.TryReadString(inner, out DoctorProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out PatientProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out ItemTemplateId)) return;
            if (!inner.TryGetByte(out BodyPart)) return;
            if (!inner.TryGetFloat(out HealedAmount)) return;
            if (!inner.TryGetFloat(out CostAmount)) return;

            Valid = true;
        }
    }
}
