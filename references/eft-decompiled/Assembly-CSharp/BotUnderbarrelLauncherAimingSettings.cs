using System;

[Serializable]
public class BotUnderbarrelLauncherAimingSettings
{
	public float AIMING_ON_WAY = 50f;

	[GAttribute4("Шанс что бот будет включать фонарик когда целится")]
	public float ANYTIME_LIGHT_WHEN_AIM_100 = -1f;

	[GAttribute4("Базовое кл-во попадаий мимо МИН")]
	public int BAD_SHOOTS_MIN;

	[GAttribute4("Базовое кл-во попадаий мимо МАКС")]
	public int BAD_SHOOTS_MAX;

	public float START_TIME_COEF = 1f;

	[GAttribute4("это вероятность того что бот скосит стрельбу при попадании по нему. Альтернатива - ухудшить время прицеливания.")]
	public float DAMAGE_TO_DISCARD_AIM_0_100 = 86f;

	[GAttribute4("Минимальное ухудшение времени прицеливания")]
	public float MIN_TIME_DISCARD_AIM_SEC = 0.3f;

	[GAttribute4("Макс ухудшение времени прицеливания")]
	public float MAX_TIME_DISCARD_AIM_SEC = 1.3f;

	[GAttribute4("Время для Максимальное улучшение стрельбы в зависимости от того как долго бот целиться")]
	public float MAX_AIM_PRECICING = 5f;

	[GAttribute4("насколько лучше может стать стрельба от пристрелки - 0.15 == 85%.  0.5 == 50%  . 1 == 0%")]
	public float MAX_AIMING_UPGRADE_BY_TIME = 0.7f;

	[GAttribute4("Бот считается что двигается если он прошел за кадр больше чем Х")]
	public float BOT_MOVE_IF_DELTA = 0.01f;

	[GAttribute4("Время паники при обычное")]
	public float PANIC_TIME = 6f;

	[GAttribute4("После скольки попыток доприцелится бот все равно переприцелится даже если было цель очень близко  min max")]
	public int RECALC_MUST_TIME_MIN = 1;

	[GAttribute4("После скольки попыток доприцелится бот все равно переприцелится даже если было цель очень близко  min max")]
	public int RECALC_MUST_TIME_MAX = 2;

	[GAttribute4("дистанции при сдвижение на который прицеливание не прерветься по Y")]
	public float RECLC_Y_DIST = 1.2f;

	[GAttribute4("    float BASE_ANF_COEF = 7;")]
	public float RECALC_SQR_DIST;

	[GAttribute4("Настолько дольше будет прицеливание если бот стреляет сходу")]
	public float TIME_COEF_IF_MOVE = 1.5f;

	[GAttribute4("Время прицеливания умножается на этот коэфициент если чар паникует")]
	public float PANIC_COEF = 3.5f;

	[GAttribute4("усиленное прицеливание когда выглядываешь за укрытия")]
	public float COEF_FROM_COVER = 0.85f;

	[GAttribute4("Базовое время прицеливания. Прибавляетя к результату полученному по формуле")]
	public float BOTTOM_COEF = 0.3f;

	[GAttribute4("Максимальное время прицеливания")]
	public float MAX_AIM_TIME = 1.5f;

	[GAttribute4("Модификатор зависимости прицеливанипя от дистанции не линейног (за линейность отвечает др. параметр).   //рекумендуемые знаечни 0.2..1.3.     //Меньше 1 - значит чем дальше тем будет точнее чем лнейная зависимость. Больше - косее.")]
	public float SCATTERING_DIST_MODIF = 0.8f;

	[GAttribute4(" Модификатор зависимости прицеливанипя от дистанции не линейног (за линейность отвечает др. параметр).   //рекумендуемые знаечни 0.2..1.3.     //Меньше 1 - значит чем дальше тем будет точнее чем лнейная зависимость. Больше - косее.")]
	public float SCATTERING_DIST_MODIF_CLOSE = 0.6f;

	[GAttribute4("Если враг ближе чем Х то разлета не будет.")]
	public float DIST_TO_SHOOT_NO_OFFSET = 3f;

	[GAttribute4("Коэфициент увеличения разлета при панике")]
	public float PANIC_ACCURATY_COEF = 3f;

	[GAttribute4("Коэфициент улучшенного прицеливания")]
	public float HARD_AIM = 0.75f;

	[GAttribute4("Настолько больше будет разлет если бот стреляет сходу")]
	public float COEF_IF_MOVE = 1.5f;

	[GAttribute4("Уровень сжатия прицельной сферы сверху")]
	public float Y_TOP_OFFSET_COEF = 0.2f;

	[GAttribute4("Уровень сжатия прицельной сферы снизу")]
	public float Y_BOTTOM_OFFSET_COEF = 0.2f;

	[GAttribute4("Насколько выше выстрелит бот если захочет стрелять мимо")]
	public float NEXT_SHOT_MISS_Y_OFFSET = 1f;

	[GAttribute4("если мы стреляем мимо, то бот настолько отводит свой прицел от цели в метрах")]
	public float BAD_SHOOTS_OFFSET = 1f;

	[GAttribute4("Базовый коэфициент из формулы == N        N*ln(x/5+1.2)")]
	public float BAD_SHOOTS_MAIN_COEF = 1f;

	public float OFFSET_RECAL_ANYWAY_TIME = 1f;
}
