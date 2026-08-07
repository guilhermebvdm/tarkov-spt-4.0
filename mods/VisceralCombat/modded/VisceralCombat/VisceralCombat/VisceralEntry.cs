using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.UI;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Newtonsoft.Json;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using VisceralCombat.Combat.Patches;
using VisceralCombat.Combined.Patches;
using VisceralCombat.Dismemberment.Classes;
using VisceralCombat.Dismemberment.Classes.Packets;
using VisceralCombat.Dismemberment.Patches;
using VisceralCombat.Ragdolls.Classes;
using VisceralCombat.Ragdolls.Classes.Packets;
using VisceralCombat.Ragdolls.Patches;

namespace VisceralCombat;

[BepInPlugin("com.servph.VisceralCombat", "Visceral Combat", "3.7.1")]
public class VisceralEntry : BaseUnityPlugin
{
	public List<string> SoundsList = new List<string> { "ThroatGargle1", "ThroatGargle2", "ThroatGargle3", "ThroatGargle4" };

	public List<string> BloodFXList = new List<string>
	{
		"Blood1", "Blood2", "Blood2_Left", "Blood2_Right", "Blood3", "Blood4", "Blood5", "Blood6", "Blood7", "Blood8",
		"Blood9", "Blood10", "Blood11", "Blood12", "Blood13", "Blood15"
	};

	public EffectContainer effectContainer = null;

	private string filePath = "./BepInEx/plugins/ssh/VD_Calibers.json";

	public float lerpTest = 1f;

	internal static readonly string[] WorldLayers = new string[4] { "Water", "Terrain", "HighPolyCollider", "TransparentCollider" };

	internal static readonly string[] DeadBodyLayers = new string[2] { "Deadbody", "TransparentFX" };

	internal static readonly string[] HitColliderLayers = new string[1] { "HitCollider" };

	internal static readonly string[] AuraLayers = new string[1] { "PlayerSpiritAura" };

	internal List<Player> dismemberedPlayers = new List<Player>();

	internal Dictionary<Player, int> deadPlayers = new Dictionary<Player, int>();

	internal int deadBodyTimer = 0;

	public GameWorld gameWorld;

	public GameObject weapGameObject;

	public BoxCollider boxCollider;

	public Rigidbody weapRigid;

	public Joint attachJoint;

	public CollisionDetectionMode collisionDetectionMode_0;

	public static VisceralEntry Instance { get; private set; }

	public AssetBundle goreBundle { get; set; }

	public AssetBundle bloodfxBundle { get; set; }

	public AssetBundle bloodsfxBundle { get; set; }

	public List<GameObject> goreCaps { get; set; }

	public List<GameObject> BloodFX { get; set; }

	public List<GameObject> BloodSFX { get; set; }

	public ConfigEntry<bool> EnableDismemberment { get; set; }

	public ConfigEntry<bool> EnableBloodEffects { get; set; }

	public ConfigEntry<float> BloodSplatterSize { get; set; }

	public ConfigEntry<float> ArterySprayMin { get; set; }

	public ConfigEntry<float> ArterySprayMax { get; set; }

	public ConfigEntry<float> HitSprayMax { get; set; }

	public ConfigEntry<int> MaxDecals { get; set; }

	public ConfigEntry<bool> ArterySpray { get; set; }

	public ConfigEntry<bool> UseOldBloodDecal { get; set; }

	public ConfigEntry<bool> BodyCollision { get; set; }

	public ConfigEntry<bool> ItemForce { get; set; }

	public ConfigEntry<bool> ShootHelmetOff { get; set; }

	public ConfigEntry<bool> IsSlingingEnabled { get; set; }

	public ConfigEntry<bool> UseActiveRagdolls { get; set; }

	public ConfigEntry<bool> DisableRagdollsAfterTime { get; set; }

	public ConfigEntry<float> RagdollDisableTime { get; set; }

	public ConfigEntry<float> ShotIntensity { get; set; }

	public ConfigEntry<float> HelmetShootOffChance { get; set; }

	public ConfigEntry<float> AnimSwapDuration { get; set; }

	public ConfigEntry<float> MappingWeightDuration { get; set; }

	public ConfigEntry<float> GrenadeExplIntensity { get; set; }

	public ConfigEntry<float> objectIntensity { get; set; }

	public ConfigEntry<float> headForceIntensity { get; set; }

	public ConfigEntry<float> TorsoForceIntensity { get; set; }

	public ConfigEntry<float> ArmsForceIntensity { get; set; }

	public ConfigEntry<float> LegsForceIntensity { get; set; }

	public ConfigEntry<bool> OnlyPlayersCanActiveRagdollEnemies { get; set; }

	public ConfigEntry<int> RagdollMaxDistance { get; set; }

