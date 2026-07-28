using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Ballistics;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Dismemberment.Classes;
using VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

namespace VisceralCombat.Ragdolls.Patches;

public class LimbKillPatch : ModulePatch
{
	[CompilerGenerated]
	private sealed class _003CWatchShot_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public EftBulletClass shot;

		private Collider _003ChitCollider_003E5__1;

		private AmmoItemClass _003CbulletClass_003E5__2;

		private Rigidbody _003Crb_003E5__3;

		private GameObject _003CrootGO_003E5__4;

		private PuppetMaster _003Cpm_003E5__5;

		private GameObject _003CplayerRoot_003E5__6;

		private Player _003Cplayer_003E5__7;

		private string _003CrbName_003E5__8;

		private Muscle[] _003C_003Es__9;

		private int _003C_003Es__10;

		private Muscle _003Cmuscle_003E5__11;

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
		public _003CWatchShot_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003ChitCollider_003E5__1 = null;
			_003CbulletClass_003E5__2 = null;
			_003Crb_003E5__3 = null;
			_003CrootGO_003E5__4 = null;
			_003Cpm_003E5__5 = null;
			_003CplayerRoot_003E5__6 = null;
			_003Cplayer_003E5__7 = null;
			_003CrbName_003E5__8 = null;
			_003C_003Es__9 = null;
			_003Cmuscle_003E5__11 = null;
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
			_003ChitCollider_003E5__1 = shot.HitCollider;
			if ((Object)(object)_003ChitCollider_003E5__1 == (Object)null)
			{
				return false;
			}
			Item ammo = shot.Ammo;
			_003CbulletClass_003E5__2 = (AmmoItemClass)(object)((ammo is AmmoItemClass) ? ammo : null);
			if (_003CbulletClass_003E5__2 == null)
			{
				return false;
			}
			_003Crb_003E5__3 = _003ChitCollider_003E5__1.attachedRigidbody;
			if ((Object)(object)_003Crb_003E5__3 == (Object)null)
			{
				return false;
			}
			_003CrootGO_003E5__4 = Utils.GetRootGameObject(((Component)_003Crb_003E5__3).gameObject);
			_003Cpm_003E5__5 = _003CrootGO_003E5__4.GetComponentInChildren<PuppetMaster>();
			if ((Object)(object)_003Cpm_003E5__5 == (Object)null || !_003Cpm_003E5__5.initiated)
			{
				return false;
			}
			_003CplayerRoot_003E5__6 = Utils.GetRootGameObject(((Component)_003Cpm_003E5__5).gameObject);
			_003Cplayer_003E5__7 = _003CplayerRoot_003E5__6.GetComponentInChildren<Player>();
			if ((Object)(object)_003Cplayer_003E5__7 == (Object)null || ((GClass3009<GClass3008>)(object)_003Cplayer_003E5__7.ActiveHealthController).IsAlive)
			{
				return false;
			}
			_003CrbName_003E5__8 = ((Object)((Component)_003Crb_003E5__3).gameObject).name;
			_003C_003Es__9 = _003Cpm_003E5__5.muscles;
			for (_003C_003Es__10 = 0; _003C_003Es__10 < _003C_003Es__9.Length; _003C_003Es__10++)
			{
				_003Cmuscle_003E5__11 = _003C_003Es__9[_003C_003Es__10];
				if (_003Cmuscle_003E5__11.name.Contains(_003CrbName_003E5__8))
				{
					if (_003CrbName_003E5__8.Contains("Head"))
					{
						_003Cpm_003E5__5.stateSettings.killDuration = 0f;
						_003Cpm_003E5__5.state = PuppetMaster.State.Dead;
					}
					_003Cmuscle_003E5__11.props.muscleWeight *= 0.5f;
				}
				_003Cmuscle_003E5__11 = null;
			}
			_003C_003Es__9 = null;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethods(BindingFlags.Instance | BindingFlags.Public).First((MethodInfo m) => m.Name == "Shoot" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(EftBulletClass));
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		((MonoBehaviour)StaticManager.Instance).StartCoroutine(WatchShot(shot));
	}

	[IteratorStateMachine(typeof(_003CWatchShot_003Ed__2))]
	private static IEnumerator WatchShot(EftBulletClass shot)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWatchShot_003Ed__2(0)
		{
			shot = shot
		};
	}
}
