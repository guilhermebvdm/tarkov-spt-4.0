using System;

[Serializable]
public class BotGlobalShootData
{
	[GAttribute4("Кулдаун очередного переключения на подствольный гранатомёт. В секундах")]
	public float SWITCH_TO_UNDERBARREL_WEAPON_COOLDOWN = 10f;

	[GAttribute4("Минимальное время в секундах для проверки принудительного переключения на подствол во время боя")]
	public float MIN_TIME_TO_CHECK_FORCE_SWITCH_TO_GRENADE_LAUNCHER = 1.5f;

	[GAttribute4("Максимальное время в секундах для проверки принудительного переключения на подствол во время боя")]
	public float MAX_TIME_TO_CHECK_FORCE_SWITCH_TO_GRENADE_LAUNCHER = 3f;

	[GAttribute4("Максимальное время в секундах после того как цель вышла из прямой видимости для переключения на подствольный гранатомёт.")]
	public float MAX_TIME_SEEN_TO_SWITCH_TO_GRENADE_LAUNCHER = 5f;

	[GAttribute4("Максимальное количество попыток выстрелов подряд из подствольного гранатамёта.")]
	public uint MAX_SUCCESS_GRENADE_LAUNCHER_SHOOT_ATTEMPTS = 10u;

	[GAttribute4("0-100. Вероятность очередного выстрела из подствольного гранатамёта. ")]
	public float SHOOT_PROBABILITY_GRENADE_LAUNCHER = 50f;

	[GAttribute4("Коэфициент в формуле расчёта минимальной дистанции ведения огня с подствольного гранатамёта")]
	public float LOW_DIST_K_FOR_GRENADE_LAUNCHER = 1.1f;

	[GAttribute4("Минимальная дистанция до цели в метрах, для переключения на подствольный гранатомёт")]
	public float DEFAULT_LOW_DIST_TO_USE_GRENADE_LAUNCHER = 18f;

	[GAttribute4("Порог дистанции до ещё одной цели рядом с текущей")]
	public float DISTANCE_TO_TARGET_NEAR_ENEMY_TRESHOLD = 12.5f;

	[GAttribute4("Отклонение гистерезиса дистанции до ещё одной цели рядом с текущей")]
	public float DISTANCE_TO_TARGET_NEAR_ENEMY_DEVIATION = 2.5f;

	[GAttribute4("Время за которое отдача уйдет")]
	public float RECOIL_TIME_NORMALIZE = 4f;

	[GAttribute4("НАсколько высоко подскочит отдача вверх в метрах в зависимостии от расстояния")]
	public float RECOIL_PER_METER = 0.3f;

	[GAttribute4("НАсколько высоко подскочит отдача вверх в метрах в зависимостии от расстояния")]
	public float MAX_RECOIL_PER_METER = 0.2f;

	[GAttribute4("НАсколько высоко подскочит отдача вбок в зависимостии от вертикальной отдачи")]
	public float HORIZONT_RECOIL_COEF = 0.4f;

	[GAttribute4("Перерыв между выстрелами.")]
	public float WAIT_NEXT_SINGLE_SHOT = 0.3f;

	[GAttribute4("Перерыв между выстрелами стационарка пулевая.")]
	public float WAIT_NEXT_STATIONARY_BULLET = 0.3f;

	[GAttribute4("Перерыв между выстрелами стационарко гранатомет.")]
	public float WAIT_NEXT_STATIONARY_GRENADE = 0.3f;

	[GAttribute4("Максимальный перерыв между выстремами для снайперов базовый")]
	public float WAIT_NEXT_SINGLE_SHOT_LONG_MAX = 3.3f;

	[GAttribute4("Минимальный")]
	public float WAIT_NEXT_SINGLE_SHOT_LONG_MIN = 0.8f;

	[GAttribute4("Перерыв между сериями выстрелов для заражённых")]
	public float NEXT_SINGLE_SHOT_PAUSE = 5f;

	[GAttribute4("Минимальная длина серии выстрелов для заражённых")]
	public float SINGLE_SHOT_SERIES_TIME_MIN = 9f;

	[GAttribute4("Максимальная длина серии выстрелов для заражённых")]
	public float SINGLE_SHOT_SERIES_TIME_MAX = 11f;

	[GAttribute4("Использовать серии одиночных выстрелов для заражённых")]
	public bool USE_SINGLE_SHOT_SERIES;

	[GAttribute4("Коэфициент зависимости частоты выстрелов снайперов (каждые Х метров - примерно сек)")]
	public float MARKSMAN_DIST_SEK_COEF = 44f;

	[GAttribute4("Сколько будет зажат палец курсе при одиночном огне")]
	public float FINGER_HOLD_SINGLE_SHOT = 0.14f;

	[GAttribute4("Сколько будет зажат палец  курсе при огне стационарке пулемете")]
	public float FINGER_HOLD_STATIONARY_BULLET = 0.14f;

	[GAttribute4("Сколько будет зажат палец  курсе при огне стационарке гранатомете")]
	public float FINGER_HOLD_STATIONARY_GRENADE = 0.14f;

