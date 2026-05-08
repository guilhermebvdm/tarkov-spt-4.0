using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EFT;

public class EndByTimerScenario : MonoBehaviour
{
	public interface Interface8 : IGame
	{
		void StopGame();
	}

	private Interface8 interface8_0;

	private GameTimerClass gameTimerClass;

	[CompilerGenerated]
	private GameStatus gameStatus_0 = GameStatus.Started;

	public GameStatus GameStatus_0
	{
		[CompilerGenerated]
		get
		{
			return gameStatus_0;
		}
		[CompilerGenerated]
		set
		{
			gameStatus_0 = value;
		}
	}

	public static EndByTimerScenario smethod_0(Interface8 game)
	{
		EndByTimerScenario endByTimerScenario = game.gameObject.AddComponent<EndByTimerScenario>();
		endByTimerScenario.interface8_0 = game;
		endByTimerScenario.gameTimerClass = game.GameTimer;
		return endByTimerScenario;
	}

	public void Update()
	{
		if (interface8_0.Status == GameStatus_0 && gameTimerClass.SessionTime.HasValue)
		{
			TimeSpan pastTime = gameTimerClass.PastTime;
			TimeSpan? sessionTime = gameTimerClass.SessionTime;
			if (pastTime >= sessionTime)
			{
				interface8_0.StopGame();
				base.enabled = false;
			}
		}
	}
}
