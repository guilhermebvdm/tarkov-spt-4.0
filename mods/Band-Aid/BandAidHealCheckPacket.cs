using LiteNetLib.Utils;
using Fika.Core.Networking;

namespace Band_Aid
{
    /// <summary>
    /// Handshake Step 1: Médico pergunta ao paciente se pode usar item X.
    /// Médico → (rede) → Paciente
    /// </summary>
    public struct BandAidHealCheckPacket : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(DoctorProfileId);
            writer.Put(PatientProfileId);
            writer.Put(ItemTemplateId);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = reader.GetString();
            PatientProfileId = reader.GetString();
            ItemTemplateId = reader.GetString();
        }
    }

    /// <summary>
    /// Handshake Step 2: Paciente responde se o item pode ser usado.
    /// Paciente → (rede) → Médico
    /// </summary>
    public struct BandAidHealCheckResponsePacket : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;
        public bool Approved;
        public string DenyReason; // Motivo da recusa (ex: "Sem sangramento")

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(DoctorProfileId);
            writer.Put(PatientProfileId);
            writer.Put(ItemTemplateId);
            writer.Put(Approved);
            writer.Put(DenyReason ?? "");
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = reader.GetString();
            PatientProfileId = reader.GetString();
            ItemTemplateId = reader.GetString();
            Approved = reader.GetBool();
            DenyReason = reader.GetString();
        }
    }
}
