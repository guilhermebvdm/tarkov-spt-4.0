using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;
using Audio.SpatialSystem.Utils;
using UnityEngine;

public class GClass1164
{
	public class Class798
	{
		public int size;

		public Func<object> creator;

		public Action<object> resetter;

		public Action<object> disposer;

		public Action<object> returner;
	}

	public class Class799
	{
		public object Calculator;

		public Class798 Info;

		public void Initialize(object calculator, Class798 info)
		{
			Calculator = calculator;
			Info = info;
		}

		public void Clear()
		{
			Calculator = null;
			Info = null;
		}

		public bool TryFinalize()
		{
			object calculator = Calculator;
			if (!(calculator is GClass1129 { LastHandle: var lastHandle } gClass))
			{
				if (!(calculator is GClass1134 gClass2))
				{
					if (!(calculator is GClass1170 { LastJobHandle: var lastJobHandle } gClass3))
					{
						Info.resetter(Calculator);
						Info.returner(Calculator);
						return true;
					}
					if (!lastJobHandle.IsCompleted)
					{
						return false;
					}
					gClass3.CompleteCalculation();
					Info.resetter(gClass3);
					Info.returner(gClass3);
					return true;
				}
				if (gClass2.IsCalculating)
				{
					gClass2.ManualLateUpdate();
					return false;
				}
				gClass2.FinalizeAndGetAggregateResult();
				Info.resetter(gClass2);
				Info.returner(gClass2);
				return true;
			}
			if (!lastHandle.IsCompleted)
			{
				return false;
			}
			gClass.ManualLateUpdate();
			Info.resetter(gClass);
			Info.returner(gClass);
			return true;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class800
	{
		public static readonly Class800 class800_0 = new Class800();

		public static Func<Class799> func_0;

		public static Action<Class799> action_0;

		public Class799 method_0()
		{
			return new Class799();
		}

		public void method_1(Class799 pr)
		{
			pr.Clear();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class801<TCalculator>
	{
		public static readonly Class801<TCalculator> class801_0 = new Class801<TCalculator>();

		public static Action<object> action_0;

		public void method_0(object obj)
		{
			GClass1163.Return((TCalculator)obj);
		}
	}

	[CompilerGenerated]
	public class Class802<T>
	{
		public Class798 info;

		public T method_0()
		{
			return (T)info.creator();
		}

		public void method_1(T calc)
		{
			info.resetter(calc);
		}

		public void method_2(T calc)
		{
			info.disposer(calc);
		}
	}

	[CompilerGenerated]
	public class Class803<T>
	{
		public Func<T> creator;

		public Action<T> resetter;

		public Action<T> disposer;

		public object method_0()
		{
			return creator();
		}

		public void method_1(object obj)
		{
			resetter((T)obj);
		}

		public void method_2(object obj)
		{
			disposer((T)obj);
		}
	}

	[NonSerialized]
	public Dictionary<Type, Class798> Dictionary_0 = new Dictionary<Type, Class798>(3);

	[NonSerialized]
	public Queue<Class799> Queue_0 = new Queue<Class799>(90);

	[NonSerialized]
	public GClass835<Class799> Gclass835_0;

	[NonSerialized]
	public SpatialAudioSystem SpatialAudioSystem_0;

	[NonSerialized]
	public SpatialAudioPoolsConfig SpatialAudioPoolsConfig_0;

	[NonSerialized]
	public AudioOcclusionSettings AudioOcclusionSettings_0;

	public GClass1164(SpatialAudioSystem audioSystem, SpatialAudioPoolsConfig config, AudioOcclusionSettings occlusionSettings)
	{
		SpatialAudioSystem_0 = audioSystem;
		SpatialAudioPoolsConfig_0 = config;
		AudioOcclusionSettings_0 = occlusionSettings;
		int size = SpatialAudioPoolsConfig_0.parallelPropagationCalculatorsPreCacheCount + SpatialAudioPoolsConfig_0.combinedCalculatorsPreCacheCount + SpatialAudioPoolsConfig_0.transmissionCalculatorsPoolCount;
		Gclass835_0 = new GClass835<Class799>(size, () => new Class799(), delegate(Class799 pr)
		{
			pr.Clear();
		});
	}

	public void PreCreatePools()
	{
		method_0(SpatialAudioPoolsConfig_0.parallelPropagationCalculatorsPreCacheCount, method_1, method_2, method_3);
		method_0(SpatialAudioPoolsConfig_0.combinedCalculatorsPreCacheCount, method_4, method_5, method_6);
		method_0(SpatialAudioPoolsConfig_0.transmissionCalculatorsPoolCount, method_7, method_8, method_9);
	}

	public TCalculator WithdrawCalculator<TCalculator>()
	{
		Type typeFromHandle = typeof(TCalculator);
		if (Dictionary_0.TryGetValue(typeFromHandle, out var info))
		{
			return GClass1163.Withdraw(info.size, () => (TCalculator)info.creator(), delegate(TCalculator calc)
			{
				info.resetter(calc);
			}, delegate(TCalculator calc)
			{
				info.disposer(calc);
			});
		}
		return default(TCalculator);
	}

	public void ReturnCalculator<TCalculator>(TCalculator calculator)
	{
		if (calculator != null)
		{
			Type type = calculator.GetType();
			if (Dictionary_0.TryGetValue(type, out var value))
			{
				Class799 @class = Gclass835_0.Withdraw();
				@class.Initialize(calculator, value);
				Queue_0.Enqueue(@class);
			}
		}
	}

	public void UpdatePendingCalculators(int maxPerFrame = 1)
	{
		for (int i = 0; i < maxPerFrame; i++)
		{
			if (Queue_0.Count <= 0)
			{
				break;
			}
			Class799 @class = Queue_0.Peek();
			if (@class.TryFinalize())
			{
				Queue_0.Dequeue();
				Gclass835_0.Return(@class);
				continue;
			}
			break;
		}
	}

	public void DisposePools()
	{
		while (Queue_0.Count > 0)
		{
			Class799 @class = Queue_0.Dequeue();
			if (!@class.TryFinalize())
			{
				@class.Info.disposer(@class.Calculator);
			}
			Gclass835_0.Return(@class);
		}
		Gclass835_0.Dispose();
		GClass1163.DisposePool<GClass1129>();
		GClass1163.DisposePool<GClass1134>();
		GClass1163.DisposePool<GClass1170>();
		GClass1163.ClearPools();
	}

	public void method_0<T>(int size, Func<T> creator, Action<T> resetter, Action<T> disposer)
	{
		Type typeFromHandle = typeof(T);
		Dictionary_0[typeFromHandle] = new Class798
		{
			size = size,
			creator = () => creator(),
			resetter = delegate(object obj)
			{
				resetter((T)obj);
			},
			disposer = delegate(object obj)
			{
				disposer((T)obj);
			},
			returner = delegate(object obj)
			{
				GClass1163.Return((T)obj);
			}
		};
		GClass1163.PreCreatePool(size, creator, resetter, disposer);
	}

	public GClass1129 method_1()
	{
		return new GClass1129(SpatialAudioSystem_0);
	}

	public void method_2<T>(T calculator)
	{
		if (calculator is GClass1129 gClass)
		{
			gClass.Reset();
			if (gClass.LastHandle.IsCompleted)
			{
				gClass.ManualLateUpdate();
			}
		}
	}

	public void method_3<T>(T calculator)
	{
		if (calculator is GClass1129 gClass)
		{
			gClass.Dispose();
		}
	}

	public GClass1134 method_4()
	{
		return new GClass1134(AudioOcclusionSettings_0, SpatialAudioSystem_0);
	}

	public void method_5<T>(T calculator)
	{
		if (calculator is GClass1134 { IsCalculating: false } gClass)
		{
			gClass.FinalizeAndGetAggregateResult();
		}
	}

	public void method_6<T>(T calculator)
	{
		if (calculator is GClass1134 gClass)
		{
			gClass.Dispose();
		}
	}

	public GClass1170 method_7()
	{
		return new GClass1170(AudioOcclusionSettings_0.transmissionSettings, new QueryParameters(AudioOcclusionSettings_0.commonSettings.occlusionLayerMask), AudioOcclusionSettings_0.commonSettings.maxDistance);
	}

	public void method_8<T>(T calculator)
	{
		if (calculator is GClass1170 { IsCalculationInProgress: false } gClass)
		{
			gClass.CompleteCalculation();
		}
	}

	public void method_9<T>(T calculator)
	{
		if (calculator is GClass1170 gClass)
		{
			gClass.Dispose();
		}
	}
}
