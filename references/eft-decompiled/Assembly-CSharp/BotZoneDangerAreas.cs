using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class BotZoneDangerAreas
{
	[CompilerGenerated]
	public class Class156
	{
		public BotZoneDangerAreas botZoneDangerAreas_0;

		public AIDangerArea area;

		public GClass641.IBotTimer timer;

		public void method_0()
		{
			botZoneDangerAreas_0.Complete(area);
			botZoneDangerAreas_0.Timers.Remove(timer);
		}
	}

	[NonSerialized]
	public const float DANGER_AREA_RADIUS = 8f;

	[NonSerialized]
	public const float SDIST_TO_CREATE_AREA = 400f;

	[NonSerialized]
	public const float STANDART_DANGER_AREA_PERIOD_SEC = 600f;

	public BotZone ZoneOwner;

	public HashSet<AIDangerArea> ActiveAreas = new HashSet<AIDangerArea>();

	[NonSerialized]
	public HashSet<GClass641.IBotTimer> Timers = new HashSet<GClass641.IBotTimer>();

	[NonSerialized]
	public List<Vector3> DeathPositions = new List<Vector3>();

	public event Action<AIDangerArea> OnDangerAreaCreated;

	public event Action<AIDangerArea> OnDangerAreaRemoved;

	public void Init(BotZone zone)
	{
		ZoneOwner = zone;
	}

	public void AddDanger(Vector3 point, float rad, float period = -1f)
	{
		AIDangerArea area = new AIDangerArea(point, rad, period);
		ActiveAreas.Add(area);
		this.OnDangerAreaCreated?.Invoke(area);
		if (period > 0f)
		{
			GClass641.IBotTimer timer = StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(period));
			Timers.Add(timer);
			timer.OnTimer += delegate
			{
				Complete(area);
				Timers.Remove(timer);
			};
		}
	}

	public void BotDied(Vector3 position)
	{
		foreach (Vector3 deathPosition in DeathPositions)
		{
			if (!((deathPosition - position).sqrMagnitude >= 400f))
			{
				method_0(deathPosition, position);
				DeathPositions.Remove(deathPosition);
				return;
			}
		}
		DeathPositions.Add(position);
	}

	public void Complete(AIDangerArea area)
	{
		if (ActiveAreas.Remove(area))
		{
			this.OnDangerAreaRemoved?.Invoke(area);
		}
	}

	public void OnDrawGizmosSelected()
	{
		foreach (AIDangerArea activeArea in ActiveAreas)
		{
			activeArea.OnDrawGizmosSelected();
		}
	}

	public void method_0(Vector3 p1, Vector3 p2)
	{
		Vector3 point = (p1 + p2) / 2f;
		AddDanger(point, 8f, 600f);
	}

	public void Dispose()
	{
		foreach (GClass641.IBotTimer timer in Timers)
		{
			timer.Stop();
		}
		Timers.Clear();
	}
}
