using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using JetBrains.Annotations;
using UnityEngine;

public class EnvironmentManagerBase : MonoBehaviour
{
	public interface GInterface24
	{
		void Reinit();

		IndoorTrigger Check(Vector3 pos);
	}

	protected Dictionary<int, IndoorTrigger> _cachedPlayerTrigger = new Dictionary<int, IndoorTrigger>();

	protected TriggerGroup _rootTriggerGroup;

	private bool bool_0;

	private bool bool_1 = true;

	private CancellationTokenSource cancellationTokenSource_0;

	protected static EnvironmentManagerBase _instance;

	public static EnvironmentManagerBase Instance => _instance;

	public void Awake()
	{
		Init();
	}

	public virtual void OnDestroy()
	{
		if (cancellationTokenSource_0 != null)
		{
			cancellationTokenSource_0.Cancel();
		}
	}

	public virtual void Init()
	{
		cancellationTokenSource_0 = new CancellationTokenSource();
		_instance = this;
		InitIndoors();
		method_0().HandleExceptions();
		bool_0 = _rootTriggerGroup != null;
		bool_1 = true;
	}

	public void InitIndoors()
	{
		_rootTriggerGroup = base.gameObject.GetComponentInChildren<TriggerGroup>();
	}

	public async Task method_0()
	{
		while (bool_1 && !cancellationTokenSource_0.IsCancellationRequested)
		{
			if (Singleton<GameWorld>.Instance == null)
			{
				await Task.Yield();
				continue;
			}
			List<IPlayer> registeredPlayers = Singleton<GameWorld>.Instance.RegisteredPlayers;
			for (int i = 0; i < registeredPlayers.Count; i++)
			{
				if (cancellationTokenSource_0.IsCancellationRequested)
				{
					break;
				}
				IPlayer player = registeredPlayers[i];
				if (player != null)
				{
					Collider collider = player.CharacterController.GetCollider();
					IndoorTrigger indoorTrigger = method_1(collider.bounds.center);
					_cachedPlayerTrigger.TryGetValue(player.Id, out var value);
					if (value == indoorTrigger)
					{
						await Task.Yield();
						continue;
					}
					_cachedPlayerTrigger[player.Id] = indoorTrigger;
					SetTriggerForPlayer(player, indoorTrigger);
					await Task.Yield();
				}
			}
			await Task.Yield();
		}
	}

	[CanBeNull]
	public IndoorTrigger method_1(Vector3 pos)
	{
		if (!bool_0)
		{
			return null;
		}
		return _rootTriggerGroup.Check(pos);
	}

	public int TryFindEnvironmentIdByPos(Vector3 pos)
	{
		if (!bool_0)
		{
			return 0;
		}
		IndoorTrigger indoorTrigger = _rootTriggerGroup.Check(pos);
		if (indoorTrigger == null)
		{
			return 0;
		}
		return indoorTrigger.AreaAutoId;
	}

	public virtual void SetTriggerForPlayer(IPlayer player, IndoorTrigger trigger)
	{
		player.AIData.SetEnvironment(trigger);
	}

	public void Stop()
	{
		bool_1 = false;
	}
}
