using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT.InputSystem;

public class GClass2409 : GClass2408
{
	[Flags]
	public enum EUseSide
	{
		None = 0,
		Positive = 1,
		Negative = 2
	}

	public enum EAxisState
	{
		JustAxis,
		WithKeysIdling,
		WithKeysPressed
	}

	public abstract class Class1862
	{
		[NonSerialized]
		public const float Float_0 = 1000f;

		public readonly GClass2409 AxisCombination;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_1;

		public float Value
		{
			[CompilerGenerated]
			get
			{
				return Float_1;
			}
			[CompilerGenerated]
			set
			{
				Float_1 = value;
			}
		}

		public abstract void Update();

		public virtual EUseSide vmethod_0(out EKeyPress keysStatus)
		{
			keysStatus = EKeyPress.Hold;
			int num = 0;
			while (true)
			{
				if (num < AxisCombination.KeyValuePair_0.Length)
				{
					keysStatus = EKeyPress.Hold;
					if (AxisCombination.KeyValuePair_0[num].Value != null && AxisCombination.KeyValuePair_0[num].Value.Length != 0)
					{
						for (int i = 0; i < AxisCombination.KeyValuePair_0[num].Value.Length; i++)
						{
							if ((AxisCombination.KeyValuePair_0[num].Value[i].InUseBy == null || !AxisCombination.IsLowerPriority(AxisCombination.KeyValuePair_0[num].Value[i].InUseBy)) && keysStatus != EKeyPress.None)
							{
								EKeyPress eKeyPress = EKeyPress.None;
								eKeyPress = (AxisCombination.KeyValuePair_0[num].Value[i].IsAxis ? ((AxisCombination.KeyValuePair_0[num].Value[i].GetValue() > 0f) ? EKeyPress.Hold : EKeyPress.None) : AxisCombination.KeyValuePair_0[num].Value[i].Press);
								keysStatus = GClass2408.MergeMatrix[(int)eKeyPress, (int)keysStatus];
								continue;
							}
							keysStatus = EKeyPress.None;
							break;
						}
						if (keysStatus != EKeyPress.None)
						{
							break;
						}
					}
					num++;
					continue;
				}
				return EUseSide.None;
			}
			return AxisCombination.KeyValuePair_0[num].Key;
		}

		public float method_0(EUseSide side)
		{
			if (side == EUseSide.None)
			{
				return 0f;
			}
			float calculationValue = 1000f;
			method_1((side == EUseSide.Positive) ? AxisCombination.KeyValuePair_0[AxisCombination.Int_1].Value : AxisCombination.KeyValuePair_0[(AxisCombination.Int_1 + 1) % 2].Value, ref calculationValue);
			if (calculationValue != 0f)
			{
				method_1(AxisCombination.Ginterface240_1, ref calculationValue);
				if (side != EUseSide.Positive)
				{
					return 0f - calculationValue;
				}
				return calculationValue;
			}
			return 0f;
		}

		public void method_1(GInterface240[] axisGroup, ref float calculationValue)
		{
			float num = 0f;
			bool flag = false;
			for (int i = 0; i < axisGroup.Length; i++)
			{
				if (axisGroup[i].IsAxis)
				{
					calculationValue = axisGroup[i].GetValue();
					if (calculationValue == 0f)
					{
						break;
					}
					flag = true;
					continue;
				}
				num = axisGroup[i].GetValue();
				if (num == 0f)
				{
					calculationValue = 0f;
				}
				else if (!flag && calculationValue > num)
				{
					calculationValue = num;
				}
			}
		}

		public Class1862(GClass2409 axisCombination)
		{
			AxisCombination = axisCombination;
		}
	}

	public class Class1863 : Class1862
	{
		public override void Update()
		{
			EKeyPress keysStatus;
			EUseSide eUseSide = vmethod_0(out keysStatus);
			if (eUseSide == EUseSide.None)
			{
				base.Value = 0f;
			}
			else
			{
				base.Value = method_0(eUseSide);
			}
		}

		public Class1863(GClass2409 axisCombination)
			: base(axisCombination)
		{
		}
	}

	public abstract class Class1864 : Class1862
	{
		public Class1864(GClass2409 axisCombination)
			: base(axisCombination)
		{
		}

		public override EUseSide vmethod_0(out EKeyPress keysStatus)
		{
			EUseSide eUseSide = base.vmethod_0(out keysStatus);
			if (eUseSide == EUseSide.None)
			{
				return eUseSide;
			}
			int num = 0;
			while (true)
			{
				if (num < AxisCombination.Ginterface240_1.Length)
				{
					if (AxisCombination.Ginterface240_1[num].InUseBy != null && AxisCombination.IsLowerPriority(AxisCombination.Ginterface240_1[num].InUseBy))
					{
						break;
					}
					keysStatus = GClass2408.MergeMatrix[(int)keysStatus, (int)AxisCombination.Ginterface240_1[num].Press];
					num++;
					continue;
				}
				int num2 = 0;
				while (true)
				{
					if (num2 < AxisCombination.Ginterface240_0.Length)
					{
						if (AxisCombination.Ginterface240_0[num2].InUseBy != null && AxisCombination.IsLowerPriority(AxisCombination.Ginterface240_0[num2].InUseBy))
						{
							break;
						}
						keysStatus = GClass2408.MergeMatrix[(int)keysStatus, (int)AxisCombination.Ginterface240_0[num2].Press];
						num2++;
						continue;
					}
					return eUseSide;
				}
				keysStatus = EKeyPress.None;
				return EUseSide.None;
			}
			keysStatus = EKeyPress.None;
			return EUseSide.None;
		}

