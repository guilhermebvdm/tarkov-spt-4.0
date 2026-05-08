using System;
using System.Collections.Generic;
using EFT;
using EFT.SynchronizableObjects;
using UnityEngine;

public abstract class ResourceKeyManagerAbstractClass
{
	public const string DEFAULT_BALLISTIC_COLLIDER = "assets/content/commonprefabs/defaultballisticcollider.bundle";

	public const string EFFECTS_BUNDLE_NAME = "assets/systems/effects/particlesystems/effects.bundle";

	public const string PLAYER_SPRING_BUNDLE_NAME = "assets/commonassets/player/playerspring.bundle";

	public const string PLAYER_SIMPLE_BUNDLE_NAME = "assets/content/commonprefabs/playersimple.bundle";

	public const string ZOMBIE_SIMPLE_BUNDLE_NAME = "assets/content/commonprefabs/playerzombie_simple.bundle";

	[NonSerialized]
	public static ResourceKey ResourceKey_0 = new ResourceKey
	{
		path = "assets/content/commonprefabs/playersuperior.bundle"
	};

	[NonSerialized]
	public static ResourceKey ResourceKey_1 = new ResourceKey
	{
		path = "assets/content/location_objects/lootable/prefab/scontainer_crate.bundle"
	};

	[NonSerialized]
	public static ResourceKey ResourceKey_2 = new ResourceKey
	{
		path = "assets/content/location_objects/lootable/prefab/il76md-90.prefab"
	};

	[NonSerialized]
	public static ResourceKey ResourceKey_3 = new ResourceKey
	{
		path = "assets/content/location_objects/lootable/prefab/tripwire.bundle"
	};

	[NonSerialized]
	public static ResourceKey ResourceKey_4 = new ResourceKey
	{
		path = "assets/content/commonprefabs/playerzombie_superior.bundle"
	};

	public static Dictionary<SynchronizableObjectType, ResourceKey> SynchronizableObjectPath = new Dictionary<SynchronizableObjectType, ResourceKey>
	{
		{
			SynchronizableObjectType.AirDrop,
			ResourceKey_1
		},
		{
			SynchronizableObjectType.AirPlane,
			ResourceKey_2
		},
		{
			SynchronizableObjectType.Tripwire,
			ResourceKey_3
		}
	};

	public static readonly ResourceKey PLAYER_SPIRIT_RESOURCE_KEY = new ResourceKey
	{
		path = "assets/commonassets/prefabs/playerspirit.bundle",
		rcid = "PlayerSpirit"
	};

	public static readonly ResourceKey ZOMBIE_SPIRIT_RESOURCE_KEY = new ResourceKey
	{
		path = "assets/content/commonprefabs/playerzombie_spirit.bundle",
		rcid = "PlayerZombie_Spirit"
	};

	public const string ControllersBundle = "assets/content/characters/controllers/player_anim_controller.bundle";

	public const string FastControllersBundle = "assets/content/characters/controllers/player_anim_controller.animatorcontrollerdata.bundle";

	public const string PlayerRootMotionBundle = "assets/content/characters/rootmotiontable/playerrootmotiontable.asset.bundle";

	public const string CharacterAnimationsBundle = "assets/content/characters/animations/character_animations.bundle";

	public const string SimpleCharacterAnimationsBundle = "assets/content/characters/animations/simple_character_animations.bundle";

	public const string SpiritCharacterAnimationsBundle = "assets/content/characters/animations/spirit_character_animations.bundle";

	public const string WeaponAnimationsBundle = "/client_assets.bundle";

	public const string SimpleWeaponAnimationsBundle = "assets/content/weapons/animations/simple_animations.bundle";

	public const string SpiritWeaponAnimationsBundle = "assets/content/weapons/animations/spirit_animations.bundle";

	public const string ZombieSimpleControllersBundle = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean_simple.generated.bundle";

	public const string ZombieSpiritControllersBundle = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean_spirit.generated.bundle";

	public const string ZombieControllersBundle = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean.bundle";

	public const string ZombieRootMotionBundle = "assets/content/characters/rootmotiontable/zombierootmotiontable.bundle";

	public const string ZombieFastControllersBundle = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean.animatorcontrollerdata.bundle";

	public const string BossKabanControllersBundle = "assets/content/characters/controllers/kaban_anim_controller/kaban_anim_controller.bundle";

