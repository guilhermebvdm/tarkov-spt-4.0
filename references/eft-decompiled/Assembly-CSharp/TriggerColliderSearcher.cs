using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

public class TriggerColliderSearcher : MonoBehaviour
{
	public class Class422
	{
		[NonSerialized]
		public string String_0;

		[NonSerialized]
		public Collider Collider_0;

		[NonSerialized]
		public LayerMask LayerMask_0;

		[NonSerialized]
		public Func<Bounds> Func_0;

		public Transform OverrideTransformPosition;

		[NonSerialized]
		public EFTPhysicsClass.GClass747.EWorldType EworldType_0;

		[NonSerialized]
		public Collider[] Collider_1 = new Collider[512];

		[NonSerialized]
		public Collider[][] Collider_2;

		[NonSerialized]
		public List<Collider> List_0 = new List<Collider>(512);

		[NonSerialized]
		public List<Collider> List_1 = new List<Collider>(512);

		[NonSerialized]
		public Dictionary<Collider, List<IPhysicsTrigger>> Dictionary_0 = new Dictionary<Collider, List<IPhysicsTrigger>>(512);

		[NonSerialized]
		public GClass834<IPhysicsTrigger> Gclass834_0 = new GClass834<IPhysicsTrigger>(512, 2);

		[NonSerialized]
		public List<Component> List_2 = new List<Component>(4);

		[CompilerGenerated]
		private Action<IPhysicsTrigger> action_0;

		[CompilerGenerated]
		private Action<IPhysicsTrigger> action_1;

		[NonSerialized]
		public IEnumerator Ienumerator_0;

		[NonSerialized]
		public long Long_0;

		[NonSerialized]
		public Stopwatch Stopwatch_0 = new Stopwatch();

