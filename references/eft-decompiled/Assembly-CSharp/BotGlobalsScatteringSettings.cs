using System;

[Serializable]
public class BotGlobalsScatteringSettings
{
	[GAttribute4(" [метры на расстояние]\tМинимальный угол разброса")]
	public float MinScatter = 0.03f;

	[GAttribute4(" [метры на расстояние]\tРабочий угол разброса")]
	public float WorkingScatter = 0.15f;

	[GAttribute4("[метры на расстояние]\tМаксимальный угол разброса")]
	public float MaxScatter = 0.4f;

	[GAttribute4(" [метры на расстояние]/сек\tСкорость схождения угла разброса")]
	public float SpeedUp = 0.3f;

	[GAttribute4("  Float\tКоэффициент на который умножается скорость схождения угла разброса при прицеливании. Больше лучше")]
	public float SpeedUpAim = 1.4f;

	[GAttribute4("   Попугаи/сек\tСкорость расхождения угла разброса. Больше лучше")]
	public float SpeedDown = -0.3f;

	[GAttribute4("   Попугаи/сек\tСкорость бота после которой начинается замедление схождения угла разброса")]
	public float ToSlowBotSpeed = 1.5f;

	[GAttribute4("   Попугаи/сек\tСкорость бота после которой начинается останавливается схождение угла разброса")]
	public float ToLowBotSpeed = 2.4f;

	[GAttribute4(" Попугаи/сек Скорость бота после которой начинается расхождение угла разброса")]
	public float ToUpBotSpeed = 3.6f;

	[GAttribute4("Коэфициент. Больше хуже. Насколько изменится скорсоть сведения если удельная скорость (ToSlowBotSpeed,ToLowBotSpeed) в этом промежутке")]
	public float MovingSlowCoef = 1.5f;

	[GAttribute4("  Градусы/сек\tСкорость поворота бота после которой начинается расхождение угла разброса")]
	public float ToLowBotAngularSpeed = 80f;

	[GAttribute4("")]
	public float ToStopBotAngularSpeed = 40f;

	[GAttribute4("   Градусы\tНа сколько расходится угол разброса бота при попадании по нему умноженное на урон.")]
	public float FromShot = 0.001f;

	[GAttribute4(" Float\tМножитель на сколько быстрей будет сходиться значение ScatterSpeed при использовании трассирующих пуль")]
	public float TracerCoef = 1.3f;

	[GAttribute4(" Float\tКоэффициент изменения минимального круга точности  при выбитой руке")]
	public float HandDamageScatteringMinMax = 0.7f;

	[GAttribute4(" Float kоэффициент скорости схождения угла прицеливания при выбитой руке")]
	public float HandDamageAccuracySpeed = 1.3f;

	[GAttribute4("   Float\tКоэффициент изменения рабочего круга точности при кровотечении")]
	public float BloodFall = 1.45f;

	[GAttribute4(" В процентах\tКоличество оставшихся патронов для перехода в состояние экономии патронов 0_1")]
	public float Caution = 0.3f;

	[GAttribute4("  Float\tКоэффициент изменения предпочтительного круга точности в режиме экономии патронов")]
	public float ToCaution = 0.6f;

	[GAttribute4("  Float\tКоэффициент контроля отдачи зависящий от отдачи оружия. Увеличивает текущий круг при вылете пули из ствола. Для одиночных выстрелов.")]
	public float RecoilControlCoefShootDone = 0.0003f;

	[GAttribute4("  Float\tКоэффициент контроля отдачи зависящий от отдачи оружия. Увеличивает текущий круг при вылете пули из ствола. для автоматического огня")]
	public float RecoilControlCoefShootDoneAuto = 0.00015f;

	[GAttribute4("Попугаи. Как высоко подскакивает при амплитуде прицел")]
	public float AMPLITUDE_FACTOR = 0.25f;

	[GAttribute4("Попугаи. Скорость амплитуды прицеливания")]
	public float AMPLITUDE_SPEED = 0.1f;

	[GAttribute4("Метры. Дистанция от новой точки прицеливая до старой, если больше чем Х то бот автоматически считает что он неприцелился независимо не от чего остального.")]
	public float DIST_FROM_OLD_POINT_TO_NOT_AIM = 15f;

	[GAttribute4("")]
	public float DIST_FROM_OLD_POINT_TO_NOT_AIM_SQRT;

	[GAttribute4("Метры. Если точка прицеливания ближе чем Х то бот не будет стрелять")]
	public float DIST_NOT_TO_SHOOT = 0.3f;

	[GAttribute4("В момент смены положения текущий круг сведения увеличится на Х*степень смены положения")]
	public float PoseChnageCoef = 0.1f;

	[GAttribute4(" В момент смены положения на лежа/нележа текущий круг сведения увеличится на Х")]
	public float LayFactor = 0.1f;

	[GAttribute4("насколько вверх подбрасывает ствол. Коэфициент от отдачи оружия.")]
	public float RecoilYCoef = 0.0005f;

	[GAttribute4("Скорость снижения отдачи вверх")]
	public float RecoilYCoefSppedDown = -0.52f;

	[GAttribute4("насколько максимально может подняться отдача.")]
	public float RecoilYMax = 1f;

	public BotGlobalsScatteringSettings()
	{
		Update();
	}

	public void Update()
	{
		DIST_FROM_OLD_POINT_TO_NOT_AIM_SQRT = DIST_FROM_OLD_POINT_TO_NOT_AIM * DIST_FROM_OLD_POINT_TO_NOT_AIM;
	}
}
