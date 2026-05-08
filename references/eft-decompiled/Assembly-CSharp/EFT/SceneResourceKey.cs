using System;
using UnityEngine;

namespace EFT;

[Serializable]
[GAttribute29]
public class SceneResourceKey : ResourceKey, ISceneResource
{
	[SerializeField]
	public bool _onlyOffline;

	[GAttribute31]
	public bool onlyOffline
	{
		get
		{
			return _onlyOffline;
		}
		set
		{
			_onlyOffline = value;
		}
	}

	public bool Equals(SceneResourceKey other)
	{
		if (Equals((ResourceKey)other))
		{
			return onlyOffline == other.onlyOffline;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SceneResourceKey)obj);
	}
}
