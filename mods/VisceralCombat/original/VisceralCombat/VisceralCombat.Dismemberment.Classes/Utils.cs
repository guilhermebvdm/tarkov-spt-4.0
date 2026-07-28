using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes;

public class Utils
{
	[CompilerGenerated]
	private sealed class _003CEnumerateHierarchyCore_003Ed__6 : IEnumerable<Transform>, IEnumerable, IEnumerator<Transform>, IEnumerator, IDisposable
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
		public _003CEnumerateHierarchyCore_003Ed__6(int _003C_003E1__state)
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
				if (!Object.op_Implicit((Object)(object)_003CparentTransform_003E5__2))
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
			_003CEnumerateHierarchyCore_003Ed__6 _003CEnumerateHierarchyCore_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateHierarchyCore_003Ed__ = this;
			}
			else
			{
				_003CEnumerateHierarchyCore_003Ed__ = new _003CEnumerateHierarchyCore_003Ed__6(0);
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

	public static T GetComponentInParentRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		Transform val = obj.transform;
		while ((Object)(object)val != (Object)null)
		{
			T component = ((Component)val).GetComponent<T>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
			val = val.parent;
		}
		return default(T);
	}

	public static List<T> GetComponentsInParentRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		List<T> list = new List<T>();
		Transform val = obj.transform;
		while ((Object)(object)val != (Object)null)
		{
			T[] components = ((Component)val).GetComponents<T>();
			if (components.Length != 0)
			{
				list.AddRange(components);
			}
			val = val.parent;
		}
		return list;
	}

	public static T GetComponentInChildRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		Transform transform = obj.transform;
		T component = ((Component)transform).GetComponent<T>();
		if ((Object)(object)component != (Object)null)
		{
			return component;
		}
		foreach (Transform item in transform)
		{
			Transform val = item;
			component = GetComponentInChildRecursive<T>(((Component)val).gameObject);
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return default(T);
	}

	public static List<T> GetComponentsInChildRecursive<T>(GameObject obj) where T : MonoBehaviour
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		List<T> list = new List<T>();
		Transform transform = obj.transform;
		T[] components = ((Component)transform).GetComponents<T>();
		if (components.Length != 0)
		{
			list.AddRange(components);
		}
		foreach (Transform item in transform)
		{
			Transform val = item;
			list.AddRange(GetComponentsInChildRecursive<T>(((Component)val).gameObject));
		}
		return list;
	}

	public static List<Collider> GetCollidersInChildRecursive(GameObject obj)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		List<Collider> list = new List<Collider>();
		Transform transform = obj.transform;
		Collider[] components = ((Component)transform).GetComponents<Collider>();
		if (components.Length != 0)
		{
			list.AddRange(components);
		}
		foreach (Transform item in transform)
		{
			Transform val = item;
			list.AddRange(GetCollidersInChildRecursive(((Component)val).gameObject));
		}
		return list;
	}

	public static bool CheckNameInHierarchyRecursive(GameObject obj, string word)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if (((Object)obj).name.Contains(word))
		{
			return true;
		}
		Transform val = obj.transform;
		while ((Object)(object)val != (Object)null)
		{
			if (((Object)val).name.Contains(word))
			{
				return true;
			}
			val = val.parent;
		}
		foreach (Transform item in obj.transform)
		{
			Transform val2 = item;
			if (CheckNameInHierarchyRecursive(((Component)val2).gameObject, word))
			{
				return true;
			}
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CEnumerateHierarchyCore_003Ed__6))]
	public static IEnumerable<Transform> EnumerateHierarchyCore(Transform root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateHierarchyCore_003Ed__6(-2)
		{
			_003C_003E3__root = root
		};
	}

	public static bool ParentContains(Transform t, string name)
	{
		while ((Object)(object)t.parent != (Object)null)
		{
			t = t.parent;
			if (((Object)t).name.Contains(name))
			{
				return true;
			}
		}
		return false;
	}

	public static GameObject GetRootGameObject(GameObject obj)
	{
		while ((Object)(object)obj.transform.parent != (Object)null)
		{
			obj = ((Component)obj.transform.parent).gameObject;
		}
		return obj;
	}

	public static Player GetPlayerFromRootGameObject(GameObject obj)
	{
		return GetRootGameObject(obj).GetComponentInChildren<Player>();
	}
}
