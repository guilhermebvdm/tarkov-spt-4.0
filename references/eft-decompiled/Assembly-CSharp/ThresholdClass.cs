using System;
using System.Globalization;

public class ThresholdClass : IDisposable
{
	public class GException9 : Exception
	{
		public GException9()
		{
		}

		public GException9(string message)
			: base(message)
		{
		}
	}

	public enum EAnimatorValueType
	{
		Undefined = -1,
		Int,
		Float,
		Boolean,
		Trigger
	}

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public int Int_0;

	public readonly EAnimatorValueType ValueType;

	public ThresholdClass(EAnimatorValueType valueType)
	{
		ValueType = valueType;
	}

	public float GetFloatValue()
	{
		if (ValueType == EAnimatorValueType.Float)
		{
			return Float_0;
		}
		return method_0();
	}

	public int GetIntValue()
	{
		if (ValueType == EAnimatorValueType.Int)
		{
			return Int_0;
		}
		return method_1();
	}

	public bool GetBoolValue()
	{
		if (ValueType != EAnimatorValueType.Boolean && ValueType != EAnimatorValueType.Trigger)
		{
			return method_2();
		}
		return Bool_0;
	}

	public string GetStringValue()
	{
		switch (ValueType)
		{
		default:
			throw new GException9($"Can't create delegate for {ValueType} type");
		case EAnimatorValueType.Int:
			return GetIntValue().ToString(CultureInfo.InvariantCulture);
		case EAnimatorValueType.Float:
			return GetFloatValue().ToString(CultureInfo.InvariantCulture);
		case EAnimatorValueType.Boolean:
		case EAnimatorValueType.Trigger:
			return GetBoolValue().ToString(CultureInfo.InvariantCulture);
		}
	}

	public void SetValue(float value)
	{
		if (ValueType == EAnimatorValueType.Float)
		{
			if (Math.Abs(Float_0 - value) >= float.Epsilon)
			{
				Float_0 = value;
				ValueChanged();
			}
		}
		else
		{
			method_3(value);
		}
	}

	public void SetValue(int value)
	{
		if (ValueType == EAnimatorValueType.Int)
		{
			if (Int_0 != value)
			{
				Int_0 = value;
				ValueChanged();
			}
		}
		else
		{
			method_4(value);
		}
	}

	public void SetValue(bool value)
	{
		if (ValueType != EAnimatorValueType.Boolean && ValueType != EAnimatorValueType.Trigger)
		{
			method_5(value);
		}
		else if (Bool_0 != value)
		{
			Bool_0 = value;
			ValueChanged();
		}
	}

	public void SetValue(string value)
	{
		switch (ValueType)
		{
		default:
			throw new GException9($"Can't create delegate for {ValueType} type");
		case EAnimatorValueType.Int:
			SetValue(int.Parse(value));
			break;
		case EAnimatorValueType.Float:
			SetValue(float.Parse(value, CultureInfo.InvariantCulture));
			break;
		case EAnimatorValueType.Boolean:
		case EAnimatorValueType.Trigger:
			SetValue(bool.Parse(value));
			break;
		}
	}

	public virtual void ValueChanged()
	{
	}

	public float method_0()
	{
		return 0f;
	}

	public int method_1()
	{
		return 0;
	}

	public bool method_2()
	{
		return false;
	}

	public void method_3(float value)
	{
	}

	public void method_4(int value)
	{
	}

	public void method_5(bool value)
	{
	}

	public override string ToString()
	{
		return $"{ValueType}:'{GetStringValue()}'";
	}

	public virtual void Dispose(bool disposing)
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
