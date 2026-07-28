using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	private static T sInstance;

	public static T instance => sInstance;

	public static void Clear()
	{
		sInstance = null;
	}

	protected virtual void Awake()
	{
		if ((Object)(object)sInstance != (Object)null)
		{
			sInstance = (T)this;
		}
	}
}
