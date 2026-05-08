using System;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using UnityEngine;

public abstract class GClass897
{
	[CompilerGenerated]
	private static Action<SonicBulletSoundPlayer.GClass898> action_0;

	[CompilerGenerated]
	private static Action<SonicBulletSoundPlayer.GClass898> action_1;

	[CompilerGenerated]
	private static Action<IPlayer, IPlayer, Vector3> action_2;

	public static event Action<SonicBulletSoundPlayer.GClass898> OnShoot
	{
		[CompilerGenerated]
		add
		{
			Action<SonicBulletSoundPlayer.GClass898> action = action_0;
			Action<SonicBulletSoundPlayer.GClass898> action2;
			do
			{
				action2 = action;
				Action<SonicBulletSoundPlayer.GClass898> value2 = (Action<SonicBulletSoundPlayer.GClass898>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<SonicBulletSoundPlayer.GClass898> action = action_0;
			Action<SonicBulletSoundPlayer.GClass898> action2;
			do
			{
				action2 = action;
				Action<SonicBulletSoundPlayer.GClass898> value2 = (Action<SonicBulletSoundPlayer.GClass898>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<SonicBulletSoundPlayer.GClass898> OnSniperZoneShoot
	{
		[CompilerGenerated]
		add
		{
			Action<SonicBulletSoundPlayer.GClass898> action = action_1;
			Action<SonicBulletSoundPlayer.GClass898> action2;
			do
			{
				action2 = action;
				Action<SonicBulletSoundPlayer.GClass898> value2 = (Action<SonicBulletSoundPlayer.GClass898>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<SonicBulletSoundPlayer.GClass898> action = action_1;
			Action<SonicBulletSoundPlayer.GClass898> action2;
			do
			{
				action2 = action;
				Action<SonicBulletSoundPlayer.GClass898> value2 = (Action<SonicBulletSoundPlayer.GClass898>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<IPlayer, IPlayer, Vector3> OnArmorHit
	{
		[CompilerGenerated]
		add
		{
			Action<IPlayer, IPlayer, Vector3> action = action_2;
			Action<IPlayer, IPlayer, Vector3> action2;
			do
			{
				action2 = action;
				Action<IPlayer, IPlayer, Vector3> value2 = (Action<IPlayer, IPlayer, Vector3>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IPlayer, IPlayer, Vector3> action = action_2;
			Action<IPlayer, IPlayer, Vector3> action2;
			do
			{
				action2 = action;
				Action<IPlayer, IPlayer, Vector3> value2 = (Action<IPlayer, IPlayer, Vector3>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static void HitArmor(IPlayer player, IPlayer enemy, Vector3 direction)
	{
		action_2?.Invoke(player, enemy, direction);
	}

	public static void SniperZoneShoot(SonicBulletSoundPlayer.GClass898 sonicInfo)
	{
		action_1?.Invoke(sonicInfo);
	}

	public static void Shoot(SonicBulletSoundPlayer.GClass898 sonicInfo)
	{
		action_0?.Invoke(sonicInfo);
	}

	public static bool SphereIntersection(Vector3 start, Vector3 end, Vector3 center, float radius)
	{
		float num = Mathf.Sqrt(Mathf.Pow(Vector3.Dot((end - start).normalized, start - center), 2f) - (start - center).sqrMagnitude + Mathf.Pow(radius, 2f));
		if (!float.IsNaN(num))
		{
			return num >= 0f;
		}
		return false;
	}

	public static Vector3 CalculateNormalFromPoint(Vector3 start, Vector3 end, Vector3 center)
	{
		Vector3 vector = end - start;
		Vector3 normalized = vector.normalized;
		float num = Vector3.Dot(normalized, center - start);
		Vector3 vector2 = num * normalized + start;
		if (num < 0f && num > vector.magnitude)
		{
			return Vector3.zero;
		}
		return vector2 - center;
	}

	public static float CalculateRadAngle(Vector3 forward, Vector3 bullet)
	{
		Vector2 vector = new Vector2(forward.x, forward.z);
		Vector2 vector2 = new Vector2(bullet.x, bullet.z);
		float num = Vector3.Angle(vector, vector2);
		num *= MathF.PI / 180f;
		if (vector2.x * vector.y - vector2.y * vector.x < 0f)
		{
			num = 0f - num;
		}
		return num;
	}

	public static float CalculatePan(Vector3 forward, Vector3 bullet)
	{
		return Mathf.Clamp(Mathf.Sin(CalculateRadAngle(forward, bullet)), -0.9f, 0.9f);
	}

	public static float CalculateInversePan(Vector3 forward, Vector3 bullet)
	{
		return Mathf.Clamp(Mathf.Cos(CalculateRadAngle(forward, bullet)), -0.9f, 0.9f);
	}
}