	public const string KabanRootMotionBundle = "assets/content/characters/rootmotiontable/kabanrootmotiontable.asset.bundle";

	[NonSerialized]
	public static ResourceKey ResourceKey_5 = new ResourceKey
	{
		path = "assets/content/characters/controllers/player_anim_controller.bundle",
		rcid = "Player_anim_controller"
	};

	[NonSerialized]
	public static ResourceKey ResourceKey_6 = new ResourceKey
	{
		path = "assets/content/characters/controllers/kaban_anim_controller/kaban_anim_controller.bundle",
		rcid = "Kaban_anim_controller"
	};

	[NonSerialized]
	public static ResourceKey ResourceKey_7 = new ResourceKey
	{
		path = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean.bundle",
		rcid = "Zombie_anim_controller_clean"
	};

	public static readonly ResourceKey PLAYER_FAST_ANIMATOR_CONTROLLER = new ResourceKey
	{
		path = "assets/content/characters/controllers/player_anim_controller.animatorcontrollerdata.bundle",
		rcid = "Player_anim_controller.animatorcontrollerdata"
	};

	public static readonly ResourceKey ZOMBIE_FAST_ANIMATOR_CONTROLLER = new ResourceKey
	{
		path = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean.animatorcontrollerdata.bundle",
		rcid = "Zombie_anim_controller_clean.animatorcontrollerdata"
	};

	public static readonly ResourceKey PLAYER_DEFAULT_ANIMATOR_CONTROLLER_SPIRIT = new ResourceKey
	{
		path = "assets/content/characters/controllers/player_anim_controller.bundle",
		rcid = "Player_anim_controller_spirit.generated"
	};

	public static readonly ResourceKey ZOMBIE_ANIMATOR_CONTROLLER_SPIRIT = new ResourceKey
	{
		path = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean_spirit.generated.bundle"
	};

	public static readonly ResourceKey PLAYER_ANIMATION_CLIPS_KEEPER = new ResourceKey
	{
		path = "assets/content/characters/controllers/player_anim_controller.bundle",
		rcid = "Player_anim_clips_keeper"
	};

	public static readonly ResourceKey ZOMBIE_ANIMATION_CLIPS_KEEPER = new ResourceKey
	{
		path = "assets/content/characters/controllers/zombies/zombie_anim_controller_clean.bundle",
		rcid = "Zombie_anim_clips_keeper"
	};

	public static readonly ResourceKey PLAYER_ROOTMOTION_TABLE = new ResourceKey
	{
		path = "assets/content/characters/rootmotiontable/playerrootmotiontable.asset.bundle",
		rcid = "PlayerRootMotionTable"
	};

	public static readonly ResourceKey ZOMBIE_ROOTMOTION_TABLE = new ResourceKey
	{
		path = "assets/content/characters/rootmotiontable/zombierootmotiontable.bundle",
		rcid = "ZombieRootMotionTable"
	};

	public static readonly ResourceKey KABAN_ROOTMOTION_TABLE = new ResourceKey
	{
		path = "assets/content/characters/rootmotiontable/kabanrootmotiontable.asset.bundle",
		rcid = "KabanRootMotionTable"
	};

	public static readonly ResourceKey BOSS_KABAN_ANIMATOR_CONTROLLER_SPIRIT = new ResourceKey
	{
		path = "assets/content/characters/controllers/kaban_anim_controller/kaban_anim_controller.bundle",
		rcid = "Kaban_anim_controller_spirit.generated"
	};

	public static readonly ResourceKey BOSS_KABAN_ROOTMOTION_TABLE = new ResourceKey
	{
		path = "assets/content/characters/controllers/kaban_anim_controller/kaban_anim_controller.bundle",
		rcid = "BossKabanRootMotionTable"
	};

	public const string CAMERA_CONTAINER_BUNDLE_NAME = "assets/commonassets/player/cameracontainer.bundle";

	public const string USEC_HEAD_BUNDLE_NAME = "assets/content/characters/character/prefabs/usec_head.bundle";

	public const string USEC_HEAD_1_BUNDLE_NAME = "assets/content/characters/character/prefabs/usec_head_1.bundle";

	public const string USEC_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/usec_body.bundle";