	[GAttribute4("Сколько будем зажат палец на курсе при автоматическом огне")]
	public float BASE_AUTOMATIC_TIME = 0.1f;

	[GAttribute4("Коэфициент разлета при автоматической стрельбе")]
	public float AUTOMATIC_FIRE_SCATTERING_COEF = 2.5f;

	[GAttribute4("Шанс переключится в автоматический огонь еа старте игры")]
	public float CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = 76f;

	[GAttribute4("Минимальная дельта по стрельбе оружия для укрытий")]
	public float FAR_DIST_ENEMY = 20f;

	[GAttribute4("Если выстрелов из укрытия было сделно больше чем Х то возвращается в укрытие")]
	public int SHOOT_FROM_COVER = 4;

	[GAttribute4("")]
	public float FAR_DIST_ENEMY_SQR;

	[GAttribute4("Коэфициент на каоторы домнажается эффективная дистанция стрельбы что бы получить максимальную дистанцию стельбы.")]
	public float MAX_DIST_COEF = 1.35f;

	[GAttribute4("типа скорострельность 600 в мин => 10 в сек => 1 в 0.1 сек.")]
	public float RECOIL_DELTA_PRESS = 0.15f;

	[GAttribute4("Ессли враг дальше чем Х и у бота закончились патроны то он побежит в укрытие и перезарядится там.")]
	public float RUN_DIST_NO_AMMO = 25f;

	[GAttribute4("")]
	public float RUN_DIST_NO_AMMO_SQRT;

	[GAttribute4("Сколько раз надо несмочь выстрелить что бы не пытаться дальше стрелять, а пойти попрятаться.")]
	public int CAN_SHOOTS_TIME_TO_AMBUSH = 3;

	[GAttribute4("Если враг был виден более NOT_TO_SEE_ENEMY_TO_WANT_RELOAD_SEC сек назад и в магазине меньше чем Х патронов то перезарядится")]
	public float NOT_TO_SEE_ENEMY_TO_WANT_RELOAD_PERCENT = 0.4f;

	[GAttribute4("")]
	public float NOT_TO_SEE_ENEMY_TO_WANT_RELOAD_SEC = 2f;

	[GAttribute4("Если врагов давно небыло и в магазине меньше чем Х процентов патронов то перезарядится")]
	public float RELOAD_PECNET_NO_ENEMY = 0.6f;

	[GAttribute4("Шанс поменять оружие если кончились патроны.")]
	public float CHANCE_TO_CHANGE_WEAPON = 100f;

	[GAttribute4("Шанс поменять оружие если кончились патроны иу врага есть шлем.")]
	public float CHANCE_TO_CHANGE_WEAPON_WITH_HELMET = 100f;

	[GAttribute4("бот будет менять оружие только если враг дальше чем Х.")]
	public float LOW_DIST_TO_CHANGE_WEAPON = 10f;

	[GAttribute4("бот будет менять оружие только если враг ближе чем Х.")]
	public float FAR_DIST_TO_CHANGE_WEAPON = 50f;

	[GAttribute4("Настолько противник будет считаться засапрешеным если его сапрсить пулями")]
	public float SUPPRESS_BY_SHOOT_TIME = 6f;

	[GAttribute4("Сколько раз надо нажать на спуск что бы противник стал засапрешеным")]
	public int SUPPRESS_TRIGGERS_DOWN = 3;

	[GAttribute4("Сколько раз надо нажать на спуск что бы противник стал засапрешеным при условии списка точек")]
	public int SUPPRESS_TRIGGERS_DOWN_AS_LIST = 6;

	[GAttribute4("")]
	public float DIST_TO_CHANGE_TO_MAIN = 15f;

	[GAttribute4("Дистанция от врага при которой бот покинет АГС_17. ")]
	public float AGS_17_DIST_TO_LEAVE = 25f;

	[GAttribute4("Дистанция с которой можно бить/начать комбо")]
	public float DIST_TO_HIT_MELEE = 2f;

	[GAttribute4("Дистанция с которой можно продолжить комбо")]
	public float DIST_TO_HIT_MELEE_CONTINUE_COMBO = 1.8f;

	[GAttribute4("Дистанция с которой нужно остановить спринт")]
	public float DIST_TO_STOP_SPRINT_MELEE = 2.4f;

	[GAttribute4("Переодичность удара")]
	public float TRY_HIT_PERIOD_MELEE = 0.5f;

	[GAttribute4("насколько блокировать стрельбу когда ложишься")]
	public float BLOCK_PERIOD_WHEN_LAY = 1.25f;

	[GAttribute4("как часто может менять оружие в руках")]
	public float CHANGE_WEAPON_PERIOD = 1f;

	[GAttribute4("флаг для комбо атак в OneMeleeAttackNode")]
	public bool USE_MELEE_COMBOS;

	[GAttribute4("Время перезарядки мили удара в ноде OneHitNode")]
	public float MELEE_RESET_HIT_TIME = 0.5f;

	[GAttribute4("Дистанция, ближе которой бот не пытается подойти в мили ноде")]
	public float MELEE_STOP_MOVE_DISTANCE;

