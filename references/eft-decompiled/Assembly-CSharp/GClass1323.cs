using System;
using System.Runtime.CompilerServices;

public abstract class GClass1323
{
	[Serializable]
	[CompilerGenerated]
	public class Class905
	{
		public static readonly Class905 class905_0 = new Class905();

		public static Comparison<GClass1315> comparison_0;

		public static Comparison<CurrentStateAbstractClass> comparison_1;

		public static Comparison<TransitionClass> comparison_2;

		public int method_0(GClass1315 m1, GClass1315 m2)
		{
			return string.CompareOrdinal(m1.Name, m2.Name);
		}

		public int method_1(CurrentStateAbstractClass s1, CurrentStateAbstractClass s2)
		{
			return string.CompareOrdinal(s1.Name, s2.Name);
		}

		public int method_2(TransitionClass t1, TransitionClass t2)
		{
			return string.CompareOrdinal(t1.DestinationState.Name, t2.DestinationState.Name);
		}
	}

	public static void AddParameter(this FastAnimatorControllerClass controllerData, SpeedAnimParamClass parameter)
	{
		controllerData.List_0.Add(parameter);
		parameter.FastAnimatorControllerClass = controllerData;
	}

	public static void RemoveParameter(this FastAnimatorControllerClass controllerData, SpeedAnimParamClass parameter)
	{
		controllerData.List_0.Remove(parameter);
		parameter.FastAnimatorControllerClass = null;
	}

	public static void AddLayer(this FastAnimatorControllerClass controllerData, GClass1316 layerStateMachine)
	{
		controllerData.List_1.Add(layerStateMachine);
		layerStateMachine.FastAnimatorControllerClass = controllerData;
	}

	public static void RemoveLayer(this FastAnimatorControllerClass controllerData, GClass1316 layerStateMachine)
	{
		controllerData.List_1.Remove(layerStateMachine);
		layerStateMachine.FastAnimatorControllerClass = null;
	}

	public static void AddStateMachine(this GClass1315 parentInnerStateMachine, GClass1317 childInnerStateMachine)
	{
		if (smethod_0(parentInnerStateMachine, childInnerStateMachine))
		{
			parentInnerStateMachine.List_0.Add(childInnerStateMachine);
			parentInnerStateMachine.List_0.Sort((GClass1315 m1, GClass1315 m2) => string.CompareOrdinal(m1.Name, m2.Name));
			childInnerStateMachine.Gclass1315_0 = parentInnerStateMachine;
		}
	}

	public static bool smethod_0(GClass1315 parent, GClass1317 child)
	{
		return true;
	}

	public static void RemoveStateMachine(this GClass1315 innerStateMachine, GClass1317 layerStateMachine)
	{
		innerStateMachine.List_0.Remove(layerStateMachine);
		layerStateMachine.Gclass1315_0 = null;
	}

	public static void AddState(this GClass1315 stateMachine, CurrentStateAbstractClass state)
	{
		stateMachine.List_1.Add(state);
		state.Gclass1313_0 = stateMachine;
	}

	public static void SortStates(this GClass1315 stateMachine)
	{
		stateMachine.List_1.Sort((CurrentStateAbstractClass s1, CurrentStateAbstractClass s2) => string.CompareOrdinal(s1.Name, s2.Name));
	}

	public static void RemoveState(this GClass1315 stateMachine, CurrentStateAbstractClass state)
	{
		stateMachine.List_1.Remove(state);
		state.Gclass1313_0 = null;
	}

	public static void AddTransition(this CurrentStateAbstractClass stateMachine, TransitionClass transition)
	{
		stateMachine.List_0.Add(transition);
		if (transition.HasExitTime)
		{
			stateMachine._outTransitions__WITH_EXIT_TIME.Add(transition);
		}
		else
		{
			stateMachine._outTransitions__NO_EXIT_TIME.Add(transition);
		}
		for (int i = 0; i < stateMachine.List_0.Count; i++)
		{
			stateMachine.List_0[i].OrderIndex = i;
		}
	}

	public static void SortTransitions(this CurrentStateAbstractClass stateMachine)
	{
		stateMachine.List_0.Sort((TransitionClass t1, TransitionClass t2) => string.CompareOrdinal(t1.DestinationState.Name, t2.DestinationState.Name));
	}

	public static void RemoveTransition(this CurrentStateAbstractClass stateMachine, TransitionClass transition)
	{
		stateMachine.List_0.Remove(transition);
		if (transition.HasExitTime)
		{
			stateMachine._outTransitions__WITH_EXIT_TIME.Remove(transition);
		}
		else
		{
			stateMachine._outTransitions__NO_EXIT_TIME.Remove(transition);
		}
	}

	public static void AddBehavior(this GClass1312 abstractState, GClass1328 behavior)
	{
		int behaviorsCount = abstractState.BehaviorsCount;
		Array.Resize(ref abstractState.Gclass1328_0, behaviorsCount + 1);
		abstractState.Gclass1328_0[behaviorsCount] = behavior;
	}
}
