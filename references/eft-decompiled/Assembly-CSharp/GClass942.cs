using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using TMPro;
using UnityEngine;

public abstract class GClass942
{
	[Serializable]
	[CompilerGenerated]
	public class Class593
	{
		public static readonly Class593 class593_0 = new Class593();

		public static Func<KeyValuePair<GInterface234, int>, int> func_0;

		public static Func<GInterface234, IEnumerable<IExchangeRequirement>> func_1;

		public static Func<IExchangeRequirement, bool> func_2;

		public static Func<IExchangeRequirement, double> func_3;

		public static Func<KeyValuePair<GInterface234, int>, GInterface234> func_4;

		public static Func<KeyValuePair<GInterface234, int>, GInterface234> func_5;

		public static Func<KeyValuePair<GInterface234, int>, int> func_6;

		public int method_0(KeyValuePair<GInterface234, int> x)
		{
			return x.Key.CurrentItemCount;
		}

		public IEnumerable<IExchangeRequirement> method_1(GInterface234 item)
		{
			return item.Requirements;
		}

		public bool method_2(IExchangeRequirement requirement)
		{
			return requirement.Item is MoneyItemClass;
		}

		public double method_3(IExchangeRequirement m)
		{
			return m.PreciseCount;
		}

		public GInterface234 method_4(KeyValuePair<GInterface234, int> x)
		{
			return x.Key;
		}

		public GInterface234 method_5(KeyValuePair<GInterface234, int> x)
		{
			return x.Key;
		}

		public int method_6(KeyValuePair<GInterface234, int> x)
		{
			return 0;
		}
	}

	[CompilerGenerated]
	public class Class594
	{
		public int newCount;

		public int method_0(KeyValuePair<GInterface234, int> x)
		{
			return newCount;
		}
	}

	public static void RagfairValidation(string newText, TMP_InputField inputField, ref Dictionary<GInterface234, int> offers, Action<bool> setAcceptActive, Action onItemsUpdated)
	{
		bool obj = false;
		bool flag = false;
		if (int.TryParse(newText, out var result))
		{
			int num = ((offers.Count > 0) ? offers.Min((KeyValuePair<GInterface234, int> x) => x.Key.CurrentItemCount) : 0);
			double num2 = (from requirement in offers.Keys.SelectMany((GInterface234 item) => item.Requirements)
				where requirement.Item is MoneyItemClass
				select requirement).Sum((IExchangeRequirement m) => m.PreciseCount);
			if (num2 > 0.0)
			{
				num = (int)Math.Ceiling(Math.Min(num, 2147483647.0 / num2));
			}
			int newCount = Mathf.Clamp(result, 0, num);
			if (newText != newCount.ToString() || newCount != result)
			{
				inputField.text = newCount.ToString();
			}
			offers = offers.ToDictionary((KeyValuePair<GInterface234, int> x) => x.Key, (KeyValuePair<GInterface234, int> x) => newCount);
			flag = true;
		}
		else
		{
			GClass854.LogRagfair("<color=red>Bad character!</color>");
			inputField.text = ((newText.Length <= 1) ? string.Empty : newText.Remove(newText.Length - 1));
		}
		if (int.TryParse(inputField.text, out var result2))
		{
			obj = result2 > 0;
		}
		else
		{
			offers = offers.ToDictionary((KeyValuePair<GInterface234, int> x) => x.Key, (KeyValuePair<GInterface234, int> x) => 0);
			flag = true;
		}
		setAcceptActive?.Invoke(obj);
		if (flag)
		{
			onItemsUpdated?.Invoke();
		}
	}
}
