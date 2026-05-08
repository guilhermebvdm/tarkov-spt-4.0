using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class MineDirectional : MonoBehaviour, IPhysicsTrigger
{
	public class GClass704
	{
		[NonSerialized]
		public ArrayPool<byte> ArrayPool_0;

		public GClass704()
		{
			ArrayPool_0 = ArrayPool<byte>.Shared;
			ArrayPool_0.Preallocate(59392, 2);
		}

		public byte[] Serialize(MineDirectional mineDirectional)
		{
			EFTWriterClass eFTWriterClass = new EFTWriterClass();
			GClass1290.WriteVector3(eFTWriterClass, mineDirectional.transform.position);
			return eFTWriterClass.ToArray();
		}

		public static Vector3 Deserialize(byte[] data)
		{
			return GClass1285.ReadVector3(new EFTReaderClass(data));
		}
	}

	[Serializable]
	public struct MineSettings : IExplosiveItem
	{
		[SerializeField]
		public Vector3 _blindness;

		[SerializeField]
		public Vector3 _contusion;

		[SerializeField]
		public Vector3 _armorDistanceDistanceDamage;

		[SerializeField]
		public float _minExplosionDistance;

		[SerializeField]
		public float _maxExplosionDistance;

		[SerializeField]
		public int _fragmentsCount;

		[SerializeField]
		public float _strength;

		[SerializeField]
		public string _tag;

		[SerializeField]
		public float _armorDamage;

		[SerializeField]
		public float _staminaBurnRate;

		[SerializeField]
		public float _penetrationPower;

		[SerializeField]
		public string _fragmentType;

		[SerializeField]
		public string _fxName;

		[SerializeField]
		public WildSpawnType _ignoreRole;

		[SerializeField]
		public float _directionalDamageAngle;

		[SerializeField]
		public float _directionalDamageMultiplier;

		public WildSpawnType IgnoreRole => _ignoreRole;

		public float ArmorDamage => _armorDamage;

		public float StaminaBurnRate => _staminaBurnRate;

		public float PenetrationPower => _penetrationPower;

		public string FXName => _fxName;

		public Vector3 Blindness => _blindness;

		public Vector3 Contusion => _contusion;

		public Vector3 ArmorDistanceDistanceDamage => _armorDistanceDistanceDamage;

		public float MinExplosionDistance => _minExplosionDistance;

		public float MaxExplosionDistance => _maxExplosionDistance;

		public int FragmentsCount => _fragmentsCount;

		public float GetStrength => _strength;

		public float GetDirectionalDamageAngle => _directionalDamageAngle;

		public float GetDirectionalDamageMultiplier => _directionalDamageMultiplier;

		public string Tag => _tag;

		public string FragmentType => _fragmentType;

		[field: NonSerialized]
		public bool IsDummy { get; }

		public AmmoItemClass CreateFragment()
		{
			if (!Singleton<ItemFactoryClass>.Instance.ItemTemplates.ContainsKey(FragmentType))
			{
				return null;
			}
			return (AmmoItemClass)Singleton<ItemFactoryClass>.Instance.CreateItem(MongoID.Generate(), FragmentType, null);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class367
	{
		public static readonly Class367 class367_0 = new Class367();

		public ISharedBallisticsCalculator method_0()
		{
			return Singleton<GInterface169>.Instance.CreateBallisticCalculator(0);
		}
	}

	private static int int_0;

	public static List<MineDirectional> Mines = new List<MineDirectional>();

	private static Lazy<ISharedBallisticsCalculator> lazy_0 = new Lazy<ISharedBallisticsCalculator>(() => Singleton<GInterface169>.Instance.CreateBallisticCalculator(0));

	[SerializeField]
	private MineSettings _mineData;

	private bool bool_0;

	private bool bool_1;

	public string Tag => _mineData.Tag;

	public string Description => "MineDirectional";

	bool IPhysicsTrigger.enabled
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public void Awake()
	{
		Mines.Add(this);
	}

	public bool method_0(Player player)
	{
		if (Singleton<AbstractGame>.Instance.GameType != EGameType.Offline)
		{
			return false;
		}
		LighthouseTraderZone lighthouseTraderZone = Singleton<GameWorld>.Instance.RestrictableZones.OfType<LighthouseTraderZone>().FirstOrDefault();
		if (lighthouseTraderZone == null)
		{
			return false;
		}
		return lighthouseTraderZone.IsAllowedPlayer(player);
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!bool_0 && !bool_1 && !method_1(other) && Singleton<GameWorld>.Instance is ClientLocalGameWorld)
		{
			Explosion();
		}
	}

	public bool method_1(Collider other)
	{
		try
		{
			Player playerByCollider = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
			if (playerByCollider == null)
			{
				return true;
			}
			if (!playerByCollider.IsAI)
			{
				return method_0(playerByCollider);
			}
			IAIData aIData = playerByCollider.AIData;
			if (aIData == null)
			{
				return false;
			}
			if (_mineData.IgnoreRole.Equals(null))
			{
				return false;
			}
			return aIData.BotOwner.IsRole(_mineData.IgnoreRole);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return false;
	}

	public void OnTriggerExit(Collider collider)
	{
	}

	public void SetArmed(bool isArmed)
	{
		bool_0 = !isArmed;
	}

	public void Explosion()
	{
		Singleton<GameWorld>.Instance.MineManager.OnMineExplosion(this);
		if (!string.IsNullOrEmpty(_mineData.FXName))
		{
			Singleton<Effects>.Instance.EmitGrenade(_mineData.FXName, base.transform.position, Vector3.up);
		}
		method_2(_mineData, base.transform.position, lazy_0.Value);
		bool_0 = true;
		bool_1 = true;
		base.gameObject.SetActive(value: false);
	}

	public void method_2(MineSettings mineData, Vector3 explosionPosition, ISharedBallisticsCalculator ballisticsCalculator)
	{
		if (!mineData.IsDummy && !bool_0 && !bool_1)
		{
			GClass2085.Explosion(mineData, explosionPosition, null, ballisticsCalculator, null, method_3, mineData.GetDirectionalDamageMultiplier, mineData.GetDirectionalDamageAngle, base.transform.forward, deadlyMinDistance: false);
		}
	}

	public DamageInfoStruct method_3()
	{
		return new DamageInfoStruct
		{
			DamageType = EDamageType.Landmine,
			ArmorDamage = _mineData.ArmorDamage,
			StaminaBurnRate = _mineData.StaminaBurnRate,
			PenetrationPower = _mineData.PenetrationPower,
			Direction = Vector3.zero,
			Player = null,
			IsForwardHit = true
		};
	}
}