	[GAttribute4("100% - оружие ломается как у обычных игроков, 50% - в 2 раза реже, 0 - никогда")]
	public int VALIDATE_MALFUNCTION_CHANCE = 100;

	[GAttribute4("шанс чинить оружие сразу в момент поломки, не убегая в укрытие. Если не срабатывает - бот сначала прячется, только потом чинит")]
	public int REPAIR_MALFUNCTION_IMMEDIATE_CHANCE = 25;

	[GAttribute4("время в секундах между перехода в ноду малфанкшена и осмотром оружия")]
	public float DELAY_BEFORE_EXAMINE_MALFUNCTION = 1f;

	[GAttribute4("время в секундах между осмотром оружия и непосредственно починкой")]
	public float DELAY_BEFORE_FIX_MALFUNCTION = 1.5f;

	[GAttribute4("Бот попытается сменить оружие вместо перезарядки в бою")]
	public bool TRY_CHANGE_WEAPON_INSTEAD_RELOAD;

	[GAttribute4("Милишная атака делает зиг заг")]
	public bool MELEE_ATTACK_ZIG_ZAG;

	[GAttribute4("Минимальная дистанция до противника когда бот попытается сменить оружие вместо перезарядки в бою")]
	public float MIN_DIST_TO_ENEMY_TO_CHANGE_WEAPON_INSTEAD_RELOAD = 30f;

	[GAttribute4("Шанс сменить оружие вместо перезарядки в бою")]
	public float CHANCE_TO_CHANGE_WEAPON_INSTEAD_RELOAD = 60f;

	[GAttribute4("Шанс сменить оружие вместо перезарядки в бою когда у противника нет шлема")]
	public float CHANCE_TO_CHANGE_WEAPON_INSTEAD_RELOAD_ENEMY_WITHOUT_HELM = 90f;

	[GAttribute4("дистанция для остановки перед рукопашной атакой (положительная == промежать за спину игрока)")]
	public float MELEE_STOP_DIST = 0.3f;

	[GAttribute4("Блокировка стиринга ботгов (создавалась для БТРа)")]
	public bool BLOCK_STEERING;

	[GAttribute4("Блокировка стиринга ботгов (создавалась для БТРа)")]
	public bool USE_BTR_CANSHOOT;

	[GAttribute4("Дистанция, на которой используется AssaultEnemyFarLayer для всех пушек")]
	public float FAR_DISTANCE_ALL_WEAPONS = 100f;

	[GAttribute4("Дистанция, на которой используется AssaultEnemyFarLayer для пистолетов")]
	public float FAR_DISTANCE_PISTOLS = 30f;

	[GAttribute4("Дистанция, на которой используется AssaultEnemyFarLayer для дробовиков")]
	public float FAR_DISTANCE_SHOTGUNS = 30f;

	[GAttribute4("Длительность замечения противника, после которой бот в AssaultEnemyFarLayer будет менять позицию")]
	public float FAR_DIST_EYE_CONTACT_TIME_TO_CHANGE_COVER = 5f;

	[GAttribute4("меняет ли бот оружие на основное во время патруля")]
	public bool CHANGE_TO_MAIN_WEAPON_WHEN_PATROL;

	[GAttribute4("")]
	public float SHOOT_IMMEDIATELY_DIST = 25f;

	public bool CAN_STOP_SHOOT_CAUSE_ANIMATOR;

	public bool TRY_CHANGE_WEAPON_WHEN_RELOAD = true;

	public bool CHANGE_TO_MAIN_WHEN_SUPPORT_NO_AMMO = true;

	public float LAST_SEEN_TIME_TO_START_SUPPRESS_STATIONARY_AGS = 20f;

	public float STATIONARY_GRENADE_MIN_DIST_TO_TAKE = 20f;

	public double STATIONARY_SIMPLE_MIN_DIST_TO_TAKE = 5.0;

	public bool NO_OFFSET_SHOOTING_FROM_PLAYER;

	public bool ALTERNATIVE_KNIFE_KICK;

	[GAttribute4("дистанция с которой бот выключает авто огонь")]
	public float DITANCE_TO_OFF_AUTO_FIRE = -1f;

	[GAttribute4("дистанция с которой бот может включать авто огонь")]
	public float DITANCE_TO_ON_AUTO_FIRE = 50f;

	[GAttribute4("дистанция с которой бот не промахиваеться")]
	public float MISS_ON_CRITICAL_DIST = 3f;

	[GAttribute4("Бот промахиваеться после спринта")]
	public bool MISS_AFTER_SPRINT;

	[GAttribute4("Бот промахиваеться в голову")]
	public bool MISS_TO_HEAD;

	[GAttribute4("Бот промахиваеться во время движения")]
	public bool MISS_ON_MOVE;

	[GAttribute4("Бот промахиваеться во время анимаций перехода")]
	public bool MISS_ON_TRANSITION;

	public void Update()
	{
		FAR_DIST_ENEMY_SQR = FAR_DIST_ENEMY * FAR_DIST_ENEMY;
		RUN_DIST_NO_AMMO_SQRT = RUN_DIST_NO_AMMO * RUN_DIST_NO_AMMO;
	}
}
