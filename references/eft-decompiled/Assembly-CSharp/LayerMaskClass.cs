using UnityEngine;

public abstract class LayerMaskClass
{
	public static readonly LayerMask HighPolyWithTerrainMask;

	public static readonly LayerMask HighPolyWithTerrainNoGrassMask;

	public static readonly LayerMask PlayerMask;

	public static readonly LayerMask HighPolyWithTerrainMaskAI;

	public static readonly LayerMask layerMask_0;

	public static readonly LayerMask HighPolyCollider;

	public static readonly LayerMask TerrainLowPoly;

	public static readonly LayerMask TripwireCheckLayerMaskNoPlayer;

	public static readonly LayerMask TripwireCheckLayerMask;

	public static readonly LayerMask DefaultLayerMask;

	public static readonly LayerMask AI;

	public static readonly LayerMask Grass;

	public static readonly LayerMask Foliage;

	public static readonly LayerMask TerrainLayer;

	public static readonly LayerMask TerrainMask;

	public static readonly LayerMask HitColliderMask;

	public static readonly LayerMask PlayerCollisionsMask;

	public static readonly LayerMask PlayerStaticDoorMask;

	public static readonly LayerMask PlayerStaticCollisionsMask;

	public static readonly LayerMask ShellsCollisionsMask;

	public static readonly LayerMask PlayerCollisionTestMask;

	public static readonly LayerMask GrenadeAffectedMask;

	public static readonly LayerMask GrenadeObstaclesColliderMask;

	public static readonly LayerMask WaterLayer;

	public static readonly LayerMask LootLayerMask;

	public static readonly LayerMask LootLayer;

	public static readonly LayerMask InteractiveMask;

	public static readonly LayerMask InteractiveLayer;

	public static readonly LayerMask LootCollisionMask;

	public static readonly LayerMask TriggersLayer;

	public static readonly LayerMask TriggersMask;

	public static readonly LayerMask AudioControllerStepLayerMask;

	public static readonly LayerMask TransparentLayerMask;

	public static readonly int DisablerCullingObjectLayer;

	public static readonly int DisablerCullingObjectLayerMask;

	public static int PlayerLayer;

	public static int DeadbodyLayer;

	public static int HitColliderLayer;

	public static int DoorLayer;

	public static int LowPolyColliderLayer;

	public static LayerMask LowPolyColliderLayerMask;

	public static int ShellsLayer;

	public static int PlayerCollisionTestLayer;

	public static int WeaponPreview;

	static LayerMaskClass()
	{
		PlayerLayer = LayerMask.NameToLayer("Player");
		DeadbodyLayer = LayerMask.NameToLayer("Deadbody");
		HitColliderLayer = LayerMask.NameToLayer("HitCollider");
		LowPolyColliderLayer = LayerMask.NameToLayer("LowPolyCollider");
		LowPolyColliderLayerMask = 1 << LowPolyColliderLayer;
		TerrainLowPoly = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LowPolyColliderLayer);
		HighPolyWithTerrainMask = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("HighPolyCollider"));
		HighPolyWithTerrainMaskAI = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("HighPolyCollider")) | (1 << LayerMask.NameToLayer("Grass")) | (1 << LayerMask.NameToLayer("Foliage"));
		HighPolyWithTerrainNoGrassMask = (1 << LayerMask.NameToLayer("Terrain")) | (1 << LayerMask.NameToLayer("HighPolyCollider")) | (1 << LayerMask.NameToLayer("Foliage"));
		HighPolyCollider = 1 << LayerMask.NameToLayer("HighPolyCollider");
		PlayerMask = 1 << PlayerLayer;
		Grass = 1 << LayerMask.NameToLayer("Grass");
		Foliage = 1 << LayerMask.NameToLayer("Foliage");
		TerrainLayer = LayerMask.NameToLayer("Terrain");
		TerrainMask = 1 << (int)TerrainLayer;
		layerMask_0 = (int)HighPolyWithTerrainMask | (1 << HitColliderLayer);
		DefaultLayerMask = 1 << LayerMask.NameToLayer("Default");
		AI = GClass1458.Create("Foliage", "Grass");
		HitColliderMask = 1 << HitColliderLayer;
		PlayerCollisionsMask = GClass1458.GetAllCollisionsLayerMask(PlayerLayer);
		PlayerStaticCollisionsMask = (int)PlayerCollisionsMask & ~(int)PlayerMask;
		ShellsCollisionsMask = GClass1458.GetAllCollisionsLayerMask(LayerMask.NameToLayer("Shells"));
		PlayerCollisionTestLayer = LayerMask.NameToLayer("PlayerCollisionTest");
		PlayerCollisionTestMask = LayerMask.GetMask("LowPolyCollider", "DoorLowPolyCollider", "Terrain", "Player");
		GrenadeAffectedMask = LayerMask.GetMask("Player", "Interactive");
		GrenadeObstaclesColliderMask = LayerMask.GetMask("HighPolyCollider", "DoorLowPolyCollider", "Terrain");
		PlayerStaticDoorMask = LayerMask.GetMask("DoorLowPolyCollider");
		DisablerCullingObjectLayer = LayerMask.NameToLayer("DisablerCullingObject");
		DisablerCullingObjectLayerMask = 1 << DisablerCullingObjectLayer;
		DoorLayer = LayerMask.NameToLayer("DoorLowPolyCollider");
		ShellsLayer = LayerMask.NameToLayer("Shells");
		WaterLayer = LayerMask.NameToLayer("Water");
		LootLayer = LayerMask.NameToLayer("Loot");
		LootLayerMask = 1 << (int)LootLayer;
		LootCollisionMask = GClass1458.GetAllCollisionsLayerMask(LootLayer);
		InteractiveLayer = LayerMask.NameToLayer("Interactive");
		InteractiveMask = 1 << (int)InteractiveLayer;
		TriggersLayer = LayerMask.NameToLayer("Triggers");
		TriggersMask = 1 << (int)TriggersLayer;
		TripwireCheckLayerMask = (int)TerrainLowPoly | (1 << DoorLayer) | (int)LootLayerMask | (int)PlayerMask | (int)DefaultLayerMask | (int)HighPolyCollider;
		TripwireCheckLayerMaskNoPlayer = (int)TerrainLowPoly | (1 << DoorLayer) | (int)LootLayerMask | (int)DefaultLayerMask | (int)HighPolyCollider;
		AudioControllerStepLayerMask = LayerMask.GetMask("Terrain", "HighPolyCollider", "TransparentCollider", "Water");
		WeaponPreview = LayerMask.NameToLayer("Weapon Preview");
		TransparentLayerMask = 1 << LayerMask.NameToLayer("TransparentCollider");
	}

	public static int HighPolyAsLayer()
	{
		return LayerMask.NameToLayer("HighPolyCollider");
	}

	public static int LowPolyAsLayer()
	{
		return LayerMask.NameToLayer("LowPolyCollider");
	}
}
