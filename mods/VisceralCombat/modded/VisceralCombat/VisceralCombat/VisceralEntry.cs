using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using Newtonsoft.Json;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Combat.Patches;
using VisceralCombat.Combined.Patches;
using VisceralCombat.Dismemberment.Classes;
using VisceralCombat.Dismemberment.Classes.Packets;
using VisceralCombat.Dismemberment.Patches;
using VisceralCombat.Ragdolls.Classes;
using VisceralCombat.Ragdolls.Classes.Packets;
using VisceralCombat.Ragdolls.Patches;

namespace VisceralCombat;

[BepInPlugin("com.servph.VisceralCombat", "Visceral Combat", "3.8.1")]
/// <remarks>
/// GUID used for FIKA mod-presence checks. Must match BepInPlugin first arg.
/// </remarks>
public class VisceralEntry : BaseUnityPlugin
{
	public List<string> SoundsList = new List<string> { "ThroatGargle1", "ThroatGargle2", "ThroatGargle3", "ThroatGargle4" };

	public List<string> BloodFXList = new List<string>
	{
		"Blood1", "Blood2", "Blood2_Left", "Blood2_Right", "Blood3", "Blood4", "Blood5", "Blood6", "Blood7", "Blood8",
		"Blood9", "Blood10", "Blood11", "Blood12", "Blood13", "Blood15"
	};

	public EffectContainer effectContainer = null;

	private string filePath = "";

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
	public static ManualLogSource LogSource { get; private set; }

	/// <summary>
	/// True only when ALL human players in the current FIKA raid have VisceralCombat.
	/// Always true in solo SPT (no FIKA). Feature-gating flag for living-leg dismemberment.
	/// </summary>
	public static bool AllPlayersHaveVisceralCombat { get; private set; } = false;

	// Handshake internals
	private readonly HashSet<int> _handshakeAcks = new();
	private int _expectedHumanCount = 0;
	private const string VisceralGuid = "com.servph.VisceralCombat";

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

	public GameObject ragdollAnimMaster { get; set; }

	public ConfigEntry<bool> NeverDeleteShells { get; set; }

	public void Awake()
	{
		Instance = this;
		LogSource = Logger;

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
		((ModulePatch)new VisceralCombat.Combined.Patches.PlaySoundBankPatch()).Enable();
		((ModulePatch)new VisceralCombat.Combined.Patches.PlayStepSoundPatch()).Enable();
		((ModulePatch)new VisceralCombat.Combined.Patches.DefaultPlayPatch()).Enable();

		try
		{
			ConsoleScreen.Processor.RegisterCommand("UpdateCalibers", (Action)delegate
			{
				ParseDismembermentJson();
			}, (string)null);
			ConsoleScreen.Processor.RegisterCommand("LayerCheck", (Action)delegate
			{
				LayerMaskRun();
			}, (string)null);
		}
		catch { }

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
		ItemForce = ((BaseUnityPlugin)this).Config.Bind<bool>("Physics | Item Physical Properties", "Item Physics", false, "If you are getting too much lag turn this off. But most capable PC's should run this fine. (Besides on SoT)");
		objectIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Physics | Item Physical Properties", "Item Force Intensity", 0.3f, "Multiplier that determines the amount of force applied to physics objects.");
		headForceIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Ragdoll Physical Properties", "Head Impulse Intensity", 1.0f, "Multiplier for head shot force.");
		TorsoForceIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Ragdoll Physical Properties", "Torso Impulse Intensity", 1.0f, "Multiplier for torso shot force.");
		ArmsForceIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Ragdoll Physical Properties", "Arms Impulse Intensity", 1.0f, "Multiplier for arms shot force.");
		LegsForceIntensity = ((BaseUnityPlugin)this).Config.Bind<float>("Ragdolls | Ragdoll Physical Properties", "Legs Impulse Intensity", 1.0f, "Multiplier for legs shot force.");
	}

	private void onFikaNetworkManagerCreatedEvent(FikaNetworkManagerCreatedEvent @event)
	{
		if (FikaBackendUtils.IsClient)
		{
			@event.Manager.RegisterPacket<DismembermentPacket>((Action<DismembermentPacket>)OnDismembermentPacket);
			@event.Manager.RegisterPacket<RagdollSyncPacket>((Action<RagdollSyncPacket>)OnRagdollSyncPacket);
			// Client-side: receive handshake ping from host and reply with ACK
			@event.Manager.RegisterPacket<VisceralHandshakePacket>((Action<VisceralHandshakePacket>)OnVisceralHandshakePacketClient);
		}
		if (FikaBackendUtils.IsServer || FikaBackendUtils.IsHeadless)
		{
			// Host-side: receive ACK responses from clients
			@event.Manager.RegisterPacket<VisceralHandshakePacket>((Action<VisceralHandshakePacket>)OnVisceralHandshakePacketServer);
		}
	}

	/// <summary>Called on clients when the host broadcasts a handshake ping.</summary>
	private void OnVisceralHandshakePacketClient(VisceralHandshakePacket packet)
	{
		if (!packet.IsRequest) return; // ignore stray ACKs
		// Reply ACK to host: we have the mod
		if (!Singleton<FikaClient>.Instantiated || Singleton<FikaClient>.Instance == null) return;
		var myPlayer = Singleton<FikaClient>.Instance.CoopHandler?.MyPlayer;
		if (myPlayer == null) return;

		VisceralHandshakePacket ack = new()
		{
			IsRequest = false,
			ResponderNetId = myPlayer.NetId
		};
		Singleton<FikaClient>.Instance.SendData<VisceralHandshakePacket>(ref ack, (DeliveryMethod)0, false);
	}

