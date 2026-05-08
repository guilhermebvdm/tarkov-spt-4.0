using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public abstract class Throwable : MonoBehaviour
{
	private const float float_0 = 0.5f;

	[CompilerGenerated]
	private Action<Throwable> action_0;

	[CompilerGenerated]
	private Action<Throwable> action_1;

	protected Rigidbody Rigidbody;

	protected Vector3 Velocity;

	protected float IgnoreCollisionTrackingTimer;

	private int int_0;

	private const int int_1 = 180;

	private bool bool_0 = true;

	private bool bool_1;

	private ThrowableSettings throwableSettings_0;

	private float float_1;

	private byte byte_0;

	private Coroutine coroutine_0;

	private const float float_2 = 0.2f;

	public byte CollisionNumber
	{
		get
		{
			return byte_0;
		}
		set
		{
			if (value > byte_0)
			{
				byte_0 = value;
				if (Time.time > IgnoreCollisionTrackingTimer)
				{
					OnCollisionHandler();
				}
			}
		}
	}

	public abstract int Id { get; }

	public abstract bool HasNetData { get; }

	public event Action<Throwable> DestroyEvent
	{
		[CompilerGenerated]
		add
		{
			Action<Throwable> action = action_0;
			Action<Throwable> action2;
			do
			{
				action2 = action;
				Action<Throwable> value2 = (Action<Throwable>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Throwable> action = action_0;
			Action<Throwable> action2;
			do
			{
				action2 = action;
				Action<Throwable> value2 = (Action<Throwable>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<Throwable> VelocityBelowThreshold
	{
		[CompilerGenerated]
		add
		{
			Action<Throwable> action = action_1;
			Action<Throwable> action2;
			do
			{
				action2 = action;
				Action<Throwable> value2 = (Action<Throwable>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Throwable> action = action_1;
			Action<Throwable> action2;
			do
			{
				action2 = action;
				Action<Throwable> value2 = (Action<Throwable>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public virtual void Init(ThrowableSettings throwableSettings)
	{
		throwableSettings_0 = throwableSettings;
	}

	public virtual void OnCollisionHandler()
	{
		IgnoreCollisionTrackingTimer = Time.time + 0.5f;
	}

	public virtual void OnDestroy()
	{
		bool_1 = true;
		action_0?.Invoke(this);
	}

	public void FixedUpdate()
	{
		if (float_1 > 0f)
		{
			Velocity = ((Rigidbody == null) ? Vector3.zero : Rigidbody.velocity);
		}
		if (!(Velocity.sqrMagnitude > throwableSettings_0.VelocityTreshold) && bool_0)
		{
			int_0++;
			if (int_0 >= 180)
			{
				bool_0 = false;
				action_1?.Invoke(this);
			}
		}
		else
		{
			int_0 = 0;
		}
		float_1 += Time.fixedDeltaTime;
	}

	public virtual GrenadeDataPacketStruct GetNetPacket()
	{
		GrenadeDataPacketStruct result = new GrenadeDataPacketStruct
		{
			Id = Id,
			Position = base.transform.position,
			Rotation = base.transform.rotation,
			CollisionNumber = CollisionNumber
		};
		if (Rigidbody != null && !bool_1)
		{
			result.Velocity = Rigidbody.velocity;
			result.AngularVelocity = Rigidbody.angularVelocity;
		}
		else
		{
			result.Done = true;
		}
		return result;
	}

	public virtual void ApplyNetPacket(GrenadeDataPacketStruct packet)
	{
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
		}
		CollisionNumber = packet.CollisionNumber;
		coroutine_0 = StartCoroutine(method_0(packet));
		if (packet.Done)
		{
			OnDoneFromNet();
		}
	}

	public void SetKinematic(bool isKinematic)
	{
		Rigidbody.isKinematic = isKinematic;
	}

	public IEnumerator method_0(GrenadeDataPacketStruct packet)
	{
		while (!packet.Done && !((packet.Position - base.transform.position).sqrMagnitude <= 1E-06f))
		{
			yield return new WaitForFixedUpdate();
			Vector3 position = Vector3.Lerp(base.transform.position, packet.Position, 0.2f);
			Quaternion rotation = Quaternion.Lerp(base.transform.rotation, packet.Rotation, 0.2f);
			base.transform.SetPositionAndRotation(position, rotation);
			if (CollisionNumber == packet.CollisionNumber)
			{
				if (Rigidbody != null)
				{
					Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, packet.Velocity, 0.2f);
					Rigidbody.angularVelocity = Vector3.Lerp(Rigidbody.angularVelocity, packet.AngularVelocity, 0.2f);
				}
			}
			else
			{
				method_1(packet);
			}
		}
		if (packet.Done)
		{
			base.transform.SetPositionAndRotation(packet.Position, packet.Rotation);
			method_1(packet);
		}
	}

	public void method_1(GrenadeDataPacketStruct packet)
	{
		if (Rigidbody != null)
		{
			Rigidbody.velocity = packet.Velocity;
			Rigidbody.angularVelocity = packet.AngularVelocity;
		}
	}

	public virtual void OnDoneFromNet()
	{
	}

	public Throwable()
	{
	}
}
