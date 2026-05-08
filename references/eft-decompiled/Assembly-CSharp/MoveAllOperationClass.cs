using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class MoveAllOperationClass : GClass1320
{
	[Serializable]
	[CompilerGenerated]
	public class Class907
	{
		public static readonly Class907 class907_0 = new Class907();

		public static Func<GClass1326, string> func_0;

		public string method_0(GClass1326 x)
		{
			string name = x.Name;
			float duration = x.Duration;
			return (name + ":" + duration).ToString();
		}
	}

	[NonSerialized]
	public List<GClass1326> List_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public bool Bool_5;

	public List<GClass1326> BlendSettings => List_1;

	public override bool Loop
	{
		get
		{
			if (!Bool_4)
			{
				return Bool_5;
			}
			return base.Loop;
		}
	}

	public override float RealMotionDuration
	{
		get
		{
			if (!Bool_4)
			{
				return Float_2;
			}
			return base.RealMotionDuration;
		}
	}

	public MoveAllOperationClass(string name, int hash, float speedMultiplier, SpeedAnimParamClass speedAnimParam, List<TransitionClass> outTransitions, List<GClass1326> blendSettings)
		: base(name, hash, speedMultiplier, speedAnimParam, outTransitions)
	{
		List_1 = blendSettings;
		method_3();
	}

	public MoveAllOperationClass(string name, int hash, float speedMultiplier, SpeedAnimParamClass speedAnimParam, List<GClass1326> blendSettings)
		: base(name, hash, speedMultiplier, speedAnimParam)
	{
		List_1 = blendSettings;
		method_3();
	}

	public void method_3()
	{
		Float_2 = float.MinValue;
		Bool_5 = List_1.Count > 0;
		for (int i = 0; i < List_1.Count; i++)
		{
			GClass1326 gClass = List_1[i];
			Float_2 = Math.Max(Float_2, gClass.Duration);
			Bool_5 &= gClass.IsLooped;
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()}, [{method_4()}]";
	}

	public string method_4()
	{
		if (string.IsNullOrEmpty(String_0))
		{
			string[] value = List_1.Select(delegate(GClass1326 x)
			{
				string name = x.Name;
				float duration = x.Duration;
				return (name + ":" + duration).ToString();
			}).ToArray();
			String_0 = string.Join(", ", value);
		}
		return String_0;
	}

	public override void Dispose(bool disposing)
	{
		if (disposing)
		{
			for (int i = 0; i < List_1.Count; i++)
			{
				List_1[i] = null;
			}
		}
		base.Dispose(disposing);
	}
}
