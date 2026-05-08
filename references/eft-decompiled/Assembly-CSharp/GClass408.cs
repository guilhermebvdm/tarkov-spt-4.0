using UnityEngine;

public class GClass408
{
	public float LastUpdate;

	public ActorDataStruct Data;

	public void SetData(ActorDataStruct dataBot)
	{
		LastUpdate = Time.time;
		Data = dataBot;
	}
}
