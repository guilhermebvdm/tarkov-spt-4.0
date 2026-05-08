using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CW2.Animations;

public class PhysicsSimulator : MonoBehaviour
{
	[Serializable]
	public class RandomDevice
	{
		public float Frequency = 1f;

		public float Intensity = 1f;

		public float IntensityShift;

		public float TimeShift;

		public bool RandomTimeShift;

		public bool SmoothStart;

		public Val ValueConfiguration;

		[NonSerialized]
		public float RndSeed;

		[NonSerialized]
		public float StartT;

		public void Initialize()
		{
			if (RandomTimeShift)
			{
				TimeShift = UnityEngine.Random.value * 300f;
			}
			RndSeed = UnityEngine.Random.value * 10f;
			ValueConfiguration.Initialize();
		}

		public void Process(PhysicsSimulator physicsSimulator)
		{
			float num = (Mathf.PerlinNoise(TimeShift + Time.time * Frequency, RndSeed) - 0.5f) * Intensity;
			if (SmoothStart)
			{
				if (StartT < 0f)
				{
					StartT = Time.time + 3f;
				}
				float num2 = (Time.time - StartT) * (1f / 3f);
				if (num2 > 3f)
				{
					SmoothStart = false;
					StartT = -1f;
				}
				else
				{
					num *= num2;
				}
			}
			ValueConfiguration.Process(physicsSimulator, num + IntensityShift);
		}
	}

	[Serializable]
	public class CurveDevice
	{
		public AnimationCurve Curve;

		public float Frequency = 1f;

		public float Intensity = 1f;

		public float TimeShift;

		public float IntensityShift;

		public Val ValueConfiguration;

		public void Initialize()
		{
			ValueConfiguration.Initialize();
		}

		public void Process(PhysicsSimulator physicsSimulator)
		{
			float time = TimeShift + Time.time * Frequency;
			float val = IntensityShift + Curve.Evaluate(time) * Intensity;
			ValueConfiguration.Process(physicsSimulator, val);
		}
	}

	[Serializable]
	public class InputDevice
	{
		public string AxisName;

		public float Intensity = 1f;

		public float IntensityShift;

		public Val ValueConfiguration;

		public void Initialize()
		{
			ValueConfiguration.Initialize();
		}

		public void Process(PhysicsSimulator physicsSimulator)
		{
			float val = IntensityShift + Input.GetAxis(AxisName) * Intensity;
			ValueConfiguration.Process(physicsSimulator, val);
		}
	}

	[Serializable]
	public class ScriptValueDevice
	{
		public string ValueName;

		public float Intensity = 1f;

		public float IntensityShift;

		public Val ValueConfiguration;

		public void Initialize()
		{
			ValueConfiguration.Initialize();
		}

		public void Add(PhysicsSimulator physicsSimulator, float value)
		{
			value = IntensityShift + value * Intensity;
			ValueConfiguration.Process(physicsSimulator, value);
		}
	}

	[Serializable]
	public class UnityValueDevice
	{
		public enum Values
		{
			MousePosX,
			MousePosY
		}

		[Serializable]
		[CompilerGenerated]
		public class Class740
		{
			public static readonly Class740 class740_0 = new Class740();

			public float method_0()
			{
				return Mathf.Clamp01(Input.mousePosition.x / (float)Screen.width) * 2f - 1f;
			}

			public float method_1()
			{
				return Mathf.Clamp01(Input.mousePosition.y / (float)Screen.height) * 2f - 1f;
			}
		}

		public Values ValueType;

		public float Intensity = 1f;

		public float IntensityShift;

		public Val ValueConfiguration;

		[NonSerialized]
		public static Func<float>[] Values;

		public void Initialize()
		{
			ValueConfiguration.Initialize();
		}

		static UnityValueDevice()
		{
			UnityValueDevice.Values = new Func<float>[2];
			UnityValueDevice.Values[0] = () => Mathf.Clamp01(Input.mousePosition.x / (float)Screen.width) * 2f - 1f;
			UnityValueDevice.Values[1] = () => Mathf.Clamp01(Input.mousePosition.y / (float)Screen.height) * 2f - 1f;
		}

		public void Process(PhysicsSimulator physicsSimulator)
		{
			float val = IntensityShift + UnityValueDevice.Values[(int)ValueType]() * Intensity;
			ValueConfiguration.Process(physicsSimulator, val);
		}
	}

