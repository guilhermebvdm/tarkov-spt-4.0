using System;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;
using UnityEngine;

[Serializable]
public class AIExfiltrationPoint : IAICorePointLink
{
	[SerializeField]
	public int _id;

	[SerializeField]
	public Vector3 _pos;

	[SerializeField]
	public List<Vector3> _posAdditional = new List<Vector3>();

	[NonSerialized]
	public AICorePoint Core;

	[SerializeField]
	public int _coreId;

	[SerializeField]
	public string _name;

	[field: NonSerialized]
	public AICorePoint CorePointInGame { get; }

	public Vector3 Position => _pos;

	public int Id => _id;

	public float ExfiltrationTime => ExfiltrationPoint.Settings.ExfiltrationTime;

	public string Name => _name;

	[field: NonSerialized]
	public ExfiltrationPoint ExfiltrationPoint { get; set; }

	public AIExfiltrationPoint(int id, Vector3 stayPoint, AICorePoint core, ExfiltrationPoint exfiltration)
	{
		_id = id;
		_pos = stayPoint;
		Core = core;
		_coreId = Core.Id;
		_name = exfiltration.Settings.Name;
	}

	public void LinkExfiltration(ExfiltrationPoint point)
	{
		ExfiltrationPoint = point;
	}

	public void AddPositionAdditional(Vector3 pos)
	{
		_posAdditional.Add(pos);
	}

	public Vector3 GetPosition(BotOwner owner)
	{
		if (owner.BotsGroup.MembersCount == 1)
		{
			return _pos;
		}
		if (!owner.Boss.IamBoss && _posAdditional.Count != 0)
		{
			int index = owner.BotFollower.Index;
			return _posAdditional[index % _posAdditional.Count];
		}
		return _pos;
	}

	public void Restore(AICorePointHolder coresHolder)
	{
		Core = coresHolder.GetCorePoint(_coreId);
	}

	public void DrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(_pos, 0.1f);
		Gizmos.DrawWireSphere(_pos + Vector3.up, 0.1f);
		Gizmos.DrawLine(_pos + Vector3.up, _pos);
	}
}
