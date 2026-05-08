using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InputSystem;

public class GClass2407 : GInterface240, GInterface241
{
	[NonSerialized]
	public static Dictionary<string, GClass2407> Dictionary_0 = new Dictionary<string, GClass2407>();

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public GInterface241 Ginterface241_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass2408 Gclass2408_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public EKeyPress EkeyPress_0;

	public bool IsAxis => true;

	public GClass2408 InUseBy
	{
		[CompilerGenerated]
		get
		{
			return Gclass2408_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass2408_0 = value;
		}
	}

	public bool ReleaseNextFrame
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public EKeyPress Press
	{
		[CompilerGenerated]
		get
		{
			return EkeyPress_0;
		}
		[CompilerGenerated]
		set
		{
			EkeyPress_0 = value;
		}
	}

	public static GClass2407 GetAxis(string axisName, bool positiveAxis, float deadZone, float sensitivity)
	{
		string key = axisName + (positiveAxis ? "+" : "-");
		if (!Dictionary_0.TryGetValue(key, out var value))
		{
			value = new GClass2407(axisName, positiveAxis ? 1 : (-1), deadZone, sensitivity);
			Dictionary_0.Add(key, value);
		}
		value.method_0(axisName);
		return value;
	}

	public override string ToString()
	{
		return String_0 + ((Float_2 > 0f) ? "+" : "-");
	}

	public void Update()
	{
		int num = ((Ginterface241_0.GetValue() == Float_2) ? 1 : 0);
		Press = InputManager.UpdateInputMatrix[num, (int)Press];
	}

	public override int GetHashCode()
	{
		int hashCode = String_0.GetHashCode();
		float float_ = Float_2;
		return hashCode + float_.GetHashCode();
	}

	public float GetValue()
	{
		float value = Ginterface241_0.GetValue();
		if (GamePlayerOwner.IgnoreInputInNPCDialog && GamePlayerOwner.MyPlayer.IsResettingLook)
		{
			return 0f;
		}
		if (!(Float_2 * value > Float_2 * Float_0))
		{
			return 0f;
		}
		return Float_1 * (Float_2 * value - Float_0) / (1f - Float_0);
	}

	public GInterface241 GetActualAxis()
	{
		return Ginterface241_0;
	}

	public void method_0(string axisName)
	{
		Ginterface241_0 = GClass2405.GetAxisUpdater(axisName);
	}

	public GClass2407(string axisName, float sign, float deadZone, float sensitivity)
	{
		String_0 = axisName;
		Float_2 = sign;
		Float_0 = deadZone;
		Float_1 = sensitivity;
	}
}
