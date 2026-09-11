using EFT;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    /// Report do PACIENTE → MÉDICO com o resultado real do tratamento remoto:
    /// qual membro foi escolhido pelo smart-target, quanto HP curou de fato, o
    /// CUSTO REAL de recurso (HP curado + custos por efeito removido, tabela do
    /// ItemDatabase) e o XP de cura a creditar ao médico (022 — HealExperience por
    /// efeito + ExpForHeal por HP). Alimenta o feedback visual do HUD do médico, o
    /// consumo autoritativo do item (ref: CR-05 — o médico debita o que o PACIENTE
    /// efetivamente aplicou, não uma estimativa da saúde observada) e o crédito de XP.
    ///
    /// Corpo dentro de um envelope de comprimento (ver <see cref="PacketEnvelope"/>).
    ///
    /// V2 → V3 (022): layout mudou (campo XpAwarded novo) — tipo renomeado porque a
    /// hash de roteamento do FIKA deriva do NOME do tipo, não da versão do mod
    /// (ref: docs/technical/fika-packet-desync-prevention-plan.md:50). Mudança lockstep —
    /// host e clients atualizam juntos. O formato V2 agora é um stub de descarte em
    /// LegacyPackets.cs (mesmo padrão dos formatos ≤1.10.0 já registrados lá).
    /// </summary>
    public struct BandAidTreatmentReportPacketV3 : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;
        public byte BodyPart;      // EBodyPart
        public float HealedAmount;
        public float CostAmount;   // recurso a debitar do item do médico (CR-05)
        public int XpAwarded;      // 022 — XP de cura a creditar ao médico (efeito + HP)

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
            inner.Put(XpAwarded);
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
            XpAwarded = 0;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;

            if (!PacketEnvelope.TryReadString(inner, out DoctorProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out PatientProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out ItemTemplateId)) return;
            if (!inner.TryGetByte(out BodyPart)) return;
            if (!inner.TryGetFloat(out HealedAmount)) return;
            if (!inner.TryGetFloat(out CostAmount)) return;
            if (!inner.TryGetInt(out XpAwarded)) return;

            Valid = true;
        }
    }
}
