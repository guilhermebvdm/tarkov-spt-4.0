using System;
using AnimationEventSystem;

namespace EFT;

public interface IDropBackPackEvents
{
	event Action<IAnimatorEventParameter> OnBackpackDropEvent;
}
