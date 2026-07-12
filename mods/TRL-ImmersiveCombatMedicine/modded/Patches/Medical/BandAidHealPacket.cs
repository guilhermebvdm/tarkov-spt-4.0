using Fika.Core.Networking.LiteNetLib.Utils;
using EFT;
using Fika.Core.Networking;

namespace Band_Aid
{
    public struct BandAidHealPacket : INetSerializable
    {
        public string DoctorProfileId;
        public string PatientProfileId;
        public string ItemTemplateId;
        public EBodyPart BodyPart;
        public float HealAmount;
        public bool IsSurgery;
        public float SurgeryPenalty;
        // Flags de efeitos tratados
        public bool RemovedHeavyBleed;
        public bool RemovedLightBleed;
        public bool RemovedFracture;
        // Flag: paciente deve aplicar tratamento completo em si mesmo
        public bool ApplyFullTreatment;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(DoctorProfileId);
            writer.Put(PatientProfileId);
            writer.Put(ItemTemplateId);
            writer.Put((byte)BodyPart);
            writer.Put(HealAmount);
            writer.Put(IsSurgery);
            writer.Put(SurgeryPenalty);
            writer.Put(RemovedHeavyBleed);
            writer.Put(RemovedLightBleed);
            writer.Put(RemovedFracture);
            writer.Put(ApplyFullTreatment);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = reader.GetString();
            PatientProfileId = reader.GetString();
            ItemTemplateId = reader.GetString();
            BodyPart = (EBodyPart)reader.GetByte();
            HealAmount = reader.GetFloat();
            IsSurgery = reader.GetBool();
            SurgeryPenalty = reader.GetFloat();
            RemovedHeavyBleed = reader.GetBool();
            RemovedLightBleed = reader.GetBool();
            RemovedFracture = reader.GetBool();
            ApplyFullTreatment = reader.GetBool();
        }
    }
}
