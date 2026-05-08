using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class GClass50 : BaseLogicLayerSimpleAbstractClass
{
	[CompilerGenerated]
	public class Class210
	{
		public BotOwner bot;

		public bool method_0(GroupPoint x)
		{
			if (x.Special.HasFlag(ECoverPointSpecial.forFollowers))
			{
				return x.ConnectionGroup == bot.StartCorePoint.ConnectionGroupId;
			}
			return false;
		}

		public bool method_1(GroupPoint x)
		{
			if (x.Special.HasFlag(ECoverPointSpecial.forBoss))
			{
				return x.ConnectionGroup == bot.StartCorePoint.ConnectionGroupId;
			}
			return false;
		}
	}

	[NonSerialized]
	public int Int_1 = -1;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public List<GroupPoint> List_0;

	public GClass50(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Vector3_0 = BotOwner_0.Position;
		method_13();
		List_0 = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.Points.Where((GroupPoint x) => x.Special.HasFlag(ECoverPointSpecial.forFollowers) && x.ConnectionGroup == bot.StartCorePoint.ConnectionGroupId).ToList();
		int num = BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.Points.Count((GroupPoint x) => x.Special.HasFlag(ECoverPointSpecial.forBoss) && x.ConnectionGroup == bot.StartCorePoint.ConnectionGroupId);
		Bool_4 = List_0.Count > 10 && num > 5;
	}

	public void method_13()
	{
		foreach (AIPlaceInfo place in BotOwner_0.BotsController.CoversData.AIPlaceInfoHolder.Places)
		{
			if (place.InfoLogicAllEnemy is AIPlaceInfoLogicBoar infoLogicZryachiy)
			{
				method_14(place, infoLogicZryachiy);
				break;
			}
		}
	}

	public void method_14(AIPlaceInfo aiPlaceInfo, AIPlaceInfoLogicBoar infoLogicZryachiy)
	{
		Vector3_0 = infoLogicZryachiy.Position;
	}
}
