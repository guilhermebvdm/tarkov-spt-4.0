using Fika.Core.Networking.LiteNetLib.Utils;
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

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(DoctorProfileId);
            writer.Put(PatientProfileId);
            writer.Put(ItemTemplateId);
            writer.Put(Approved);
            writer.Put((byte)DenyReasonId);
            writer.Put(ExpectedBodyPart);
        }

        public void Deserialize(NetDataReader reader)
        {
            DoctorProfileId = reader.GetString();
            PatientProfileId = reader.GetString();
            ItemTemplateId = reader.GetString();
            Approved = reader.GetBool();
            DenyReasonId = (TRLImmersiveCombatMedicine.MedicDenyReasonId)reader.GetByte();
            ExpectedBodyPart = reader.GetByte();
        }
    }
}
