using UnityEngine;

public abstract class BaseSystemComponent<T> : MonoBehaviour, GInterface31 where T : BaseSystemComponent<T>, GInterface31
{
	public virtual void OnEnable()
	{
		GClass862.RegisterInSystem((T)this);
	}

	public virtual void OnDisable()
	{
		GClass862.UnregisterInSystem((T)this);
	}

	public BaseSystemComponent()
	{
	}
}