		public void method_2(EUseSide useSide)
		{
			for (int i = 0; i < AxisCombination.Ginterface240_1.Length; i++)
			{
				AxisCombination.Ginterface240_1[i].InUseBy = AxisCombination;
			}
			GInterface240[] array = ((useSide == EUseSide.Positive) ? AxisCombination.KeyValuePair_0[AxisCombination.Int_1].Value : AxisCombination.KeyValuePair_0[(AxisCombination.Int_1 + 1) % 2].Value);
			for (int j = 0; j < array.Length; j++)
			{
				array[j].InUseBy = AxisCombination;
			}
		}

		public void method_3()
		{
			for (int i = 0; i < AxisCombination.Ginterface240_1.Length; i++)
			{
				if (AxisCombination.Ginterface240_1[i].InUseBy == AxisCombination)
				{
					AxisCombination.Ginterface240_1[i].InUseBy = null;
				}
			}
			for (int j = 0; j < AxisCombination.KeyValuePair_0.Length; j++)
			{
				for (int k = 0; k < AxisCombination.KeyValuePair_0[j].Value.Length; k++)
				{
					if (AxisCombination.KeyValuePair_0[j].Value[k].InUseBy == AxisCombination)
					{
						AxisCombination.KeyValuePair_0[j].Value[k].InUseBy = null;
					}
				}
			}
		}
	}

	public class Class1865 : Class1864
	{
		public override void Update()
		{
			EKeyPress keysStatus;
			EUseSide eUseSide = vmethod_0(out keysStatus);
			if (eUseSide != EUseSide.None && (keysStatus == EKeyPress.Down || keysStatus == EKeyPress.Hold))
			{
				method_2(eUseSide);
				AxisCombination.method_0(EAxisState.WithKeysPressed);
			}
			base.Value = 0f;
		}

		public Class1865(GClass2409 axisCombination)
			: base(axisCombination)
		{
		}
	}

	public class Class1866 : Class1864
	{
		public override void Update()
		{
			EKeyPress keysStatus;
			EUseSide eUseSide = vmethod_0(out keysStatus);
			if (eUseSide != EUseSide.None && keysStatus != EKeyPress.Up && keysStatus != EKeyPress.None)
			{
				method_2(eUseSide);
				base.Value = method_0(eUseSide);
			}
			else
			{
				method_3();
				AxisCombination.method_0(EAxisState.WithKeysIdling);
				base.Value = 0f;
			}
		}

		public Class1866(GClass2409 axisCombination)
			: base(axisCombination)
		{
		}
	}

	public readonly EAxis GameAxis;

	public readonly int IntAxis;

	public readonly EGameKey Modificator;

	[NonSerialized]
	public GInterface240[] Ginterface240_0 = new GClass2412[0];

	[NonSerialized]
	public GInterface240[] Ginterface240_1 = new GClass2412[0];

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public KeyValuePair<EUseSide, GInterface240[]>[] KeyValuePair_0 = new KeyValuePair<EUseSide, GInterface240[]>[2];

	[NonSerialized]
	public Class1862[] Class1862_0;

	[NonSerialized]
	public Class1862 Class1862_1;

	public void UpdateInput(GInterface240[] positiveAxis, GInterface240[] negativeAxis, List<GInterface240> keys, List<GInterface240> modificator)
	{
		Int_1 = ((positiveAxis.Length < negativeAxis.Length) ? 1 : 0);
		KeyValuePair_0[Int_1] = new KeyValuePair<EUseSide, GInterface240[]>(EUseSide.Positive, positiveAxis);
		KeyValuePair_0[(Int_1 + 1) % 2] = new KeyValuePair<EUseSide, GInterface240[]>(EUseSide.Negative, negativeAxis);
		Ginterface240_1 = keys.ToArray();
		Ginterface240_0 = modificator.ToArray();
		if (Ginterface240_1.Length == 0 && Ginterface240_0.Length == 0)
		{
			method_0(EAxisState.JustAxis);
		}
		else
		{
			method_0(EAxisState.WithKeysIdling);
		}
	}

	public override int GetInputCount()
	{
		return Ginterface240_0.Length + Ginterface240_1.Length + 1;
	}

	public override ECommand UpdateCommand(float deltaTime)
	{
		Class1862_1.Update();
		return ECommand.None;
	}

	public override bool IsLowerPriority(GClass2408 combination)
	{
		if (combination == this)
		{
			return false;
		}
		if (combination is KeyBindingClass keyBindingClass && (keyBindingClass.GameKey == Modificator || keyBindingClass.GameKey == EGameKey.BlindShootAbove || keyBindingClass.GameKey == EGameKey.BlindShootRight))
		{
			return false;
		}
		return base.IsLowerPriority(combination);
	}

	public float GetValue()
	{
		return Class1862_1.Value;
	}

	public GClass2409 Clone()
	{
		return new GClass2409(GameAxis, Modificator);
	}

	public GClass2409(EAxis gameAxis)
	{
		GameAxis = gameAxis;
		IntAxis = (int)GameAxis;
		Class1862_0 = new Class1862[Enum.GetValues(typeof(EAxisState)).Length];
		Class1862_0[0] = new Class1863(this);
		Class1862_0[1] = new Class1865(this);
		Class1862_0[2] = new Class1866(this);
		Class1862_1 = Class1862_0[0];
	}

	public GClass2409(EAxis gameAxis, EGameKey modificator)
		: this(gameAxis)
	{
		Modificator = modificator;
	}

	public void method_0(EAxisState state)
	{
		Class1862_1 = Class1862_0[(int)state];
	}
}
