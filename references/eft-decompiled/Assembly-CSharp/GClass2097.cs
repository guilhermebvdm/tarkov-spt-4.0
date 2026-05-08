using System;
using System.Collections.Generic;
using AnimationEventSystem;

public abstract class GClass2097 : GInterface212
{
	[NonSerialized]
	public Dictionary<int, Action<IAnimatorEventParameter>> Dictionary_0;

	public Dictionary<int, Action<IAnimatorEventParameter>> BindedFunctions => Dictionary_0;

	public GClass2097()
	{
	}
}
