using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EFT;

public class ClientLocalGameWorld : ClientGameWorld
{
	public override void Awake()
	{
		SpeedLimitsEnabled = true;
		base.Awake();
	}

	public override bool IsLocalGame()
	{
		return true;
	}

	public override void PlayerTick(float dt)
	{
		base.PlayerTick(dt);
		vmethod_1(dt);
	}

	public virtual void vmethod_1(float dt)
	{
		base.ClientSynchronizableObjectLogicProcessor.RemoveNonActiveAndStaticObjects();
		base.ClientSynchronizableObjectLogicProcessor.ManualUpdate(dt);
	}

	public override Task InitLevel(ItemFactoryClass itemFactory, ObjectsFactoryDataClass config, bool loadBundlesAndCreatePools = true, List<ResourceKey> resources = null, IProgress<LoadingProgressStruct> progress = null, CancellationToken ct = default(CancellationToken))
	{
		base.TriggersEmitter = new GClass3594();
		return base.InitLevel(itemFactory, config, loadBundlesAndCreatePools, resources, progress, ct);
	}
}