	public const string USEC_BODY_FEET_NAME = "assets/content/characters/character/prefabs/usec_feet.bundle";

	public const string USEC_HANDS_BUNDLE_NAME = "assets/content/hands/usec/usec_hands_skin.bundle";

	public const string BEAR_HEAD_0_BUNDLE_NAME = "assets/content/characters/character/prefabs/bear_head.bundle";

	public const string BEAR_HEAD_1_BUNDLE_NAME = "assets/content/characters/character/prefabs/bear_head_1.bundle";

	public const string BEAR_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/bear_body.bundle";

	public const string BEAR_FEET_BUNDLE_NAME = "assets/content/characters/character/prefabs/bear_feet.bundle";

	public const string BEAR_HANDS_BUNDLE_NAME = "assets/content/hands/bear/bear_hands_skin.bundle";

	public const string WILD_HEAD_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_head.bundle";

	public const string WILD_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_body.bundle";

	public const string WILD_FEET_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_feet.bundle";

	public const string WILD1_HEAD_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_head_1.bundle";

	public const string WILD1_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_body_1.bundle";

	public const string WILD1_FEET_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_feet_1.bundle";

	public const string WILD2_HEAD_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_head_2.bundle";

	public const string WILD2_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_body_2.bundle";

	public const string WILD2_FEET_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_feet_2.bundle";

	public const string WILD_DEALMAKER_HEAD = "assets/content/characters/character/prefabs/wild_dealmaker_head.bundle";

	public const string WILD_DEALMAKER_BODY = "assets/content/characters/character/prefabs/wild_dealmaker_body.bundle";

	public const string WILD_DEALMAKER_FEET = "assets/content/characters/character/prefabs/wild_dealmaker_feet.bundle";

	public const string BEAR_TSHIRT_BLACK_HANDS = "assets/content/hands/hands_tshirt_bear_black/hands_tshirt_bear_black.skin.bundle";

	public const string BEAR_BLACK_LYNX_HANDS = "assets/content/hands/hand_bear_blacklynx/hand_bear_blacklynx.skin.bundle";

	public const string BEAR_GHOSTMARKSMAN_HANDS = "assets/content/hands/hands_bear_ghostmarksman/hands_bear_ghostmarksman.skin.bundle";

	public const string BEAR_POLEVOI_HANDS = "assets/content/hands/hands_bear_polevoi/hands_bear_polevoi.skin.bundle";

	public const string BEAR_FSB_FAST_RESPONSE_HANDS = "assets/content/hands/hands_bear_fsbfastresponse/hands_bear_fsbfastresponse.skin.bundle";

	public const string USEC_AGRESSOR_PARKA_HANDS = "assets/content/hands/hands_usec_aggressor_parka/hands_usec_aggressor_parka.skin.bundle";

	public const string USEC_COMBATSHIRT_HANDS = "assets/content/hands/hands_usec_combatshirt/hands_usec_combatshirt.skin.bundle";

	public const string USEC_ORC_PCU_HANDS = "assets/content/hands/hands_usec_orc_pcu/hands_usec_orc_pcu.skin.bundle";

	public const string USEC_SOFTSHELL_HANDS = "assets/content/hands/hands_usec_softshell/hands_usec_softshell.skin.bundle";

	public const string USEC_COMMANDO_HANDS = "assets/content/hands/hands_usec_commando/hands_usec_commando.skin.bundle";

	public const string BOSS_KILLA_FEET = "assets/content/characters/character/prefabs/pant_boss_killa.bundle";

	public const string BOSS_KILLA_BODY = "assets/content/characters/character/prefabs/top_boss_killa.bundle";

	public const string BOSS_KILLA_HEAD = "assets/content/characters/character/prefabs/head_boss_killa.bundle";

	public const string SCAV_ELITE_FEET = "assets/content/characters/character/prefabs/pants_wild_scavelite.bundle";

	public const string SCAV_ELITE_BODY = "assets/content/characters/character/prefabs/top_wild_scavelite.bundle";

	public const string WILD3_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_body_3.bundle";

	public const string WILD3_HEAD_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_head_3.bundle";

	public const string WILD_SECURITY1_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_security_body_1.bundle";

	public const string WILD_SECURITY_FEET_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_security_feet_1.bundle";

	public const string WILD_SECURITY2_BODY_BUNDLE_NAME = "assets/content/characters/character/prefabs/wild_security_body_2.bundle";

