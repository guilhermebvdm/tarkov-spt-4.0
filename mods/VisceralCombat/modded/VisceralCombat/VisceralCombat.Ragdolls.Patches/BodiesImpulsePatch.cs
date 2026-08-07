using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Patches;

public class BodiesImpulsePatch : ModulePatch
{
	[CompilerGenerated]
	private sealed class _003CWatchShot_003Ed__4 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public EftBulletClass shot;

		private Collider _003ChitCollider_003E5__1;

		private AmmoItemClass _003CbulletClass_003E5__2;

		private float _003Cmodifier_003E5__3;

		private Rigidbody _003Crb_003E5__4;

		private float _003CboneModifier_003E5__5;

		private Vector3 _003Cforce_003E5__6;

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
		public _003CWatchShot_003Ed__4(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003ChitCollider_003E5__1 = null;
			_003CbulletClass_003E5__2 = null;
			_003Crb_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
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
			if (!_dictionary.TryGetValue(_003CbulletClass_003E5__2.Caliber, out _003Cmodifier_003E5__3))
			{
				return false;
			}
			_003Crb_003E5__4 = _003ChitCollider_003E5__1.attachedRigidbody;
			if ((Object)(object)_003Crb_003E5__4 == (Object)null)
			{
				return false;
			}
			_003Cmodifier_003E5__3 /= Mathf.Max(_003CbulletClass_003E5__2.ProjectileCount, 1);
			if (_003CbulletClass_003E5__2.Caliber != "12g" && _bonedictionary.TryGetValue(((Object)_003ChitCollider_003E5__1).name, out _003CboneModifier_003E5__5))
			{
				_003Cmodifier_003E5__3 *= _003CboneModifier_003E5__5;
			}
			if ((Object)(object)((Component)_003Crb_003E5__4).gameObject.GetComponent<ObservedLootItem>() != (Object)null)
			{
				_003Cmodifier_003E5__3 *= VisceralEntry.Instance.objectIntensity.Value;
			}
			_003Cforce_003E5__6 = shot.Direction * (_003Cmodifier_003E5__3 * VisceralEntry.Instance.ShotIntensity.Value);
			_003Crb_003E5__4.AddForceAtPosition(_003Cforce_003E5__6, shot.HitPoint);
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

	private static Dictionary<string, float> _dictionary = new Dictionary<string, float>
	{
		{ "12g", 150f },
		{ "762x51", 65f },
		{ "762x39", 45f },
		{ "9x39", 33f },
		{ "545x39", 35f },
		{ "9x18PM", 12f },
		{ "762x35", 60f },
		{ "556x45NATO", 30f },
		{ "127x55", 100f },
		{ "127x108", 350f },
		{ "366TKM", 60f },
		{ "40x46", 200f },
		{ "26x75", 70f },
		{ "30x29", 350f },
		{ "762x54R", 95f },
		{ "86x70", 800f },
		{ "9x19PARA", 12f },
		{ "1143x23ACP", 12f },
		{ "Caliber9x21", 5f },
		{ "57x28", 40f },
		{ "23x75", 200f },
		{ "25x59mm", 180f },
		{ "12.7x99", 110f },
		{ "68x51", 40f }
	};

	private static Dictionary<string, float> _bonedictionary = new Dictionary<string, float>
	{
		{ "Base HumanSpine3", 0.65f },
		{ "Base HumanSpine2", 0.65f },
		{ "Base HumanSpine1", 0.65f },
		{ "Base HumanPelvis", 0.65f },
		{ "Base HumanHead", 3.5f },
		{ "Base HumanNeck", 3.5f },
		{ "Base HumanLUpperarm", 0.75f },
		{ "Base HumanLForearm1", 1f },
		{ "Base HumanRUpperarm", 0.75f },
		{ "Base HumanRForearm1", 1f },
		{ "Base HumanLThigh1", 0.75f },
		{ "Base HumanLThigh2", 0.8f },
		{ "Base HumanLCalf", 1f },
		{ "Base HumanLFoot", 1f },
		{ "Base HumanLToe", 1f },
		{ "Base HumanRThigh1", 0.75f },
		{ "Base HumanRThigh2", 0.8f },
		{ "Base HumanRCalf", 1f },
		{ "Base HumanRFoot", 1f },
		{ "Base HumanRToe", 1f }
	};

	protected override MethodBase GetTargetMethod()
	{
		return typeof(BallisticsCalculator).GetMethod("Shoot", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(EftBulletClass) }, null);
	}

	[PatchPostfix]
	private static void Postfix(EftBulletClass shot)
	{
		if (shot == null) return;

		if (shot.IsShotFinished)
		{
			ProcessImpulse(shot);
		}
		else
		{
			((MonoBehaviour)StaticManager.Instance).StartCoroutine(WatchShot(shot));
		}
	}

	private static void ProcessImpulse(EftBulletClass shot)
	{
		if (shot.HitCollider == null) return;

		Transform rootTransform = shot.HitCollider.transform.root;
		if (rootTransform == null) return;

		VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics.PuppetMaster puppet = rootTransform.GetComponentInChildren<VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics.PuppetMaster>();
		if (puppet != null && puppet.state == VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics.PuppetMaster.State.Dead)
		{
			Rigidbody body = shot.HitCollider.GetComponent<Rigidbody>();
			if (body != null && shot.Ammo != null)
			{
				string caliber = (shot.Ammo is AmmoItemClass ammoItem) ? ammoItem.Caliber : string.Empty;
				float baseForce = _dictionary.TryGetValue(caliber, out float val) ? val : 25f;
				float boneMult = _bonedictionary.TryGetValue(shot.HitCollider.name, out float mult) ? mult : 1f;

				Vector3 impulse = shot.Direction * (baseForce * boneMult * VisceralEntry.Instance.ShotIntensity.Value);
				body.AddForceAtPosition(impulse, shot.HitPoint, ForceMode.Impulse);
			}
		}
	}

	[IteratorStateMachine(typeof(_003CWatchShot_003Ed__4))]
	private static IEnumerator WatchShot(EftBulletClass shot)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWatchShot_003Ed__4(0)
		{
			shot = shot
		};
	}
}
