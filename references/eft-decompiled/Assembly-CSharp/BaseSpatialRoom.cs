using System.Collections.Generic;
using Audio.SpatialSystem;
using EFT;
using Unity.AI.Navigation;
using UnityEngine;

public class BaseSpatialRoom : MonoBehaviour, ISpatialRoom
{
	public List<ServerRoomColliderArea> Areas = new List<ServerRoomColliderArea>();

	private readonly Dictionary<int, uint> dictionary_0 = new Dictionary<int, uint>();

	[SerializeField]
	private short _id = -1;

	private List<ISpatialPortal> list_0 = new List<ISpatialPortal>();

	public short ID => _id;

	public List<ISpatialPortal> GetPortals()
	{
		return list_0;
	}

	public void Init()
	{
		foreach (ServerRoomColliderArea area in Areas)
		{
			area.OnTriggerArea += method_0;
		}
	}

	public void AddPortal(ServerSpatialPortal serverSpatialPortal)
	{
		list_0.Add(serverSpatialPortal);
	}

	public void SetId(short servRoomId)
	{
		_id = servRoomId;
	}

	public void method_0(object sender, GEventArgs0 e)
	{
		if (!MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			return;
		}
		IPlayer player = e.Player;
		if (player == null || !player.HealthController.IsAlive)
		{
			return;
		}
		int id = player.Id;
		if (e.TriggerEventType == GEventArgs0.ETriggerEventType.TriggerEnter)
		{
			if (dictionary_0.TryGetValue(id, out var value))
			{
				if (value == 0)
				{
					player.AIData.roomLogicLogic.AddRoom(this);
				}
				dictionary_0[id]++;
			}
			else
			{
				dictionary_0.Add(id, 1u);
				player.AIData.roomLogicLogic.AddRoom(this);
			}
		}
		if (e.TriggerEventType == GEventArgs0.ETriggerEventType.TriggerExit && dictionary_0.TryGetValue(id, out var value2))
		{
			value2--;
			dictionary_0[id] = value2;
			if (value2 == 0)
			{
				player.AIData.roomLogicLogic.RemoveRoom(this);
				dictionary_0.Remove(id);
			}
		}
	}

	public void SetPosition(Transform transformPosition)
	{
		base.transform.position = transformPosition.position;
		base.transform.rotation = transformPosition.rotation;
		base.transform.localScale = transformPosition.localScale;
	}

	public void AddColliders(List<AudioTriggerArea> roomColliders)
	{
		foreach (AudioTriggerArea roomCollider in roomColliders)
		{
			GameObject obj = new GameObject(roomCollider.name);
			obj.transform.SetParent(base.transform);
			BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
			ServerRoomColliderArea serverRoomColliderArea = obj.AddComponent<ServerRoomColliderArea>();
			obj.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
			Areas.Add(serverRoomColliderArea);
			serverRoomColliderArea.GetCollider();
			boxCollider.center = roomCollider.AreaCollider.center;
			boxCollider.size = roomCollider.AreaCollider.size;
			boxCollider.transform.localPosition = roomCollider.transform.localPosition;
			boxCollider.transform.localRotation = roomCollider.transform.localRotation;
			boxCollider.transform.localScale = roomCollider.transform.localScale;
			boxCollider.gameObject.layer = LayerMaskClass.TriggersLayer;
			boxCollider.isTrigger = true;
		}
	}

	public void OnDestroy()
	{
		method_1();
	}

	public void method_1()
	{
		foreach (ServerRoomColliderArea area in Areas)
		{
			area.OnTriggerArea -= method_0;
		}
	}
}
