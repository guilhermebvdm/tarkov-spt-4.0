using System;
using Newtonsoft.Json;

public class CoreBotSettingsClass
{
	public int SAVAGE_KILL_DIST = 90;

	public float SOUND_DOOR_BREACH_METERS = 35f;

	public float SOUND_DOOR_OPEN_METERS = 10f;

	public float STEP_NOISE_DELTA = 1f;

	public float JUMP_NOISE_DELTA = 1f;

	public int GUNSHOT_SPREAD = 280;

	public int GUNSHOT_SPREAD_SILENCE = 60;

	public float BASE_WALK_SPEREAD2 = 26f;

	public float MOVE_SPEED_COEF_MAX = 2.1f;

	public float SPEED_SERV_SOUND_COEF_A = 0.3f;

	public float SPEED_SERV_SOUND_COEF_B = 0.7f;

	public float G = 9.8f;

	public float STAY_COEF = 1f;

	public float SIT_COEF = 0.8f;

	public float LAY_COEF = 1f;

	public int MAX_ITERATIONS = 6;

	public float START_DIST_TO_COV = 12100f;

	public float MAX_DIST_TO_COV = 220f;

	public float CLOSE_POINTS = 3f;

	public int COUNT_TURNS = 5;

	public float SIMPLE_POINT_LIFE_TIME_SEC = 60f;

	public float DANGER_POINT_LIFE_TIME_SEC = 300f;

	public float DANGER_POWER = 38f;

	public float COVER_DIST_CLOSE = 10f;

	public float GOOD_DIST_TO_POINT = 7f;

	public float COVER_TOOFAR_FROM_BOSS = 20f;

	public float COVER_TOOFAR_FROM_BOSS_SQRT = 400f;

	public float MAX_Y_DIFF_TO_PROTECT = 1f;

	public float FLARE_POWER = 8f;

	public float MOVE_COEF = 1.2f;

	public float PRONE_POSE = 0.7f;

	public float LOWER_POSE = 1f;

	public float MAX_POSE = 1.5f;

	public float FLARE_TIME = 1.5f;

	public int MAX_REQUESTS__PER_GROUP = 10;

	public float UPDATE_GOAL_TIMER_SEC = 3.3f;

	public float DIST_NOT_TO_GROUP = 300f;

	public float DIST_NOT_TO_GROUP_SQR;

	public float LAST_SEEN_POS_LIFETIME = 120f;

	public float DELTA_GRENADE_START_TIME = 0.9f;

	public float DELTA_GRENADE_END_TIME = 5f;

	public float DELTA_GRENADE_RUN_DIST = 7f;

	public float DELTA_GRENADE_BEWARE_DIST_SQRT;

	public float DELTA_GRENADE_SAFE_DIST_SQRT;

	public float PATROL_MIN_LIGHT_DIST = 8f;

	public float HOLD_MIN_LIGHT_DIST = 3.5f;

	public float STANDART_BOT_PAUSE_DOOR = 0.5f;

	public float ARMOR_CLASS_COEF = 6f;

	public float SHOTGUN_POWER = 30f;

	public float RIFLE_POWER = 60f;

	public float PISTOL_POWER = 20f;

	public float SMG_POWER = 45f;

	public float SNIPE_POWER = 55f;

	public float GESTUS_PERIOD_SEC = 3f;

	public float GESTUS_AIMING_DELAY = 2.5f;

	public float GESTUS_REQUEST_LIFETIME = 10f;

	public float GESTUS_FIRST_STAGE_MAX_TIME = 5f;

	public float GESTUS_SECOND_STAGE_MAX_TIME = 2f;

	public int GESTUS_MAX_ANSWERS = 4;

	public int GESTUS_FUCK_TO_SHOOT = 3;

	public float GESTUS_DIST_ANSWERS = 6f;

	public float GESTUS_DIST_ANSWERS_SQRT;

	public float GESTUS_ANYWAY_CHANCE = 10f;

	public float TALK_DELAY = 10f;

	public bool CAN_SHOOT_TO_HEAD = true;

	public bool CAN_TILT = true;

