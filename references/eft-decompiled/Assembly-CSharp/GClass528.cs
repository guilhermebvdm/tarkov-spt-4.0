using System;
using EFT;
using JetBrains.Annotations;

public class GClass528([NotNull] BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public const int Int_0 = 10;

	[NonSerialized]
	public const int Int_1 = 9;

	[NonSerialized]
	public float[] Float_0 = new float[10];

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public float Float_1;

	public bool CheckIsBadVal(float v, float isGood)
	{
		int int_ = Int_2;
		int_ = ((Int_2 < 9) ? (int_ + 1) : 0);
		float num = Float_0[int_];
		Float_1 -= num;
		Float_0[int_] = v;
		Float_1 += v;
		Int_2 = int_;
		return Float_1 < isGood;
	}
}
