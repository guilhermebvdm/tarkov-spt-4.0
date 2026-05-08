using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.GlobalEvents.ArtilleryShellingEcents;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.MovingPlatforms;
using EFT.SynchronizableObjects;
using UnityEngine;

public class BotEventHandler : Singleton<BotEventHandler>
{
	public delegate void GDelegate10(string zoneId, Vector3 position, float radius);

	public delegate void GDelegate11(Vector3 position, float radius, float delay);

	public delegate void GDelegate12(GClass532 data);

	public delegate void GDelegate13(IPlayer player);

	public delegate void GDelegate14(IPlayer player, bool status);

	public delegate void GDelegate15(LootItem item);

	public delegate void GDelegate16(LootItem item);

	public delegate void GDelegate17(Vector3 position, string playerProfileID, bool isSmoke, float smokeRadius, float smokeLifeTime, int throwableId);

	public delegate void GDelegate18(Vector3 position, string playerProfileID);

	public delegate void GDelegate19(Grenade grenade, Vector3 position, Vector3 force, float mass);

	public delegate void GDelegate20(IPlayer killer, IPlayer target);

	public delegate void GDelegate21(IPlayer player, int pose);

	public delegate void GDelegate22(IPlayer player);

	public delegate void GDelegate23(IList<Vector3> lootPositions);

	public class GClass692
	{
		public IPlayer PlayerRequester;

		public EPhraseTrigger phrase;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public bool Bool_1;

		public int UsedTimes;

		public bool CanUse => Bool_0;

		public bool CalcChanceByLoyalty()
		{
			if (Bool_1)
			{
				return Bool_0;
			}
			Bool_1 = true;
			switch (phrase)
			{
			case EPhraseTrigger.Silence:
				Bool_0 = GClass856.IsTrue100(PlayerRequester.Loyalty.ChanceApplySilenceRequest);
				break;
			case EPhraseTrigger.Stop:
				Bool_0 = GClass856.IsTrue100(PlayerRequester.Loyalty.ChanceApplyStopRequest);
				break;
			case EPhraseTrigger.FollowMe:
				Bool_0 = GClass856.IsTrue100(PlayerRequester.Loyalty.ChanceApplyFollowRequest);
				break;
			case EPhraseTrigger.NeedHelp:
				Bool_0 = GClass856.IsTrue100(PlayerRequester.Loyalty.ChanceApplyHelpRequest);
				break;
			case EPhraseTrigger.GetInCover:
				Bool_0 = GClass856.IsTrue100(PlayerRequester.Loyalty.ChanceApplyGetInCoverRequest);
				break;
			case EPhraseTrigger.Spreadout:
				Bool_0 = GClass856.IsTrue100(PlayerRequester.Loyalty.ChanceApplySpreadoutRequest);
				break;
			}
			if (DebugBotData.UseDebugData && DebugBotData.Instance.PhraseRequestAlwaysGood)
			{
				Bool_0 = true;
			}
			return Bool_0;
		}
	}

	public delegate void GDelegate24(GClass692 info);

	public delegate void GDelegate25(DamageInfoStruct damageInfo, Player victim);

	public delegate void GDelegate26(TripwireSynchronizableObject throwWeap, Vector3 position);

	public delegate void GDelegate27(IPlayer player, Vector3 position, float power, AISoundType type);

	public delegate void GDelegate28(TripwireSynchronizableObject throwWear);

	public delegate void GDelegate29(IPlayer player, Vector3 position, AmmoTemplate ammoTemplate);

	public delegate void GDelegate30(string doorId, Vector3 position);

	[Serializable]
	[CompilerGenerated]
	public class Class356
	{
		public static readonly Class356 class356_0 = new Class356();

		public static Action<int, WorldInteractiveObject> action_0;

		public void method_0(int i, WorldInteractiveObject o)
		{
		}
	}

	[CompilerGenerated]
	private GDelegate27 gdelegate27_0;

	[CompilerGenerated]
	private GDelegate20 gdelegate20_0;

