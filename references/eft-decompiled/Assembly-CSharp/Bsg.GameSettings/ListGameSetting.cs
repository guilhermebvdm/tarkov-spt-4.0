using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Bsg.GameSettings;

[Serializable]
public class ListGameSetting<TType> : GameSetting<List<TType>>
{
	public delegate bool GDelegate36(TType x, TType y);

	public delegate TType GDelegate37(TType item);

	[Serializable]
	[CompilerGenerated]
	public class Class772
	{
		public static readonly Class772 class772_0 = new Class772();

		public static GDelegate37 gdelegate37_0;

		public TType method_0(TType item)
		{
			return item;
		}
	}

	[NonSerialized]
	public global::OnEventClass<List<TType>> OnListChanged = new global::OnEventClass<List<TType>>();

	[NonSerialized]
	public GDelegate36 EqualityCheck;

	[NonSerialized]
	public GDelegate37 CopyItem;

	[NonSerialized]
	public new List<TType> Value;

	public ListGameSetting(string key, GDelegate37 copyItem = null, GDelegate36 equalityCheck = null, Func<List<TType>, List<TType>> preProcessor = null)
		: base(key, preProcessor)
	{
		Value = new List<TType>();
		CopyItem = copyItem ?? ((GDelegate37)((TType item) => item));
		EqualityCheck = equalityCheck ?? new GDelegate36(EqualityComparer<TType>.Default.Equals);
	}

	public override List<TType> GetValue()
	{
		return Value;
	}

	public override async Task SetValue(List<TType> value)
	{
		Value = await PreProcessValue(value);
		OnListChanged.Invoke(Value);
	}

	public override bool HasSameValue(GameSetting<List<TType>> other)
	{
		List<TType> value = other.Value;
		bool flag = base.Value == null;
		bool flag2 = value == null;
		if (flag || flag2)
		{
			return flag == flag2;
		}
		if (base.Value.Count != value.Count)
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < base.Value.Count)
			{
				if (!EqualityCheck(base.Value[num], value[num]))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	public override void TakeValueFrom(GameSetting<List<TType>> other)
	{
		base.Value.Clear();
		if (other.Value != null)
		{
			foreach (TType item in other.Value)
			{
				base.Value.Add(CopyItem(item));
			}
		}
		ForceApply();
	}

	public override Action Bind(Action<List<TType>> handler)
	{
		return OnListChanged.Bind(handler, base.Value);
	}

	public override Action Subscribe(Action<List<TType>> handler)
	{
		return OnListChanged.Subscribe(handler);
	}

	public override Action BindWithoutValue(IHandler handler)
	{
		throw new Exception("ListGameSetting does not support BindWithoutValue");
	}

	public override void ResetToDefault()
	{
		base.Value.Clear();
		ForceApply();
	}

	public override void ForceApply()
	{
		OnListChanged.Invoke(base.Value);
	}

	public TType[] ToArray()
	{
		return base.Value.ToArray();
	}
}
