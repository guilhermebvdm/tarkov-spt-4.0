using EFT;
using UnityEngine;

public interface IGame
{
	GameObject gameObject { get; }

	GameStatus Status { get; }

	GameTimerClass GameTimer { get; }

	float PastTime { get; }
}
