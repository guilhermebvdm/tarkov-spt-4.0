using System;
using System.Collections.Generic;

public class SpeedAnimParamClass : ThresholdClass
{
	public readonly bool IsControlledByAnimationCurve;

	public readonly string Name;

	public readonly int NameHash;

	public readonly bool DefaultBoolValue;

	public readonly float DefaultFloatValue;

	public readonly int DefaultIntValue;

	[NonSerialized]
	public List<IValueListener> List_0 = new List<IValueListener>();

	[NonSerialized]
	public FastAnimatorControllerClass FastAnimatorControllerClass;

	public SpeedAnimParamClass(string name, EAnimatorValueType valueType, int nameHash, bool isControlledByAnimationCurve, float defaultFloatValue, int defaultIntValue, bool defaultBoolValue)
		: base(valueType)
	{
		Name = name;
		IsControlledByAnimationCurve = isControlledByAnimationCurve;
		NameHash = nameHash;
		DefaultBoolValue = defaultBoolValue;
		DefaultFloatValue = defaultFloatValue;
		DefaultIntValue = defaultIntValue;
	}

	public override string ToString()
	{
		return $"Name: {Name} | {base.ToString()}";
	}

	public void AddValueListener(IValueListener valueListener)
	{
		if (List_0.IndexOf(valueListener) == -1)
		{
			List_0.Add(valueListener);
		}
	}

	public void RemoveValueListener(IValueListener valueListener)
	{
		List_0.Remove(valueListener);
	}

	public override void ValueChanged()
	{
		for (int i = 0; i < List_0.Count; i++)
		{
			List_0[i].OnParameterValueChanged();
		}
	}

	public override void Dispose(bool disposing)
	{
		if (disposing)
		{
			List_0.Clear();
			List_0 = null;
		}
		base.Dispose(disposing);
	}
}
