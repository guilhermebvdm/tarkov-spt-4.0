using System;
using UnityEngine;

namespace Audio.Data;

public abstract class SoundBankContainerBase<T> : ScriptableObject where T : struct, Enum
{
	public abstract bool TryGetBank(T soundType, out SoundBankWithSettings bank);

	public SoundBankContainerBase()
	{
	}
}
