using System;
using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

public abstract class EnumSoundContentPreset<TEnum, TObj> : ScriptableObject where TEnum : struct, Enum
{
	[Serializable]
	public class Content : SerializableEnumDictionary<TEnum, TObj>
	{
	}

	[SerializeField]
	private Content _content;

	public bool TryGetContent(TEnum key, out TObj content)
	{
		content = default(TObj);
		if (_content.TryGetValue(key, out content) && content != null)
		{
			return true;
		}
		return false;
	}

	public EnumSoundContentPreset()
	{
	}
}
