using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class MuzzleSmoke : MonoBehaviour
{
	public class Class651
	{
		public bool SkipProcess;

		public Vector3 Position;

		public Vector3 Velocity;

		public Vector3 Turbulence;

		public Vector3 Direction;

		public Color32 Color;

		public float Diffusion;

		public float YUv;
	}

	public Material Material;

	public float SmokeEnd = 10f;

	public float BrakeDistance = 0.1f;

	[Space(8f)]
	public float DragValue = 0.98f;

	public float Gravity = -2f;

	public float SmokeVelocity = 0.1f;

	[Space(8f)]
	public float TurbulenceDensity = 0.1f;

	public float TurbulenceIntensity = 0.5f;

	[Space(8f)]
	public float SmokeDiffusionBySmokeVelocity;

	[Header("Driven By Muzzle Speed")]
	public float MuzzleSpeedMultiplier;

	public AnimationCurve SpeedTurbulenceDensity = AnimationCurve.Linear(0f, 0f, 30f, 6f);

	public AnimationCurve SpeedTurbulenceStrength = AnimationCurve.Linear(0f, 0f, 30f, 80f);

	public AnimationCurve SpeedSmokeStrength = AnimationCurve.Linear(0f, 1f, 20f, 0.1f);

	public AnimationCurve SpeedStartDiffusion = AnimationCurve.Linear(0f, 1f, 20f, 0.1f);

	[Header("Driven By Time")]
	public AnimationCurve Smoke = AnimationCurve.EaseInOut(0.1f, 0.2f, 3f, 0f);

	public float SmokeStrength = 1f;

	public float SmokeLength = 1f;

	public float SmokeLengthRandomness;

	public float SmokeIncreasingByShot = 0.4f;

	public float ShotFactorDropTime = 0.5f;

	private float float_0;

	private Transform transform_0;

	[CompilerGenerated]
	private bool bool_0;

	private float float_1;

	private LinkedList<Class651> linkedList_0 = new LinkedList<Class651>();

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private float float_2;

	private int int_0;

	private float float_3;

	private float float_4;

	private float float_5;

	private int int_1;

	public bool Destroyed
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void Awake()
	{
		Clear();
		float_3 = BrakeDistance * BrakeDistance;
		transform_0 = base.transform;
	}

	public void OnValidate()
	{
		float_3 = BrakeDistance * BrakeDistance;
	}

	public void LateUpdateValues(Camera cam, float dt)
	{
		float_0 = Mathf.Max(0f, float_0 - dt * ShotFactorDropTime);
		vector3_1 = vector3_0;
		vector3_0 = Vector3.Slerp(vector3_0, transform_0.position, 0.125f);
		vector3_2 = Vector3.Slerp(vector3_2, (vector3_0 - vector3_1) / dt, 1f / 32f);
		float_2 = Mathf.Lerp(float_2, vector3_2.magnitude * MuzzleSpeedMultiplier, 0.25f);
		float num = (float)int_1 * float_5;
		float_1 = Smoke.Evaluate(num) * SmokeStrength;
		if (num >= 1f)
		{
			float_1 = 0f;
		}
		if (cam != null)
		{
			method_1(cam);
			if (linkedList_0.Count != 0)
			{
				method_3();
			}
		}
	}

	public void OnRenderObject()
	{
		if (!(Camera.current != CameraClass.Instance.Camera) && linkedList_0.Count != 0)
		{
			Material.SetPass(0);
			GL.Begin(7);
			method_4();
			GL.End();
		}
	}

	public void method_0()
	{
		base.enabled = true;
		int_1 = 1;
		float_5 = 1f / (SmokeLength + Random.Range(0f, SmokeLengthRandomness));
	}

	public void Clear()
	{
		linkedList_0.Clear();
		float_1 = 0f;
		if (this == null)
		{
			Debug.LogError("MissingReferenceException: The object of type 'MuzzleSmoke' has been destroyed but you are still trying to access it.\r\nYour script should either check if it is null or you should not destroy the object.");
		}
		else
		{
			base.enabled = false;
		}
	}

	public void Shot()
	{
		float_0 = Mathf.Min(float_0 + SmokeIncreasingByShot, 1f);
		if (float_0 >= 1f)
		{
			Clear();
			method_0();
		}
	}

	public void method_1(Camera cam)
	{
		if (float_1 > 0.001f)
		{
			if (linkedList_0.Count == 0)
			{
				method_2(float_2, skipProcess: false);
				method_2(float_2);
			}
			else
			{
				linkedList_0.First.Value.Position = vector3_0;
			}
			if ((linkedList_0.First.Value.Position - linkedList_0.First.Next.Value.Position).sqrMagnitude > float_3)
			{
				Class651 value = linkedList_0.First.Value;
				int_1++;
				float num = SpeedTurbulenceDensity.Evaluate(float_2);
				float_4 += num * TurbulenceDensity;
				Vector3 lhs = vector3_2;
				lhs.y += 0.1f;
				value.Turbulence = Vector3.Cross(lhs, cam.transform.forward).normalized * smethod_0(float_4) * TurbulenceIntensity;
				value.Velocity += (vector3_2 + value.Turbulence * SpeedTurbulenceStrength.Evaluate(float_2)) * SmokeVelocity;
				value.Diffusion = SpeedStartDiffusion.Evaluate(float_2) * SmokeEnd;
				value.SkipProcess = false;
				method_2(float_2);
			}
		}
		else if (linkedList_0.Count >= 2 && linkedList_0.First.Value.SkipProcess && (linkedList_0.First.Value.Position - linkedList_0.First.Next.Value.Position).sqrMagnitude > float_3)
		{
			linkedList_0.First.Value.SkipProcess = false;
		}
		if (linkedList_0.Count > 0)
		{
			if (linkedList_0.Count == 2)
			{
				if (linkedList_0.Last.Previous.Value.Diffusion >= SmokeEnd)
				{
					linkedList_0.Clear();
				}
			}
			else if (linkedList_0.Last.Previous.Value.Diffusion >= SmokeEnd)
			{
				linkedList_0.RemoveLast();
			}
		}
		else
		{
			Clear();
		}
	}

	public static float smethod_0(float t)
	{
		int num = (int)t;
		int num2 = num + 1;
		float a = (float)GClass2608.Int(num + 8324234, -1000, 2000) * 0.001f;
		float b = (float)GClass2608.Int(num2 + 8324234, -1000, 2000) * 0.001f;
		t -= (float)num;
		t = t * t * (3f - 2f * t);
		return Mathf.Lerp(a, b, t);
	}

	public void method_2(float velocity, bool skipProcess = true)
	{
		byte a = (byte)(Mathf.Clamp01(float_1 * SpeedSmokeStrength.Evaluate(velocity)) * 255f);
		linkedList_0.AddFirst(new Class651
		{
			Position = vector3_0,
			Color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a),
			YUv = int_0++,
			SkipProcess = skipProcess
		});
	}

	public void method_3()
	{
		float deltaTime = Time.deltaTime;
		float num = Gravity * deltaTime;
		foreach (Class651 item in linkedList_0)
		{
			if (!item.SkipProcess)
			{
				item.Velocity.y -= num;
				item.Velocity *= DragValue;
				Vector3 vector = (item.Velocity + item.Turbulence) * deltaTime;
				item.Position += vector;
				item.Diffusion += vector.sqrMagnitude * SmokeDiffusionBySmokeVelocity;
			}
		}
	}

	public void method_4()
	{
		linkedList_0.First.Value.Direction = linkedList_0.Last.Value.Position - linkedList_0.First.Value.Position;
		for (LinkedListNode<Class651> next = linkedList_0.First.Next; next != null; next = next.Next)
		{
			Class651 value = next.Value;
			LinkedListNode<Class651> previous = next.Previous;
			LinkedListNode<Class651> next2 = next.Next;
			Vector3 direction = ((next2 != null) ? (next2.Value.Position - previous.Value.Position) : (value.Position - previous.Value.Position));
			value.Direction = direction;
		}
		Class651 @class = linkedList_0.First.Value;
		for (LinkedListNode<Class651> next3 = linkedList_0.First.Next; next3 != null; next3 = next3.Next)
		{
			Class651 value2 = next3.Value;
			GL.Color(@class.Color);
			GL.MultiTexCoord3(0, -1f, @class.YUv, @class.Diffusion);
			GL.MultiTexCoord(1, @class.Direction);
			GL.Vertex(@class.Position);
			GL.Color(value2.Color);
			GL.MultiTexCoord3(0, -1f, value2.YUv, value2.Diffusion);
			GL.MultiTexCoord(1, value2.Direction);
			GL.Vertex(value2.Position);
			GL.Color(value2.Color);
			GL.MultiTexCoord3(0, 1f, value2.YUv, value2.Diffusion);
			GL.MultiTexCoord(1, value2.Direction);
			GL.Vertex(value2.Position);
			GL.Color(@class.Color);
			GL.MultiTexCoord3(0, 1f, @class.YUv, @class.Diffusion);
			GL.MultiTexCoord(1, @class.Direction);
			GL.Vertex(@class.Position);
			@class = value2;
		}
	}
}
