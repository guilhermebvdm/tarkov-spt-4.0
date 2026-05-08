using System;
using AnimationEventSystem;

namespace EFT;

public interface IVaultingSoundsEvents
{
	event Action<IAnimatorEventParameter> OnStartEvent;

	event Action<IAnimatorEventParameter> OnEndEvent;

	event Action<IAnimatorEventParameter> OnGrabEvent;

	event Action<IAnimatorEventParameter> OnSlideEvent;
}
