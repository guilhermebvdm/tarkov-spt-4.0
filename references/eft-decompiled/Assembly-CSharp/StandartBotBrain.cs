using System;
using System.Runtime.CompilerServices;
using EFT;

public class StandartBotBrain : GClass429
{
	[field: NonSerialized]
	public global::AICoreAgentClass<BotLogicDecision> Agent { get; set; }

	[field: NonSerialized]
	public BaseBrain BaseBrain { get; set; }

	[field: NonSerialized]
	public string GetStateName { get; set; }

	public BotLogicDecision? LastDecision
	{
		get
		{
			if (Agent == null)
			{
				return null;
			}
			return Agent.LastResult().Action;
		}
	}

	public event Action<BaseBrain> OnSetStrategy;

	public StandartBotBrain(BotOwner owner)
		: base(owner)
	{
	}

	public string GetCustomData()
	{
		return Agent.GetCustomData();
	}

	public void Activate()
	{
		switch (BotOwner_0.Profile.Info.Settings.Role)
		{
		case WildSpawnType.marksman:
			BaseBrain = new GClass346(BotOwner_0);
			break;
		case WildSpawnType.bossTest:
			BaseBrain = new GClass320(BotOwner_0);
			break;
		case WildSpawnType.bossBully:
			BaseBrain = new GClass314(BotOwner_0);
			break;
		case WildSpawnType.followerBully:
			BaseBrain = new GClass332(BotOwner_0);
			break;
		case WildSpawnType.bossKilla:
			BaseBrain = new GClass342(BotOwner_0);
			break;
		case WildSpawnType.bossKojaniy:
			BaseBrain = new GClass357(BotOwner_0);
			break;
		case WildSpawnType.followerKojaniy:
			BaseBrain = new BossKojaniyBrainClass(BotOwner_0);
			break;
		case WildSpawnType.cursedAssault:
			BaseBrain = new GClass322(BotOwner_0);
			break;
		case WildSpawnType.bossGluhar:
			BaseBrain = new GClass338(BotOwner_0);
			break;
		case WildSpawnType.followerGluharAssault:
			BaseBrain = new GClass339(BotOwner_0);
			break;
		case WildSpawnType.followerGluharScout:
			BaseBrain = new GClass341(BotOwner_0);
			break;
		case WildSpawnType.followerGluharSecurity:
		case WildSpawnType.followerGluharSnipe:
			BaseBrain = new GClass340(BotOwner_0);
			break;
		case WildSpawnType.followerSanitar:
			BaseBrain = new GClass333(BotOwner_0);
			break;
		case WildSpawnType.bossSanitar:
			BaseBrain = new GClass317(BotOwner_0);
			break;
		case WildSpawnType.assaultGroup:
			BaseBrain = new GClass311(BotOwner_0, withPursuit: true);
			break;
		case WildSpawnType.sectantWarrior:
			BaseBrain = new GClass360(BotOwner_0);
			break;
		case WildSpawnType.sectantPriest:
			BaseBrain = new GClass362(BotOwner_0);
			break;
		case WildSpawnType.followerTagilla:
			BaseBrain = new GClass334(BotOwner_0);
			break;
		case WildSpawnType.exUsec:
			BaseBrain = new ExUsecBrainClass(BotOwner_0);
			break;
		case WildSpawnType.gifter:
			BaseBrain = new GClass336(BotOwner_0);
			break;
		case WildSpawnType.bossKnight:
			BaseBrain = new BossKnightBrainClass(BotOwner_0);
			break;
		case WildSpawnType.followerBigPipe:
			BaseBrain = new GClass329(BotOwner_0);
			break;
		case WildSpawnType.followerBirdEye:
			BaseBrain = new GClass330(BotOwner_0);
			break;
		case WildSpawnType.bossZryachiy:
			BaseBrain = new GClass321(BotOwner_0);
			break;
		case WildSpawnType.followerZryachiy:
			BaseBrain = new GClass335(BotOwner_0);
			break;
		case WildSpawnType.bossBoar:
			BaseBrain = new GClass312(BotOwner_0);
			break;
		case WildSpawnType.followerBoar:
			BaseBrain = new GClass331(BotOwner_0, closePatrol: false);
			break;
		case WildSpawnType.arenaFighter:
			BaseBrain = new GClass310(BotOwner_0);
			break;
		case WildSpawnType.pmcBot:
		case WildSpawnType.arenaFighterEvent:
			BaseBrain = new GClass349(BotOwner_0, withPursuit: false);
			break;
		case WildSpawnType.bossBoarSniper:
			BaseBrain = new GClass313(BotOwner_0);
			break;
		case WildSpawnType.crazyAssaultEvent:
			BaseBrain = new GClass325(BotOwner_0);
			break;
		case WildSpawnType.peacefullZryachiyEvent:
			BaseBrain = new GClass347(BotOwner_0);
			break;
		case WildSpawnType.sectactPriestEvent:
			BaseBrain = new GClass352(BotOwner_0);
			break;
		case WildSpawnType.ravangeZryachiyEvent:
			BaseBrain = new GClass351(BotOwner_0);
			break;
		case WildSpawnType.followerBoarClose1:
		case WildSpawnType.followerBoarClose2:
			BaseBrain = new GClass331(BotOwner_0, closePatrol: true);
			break;
		case WildSpawnType.bossKolontay:
			BaseBrain = new GClass343(BotOwner_0);
			break;
		case WildSpawnType.followerKolontayAssault:
			BaseBrain = new GClass344(BotOwner_0);
			break;
		case WildSpawnType.followerKolontaySecurity:
			BaseBrain = new GClass345(BotOwner_0);
			break;
		case WildSpawnType.shooterBTR:
			BaseBrain = new GClass354(BotOwner_0);
			break;
		case WildSpawnType.bossPartisan:
			BaseBrain = new GClass316(BotOwner_0);
			break;
		case WildSpawnType.pmcBEAR:
			BaseBrain = new GClass348(BotOwner_0, withPursuit: false);
			break;
		case WildSpawnType.pmcUSEC:
			BaseBrain = new GClass350(BotOwner_0, withPursuit: false);
			break;
		default:
			BaseBrain = new GClass355(BotOwner_0);
			break;
		case WildSpawnType.sectantPredvestnik:
			BaseBrain = new GClass361(BotOwner_0);
			break;
		case WildSpawnType.sectantPrizrak:
			BaseBrain = new GClass363(BotOwner_0);
			break;
		case WildSpawnType.sectantOni:
			BaseBrain = new GClass353(BotOwner_0);
			break;
		case WildSpawnType.infectedAssault:
		case WildSpawnType.infectedPmc:
		case WildSpawnType.infectedCivil:
		case WildSpawnType.infectedLaborant:
			BaseBrain = new GClass324(BotOwner_0);
			break;
		case WildSpawnType.bossTagilla:
		case WildSpawnType.infectedTagilla:
			BaseBrain = new GClass319(BotOwner_0);
			break;
		case WildSpawnType.bossTagillaAgro:
			BaseBrain = new GClass318(BotOwner_0);
			break;
		case WildSpawnType.bossKillaAgro:
			BaseBrain = new GClass315(BotOwner_0);
			break;
		case WildSpawnType.tagillaHelperAgro:
			BaseBrain = new GClass364(BotOwner_0);
			break;
		}
		BaseBrain baseBrain = BaseBrain;
		string name = BotOwner_0.name + " " + BotOwner_0.Profile.Info.Settings.Role;
		Agent = new global::AICoreAgentClass<BotLogicDecision>(BotOwner_0.BotsController.AICoreController, baseBrain, BotActionNodesClass.ActionsList(BotOwner_0), BotOwner_0.gameObject, name, (BotLogicDecision decision) => BotActionNodesClass.CreateNode(decision, BotOwner_0));
		this.OnSetStrategy?.Invoke(BaseBrain);
	}

	public bool IsProblem()
	{
		return Agent.IsProblem();
	}

	public void SetHandlerName(string info)
	{
		GetStateName = info;
	}

	public void Deactivate()
	{
		BaseBrain.Dispose();
	}

	public string ActiveLayerName()
	{
		return Agent.UsingLayer;
	}

	public string GetLastNode()
	{
		return Agent.GetActiveNodeName();
	}

	public string GetActiveNodeReason()
	{
		return Agent.GetActiveNodeReason();
	}

	public CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> getCoverPointMain2, bool checkCurrent)
	{
		return BaseBrain.FindPoint(data, getCoverPointMain2, checkCurrent);
	}

	public string DebugHandler()
	{
		return $"{BotOwner_0.Id} {BaseBrain.DebugHandler()}\n{GetLastNode()}";
	}

	public void Dispose()
	{
		BaseBrain?.Dispose();
		Agent?.Dispose();
	}

	public int NodeRepeats()
	{
		return Agent.NodeRepeats();
	}

	[CompilerGenerated]
	public BotNodeAbstractClass method_0(BotLogicDecision decision)
	{
		return BotActionNodesClass.CreateNode(decision, BotOwner_0);
	}
}