	[Serializable]
	public class Spring
	{
		[HideInInspector]
		public Vector3 Zero;

		public float ReturnSpeed = 1f;

		public float Damping = 0.1f;

		public Vector3 Min = new Vector3(-180f, -180f, -180f);

		public Vector3 Max = new Vector3(180f, 180f, 180f);

		public float VelocityMax = 10f;

		public void FixedUpdate(ref Vector3 velocity, ref Vector3 current)
		{
			velocity += (Zero - current) * ReturnSpeed;
			velocity *= Damping;
			velocity = smethod_0(velocity, VelocityMax);
			current += velocity;
			current = Vector3.Min(Vector3.Max(current, Min), Max);
		}

		public static Vector3 smethod_0(Vector3 vec, float limit)
		{
			if (vec.x > limit)
			{
				vec.x = limit;
			}
			if (vec.y > limit)
			{
				vec.y = limit;
			}
			if (vec.z > limit)
			{
				vec.z = limit;
			}
			if (vec.x < 0f - limit)
			{
				vec.x = 0f - limit;
			}
			if (vec.y < 0f - limit)
			{
				vec.y = 0f - limit;
			}
			if (vec.z < 0f - limit)
			{
				vec.z = 0f - limit;
			}
			return vec;
		}
	}

	[Serializable]
	public class Val
	{
		public enum TargetType
		{
			Position,
			PositionVelocity,
			Rotation,
			RotationVelocity
		}

		public enum ComponentType
		{
			X,
			Y,
			Z
		}

		public enum OperationType
		{
			Set,
			Add,
			Mult,
			Clamp,
			Min,
			Max
		}

		[Serializable]
		[CompilerGenerated]
		public class Class741
		{
			public static readonly Class741 class741_0 = new Class741();

			public float method_0(float f, float f1)
			{
				return f1;
			}

			public float method_1(float f, float f1)
			{
				return f + f1;
			}

			public float method_2(float f, float f1)
			{
				return f * f1;
			}

			public float method_3(float f, float f1)
			{
				return Mathf.Clamp(f, 0f - f1, f1);
			}

			public Vector3 method_4(PhysicsSimulator s)
			{
				return s.Position;
			}

			public Vector3 method_5(PhysicsSimulator s)
			{
				return s.PositionVelocity;
			}

			public Vector3 method_6(PhysicsSimulator s)
			{
				return s.Rotation;
			}

			public Vector3 method_7(PhysicsSimulator s)
			{
				return s.RotationVelocity;
			}

			public void method_8(PhysicsSimulator s, Vector3 v)
			{
				s.Position = v;
			}

			public void method_9(PhysicsSimulator s, Vector3 v)
			{
				s.PositionVelocity = v;
			}

			public void method_10(PhysicsSimulator s, Vector3 v)
			{
				s.Rotation = v;
			}

			public void method_11(PhysicsSimulator s, Vector3 v)
			{
				s.RotationVelocity = v;
			}
		}

		public TargetType Target;

		public OperationType Operation;

		public ComponentType Component;

		[NonSerialized]
		public Func<float, float, float> Operation_1;

		[NonSerialized]
		public Func<PhysicsSimulator, Vector3> TargetGet;

		[NonSerialized]
		public Action<PhysicsSimulator, Vector3> TargetSet;

		[NonSerialized]
		public int Component_1;

		[NonSerialized]
		public static Func<float, float, float>[] Operations;

		[NonSerialized]
		public static Func<PhysicsSimulator, Vector3>[] TargetGets;

		[NonSerialized]
		public static Action<PhysicsSimulator, Vector3>[] TargetSets;

		static Val()
		{
			Operations = new Func<float, float, float>[6];
			Operations[0] = (float f, float f1) => f1;
			Operations[1] = (float f, float f1) => f + f1;
			Operations[2] = (float f, float f1) => f * f1;
			Operations[3] = (float f, float f1) => Mathf.Clamp(f, 0f - f1, f1);
			Operations[4] = Mathf.Min;
			Operations[5] = Mathf.Max;
			TargetGets = new Func<PhysicsSimulator, Vector3>[4];
			TargetGets[0] = (PhysicsSimulator s) => s.Position;
			TargetGets[1] = (PhysicsSimulator s) => s.PositionVelocity;
			TargetGets[2] = (PhysicsSimulator s) => s.Rotation;
			TargetGets[3] = (PhysicsSimulator s) => s.RotationVelocity;
			TargetSets = new Action<PhysicsSimulator, Vector3>[4];
			TargetSets[0] = delegate(PhysicsSimulator s, Vector3 v)
			{
				s.Position = v;
			};
			TargetSets[1] = delegate(PhysicsSimulator s, Vector3 v)
			{
				s.PositionVelocity = v;
			};
			TargetSets[2] = delegate(PhysicsSimulator s, Vector3 v)
			{
				s.Rotation = v;
			};
			TargetSets[3] = delegate(PhysicsSimulator s, Vector3 v)
			{
				s.RotationVelocity = v;
			};
		}

