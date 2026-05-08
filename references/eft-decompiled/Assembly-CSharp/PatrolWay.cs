using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using UnityEngine;

[Serializable]
public class PatrolWay : MonoBehaviour
{
	public PatrolType PatrolType;

	public List<PatrolPoint> Points = new List<PatrolPoint>();

	[SerializeField]
	private int _maxPersons = -1;

	private HashSet<BotOwner> _users = new HashSet<BotOwner>();

	public float CoefSubPoints = 1f;

	public int MaxPersons => _maxPersons;

	public Vector3 Vector3_0
	{
		get
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < Points.Count; i++)
			{
				PatrolPoint patrolPoint = Points[i];
				zero += patrolPoint.position;
			}
			return zero / Points.Count;
		}
	}

	public void InitPoints()
	{
		foreach (PatrolPoint point in Points)
		{
			point.SetWay(this);
		}
	}

	public void EditorInitPoints()
	{
		List<PatrolPoint> list = new List<PatrolPoint>();
		foreach (Transform item in base.transform)
		{
			if (!item.gameObject.activeInHierarchy)
			{
				continue;
			}
			PatrolPoint component = item.GetComponent<PatrolPoint>();
			if (component != null)
			{
				if (component.Id == 0)
				{
					component.Id = Guid.NewGuid().GetHashCode();
				}
				list.Add(component);
			}
		}
		Points = list.ToList();
		if (PatrolType == PatrolType.reserved)
		{
			_ = Points.Count;
		}
	}

	public void RemoveMe(BotOwner owner)
	{
		_users.Remove(owner);
	}

	public void AddUser(BotOwner owner)
	{
		if (!_users.Contains(owner))
		{
			_users.Add(owner);
		}
	}

	public bool HaveFreeSpace()
	{
		if (MaxPersons <= 0)
		{
			return true;
		}
		return _users.Count < MaxPersons;
	}

	public bool HaveFreeSpace(BotOwner exeptBot)
	{
		if (MaxPersons <= 0)
		{
			return true;
		}
		int num = ((!_users.Contains(exeptBot)) ? _users.Count : (_users.Count - 1));
		return num < MaxPersons;
	}

	public int UsersCount()
	{
		return _users.Count;
	}

	public string AllUsersDebugIdInfo()
	{
		string text = "";
		foreach (BotOwner user in _users)
		{
			text = text + " " + user.Id;
		}
		return text;
	}

	public string AllUsersDebugIdInfoRole()
	{
		string text = "";
		foreach (BotOwner user in _users)
		{
			text = text + " " + user.Profile.Info.Settings.Role;
		}
		return text;
	}

	public bool IsCloseToSelect(BotOwner owner, float closeToSelectReservWay)
	{
		float num = closeToSelectReservWay * closeToSelectReservWay;
		return (owner.Position - Vector3_0).sqrMagnitude < num;
	}

	public virtual string SuitableData()
	{
		return "Base way. All fine";
	}

	public virtual bool Suitable(BotOwner bot, IGetProfileData data)
	{
		return false;
	}

	public bool CanBeUsedByRole(WildSpawnType settingsRole)
	{
		return true;
	}
}
