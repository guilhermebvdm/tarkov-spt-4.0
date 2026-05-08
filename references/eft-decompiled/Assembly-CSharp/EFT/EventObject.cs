using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT.Ballistics;
using EFT.Interactive;
using EFT.UI;
using UnityEngine;

namespace EFT;

public class EventObject : MonoBehaviour
{
	public enum EInteraction
	{
		Run,
		Repair
	}

	public enum EState
	{
		Disable,
		Ready,
		Running,
		Broken
	}

	[SerializeField]
	private EventObjectTrigger _trigger;

	[SerializeField]
	private EventObjectInteractive _interactive;

	[SerializeField]
	private ulong[] _controlledLampGroupIDs;

	public BallisticCollider ballistics;

	public int id = 1;

	[CompilerGenerated]
	private EState estate_0;

	[CompilerGenerated]
	private RunddansControllerAbstractClass runddansControllerAbstractClass;

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private Action<EState> action_0;

	private float float_0;

	private float float_1;

	private int int_0;

	private DateTime dateTime_0;

	[CompilerGenerated]
	private Action action_1;

	public EState State
	{
		[CompilerGenerated]
		get
		{
			return estate_0;
		}
		[CompilerGenerated]
		set
		{
			estate_0 = value;
		}
	}

	public RunddansControllerAbstractClass Controller
	{
		[CompilerGenerated]
		get
		{
			return runddansControllerAbstractClass;
		}
		[CompilerGenerated]
		set
		{
			runddansControllerAbstractClass = value;
		}
	}

	public bool Handled
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

	public bool InProgress => (dateTime_0 - EFTDateTimeClass.UtcNow).TotalSeconds > 0.0;

	public bool Boolean_0
	{
		get
		{
			if (Handled && State == EState.Running)
			{
				return InProgress;
			}
			return false;
		}
	}

	public event Action<EState> OnStateChanged
	{
		[CompilerGenerated]
		add
		{
			Action<EState> action = action_0;
			Action<EState> action2;
			do
			{
				action2 = action;
				Action<EState> value2 = (Action<EState>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EState> action = action_0;
			Action<EState> action2;
			do
			{
				action2 = action;
				Action<EState> value2 = (Action<EState>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnFinish
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static string smethod_0(EState state)
	{
		return $"Generator/{state}";
	}

	public void OnEnable()
	{
		if (ballistics != null)
		{
			ballistics.OnHitAction += method_0;
		}
	}

	public void OnDisable()
	{
		if (ballistics != null)
		{
			ballistics.UnsubscribeHitAction();
		}
	}

	public void method_0(DamageInfoStruct damageInfo)
	{
		if (Controller == null || State == EState.Disable || !Boolean_0)
		{
			return;
		}
		int frameCount = Time.frameCount;
		if (int_0 != frameCount)
		{
			float damage = damageInfo.Damage;
			if (damageInfo.DamageType == EDamageType.Melee && Controller.GetKnifeCritToBreak)
			{
				damage = float_1;
			}
			float num = float_1 - damage;
			float_1 = num;
			int_0 = frameCount;
			if (float_1 <= 0f)
			{
				SetState(EState.Broken);
			}
		}
	}

	public void ExplosionCheck(IExplosiveItem grenadeItem, in Vector3 grenadePosition)
	{
		if (Controller != null && State != EState.Disable && Boolean_0 && (grenadePosition - base.transform.position).sqrMagnitude <= Controller.GrenadeDistanceSqr)
		{
			ballistics.ApplyHit(new DamageInfoStruct
			{
				DamageType = EDamageType.Explosion,
				Damage = float_1,
				HitPoint = grenadePosition
			}, ShotIdStruct.EMPTY_SHOT_ID);
		}
	}

	public void SetTime(ICollection<TransitPoint> points)
	{
		foreach (TransitPoint point in points)
		{
			if (point.parameters.events)
			{
				dateTime_0 = EFTDateTimeClass.UtcNow.AddSeconds(point.parameters.activateAfterSec);
				MonoBehaviourSingleton<GameUI>.Instance.EventStatePanel.dateTime = dateTime_0;
				break;
			}
		}
		if (_trigger.Inside)
		{
			ShowPanel();
		}
	}

	public void FixedUpdate()
	{
		if (Controller == null || State == EState.Disable)
		{
			return;
		}
		if (State == EState.Running && action_1 != null && !InProgress)
		{
			action_1();
			action_1 = null;
		}
		if (Boolean_0)
		{
			if (float_0 > 0f)
			{
				float_0 -= Time.deltaTime;
			}
			else
			{
				SetState(EState.Broken);
			}
		}
	}

	public void SetState(EState state)
	{
		if (Controller == null || state == State)
		{
			return;
		}
		State = state;
		if (Handled && state == EState.Running)
		{
			float_0 = Controller.GetSecToBreak;
			float_1 = Controller.GetDurability;
		}
		method_1(state == EState.Running);
		switch (State)
		{
		}
		if (_trigger.Inside)
		{
			ShowPanel();
		}
		action_0?.Invoke(state);
	}

	public void method_1(bool state)
	{
		GameWorld instance = Singleton<GameWorld>.Instance;
		if (instance == null || _controlledLampGroupIDs == null)
		{
			return;
		}
		ulong[] controlledLampGroupIDs = _controlledLampGroupIDs;
		foreach (ulong num in controlledLampGroupIDs)
		{
			if (instance.TryGetControlledLampGroup(num, out var controlledLampGroup))
			{
				controlledLampGroup.SetLightStateExternal(state);
			}
		}
	}

	public void ShowPanel()
	{
		string text = smethod_0(State);
		MonoBehaviourSingleton<GameUI>.Instance.EventStatePanel.Show(text, State != EState.Running);
	}
}
