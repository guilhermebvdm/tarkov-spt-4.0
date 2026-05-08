using System.Collections;
using System.Collections.Generic;
using Audio.AmbientSubsystem.PathMoverStrategy;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter;

public class SplineEmitterPathMover : BaseSplineEmitterCalculator
{
	public enum EMovementAction : byte
	{
		Stop,
		Continue,
		Repeat,
		Reverse
	}

	private const float float_0 = 0.3f;

	[SerializeField]
	private EMovementStrategy MovementStrategy;

	[SerializeField]
	private EMovementAction _movementActionOnCompleted;

	[SerializeField]
	private Vector2 _startMovementRandomDelayRange = new Vector2(0f, 0f);

	[SerializeField]
	private float _movementSpeed = 0.5f;

	[SerializeField]
	private bool _resetPositionOnMovementEnd;

	[SerializeField]
	private bool _fadeEmitter = true;

	private readonly Dictionary<EMovementStrategy, GInterface106> dictionary_0 = new Dictionary<EMovementStrategy, GInterface106>();

	private GInterface106 ginterface106_0;

	private EMovementAction emovementAction_0;

	private float[] float_1;

	private float float_2;

	private IEnumerator ienumerator_0;

	public override void Init(Transform target)
	{
		base.Init(target);
		method_0();
		method_2();
		SetMovementStrategy(MovementStrategy);
		method_3();
	}

	public void method_0()
	{
		int num = (Points.Length - 1) / 3;
		float_1 = new float[num];
		float_2 = 0f;
		for (int i = 0; i < num; i++)
		{
			int startIndex = i * 3;
			float_1[i] = method_1(startIndex);
			float_2 += float_1[i];
		}
	}

	public float method_1(int startIndex)
	{
		float num = 0f;
		Vector3 a = Points[startIndex];
		for (float num2 = 0f; num2 <= 1f; num2 += 0.01f)
		{
			Vector3 point = GClass1280.GetPoint(Points[startIndex], Points[startIndex + 1], Points[startIndex + 2], Points[startIndex + 3], num2);
			num += Vector3.Distance(a, point);
			a = point;
		}
		return num;
	}

	public void method_2()
	{
		dictionary_0[EMovementStrategy.Forward] = new GClass1192(this, _splineEmitter, Points, float_1, float_2);
		dictionary_0[EMovementStrategy.Backward] = new GClass1191(this, _splineEmitter, Points, float_1, float_2);
		dictionary_0[EMovementStrategy.Random] = new GClass1193(this, _splineEmitter, Points, float_1, float_2);
		ginterface106_0 = dictionary_0[EMovementStrategy.Forward];
		emovementAction_0 = _movementActionOnCompleted;
	}

	public void SetMovementStrategy(EMovementStrategy strategy)
	{
		if (dictionary_0.TryGetValue(strategy, out var value))
		{
			ginterface106_0.MovementCompleted -= method_7;
			ginterface106_0 = value;
			ginterface106_0.MovementCompleted += method_7;
		}
	}

	public void method_3()
	{
		_splineEmitter.StartEmit += method_4;
		_splineEmitter.Stopped += method_5;
	}

	public void method_4()
	{
		emovementAction_0 = _movementActionOnCompleted;
		ExecuteCurrentStrategy();
	}

	public void method_5()
	{
		emovementAction_0 = EMovementAction.Stop;
		ResetCurrentStrategy();
	}

	public void ExecuteCurrentStrategy()
	{
		method_12();
		float num = Random.Range(_startMovementRandomDelayRange.x, _startMovementRandomDelayRange.y);
		if (num > 0f)
		{
			if (_fadeEmitter)
			{
				float fadeOutSec = Mathf.Max(0.3f, _startMovementRandomDelayRange.x);
				_splineEmitter.FadeOut(fadeOutSec);
			}
			ienumerator_0 = GClass7.StartBehaviourTimer(this, num, method_6);
		}
		else
		{
			method_6();
		}
	}

	public void method_6()
	{
		SetMovementSpeed(_movementSpeed);
		ginterface106_0.Execute();
		if (_fadeEmitter)
		{
			_splineEmitter.FadeIn(0.3f);
		}
	}

	public void ResetCurrentStrategy()
	{
		ginterface106_0.Stop(_resetPositionOnMovementEnd);
	}

	public void SetMovementSpeed(float durationSec)
	{
		ginterface106_0.SetSpeed(durationSec);
	}

	public void method_7()
	{
		method_12();
		switch (emovementAction_0)
		{
		default:
			Debug.LogError($"Can't handle target action: {emovementAction_0}");
			method_8();
			break;
		case EMovementAction.Stop:
			method_8();
			break;
		case EMovementAction.Continue:
			break;
		case EMovementAction.Repeat:
			method_9();
			break;
		case EMovementAction.Reverse:
			method_10();
			break;
		}
	}

	public void method_8()
	{
		ResetCurrentStrategy();
		if (_fadeEmitter)
		{
			_splineEmitter.FadeOut(0.3f);
		}
	}

	public void method_9()
	{
		ExecuteCurrentStrategy();
	}

	public void method_10()
	{
		switch (ginterface106_0.MovementStrategy)
		{
		default:
			Debug.LogError($"Movement Strategy : {ginterface106_0.MovementStrategy} unsupported for action: {emovementAction_0}");
			method_8();
			break;
		case EMovementStrategy.Backward:
			SetMovementStrategy(EMovementStrategy.Forward);
			ExecuteCurrentStrategy();
			break;
		case EMovementStrategy.Forward:
			SetMovementStrategy(EMovementStrategy.Backward);
			ExecuteCurrentStrategy();
			break;
		}
	}

	public void method_11()
	{
		_splineEmitter.StartEmit -= method_4;
		_splineEmitter.Stopped -= method_5;
	}

	public void method_12()
	{
		GClass7.StopBehaviourTimer(this, ref ienumerator_0);
	}

	public override void OnDestroy()
	{
		method_12();
		method_11();
		if (ginterface106_0 != null)
		{
			ginterface106_0.MovementCompleted -= method_7;
			ginterface106_0.Stop();
		}
		base.OnDestroy();
	}
}
