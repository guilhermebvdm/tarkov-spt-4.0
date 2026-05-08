using System;
using AnimationEventSystem;

namespace EFT;

public interface ILeftHandInteractionEvents
{
	event Action<IAnimatorEventParameter> OnTakeEvent;

	event Action<IAnimatorEventParameter> OnRemoveEvent;

	event Action<IAnimatorEventParameter> OnActionStartedEvent;

	event Action<IAnimatorEventParameter> OnActionEndedEvent;
}
