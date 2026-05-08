using System;
using EFT;
using UnityEngine;

[Serializable]
public class KhorovodPosition
{
	[HideInInspector]
	public float angle;

	public BotOwner attachedOwner;

	public bool havePhrase;

	public Vector3 CalcPosition(float radius, float time)
	{
		return new Vector3(Mathf.Cos(angle + time), 0f, Mathf.Sin(angle + time)) * radius;
	}
}
