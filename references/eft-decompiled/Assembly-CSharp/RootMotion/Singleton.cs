using UnityEngine;

namespace RootMotion;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
	private static T gparam_0;

	public static T instance => gparam_0;

	public virtual void Awake()
	{
		if (gparam_0 != null)
		{
			Debug.LogError(base.name + "error: already initialized", this);
		}
		gparam_0 = (T)this;
	}

	public Singleton()
	{
	}
}