	public float TILT_CHANCE = 100f;

	public float MIN_BLOCK_DIST = 3f;

	public float MIN_BLOCK_TIME = 5f;

	public float COVER_SECONDS_AFTER_LOSE_VISION = 10f;

	public float MIN_ARG_COEF = 0.3f;

	public float MAX_ARG_COEF = 100f;

	public float DEAD_AGR_DIST = 60f;

	public float MAX_DANGER_CARE_DIST_SQRT;

	public float MAX_DANGER_CARE_DIST = 230f;

	public int MIN_MAX_PERSON_SEARCH = 2;

	public float PERCENT_PERSON_SEARCH = 0.3f;

	public float LOOK_ANYSIDE_BY_WALL_SEC_OF_ENEMY = 20f;

	public float CLOSE_TO_WALL_ROTATE_BY_WALL_SQRT = 4f;

	public int SHOOT_TO_CHANGE_RND_PART_MIN = 2;

	public int SHOOT_TO_CHANGE_RND_PART_MAX = 5;

	public float SHOOT_TO_CHANGE_RND_PART_DELTA = 9f;

	public float FORMUL_COEF_DELTA_DIST = 2.6f;

	public float FORMUL_COEF_DELTA_SHOOT = 1f;

	public float FORMUL_COEF_DELTA_FRIEND_COVER = 15f;

	public float SUSPETION_POINT_DIST_CHECK = 5f;

	public int MAX_BASE_REQUESTS_PER_PLAYER = 1;

	public int MAX_HOLD_REQUESTS_PER_PLAYER = 3;

	public int MAX_GO_TO_REQUESTS_PER_PLAYER = 3;

	public int MAX_COME_WITH_ME_REQUESTS_PER_PLAYER = 1;

	public int MAX_GET_IN_COVER_REQUESTS_PER_PLAYER = 10;

	public int MAX_WAIT_REQUESTS_PER_PLAYER = 10;

	public float CORE_POINT_MAX_VALUE = 60f;

	public int CORE_POINTS_MAX = 3;

	public int CORE_POINTS_MIN = 1;

	public bool BORN_POISTS_FREE_ONLY_FAREST_BOT;

	public bool BORN_POINSTS_FREE_ONLY_FAREST_PLAYER;

	public bool SCAV_GROUPS_TOGETHER = true;

	public float LAY_DOWN_ANG_SHOOT = 40f;

	public float HOLD_REQUEST_TIME_SEC = 240f;

	public int TRIGGERS_DOWN_TO_RUN_WHEN_MOVE = 5;

	public float MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING = 5.5f;

	public float MIN_DIST_TO_RUN_WHILE_ATTACK_MOVING_OTHER_ENEMIS = 10f;

	public float MIN_DIST_TO_STOP_RUN = 3.5f;

	public float JUMP_SPREAD_DIST = 25f;

	public int LOOK_TIMES_TO_KILL_PLAYER = 4;

	public int LOOK_TIMES_TO_KILL_BOT = 8;

	public float MOVING_AIM_COEF = 0.23f;

	public float VERTICAL_DIST_TO_IGNORE_SOUND = 116f;

	public float DEFENCE_LEVEL_SHIFT;

	public float MIN_DIST_CLOSE_DEF = 1.2f;

	public bool USE_ID_PRIOR_WHO_GO;

	public float SMOKE_GRENADE_RADIUS_COEF = 2f;

	public int GRENADE_PRECISION = 4;

	public int MAX_WARNS_BEFORE_KILL = 4;

	public float CARE_ENEMY_ONLY_TIME = 30f;

	public float MIDDLE_POINT_COEF = 0.5f;

	public bool MAIN_TACTIC_ONLY_ATTACK = true;

	public float LAST_DAMAGE_ACTIVE = 5f;

	public bool SHALL_DIE_IF_NOT_INITED = true;

	public float CHECK_BOT_INIT_TIME_SEC = 20f;

	public float WEAPON_ROOT_Y_OFFSET = 0.175f;

	public float DELTA_SUPRESS_DISTANCE_SQRT = 25f;

