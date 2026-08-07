using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.Debug;

public class RagdollSpawner
{
	[CompilerGenerated]
	private sealed class _003CEnumerateHierarchyCore_003Ed__5 : IEnumerable<Transform>, IEnumerable, IEnumerator<Transform>, IEnumerator, IDisposable
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
		public _003CEnumerateHierarchyCore_003Ed__5(int _003C_003E1__state)
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
			_003CEnumerateHierarchyCore_003Ed__5 _003CEnumerateHierarchyCore_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateHierarchyCore_003Ed__ = this;
			}
			else
			{
				_003CEnumerateHierarchyCore_003Ed__ = new _003CEnumerateHierarchyCore_003Ed__5(0);
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

	public static bool CheckCorpseIsStill(bool sleeping, float timePass)
	{
		return sleeping || timePass >= 15f;
	}

	public static void SetLayersRecursively(GameObject gameObject, int newLayer, int checkLayer, string[] whitelist = null, params string[] ignore)
	{
		HashSet<string> hashSet = new HashSet<string>(ignore);
		HashSet<string> hashSet2 = ((whitelist != null) ? new HashSet<string>(whitelist) : null);
		Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GameObject gameObject2 = ((Component)componentsInChildren[i]).gameObject;
			if (!hashSet.Contains(((Object)gameObject2).name) && (hashSet2 == null || hashSet2.Contains(((Object)gameObject2).name)) && gameObject2.layer == checkLayer)
			{
				gameObject2.layer = newLayer;
			}
		}
		if (!hashSet.Contains(((Object)gameObject).name) && gameObject.layer == checkLayer && (hashSet2 == null || hashSet2.Contains(((Object)gameObject).name)))
		{
			gameObject.layer = newLayer;
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateHierarchyCore_003Ed__5))]
	private static IEnumerable<Transform> EnumerateHierarchyCore(Transform root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateHierarchyCore_003Ed__5(-2)
		{
			_003C_003E3__root = root
		};
	}
}
