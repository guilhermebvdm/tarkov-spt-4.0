using Fika.Core.Networking.LiteNetLib.Utils;
using EFT;
using Fika.Core.Networking;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    /// Aplicação de cura do médico no paciente.
    ///
    /// Corpo dentro de um envelope de comprimento (ver <see cref="PacketEnvelope"/>). O sufixo V2
    /// existe porque a hash de identificação do FIKA deriva do NOME DO TIPO, não da versão do mod:
    /// sem renomear, um peer antigo e um novo aceitariam o pacote um do outro e desalinhariam o
    /// leitor em silêncio. Com o nome novo, um peer desatualizado falha com
    /// `Undefined packet` — diagnosticável. Todos os peers e o headless sobem juntos (lockstep).
    /// </summary>
    public struct BandAidHealPacketV2 : INetSerializable
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

        /// <summary>
        /// NÃO serializado. Falso quando o corpo veio truncado e a leitura parou no meio.
        /// Sem isto um pacote parcial é indistinguível de um válido no callback, e o host chega a
        /// RETRANSMITIR o lixo re-serializado para todos os peers (o relay dispara porque o
        /// DoctorProfileId vazio não bate com o id do host).
        /// </summary>
        internal bool Valid;

        public void Serialize(NetDataWriter writer)
        {
            var inner = PacketEnvelope.Open();
            inner.Put(DoctorProfileId ?? string.Empty);
            inner.Put(PatientProfileId ?? string.Empty);
            inner.Put(ItemTemplateId ?? string.Empty);
            inner.Put((byte)BodyPart);
            inner.Put(HealAmount);
            inner.Put(IsSurgery);
            inner.Put(SurgeryPenalty);
            inner.Put(RemovedHeavyBleed);
            inner.Put(RemovedLightBleed);
            inner.Put(RemovedFracture);
            inner.Put(ApplyFullTreatment);
            PacketEnvelope.Close(writer, inner);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = string.Empty;
            PatientProfileId = string.Empty;
            ItemTemplateId = string.Empty;
            BodyPart = EBodyPart.Common;
            HealAmount = 0f;
            IsSurgery = false;
            SurgeryPenalty = 0f;
            RemovedHeavyBleed = false;
            RemovedLightBleed = false;
            RemovedFracture = false;
            ApplyFullTreatment = false;
            Valid = false;

            if (!PacketEnvelope.TryOpen(reader, out var inner)) return;

            // TryGet*: não lançam. Uma exceção aqui abortaria a fila de rede do frame inteiro.
            if (!PacketEnvelope.TryReadString(inner, out DoctorProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out PatientProfileId)) return;
            if (!PacketEnvelope.TryReadString(inner, out ItemTemplateId)) return;
            if (!inner.TryGetByte(out var bodyPart)) return;
            BodyPart = (EBodyPart)bodyPart;
            if (!inner.TryGetFloat(out HealAmount)) return;
            if (!inner.TryGetBool(out IsSurgery)) return;
            if (!inner.TryGetFloat(out SurgeryPenalty)) return;
            if (!inner.TryGetBool(out RemovedHeavyBleed)) return;
            if (!inner.TryGetBool(out RemovedLightBleed)) return;
            if (!inner.TryGetBool(out RemovedFracture)) return;
            if (!inner.TryGetBool(out ApplyFullTreatment)) return;

            Valid = true;
        }
    }
}
