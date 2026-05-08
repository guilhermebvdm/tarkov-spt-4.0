using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.Counters;
using EFT.Interactive;
using UnityEngine;

namespace CommonAssets.Scripts.Game;

public class EndByExitTrigerScenario : MonoBehaviour
{
	public class Class984
	{
		public readonly Player Player;

		public readonly ExfiltrationPoint Trigger;

		public readonly float StartTime;

		public Class984(Player player, ExfiltrationPoint trigger, float startTime)
		{
			Player = player;
			Trigger = trigger;
			StartTime = startTime;
		}
	}

	public interface GInterface146 : IGame
	{
		void StopSession(string profileId, ExitStatus exitStatus, string exitName);
	}

	[CompilerGenerated]
	public class Class985
	{
		public Player player;

		public bool method_0(Class984 x)
		{
			return x.Player != player;
		}
	}

	[CompilerGenerated]
	public class Class986
	{
		public Player player;

		public bool method_0(Class984 x)
		{
			return x.Player == player;
		}
	}

	private GInterface146 ginterface146_0;

	private ExfiltrationPoint[] exfiltrationPoint_0;

	private readonly List<Class984> list_0 = new List<Class984>();

	private readonly List<(Player, string, IEnumerable<IExfiltrationRequirement>)> list_1 = new List<(Player, string, IEnumerable<IExfiltrationRequirement>)>();

	private readonly List<ExfiltrationPoint> list_2 = new List<ExfiltrationPoint>();

	public static EndByExitTrigerScenario Create(GInterface146 game)
	{
		EndByExitTrigerScenario endByExitTrigerScenario = game.gameObject.AddComponent<EndByExitTrigerScenario>();
		endByExitTrigerScenario.ginterface146_0 = game;
		return endByExitTrigerScenario;
	}

	public void Run()
	{
		ExfiltrationControllerClass instance = ExfiltrationControllerClass.Instance;
		exfiltrationPoint_0 = instance.ExfiltrationPoints.Concat(instance.ScavExfiltrationPoints).Concat(instance.SecretExfiltrationPoints).ToArray();
		ExfiltrationControllerClass.Instance.LogDebug("EndByExitTrigerScenario:Run; _exitTriggers:{0}", exfiltrationPoint_0.Length);
		ExfiltrationPoint[] array = exfiltrationPoint_0;
		foreach (ExfiltrationPoint obj in array)
		{
			obj.OnStartExtraction = (Action<ExfiltrationPoint, Player>)Delegate.Combine(obj.OnStartExtraction, new Action<ExfiltrationPoint, Player>(method_0));
			obj.OnCancelExtraction = (Action<ExfiltrationPoint, Player>)Delegate.Combine(obj.OnCancelExtraction, new Action<ExfiltrationPoint, Player>(method_1));
			obj.OnStatusChanged += method_2;
		}
	}

	public void Stop()
	{
		ExfiltrationControllerClass.Instance.LogDebug("EndByExitTrigerScenario:Stop");
		ExfiltrationPoint[] array = Interlocked.Exchange(ref exfiltrationPoint_0, null);
		if (array != null)
		{
			list_2.Clear();
			ExfiltrationPoint[] array2 = array;
			foreach (ExfiltrationPoint obj in array2)
			{
				obj.OnStartExtraction = (Action<ExfiltrationPoint, Player>)Delegate.Remove(obj.OnStartExtraction, new Action<ExfiltrationPoint, Player>(method_0));
				obj.OnCancelExtraction = (Action<ExfiltrationPoint, Player>)Delegate.Remove(obj.OnCancelExtraction, new Action<ExfiltrationPoint, Player>(method_1));
				obj.OnStatusChanged -= method_2;
				obj.Disable();
			}
		}
	}

