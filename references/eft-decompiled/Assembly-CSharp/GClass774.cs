using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass774
{
	[CompilerGenerated]
	public class Class437
	{
		public GClass774 gclass774_0;

		public PlayerPhysicalClass.GClass773 consumption;

		public void method_0()
		{
			gclass774_0.RemoveConsumption(consumption);
		}
	}

	public const float EXHAUSTED_THRESHOLD = 15f;

	public float Current;

	public float Overuse;

	public readonly GClass848<float> TotalCapacity;

	public readonly GClass848<float> SelfRestoration;

	public float DisableRestoration;

	public GClass1628<PlayerPhysicalClass.GClass773> Consumptions = new GClass1628<PlayerPhysicalClass.GClass773>();

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1 = 1f;

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Action action_3;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_0;

	public float BuffRestoration
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public float Multiplier
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

	public float NormalValue => Current / (float)TotalCapacity;

	public bool ForceMode
	{
		get
		{
			return Bool_0;
		}
		set
		{
			Bool_0 = value;
			DisableRestoration = 0f;
		}
	}

	public float Single_0 => 1f - NormalValue;

	public bool Exhausted => Current < 15f;

	public event Action OnExpired
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnThresholdPass
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnChanged
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnValueChanged
	{
		[CompilerGenerated]
		add
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_3;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass774(GClass848<float> capacityFunc, GClass848<float> restorationFunc)
	{
		TotalCapacity = capacityFunc;
		SelfRestoration = restorationFunc;
		Consumptions.ItemsChanged.Bind(InvokeChangedAction);
		Current = TotalCapacity;
	}

	public void InvokeChangedAction()
	{
		action_2?.Invoke();
	}

	public float Consume(PlayerPhysicalClass.GClass773 consumption, bool fromDamage = false)
	{
		if (Multiplier <= 0f)
		{
			return Current;
		}
		float result = 0f;
		if (!ForceMode)
		{
			float current = Current;
			float num = consumption.Delta.Value * Multiplier;
			result = Mathf.Max(0f, num - Current);
			Current -= num;
			if (Current < 0f && !fromDamage)
			{
				Overuse -= Current;
			}
			Current = Mathf.Max(Current, 0f);
			if (num > 0f)
			{
				action_3?.Invoke();
				if (!consumption.AllowsRestoration)
				{
					DisableRestoration = Time.time + consumption.Downtime + Single_0;
				}
				if ((Current < 15f) ^ (current < 15f))
				{
					action_1?.Invoke();
				}
				if (Current <= 0f)
				{
					HandleExpiration();
				}
			}
		}
		InvokeChangedAction();
		return result;
	}

	public Action AddConsumption(PlayerPhysicalClass.GClass773 consumption)
	{
		if (!Consumptions.Contains(consumption))
		{
			Consumptions.Add(consumption);
			return delegate
			{
				RemoveConsumption(consumption);
			};
		}
		return null;
	}

	public void RemoveConsumption(PlayerPhysicalClass.GClass773 consumption)
	{
		Consumptions.Remove(consumption);
	}

	public void HandleExpiration()
	{
		action_0?.Invoke();
	}

	public void Process(float dt)
	{
		if (Multiplier <= 0f)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		float current = Current;
		if (!ForceMode)
		{
			for (int i = 0; i < Consumptions.Count; i++)
			{
				PlayerPhysicalClass.GClass773 gClass = Consumptions[i];
				num += (float)gClass.Delta;
				bool flag2 = (float)gClass.Delta > 0f && !gClass.AllowsRestoration;
				flag = flag || flag2;
				if (flag2)
				{
					num2 = Mathf.Max(num2, gClass.Downtime);
				}
			}
		}
		if (flag)
		{
			DisableRestoration = Time.time + num2 + Single_0;
			DisableRestoration = Time.time + num2 + Single_0;
		}
		if (num > 0f)
		{
			Current -= num * dt * Multiplier;
			if (Current < 0f)
			{
				Overuse -= Current;
			}
			Current = Mathf.Max(Current, 0f);
			if (Mathf.Approximately(Current, 0f))
			{
				HandleExpiration();
			}
		}
		GClass848<float> totalCapacity = TotalCapacity;
		if (Current < (float)totalCapacity)
		{
			Current += BuffRestoration * dt;
			if (Time.time > DisableRestoration)
			{
				float num3 = (float)SelfRestoration * dt * Multiplier;
				Current += num3;
				Current = Mathf.Min(Current, totalCapacity);
				Overuse = 0f;
			}
		}
		if (Current > (float)totalCapacity)
		{
			Current = Mathf.Clamp(Current, 0f, totalCapacity);
		}
		if (Int_0 != (int)Current)
		{
			Int_0 = (int)Current;
			action_3?.Invoke();
			if ((Current < 15f) ^ (current < 15f))
			{
				action_1?.Invoke();
			}
		}
	}

	public bool CanAdd(PlayerPhysicalClass.GClass773 consumption)
	{
		if (ForceMode)
		{
			return true;
		}
		if (Current <= consumption.StartThreshold)
		{
			return false;
		}
		if (consumption.Availability != null)
		{
			return consumption.Availability;
		}
		return true;
	}

	public void UpdateStamina(float stamina)
	{
		float current = Current;
		if (!(Math.Abs(current - stamina) < 1f))
		{
			Current = stamina;
			action_3?.Invoke();
			if ((Current < 15f) ^ (current < 15f))
			{
				action_1?.Invoke();
			}
			InvokeChangedAction();
		}
	}
}
