using System;
using EFT;

public interface IPlayerStateContainerBehaviour : IStateBehaviour
{
	bool DefaultState { get; set; }

	EPlayerState PlayerStateName { get; set; }

	BaseMovementState EncapsulatedState { get; set; }

	int FullNameHash { get; }

	void Init(Func<EPlayerState, int, bool, BaseMovementState> newStateFunc);

	void Deinit();
}
