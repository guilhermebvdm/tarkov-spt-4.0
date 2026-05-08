using System;
using System.Runtime.CompilerServices;
using System.Threading;

public abstract class AICoreLayerClass<T>
{
	[NonSerialized]
	[CompilerGenerated]
	public Action<AICoreActionEndStruct> Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public Action<global::AICoreActionResultStruct<T, GClass26>> Action_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public abstract int Priority { get; }

	public bool IsActive
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

	public event Action<AICoreActionEndStruct> OnEndCurDecision
	{
		[CompilerGenerated]
		add
		{
			Action<AICoreActionEndStruct> action = Action_0;
			Action<AICoreActionEndStruct> action2;
			do
			{
				action2 = action;
				Action<AICoreActionEndStruct> value2 = (Action<AICoreActionEndStruct>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<AICoreActionEndStruct> action = Action_0;
			Action<AICoreActionEndStruct> action2;
			do
			{
				action2 = action;
				Action<AICoreActionEndStruct> value2 = (Action<AICoreActionEndStruct>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<global::AICoreActionResultStruct<T, GClass26>> OnStartCurDecision
	{
		[CompilerGenerated]
		add
		{
			Action<global::AICoreActionResultStruct<T, GClass26>> action = Action_1;
			Action<global::AICoreActionResultStruct<T, GClass26>> action2;
			do
			{
				action2 = action;
				Action<global::AICoreActionResultStruct<T, GClass26>> value2 = (Action<global::AICoreActionResultStruct<T, GClass26>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<global::AICoreActionResultStruct<T, GClass26>> action = Action_1;
			Action<global::AICoreActionResultStruct<T, GClass26>> action2;
			do
			{
				action2 = action;
				Action<global::AICoreActionResultStruct<T, GClass26>> value2 = (Action<global::AICoreActionResultStruct<T, GClass26>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref Action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public abstract global::AICoreActionResultStruct<T, GClass26> GetDecision();

	public abstract AICoreActionEndStruct ShallEndCurrentDecision(global::AICoreActionResultStruct<T, GClass26> curDecision);

	public abstract bool ShallUseNow();

	public virtual string GetCustomData()
	{
		return "";
	}

	public global::AICoreActionResultStruct<T, GClass26>? Update(global::AICoreActionResultStruct<T, GClass26>? prevDecision)
	{
		if (!prevDecision.HasValue)
		{
			prevDecision = GetDecision();
			return prevDecision.Value;
		}
		ManualUpdate();
		AICoreActionEndStruct obj = ShallEndCurrentDecision(prevDecision.Value);
		if (obj.Value)
		{
			Action_0?.Invoke(obj);
			global::AICoreActionResultStruct<T, GClass26> decision = GetDecision();
			DecisionChanged(prevDecision, decision);
			prevDecision = decision;
			Action_1?.Invoke(prevDecision.Value);
			return prevDecision.Value;
		}
		return prevDecision;
	}

	public virtual void DecisionChanged(global::AICoreActionResultStruct<T, GClass26>? prevDecision, global::AICoreActionResultStruct<T, GClass26> nextDecision)
	{
	}

	public void Activate()
	{
		IsActive = true;
		OnActivate();
	}

	public virtual void OnActivate()
	{
	}

	public virtual void OnDrawGizmos()
	{
	}

	public abstract string Name();

	public virtual void ManualUpdate()
	{
	}

	public virtual void Dispose()
	{
	}

	public AICoreLayerClass()
	{
	}
}