	public void method_0(ExfiltrationPoint trigger, Player player)
	{
		if (ginterface146_0.Status != GameStatus.Started && ginterface146_0.Status != GameStatus.SoftStopping)
		{
			ExfiltrationControllerClass.Instance.LogDebug("Extraction attempt while _game.Status is {0}", ginterface146_0.Status);
		}
		else if (list_0.All((Class984 x) => x.Player != player))
		{
			list_0.Add(new Class984(player, trigger, ginterface146_0.PastTime));
			ExfiltrationControllerClass.Instance.LogDebug("Started escape for {0}({1}) on {2} point. _game.PastTime:{3}", player.Profile.Nickname, player.Profile.Info.EntryPoint, trigger.Settings.Name, ginterface146_0.PastTime);
		}
	}

	public void method_1(ExfiltrationPoint trigger, Player player)
	{
		if (ginterface146_0.Status == GameStatus.Started || ginterface146_0.Status == GameStatus.SoftStopping)
		{
			Class984 @class = list_0.FirstOrDefault((Class984 x) => x.Player == player);
			if (@class != null)
			{
				list_0.Remove(@class);
			}
		}
	}

	public void method_2(ExfiltrationPoint point, EExfiltrationStatus prevStatus)
	{
		bool num = list_2.Contains(point);
		if (num && point.Status != EExfiltrationStatus.Countdown)
		{
			point.ExfiltrationStartTime = -1E-45f;
			list_2.Remove(point);
		}
		if (!num && point.Status == EExfiltrationStatus.Countdown)
		{
			if (point.ExfiltrationStartTime <= 0f)
			{
				point.ExfiltrationStartTime = ginterface146_0.PastTime;
			}
			list_2.Add(point);
		}
	}

	public void Update()
	{
		if (exfiltrationPoint_0 == null)
		{
			return;
		}
		for (int num = list_0.Count - 1; num > -1; num--)
		{
			Class984 @class = list_0[num];
			if (!(@class.StartTime + @class.Trigger.Settings.ExfiltrationTime - ginterface146_0.PastTime > 0f))
			{
				list_0.Remove(@class);
				list_1.Add((@class.Player, @class.Trigger.Settings.Name, @class.Trigger.Requirements));
			}
		}
		for (int num2 = list_2.Count - 1; num2 >= 0; num2--)
		{
			ExfiltrationPoint exfiltrationPoint = list_2[num2];
			if (!(ginterface146_0.PastTime - exfiltrationPoint.ExfiltrationStartTime <= exfiltrationPoint.Settings.ExfiltrationTime))
			{
				Player[] array = exfiltrationPoint.Entered.ToArray();
				foreach (Player player in array)
				{
					bool flag = !exfiltrationPoint.UnmetRequirements(player).Any();
					if (player != null && player.HealthController.IsAlive && flag)
					{
						list_1.Add((player, exfiltrationPoint.Settings.Name, exfiltrationPoint.Requirements));
					}
				}
				exfiltrationPoint.SetStatusLogged((!exfiltrationPoint.Reusable) ? EExfiltrationStatus.NotPresent : EExfiltrationStatus.UncompleteRequirements, "EndByExitTriggerScenario:168");
			}
		}
		BackendConfigSettingsClass.GClass1720.GClass1726 matchEnd = Singleton<BackendConfigSettingsClass>.Instance.Experience.MatchEnd;
		foreach (var (player2, exitName, _) in list_1)
		{
			if (player2.StatisticsManager is GClass2266 gClass)
			{
				gClass.ConsumeExperience();
			}
			ExitStatus exitStatus = ((player2.Profile.EftStats.SessionCounters.GetAllInt(CounterTag.Exp) <= matchEnd.SurvivedExpRequirement && !(ginterface146_0.PastTime > (float)matchEnd.SurvivedTimeRequirement)) ? ExitStatus.Runner : ExitStatus.Survived);
			ginterface146_0.StopSession(player2.ProfileId, exitStatus, exitName);
		}
		list_1.Clear();
	}
}
