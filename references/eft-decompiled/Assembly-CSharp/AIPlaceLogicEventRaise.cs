using Comfort.Common;
using EFT;

public class AIPlaceLogicEventRaise : AIPlaceInfoLogic
{
	public string EventName = "none";

	private AIPlaceInfo aiplaceInfo_0;

	public bool EnterRaise = true;

	public bool ExitRaise;

	public override void Init(AIPlaceInfo aiPlaceInfo, BotsController botsController)
	{
		base.Init(aiPlaceInfo, botsController);
		aiplaceInfo_0 = aiPlaceInfo;
		aiplaceInfo_0.OnPlayerEnter += method_1;
		aiplaceInfo_0.OnPlayerExit += method_0;
	}

	public void method_0(Player player)
	{
		if (ExitRaise)
		{
			Singleton<BotEventHandler>.Instance.AnyEvent(EventName);
		}
	}

	public void method_1(Player player)
	{
		if (EnterRaise)
		{
			Singleton<BotEventHandler>.Instance.AnyEvent(EventName);
		}
	}

	public override void Dispose()
	{
		if (aiplaceInfo_0 != null)
		{
			aiplaceInfo_0.OnPlayerEnter -= method_1;
			aiplaceInfo_0.OnPlayerExit -= method_0;
		}
		base.Dispose();
	}
}
