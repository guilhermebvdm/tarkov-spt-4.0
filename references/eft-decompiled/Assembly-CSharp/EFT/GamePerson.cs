using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.CameraControl;
using EFT.Game.Spawning;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using UnityEngine;

namespace EFT;

public class GamePerson : MonoBehaviour, IPlayer
{
	[SerializeField]
	private int _id;

	[SerializeField]
	private string _profileId;

	[SerializeField]
	private ESpawnCategory _category;

	[SerializeField]
	private EPlayerSide _side;

	[SerializeField]
	private string _groupId;

	[SerializeField]
	private string _teamId;

	[SerializeField]
	private string _infiltration;

	[SerializeField]
	private string _botZoneName;

	[CompilerGenerated]
	private Action<IPlayer> action_0;

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private EPlayerBtrState eplayerBtrState_0;

	[CompilerGenerated]
	private readonly Vector2 vector2_0;

	[CompilerGenerated]
	private readonly Vector3 vector3_0;

	[CompilerGenerated]
	private readonly byte byte_0;

	[CompilerGenerated]
	private readonly ECameraType ecameraType_0;

	[CompilerGenerated]
	private readonly bool bool_1 = true;

	public int Id => _id;

	public string ProfileId => _profileId;

	public EPlayerSide Side
	{
		get
		{
			return _side;
		}
		set
		{
			_side = value;
		}
	}

	public bool IsAI => _category != ESpawnCategory.Player;

	public bool IsInBufferZone
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

	public EPlayerBtrState BtrState
	{
		[CompilerGenerated]
		get
		{
			return eplayerBtrState_0;
		}
		[CompilerGenerated]
		set
		{
			eplayerBtrState_0 = value;
		}
	}

	public bool StateIsSuitableForHandInput => false;

	public Vector2 Rotation
	{
		[CompilerGenerated]
		get
		{
			return vector2_0;
		}
	}

	public Vector3 Velocity
	{
		[CompilerGenerated]
		get
		{
			return vector3_0;
		}
	}

	public byte ChannelIndex
	{
		[CompilerGenerated]
		get
		{
			return byte_0;
		}
	}

	public ICharacterController CharacterController => null;

	public PlayerBody PlayerBody => null;

	public bool IsYourPlayer => false;

	public ESpawnCategory Category
	{
		get
		{
			return _category;
		}
		set
		{
			_category = value;
		}
	}

	public Vector3 Position => base.transform.position;

	public Vector3 LookDirection => Quaternion.Euler(base.transform.rotation.y, base.transform.rotation.x, 0f) * Vector3.forward;

	public string GroupId
	{
		get
		{
			return _groupId;
		}
		set
		{
			_groupId = value;
		}
	}

	public string TeamId
	{
		get
		{
			return _teamId;
		}
		set
		{
			_teamId = value;
		}
	}

	public string Infiltration => _infiltration;

	public BifacialTransform Transform => null;

	public BifacialTransform WeaponRoot => null;

	public IHealthController HealthController => null;

	public Profile Profile => null;

	public Player GetPlayer => null;

	public PlayerBones PlayerBones => null;

	public IAIData AIData => null;

	public PlayerLoyaltyData Loyalty => null;

	public Dictionary<BodyPartType, EnemyPart> MainParts => null;

	public string AccountId => Profile?.AccountId;

	public InventoryController InventoryController => null;

	public IPlayerSearchController SearchController => null;

	public Player.EUpdateMode ArmsUpdateMode => Player.EUpdateMode.None;

	public EUpdateQueue ArmsUpdateQueue => EUpdateQueue.None;

	public ECameraType VisibleToCameraType
	{
		[CompilerGenerated]
		get
		{
			return ecameraType_0;
		}
	}

