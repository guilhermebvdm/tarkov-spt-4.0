using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Weather;
using UnityEngine;

public class LocalGameRunddansControllerClass : RunddansControllerClass
{
	public LocalGameRunddansControllerClass(BackendConfigSettingsClass.GClass1748 settings, LocationSettingsClass.Location location)
		: base(settings, location)
	{
		method_3();
		method_15(settings.ApplyFrozenEveryMS, CancellationTokenSource_0.Token).HandleExceptions();
	}

	public async Task method_15(int delay, CancellationToken token)
	{
		GameTimerClass gameTimer = Singleton<AbstractGame>.Instance.GameTimer;
		while (true)
		{
			try
			{
				token.ThrowIfCancellationRequested();
				await Task.Delay(delay, token);
			}
			catch
			{
				break;
			}
			if (!(gameTimer.PastTime.TotalSeconds < (double)Gclass1748_0.initialFrozenDelaySec))
			{
				Player myPlayer = GamePlayerOwner.MyPlayer;
				if (!(myPlayer == null) && !myPlayer.IsAI && !myPlayer.AIData.IsInside && !method_0(myPlayer) && !(WeatherController.Instance == null) && !(WeatherController.Instance.WeatherCurve.Rain < Gclass1748_0.rainForFrozen) && !((EFTDateTimeClass.UtcNow - myPlayer.AIData.DrinkTimestamp).TotalSeconds < (double)Gclass1748_0.drunkImmunitySec))
				{
					myPlayer.ActiveHealthController?.TryDoExternalBuff("Buffs_Frostbite");
				}
			}
		}
	}

	public override void OnTriggerStateChanged(EventObject.EState state)
	{
		base.OnTriggerStateChanged(state);
		if (!TransitControllerAbstractClass.Exist<LocalGameTransitControllerClass>(out var transitController))
		{
			Debug.LogError("TransitController not found");
		}
		else
		{
			transitController.UpdateTimers();
		}
	}

	public override void InteractWithEventObject(Player player, InteractPacketStruct packet)
	{
		if (!IsValid<LocalGameTransitControllerClass>(player, out var transitController, out var info) || !info.events)
		{
			return;
		}
		if (!Objects.TryGetValue(packet.objectId, out var _))
		{
			Debug.LogError($"EventObject with id {packet.objectId} not found)");
			return;
		}
		switch (packet.interaction)
		{
		default:
			return;
		case EventObject.EInteraction.Run:
		{
			if (!method_5(player, out var item))
			{
				method_13();
				return;
			}
			if (!method_10(item))
			{
				Debug.LogError("Remove consumable error");
				return;
			}
			break;
		}
		case EventObject.EInteraction.Repair:
			break;
		}
		method_4(packet.objectId, EventObject.EState.Running);
		transitController.EnablePoints(exceptEvents: false);
		transitController.UpdateTimers();
	}
}
