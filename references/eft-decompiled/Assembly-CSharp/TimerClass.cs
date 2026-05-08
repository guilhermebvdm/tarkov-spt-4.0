using System;
using UnityEngine;

public class TimerClass
{
	public float _startTime = -1f;

	[NonSerialized]
	public float Float_0 = -1f;

	public float Time
	{
		get
		{
			if (_startTime < 0f)
			{
				return 0f;
			}
			if (Float_0 >= 0f)
			{
				return Float_0 - _startTime;
			}
			return UnityEngine.Time.realtimeSinceStartup - _startTime;
		}
	}

	public bool IsStarted => _startTime >= 0f;

	public bool IsPaused => Float_0 >= 0f;

	public TimerClass(bool instantStart = false)
	{
		if (instantStart)
		{
			Start();
		}
	}

	public void Start(float startTime = 0f)
	{
		_startTime = UnityEngine.Time.realtimeSinceStartup - startTime;
		Float_0 = -1f;
	}

	public void Stop()
	{
		_startTime = -1f;
	}

	public void SetPause(bool pause)
	{
		if (pause)
		{
			Float_0 = UnityEngine.Time.realtimeSinceStartup;
		}
		else
		{
			Start(Time);
		}
	}
}
