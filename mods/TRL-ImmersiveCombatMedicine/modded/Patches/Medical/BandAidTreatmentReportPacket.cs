using EFT;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace Band_Aid
{
    /// <summary>
    /// Report do PACIENTE → MÉDICO com o resultado real do tratamento remoto:
    /// qual membro foi escolhido pelo smart-target, quanto HP curou de fato e o
    /// CUSTO REAL de recurso (HP curado + custos por efeito removido, tabela do
    /// ItemDatabase). Alimenta o feedback visual do HUD do médico E o consumo
    /// autoritativo do item (ref: CR-05 — o médico debita o que o PACIENTE
    /// efetivamente aplicou, não uma estimativa da saúde observada).
    /// </summary>
    public struct BandAidTreatmentReportPacket : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;
        public byte BodyPart;      // EBodyPart
        public float HealedAmount;
        public float CostAmount;   // recurso a debitar do item do médico (CR-05)

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(DoctorProfileId);
            writer.Put(PatientProfileId);
            writer.Put(ItemTemplateId);
            writer.Put(BodyPart);
            writer.Put(HealedAmount);
            writer.Put(CostAmount);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = reader.GetString();
            PatientProfileId = reader.GetString();
            ItemTemplateId = reader.GetString();
            BodyPart = reader.GetByte();
            HealedAmount = reader.GetFloat();
            CostAmount = reader.GetFloat();
        }
    }
}
