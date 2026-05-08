using BezierSplineTools;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter;

public class BaseSplineEmitterCalculator : MonoBehaviour, GInterface109
{
	protected const float MIN_SPREAD_VALUE = 0f;

	protected const float MAX_SPREAD_VALUE = 1f;

	[SerializeField]
	protected AbstractSplineMappedEmitter _splineEmitter;

	[SerializeField]
	protected BezierSpline _spline;

	[Range(2f, 100f)]
	[SerializeField]
	protected int _resolution = 10;

	[Range(0f, 1000f)]
	[SerializeField]
	protected float _calculationRadius = 20f;

	protected Vector3[] Points;

	protected bool Initialized;

	private Transform transform_0;

	private Transform transform_1;

	private double double_0;

	private double double_1;

	public Vector3 TargetPos => transform_0.position;

	public Vector3 EmitterPos => _splineEmitter.EmitterPosition;

	public virtual void Init(Transform target)
	{
		if (_spline == null)
		{
			Debug.LogError("For correct work you need to set Spline Object");
			return;
		}
		CalculatePoints();
		double num = (double)_calculationRadius + double_0 + 1.0;
		double_1 = num * num;
		transform_0 = target;
		transform_1 = _splineEmitter.transform;
		Initialized = true;
	}

	public virtual void Tick()
	{
		if (Initialized)
		{
			OnTick();
		}
	}

	public virtual void LateTick(float dt = 1f)
	{
		if (Initialized)
		{
			OnLateTick(dt);
		}
	}

	public virtual void OnTick()
	{
	}

	public virtual void OnLateTick(float dt)
	{
	}

	public virtual void CalculatePoints()
	{
		Points = new Vector3[_resolution + 1];
		for (int i = 0; i <= _resolution; i++)
		{
			Points[i] = _spline.GetPoint((float)i / (float)_resolution);
			double num = Vector3.Distance(_spline.gameObject.transform.position, Points[i]);
			if (num > double_0)
			{
				double_0 = num;
			}
		}
	}

	public bool IsInRange()
	{
		return (double)Vector3.SqrMagnitude(TargetPos - transform_1.position) <= double_1;
	}

	public virtual void OnDestroy()
	{
	}
}
