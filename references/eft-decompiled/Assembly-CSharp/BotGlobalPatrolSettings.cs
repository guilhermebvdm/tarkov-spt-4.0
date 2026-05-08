using System;

[Serializable]
public class BotGlobalPatrolSettings
{
	[GAttribute4("Время которое бот проводит на точке для осмотра на обычном пути")]
	public float LOOK_TIME_BASE = 12f;

	[GAttribute4(" Время которое бот проводит на точки Резервного пути")]
	public float RESERVE_TIME_STAY = 72f;

	[GAttribute4(" Время которое бот проводит на точке лута Резервного пути")]
	public float RESERVE_LOOT_TIME_STAY = 15f;

	[GAttribute4("Дельта поиска длуга на резервном пути для душевной беседы")]
	public float FRIEND_SEARCH_SEC = 12f;

	[GAttribute4("Дельта фразы в беседе")]
	public float TALK_DELAY = 1.1f;

	[GAttribute4("Дельта на говорливость")]
	public float MIN_TALK_DELAY = 10f;

	[GAttribute4("Если друзей нет то будем говорить с такой дельтой")]
	public float TALK_DELAY_BIG = 15.1f;

	[GAttribute4("базовое время когда бот может решить сменить пути")]
	public float CHANGE_WAY_TIME = 125.1f;

	[GAttribute4("")]
	public float MIN_DIST_TO_CLOSE_TALK = 5f;

	[GAttribute4("Коэфициент на который режется дистанция в \"мирном\" режиме")]
	public float VISION_DIST_COEF_PEACE = 0.5f;

	[GAttribute4("")]
	public float MIN_DIST_TO_CLOSE_TALK_SQR;

	[GAttribute4("Шанс срезать путь при патрулировании")]
	public float CHANCE_TO_CUT_WAY_0_100 = 75f;

	[GAttribute4("Минимальный процент на который дистанци пути при патруле мб срезанна")]
	public float CUT_WAY_MIN_0_1 = 0.4f;

	[GAttribute4("Максимальный")]
	public float CUT_WAY_MAX_0_1 = 0.65f;

	[GAttribute4("Шанс сменить путь патрулирования")]
	public float CHANCE_TO_CHANGE_WAY_0_100 = 50f;

	[GAttribute4("Вероятность того что бот попытается сделать контрольный выстрел в тело врага")]
	public int CHANCE_TO_SHOOT_DEADBODY = 52;

	[GAttribute4("Время жизни подозрительной точки.")]
	public float SUSPETION_PLACE_LIFETIME = 7f;

	[GAttribute4("Через сколько времени если бот встал на резервный маршрут он попробует найти новый.")]
	public float RESERVE_OUT_TIME = 30f;

	[GAttribute4("Дистанция до срединной точки пути для резервных путей что бы путь можно было выбрать.")]
	public float CLOSE_TO_SELECT_RESERV_WAY = 25f;

	[GAttribute4("Если бот захочет выполнить запрос на предупреждение он должен быть ближе чем Х к цели по ОСИ Y - это для этажности, но если земля кривая то лучше ставить побольше.")]
	public float MAX_YDIST_TO_START_WARN_REQUEST_TO_REQUESTER = 4f;

	[GAttribute4("Если бот захочет выполнить запрос на предупреждение он должен быть ближе чем Х к цели по ОСИ Y - это для этажности, но если земля кривая то лучше ставить побольше. Только для друзей отряда")]
	public float MAX_YDIST_TO_START_WARN_REQUEST_TO_REQUESTER_ALLY = 4f;

	[GAttribute4("Может ли вставать на альтернативные пути")]
	public bool CAN_CHOOSE_RESERV;

	[GAttribute4("использует ли ТОЛЬКО на альтернативные пути")]
	public bool USE_ONLY_RESERV;

	[GAttribute4("Переодичность смены направления взгляда головы.")]
	public float HEAD_TURN_PERIOD_TIME = 2f;

	[GAttribute4("Переодичность смены направления взгляда головы.")]
	public float HEAD_FRONT_PERIOD_TIME = 13f;

	[GAttribute4("Шанс проиграть жест при встречи")]
	public float CHANCE_TO_PLAY_GESTURE_WHEN_CLOSE = 50f;

	[GAttribute4("Скорость поворота головы")]
	public float HEAD_TURN_SPEED = 41f;

	[GAttribute4("Угл поворота")]
	public float HEAD_ANG_ROTATE = 25f;

	[GAttribute4("Количество точек скопления лута, из которых бот выбирает случайную")]
	public int LOOT_PATROL_POTENTIAL_CLUSTERS_AMOUNT = 1;

	[GAttribute4("шанс сказать фразу во время приветствия")]
	public float CHANCE_TO_PLAY_VOICE_WHEN_CLOSE = 50f;

	[GAttribute4("")]
	public float GO_TO_NEXT_POINT_DELTA = 90f;

