using System;

[Serializable]
public class BotGlobalsMoveSettings
{
	[GAttribute4("degree per second")]
	public float BASE_ROTATE_SPEED = 270f;

	[GAttribute4("Дистанция меньше которой бот считает что он пришел в точку назначения")]
	public float REACH_DIST = 0.5f;

	[GAttribute4("Дистанция меньше которой бот считает что он пришел в точку назначения при беге")]
	public float REACH_DIST_RUN = 1f;

	[GAttribute4("Дистанция на которой бот начинает замедлятся когда приближается к точке конц маршрута")]
	public float START_SLOW_DIST = 1.5f;

	[GAttribute4("Если не было указано чо надозамедлятся в конце то коэфиц замедления такой")]
	public float BASESTART_SLOW_DIST = 1.1f;

	[GAttribute4("Коэфициент замедления")]
	public float SLOW_COEF = 7f;

	[GAttribute4("Дистанция на которой")]
	public float DIST_TO_CAN_CHANGE_WAY = 8f;

	[GAttribute4("Дистанция с которой начинаются попытки увидеть точку опастности")]
	public float DIST_TO_START_RAYCAST = 15f;

	[GAttribute4("Базовый радиус круга поиска точек укрытия для продвижения к точке апостности")]
	public float BASE_START_SERACH = 35f;

	[GAttribute4("Частота перерасчета пути то точки назначения")]
	public float UPDATE_TIME_RECAL_WAY = 4f;

	[GAttribute4("Дистанция до точки что бы начать бежать к ней")]
	public float FAR_DIST = 4f;

	[GAttribute4("    public float NEAR_DIST_SQR;")]
	public float FAR_DIST_SQR;

	[GAttribute4("")]
	public float DIST_TO_CAN_CHANGE_WAY_SQR;

	[GAttribute4("")]
	public float DIST_TO_START_RAYCAST_SQR;

	[GAttribute4("")]
	public float BASE_SQRT_START_SERACH;

	[GAttribute4("Если точка до который идет меьше чем Х по У то она срежется до 0")]
	public float Y_APPROXIMATION = 0.7f;

	[GAttribute4("по бот идет и если время видения последнего места врага то меньше Х то он будет смотреть туда")]
	public float DELTA_LAST_SEEN_ENEMY = 20f;

	[GAttribute4("если новое укрыти которое нашел бот ближе чем Х то бт ужэе счиатет что он в укрытии")]
	public float REACH_DIST_COVER = 2f;

	[GAttribute4(" Если бот бежит к точке то если до дистанции было меньше чем Х то бот перейдет на шаг")]
	public float RUN_TO_COVER_MIN = 4f;

	[GAttribute4("Вероятность того что бот будет бегать когда у него закончатся патроны и он вне укрытия")]
	public float CHANCE_TO_RUN_IF_NO_AMMO_0_100 = 100f;

	[GAttribute4("Если я не вижу врага то я буду убегать если у меня состояние Ambush")]
	public bool RUN_IF_CANT_SHOOT;

	[GAttribute4("Бежать можно если враг дальше чем Х метров")]
	public float RUN_IF_GAOL_FAR_THEN = 15f;

	[GAttribute4("Через сколько секунд после начала движения со стрельбой будет совершена проверка на возможность убежать")]
	public float SEC_TO_CHANGE_TO_RUN = 3f;

	[GAttribute4("")]
	public bool ETERNITY_STAMINA;

	public bool STOP_SPRINT_AT_TREE = true;

	[GAttribute4("Сколько ждать анимации двери")]
	public float WAIT_DOOR_OPEN_SEC = 2.5f;

	[GAttribute4("Шанс на открывание двери с ноги    ")]
	public float BREACH_CHANCE_100 = 40f;

	[GAttribute4("Скорость первого поворота при менее 90 градусов")]
	public float FIRST_TURN_SPEED = 160f;

	[GAttribute4("Скорость первого поворота при более 90 градусов")]
	public float FIRST_TURN_BIG_SPEED = 320f;

	[GAttribute4("Скорость поворота при спринте")]
	public float TURN_SPEED_ON_SPRINT = 200f;

	[GAttribute4("Отключает переделку пути в зиг-заг")]
	public bool NO_ZIG_ZAG;

	[GAttribute4("может ли спринтовать когда идет в ноде GoToSomepoint")]
	public bool CAN_SPRINT_GO_TO_SOME_POINT = true;

	[GAttribute4("до какой дистанции может спринтовать")]
	public float DIST_SPRINT_GO_TO_SOME_POINT = 5f;

	[GAttribute4("Мили атака в зиг заге")]
	public bool MELEE_ATTACK_ZIG_ZAG;

	[GAttribute4("Мили атака будт реже пересчитывать путь")]
	public bool MELEE_ATTACK_OPTIMIZE;

	public BotGlobalsMoveSettings()
	{
		Update();
	}

	public void Update()
	{
		DIST_TO_CAN_CHANGE_WAY_SQR = DIST_TO_CAN_CHANGE_WAY * DIST_TO_CAN_CHANGE_WAY;
		DIST_TO_START_RAYCAST_SQR = DIST_TO_START_RAYCAST * DIST_TO_START_RAYCAST;
		BASE_SQRT_START_SERACH = BASE_START_SERACH * BASE_START_SERACH;
		FAR_DIST_SQR = FAR_DIST * FAR_DIST;
	}
}
