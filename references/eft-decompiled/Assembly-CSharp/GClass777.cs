using System;
using EFT;
using UnityEngine;

public class GClass777
{
	[NonSerialized]
	public MovementContext MovementContext_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	public float Single_0
	{
		get
		{
			return Float_0;
		}
		set
		{
			if (Float_0 != value)
			{
				Float_0 = value;
				MovementContext_0.PlayerAnimator.SetSidebackSpeed(Float_0);
			}
		}
	}

	public float Single_1 => GClass2298.Evaluate(MovementContext_0.InertiaSettings.TiltStartSideBackSpeed, MovementContext_0.Inertia) * (Bool_0 ? Single_4 : 1f);

	public float Single_2 => GClass2298.Evaluate(MovementContext_0.InertiaSettings.TiltAcceleration, MovementContext_0.Inertia) * (Bool_0 ? Single_4 : 1f);

	public float Single_3 => GClass2298.Evaluate(MovementContext_0.InertiaSettings.TiltMaxSideBackSpeed, MovementContext_0.Inertia);

	public float Single_4 => GClass2298.Evaluate(MovementContext_0.InertiaSettings.InertiaBackwardCoef, MovementContext_0.Inertia);

	public void Init(MovementContext context)
	{
		MovementContext_0 = context;
	}

	public void Enter()
	{
		Single_0 = Single_1;
	}

	public void ProcessAnimatorStep(float deltaTime, EStateType Type)
	{
		if (Type == EStateType.None)
		{
			Single_0 = Single_1;
			return;
		}
		Bool_0 = false;
		if (Type == EStateType.Out)
		{
			Bool_0 = true;
		}
		Single_0 = Mathf.Clamp(Float_0 + Single_2 * deltaTime, Single_1, Single_3);
	}
}
