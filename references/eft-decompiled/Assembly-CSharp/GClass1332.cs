using UnityEngine;

public abstract class GClass1332<T, U> : GInterface122 where T : StateMachineBehaviour where U : GClass1328, new()
{
	public virtual U InternalCreate(T behaviour, GClass1312 state)
	{
		return GClass1328.Create<U>(state);
	}

	public GClass1328 Create(StateMachineBehaviour behaviour, GClass1312 state)
	{
		T behaviour2 = behaviour as T;
		return InternalCreate(behaviour2, state);
	}

	GClass1328 GInterface122.Create(StateMachineBehaviour behaviour, GClass1312 state)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Create
		return this.Create(behaviour, state);
	}

	public GClass1332()
	{
	}
}