	public float DELTA_SUPRESS_DISTANCE = 5f;

	public float WAVE_COEF_LOW = 1f;

	public float WAVE_COEF_MID = 1f;

	public float WAVE_COEF_HIGH = 1f;

	public float WAVE_COEF_HORDE = 2f;

	public bool WAVE_ONLY_AS_ONLINE;

	public float AI_DATA_POWER_RECALC = 3f;

	public int LOCAL_BOTS_COUNT = 50;

	public float ENEMY_TO_BE_CURRENT = 10f;

	public float DELAY_BEFORE_ENEMY = 2f;

	public float DIST_TO_BECOME_ENEMY = -1f;

	public float PERIOD_NEXT_ACTION_SEC = 30f;

	public float PERIOD_NEXT_HELLO_ACTION_SEC = 30f;

	public int AXE_MAN_KILLS_END = 4;

	public int MAX_POINS_CHECK_TOTAL = 500;

	public int MAX_POINS_CHECK_RAY = 100;

	public int IF_RAYSCAST_ON_DO_OFF_IT = 10;

	public float COVER_SEARCH_METERS_ADD = 2f;

	public int GROUP_WAVE_SIZE_MAX = 5;

	public bool CAN_ASSAULT_DANGER_ZONES;

	public bool BOT_MOVE_CAST_ONLYVERTICAL;

	public int MAX_SIMPLE_BOTS_IN_DELAY = 100;

	public int SIMPLE_BOTS_IN_DELAY_PERIOD = 15;

	public float SCAV_ATTACK_PERIOD = 120f;

	public int USEC_HISTORY_ATTACKS_REMAIN = 3;

	public bool ACTIVE_FORCE_ATTACK_EVENTS = true;

	public bool ACTIVE_FORCE_KHOROVOD_EVENTS = true;

	public bool ACTIVE_FOLLOW_PLAYER_EVENT = true;

	public bool ACTIVE_HALLOWEEN_ZOMBIES_EVENT;

	public bool START_ACTIVE_FOLLOW_PLAYER_EVENT;

	public bool START_ACTIVE_FORCE_ATTACK_PLAYER_EVENT;

	public int ACTIVE_FORCE_ATTACK_EVENTS_HOUR_START = -1;

	public string ACTIVE_FORCE_ATTACK_EVENTS_INTERACTABLE_IDS = "test1,test2";

	public string ACTIVE_FORCE_ATTACK_EVENTS_ZONE_IDS = "test1,test2";

	public bool ACTIVE_PATROL_GENERATOR_EVENT = true;

	public static CoreBotSettingsClass Create(string coreTxt)
	{
		CoreBotSettingsClass coreBotSettingsClass = JsonParserClass.ParseJsonTo<CoreBotSettingsClass>(coreTxt, Array.Empty<JsonConverter>());
		coreBotSettingsClass.Update();
		return coreBotSettingsClass;
	}

	public void Update()
	{
		DELTA_SUPRESS_DISTANCE_SQRT = DELTA_SUPRESS_DISTANCE * DELTA_SUPRESS_DISTANCE;
		COVER_TOOFAR_FROM_BOSS_SQRT = COVER_TOOFAR_FROM_BOSS * COVER_TOOFAR_FROM_BOSS;
		MAX_DANGER_CARE_DIST_SQRT = MAX_DANGER_CARE_DIST * MAX_DANGER_CARE_DIST;
		GESTUS_DIST_ANSWERS_SQRT = GESTUS_DIST_ANSWERS * GESTUS_DIST_ANSWERS;
		DELTA_GRENADE_BEWARE_DIST_SQRT = DELTA_GRENADE_RUN_DIST * DELTA_GRENADE_RUN_DIST;
		float num = DELTA_GRENADE_RUN_DIST * 1.2f;
		DELTA_GRENADE_SAFE_DIST_SQRT = num * num;
		DIST_NOT_TO_GROUP_SQR = DIST_NOT_TO_GROUP * DIST_NOT_TO_GROUP;
		LAY_COEF = SIT_COEF;
	}
}