	public ConfigEntry<int> RagdollSleepTime { get; set; }

	public ConfigEntry<float> timer { get; set; }

	public ConfigEntry<float> x { get; set; }

	public ConfigEntry<float> y { get; set; }

	public ConfigEntry<float> z { get; set; }

	public GameObject ragdollAnimMaster { get; set; }

	public ConfigEntry<bool> NeverDeleteShells { get; set; }

	public void Awake()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Expected O, but got Unknown
		Instance = this;
		EnableDismemberment = ((BaseUnityPlugin)this).Config.Bind<bool>("Dismemberment", "Dismemberment Enabled", true, new ConfigDescription("Disables literally EVERYTHING for dismemberment.", (AcceptableValueBase)null, new object[1]
		{
			new ConfigurationManagerAttributes
			{
				IsAdvanced = true,
				Order = 3
			}
		}));
		EnableBloodEffects = ((BaseUnityPlugin)this).Config.Bind<bool>("Blood", "Blood Effects Enabled", true, new ConfigDescription("Disables literally EVERYTHING for blood.", (AcceptableValueBase)null, new object[1]
		{
			new ConfigurationManagerAttributes
			{
				IsAdvanced = true,
				Order = 3
			}
		}));
		BloodSplatterSize = ((BaseUnityPlugin)this).Config.Bind<float>("Blood | Splatters", "Blood Splatter Size", 1f, (ConfigDescription)null);
		UseOldBloodDecal = ((BaseUnityPlugin)this).Config.Bind<bool>("Blood | Splatters", "Use Old Blood Decals", false, "Hides the old blood decals that stain the floor. Keeps the actual effect though.");
		ArterySpray = ((BaseUnityPlugin)this).Config.Bind<bool>("Blood | Trails & Flows", "Arterial Spraying", true, (ConfigDescription)null);
		ArterySprayMin = ((BaseUnityPlugin)this).Config.Bind<float>("Blood | Trails & Flows", "Arterial Spray Minimum Time (Seconds)", 8f, new ConfigDescription("", (AcceptableValueBase)null, new object[1]
		{
			new ConfigurationManagerAttributes
			{
				IsAdvanced = true,
				Order = 3
			}
		}));
		ArterySprayMax = ((BaseUnityPlugin)this).Config.Bind<float>("Blood | Trails & Flows", "Arterial Spray Maxmimum Time (Seconds)", 2f, new ConfigDescription("", (AcceptableValueBase)null, new object[1]
		{
			new ConfigurationManagerAttributes
			{
				IsAdvanced = true,
				Order = 3
			}
		}));
		HitSprayMax = ((BaseUnityPlugin)this).Config.Bind<float>("Blood | Spurts", "Bleed Maxmimum Time (Seconds)", 2f, new ConfigDescription("", (AcceptableValueBase)null, new object[1]
		{
			new ConfigurationManagerAttributes
			{
				IsAdvanced = true,
				Order = 3
			}
		}));
		MaxDecals = ((BaseUnityPlugin)this).Config.Bind<int>("Blood | Performance", "Maximum Ground Decals", 2048, new ConfigDescription("Maximum of BSG's Blood Decals that can be placed on the floor. Changes upon next Raid. Be careful with this value!!", (AcceptableValueBase)null, new object[1]
		{
			new ConfigurationManagerAttributes
			{
				IsAdvanced = true,
				Order = 3
			}
		}));
		((ModulePatch)new VisceralCombat.Dismemberment.Patches.GameStartedPatch()).Enable();
		((ModulePatch)new KillPatch()).Enable();
		((ModulePatch)new BleedPatch()).Enable();
		ConsoleScreen.Processor.RegisterCommand("UpdateCalibers", (Action)delegate
		{
			ParseDismembermentJson();
		}, (string)null);
		ConsoleScreen.Processor.RegisterCommand("LayerCheck", (Action)delegate
		{
			LayerMaskRun();
		}, (string)null);
		FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>((Action<FikaNetworkManagerCreatedEvent>)onFikaNetworkManagerCreatedEvent);
		ShotIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Ragdoll Phsyical Properties", "Bullet Intensity", 85f, "How much force is applied to a shot. This is also dependent on caliber. Default is 85");
		GrenadeExplIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Ragdoll Phsyical Properties", "Grenade Intensity", 190f, "How much force is applied to a grenade explosion. This is also dependent on caliber. Default is 190");
		BodyCollision = ((BaseUnityPlugin)this).Config.Bind<bool>("Ragdolls | Ragdoll Phsyical Properties", "Player Body Collision", false, "Allows you to step on bodies. You can potentially get stuck on them once in awhile for brief moments. Turn this off if you do not like it.");
		ShootHelmetOff = ((BaseUnityPlugin)this).Config.Bind<bool>("Ragdolls | Character Properties", "Shoot off Helmets", true, (ConfigDescription)null);
		HelmetShootOffChance = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Character Properties", "Helmet Knock Off Chance", 15f, (ConfigDescription)null);
		AnimSwapDuration = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Character Properties", "Duration for anim swap", 1f, (ConfigDescription)null);
		MappingWeightDuration = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Character Properties", "Duration for Mapping Weight swap", 1f, (ConfigDescription)null);
		UseActiveRagdolls = ((BaseUnityPlugin)this).Config.Bind<bool>("Ragdolls | Character Properties", "Use Active Ragdolls", true, (ConfigDescription)null);
		DisableRagdollsAfterTime = ((BaseUnityPlugin)this).Config.Bind<bool>("Ragdolls | Performance", "Disable Active Ragdolls After Animation", true, (ConfigDescription)null);
		OnlyPlayersCanActiveRagdollEnemies = ((BaseUnityPlugin)this).Config.Bind<bool>("Ragdolls | Performance", "Allow AI to Activate Ragdolls", false, (ConfigDescription)null);
		RagdollMaxDistance = ((BaseUnityPlugin)this).Config.Bind<int>("Ragdolls | Performance", "Max Distance the Ragdolls can Activate at", 50, (ConfigDescription)null);
		RagdollSleepTime = ((BaseUnityPlugin)this).Config.Bind<int>("Ragdolls | Performance", "Ragdoll Sleep Time", 60, "Experiemental! May cause bodies to go be sent into ORBIT.");
		((ModulePatch)new BodiesImpulsePatch()).Enable();
		((ModulePatch)new CreateCorpsePatch()).Enable();
		((ModulePatch)new GrenadeDeadBodiesPatch()).Enable();
		((ModulePatch)new GrenadeItemsPatch()).Enable();
		((ModulePatch)new VisceralCombat.Ragdolls.Patches.GameStartedPatch()).Enable();
		((ModulePatch)new PhysicalItemsPatch()).Enable();
		((ModulePatch)new ShootOffHelmetPatch()).Enable();
		((ModulePatch)new AttachWeaponPatch()).Enable();
		((ModulePatch)new PlayerInitPatch()).Enable();
		((ModulePatch)new LimbKillPatch()).Enable();
		((ModulePatch)new CreateBSGRagdollPatch()).Enable();
		((ModulePatch)new RagdollClassPatch()).Enable();
		((ModulePatch)new VisceralCombat.Ragdolls.Patches.MovementContextPatch()).Enable();
		NeverDeleteShells = ((BaseUnityPlugin)this).Config.Bind<bool>("Combat | Visuals", "Infinite Shell Casing Lifetime", false, "Turns off Used Shell Casing Deletion");
		((ModulePatch)new ShellCasingPatch()).Enable();
		ItemForce = ((BaseUnityPlugin)this).Config.Bind<bool>("Physics | Item Physical Properties", "Item Physics", false, "If you are getting too much lag turn this off. But most capable PC's should run this fine. (Besides on SoT)");
		objectIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Physics | Item Physical Properties", "Item Force Intensity", 0.3f, "Multiplier that determines the amount of force applied to physics objects.");
	}

	private void onFikaNetworkManagerCreatedEvent(FikaNetworkManagerCreatedEvent @event)
	{
		if (FikaBackendUtils.IsClient)
		{
			@event.Manager.RegisterPacket<DismembermentPacket>((Action<DismembermentPacket>)OnDismembermentPacket);
			@event.Manager.RegisterPacket<RagdollSyncPacket>((Action<RagdollSyncPacket>)OnRagdollSyncPacket);
		}
	}

	private void OnDismembermentPacket(DismembermentPacket packet)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Transform[] affectedLimbs = null;
		QuickLogger.Log(ELogType.Log, $"Dismemberment Packet Received: {packet.playerID}, {packet.Direction}, {packet.bodyPartType}, {packet.bone}, {packet.capAssetName}, {packet.assetNames}");
		KillPatch.DismemberLimb((Player)(object)Singleton<FikaClient>.Instance.CoopHandler.Players[packet.playerID], packet.Direction, packet.bodyPartType, packet.bone, packet.capAssetName, packet.assetNames, out affectedLimbs);
	}

	private void OnRagdollSyncPacket(RagdollSyncPacket packet)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		QuickLogger.Log(ELogType.Log, $"Ragdoll Packet Received: {packet.PlayerID}, {packet.BodyPart}, {packet.RandomChance}");
		KillPatch.DeathSetup((Player)(object)Singleton<FikaClient>.Instance.CoopHandler.Players[packet.PlayerID], packet.BodyPart, packet.RandomChance);
	}

	private void Start()
	{
		string moddedPath1 = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "VisceralCombat", "ssh", "VD_Calibers.json");
		string moddedPath2 = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "VisceralCombat", "VD_Calibers.json");
		string legacyPath = Path.Combine(Environment.CurrentDirectory, "BepInEx", "plugins", "ssh", "VD_Calibers.json");

		if (File.Exists(moddedPath1)) filePath = moddedPath1;
		else if (File.Exists(moddedPath2)) filePath = moddedPath2;
		else if (File.Exists(legacyPath)) filePath = legacyPath;

		if (File.Exists(filePath))
		{
			ParseDismembermentJson();
		}
		else
		{
			QuickLogger.Log(ELogType.Warn, $"Config file '{filePath}' not found. Dismemberment/bleed calibers will use defaults.");
		}
	}

	internal LayerMask LayerMaskConstructor(string[] layers)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (string text in layers)
		{
			num |= 1 << LayerMask.NameToLayer(text);
		}
		return num;
	}

	internal void LayerMaskRun()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		LayerMask val = LayerMaskConstructor(WorldLayers.Concat(DeadBodyLayers.Concat(HitColliderLayers)).ToArray());
		QuickLogger.Log(ELogType.Log, val.ToString());
	}

	public void ParseDismembermentJson()
	{
		string text = File.ReadAllText(filePath);
		List<Dictionary<string, object>> list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(text);
		BleedPatch.light_calibers.Clear();
		BleedPatch.heavy_calibers.Clear();
		foreach (Dictionary<string, object> item in list)
		{
			if (item.ContainsKey("dismember_calibers"))
			{
				Dictionary<string, float> dictionary = JsonConvert.DeserializeObject<Dictionary<string, float>>(item["dismember_calibers"].ToString());
				foreach (KeyValuePair<string, float> item2 in dictionary)
				{
					KillPatch.calibers[item2.Key] = item2.Value;
				}
			}
			if (item.ContainsKey("bleed_calibers"))
			{
				Dictionary<string, float> dictionary2 = JsonConvert.DeserializeObject<Dictionary<string, float>>(item["bleed_calibers"].ToString());
				foreach (KeyValuePair<string, float> item3 in dictionary2)
				{
					BleedPatch.calibers[item3.Key] = item3.Value;
				}
			}
			if (item.ContainsKey("ragdoll_limb_chances"))
			{
				Dictionary<string, float> dictionary3 = JsonConvert.DeserializeObject<Dictionary<string, float>>(item["ragdoll_limb_chances"].ToString());
				foreach (KeyValuePair<string, float> item4 in dictionary3)
				{
					RagdollHelperClass.limb_chances[item4.Key] = item4.Value;
				}
			}
			if (item.ContainsKey("light_bleed_calibers"))
			{
				List<string> collection = JsonConvert.DeserializeObject<List<string>>(item["light_bleed_calibers"].ToString());
				BleedPatch.light_calibers.AddRange(collection);
			}
			if (item.ContainsKey("heavy_bleed_calibers"))
			{
				List<string> collection2 = JsonConvert.DeserializeObject<List<string>>(item["heavy_bleed_calibers"].ToString());
				BleedPatch.heavy_calibers.AddRange(collection2);
			}
		}
		if (KillPatch.calibers.ContainsKey("12g"))
		{
			QuickLogger.Log(ELogType.Log, "Calibers Found & Added.");
		}
	}

	private void LayerMaskOutput()
	{
		List<LayerCollisionData> list = new List<LayerCollisionData>();
		for (int i = 0; i < 32; i++)
		{
			string text = LayerMask.LayerToName(i);
			if (string.IsNullOrEmpty(text))
			{
				text = $"Layer_{i}";
			}
			LayerCollisionData layerCollisionData = new LayerCollisionData();
			layerCollisionData.layerName = text;
			layerCollisionData.collidesWith = new List<string>();
			for (int j = 0; j < 32; j++)
			{
				if (i != j)
				{
					string text2 = LayerMask.LayerToName(j);
					if (string.IsNullOrEmpty(text2))
					{
						text2 = $"{j}";
					}
					if (!Physics.GetIgnoreLayerCollision(i, j))
					{
						layerCollisionData.collidesWith.Add(text2);
					}
				}
			}
			list.Add(layerCollisionData);
		}
		string contents = JsonConvert.SerializeObject((object)new
		{
			layers = list
		}, (Formatting)1);
		string text3 = Path.Combine(Application.dataPath, "LayerCollisionData.json");
		File.WriteAllText(text3, contents);
		Debug.Log((object)("Layer collision data saved to: " + text3));
	}
}