	/// <summary>Called on the host when a client sends an ACK.</summary>
	private void OnVisceralHandshakePacketServer(VisceralHandshakePacket packet)
	{
		if (packet.IsRequest) return; // ignore stray pings
		_handshakeAcks.Add(packet.ResponderNetId);
	}

	/// <summary>
	/// Called by GameStartedPatch to begin the handshake.
	/// In solo SPT (no FIKA server running) the flag is enabled immediately.
	/// In FIKA, the host broadcasts a ping and waits up to 5 seconds for all ACKs.
	/// </summary>
	public void StartVisceralHandshake()
	{
		_handshakeAcks.Clear();
		AllPlayersHaveVisceralCombat = false;

		bool fikaServerUp = Singleton<FikaServer>.Instantiated && Singleton<FikaServer>.Instance != null;
		if (!fikaServerUp)
		{
			// Solo SPT: always enable
			AllPlayersHaveVisceralCombat = true;
			QuickLogger.Log(ELogType.Log, "[VisceralCombat] Solo raid — LivingDismemberment enabled.");
			return;
		}

		// FIKA raid: host sends ping to all clients
		// Use CoopHandler.AmountOfHumans from the FIKA server
		var serverCoopHandler = Singleton<FikaServer>.Instance.CoopHandler;
		_expectedHumanCount = serverCoopHandler != null ? serverCoopHandler.AmountOfHumans - 1 : 0; // -1 for host (host counts as confirmed)

		VisceralHandshakePacket ping = new() { IsRequest = true, ResponderNetId = 0 };
		Singleton<FikaServer>.Instance.SendData<VisceralHandshakePacket>(ref ping, (DeliveryMethod)0, false);
		QuickLogger.Log(ELogType.Log, $"[VisceralCombat] FIKA handshake sent — expecting {_expectedHumanCount} client ACKs.");

		// Wait up to 5 seconds then evaluate
		StartCoroutine(EvaluateHandshakeAfterDelay(5f));
	}

	private IEnumerator EvaluateHandshakeAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);

		int acks = _handshakeAcks.Count;
		bool allConfirmed = acks >= _expectedHumanCount;
		AllPlayersHaveVisceralCombat = allConfirmed;

		if (allConfirmed)
			QuickLogger.Log(ELogType.Log, $"[VisceralCombat] All {acks}/{_expectedHumanCount} players confirmed mod — LivingDismemberment ENABLED.");
		else
			QuickLogger.Log(ELogType.Log, $"[VisceralCombat] Only {acks}/{_expectedHumanCount} players confirmed mod — LivingDismemberment DISABLED.");
	}

	private void OnDismembermentPacket(DismembermentPacket packet)
	{
		Transform[] affectedLimbs = null;
		QuickLogger.Log(ELogType.Log, $"Dismemberment Packet Received: {packet.playerID}, {packet.Direction}, {packet.bodyPartType}, {packet.bone}, {packet.capAssetName}, {packet.assetNames}");
		KillPatch.DismemberLimb((Player)(object)Singleton<FikaClient>.Instance.CoopHandler.Players[packet.playerID], packet.Direction, packet.bodyPartType, packet.bone, packet.capAssetName, packet.assetNames, out affectedLimbs);
	}

	private void OnRagdollSyncPacket(RagdollSyncPacket packet)
	{
		QuickLogger.Log(ELogType.Log, $"Ragdoll Packet Received: {packet.PlayerID}, {packet.BodyPart}, {packet.RandomChance}");
		KillPatch.DeathSetup((Player)(object)Singleton<FikaClient>.Instance.CoopHandler.Players[packet.PlayerID], packet.BodyPart, packet.RandomChance);
	}

	private void Start()
	{
		string pluginRoot = BepInEx.Paths.PluginPath;
		string moddedPath1 = Path.Combine(pluginRoot, "VisceralCombat", "ssh", "VD_Calibers.json");
		string moddedPath2 = Path.Combine(pluginRoot, "VisceralCombat", "VD_Calibers.json");
		string legacyPath  = Path.Combine(pluginRoot, "ssh", "VD_Calibers.json");

		if (File.Exists(moddedPath1))      filePath = moddedPath1;
		else if (File.Exists(moddedPath2)) filePath = moddedPath2;
		else if (File.Exists(legacyPath))  filePath = legacyPath;

		if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
		{
			ParseDismembermentJson();
			QuickLogger.Log(ELogType.Log, $"[VisceralCombat] Loaded {KillPatch.calibers.Count} dismember calibers from: {filePath}");
		}
		else
		{
			QuickLogger.Log(ELogType.Error, $"[VisceralCombat] VD_Calibers.json NOT FOUND!");
		}
	}

	internal LayerMask LayerMaskConstructor(string[] layers)
	{
		int num = 0;
		foreach (string text in layers)
		{
			num |= 1 << LayerMask.NameToLayer(text);
		}
		return (LayerMask)num;
	}

	internal void LayerMaskRun()
	{
		LayerMask val = LayerMaskConstructor(WorldLayers.Concat(DeadBodyLayers.Concat(HitColliderLayers)).ToArray());
		QuickLogger.Log(ELogType.Log, val.ToString());
	}

	public void ParseDismembermentJson()
	{
		string text = File.ReadAllText(filePath);
		List<Dictionary<string, object>> list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(text);
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
}
