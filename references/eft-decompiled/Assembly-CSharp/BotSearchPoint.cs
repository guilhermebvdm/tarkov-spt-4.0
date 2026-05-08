using UnityEngine;

public class BotSearchPoint
{
	public Vector3 Position;

	public EBotSearchPoint TypeSearch;

	public BotSearchPoint(Vector3 position, EBotSearchPoint typeSearch)
	{
		Position = position;
		TypeSearch = typeSearch;
	}

	public override string ToString()
	{
		return $"Position:{Position}   TypeSearch:{TypeSearch.ToString()}";
	}

	public virtual void Come()
	{
	}
}
