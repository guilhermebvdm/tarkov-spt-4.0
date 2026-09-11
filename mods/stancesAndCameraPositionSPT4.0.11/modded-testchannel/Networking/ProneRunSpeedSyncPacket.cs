using System;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace CameraRotationMod.Networking
{
    /// <summary>
    /// Item 018 — sincroniza o multiplicador de <c>Animator.speed</c> (playback do Mecanim) que o
    /// prone-run aplica LOCALMENTE via <c>MovementContext.PlayerAnimatorSetSprintToIdleSpeed</c>
    /// (ref: Assembly-CSharp/EFT/MovementContext.cs:3837-3840). Investigação confirmou que essa
    /// escrita é 100% client-local — nem o pacote nativo de movimento (<c>MovementInfoPacketStruct</c>)
    /// nem o pacote compacto do Fika (<c>PlayerStateData</c>, que ainda por cima empacota
    /// <c>SmoothedCharacterMovementSpeed</c> num <c>ushort</c> 0..1 via <c>PackFloatToUShort</c> —
    /// qualquer boost acima de 1.0 já sai cortado antes de sair pela rede) carregam esse valor. Sem
    /// este pacote, outros jogadores veem a POSIÇÃO do jogador que está usando prone-run avançar no
    /// ritmo boostado (posição já é replicada corretamente), mas as pernas animando no ritmo normal —
    /// um descompasso visual tipo "patinação".
    ///
    /// Mesmo formato de envelope do <see cref="StanceSyncPacketV2"/> (PutBytesWithLength) pelo mesmo
    /// motivo: o NetPacketProcessor do Fika lê o datagrama inteiro em laço, e um Deserialize que
    /// consuma um número de bytes diferente do Serialize desalinha o NetDataReader pro resto do lote
    /// (inclusive pacotes de outros mods e de movimento do próprio Fika).
    /// </summary>
    public struct ProneRunSpeedSyncPacket : INetSerializable
    {
        public string ProfileId;
        public float AnimatorSpeed; // multiplicador atual (1f = ritmo normal)

        [ThreadStatic] private static NetDataWriter _innerWriter;

        public void Serialize(NetDataWriter writer)
        {
            var inner = _innerWriter ??= new NetDataWriter(true, 32);
            inner.Reset();

            inner.Put(ProfileId ?? string.Empty);
            inner.Put(AnimatorSpeed);

            writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            AnimatorSpeed = 1f;

            if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

            try
            {
                var inner = new NetDataReader(payload);
                if (!inner.TryGetString(out ProfileId)) return;
                if (!inner.TryGetFloat(out AnimatorSpeed)) return;
            }
            catch (Exception ex)
            {
                FikaSyncManager.LogErrorThrottled("ProneRunSpeedSyncPacket.Deserialize", ex);
            }
        }
    }
}