		public void Initialize()
		{
			Component_1 = (int)Component;
			Operation_1 = Operations[(int)Operation];
			TargetGet = TargetGets[(int)Target];
			TargetSet = TargetSets[(int)Target];
		}

		public void Process(PhysicsSimulator physicsSimulator, float val)
		{
			Vector3 arg = TargetGet(physicsSimulator);
			arg[Component_1] = Operation_1(arg[Component_1], val);
			TargetSet(physicsSimulator, arg);
		}
	}

	public Vector3 Pivot;

	public Spring PositionSpring;

	public Spring RotationSpring;

	public RandomDevice[] RandomDevices;

	public CurveDevice[] CurveDevices;

	public InputDevice[] InputDevices;

	public ScriptValueDevice[] ScriptValueDevices;

	public UnityValueDevice[] UnityValueDevices;

	[HideInInspector]
	public Vector3 Position;

	[HideInInspector]
	public Vector3 PositionVelocity;

	[HideInInspector]
	public Vector3 Rotation;

	[HideInInspector]
	public Vector3 RotationVelocity;

	private Dictionary<string, ScriptValueDevice> dictionary_0 = new Dictionary<string, ScriptValueDevice>();

	[ContextMenu("Awake")]
	public void Awake()
	{
		RandomDevice[] randomDevices = RandomDevices;
		for (int i = 0; i < randomDevices.Length; i++)
		{
			randomDevices[i].Initialize();
		}
		CurveDevice[] curveDevices = CurveDevices;
		for (int i = 0; i < curveDevices.Length; i++)
		{
			curveDevices[i].Initialize();
		}
		InputDevice[] inputDevices = InputDevices;
		for (int i = 0; i < inputDevices.Length; i++)
		{
			inputDevices[i].Initialize();
		}
		foreach (KeyValuePair<string, ScriptValueDevice> item in dictionary_0)
		{
			item.Value.Initialize();
		}
		UnityValueDevice[] unityValueDevices = UnityValueDevices;
		for (int i = 0; i < unityValueDevices.Length; i++)
		{
			unityValueDevices[i].Initialize();
		}
		ScriptValueDevice[] scriptValueDevices = ScriptValueDevices;
		foreach (ScriptValueDevice scriptValueDevice in scriptValueDevices)
		{
			dictionary_0.Add(scriptValueDevice.ValueName, scriptValueDevice);
		}
		Position = (PositionSpring.Zero = base.transform.localPosition - Pivot);
		Rotation = (RotationSpring.Zero = base.transform.localEulerAngles);
		PositionVelocity = Vector3.zero;
		RotationVelocity = Vector3.zero;
	}

	public void Add(string name, float value)
	{
		if (dictionary_0.TryGetValue(name, out var value2))
		{
			value2.Add(this, value);
		}
	}

	public void FixedUpdate()
	{
		RandomDevice[] randomDevices = RandomDevices;
		for (int i = 0; i < randomDevices.Length; i++)
		{
			randomDevices[i].Process(this);
		}
		CurveDevice[] curveDevices = CurveDevices;
		for (int i = 0; i < curveDevices.Length; i++)
		{
			curveDevices[i].Process(this);
		}
		InputDevice[] inputDevices = InputDevices;
		for (int i = 0; i < inputDevices.Length; i++)
		{
			inputDevices[i].Process(this);
		}
		UnityValueDevice[] unityValueDevices = UnityValueDevices;
		for (int i = 0; i < unityValueDevices.Length; i++)
		{
			unityValueDevices[i].Process(this);
		}
		PositionSpring.FixedUpdate(ref PositionVelocity, ref Position);
		RotationSpring.FixedUpdate(ref RotationVelocity, ref Rotation);
		Quaternion quaternion = Quaternion.Euler(Rotation);
		base.transform.localPosition = Position + quaternion * Pivot;
		base.transform.localRotation = quaternion;
	}
}
