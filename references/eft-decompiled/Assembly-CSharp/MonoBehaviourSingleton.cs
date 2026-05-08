using Comfort.Common;
using UnityEngine;

public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
{
	public static T Instance => Singleton<T>.Instance;

	public static bool Instantiated => Singleton<T>.Instantiated;

	public virtual void Awake()
	{
		Singleton<T>.Create((T)this);
	}

	public virtual void OnDestroy()
	{
		Singleton<T>.TryRelease((T)this);
	}

	public static bool Exist(out T component)
	{
		if (Instantiated)
		{
			component = Instance;
			return true;
		}
		component = null;
		return false;
	}

	public MonoBehaviourSingleton()
	{
	}
}