	public const string WILD_BODY_FIRSTHANDS_BUNDLE_NAME = "assets/content/hands/wild/wild_body_firsthands.bundle";

	public const string WILD_BODY_FIRSTHANDS_1_BUNDLE_NAME = "assets/content/hands/wild/wild_body_1_firsthands.bundle";

	public const string WILD_BODY_FIRSTHANDS_2_BUNDLE_NAME = "assets/content/hands/wild/wild_body_2_firsthands.bundle";

	public const string WILD_BODY_FIRSTHANDS_3_BUNDLE_NAME = "assets/content/hands/wild/wild_body_3_firsthands.bundle";

	public const string WEAPON_EMPTY_HANDS_BUNDLE_NAME = "assets/content/weapons/empty hands/weapon_empty_hands_container.bundle";

	public const string AUDIO_SHOT_BUNDLE_NAME = "assets/content/audio/audioshot.bundle";

	public const string PLAYER_MOVEMENT_SOUNDS_BUNDLE = "assets/content/audio/prefabs/movement/sounds.bundle";

	public const string FIRST_PERSON_PLAYER_HEARING_SETTINGS = "assets/content/audio/prefabs/character/firstpersonplayerhearingsettings.bundle";

	public const string PLAYER_WEAPON_SOUNDS_BUNDLE = "assets/content/audio/prefabs/shells/weaponsounds.bundle";

	public const string ITEM_DROP_SOUNDS_BUNDLE = "assets/content/audio/prefabs/items/itemdropsounds.bundle";

	public const string AIRDROP_LANDING_SOUNDS_BUNDLE = "assets/content/audio/prefabs/airdrop/airdropsounds.bundle";

	public const string item_equipment_backpack = "assets/content/items/equipment/backpack/item_equipment_backpack.bundle";

	public const string item_equipment_usec_vest = "assets/content/items/equipment/usec_vest/item_equipment_usec_vest.bundle";

	public const string weapon_colt_m4a1_556x45_container = "assets/content/weapons/m4a1/weapon_colt_m4a1_556x45_container.bundle";

	public const string PATRON_RSP_GREEN = "assets/content/items/ammo/patrons/patron_rsp_green.bundle";

	public const string PATRON_RSP_RED = "assets/content/items/ammo/patrons/patron_rsp_red.bundle";

	public const string PATRON_RSP_YELLOW = "assets/content/items/ammo/patrons/patron_rsp_yellow.bundle";

	public const string PATRON_RSP_WHITE = "assets/content/items/ammo/patrons/patron_rsp_white.bundle";

	public const string PATRON_RSP_BLUE = "assets/content/items/ammo/patrons/patron_26x75_blue.bundle";

	public const string PATRON_RSP_NY = "assets/content/items/ammo/patrons/patron_rsp_newyear.bundle";

	public const string PHRASES = "assets/content/audio/phrases/allphrases.bundle";

	public const string ITEM_SOUNDS = "assets/content/audio/itemsounds/itemsounds.bundle";

	public const string BEAR_1 = "assets/content/audio/phrases/bear_1_voice.bundle";

	public const string BEAR_1_ENG = "assets/content/audio/phrases/bear_1_eng_voice.bundle";

	public const string BEAR_2 = "assets/content/audio/phrases/bear_2_voice.bundle";

	public const string BEAR_2_ENG = "assets/content/audio/phrases/bear_2_eng_voice.bundle";

	public const string BEAR_3 = "assets/content/audio/phrases/bear_3_voice.bundle";

	public const string BEAR_4 = "assets/content/audio/phrases/bear_vitaly_voice.bundle";

	public const string USEC_1 = "assets/content/audio/phrases/usec_1_voice.bundle";

	public const string USEC_2 = "assets/content/audio/phrases/usec_2_voice.bundle";

	public const string USEC_3 = "assets/content/audio/phrases/usec_3_voice.bundle";

	public const string USEC_4 = "assets/content/audio/phrases/usec_4_voice.bundle";

	public const string USEC_5 = "assets/content/audio/phrases/usec_5_voice.bundle";

	public const string USEC_6 = "assets/content/audio/phrases/usec_6_voice.bundle";

	public const string USEC_7 = "assets/content/audio/phrases/usec_rob_voice.bundle";

