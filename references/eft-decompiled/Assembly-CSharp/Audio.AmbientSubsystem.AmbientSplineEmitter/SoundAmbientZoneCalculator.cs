using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter;

public class SoundAmbientZoneCalculator : BaseSplineEmitterCalculator
{
	[SerializeField]
	private float _blendSpeed = 3f;

	private Vector3[] vector3_0;

	private Vector3[] vector3_1;

	private NativeArray<Struct225> nativeArray_0;

	private Struct225 struct225_0;

	private JobHandle jobHandle_0;

	private Class822 class822_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private bool bool_0;

	private bool bool_1;

	private Vector3 vector3_2;

	private bool bool_2;

	public bool Boolean_0 => Mathf.Approximately(float_0, float_1);

	public override void Init(Transform target)
	{
		base.Init(target);
		nativeArray_0 = new NativeArray<Struct225>(1, Allocator.Persistent);
		class822_0 = new Class822(Points, vector3_0, vector3_1, nativeArray_0, _resolution, _calculationRadius);
		bool_0 = true;
		bool_1 = true;
		bool_2 = true;
	}

	public override void CalculatePoints()
	{
		base.CalculatePoints();
		vector3_0 = new Vector3[_resolution];
		vector3_1 = new Vector3[_resolution];
		for (int i = 0; i <= _resolution; i++)
		{
			if (i > 0)
			{
				vector3_0[i - 1] = Points[i - 1];
				vector3_1[i - 1] = Points[i];
			}
		}
	}

	public override void OnTick()
	{
		method_0();
		base.OnTick();
	}

	public override void OnLateTick(float dt)
	{
		if (!bool_2)
		{
			return;
		}
		if (IsInRange())
		{
			if (!nativeArray_0.IsCreated)
			{
				return;
			}
			if (!(base.TargetPos != vector3_2) && !bool_0)
			{
				if (!Boolean_0)
				{
					method_1(float_1, dt);
				}
			}
			else
			{
				bool_1 = false;
				jobHandle_0.Complete();
				Struct225 @struct = nativeArray_0[0];
				if (!@struct.withinRange)
				{
					return;
				}
				struct225_0 = @struct;
				Vector3 vector = vector3_2 + struct225_0.averageDirection * (float)struct225_0.closestDistance;
				float value = method_5();
				_splineEmitter.Translate(new Vector3(vector.x, struct225_0.closestPoint.y, vector.z));
				_splineEmitter.ScaleMaxDistance(value);
				float_1 = method_4();
				if (bool_0)
				{
					dt = 1f;
				}
				method_1(float_1, dt);
				vector3_2 = base.TargetPos;
				nativeArray_0[0] = default(Struct225);
				bool_0 = false;
			}
		}
		else if (!(float_1 < 1f) && Boolean_0)
		{
			if (!bool_1)
			{
				method_7();
			}
		}
		else
		{
			float_1 = 1f;
			method_1(float_1, dt);
		}
		base.OnLateTick(dt);
	}

	public void method_0()
	{
		if (Initialized && IsInRange() && base.TargetPos != vector3_2)
		{
			jobHandle_0 = class822_0.Schedule(jobHandle_0, base.TargetPos);
		}
	}

	public void method_1(float targetValue, float dt)
	{
		method_3(targetValue, dt);
		method_2(targetValue, dt);
	}

	public void method_2(float targetValue, float dt)
	{
		float value = Mathf.Lerp(float_0, targetValue, dt * _blendSpeed);
		_splineEmitter.UpdateSpread(value);
		float_0 = value;
	}

	public void method_3(float targetValue, float dt)
	{
		float value = Mathf.Lerp(float_0, targetValue, dt * _blendSpeed);
		if (_spline.Loop && struct225_0.isInside)
		{
			_splineEmitter.UpdateSpatialBlend(value);
			float_2 = value;
		}
		else if (float_2 > 0f && float_2 < 1f)
		{
			float value2 = Mathf.Lerp(float_2, 1f, dt * _blendSpeed);
			_splineEmitter.UpdateSpatialBlend(value2);
			float_2 = value2;
		}
	}

	public float method_4()
	{
		return (float)((!_spline.Loop || !struct225_0.isInside) ? struct225_0.averageSpread : 0.0);
	}

	public float method_5()
	{
		if (_spline.Loop && struct225_0.isInside)
		{
			return float.MaxValue;
		}
		return 0f;
	}

	public void method_6()
	{
		if (nativeArray_0.IsCreated)
		{
			class822_0.Dispose();
			nativeArray_0.Dispose();
		}
	}

	public void method_7()
	{
		bool_1 = true;
		jobHandle_0.Complete();
		Vector3 position = Points[0];
		_splineEmitter.Translate(position);
		_splineEmitter.ScaleMaxDistance(0f);
		_splineEmitter.UpdateSpatialBlend(1f);
		_splineEmitter.UpdateSpread(1f);
		float_1 = 1f;
	}

	public override void OnDestroy()
	{
		jobHandle_0.Complete();
		method_6();
		base.OnDestroy();
	}
}