		public event Action<IPhysicsTrigger> OnEnter
		{
			[CompilerGenerated]
			add
			{
				Action<IPhysicsTrigger> action = action_0;
				Action<IPhysicsTrigger> action2;
				do
				{
					action2 = action;
					Action<IPhysicsTrigger> value2 = (Action<IPhysicsTrigger>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<IPhysicsTrigger> action = action_0;
				Action<IPhysicsTrigger> action2;
				do
				{
					action2 = action;
					Action<IPhysicsTrigger> value2 = (Action<IPhysicsTrigger>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_0, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public event Action<IPhysicsTrigger> OnExit
		{
			[CompilerGenerated]
			add
			{
				Action<IPhysicsTrigger> action = action_1;
				Action<IPhysicsTrigger> action2;
				do
				{
					action2 = action;
					Action<IPhysicsTrigger> value2 = (Action<IPhysicsTrigger>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<IPhysicsTrigger> action = action_1;
				Action<IPhysicsTrigger> action2;
				do
				{
					action2 = action;
					Action<IPhysicsTrigger> value2 = (Action<IPhysicsTrigger>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange(ref action_1, value2, action2);
				}
				while ((object)action != action2);
			}
		}

		public Class422(string name, Collider collider, float timeBreakerInterval = 9999f)
		{
			String_0 = name;
			Collider_0 = collider;
			Ienumerator_0 = method_0();
			Long_0 = (long)(timeBreakerInterval * 10000f);
			Collider_2 = new Collider[8][];
			for (int i = 0; i < 8; i++)
			{
				Collider_2[i] = new Collider[512];
			}
		}

		public void Connect(Func<Bounds> boundsGetter)
		{
			Func_0 = boundsGetter;
		}

		public void ManualUpdate(EFTPhysicsClass.GClass747.EWorldType physicsWorldTypeMask, LayerMask castLayerMask)
		{
			EworldType_0 = physicsWorldTypeMask;
			LayerMask_0 = castLayerMask;
			Ienumerator_0.MoveNext();
			for (int i = 0; i < List_1.Count; i++)
			{
				Collider collider = List_1[i];
				method_3(collider);
			}
		}

		public IEnumerator method_0()
		{
			EFTPhysicsClass.GClass747.GClass748 complete = new EFTPhysicsClass.GClass747.GClass748
			{
				Results = Collider_1,
				Buffers = Collider_2
			};
			Vector3 vector = Vector3.down;
			EFTPhysicsClass.GClass747.GClass748 gClass = null;
			while (true)
			{
				Bounds bounds = Func_0();
				Vector3 vector2 = ((!(OverrideTransformPosition != null)) ? bounds.center : OverrideTransformPosition.position);
				if (vector != vector2)
				{
					vector = vector2;
					gClass = EFTPhysicsClass.GClass747.OverlapBoxAsync(EworldType_0, vector2, bounds.extents, Collider_2, Collider_0.transform.rotation, LayerMask_0, QueryTriggerInteraction.Collide, complete);
				}
				yield return null;
				while (!gClass.IsComplete)
				{
					yield return null;
				}
				int count = gClass.Count;
				Stopwatch_0.Reset();
				Stopwatch_0.Start();
				int count2 = List_0.Count;
				long num = Stopwatch_0.ElapsedTicks;
				for (int i = 0; i < count2; i++)
				{
					if (num >= Long_0)
					{
						yield return null;
						Stopwatch_0.Restart();
						num = 0L;
					}
					Collider collider = List_0[i];
					bool flag = false;
					for (int j = 0; j < count; j++)
					{
						if ((object)Collider_1[j] == collider)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						method_2(collider, i);
						count2 = List_0.Count;
					}
				}
				for (int i = 0; i < count; i++)
				{
					if (num >= Long_0)
					{
						yield return null;
						Stopwatch_0.Restart();
						num = 0L;
					}
					Collider collider2 = Collider_1[i];
					bool flag2 = false;
					for (int k = 0; k < count2; k++)
					{
						if ((object)collider2 == List_0[k])
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						method_1(collider2);
						count2 = List_0.Count;
					}
				}
				Stopwatch_0.Stop();
			}
		}

		public void method_1(Collider collider)
		{
			List_0.Add(collider);
			bool flag = false;
			collider.GetComponents(typeof(IPhysicsTrigger), List_2);
			List<IPhysicsTrigger> list = Gclass834_0.Withdraw();
			for (int i = 0; i < List_2.Count; i++)
			{
				IPhysicsTrigger physicsTrigger = List_2[i] as IPhysicsTrigger;
				list.Add(physicsTrigger);
				if (physicsTrigger is IPhysicsTriggerWithStay)
				{
					flag = true;
				}
				try
				{
					physicsTrigger.OnTriggerEnter(Collider_0);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				try
				{
					action_0?.Invoke(physicsTrigger);
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
			}
			Dictionary_0[collider] = list;
			if (flag)
			{
				List_1.Add(collider);
			}
		}

		public void method_2(Collider collider, int index)
		{
			List_0.RemoveAt(index);
			List_1.Remove(collider);
			List<IPhysicsTrigger> list = Dictionary_0[collider];
			for (int i = 0; i < list.Count; i++)
			{
				IPhysicsTrigger physicsTrigger = list[i];
				try
				{
					physicsTrigger.OnTriggerExit(Collider_0);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				try
				{
					action_1?.Invoke(physicsTrigger);
				}
				catch (Exception exception2)
				{
					UnityEngine.Debug.LogException(exception2);
				}
			}
			Gclass834_0.Return(list);
			Dictionary_0.Remove(collider);
		}

		public void method_3(Collider collider)
		{
			List<IPhysicsTrigger> list = Dictionary_0[collider];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is IPhysicsTriggerWithStay physicsTriggerWithStay)
				{
					try
					{
						physicsTriggerWithStay.OnTriggerStay(Collider_0, collider);
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
			}
		}
	}

	public const int MAX_COLLIDERS = 512;

	public const int MAX_WORLDS_COLLIDERS = 8;

	[SerializeField]
	private Collider _collider;

	[SerializeField]
	private LayerMask _castLayerMask;

	private ICharacterController icharacterController_0;

	private Transform transform_0;

	private Class422 class422_0;

	[CanBeNull]
	private Class422 class422_1;

	private List<Class422> list_0 = new List<Class422>();

	public bool IsEnabled;

	[CompilerGenerated]
	private bool bool_0;

	public Transform OverrideTransformPosition
	{
		get
		{
			return transform_0;
		}
		set
		{
			transform_0 = value;
			foreach (Class422 item in list_0)
			{
				item.OverrideTransformPosition = transform_0;
			}
		}
	}

	public bool IsInited
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public event Action<IPhysicsTrigger> OnEnter
	{
		add
		{
			foreach (Class422 item in list_0)
			{
				item.OnEnter += value;
			}
		}
		remove
		{
			foreach (Class422 item in list_0)
			{
				item.OnEnter -= value;
			}
		}
	}

	public event Action<IPhysicsTrigger> OnExit
	{
		add
		{
			foreach (Class422 item in list_0)
			{
				item.OnExit += value;
			}
		}
		remove
		{
			foreach (Class422 item in list_0)
			{
				item.OnExit -= value;
			}
		}
	}

	public void Init(Collider collider, LayerMask castLayerMask)
	{
		_collider = collider;
		_castLayerMask = castLayerMask;
		list_0.Clear();
		class422_0 = new Class422("DefaultCore", _collider);
		list_0.Add(class422_0);
		if (((int)_castLayerMask & LayerMaskClass.DisablerCullingObjectLayerMask) != 0)
		{
			class422_1 = new Class422("HoboCore", _collider, EFTHardSettings.Instance.HoboCastTimeBreakInterval);
			list_0.Add(class422_1);
		}
		IsInited = true;
	}

	public void ConnectToCharacterController(ICharacterController characterController)
	{
		icharacterController_0 = characterController;
		foreach (Class422 item in list_0)
		{
			item.Connect(() => icharacterController_0.bounds);
		}
	}

	public void ManualUpdate(float deltaTime, bool isCloseToCamera = true)
	{
		if (IsEnabled)
		{
			EFTPhysicsClass.GClass747.EWorldType eWorldType = (EFTPhysicsClass.GClass747.EWorldType)0;
			eWorldType = EFTPhysicsClass.GClass747.EWorldType.VolumePropagationAndEnvironmentSwitcherTriggers;
			if (isCloseToCamera)
			{
				eWorldType |= EFTPhysicsClass.GClass747.EWorldType.Default;
			}
			class422_0.ManualUpdate(eWorldType, _castLayerMask);
			class422_1?.ManualUpdate(EFTPhysicsClass.GClass747.EWorldType.DisablerCullingObjectTriggers, _castLayerMask);
		}
	}

	[CompilerGenerated]
	public Bounds method_0()
	{
		return icharacterController_0.bounds;
	}
}