	public const string SCAV_1 = "assets/content/audio/phrases/scav_1_voice.bundle";

	public const string SCAV_2 = "assets/content/audio/phrases/scav_2_voice.bundle";

	public const string SCAV_3 = "assets/content/audio/phrases/scav_3_voice.bundle";

	public const string SCAV_4 = "assets/content/audio/phrases/scav_4_voice.bundle";

	public const string SCAV_5 = "assets/content/audio/phrases/scav_5_voice.bundle";

	public const string SCAV_6 = "assets/content/audio/phrases/scav_6_voice.bundle";

	public const string BOSSSANITAR = "assets/content/audio/phrases/bosssanitar_voice.bundle";

	public const string BOSSBULLY = "assets/content/audio/phrases/bossbully_voice.bundle";

	public const string BOSSGLUHAR = "assets/content/audio/phrases/bossgluhar_voice.bundle";

	public const string BOSSPRIEST = "assets/content/audio/phrases/sectantpriest.bundle";

	public const string BOSSWARRIOR = "assets/content/audio/phrases/sectantwarrior_voice.bundle";

	public const string BOSS_BIG_PIPE = "assets/content/audio/phrases/bossbigpipe_voice.bundle";

	public const string BOSS_BIRD_EYE = "assets/content/audio/phrases/bossbirdeye_voice.bundle";

	public const string BOSS_KNIGHT = "assets/content/audio/phrases/bossknight_voice.bundle";

	public const string BOSS_KILLA = "assets/content/audio/phrases/bosskilla_voice.bundle";

	public const string BOSS_TAGILLA = "assets/content/audio/phrases/bosstagilla_voice.bundle";

	public const string ARENA_GUARD_1 = "assets/content/audio/phrases/arena_guard_1_voice.bundle";

	public const string ARENA_GUARD_2 = "assets/content/audio/phrases/arena_guard_2_voice.bundle";

	public const string BOSS_KABAN = "assets/content/audio/phrases/boss_kaban_voice.bundle";

	public const string BOSS_KOLLONTAY = "assets/content/audio/phrases/boss_kollontay_voice.bundle";

	public const string BOSS_STURMAN = "assets/content/audio/phrases/boss_sturman_voice.bundle";

	public const string BOSS_PARTIZAN = "assets/content/audio/phrases/boss_partizan_voice.bundle";

	public const string ZOMBIE_GENERIC = "assets/content/audio/phrases/zombie_generic.bundle";

	public const string ZOMBIE_FAST = "assets/content/audio/phrases/zombie_fast.bundle";

	public const string ZOMBIE_MEDIUM = "assets/content/audio/phrases/zombie_medium.bundle";

	public const string BOSS_ZOMBIE_TAGILLA = "assets/content/audio/phrases/bosszombietagilla_voice.bundle";

	public const string BOSS_AGRO_TAGILLA = "assets/content/audio/phrases/bossagrotagilla_voice.bundle";

	public const string PRECIPITATION_SOUND_STORAGE_SO_BUNDLE = "assets/content/audio/data/soundcontainers/precipitationsoundstorageso.bundle";

	public const string TRIPWIRE_SOUND_STORAGE_SO_BUNDLE = "assets/content/audio/data/soundcontainers/tripwiresoundstorage.bundle";

	public const string ARTILLERY_SHELLING_SOUNDS_SO_BUNDLE = "assets/content/audio/data/soundcontainers/artilleryshellingsounds.bundle";

