using System;
using EFT;
using UnityEngine;

public class GClass531 : GClass429
{
	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public const int Int_0 = 5;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public bool Bool_0;

	public GClass531(BotOwner botOwner)
		: base(botOwner)
	{
		Bool_0 = GClass398.Instance.IsTraceEnable();
	}

	public void DoCalcCover()
	{
		if (Bool_0)
		{
			float time = Time.time;
			if (time - Float_2 < 1f)
			{
				Int_1++;
				return;
			}
			Int_1 = 0;
			Float_2 = time;
		}
	}
}
