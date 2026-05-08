using System;
using EFT;
using UnityEngine;

public class GrassTrampler : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve _angleCurve;

	[SerializeField]
	public float TransitionTimeBetweenStates;

	[SerializeField]
	public GrassValues StandGrassValues;

	[SerializeField]
	public GrassValues DuckGrassValues;

	[SerializeField]
	public GrassValues ProneGrassValues;

	private const string string_0 = "_GrassPlayerPosition";

	private const string string_1 = "_GrassPlayerDirection";

	private const string string_2 = "_GrassValues";

	private AnimationCurve animationCurve_0 = new AnimationCurve();

	private GrassValues grassValues_0;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private EPlayerPose eplayerPose_0;

	private bool bool_0;

	private float float_0;

	private float float_1;

	private float float_2;

	private float float_3;

	private float float_4;

	private float float_5;

	private GrassValues grassValues_1;

	private float float_6;

	public Player Player_0 => GamePlayerOwner.MyPlayer;

	public EPlayerPose EPlayerPose_0 => Player_0.Pose;

	public void Awake()
	{
		grassValues_0 = (grassValues_1 = StandGrassValues);
		float_6 = grassValues_0.AngleInRads;
		eplayerPose_0 = EPlayerPose.Stand;
		animationCurve_0.keys = _angleCurve.keys;
		for (int i = 0; i < animationCurve_0.keys.Length; i++)
		{
			animationCurve_0.keys[i].value = _angleCurve.keys[i].value * grassValues_0.AngleInRads;
		}
	}

	public void Update()
	{
		if (!(Player_0 == null))
		{
			if (EPlayerPose_0 != eplayerPose_0)
			{
				float_2 = Time.time;
				grassValues_1 = grassValues_0;
				eplayerPose_0 = EPlayerPose_0;
			}
			method_0();
			method_2();
		}
	}

	public void method_0()
	{
		GrassValues to = method_1();
		float t = Mathf.Min(Mathf.Sqrt(Time.time - float_2 / TransitionTimeBetweenStates), 1f);
		grassValues_0.Lerp(grassValues_1, to, t);
		bool_0 = Time.time - float_2 < TransitionTimeBetweenStates && EPlayerPose_0 == EPlayerPose.Prone;
		vector3_1 = ((EPlayerPose_0 != EPlayerPose.Prone) ? Player_0.Velocity : Player_0.LookDirection);
		float_1 = grassValues_0.ReturnTime / 2f;
		float_0 = 0.2f * grassValues_0.ReturnTime;
		float num = Time.time - float_4;
		if (vector3_1.sqrMagnitude > 0.0001f)
		{
			if (Vector3.Angle(vector3_1, vector3_2) > 40f && num <= float_1 && !bool_0)
			{
				if (Math.Abs(num - float_1) < 0.025f)
				{
					float_3 = Time.time;
				}
				float_5 = num + float_0;
			}
			else
			{
				vector3_2 = vector3_1;
				float_4 = Time.time;
				float_5 = float.MaxValue;
			}
		}
		else
		{
			bool flag = num <= grassValues_0.ReturnTime;
			float_5 = (flag ? (num + float_0) : (-1f));
			float_3 = Time.time;
		}
		if (Math.Abs(float_6 - grassValues_0.AngleInRads) > 0.01f)
		{
			for (int i = 0; i < animationCurve_0.keys.Length; i++)
			{
				animationCurve_0.keys[i].value = _angleCurve.keys[i].value * grassValues_0.AngleInRads;
			}
			float_6 = grassValues_0.AngleInRads;
		}
	}

	public GrassValues method_1()
	{
		return EPlayerPose_0 switch
		{
			EPlayerPose.Prone => ProneGrassValues, 
			EPlayerPose.Duck => DuckGrassValues, 
			EPlayerPose.Stand => StandGrassValues, 
			_ => new GrassValues(), 
		};
	}

	public void method_2()
	{
		float startTimeOfMove = Time.time - float_3;
		float w = ((EPlayerPose_0 != EPlayerPose.Prone) ? method_3(startTimeOfMove, float_5) : method_4(startTimeOfMove, float_5));
		Shader.SetGlobalVector("_GrassValues", grassValues_0.ToVector4ExceptReturnTime());
		Shader.SetGlobalVector("_GrassPlayerPosition", Player_0.Position);
		Shader.SetGlobalVector("_GrassPlayerDirection", new Vector4(vector3_2.x, vector3_2.z, 0f, w));
	}

	public float method_3(float startTimeOfMove, float delta)
	{
		float num = 0f;
		if (delta <= grassValues_0.ReturnTime + float_0)
		{
			float value = delta / grassValues_0.ReturnTime;
			value = Mathf.Clamp(value, 0f, 1f);
			return animationCurve_0.Evaluate(value);
		}
		if (startTimeOfMove > 0f && startTimeOfMove < float_0)
		{
			return animationCurve_0.Evaluate(startTimeOfMove / grassValues_0.ReturnTime);
		}
		return grassValues_0.AngleInRads;
	}

	public float method_4(float startTimeOfMove, float delta)
	{
		if (delta > grassValues_0.ReturnTime && startTimeOfMove > 0f && !(startTimeOfMove >= float_0))
		{
			return animationCurve_0.Evaluate(startTimeOfMove / grassValues_0.ReturnTime);
		}
		return grassValues_0.AngleInRads;
	}
}
