using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.AssetsManager;
using System.Linq;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Patches;

public class RagdollClassPatch : ModulePatch
{
	private static readonly MethodInfo _supportRigidbodyMethod = typeof(Player).Assembly.GetTypes()
		.FirstOrDefault(t => t.GetMethod("SupportRigidbody") != null)?.GetMethod("SupportRigidbody");

	[CompilerGenerated]
	private sealed class _003CRagdollSleepHandler_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public RagdollClass __instance;

		private bool _003Carg_003E5__1;

		private float _003Cnum_003E5__2;

		private int _003Cnum2_003E5__3;

		private List<PlayerRigidbodySleepHierarchy>.Enumerator _003Cenumerator_003E5__4;

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
		public _003CRagdollSleepHandler_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cenumerator_003E5__4 = default(List<PlayerRigidbodySleepHierarchy>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				__instance.method_7();
				_003C_003E2__current = (object)new WaitForSeconds((float)VisceralEntry.Instance.RagdollSleepTime.Value);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003Carg_003E5__1 = false;
				_003Cnum_003E5__2 = 0f;
				goto IL_009d;
			case 3:
				_003C_003E1__state = -1;
				_003Cnum_003E5__2 += Time.deltaTime;
				if (__instance.Func_0(_003Carg_003E5__1, _003Cnum_003E5__2))
				{
					__instance.method_5();
					__instance.Bool_2 = true;
					__instance.Action_0();
					break;
				}
				goto IL_009d;
			case 4:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_009d:
				__instance.method_7();
				_003Cnum2_003E5__3 = 0;
				_003Cenumerator_003E5__4 = __instance.List_0.GetEnumerator();
				try
				{
					while (_003Cenumerator_003E5__4.MoveNext())
					{
						if (_003Cenumerator_003E5__4.Current.TryPutToSleep())
						{
							_003Cnum2_003E5__3++;
						}
					}
				}
				finally
				{
					((IDisposable)_003Cenumerator_003E5__4/*cast due to .constrained prefix*/).Dispose();
				}
				_003Cenumerator_003E5__4 = default(List<PlayerRigidbodySleepHierarchy>.Enumerator);
				_003Carg_003E5__1 = _003Cnum2_003E5__3 >= __instance.List_0.Count;
				_003C_003E2__current = null;
				_003C_003E1__state = 3;
				return true;
			}
			if ((Object)(object)__instance.PlayerBody_0 != (Object)null && __instance.Func_1 != null && __instance.Func_1())
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 4;
				return true;
			}
			if ((Object)(object)__instance.PlayerBody_0 == (Object)null)
			{
				return false;
			}
			__instance.method_2();
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
		return typeof(RagdollClass).GetMethod("Start");
	}

	[PatchPrefix]
	private static bool Prefix(RagdollClass __instance)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		GClass4062.ReleaseBeginSample("CorpseRagdoll.SpawnRigidbodies", "Start");
		CharacterJointSpawner[] characterJointSpawner_ = __instance.CharacterJointSpawner_0;
		for (int i = 0; i < characterJointSpawner_.Length; i++)
		{
			Joint val = ((JointSpawner)characterJointSpawner_[i]).Create();
			val.enablePreprocessing = false;
			ConfigurableJoint val2 = (ConfigurableJoint)(object)((val is ConfigurableJoint) ? val : null);
			if ((Object)(object)val2 != (Object)null)
			{
				val2.projectionMode = (JointProjectionMode)1;
			}
			else
			{
				CharacterJoint val3 = (CharacterJoint)(object)((val is CharacterJoint) ? val : null);
				if ((Object)(object)val3 != (Object)null)
				{
					val3.enableProjection = true;
				}
			}
			val.massScale = val.connectedBody.mass / ((Component)val).GetComponent<Rigidbody>().mass;
			val.connectedMassScale = 1f;
		}
		RigidbodySpawner[] rigidbodySpawner_ = __instance.RigidbodySpawner_0;
		for (int j = 0; j < rigidbodySpawner_.Length; j++)
		{
			Rigidbody val4 = rigidbodySpawner_[j].Create();
			Vector3 normalized = __instance.Vector3_0.normalized;
			__instance.Vector3_0 = (normalized.Equals(Vector3.up) ? normalized : Vector3.ClampMagnitude(__instance.Vector3_0, 2f));
			val4.isKinematic = false;
			val4.maxDepenetrationVelocity = __instance.Float_0;
			val4.velocity = __instance.Vector3_0;
			val4.collisionDetectionMode = __instance.CollisionDetectionMode_0;
			_supportRigidbodyMethod?.Invoke(null, new object[] { val4, 0f, null });
		}
		__instance.Bool_2 = false;
		if (__instance.Bool_1)
		{
			__instance.MonoBehaviour_0.StartCoroutine(RagdollSleepHandler(__instance));
		}
		ArmorPlateCollider[] armorPlateColliders = __instance.PlayerBody_0.PlayerBones.ArmorPlateColliders;
		for (int k = 0; k < armorPlateColliders.Length; k++)
		{
			((Component)armorPlateColliders[k]).gameObject.SetActive(false);
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CRagdollSleepHandler_003Ed__2))]
	public static IEnumerator RagdollSleepHandler(RagdollClass __instance)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRagdollSleepHandler_003Ed__2(0)
		{
			__instance = __instance
		};
	}
}