	[CompilerGenerated]
	private GDelegate14 gdelegate14_0;

	[CompilerGenerated]
	private GDelegate19 gdelegate19_0;

	[CompilerGenerated]
	private GDelegate13 gdelegate13_0;

	[CompilerGenerated]
	private GDelegate17 gdelegate17_0;

	[CompilerGenerated]
	private GDelegate18 gdelegate18_0;

	[CompilerGenerated]
	private Action<Vector3> action_0;

	[CompilerGenerated]
	private Action<Locomotive.ETravelState> action_1;

	[CompilerGenerated]
	private GDelegate24 gdelegate24_0;

	[CompilerGenerated]
	private GDelegate29 gdelegate29_0;

	[CompilerGenerated]
	private GDelegate26 gdelegate26_0;

	[CompilerGenerated]
	private GDelegate28 gdelegate28_0;

	[CompilerGenerated]
	private GDelegate25 gdelegate25_0;

	[CompilerGenerated]
	private GDelegate12 gdelegate12_0;

	[CompilerGenerated]
	private GDelegate30 gdelegate30_0;

	[CompilerGenerated]
	private GDelegate10 gdelegate10_0;

	[CompilerGenerated]
	private GDelegate10 gdelegate10_1;

	[CompilerGenerated]
	private GDelegate11 gdelegate11_0;

	[CompilerGenerated]
	private GDelegate21 gdelegate21_0;

	[CompilerGenerated]
	private GDelegate22 gdelegate22_0;

	[CompilerGenerated]
	private GDelegate23 gdelegate23_0;

	[CompilerGenerated]
	private Action<TripwireSynchronizableObject> action_2;

	[CompilerGenerated]
	private Action<string> action_3;

	[CompilerGenerated]
	private Action<string> action_4;

	[CompilerGenerated]
	private Action<int> action_5;

	[CompilerGenerated]
	private Action<int, WorldInteractiveObject> action_6 = delegate
	{
	};

	[CompilerGenerated]
	private Action action_7;

	[CompilerGenerated]
	private Action<Player> action_8;

	[CompilerGenerated]
	private Action<Player> action_9;

	[CompilerGenerated]
	private Action<List<Player>> action_10;

	[CompilerGenerated]
	private Action action_11;

	[CompilerGenerated]
	private Action<Player> action_12;

