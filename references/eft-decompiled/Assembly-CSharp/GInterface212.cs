using System;
using System.Collections.Generic;
using AnimationEventSystem;

public interface GInterface212
{
	Dictionary<int, Action<IAnimatorEventParameter>> BindedFunctions { get; }
}
