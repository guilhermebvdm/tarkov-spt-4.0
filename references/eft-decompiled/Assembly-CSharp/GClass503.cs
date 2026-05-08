using System;
using EFT;
using UnityEngine;

public class GClass503(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public float Float_0 = 8f;

	[NonSerialized]
	public float Float_1 = 12f;

	[NonSerialized]
	public float Float_2 = 10f;

	[NonSerialized]
	public float Float_3 = 11f;

	[NonSerialized]
	public float Float_4 = 1f;

	public float PeriodNoOffsetCoef = 0.1f;

	public float PeriodWithOffsettCoef = 2f;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public int Int_0 = 1;

	public void ManualUpdate()
	{
		VertialUpdate();
		if (!(Float_7 > Time.time))
		{
			Bool_0 = !Bool_0;
			float num = (Bool_0 ? PeriodWithOffsettCoef : PeriodNoOffsetCoef) * GClass856.Random(Float_2, Float_3);
			Float_7 = Time.time + num;
			if (Bool_0)
			{
				Int_0 = ((Int_0 <= 0) ? 1 : (-1));
				Float_6 = (float)Int_0 * GClass856.Random(Float_0, Float_1);
				Bool_1 = false;
			}
		}
	}

	public void VertialUpdate()
	{
		if (Bool_1)
		{
			return;
		}
		if (Float_6 > Float_5)
		{
			Float_5 += Time.deltaTime * Float_4;
			if (Float_6 < Float_5)
			{
				Float_6 = Float_5;
				Bool_1 = true;
			}
		}
		else
		{
			Float_5 -= Time.deltaTime * Float_4;
			if (Float_6 > Float_5)
			{
				Float_6 = Float_5;
				Bool_1 = true;
			}
		}
	}

	public float GetVerAng()
	{
		if (BotOwner_0.GetPlayer.MovementContext.PoseLevel < 0.5f)
		{
			return 0f;
		}
		return Float_5;
	}
}