	[GAttribute4("")]
	public float GO_TO_NEXT_POINT_DELTA_RESERV_WAY = 15f;

	[GAttribute4("С какого расстояния ощущают труп")]
	public float DEAD_BODY_SEE_DIST = 50f;

	[GAttribute4("Если бот оказался дальше чем Х метров то он забудет о теле")]
	public float DEAD_BODY_LEAVE_DIST = 70f;

	[GAttribute4("Может ли бот смотреть на трупы в мирном режиме")]
	public bool CAN_LOOK_TO_DEADBODIES;

	[GAttribute4("Сколько бот будет делать PeacefulAction")]
	public float GESTURE_LENGTH = 2f;

	[GAttribute4("Надо ли остановиться для PeacefulAction")]
	public bool SHALL_STOP_IN_PEACEFUL_ACTION;

	[GAttribute4("Зафорсить ли оппонента на акшен")]
	public bool FORCE_OPPONENT_TO_PEAEFUL;

	[GAttribute4("Сколько секунд бот будет стоять на точке применения набора хирурга")]
	public float RESERVE_USE_SURGE_TIME_STAY = 40f;

	[GAttribute4("")]
	public bool RESERV_CAN_USE_MEDS;

	[GAttribute4("Надо ли остановиться для PeacefulAction")]
	public bool USE_PATROL_POINT_ACTION_MOVE_BY_RESERVE_WAY = true;

	[GAttribute4("Шанс начать использовать хир набор если бот смотрит на тело.")]
	public float USE_SURGIAL_KIT_OVER_THE_BODY_CAHNCE_100 = 50f;

	[GAttribute4("Шанс начать использовать хир набор второй раз если бот смотрит на тело.")]
	public float USE_SURGIAL_KIT_OVER_THE_BODY_SECOND_CAHNCE_100 = 50f;

	[GAttribute4("Если у фолловера стоит режим изди вместе с боссом то он задержится на Х")]
	public float FOLLOWER_START_MOVE_DELAY;

	[GAttribute4("Использование закешированных путей для патруля")]
	public bool USE_CHACHE_WAYS = true;

	[GAttribute4("Список вещей доступных для дропа")]
	public string ITEMS_TO_DROP = "";

	[GAttribute4("Шанс что бот побежит между точками на патруле если это предусмотрено другими условиями и бот использует кешированные маршруты")]
	public float SPRINT_BETWEEN_CACHED_POINTS = 55f;

	[GAttribute4("Базовая Переодичность проверки магазина")]
	public float CHECK_MAGAZIN_PERIOD = 30f;

	[GAttribute4("Переодичность с которой бот может есть/пить")]
	public float EAT_DRINK_PERIOD = 30f;

	[GAttribute4("")]
	public float WATCH_SECOND_WEAPON_PERIOD = 30f;

	[GAttribute4("")]
	public bool CAN_WATCH_SECOND_WEAPON;

	[GAttribute4("Базовое время осмотра трупа")]
	public float DEAD_BODY_LOOK_PERIOD = 17f;

	[GAttribute4("")]
	public bool CAN_HARD_AIM;

	[GAttribute4("")]
	public bool CAN_PEACEFUL_LOOK = true;

	[GAttribute4("")]
	public bool CAN_FRIENDLY_TILT;

	[GAttribute4("может ли бот жестикулировать")]
	public bool CAN_GESTUS = true;

	[GAttribute4("При рождении будет пытаться выбрать резервный путь")]
	public bool TRY_CHOOSE_RESERV_WAY_ON_START;

	[GAttribute4("")]
	public bool CAN_CHECK_MAGAZINE = true;

	[GAttribute4(" При поднятии предмета всегда отправлять в рюкзак или контейнер")]
	public bool PICKUP_ITEMS_TO_BACKPACK_OR_CONTAINER;

	public bool DO_RANDOM_DROP_ITEM;

	[GAttribute4("Диапазон времени, которое должно пройти после лутания для возможности лутаться в соледующий раз")]
	public float STAY_AFTER_LOOT_TIME_MIN = 20f;

	[GAttribute4("Диапазон времени, которое должно пройти после лутания для возможности лутаться в соледующий раз")]
	public float STAY_AFTER_LOOT_TIME_MAX = 40f;

	[GAttribute4("Боты используют реальный лутинг или его симуляцию")]
	public bool USE_REAL_LOOTING = true;

	[GAttribute4("Вероятность в процентах того, что бот дропнет вещь, которую он не стал брать из трупа. Больше = вероятнее.")]
	public float DEAD_BODY_DROP_ITEM_PROBABILITY = 50f;

	public BotGlobalPatrolSettings()
	{
		Update();
	}

	public void Update()
	{
		MIN_DIST_TO_CLOSE_TALK_SQR = MIN_DIST_TO_CLOSE_TALK * MIN_DIST_TO_CLOSE_TALK;
	}
}
