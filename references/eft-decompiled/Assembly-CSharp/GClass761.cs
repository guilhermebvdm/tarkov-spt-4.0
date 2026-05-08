using Comfort.Common;
using UnityEngine;

public class GClass761
{
	public int ServerFrameIndex;

	public Vector3 Position;

	public Vector2 Rotation;

	public Vector3 LocalVelocity;

	public float PoseLevel;

	public float Speed;

	public GClass766 StateInfo;

	public static GClass761 Lerp(GClass761 from, GClass761 to, float t)
	{
		t = Mathf.Clamp01(t);
		return new GClass761
		{
			Position = Vector3.Lerp(from.Position, to.Position, t),
			Rotation = new Vector2(Mathf.LerpAngle(from.Rotation.x, to.Rotation.x, t), Mathf.LerpAngle(from.Rotation.y, to.Rotation.y, t)),
			LocalVelocity = Vector3.Lerp(from.LocalVelocity, to.LocalVelocity, t),
			StateInfo = ((t < 1f) ? from.StateInfo.Clone() : to.StateInfo.Clone())
		};
	}

	public override string ToString()
	{
		return string.Format("position: {0}, rotation: {1}, local vel: {2}, pose: {3}, speed: {4}, state: {5}", Position.ToString("F5"), Rotation.ToString("F3"), LocalVelocity.ToString("F5"), PoseLevel, Speed, StateInfo.StateName);
	}

	public virtual int WriteToBuffer(byte[] buffer, int offset = 0)
	{
		int num = offset;
		offset += buffer.Write(ServerFrameIndex, offset);
		offset += GClass2294.Write(buffer, Position, offset);
		offset += GClass2294.Write(buffer, Rotation, offset);
		offset += GClass2294.Write(buffer, LocalVelocity, offset);
		offset += buffer.Write(PoseLevel, offset);
		offset += buffer.Write(Speed, offset);
		offset += GClass767.Write(buffer, StateInfo, offset);
		return offset - num;
	}

	public virtual void ReadFromBuffer(byte[] buffer, ref int offset)
	{
		ServerFrameIndex = buffer.ReadInt32(ref offset);
		Position = GClass2294.ReadVector3(buffer, ref offset);
		Rotation = GClass2294.ReadVector2(buffer, ref offset);
		LocalVelocity = GClass2294.ReadVector3(buffer, ref offset);
		PoseLevel = buffer.ReadSingle(ref offset);
		Speed = buffer.ReadSingle(ref offset);
		StateInfo = GClass767.ReadPlayerStateInfo(buffer, ref offset);
	}
}
