using System;
using System.Collections.Generic;
using EFT;

public class GClass2286 : RunddansControllerClass
{
	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	public Action Action_1;

	public GClass2286(BackendConfigSettingsClass.GClass1748 settings, LocationSettingsClass.Location location)
		: base(settings, location)
	{
		Action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<RunddansStateEvent>(method_16);
		Action_1 = GlobalEventHandlerClass.Instance.SubscribeOnEvent<RunddansMessagesEvent>(method_15);
	}

	public static bool smethod_1(int playerId, out Player player)
	{
		player = GamePlayerOwner.MyPlayer;
		if (playerId == -1)
		{
			return true;
		}
		if (player != null)
		{
			return player.PlayerId == playerId;
		}
		return false;
	}

	public void method_15(RunddansMessagesEvent data)
	{
		if (smethod_1(data.PlayerId, out var _))
		{
			switch (data.Type)
			{
			case RunddansMessagesEvent.EType.NonInteractive:
				method_14();
				break;
			case RunddansMessagesEvent.EType.NoRequiredItem:
				method_13();
				break;
			}
		}
	}

	public void method_16(RunddansStateEvent data)
	{
		if (!smethod_1(data.PlayerId, out var player))
		{
			return;
		}
		foreach (var (objectId, state) in data.Objects)
		{
			method_4(objectId, state);
		}
		player.ForceInteractionsChanged();
	}

	public override void Dispose()
	{
		base.Dispose();
		Action_0();
		Action_1();
	}
}
