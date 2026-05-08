using System;
using Audio.AmbientSubsystem.PathMoverStrategy;

public interface GInterface106
{
	bool IsCompleted { get; }

	EMovementStrategy MovementStrategy { get; }

	event Action MovementCompleted;

	void Execute();

	void SetSpeed(float duration);

	void Stop(bool resetPosition = false);
}
