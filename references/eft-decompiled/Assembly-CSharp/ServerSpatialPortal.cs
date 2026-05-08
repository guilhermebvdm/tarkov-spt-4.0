using System.Collections.Generic;
using EFT.Interactive;
using UnityEngine;

public class ServerSpatialPortal : MonoBehaviour, ISpatialPortal
{
	public List<int> ConnectedRoomsId;

	[SerializeField]
	private int _id;

	[SerializeField]
	private bool _alwaysOpen;

	[SerializeField]
	private NavMeshDoorLink _connectedDoor;

	public int Id => _id;

	public Vector3 Center()
	{
		return base.transform.position;
	}

	public void Start()
	{
		if (_connectedDoor == null)
		{
			_alwaysOpen = true;
		}
	}

	public bool IsOpen()
	{
		if (_alwaysOpen)
		{
			return true;
		}
		return _connectedDoor.Door.DoorState == EDoorState.Open;
	}

	public void SetId(int portalId)
	{
		_id = portalId;
	}

	public void SetPosition(Vector3 center)
	{
		base.transform.position = center;
	}

	public void AlwaysOpen()
	{
		_alwaysOpen = true;
	}

	public void SetDoorConnector(NavMeshDoorLink possibleDoor)
	{
		_connectedDoor = possibleDoor;
		_alwaysOpen = false;
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(base.transform.position, 0.09f);
		if (_alwaysOpen)
		{
			Gizmos.color = Color.yellow;
		}
		else
		{
			Gizmos.color = Color.blue;
		}
		Gizmos.DrawWireSphere(base.transform.position, 0.1f);
	}
}
