using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using UnityEngine;

public class ArtilleryServerProjectileClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class1001
	{
		public static readonly Class1001 class1001_0 = new Class1001();

		public ISharedBallisticsCalculator method_0()
		{
			return Singleton<GInterface169>.Instance.CreateBallisticCalculator(0);
		}
	}

	public int id;

	public Vector3 finalPosition;

	public Vector3 startPosition;

	public Vector3 currentPosition;

	public bool explosion;

	public bool firstInShellingForZone;

	public bool lastInShellingForZone;

	public bool firstInRoundForZone;

	public bool lastInRoundForZone;

	public float speed = 50f;

	public float arcHeight = -150f;

	public Vector2 explosionDistnaceRange = new Vector3(3f, 5f);

	public string zoneID = "";

	[CompilerGenerated]
	private Action<ArtilleryServerProjectileClass> action_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public MineDataClass MineDataClass;

	[NonSerialized]
	public static Lazy<ISharedBallisticsCalculator> Lazy_0 = new Lazy<ISharedBallisticsCalculator>(() => Singleton<GInterface169>.Instance.CreateBallisticCalculator(0));

	public event Action<ArtilleryServerProjectileClass> ExplosionEvent
	{
		[CompilerGenerated]
		add
		{
			Action<ArtilleryServerProjectileClass> action = action_0;
			Action<ArtilleryServerProjectileClass> action2;
			do
			{
				action2 = action;
				Action<ArtilleryServerProjectileClass> value2 = (Action<ArtilleryServerProjectileClass>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<ArtilleryServerProjectileClass> action = action_0;
			Action<ArtilleryServerProjectileClass> action2;
			do
			{
				action2 = action;
				Action<ArtilleryServerProjectileClass> value2 = (Action<ArtilleryServerProjectileClass>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public ArtilleryServerProjectileClass()
	{
		(Singleton<AbstractGame>.Instance as LocalGame).UpdateByUnity += OnUpdate;
		MineDataClass = new MineDataClass();
	}

	public void OnUpdate()
	{
		method_0();
	}

	public void SetExplosiveItemParams(BackendConfigSettingsClass.ArtilleryProjectileExplosionParams explosiveParams)
	{
		MineDataClass.SetExplosiveItemParams(explosiveParams);
	}

	public void InitFly(Vector3 startPos, Vector3 finalPos, Vector2 ExplosionDistnace, string idZone, bool firstInShellingZone, bool lastInShellingZone, bool firstInRound, bool lastInRound)
	{
		explosionDistnaceRange = ExplosionDistnace;
		float num = UnityEngine.Random.Range(explosionDistnaceRange.x, explosionDistnaceRange.y);
		finalPosition = finalPos + Vector3.up * num;
		startPosition = startPos;
		currentPosition = startPosition;
		zoneID = idZone;
		firstInShellingForZone = firstInShellingZone;
		lastInShellingForZone = lastInShellingZone;
		firstInRoundForZone = firstInRound;
		lastInRoundForZone = lastInRound;
		Bool_0 = true;
	}

	public void Deactivate()
	{
		finalPosition = Vector3.zero;
		startPosition = Vector3.zero;
		firstInShellingForZone = false;
		lastInShellingForZone = false;
		firstInRoundForZone = false;
		lastInRoundForZone = false;
		explosion = false;
		Bool_0 = false;
	}

	public ArtilleryPacketStruct GetNetPacket()
	{
		return new ArtilleryPacketStruct
		{
			id = id,
			position = currentPosition
		};
	}

	public void method_0()
	{
		if (Bool_0)
		{
			Vector3 vector = currentPosition;
			Vector2 current = new Vector2(currentPosition.x, currentPosition.z);
			Vector2 b = new Vector2(startPosition.x, startPosition.z);
			Vector2 vector2 = new Vector2(finalPosition.x, finalPosition.z);
			float num = Vector2.Distance(vector2, b);
			Vector2 a = Vector2.MoveTowards(current, vector2, speed * Time.deltaTime);
			float num2 = Vector2.Distance(a, b);
			float num3 = Vector2.Distance(a, vector2);
			float num4 = Mathf.Lerp(startPosition.y, finalPosition.y, Vector2.Distance(a, b) / num);
			float num5 = arcHeight * num2 * num3 / (-0.25f * num * num);
			vector = (currentPosition = new Vector3(a.x, num4 + num5, a.y));
			if (finalPosition == vector)
			{
				method_1();
			}
		}
	}

	public void method_1()
	{
		Bool_0 = false;
		action_0?.Invoke(this);
		method_2(MineDataClass, currentPosition, Lazy_0.Value);
	}

	public void method_2(MineDataClass mineData, Vector3 explosionPosition, ISharedBallisticsCalculator ballisticsCalculator)
	{
		if (!mineData.IsDummy)
		{
			GClass2085.Explosion(mineData, explosionPosition, null, ballisticsCalculator, null, method_3, mineData.GetDirectionalDamageMultiplier, mineData.GetDirectionalDamageAngle, Vector3_0, deadlyMinDistance: false);
		}
	}

	public DamageInfoStruct method_3()
	{
		return new DamageInfoStruct
		{
			DamageType = EDamageType.Artillery,
			ArmorDamage = MineDataClass.ArmorDamage,
			StaminaBurnRate = MineDataClass.StaminaBurnRate,
			PenetrationPower = MineDataClass.PenetrationPower,
			Direction = Vector3.zero,
			Player = null,
			IsForwardHit = true
		};
	}
}
