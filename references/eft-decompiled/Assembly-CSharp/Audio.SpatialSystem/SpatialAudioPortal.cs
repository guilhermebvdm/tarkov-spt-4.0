using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.GlobalEvents;
using EFT.Interactive;
using UnityEngine;

namespace Audio.SpatialSystem;

[Serializable]
[RequireComponent(typeof(BoxCollider))]
public class SpatialAudioPortal : BaseSpatialAudioPortal, IIdentifiable
{
	public string DoorID;

	private Action _unsubscribeOnDestroy;

	private List<string> _ids = new List<string>();

	public IReadOnlyCollection<string> Ids => _ids;

	public override void Subscribe()
	{
		_unsubscribeOnDestroy = GlobalEventHandlerClass.Instance.SubscribeOnEvent<InteractiveObjectInteractionResultEvent>(method_7);
		base.Subscribe();
	}

	public override async Task Init()
	{
		if (!string.IsNullOrEmpty(DoorID))
		{
			_ids.Add(DoorID);
		}
		await GClass2475.WaitGameStarted();
		await base.Init();
	}

	public override void SyncState()
	{
		if (portalType == PortalType.Opening && Singleton<ClientWorld>.Instantiated)
		{
			WorldInteractiveObject worldInteractiveObject = Singleton<ClientWorld>.Instance.FindDoor(DoorID);
			if (worldInteractiveObject != null && IsPositionIdentical(worldInteractiveObject.transform.position))
			{
				base.State = ((worldInteractiveObject.DoorState != EDoorState.Open) ? PortalState.Closed : PortalState.Open);
			}
		}
		if (portalType == PortalType.Window && Singleton<GameWorld>.Instance.Windows.TryGetByKey(DoorID.GetHashCode(), out var value))
		{
			if (IsPositionIdentical(value.transform.position))
			{
				base.State = ((!value.IsDamaged && !value.BrakeByDefault) ? PortalState.Closed : PortalState.Open);
			}
			WindowBreaker.OnWindowHitAction += method_8;
		}
		SetPortalStatus(state, allowFade: false);
		base.SyncState();
	}

	public void method_7(InteractiveObjectInteractionResultEvent resultEvent)
	{
		if (portalType == PortalType.Opening && DoorID.Equals(resultEvent.ObjectID) && resultEvent.Operatable && IsPositionIdentical(resultEvent.Position))
		{
			if ((state == PortalState.Closed || state == PortalState.InProgressClose) && resultEvent.InteractionResult == EDoorState.Open)
			{
				SetPortalStatus(PortalState.Open, allowFade: true);
			}
			else if ((state != PortalState.Closed || state != PortalState.InProgressClose) && resultEvent.InteractionResult == EDoorState.Shut)
			{
				SetPortalStatus(PortalState.Closed, allowFade: true);
			}
		}
	}

	public void method_8(WindowBreaker breaker, DamageInfoStruct info, WindowBreakingConfig.Crack crack, float damage)
	{
		if (portalType == PortalType.Window && (object)breaker != null && DoorID.Equals(breaker.Id) && IsPositionIdentical(breaker.transform.position) && breaker.IsDamaged)
		{
			SetPortalOpen(allowFade: false);
		}
	}

	public override void Unsubscribe()
	{
		_unsubscribeOnDestroy?.Invoke();
		_unsubscribeOnDestroy = null;
		WindowBreaker.OnWindowHitAction -= method_8;
		base.Unsubscribe();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	public Task method_9()
	{
		return base.Init();
	}
}
