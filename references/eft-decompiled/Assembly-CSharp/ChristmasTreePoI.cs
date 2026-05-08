using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class ChristmasTreePoI : AIPlaceInfoLogic
{
	private const float GRENADE_CHECK_PERIOD = 3f;

	private const int KHOROVOD_BOTS_LIMIT = 6;

	[SerializeField]
	private List<KhorovodPosition> points = new List<KhorovodPosition>();

	[SerializeField]
	private List<KhorovodState> _khorovodStates = new List<KhorovodState>();

	private int _currentStateIndex;

	private float _time;

	private float _timeFromStartKhorovodState;

	private float _nextGrenadeCheck;

	private List<EPhraseTrigger> _allPhrases = new List<EPhraseTrigger>();

	[SerializeField]
	[Range(0f, 1f)]
	private float _lookDirectionDelta = 0.2f;

	[SerializeField]
	private float _phrasesRecalcPeriod = 1.2f;

	private int _shootingIndex;

	private float _prevPhraseRecalcTime;

	private float _prevShootingSwitchIndexTime;

	public float shootingTime = 0.6f;

	[Range(1f, 3f)]
	public int shootersLimit = 1;

	public int skipRateMin = 10;

	public int skipRateMax = 16;

	public int skips = 9999;

	public int skipRateRolled;

	public int ConnectionGroupId { get; set; } = -1;

	public KhorovodState KhorovodState_0 => _khorovodStates[_currentStateIndex % _khorovodStates.Count];

	public EInteraction Gesture
	{
		get
		{
			if (!RandomGesture)
			{
				return KhorovodState_0.Gesture;
			}
			return (EInteraction)GClass856.Random(0f, 8f);
		}
	}

	public EPhraseTrigger Phrase
	{
		get
		{
			if (!RandomPhrase)
			{
				return KhorovodState_0.Phrase;
			}
			return _allPhrases[UnityEngine.Random.Range(0, _allPhrases.Count)];
		}
	}

	public bool HaveGesture => KhorovodState_0.HaveGesture;

	public bool RandomGesture => KhorovodState_0.RandomGesture;

	public bool RandomPhrase => KhorovodState_0.RandomPhrase;

	public bool ForcedAutomaticFireMode => KhorovodState_0.ForcedAutomaticFireMode;

	public float Single_0 => KhorovodState_0.RotationVelocity;

	public float Single_1 => KhorovodState_0.Radius;

	public float Single_2 => KhorovodState_0.BotsVelocity;

	public float Single_3 => KhorovodState_0.LookHeight;

	public float LookDirectionDelta => _lookDirectionDelta;

	public void method_0()
	{
		if (skips > skipRateRolled)
		{
			skips = 0;
			skipRateRolled = UnityEngine.Random.Range(skipRateMin, skipRateMax);
		}
	}

	public override void Init(AIPlaceInfo aiPlaceInfo, BotsController botsController)
	{
		Vector3 position = aiPlaceInfo.transform.position;
		GroupPoint closest = botsController.CoversData.GetClosest(position);
		if (closest != null)
		{
			ConnectionGroupId = closest.ConnectionGroup;
		}
		method_0();
		skips = skipRateRolled / 2;
		bool flag = GClass856.Random(0f, 100f) < botsController.EventsController.KhorovodEvent.Config.Chance;
		base.gameObject.SetActive(flag);
		base.enabled = flag;
		if (flag)
		{
			Array values = Enum.GetValues(typeof(EPhraseTrigger));
			for (int i = 0; i < values.Length; i++)
			{
				_allPhrases.Add((EPhraseTrigger)values.GetValue(i));
			}
			Singleton<BotEventHandler>.Instance.OnKill += ResetEnemiesIgnoreState;
		}
	}

	public bool HavePhrase(BotOwner owner)
	{
		if (!KhorovodState_0.HavePhrase)
		{
			return false;
		}
		foreach (KhorovodPosition point in points)
		{
			if (point.attachedOwner == owner)
			{
				return point.havePhrase;
			}
		}
		return false;
	}

	public void FixedUpdate()
	{
		_timeFromStartKhorovodState += Time.fixedDeltaTime;
		_time += Time.fixedDeltaTime * Single_0 / (MathF.PI * 2f * Single_1);
		if (_timeFromStartKhorovodState > KhorovodState_0.StateDuration)
		{
			do
			{
				_currentStateIndex = (_currentStateIndex + 1) % _khorovodStates.Count;
				if (_currentStateIndex % _khorovodStates.Count == 0)
				{
					skips++;
					method_0();
				}
				_timeFromStartKhorovodState = 0f;
			}
			while (KhorovodState_0.skip && skips != skipRateRolled);
		}
		if (Time.time > _prevShootingSwitchIndexTime + shootingTime)
		{
			_prevShootingSwitchIndexTime = Time.time;
			if (points.Count > 0)
			{
				_shootingIndex = (_shootingIndex + 1) % points.Count;
			}
			else
			{
				_shootingIndex = 0;
			}
		}
	}

	public int method_1(BotOwner owner)
	{
		int num = 0;
		while (true)
		{
			if (num < points.Count)
			{
				if (points[num].attachedOwner == owner)
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}

	public bool Shooting(BotOwner owner)
	{
		int num = method_1(owner);
		switch (shootersLimit)
		{
		default:
			if (KhorovodState_0.Shooting)
			{
				return num == _shootingIndex;
			}
			return false;
		case 1:
			if (KhorovodState_0.Shooting)
			{
				return num == _shootingIndex;
			}
			return false;
		case 2:
			if (KhorovodState_0.Shooting)
			{
				if (num != _shootingIndex)
				{
					return (num + 3) % points.Count == _shootingIndex;
				}
				return true;
			}
			return false;
		case 3:
			if (KhorovodState_0.Shooting)
			{
				if (num != _shootingIndex && (num + 2) % points.Count != _shootingIndex)
				{
					return (num + 4) % points.Count == _shootingIndex;
				}
				return true;
			}
			return false;
		}
	}

	public void method_2()
	{
		foreach (BotOwner item in BotsInKhorovod())
		{
			if (item.BewareGrenade.ShallRunAway())
			{
				ResetEnemiesIgnoreState(item.BewareGrenade.GrenadeDangerPoint.Grenade.Player.iPlayer);
			}
		}
	}

	public bool ProcessKhorovodPlace(BotOwner owner)
	{
		method_4();
		if (points.Count < 6)
		{
			return true;
		}
		foreach (KhorovodPosition point in points)
		{
			if (point.attachedOwner == owner)
			{
				return true;
			}
		}
		return points.Count < 6;
	}

	public Vector3 InitialPosition()
	{
		return base.transform.position + new Vector3(Mathf.Cos(0f), 0f, Mathf.Sin(0f)) * Single_1;
	}

	public Vector3 GetPoint(BotOwner owner)
	{
		method_4();
		bool flag = false;
		KhorovodPosition khorovodPosition = null;
		for (int i = 0; i < points.Count; i++)
		{
			KhorovodPosition khorovodPosition2 = points[i];
			if (khorovodPosition2.attachedOwner == null || khorovodPosition2.attachedOwner.IsDead)
			{
				khorovodPosition2.attachedOwner = owner;
			}
			if (khorovodPosition2.attachedOwner == owner)
			{
				khorovodPosition = khorovodPosition2;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			khorovodPosition = new KhorovodPosition();
			khorovodPosition.attachedOwner = owner;
			points.Add(khorovodPosition);
		}
		method_5();
		if (Time.time > _nextGrenadeCheck)
		{
			_nextGrenadeCheck = Time.time + 3f;
			method_2();
		}
		if (Time.time > _prevPhraseRecalcTime)
		{
			_prevPhraseRecalcTime = Time.time + _phrasesRecalcPeriod;
			method_3();
		}
		return base.transform.position + khorovodPosition.CalcPosition(Single_1, _time);
	}

	public void method_3()
	{
		foreach (KhorovodPosition point in points)
		{
			point.havePhrase = false;
		}
		int count = points.Count;
		int num = Mathf.Min(KhorovodState_0.speakersLimit, count);
		for (int i = 0; i < num; i++)
		{
			int num2 = UnityEngine.Random.Range(0, count);
			for (int j = num2; j < num2 + count; j++)
			{
				if (!points[j % count].havePhrase)
				{
					points[j % count].havePhrase = true;
					break;
				}
			}
		}
	}

	public float GetMoveVelocity()
	{
		return Single_2;
	}

	public Vector3 GetLookPoint()
	{
		return base.transform.position + Vector3.up * Single_3;
	}

	public void method_4()
	{
		for (int i = 0; i < points.Count; i++)
		{
			KhorovodPosition khorovodPosition = points[i];
			if (khorovodPosition.attachedOwner == null || khorovodPosition.attachedOwner.IsDead)
			{
				points.RemoveAt(i);
				i--;
				method_5();
			}
		}
	}

	public void method_5()
	{
		for (int i = 0; i < points.Count; i++)
		{
			points[i].angle = MathF.PI * 2f / (float)points.Count * (float)i;
		}
	}

	public void ResetEnemiesIgnoreState(IPlayer player, IPlayer target)
	{
		if (IsNear(target) || method_6())
		{
			ResetEnemiesIgnoreState(player);
		}
	}

	public bool method_6()
	{
		int num = 0;
		int num2 = 0;
		foreach (BotOwner item in BotsInKhorovod())
		{
			if (item.Brain != null)
			{
				if (item.Brain.BaseBrain.IsLayerActive<GClass73>())
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		return num2 > num;
	}

	public void ResetEnemiesIgnoreState(IPlayer player)
	{
		if (player == null || player.IsAI)
		{
			return;
		}
		foreach (BotOwner item in BotsInKhorovod(nearOnly: false))
		{
			if (!(item == null) && !item.IsDead && item.BotsGroup != null && item.EnemiesController != null && item.EnemiesController.EnemyInfos != null)
			{
				item.BotsGroup.AddEnemy(player, EBotEnemyCause.christmas);
				if (item.EnemiesController.EnemyInfos.TryGetValue(player, out var value))
				{
					value.SetIgnoreState(state: false);
				}
			}
		}
	}

	public bool IsNear(IPlayer player)
	{
		if (player != null && KhorovodState_0 != null)
		{
			return GClass856.SqrDistance(player.Position, base.transform.position) < KhorovodState_0.Radius * KhorovodState_0.Radius * 4f;
		}
		return false;
	}

	public IEnumerable<BotOwner> BotsInKhorovod(bool nearOnly = true)
	{
		foreach (KhorovodPosition point in points)
		{
			if (point != null && !(point.attachedOwner == null) && KhorovodState_0 != null && point.attachedOwner != null && ((nearOnly && IsNear(point.attachedOwner)) || !nearOnly))
			{
				yield return point.attachedOwner;
			}
		}
	}

	public void OnDestroy()
	{
		if (Singleton<BotEventHandler>.Instance != null)
		{
			Singleton<BotEventHandler>.Instance.OnKill -= ResetEnemiesIgnoreState;
		}
	}

	public void OnDrawGizmosSelected()
	{
		float radius = 0f;
		if (KhorovodState_0 != null)
		{
			radius = Single_1;
		}
		GClass810.DrawCircle(base.transform.position, Vector3.up, Color.green, radius);
		method_5();
		foreach (KhorovodPosition point in points)
		{
			Gizmos.DrawSphere(base.transform.position + point.CalcPosition(radius, _time), 0.25f);
		}
	}
}
