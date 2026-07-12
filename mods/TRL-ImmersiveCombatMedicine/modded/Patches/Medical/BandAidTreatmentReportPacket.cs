using EFT;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace Band_Aid
{
    /// <summary>
    /// Report leve do PACIENTE → MÉDICO com o resultado real do tratamento remoto:
    /// qual membro foi escolhido pelo smart-target e quanto HP curou de fato.
    /// Alimenta o feedback visual do HUD do médico (destaque do membro). NÃO altera
    /// o fluxo de consumo (G-3/G-4 seguem deferidos — este pacote é só informativo).
    /// </summary>
    public struct BandAidTreatmentReportPacket : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;
        public byte BodyPart;      // EBodyPart
        public float HealedAmount;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(DoctorProfileId);
            writer.Put(PatientProfileId);
            writer.Put(ItemTemplateId);
            writer.Put(BodyPart);
            writer.Put(HealedAmount);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = reader.GetString();
            PatientProfileId = reader.GetString();
            ItemTemplateId = reader.GetString();
            BodyPart = reader.GetByte();
            HealedAmount = reader.GetFloat();
        }
    }
}
