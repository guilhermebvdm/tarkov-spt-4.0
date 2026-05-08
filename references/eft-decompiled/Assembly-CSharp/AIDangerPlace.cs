using System;
using UnityEngine;

[Serializable]
public class AIDangerPlace : IPositionPoint
{
	[SerializeField]
	public Vector3 _pos;

	public Vector3 Position => _pos;

	public void SetPos(Vector3 p)
	{
		_pos = p;
	}
}
