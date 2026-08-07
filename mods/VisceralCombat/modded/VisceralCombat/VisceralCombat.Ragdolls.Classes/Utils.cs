using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

namespace VisceralCombat.Ragdolls.Classes;

public class Utils
{
	[CompilerGenerated]
	private sealed class _003CEnumerateHierarchyCore_003Ed__0 : IEnumerable<Transform>, IEnumerable, IEnumerator<Transform>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Transform _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Transform root;

		public Transform _003C_003E3__root;

		private Queue<Transform> _003CtransformQueue_003E5__1;

		private Transform _003CparentTransform_003E5__2;

		private int _003Ci_003E5__3;

		Transform IEnumerator<Transform>.Current
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
		public _003CEnumerateHierarchyCore_003Ed__0(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CtransformQueue_003E5__1 = null;
			_003CparentTransform_003E5__2 = null;
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
				_003CtransformQueue_003E5__1 = new Queue<Transform>();
				_003CtransformQueue_003E5__1.Enqueue(root);
				break;
			case 1:
				_003C_003E1__state = -1;
				_003CparentTransform_003E5__2 = null;
				break;
			}
			while (_003CtransformQueue_003E5__1.Count > 0)
			{
				_003CparentTransform_003E5__2 = _003CtransformQueue_003E5__1.Dequeue();
				if (!_003CparentTransform_003E5__2 != null)
				{
					continue;
				}
				_003Ci_003E5__3 = 0;
				while (_003Ci_003E5__3 < _003CparentTransform_003E5__2.childCount)
				{
					_003CtransformQueue_003E5__1.Enqueue(_003CparentTransform_003E5__2.GetChild(_003Ci_003E5__3));
					_003Ci_003E5__3++;
				}
				_003C_003E2__current = _003CparentTransform_003E5__2;
				_003C_003E1__state = 1;
				return true;
			}
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

		[DebuggerHidden]
		IEnumerator<Transform> IEnumerable<Transform>.GetEnumerator()
		{
			_003CEnumerateHierarchyCore_003Ed__0 _003CEnumerateHierarchyCore_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateHierarchyCore_003Ed__ = this;
			}
			else
			{
				_003CEnumerateHierarchyCore_003Ed__ = new _003CEnumerateHierarchyCore_003Ed__0(0);
			}
			_003CEnumerateHierarchyCore_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateHierarchyCore_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Transform>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CLerpLayerWeight_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Player __instance;

		public int layer;

		public float startValue;

		public float endValue;

		public float duration;

		private float _003CtimeElapsed_003E5__1;

		private float _003CcurrentWeight_003E5__2;

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
		public _003CLerpLayerWeight_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
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
				_003CtimeElapsed_003E5__1 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003CtimeElapsed_003E5__1 < duration)
			{
				_003CcurrentWeight_003E5__2 = Mathf.Lerp(startValue, endValue, _003CtimeElapsed_003E5__1 / duration);
				__instance.BodyAnimatorCommon.SetLayerWeight(layer, _003CcurrentWeight_003E5__2);
				_003CtimeElapsed_003E5__1 += Time.deltaTime;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			__instance.BodyAnimatorCommon.SetLayerWeight(layer, endValue);
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

	[IteratorStateMachine(typeof(_003CEnumerateHierarchyCore_003Ed__0))]
	internal static IEnumerable<Transform> EnumerateHierarchyCore(Transform root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateHierarchyCore_003Ed__0(-2)
		{
			_003C_003E3__root = root
		};
	}

	internal static void SetAnimation(AnimatorOverrideController overrideController, string clipName, AnimationClip clip)
	{
		typeof(AnimatorOverrideController).GetMethod("Internal_SetClipByName", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(overrideController, new object[2] { clipName, clip });
	}

	[IteratorStateMachine(typeof(_003CLerpLayerWeight_003Ed__2))]
	internal static IEnumerator LerpLayerWeight(Player __instance, int layer, float startValue, float endValue, float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLerpLayerWeight_003Ed__2(0)
		{
			__instance = __instance,
			layer = layer,
			startValue = startValue,
			endValue = endValue,
			duration = duration
		};
	}

	internal static void SetupPuppetMaster(Player p)
	{
		if ((Object)(object)p == (Object)null || (Object)(object)((Component)p).gameObject == (Object)null)
		{
			QuickLogger.Log(ELogType.Warn, "SetupPuppetMaster called with null Player or Player GameObject!");
			return;
		}
		if ((Object)(object)VisceralEntry.Instance?.effectContainer?.activeRagdollBase == (Object)null)
		{
			Profile profile = p.Profile;
			QuickLogger.Log(ELogType.Warn, "Ragdoll Base Isn't Loaded in Effects! Skipping Player! " + ((profile != null) ? profile.Nickname : null));
			return;
		}
		try
		{
			GameObject val = Object.Instantiate<GameObject>(VisceralEntry.Instance.effectContainer.activeRagdollBase);
			val.SetActive(true);
			PuppetMaster puppetMaster = PuppetMaster.SetUp(((Component)p).gameObject.transform, val.transform, LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("TransparentFX"));
			if ((Object)(object)puppetMaster == (Object)null)
			{
				Profile profile2 = p.Profile;
				QuickLogger.Log(ELogType.Warn, "PuppetMaster setup failed for Player: " + ((profile2 != null) ? profile2.Nickname : null));
				return;
			}
			Transform transform = ((Component)puppetMaster).transform;
			if ((Object)(object)((transform != null) ? transform.parent : null) != (Object)null)
			{
				((Component)puppetMaster).transform.parent.parent = ((Component)p).gameObject.transform;
			}
			puppetMaster.mappingWeight = 0f;
			((Behaviour)puppetMaster).enabled = false;
			Profile profile3 = p.Profile;
			QuickLogger.Log(ELogType.Log, "PuppetMaster setup complete for Player: " + ((profile3 != null) ? profile3.Nickname : null));
		}
		catch (Exception arg)
		{
			Profile profile4 = p.Profile;
			QuickLogger.Log(ELogType.Error, $"Exception in SetupPuppetMaster for Player: {((profile4 != null) ? profile4.Nickname : null)}, {arg}");
		}
	}
}
