using UnityEngine;
using UnityEngine.PlayerLoop;

namespace CustomPlayerLoopSystem;

public abstract class CustomPlayerLoopSystemsInjector
{
	[RuntimeInitializeOnLoadMethod]
	public static void Injection()
	{
		Application.quitting += OnQuit;
		GClass660.InsertAsSubSystem(typeof(EarlyUpdate), StartOfFrame.GetNewSystem(), GClass660.EOrderType.First);
		GClass660.InsertAsSubSystem(typeof(PostLateUpdate), EndOfFrame.GetNewSystem(), GClass660.EOrderType.Last);
		GClass660.InsertAsSubSystem(typeof(EarlyUpdate), FrameCounter.GetNewSystem(), GClass660.EOrderType.First);
		MoveUNetUpdateSystemOnPreUpdate();
		InjectPostUNetUpdateSystem();
		InjectDataProviderSyncUpdate();
		InjectGlobalEventsOnPreUpdate();
		GClass660.InsertAsSubSystem(typeof(FixedUpdate), StartOfFixedUpdate.GetNewSystem(), GClass660.EOrderType.First);
		GClass660.InsertAsSubSystem(typeof(FixedUpdate), EndOfFixedUpdate.GetNewSystem(), GClass660.EOrderType.Last);
		GClass660.InsertAsSubSystem(typeof(Update), StartOfUpdate.GetNewSystem(), GClass660.EOrderType.First);
		GClass660.InsertAsSubSystem(typeof(Update), EndOfUpdate.GetNewSystem(), GClass660.EOrderType.Last);
		GClass660.InsertAsSubSystem(typeof(PostLateUpdate), StartOfPostLateUpdate.GetNewSystem(), GClass660.EOrderType.First);
		GClass660.PrintAllPlayerLoopSystems();
	}

	public static void MoveUNetUpdateSystemOnPreUpdate()
	{
		GClass660.InsertAsSubSystem(typeof(PreUpdate), UNetUpdate.GetNewSystem(), GClass660.EOrderType.Last);
	}

	public static void InjectGlobalEventsOnPreUpdate()
	{
		GClass660.InsertAsSubSystem(typeof(PreUpdate), GlobalEventsClear.GetNewSystem(), GClass660.EOrderType.Last);
		GClass660.InsertAsSubSystem(typeof(PreUpdate), GlobalEventsApply.GetNewSystem(), GClass660.EOrderType.Last);
	}

	public static void InjectDataProviderSyncUpdate()
	{
		GClass660.InsertAsSubSystem(typeof(PreUpdate), DataProviderSyncUpdate.GetNewSystem(), GClass660.EOrderType.Last);
	}

	public static void MoveUNetUpdateSystemOnFixedUpdate()
	{
		GClass660.InsertAsSubSystem(typeof(FixedUpdate), UNetUpdate.GetNewSystem(), GClass660.EOrderType.First);
	}

	public static void InjectPostUNetUpdateSystem()
	{
		GClass660.InsertAsSiblingSystem(typeof(UNetUpdate), PostUNetUpdate.GetNewSystem(), GClass660.EOrderType.Last);
	}

	public static void OnQuit()
	{
		GClass660.ResetGameLoop();
	}
}
