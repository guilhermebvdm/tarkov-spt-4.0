using System;
using AnimationEventSystem;

namespace EFT;

public interface IBipodToggleEvents
{
	event Action<IAnimatorEventParameter> OnBipodToggleEvent;
}
