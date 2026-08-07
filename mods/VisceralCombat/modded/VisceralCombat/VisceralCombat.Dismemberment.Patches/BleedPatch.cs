using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using VisceralCombat.Dismemberment.Classes;

namespace VisceralCombat.Dismemberment.Patches;

public class BleedPatch : ModulePatch
{
	[CompilerGenerated]
	private sealed class _003CWatchShot_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public EftBulletClass shot;

		private AmmoItemClass _003CbulletClass_003E5__1;

		private EPointOfView? _003Cpov_003E5__2;

		private Collider _003Ccol_003E5__3;

		private GameObject _003CcolGO_003E5__4;

		private int _003CcolLayer_003E5__5;

		private int _003CplayerLayer_003E5__6;

		private int _003ChitColliderLayer_003E5__7;

		private int _003CdeadBodyLayer_003E5__8;

		private bool _003CisValidLayer_003E5__9;

		private Player _003Cplayer_003E5__10;

		private List<BodyRendererDataStruct> _003Crenderers_003E5__11;

		private string _003Ccaliber_003E5__12;

		private float _003Cchance_003E5__13;

		private float _003CrandomTime_003E5__14;

		private int _003CdiceRoll_003E5__15;

		private int _003CbloodIndex_003E5__16;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CWatchShot_003Ed__5(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CbulletClass_003E5__1 = null;
			_003Ccol_003E5__3 = null;
			_003CcolGO_003E5__4 = null;
			_003Cplayer_003E5__10 = null;
			_003Crenderers_003E5__11 = null;
			_003Ccaliber_003E5__12 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (!shot.IsShotFinished)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			if (!((Object)(object)shot.HitCollider == (Object)null))
			{
				Item ammo = shot.Ammo;
				_003CbulletClass_003E5__1 = (AmmoItemClass)(object)((ammo is AmmoItemClass) ? ammo : null);
				if (_003CbulletClass_003E5__1 != null)
				{
					IPlayerOwner player = shot.Player;
					EPointOfView? obj;
					if (player == null)
					{
						obj = null;
					}
					else
					{
						IPlayer iPlayer = player.iPlayer;
						if (iPlayer == null)
						{
							obj = null;
						}
						else
						{
							PlayerBones playerBones = iPlayer.PlayerBones;
							if (playerBones == null)
							{
								obj = null;
							}
							else
							{
								Player player2 = playerBones.Player;
								obj = ((player2 != null) ? new EPointOfView?(player2.PointOfView) : ((EPointOfView?)null));
							}
						}
					}
					_003Cpov_003E5__2 = obj;
					if (_003Cpov_003E5__2 != (EPointOfView?)0)
					{
						return false;
					}
					_003Ccol_003E5__3 = shot.HitCollider;
					_003CcolGO_003E5__4 = ((Component)_003Ccol_003E5__3).gameObject;
					if ((Object)(object)_003CcolGO_003E5__4.GetComponent<ObservedLootItem>() != (Object)null)
					{
						return false;
					}
					_003CcolLayer_003E5__5 = _003CcolGO_003E5__4.layer;
					_003CplayerLayer_003E5__6 = LayerMask.NameToLayer("Player");
					_003ChitColliderLayer_003E5__7 = LayerMask.NameToLayer("HitCollider");
					_003CdeadBodyLayer_003E5__8 = LayerMask.NameToLayer("Deadbody");
					_003CisValidLayer_003E5__9 = _003CcolLayer_003E5__5 == _003CplayerLayer_003E5__6 || _003CcolLayer_003E5__5 == _003ChitColliderLayer_003E5__7 || _003CcolLayer_003E5__5 == _003CdeadBodyLayer_003E5__8;
					if (!_003CisValidLayer_003E5__9)
					{
						if (Utils.CheckNameInHierarchyRecursive(_003CcolGO_003E5__4, "generated") || Utils.CheckNameInHierarchyRecursive(_003CcolGO_003E5__4, "weapon"))
						{
							return false;
						}
						return false;
					}
					_003Cplayer_003E5__10 = Utils.GetComponentInParentRecursive<Player>(_003CcolGO_003E5__4);
					if ((Object)(object)_003Cplayer_003E5__10 != (Object)null)
					{
						_003Crenderers_003E5__11 = Traverse.Create((object)_003Cplayer_003E5__10).Field<List<BodyRendererDataStruct>>("_preAllocatedRenderersList").Value;
						if (_003Crenderers_003E5__11 != null && _003Crenderers_003E5__11.Count > 0)
						{
							Singleton<Effects>.Instance.PlayerMeshesHit(_003Crenderers_003E5__11, shot.HitPoint, -shot.HitNormal);
						}
						_003Ccaliber_003E5__12 = _003CbulletClass_003E5__1.Caliber;
						if (!calibers.TryGetValue(_003Ccaliber_003E5__12, out _003Cchance_003E5__13))
						{
							return false;
						}
						if (Random.value < _003Cchance_003E5__13)
						{
							_003CrandomTime_003E5__14 = Random.Range(1f, 5f);
							_003CdiceRoll_003E5__15 = Random.Range(0, 10);
							if (light_calibers.Contains(_003Ccaliber_003E5__12))
							{
								HitEffect(_003Cplayer_003E5__10, _003Ccol_003E5__3, shot, _003Cplayer_003E5__10.HealthController.IsAlive, _003CrandomTime_003E5__14, 18);
							}
							else if (heavy_calibers.Contains(_003Ccaliber_003E5__12))
							{
								HitEffect(_003Cplayer_003E5__10, _003Ccol_003E5__3, shot, _003Cplayer_003E5__10.HealthController.IsAlive, _003CrandomTime_003E5__14, 17);
							}
							_003CbloodIndex_003E5__16 = Random.Range(0, 10);
							BleedEffect(_003Ccol_003E5__3, shot, _003Cplayer_003E5__10.HealthController.IsAlive, _003CdiceRoll_003E5__15, _003CrandomTime_003E5__14, (_003CbloodIndex_003E5__16 <= 7) ? 10 : 11);
						}
						_003Crenderers_003E5__11 = null;
						_003Ccaliber_003E5__12 = null;
					}
					return false;
				}
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static Dictionary<string, float> calibers = new Dictionary<string, float>();

	public static List<string> light_calibers = new List<string>();

	public static List<string> heavy_calibers = new List<string>();

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethod("CreateShot", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(BallisticsCalculator __instance, EftBulletClass __result, AmmoItemClass __0, Vector3 __1, Vector3 __2, int __3, string __4, Item __5, float __6, int __7)
	{
		if (VisceralEntry.Instance != null && VisceralEntry.Instance.EnableBloodEffects.Value && __result != null)
		{
			if (__result.IsShotFinished)
			{
				ProcessWatchShot(__result);
			}
			else
			{
				((MonoBehaviour)StaticManager.Instance).StartCoroutine(WatchShot(__result));
			}
		}
	}

	private static void ProcessWatchShot(EftBulletClass shot)
	{
		if (shot == null || shot.HitCollider == null) return;

		Transform rootTransform = shot.HitCollider.transform.root;
		if (rootTransform == null) return;

		Player player = rootTransform.GetComponent<Player>();
		if (player != null && shot.Ammo != null)
		{
			string caliber = (shot.Ammo is AmmoItemClass ammoItem) ? ammoItem.Caliber : string.Empty;
			bool isAlive = player.HealthController != null && player.HealthController.IsAlive;

			if (light_calibers.Contains(caliber))
			{
				HitEffect(player, shot.HitCollider, shot, isAlive, 1f, 18);
			}
			if (heavy_calibers.Contains(caliber))
			{
				HitEffect(player, shot.HitCollider, shot, isAlive, 1f, 17);
			}
		}
	}

	[IteratorStateMachine(typeof(_003CWatchShot_003Ed__5))]
	private static IEnumerator WatchShot(EftBulletClass shot)
	{
		return new _003CWatchShot_003Ed__5(0)
		{
			shot = shot
		};
	}

	public static void HitEffect(Player player, Collider col, EftBulletClass shot, bool isAlive, float time, int bundleIndex)
	{
		GameObject val = null;
		switch (bundleIndex)
		{
		case 18:
			val = VisceralEntry.Instance.effectContainer.lightBleedEffect;
			break;
		case 17:
			val = VisceralEntry.Instance.effectContainer.heavyBleedEffect;
			break;
		default:
		{
			List<GameObject> blood3dFxEffects = VisceralEntry.Instance.effectContainer.blood3dFxEffects;
			if (blood3dFxEffects != null && bundleIndex >= 0 && bundleIndex < blood3dFxEffects.Count)
			{
				val = blood3dFxEffects[bundleIndex];
				break;
			}
			QuickLogger.Log(ELogType.Error, $"HitEffect: bundleIndex {bundleIndex} out of range or blood3dFxEffects not set.");
			return;
		}
		}
		if ((Object)(object)val == (Object)null)
		{
			QuickLogger.Log(ELogType.Error, "HitEffect: bloodParticlesPrefab is null, aborting.");
			return;
		}
		GameObject bloodParticleObject = (GoreObjectPool.Instance != null) 
			? GoreObjectPool.Instance.Spawn(val, shot.HitPoint, Quaternion.LookRotation(-shot.HitNormal), ((Component)col).transform)
			: Object.Instantiate<GameObject>(val);

		if (bloodParticleObject.GetComponent<ParticleFloorPainter>() == null)
		{
			bloodParticleObject.AddComponent<ParticleFloorPainter>();
		}
		bloodParticleObject.transform.SetParent(((Component)col).transform, false);
		bloodParticleObject.transform.position = shot.HitPoint;
		bloodParticleObject.transform.localRotation = Quaternion.LookRotation(-shot.HitNormal);
		ParticleSystem[] componentsInChildren = bloodParticleObject.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem val2 in array)
		{
			ParticleSystem.MainModule main = val2.main;
			main.loop = false;
			main.duration = time;
			ParticleSystem.CollisionModule collision = val2.collision;
			collision.sendCollisionMessages = true;
			if ((Object)(object)((Component)val2).gameObject.GetComponent<ParticleFloorPainter>() == (Object)null)
			{
				((Component)val2).gameObject.AddComponent<ParticleFloorPainter>();
			}
			val2.Play();
		}
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, time + 1f, (Action)delegate
		{
			if (bloodParticleObject != null && Singleton<GameWorld>.Instantiated)
			{
				if (GoreObjectPool.Instance != null)
				{
					GoreObjectPool.Instance.Recycle(bloodParticleObject);
				}
				else
				{
					Object.Destroy((Object)(object)bloodParticleObject);
				}
			}
		});
	}

	public static void BleedEffect(Collider col, EftBulletClass shot, bool isAlive, float chance, float time, int bundleIndex)
	{
		if (chance < 8f)
		{
			return;
		}
		GameObject val = null;
		switch (bundleIndex)
		{
		case 10:
			val = VisceralEntry.Instance.effectContainer.squirtEffect1;
			break;
		case 11:
			val = VisceralEntry.Instance.effectContainer.squirtEffect2;
			break;
		default:
		{
			List<GameObject> blood3dFxEffects = VisceralEntry.Instance.effectContainer.blood3dFxEffects;
			if (blood3dFxEffects != null && bundleIndex >= 0 && bundleIndex < blood3dFxEffects.Count)
			{
				val = blood3dFxEffects[bundleIndex];
				break;
			}
			QuickLogger.Log(ELogType.Error, $"BleedEffect: bundleIndex {bundleIndex} out of range or blood3dFxEffects not set.");
			return;
		}
		}
		if ((Object)(object)val == (Object)null)
		{
			QuickLogger.Log(ELogType.Error, "BleedEffect: bloodSquirtPrefab is null, aborting.");
			return;
		}
		GameObject bloodSquirtObject = (GoreObjectPool.Instance != null)
			? GoreObjectPool.Instance.Spawn(val, shot.HitPoint, Quaternion.LookRotation(-shot.Direction), ((Component)col).transform)
			: Object.Instantiate<GameObject>(val);

		bloodSquirtObject.transform.SetParent(((Component)col).transform, false);
		bloodSquirtObject.transform.position = shot.HitPoint;
		bloodSquirtObject.transform.rotation = Quaternion.LookRotation(-shot.Direction);
		ParticleSystem[] componentsInChildren = bloodSquirtObject.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem val2 in array)
		{
			ParticleSystem.MainModule main = val2.main;
			main.loop = false;
			main.duration = time;
			ParticleSystem.CollisionModule collision = val2.collision;
			collision.sendCollisionMessages = true;
			if ((Object)(object)((Component)val2).gameObject.GetComponent<ParticleFloorPainter>() == (Object)null)
			{
				((Component)val2).gameObject.AddComponent<ParticleFloorPainter>();
			}
			val2.Play();
		}
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, time + 1f, (Action)delegate
		{
			if (bloodSquirtObject != null && Singleton<GameWorld>.Instantiated)
			{
				if (GoreObjectPool.Instance != null)
				{
					GoreObjectPool.Instance.Recycle(bloodSquirtObject);
				}
				else
				{
					Object.Destroy((Object)(object)bloodSquirtObject);
				}
			}
		});
	}
}
