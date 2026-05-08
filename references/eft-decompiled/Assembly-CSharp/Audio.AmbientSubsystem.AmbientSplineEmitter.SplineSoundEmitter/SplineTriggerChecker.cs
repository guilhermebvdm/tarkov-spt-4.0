using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter.SplineSoundEmitter;

public class SplineTriggerChecker : BaseSplineEmitterCalculator
{
	[BurstCompile]
	public struct Struct231 : IJobParallelFor
	{
		[ReadOnly]
		public float sqrDetectionMaxRadius;

		[ReadOnly]
		public Vector3 emitterPosition;

		[ReadOnly]
		public NativeArray<Vector3> triggerPositions;

		[ReadOnly]
		public NativeArray<int> triggerIDs;

		public NativeParallelHashSet<int>.ParallelWriter validTriggerIDs;

		public void Execute(int index)
		{
			if (!(math.lengthsq(emitterPosition - triggerPositions[index]) > sqrDetectionMaxRadius))
			{
				validTriggerIDs.Add(triggerIDs[index]);
			}
		}
	}

	[SerializeField]
	private float _checkCooldown = 0.1f;

	[SerializeField]
	private float _checkMaxRadius = 5f;

	[SerializeField]
	private float _checkPositionDiff = 0.1f;

	[SerializeField]
	private List<SplineTriggerAbstract> _cachedTriggers = new List<SplineTriggerAbstract>();

	private readonly Dictionary<int, SplineTriggerAbstract> dictionary_0 = new Dictionary<int, SplineTriggerAbstract>();

	private float float_0;

	private float float_1;

	private Vector3 vector3_0;

	private JobHandle jobHandle_0;

	private Vector3 vector3_1;

	private NativeArray<Vector3> nativeArray_0;

	private NativeArray<int> nativeArray_1;

	private NativeParallelHashSet<int> nativeParallelHashSet_0;

	public override void Init(Transform target)
	{
		if (_cachedTriggers.Count == 0)
		{
			Debug.LogError("No cached triggers found!");
			return;
		}
		dictionary_0.Clear();
		foreach (SplineTriggerAbstract cachedTrigger in _cachedTriggers)
		{
			dictionary_0[cachedTrigger.GetInstanceID()] = cachedTrigger;
		}
		float_0 = _checkMaxRadius * _checkMaxRadius;
		method_0();
		base.Init(target);
	}

	public void method_0()
	{
		method_1();
		nativeArray_0 = new NativeArray<Vector3>(_cachedTriggers.Count, Allocator.Persistent);
		for (int i = 0; i < _cachedTriggers.Count; i++)
		{
			SplineTriggerAbstract splineTriggerAbstract = _cachedTriggers[i];
			nativeArray_0[i] = splineTriggerAbstract.transform.position;
		}
		nativeArray_1 = new NativeArray<int>(dictionary_0.Keys.ToArray(), Allocator.Persistent);
		nativeParallelHashSet_0 = new NativeParallelHashSet<int>(_cachedTriggers.Count, Allocator.Persistent);
	}

	public void method_1()
	{
		jobHandle_0.Complete();
		if (nativeArray_0.IsCreated)
		{
			nativeArray_0.Dispose();
		}
		if (nativeArray_1.IsCreated)
		{
			nativeArray_1.Dispose();
		}
		if (nativeArray_1.IsCreated)
		{
			nativeParallelHashSet_0.Dispose();
		}
	}

	public override void Tick()
	{
		if (Initialized)
		{
			base.Tick();
			if (Time.realtimeSinceStartup > float_1 && IsInRange() && method_2())
			{
				Struct231 jobData = new Struct231
				{
					sqrDetectionMaxRadius = float_0,
					emitterPosition = base.EmitterPos,
					triggerPositions = nativeArray_0,
					triggerIDs = nativeArray_1,
					validTriggerIDs = nativeParallelHashSet_0.AsParallelWriter()
				};
				jobHandle_0 = IJobParallelForExtensions.Schedule(jobData, _cachedTriggers.Count, 16);
			}
		}
	}

	public override void LateTick(float dt = 1f)
	{
		if (!Initialized)
		{
			return;
		}
		base.LateTick(dt);
		if (!(Time.realtimeSinceStartup > float_1))
		{
			return;
		}
		jobHandle_0.Complete();
		float_1 = Time.realtimeSinceStartup + _checkCooldown;
		if (!IsInRange())
		{
			nativeParallelHashSet_0.Clear();
		}
		else
		{
			if (!method_2())
			{
				return;
			}
			foreach (int item in nativeParallelHashSet_0)
			{
				if (dictionary_0.TryGetValue(item, out var value))
				{
					value.PushTrigger();
				}
			}
		}
	}

	public bool method_2()
	{
		bool num = GClass940.IsPositionIdentical(base.EmitterPos, vector3_0, _checkPositionDiff);
		vector3_0 = base.EmitterPos;
		return !num;
	}

	public override void OnDestroy()
	{
		method_1();
		base.OnDestroy();
	}
}
