using System;
using System.Collections.Generic;
using UnityEngine;

public class AICoreAgentClass<T> : GClass32
{
	[NonSerialized]
	public Dictionary<T, BotNodeAbstractClass> Dictionary_0;

	[NonSerialized]
	public global::AICoreStrategyAbstractClass<T> Gclass309_0;

	[NonSerialized]
	public global::AICoreActionResultStruct<T, GClass26> Gstruct8_0;

	[NonSerialized]
	public string String_6;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public global::AICoreLayerClass<T> Gclass35_0;

	[NonSerialized]
	public AICoreControllerClass AICoreControllerClass;

	[NonSerialized]
	public Func<T, BotNodeAbstractClass> Func_0;

	public AICoreAgentClass(AICoreControllerClass aiCoreController, global::AICoreStrategyAbstractClass<T> strategy, Dictionary<T, BotNodeAbstractClass> nodesDictionary, GameObject monoBehObject, string name, Func<T, BotNodeAbstractClass> lazyGetter)
		: base(name)
	{
		Func_0 = lazyGetter;
		AICoreControllerClass = aiCoreController;
		GameObject_0 = monoBehObject;
		Bool_2 = GameObject_0 != null;
		Dictionary_0 = nodesDictionary;
		Gclass309_0 = strategy;
		Gclass309_0.Activate(this);
		Gclass309_0.OnLayerChangedTo += method_6;
		aiCoreController.AddNodeController(this);
	}

	public void method_5(global::AICoreActionResultStruct<T, GClass26> obj)
	{
		method_2(obj.Action.ToString(), obj.Reason, Time.time);
	}

	public override string GetCustomData()
	{
		return Gclass35_0?.GetCustomData();
	}

	public global::AICoreActionResultStruct<T, GClass26> LastResult()
	{
		return Gstruct8_0;
	}

	public override void Update()
	{
		method_10();
		global::AICoreActionResultStruct<T, GClass26>? aICoreActionResultStruct = Gclass309_0.Update(Gstruct8_0);
		if (!aICoreActionResultStruct.HasValue)
		{
			return;
		}
		T action = aICoreActionResultStruct.Value.Action;
		if (Dictionary_0.TryGetValue(action, out var value))
		{
			value.UpdateNodeByMain(Gstruct8_0.Data);
		}
		else
		{
			BotNodeAbstractClass botNodeAbstractClass = Func_0(action);
			if (botNodeAbstractClass != null)
			{
				Dictionary_0.Add(action, botNodeAbstractClass);
				botNodeAbstractClass.UpdateNodeByMain(Gstruct8_0.Data);
			}
		}
		Gstruct8_0 = aICoreActionResultStruct.Value;
	}

	public override void OnDrawGizmos()
	{
	}

	public string GetActiveNodeName()
	{
		return Gstruct8_0.Action.ToString();
	}

	public string GetActiveNodeReason()
	{
		return Gstruct8_0.Reason;
	}

	public void method_6(global::AICoreLayerClass<T> obj)
	{
		if (Gclass35_0 != null)
		{
			method_7(Gclass35_0);
		}
		Gclass35_0 = obj;
		method_3(Gclass35_0.Name());
		method_8(Gclass35_0);
	}

	public void method_7(global::AICoreLayerClass<T> lastActiveLayer)
	{
		lastActiveLayer.OnStartCurDecision -= method_5;
		lastActiveLayer.OnEndCurDecision -= method_9;
	}

	public void method_8(global::AICoreLayerClass<T> lastActiveLayer)
	{
		lastActiveLayer.OnStartCurDecision += method_5;
		lastActiveLayer.OnEndCurDecision += method_9;
	}

	public void method_9(AICoreActionEndStruct obj)
	{
		method_1(obj.Reason);
	}

	public void method_10()
	{
		Gclass309_0.ManualUpdate();
	}

	public void Dispose()
	{
		AICoreControllerClass.RemoveNodeController(this);
		if (Gclass35_0 != null)
		{
			method_7(Gclass35_0);
		}
		if (Gclass309_0 != null)
		{
			Gclass309_0.OnLayerChangedTo -= method_6;
		}
		foreach (KeyValuePair<T, BotNodeAbstractClass> item in Dictionary_0)
		{
			item.Value.Dispose();
		}
	}
}
