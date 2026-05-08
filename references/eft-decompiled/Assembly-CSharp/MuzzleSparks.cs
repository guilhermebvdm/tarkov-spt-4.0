using System;
using UnityEngine;

public class MuzzleSparks : MonoBehaviour
{
	[Serializable]
	public class CurveValue
	{
		public AnimationCurve Curve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public float Value = 1f;

		public float[] Values = new float[128];

		public void RecalcVals()
		{
			for (int i = 0; i < 128; i++)
			{
				Values[i] = Curve.Evaluate((float)i * (1f / 128f)) * Value;
			}
		}

		public void RecalcVals(Func<float, float> func)
		{
			for (int i = 0; i < 128; i++)
			{
				Values[i] = func(Curve.Evaluate((float)i * (1f / 128f)) * Value);
			}
		}
	}

	[Serializable]
	public class ByteCurveValue
	{
		public AnimationCurve Curve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public byte Value = byte.MaxValue;

		public byte[] Values = new byte[128];

		public void RecalcVals()
		{
			for (int i = 0; i < 128; i++)
			{
				Values[i] = (byte)(Curve.Evaluate((float)i * (1f / 128f)) * (float)(int)Value);
			}
		}

		public void RecalcVals(Func<float, float> func)
		{
			for (int i = 0; i < 128; i++)
			{
				Values[i] = (byte)func(Curve.Evaluate((float)i * (1f / 128f)) * (float)(int)Value);
			}
		}
	}

	public float StartPos;

	public float EmitterRadius;

	public float ConusSize = 1f;

	public int CountMin = 1;

	public int CountRange = 3;

	[GAttribute7("Curve 240;Value 60", true)]
	public CurveValue LifetimeCurve;

	[GAttribute7("Curve 240;Value 60", true)]
	public CurveValue SpeedCurve;

	[GAttribute7("Curve 240;Value 60", true)]
	public CurveValue GravityCurve;

	[GAttribute7("Curve 240;Value 60", true)]
	public CurveValue DragCurve;

	[GAttribute7("Curve 240;Value 120 S 0 255", true)]
	public ByteCurveValue EmissionCurve;

	[GAttribute7("Curve 240;Value 120 S 0 255", true)]
	public ByteCurveValue SizeCurve;

	[GAttribute7("Curve 240;Value 120 S 0 255", true)]
	public ByteCurveValue TurbulenceAmplitudeCurve;

	[GAttribute7("Curve 240;Value 120 S 0 255", true)]
	public ByteCurveValue TurbulenceFrequencyCurve;

	public float EmissionTimeShift;

	private Transform transform_0;

	private const int int_0 = 128;

	private const float float_0 = 1f / 128f;

	public void OnValidate()
	{
		LifetimeCurve.RecalcVals();
		SpeedCurve.RecalcVals();
		GravityCurve.RecalcVals();
		DragCurve.RecalcVals(smethod_0);
		EmissionCurve.RecalcVals();
		SizeCurve.RecalcVals();
		TurbulenceAmplitudeCurve.RecalcVals();
		TurbulenceFrequencyCurve.RecalcVals();
	}

	public void Awake()
	{
		transform_0 = base.transform;
	}

	public void method_0()
	{
		Awake();
	}

	public void Emit(GInterface54 emitter)
	{
		Vector3 vector = -transform_0.up;
		Vector3 vector2 = transform_0.position + vector * StartPos;
		int num = ((CountRange < 1) ? CountMin : GClass2608.Int(CountMin, CountRange));
		float time = Time.time + EmissionTimeShift;
		for (int i = 0; i < num; i++)
		{
			Vector3 position = vector2;
			if (EmitterRadius >= float.Epsilon)
			{
				position += GClass2608.VectorNormalized() * EmitterRadius;
			}
			Vector3 normalized = (vector + GClass2608.VectorNormalized() * ConusSize).normalized;
			int num2 = UnityEngine.Random.Range(0, 128);
			float lifeTime = LifetimeCurve.Values[num2];
			float num3 = SpeedCurve.Values[num2];
			float gravity = GravityCurve.Values[num2];
			float drag = DragCurve.Values[num2];
			byte emission = EmissionCurve.Values[num2];
			byte size = SizeCurve.Values[num2];
			byte turbulence = TurbulenceAmplitudeCurve.Values[num2];
			byte frequency = TurbulenceFrequencyCurve.Values[num2];
			emitter.Emit(position, normalized * num3, time, gravity, drag, lifeTime, emission, size, turbulence, frequency);
		}
	}

	public static float smethod_0(float val)
	{
		val = 1f - val;
		val *= val;
		val *= val;
		return Math.Max(Math.Min(val, 0.999f), 0f);
	}
}
