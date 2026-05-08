using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Animations;

public class GClass907
{
	[Serializable]
	[CompilerGenerated]
	public class Class545
	{
		public static readonly Class545 class545_0 = new Class545();

		public static Func<Val, GClass907> func_0;

		public static Func<MotionEffector, float> func_1;

		public static Func<MotionEffector, float> func_2;

		public static Func<MotionEffector, float> func_3;

		public static Func<MotionEffector, float> func_4;

		public static Func<MotionEffector, float> func_5;

		public static Func<MotionEffector, float> func_6;

		public static Func<MotionEffector, float> func_7;

		public static Func<MotionEffector, float> func_8;

		public static Func<MotionEffector, float> func_9;

		public static Func<MotionEffector, float> func_10;

		public GClass907 method_0(Val x)
		{
			return new GClass907(x);
		}

		public float method_1(MotionEffector reaction)
		{
			return reaction.RotationVelocity.x;
		}

		public float method_2(MotionEffector reaction)
		{
			return reaction.RotationAcceleration.x;
		}

		public float method_3(MotionEffector reaction)
		{
			return reaction.RotationVelocity.y;
		}

		public float method_4(MotionEffector reaction)
		{
			return reaction.RotationAcceleration.y;
		}

		public float method_5(MotionEffector reaction)
		{
			return reaction.PositionVelocity.x;
		}

		public float method_6(MotionEffector reaction)
		{
			return reaction.PositionVelocity.y;
		}

		public float method_7(MotionEffector reaction)
		{
			return reaction.PositionVelocity.z;
		}

		public float method_8(MotionEffector reaction)
		{
			return reaction.PositionAcceleration.x;
		}

		public float method_9(MotionEffector reaction)
		{
			return reaction.PositionAcceleration.y;
		}

		public float method_10(MotionEffector reaction)
		{
			return reaction.PositionAcceleration.z;
		}
	}

	[NonSerialized]
	public Spring Spring_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Func<MotionEffector, float> Func_0;

	[NonSerialized]
	public static Func<MotionEffector, float>[] Func_1;

	[NonSerialized]
	public Val Val_0;

	public static GClass907[] CreateInstance(MotionEffectorParameters mep)
	{
		return mep.Vals.Select((Val x) => new GClass907(x)).ToArray();
	}

	public GClass907(Val val)
	{
		Val_0 = val;
		Func_1 = new Func<MotionEffector, float>[10];
		Func_1[0] = (MotionEffector reaction) => reaction.RotationVelocity.x;
		Func_1[1] = (MotionEffector reaction) => reaction.RotationAcceleration.x;
		Func_1[2] = (MotionEffector reaction) => reaction.RotationVelocity.y;
		Func_1[3] = (MotionEffector reaction) => reaction.RotationAcceleration.y;
		Func_1[4] = (MotionEffector reaction) => reaction.PositionVelocity.x;
		Func_1[5] = (MotionEffector reaction) => reaction.PositionVelocity.y;
		Func_1[6] = (MotionEffector reaction) => reaction.PositionVelocity.z;
		Func_1[7] = (MotionEffector reaction) => reaction.PositionAcceleration.x;
		Func_1[8] = (MotionEffector reaction) => reaction.PositionAcceleration.y;
		Func_1[9] = (MotionEffector reaction) => reaction.PositionAcceleration.z;
	}

	public void Initialize(Spring rootRotation, Spring handsPosition, Spring handsRotation)
	{
		switch (Val_0.To)
		{
		case Target.CameraRotation:
			Spring_0 = rootRotation;
			break;
		case Target.HandsPosition:
			Spring_0 = handsPosition;
			break;
		case Target.HandsRotation:
			Spring_0 = handsRotation;
			break;
		}
		Int_0 = (int)Val_0.Component;
		Func_0 = Func_1[(int)Val_0.From];
	}

	public void Process(MotionEffector motion, float deltaTime)
	{
		Spring_0.AddAcceleration(Int_0, Func_0(motion) * Val_0.Intensity * deltaTime);
	}
}