	public bool IsVisibleToCamera
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
	}

	public event Action<IPlayer> OnIPlayerDeadOrUnspawn
	{
		[CompilerGenerated]
		add
		{
			Action<IPlayer> action = action_0;
			Action<IPlayer> action2;
			do
			{
				action2 = action;
				Action<IPlayer> value2 = (Action<IPlayer>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<IPlayer> action = action_0;
			Action<IPlayer> action2;
			do
			{
				action2 = action;
				Action<IPlayer> value2 = (Action<IPlayer>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static GamePerson Create(Vector3 position, Quaternion rotation, int id, string profileId, ESpawnCategory category, EPlayerSide side, string name = null, string groupId = null, string infiltration = null, string botZoneName = null)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = "GamePerson";
		}
		GamePerson gamePerson = new GameObject(name).AddComponent<GamePerson>();
		gamePerson.transform.position = position;
		gamePerson.transform.rotation = rotation;
		gamePerson._id = id;
		gamePerson._profileId = profileId;
		gamePerson._category = category;
		gamePerson._side = side;
		gamePerson._groupId = ((groupId != "") ? groupId : null);
		gamePerson._infiltration = ((infiltration != "") ? infiltration : null);
		gamePerson._botZoneName = ((botZoneName != "") ? botZoneName : null);
		return gamePerson;
	}

	public static GamePerson Create(GClass1678 args)
	{
		return Create(args.Position, args.Rotation, args.Id, args.ProfileId, args.Category, args.Side, args.Name, args.GroupId, args.Infiltration);
	}

	public void OnDeserializeFromServer(byte channelId, IDataReader reader)
	{
	}

	public RadioTransmitterRecodableComponent FindRadioTransmitter()
	{
		return null;
	}

	public CultistAmuletItemClass FindCultistAmulet()
	{
		return null;
	}

	public bool HasMarkOfUnknown(out MarkOfUnknownItemClass markOfUnknown)
	{
		markOfUnknown = null;
		return false;
	}

	public void SetInteractInHands(EInteraction interaction)
	{
	}

	public void PlantItemLocalOnly(Item item, string zone)
	{
	}

	public void UpdateInteractionCast()
	{
	}

	public void HandleFlareSuccessEvent(Vector3 position, AmmoTemplate ammoTemplate)
	{
	}

	public Vector3 PlayerColliderPointOnCenterAxis(float relativeHeight)
	{
		return Vector3.zero;
	}

	public void Dispose()
	{
	}

	public void OnDrawGizmos()
	{
		Color color = (Gizmos.color = Side switch
		{
			EPlayerSide.Usec => Color.red, 
			EPlayerSide.Bear => Color.green, 
			EPlayerSide.Savage => Color.blue, 
			_ => throw new ArgumentOutOfRangeException(), 
		});
		Vector3 direction = base.transform.TransformDirection(Vector3.up) * 20f;
		Gizmos.DrawRay(base.transform.position, direction);
		color.a = 0.5f;
		Gizmos.color = color;
		GClass850.DrawCube(base.transform.position, base.transform.rotation, new Vector3(1f, 0.01f, 1f));
		switch (Category)
		{
		case ESpawnCategory.Player:
			Gizmos.color = new Color(1f, 0.35f, 0f);
			break;
		case ESpawnCategory.Bot:
			Gizmos.color = new Color(0.35f, 0f, 1f);
			break;
		case ESpawnCategory.Boss:
			Gizmos.color = new Color(0f, 1f, 0.35f);
			break;
		case ESpawnCategory.Coop:
		case ESpawnCategory.Group:
		case ESpawnCategory.Opposite:
			Gizmos.color = new Color(1f, 0.35f, 0f);
			break;
		}
		Gizmos.DrawSphere(base.transform.position + Vector3.up * 30f, 1f);
		Gizmos.DrawSphere(base.transform.position + Vector3.up * 28f, 2f);
		Gizmos.DrawSphere(base.transform.position + Vector3.up * 26f, 3f);
		Gizmos.color = Color.white;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.DrawLine(Vector3.zero, Vector3.forward * 2f * 0.3f);
		Gizmos.DrawLine(Vector3.forward * 2f * 0.3f, Vector3.forward * 1.5f * 0.3f + Vector3.left * 0.5f * 0.3f);
		Gizmos.DrawLine(Vector3.zero + Vector3.forward * 2f * 0.3f, Vector3.forward * 1.5f * 0.3f + Vector3.right * 0.5f * 0.3f);
	}

	public IAnimator GetArmsAnimatorCommon()
	{
		return null;
	}

	public void SetArmsAnimatorCommon(IAnimator animator)
	{
	}

	public GStruct156<Item> FindItemById(MongoID itemId, bool checkDistance = true, bool checkOwnership = true)
	{
		return new GClass1522("Not Implemented in GamePerson");
	}
}
