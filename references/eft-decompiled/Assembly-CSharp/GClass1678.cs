using EFT;
using EFT.Game.Spawning;
using UnityEngine;

public class GClass1678
{
	public int Id;

	public string ProfileId;

	public string Name;

	public Vector3 Position;

	public Quaternion Rotation;

	public ESpawnCategory Category;

	public EPlayerSide Side;

	public string GroupId;

	public string Infiltration;

	public static explicit operator GClass1678(GamePerson person)
	{
		return new GClass1678
		{
			Id = person.Id,
			ProfileId = person.ProfileId,
			Name = person.name,
			Position = person.transform.position,
			Rotation = person.transform.rotation,
			Category = person.Category,
			Side = person.Side,
			GroupId = person.GroupId,
			Infiltration = person.Infiltration
		};
	}
}
