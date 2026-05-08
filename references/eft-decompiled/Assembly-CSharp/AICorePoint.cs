using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AICorePoint : MonoBehaviour, GInterface5, IPositionPoint, IAICorePointLink
{
	[CompilerGenerated]
	public class Class135
	{
		public int i;

		public bool method_0(AICorePoint x)
		{
			return x.Id == i;
		}
	}

	[SerializeField]
	private int _id;

	[SerializeField]
	private int _connectionGroupId;

	[SerializeField]
	private List<int> _connectionsAtNetId;

	[SerializeField]
	public List<AICorePoint> ConnectionsAtNet;

	[NonSerialized]
	private bool bool_0;

	public int Id => _id;

	public int ConnectionGroupId => _connectionGroupId;

	public AICorePoint CorePointInGame => this;

	public Vector3 Position => base.transform.position;

	public void AddWay(int id, GInterface5 wayTo)
	{
		_connectionsAtNetId.Add(id);
		ConnectionsAtNet.Add(wayTo as AICorePoint);
	}

	public void ClearConnections()
	{
		_connectionsAtNetId = new List<int>();
		ConnectionsAtNet = new List<AICorePoint>();
	}

	public void SetIds(int id, int connectId)
	{
		_connectionGroupId = connectId;
		_id = id;
		base.gameObject.name = $"AICore ID:{Id} CG:{_connectionGroupId}";
	}

	public void AddToName(string addName)
	{
		base.gameObject.name = $"AICore ID:{Id} CG:{_connectionGroupId} near:{addName}";
	}

	public void GizmosDrawWays(float h)
	{
		if (base.transform == null)
		{
			return;
		}
		Gizmos.color = ((_connectionsAtNetId.Count == 0) ? Color.yellow : Color.green);
		float num = 0.5f;
		Vector3 vector = Position + Vector3.up * num;
		Gizmos.DrawCube(base.transform.position + Vector3.up * h, new Vector3(0.5f, 2f * h, 0.5f));
		foreach (AICorePoint item in ConnectionsAtNet)
		{
			if (!(item == null))
			{
				Vector3 to = item.Position + Vector3.up * num;
				Gizmos.DrawLine(item.Position, to);
				Gizmos.DrawLine(vector, to);
				Gizmos.DrawLine(Position, vector);
			}
		}
	}

	public void Restore(List<AICorePoint> corePoints)
	{
		if (bool_0)
		{
			return;
		}
		bool_0 = true;
		ConnectionsAtNet = new List<AICorePoint>();
		foreach (int i in _connectionsAtNetId)
		{
			AICorePoint aICorePoint = corePoints.FirstOrDefault((AICorePoint x) => x.Id == i);
			if (aICorePoint != null)
			{
				ConnectionsAtNet.Add(aICorePoint);
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		float num = 1f;
		Gizmos.DrawSphere(base.transform.position, 0.2f);
		Gizmos.DrawCube(base.transform.position + Vector3.up * num, new Vector3(0.1f, 2f, 0.1f));
		Gizmos.DrawWireSphere(base.transform.position, 0.3f);
	}
}