	public event GDelegate27 OnSoundPlayed
	{
		[CompilerGenerated]
		add
		{
			GDelegate27 gDelegate = gdelegate27_0;
			GDelegate27 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate27 value2 = (GDelegate27)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate27_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate27 gDelegate = gdelegate27_0;
			GDelegate27 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate27 value2 = (GDelegate27)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate27_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate20 OnKill
	{
		[CompilerGenerated]
		add
		{
			GDelegate20 gDelegate = gdelegate20_0;
			GDelegate20 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate20 value2 = (GDelegate20)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate20_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate20 gDelegate = gdelegate20_0;
			GDelegate20 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate20 value2 = (GDelegate20)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate20_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate14 OnHardAimDelegate
	{
		[CompilerGenerated]
		add
		{
			GDelegate14 gDelegate = gdelegate14_0;
			GDelegate14 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate14 value2 = (GDelegate14)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate14_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate14 gDelegate = gdelegate14_0;
			GDelegate14 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate14 value2 = (GDelegate14)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate14_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate19 OnGrenadeThrow
	{
		[CompilerGenerated]
		add
		{
			GDelegate19 gDelegate = gdelegate19_0;
			GDelegate19 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate19 value2 = (GDelegate19)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate19_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate19 gDelegate = gdelegate19_0;
			GDelegate19 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate19 value2 = (GDelegate19)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate19_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate13 OnQETilt
	{
		[CompilerGenerated]
		add
		{
			GDelegate13 gDelegate = gdelegate13_0;
			GDelegate13 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate13 value2 = (GDelegate13)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate13_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate13 gDelegate = gdelegate13_0;
			GDelegate13 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate13 value2 = (GDelegate13)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate13_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate17 OnGrenadeExplosive
	{
		[CompilerGenerated]
		add
		{
			GDelegate17 gDelegate = gdelegate17_0;
			GDelegate17 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate17 value2 = (GDelegate17)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate17_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate17 gDelegate = gdelegate17_0;
			GDelegate17 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate17 value2 = (GDelegate17)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate17_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate18 OnRocketExplosive
	{
		[CompilerGenerated]
		add
		{
			GDelegate18 gDelegate = gdelegate18_0;
			GDelegate18 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate18 value2 = (GDelegate18)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate18_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate18 gDelegate = gdelegate18_0;
			GDelegate18 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate18 value2 = (GDelegate18)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate18_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event Action<Vector3> OnBodyBotDead
	{
		[CompilerGenerated]
		add
		{
			Action<Vector3> action = action_0;
			Action<Vector3> action2;
			do
			{
				action2 = action;
				Action<Vector3> value2 = (Action<Vector3>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Vector3> action = action_0;
			Action<Vector3> action2;
			do
			{
				action2 = action;
				Action<Vector3> value2 = (Action<Vector3>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Locomotive.ETravelState> OnTrainCome
	{
		[CompilerGenerated]
		add
		{
			Action<Locomotive.ETravelState> action = action_1;
			Action<Locomotive.ETravelState> action2;
			do
			{
				action2 = action;
				Action<Locomotive.ETravelState> value2 = (Action<Locomotive.ETravelState>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Locomotive.ETravelState> action = action_1;
			Action<Locomotive.ETravelState> action2;
			do
			{
				action2 = action;
				Action<Locomotive.ETravelState> value2 = (Action<Locomotive.ETravelState>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event GDelegate24 OnPhraseSay
	{
		[CompilerGenerated]
		add
		{
			GDelegate24 gDelegate = gdelegate24_0;
			GDelegate24 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate24 value2 = (GDelegate24)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate24_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate24 gDelegate = gdelegate24_0;
			GDelegate24 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate24 value2 = (GDelegate24)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate24_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate29 OnFlareDelegate
	{
		[CompilerGenerated]
		add
		{
			GDelegate29 gDelegate = gdelegate29_0;
			GDelegate29 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate29 value2 = (GDelegate29)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate29_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate29 gDelegate = gdelegate29_0;
			GDelegate29 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate29 value2 = (GDelegate29)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate29_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate26 OnPlantTripwire
	{
		[CompilerGenerated]
		add
		{
			GDelegate26 gDelegate = gdelegate26_0;
			GDelegate26 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate26 value2 = (GDelegate26)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate26_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate26 gDelegate = gdelegate26_0;
			GDelegate26 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate26 value2 = (GDelegate26)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate26_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate28 OnDeactivateTripwire
	{
		[CompilerGenerated]
		add
		{
			GDelegate28 gDelegate = gdelegate28_0;
			GDelegate28 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate28 value2 = (GDelegate28)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate28_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate28 gDelegate = gdelegate28_0;
			GDelegate28 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate28 value2 = (GDelegate28)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate28_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate25 OnBeingHit
	{
		[CompilerGenerated]
		add
		{
			GDelegate25 gDelegate = gdelegate25_0;
			GDelegate25 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate25 value2 = (GDelegate25)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate25_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate25 gDelegate = gdelegate25_0;
			GDelegate25 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate25 value2 = (GDelegate25)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate25_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate12 OnGestusShow
	{
		[CompilerGenerated]
		add
		{
			GDelegate12 gDelegate = gdelegate12_0;
			GDelegate12 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate12 value2 = (GDelegate12)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate12_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate12 gDelegate = gdelegate12_0;
			GDelegate12 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate12 value2 = (GDelegate12)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate12_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate30 OnInteractObject
	{
		[CompilerGenerated]
		add
		{
			GDelegate30 gDelegate = gdelegate30_0;
			GDelegate30 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate30 value2 = (GDelegate30)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate30_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate30 gDelegate = gdelegate30_0;
			GDelegate30 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate30 value2 = (GDelegate30)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate30_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate10 OnArtilleryStart
	{
		[CompilerGenerated]
		add
		{
			GDelegate10 gDelegate = gdelegate10_0;
			GDelegate10 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate10 value2 = (GDelegate10)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate10_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate10 gDelegate = gdelegate10_0;
			GDelegate10 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate10 value2 = (GDelegate10)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate10_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate10 OnArtilleryEnd
	{
		[CompilerGenerated]
		add
		{
			GDelegate10 gDelegate = gdelegate10_1;
			GDelegate10 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate10 value2 = (GDelegate10)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate10_1, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate10 gDelegate = gdelegate10_1;
			GDelegate10 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate10 value2 = (GDelegate10)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate10_1, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate11 OnArtilleryStartWithTimer
	{
		[CompilerGenerated]
		add
		{
			GDelegate11 gDelegate = gdelegate11_0;
			GDelegate11 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate11 value2 = (GDelegate11)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate11_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate11 gDelegate = gdelegate11_0;
			GDelegate11 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate11 value2 = (GDelegate11)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate11_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate21 OnPoseChanged
	{
		[CompilerGenerated]
		add
		{
			GDelegate21 gDelegate = gdelegate21_0;
			GDelegate21 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate21 value2 = (GDelegate21)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate21_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate21 gDelegate = gdelegate21_0;
			GDelegate21 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate21 value2 = (GDelegate21)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate21_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate22 OnPlayerHealing
	{
		[CompilerGenerated]
		add
		{
			GDelegate22 gDelegate = gdelegate22_0;
			GDelegate22 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate22 value2 = (GDelegate22)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate22_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate22 gDelegate = gdelegate22_0;
			GDelegate22 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate22 value2 = (GDelegate22)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate22_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate23 OnLootSpawn
	{
		[CompilerGenerated]
		add
		{
			GDelegate23 gDelegate = gdelegate23_0;
			GDelegate23 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate23 value2 = (GDelegate23)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate23_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate23 gDelegate = gdelegate23_0;
			GDelegate23 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate23 value2 = (GDelegate23)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate23_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event Action<TripwireSynchronizableObject> OnTripwireChangeState
	{
		[CompilerGenerated]
		add
		{
			Action<TripwireSynchronizableObject> action = action_2;
			Action<TripwireSynchronizableObject> action2;
			do
			{
				action2 = action;
				Action<TripwireSynchronizableObject> value2 = (Action<TripwireSynchronizableObject>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<TripwireSynchronizableObject> action = action_2;
			Action<TripwireSynchronizableObject> action2;
			do
			{
				action2 = action;
				Action<TripwireSynchronizableObject> value2 = (Action<TripwireSynchronizableObject>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<string> OnPlayerComeToPlace
	{
		[CompilerGenerated]
		add
		{
			Action<string> action = action_3;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string> action = action_3;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<string> OnEvent
	{
		[CompilerGenerated]
		add
		{
			Action<string> action = action_4;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<string> action = action_4;
			Action<string> action2;
			do
			{
				action2 = action;
				Action<string> value2 = (Action<string>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int> OnExitActivated
	{
		[CompilerGenerated]
		add
		{
			Action<int> action = action_5;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int> action = action_5;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<int, WorldInteractiveObject> OnDoorInteracted
	{
		[CompilerGenerated]
		add
		{
			Action<int, WorldInteractiveObject> action = action_6;
			Action<int, WorldInteractiveObject> action2;
			do
			{
				action2 = action;
				Action<int, WorldInteractiveObject> value2 = (Action<int, WorldInteractiveObject>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int, WorldInteractiveObject> action = action_6;
			Action<int, WorldInteractiveObject> action2;
			do
			{
				action2 = action;
				Action<int, WorldInteractiveObject> value2 = (Action<int, WorldInteractiveObject>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_6, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnStopBotSpawn
	{
		[CompilerGenerated]
		add
		{
			Action action = action_7;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_7;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_7, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Player> OnApplyLighthouseKeeperFriendlyUsecs
	{
		[CompilerGenerated]
		add
		{
			Action<Player> action = action_8;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_8, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Player> action = action_8;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_8, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Player> OnApplyLighthouseKeeperFriendlyZryachiy
	{
		[CompilerGenerated]
		add
		{
			Action<Player> action = action_9;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_9, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Player> action = action_9;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_9, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<List<Player>> OnApplyTraderServiceBtrSupport
	{
		[CompilerGenerated]
		add
		{
			Action<List<Player>> action = action_10;
			Action<List<Player>> action2;
			do
			{
				action2 = action;
				Action<List<Player>> value2 = (Action<List<Player>>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_10, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<List<Player>> action = action_10;
			Action<List<Player>> action2;
			do
			{
				action2 = action;
				Action<List<Player>> value2 = (Action<List<Player>>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_10, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnStopTraderServiceBtrSupport
	{
		[CompilerGenerated]
		add
		{
			Action action = action_11;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_11, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_11;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_11, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Player> OnInterruptTraderServiceBtrSupportByBetrayer
	{
		[CompilerGenerated]
		add
		{
			Action<Player> action = action_12;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_12, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Player> action = action_12;
			Action<Player> action2;
			do
			{
				action2 = action;
				Action<Player> value2 = (Action<Player>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_12, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public BotEventHandler()
	{
		GlobalEventHandlerClass.Instance.SubscribeOnEvent(delegate(FirstShellingExplosionZoneEvent eventArgs)
		{
			gdelegate10_0?.Invoke(eventArgs.ZoneId, eventArgs.Center, eventArgs.Radius);
		});
		GlobalEventHandlerClass.Instance.SubscribeOnEvent(delegate(LastShellingExplosionZoneEvent eventArgs)
		{
			gdelegate10_1?.Invoke(eventArgs.ZoneId, eventArgs.Center, eventArgs.Radius);
		});
	}

	public void ApplyLighthouseKeeperFriendlyUsecs(Player player)
	{
		action_8?.Invoke(player);
	}

	public void ApplyLighthouseKeeperFriendlyZryachiy(Player player)
	{
		action_9?.Invoke(player);
	}

	public void ApplyTraderServiceBtrSupport(List<Player> players)
	{
		action_10?.Invoke(players);
	}

	public void StopTraderServiceBtrSupport()
	{
		action_11?.Invoke();
	}

	public void InterruptTraderServiceBtrSupportByBetrayer(Player player)
	{
		action_12?.Invoke(player);
	}

	public void StopBotSpawn()
	{
		action_7?.Invoke();
	}

	public void PlayerComeToPlace(string zone)
	{
		action_3?.Invoke(zone);
	}

	public void AnyEvent(string eventId)
	{
		action_4?.Invoke(eventId);
	}

	public void ExitActivated(int eventId)
	{
		action_5?.Invoke(eventId);
	}

	public void ArtilleryStart(Vector3 position, float radius, float delay)
	{
		gdelegate11_0?.Invoke(position, radius, delay);
	}

	public void PlaySound(IPlayer person, Vector3 position, float power, AISoundType type)
	{
		gdelegate27_0?.Invoke(person, position, power, type);
	}

	public void SayPhrase(IPlayer player, EPhraseTrigger @event)
	{
		if (gdelegate24_0 != null)
		{
			GClass692 info = new GClass692
			{
				phrase = @event,
				PlayerRequester = player
			};
			gdelegate24_0(info);
		}
	}

	public void TrainCome(Locomotive.ETravelState val)
	{
		action_1?.Invoke(val);
	}

	public void DeadBodySound(Vector3 pos)
	{
		action_0?.Invoke(pos);
	}

	public void InteractObject(string id, Vector3 pos)
	{
		gdelegate30_0?.Invoke(id, pos);
	}

	public void GrenadeExplosion(Vector3 position, string playerProfileID, bool isSmoke, float smokeRadius, float smokeLifeTime, int throwableId)
	{
		gdelegate17_0?.Invoke(position, playerProfileID, isSmoke, smokeRadius, smokeLifeTime, throwableId);
	}

	public void RocketExplosion(Vector3 position, string playerProfileID)
	{
		gdelegate18_0?.Invoke(position, playerProfileID);
	}

	public void Kill(IPlayer killer, IPlayer target)
	{
		gdelegate20_0?.Invoke(killer, target);
	}

	public void ThrowGrenade(Grenade grenade, Vector3 position, Vector3 force, float mass)
	{
		gdelegate19_0?.Invoke(grenade, position, force, mass);
	}

	public void ShowGesture(Player player, EInteraction gesture)
	{
		GClass532 data = new GClass532
		{
			Gesture = gesture,
			Player = player
		};
		gdelegate12_0?.Invoke(data);
	}

	public void QETilt(IPlayer player)
	{
		gdelegate13_0?.Invoke(player);
	}

	public void HardAim(IPlayer player, bool val)
	{
		gdelegate14_0?.Invoke(player, val);
	}

	public void Stop()
	{
		gdelegate27_0 = null;
		gdelegate20_0 = null;
		gdelegate19_0 = null;
		gdelegate12_0 = null;
		action_0 = null;
		gdelegate24_0 = null;
		gdelegate17_0 = null;
		gdelegate18_0 = null;
		gdelegate30_0 = null;
		action_1 = null;
		gdelegate10_0 = null;
		gdelegate11_0 = null;
		gdelegate10_1 = null;
		action_3 = null;
		action_4 = null;
		action_5 = null;
		gdelegate13_0 = null;
		gdelegate25_0 = null;
		action_8 = null;
		action_9 = null;
		action_10 = null;
		action_11 = null;
		action_12 = null;
	}

	public void BeingHitAction(DamageInfoStruct damageInfo, Player victim)
	{
		gdelegate25_0?.Invoke(damageInfo, victim);
	}

	public void SuccessFlare(Player player, Vector3 position, AmmoTemplate ammoTemplate)
	{
		gdelegate29_0?.Invoke(player, position, ammoTemplate);
	}

	public void PlantTripwire(TripwireSynchronizableObject throwWeap, Vector3 position)
	{
		gdelegate26_0?.Invoke(throwWeap, position);
	}

	public void DeactivateTripwire(TripwireSynchronizableObject throwWeap)
	{
		gdelegate28_0?.Invoke(throwWeap);
	}

	public void TripwireChangeState(TripwireSynchronizableObject tripwireSynchronizableObject)
	{
		action_2?.Invoke(tripwireSynchronizableObject);
	}

	public void PoseChanged(Player player, int poseToInt)
	{
		gdelegate21_0?.Invoke(player, poseToInt);
	}

	public void PlayerHealing(IPlayer player)
	{
		gdelegate22_0?.Invoke(player);
	}

	public void SpawnLoot(List<Vector3> spawnedLootPositions)
	{
		gdelegate23_0?.Invoke(spawnedLootPositions);
	}

	[CompilerGenerated]
	public void method_0(FirstShellingExplosionZoneEvent eventArgs)
	{
		gdelegate10_0?.Invoke(eventArgs.ZoneId, eventArgs.Center, eventArgs.Radius);
	}

	[CompilerGenerated]
	public void method_1(LastShellingExplosionZoneEvent eventArgs)
	{
		gdelegate10_1?.Invoke(eventArgs.ZoneId, eventArgs.Center, eventArgs.Radius);
	}
}