	[NonSerialized]
	public static Dictionary<string, string> Dictionary_0 = new Dictionary<string, string>
	{
		{ "Bear_1", "assets/content/audio/phrases/bear_1_voice.bundle" },
		{ "Bear_2", "assets/content/audio/phrases/bear_2_voice.bundle" },
		{ "Bear_3", "assets/content/audio/phrases/bear_3_voice.bundle" },
		{ "Bear_4", "assets/content/audio/phrases/bear_vitaly_voice.bundle" },
		{ "Bear_1_Eng", "assets/content/audio/phrases/bear_1_eng_voice.bundle" },
		{ "Bear_2_Eng", "assets/content/audio/phrases/bear_2_eng_voice.bundle" },
		{ "Usec_1", "assets/content/audio/phrases/usec_1_voice.bundle" },
		{ "Usec_2", "assets/content/audio/phrases/usec_2_voice.bundle" },
		{ "Usec_3", "assets/content/audio/phrases/usec_3_voice.bundle" },
		{ "Usec_4", "assets/content/audio/phrases/usec_4_voice.bundle" },
		{ "Usec_5", "assets/content/audio/phrases/usec_5_voice.bundle" },
		{ "Usec_6", "assets/content/audio/phrases/usec_6_voice.bundle" },
		{ "Usec_7", "assets/content/audio/phrases/usec_rob_voice.bundle" },
		{ "Scav_1", "assets/content/audio/phrases/scav_1_voice.bundle" },
		{ "Scav_2", "assets/content/audio/phrases/scav_2_voice.bundle" },
		{ "Scav_3", "assets/content/audio/phrases/scav_3_voice.bundle" },
		{ "Scav_4", "assets/content/audio/phrases/scav_4_voice.bundle" },
		{ "Scav_5", "assets/content/audio/phrases/scav_5_voice.bundle" },
		{ "Scav_6", "assets/content/audio/phrases/scav_6_voice.bundle" },
		{ "BossSanitar", "assets/content/audio/phrases/bosssanitar_voice.bundle" },
		{ "BossBully", "assets/content/audio/phrases/bossbully_voice.bundle" },
		{ "BossGluhar", "assets/content/audio/phrases/bossgluhar_voice.bundle" },
		{ "SectantPriest", "assets/content/audio/phrases/sectantpriest.bundle" },
		{ "SectantWarrior", "assets/content/audio/phrases/sectantwarrior_voice.bundle" },
		{ "BossKilla", "assets/content/audio/phrases/bosskilla_voice.bundle" },
		{ "BossTagilla", "assets/content/audio/phrases/bosstagilla_voice.bundle" },
		{ "Boss_Partizan", "assets/content/audio/phrases/boss_partizan_voice.bundle" },
		{ "BossBigPipe", "assets/content/audio/phrases/bossbigpipe_voice.bundle" },
		{ "BossBirdEye", "assets/content/audio/phrases/bossbirdeye_voice.bundle" },
		{ "BossKnight", "assets/content/audio/phrases/bossknight_voice.bundle" },
		{ "Arena_Guard_1", "assets/content/audio/phrases/arena_guard_1_voice.bundle" },
		{ "Arena_Guard_2", "assets/content/audio/phrases/arena_guard_2_voice.bundle" },
		{ "Boss_Kaban", "assets/content/audio/phrases/boss_kaban_voice.bundle" },
		{ "Boss_Kollontay", "assets/content/audio/phrases/boss_kollontay_voice.bundle" },
		{ "Boss_Sturman", "assets/content/audio/phrases/boss_sturman_voice.bundle" },
		{ "Zombie_Generic", "assets/content/audio/phrases/zombie_generic.bundle" },
		{ "BossZombieTagilla", "assets/content/audio/phrases/bosszombietagilla_voice.bundle" },
		{ "Zombie_Fast", "assets/content/audio/phrases/zombie_fast.bundle" },
		{ "Zombie_Medium", "assets/content/audio/phrases/zombie_medium.bundle" },
		{ "BossAgroTagilla", "assets/content/audio/phrases/bossagrotagilla_voice.bundle" }
	};

	public static readonly string[] ADDITIONAL_BUNDLES_TO_PRELOAD = new string[25]
	{
		"assets/content/characters/character/prefabs/usec_head.bundle", "assets/content/characters/character/prefabs/usec_head_1.bundle", "assets/content/characters/character/prefabs/usec_body.bundle", "assets/content/characters/character/prefabs/usec_feet.bundle", "assets/content/hands/usec/usec_hands_skin.bundle", "assets/content/characters/character/prefabs/bear_head.bundle", "assets/content/characters/character/prefabs/bear_head_1.bundle", "assets/content/characters/character/prefabs/bear_body.bundle", "assets/content/characters/character/prefabs/bear_feet.bundle", "assets/content/hands/bear/bear_hands_skin.bundle",
		"assets/content/characters/character/prefabs/wild_head.bundle", "assets/content/characters/character/prefabs/wild_body.bundle", "assets/content/characters/character/prefabs/wild_feet.bundle", "assets/content/characters/character/prefabs/wild_head_1.bundle", "assets/content/characters/character/prefabs/wild_body_1.bundle", "assets/content/characters/character/prefabs/wild_feet_1.bundle", "assets/content/characters/character/prefabs/wild_head_2.bundle", "assets/content/characters/character/prefabs/wild_body_2.bundle", "assets/content/characters/character/prefabs/wild_feet_2.bundle", "assets/content/characters/character/prefabs/wild_body_3.bundle",
		"assets/content/characters/character/prefabs/wild_head_3.bundle", "assets/content/hands/wild/wild_body_firsthands.bundle", "assets/content/hands/wild/wild_body_1_firsthands.bundle", "assets/content/hands/wild/wild_body_2_firsthands.bundle", "assets/content/hands/wild/wild_body_3_firsthands.bundle"
	};

