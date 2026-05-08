using EFT;
using UnityEngine;

public class GClass582
{
	public float Time;

	public IPlayer Player;

	public Vector3 Position;

	public GClass582(IPlayer player, Vector3 position)
	{
		Player = player;
		Position = position;
		Time = UnityEngine.Time.time;
	}

	public override string ToString()
	{
		Vector3 position = Position;
		return "P:" + position.ToString() + ",  T:" + Time;
	}
}
