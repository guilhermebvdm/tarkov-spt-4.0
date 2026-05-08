using System;
using UnityEngine;

namespace Audio.Data;

public abstract class SoundContainerBase<T, TN> : ScriptableObject where T : struct, Enum where TN : class
{
	public abstract bool TryGetContent(T soundType, out TN content);

	public SoundContainerBase()
	{
	}
}