	public static readonly string[] BUNDLES_TO_PRELOAD = new string[35]
	{
		"assets/content/commonprefabs/defaultballisticcollider.bundle", PLAYER_BUNDLE_NAME.path, PLAYER_DEFAULT_ANIMATOR_CONTROLLER.path, PLAYER_FAST_ANIMATOR_CONTROLLER.path, BOSS_KABAN_ANIMATOR_CONTROLLER.path, ZOMBIE_BUNDLE_NAME.path, ZOMBIE_ANIMATOR_CONTROLLER.path, ZOMBIE_FAST_ANIMATOR_CONTROLLER.path, ZOMBIE_ANIMATOR_CONTROLLER_SPIRIT.path, "assets/content/weapons/empty hands/weapon_empty_hands_container.bundle",
		"assets/content/items/ammo/patrons/patron_rsp_red.bundle", "assets/content/items/ammo/patrons/patron_rsp_green.bundle", "assets/content/items/ammo/patrons/patron_rsp_yellow.bundle", "assets/content/items/ammo/patrons/patron_rsp_white.bundle", "assets/content/items/ammo/patrons/patron_26x75_blue.bundle", "assets/content/items/ammo/patrons/patron_rsp_newyear.bundle", "assets/content/weapons/mp18/weapon_izhmash_mp18_multi_container.bundle", "assets/content/items/mods/handguards/handguard_mp18_izhmash_wood.bundle", "assets/content/items/mods/stocks/stock_mp18_izhmash_wood.bundle", "assets/content/items/mods/barrels/barrel_mp18_600mm_762x54r.bundle",
		"assets/content/weapons/kiba_axe/weapon_kiba_arms_axe_container.bundle", "assets/content/audio/phrases/usec_1_voice.bundle", "assets/content/weapons/m67/weapon_grenade_m67_container.bundle", "assets/content/characters/character/prefabs/usec_head_6.bundle", "assets/content/characters/character/prefabs/tshirt_usec_cryeac.bundle", "assets/content/characters/character/prefabs/pants_usec_cryeac.bundle", "assets/content/audio/audioshot.bundle", "assets/content/audio/prefabs/movement/sounds.bundle", "assets/content/audio/prefabs/shells/weaponsounds.bundle", "assets/content/audio/prefabs/items/itemdropsounds.bundle",
		"assets/content/audio/itemsounds/itemsounds.bundle", "assets/content/audio/prefabs/airdrop/airdropsounds.bundle", "assets/content/audio/data/soundcontainers/precipitationsoundstorageso.bundle", "assets/content/audio/data/soundcontainers/tripwiresoundstorage.bundle", "assets/content/audio/prefabs/character/firstpersonplayerhearingsettings.bundle"
	};

	public static ResourceKey PLAYER_BUNDLE_NAME => ResourceKey_0;

	public static ResourceKey ZOMBIE_BUNDLE_NAME => ResourceKey_4;

	public static ResourceKey PLAYER_DEFAULT_ANIMATOR_CONTROLLER => ResourceKey_5;

	public static ResourceKey BOSS_KABAN_ANIMATOR_CONTROLLER => ResourceKey_6;

	public static ResourceKey ZOMBIE_ANIMATOR_CONTROLLER => ResourceKey_7;

	public static ICollection<string> AvailableVoices => Dictionary_0.Keys;

	public static string TakePhrasePath(string name)
	{
		if (Dictionary_0.TryGetValue(name, out var value))
		{
			return value;
		}
		Debug.LogError("No bundle found for voice " + name);
		return string.Empty;
	}
}
