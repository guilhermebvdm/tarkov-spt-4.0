using EFT;
using UnityEngine;
using UnityEngine.AI;

public abstract class AReserveWayAction : MonoBehaviour
{
	protected ReserveWayResult _cuResult;

	private float _nextCheckClose;

	private bool _lastDesicion;

	public abstract Vector3 GoTo { get; }

	public abstract Vector3 LookShootTo { get; }

	public abstract ReserveWayResult ManualUpdate(BotOwner owner);

	public abstract void RefreshData();

	public abstract void RefreshBot();

	public abstract void DrawGizmos();

	public void SetCurrentUser(BotOwner bot)
	{
		if (bot == null)
		{
			SetFree();
		}
		else
		{
			RefreshBot();
		}
	}

	public virtual void SetFree()
	{
	}

	public abstract void AutoFix();

	public abstract void ComeTo(BotOwner bot);

	public virtual float TimeToUse(BotOwner owner)
	{
		return owner.Settings.FileSettings.Patrol.RESERVE_TIME_STAY;
	}

	public virtual void SetLeaveUser(BotOwner owner)
	{
	}

	public bool CheckDist(BotOwner bot)
	{
		if (_nextCheckClose < Time.time)
		{
			_nextCheckClose = Time.time + 3f;
			Vector3 vector = bot.Position - base.transform.position;
			float y = vector.y;
			vector.y = 0f;
			float sqrMagnitude = vector.sqrMagnitude;
			_lastDesicion = sqrMagnitude < 1.4f && Mathf.Abs(y) < 0.3f;
			return _lastDesicion;
		}
		return _lastDesicion;
	}

	public void CheckPoint(Vector3 point, string data)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		bool flag = true;
		if (flag = NavMesh.CalculatePath(point, base.transform.parent.position, -1, navMeshPath))
		{
			flag = navMeshPath.status == NavMeshPathStatus.PathComplete;
		}
		if (!flag)
		{
			Debug.LogError(data + " can't find path to point " + base.gameObject.name + "   parent:" + base.transform.parent.name);
		}
	}

	public void CheckWayFromParent(string nameInfo, Vector3 from)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(base.transform.parent.position, from, -1, navMeshPath) && navMeshPath.status != NavMeshPathStatus.PathComplete)
		{
			Debug.LogError(nameInfo + " can't find way from parent transform. try autofix. Check again:" + base.gameObject.name);
			AutoFix();
		}
	}

	public AReserveWayAction()
	{
	}
}
