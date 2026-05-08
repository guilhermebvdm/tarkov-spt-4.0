using System;

[Serializable]
public class BotGlobalsChangeSettings
{
	[GAttribute4("коэфициент дальности видимости когда внутри дыма")]
	public float SMOKE_VISION_DIST = 0.6f;

	[GAttribute4("коэфициент скорости замечания когда внутри дыма")]
	public float SMOKE_GAIN_SIGHT = 0.625f;

	[GAttribute4("коэфициент точности когда внутри дыма")]
	public float SMOKE_SCATTERING = 1.6f;

	[GAttribute4("коэфициент скорости прицеливания когда внутри дыма")]
	public float SMOKE_PRECICING = 1.6f;

	[GAttribute4("коэфициент слышимости дальности когда внутри дыма")]
	public float SMOKE_HEARING = 1f;

	[GAttribute4("коэфициент скорости прицеливания когда внутри дыма")]
	public float SMOKE_ACCURATY = 1.6f;

	[GAttribute4("коэфициент шанса лечь в случае опастности внезапной когда внутри дыма")]
	public float SMOKE_LAY_CHANCE = 1.6f;

	[GAttribute4("коэфициент дальности видимости когда ослеплен")]
	public float FLASH_VISION_DIST = 0.2f;

	[GAttribute4("коэфициент скорости замечания когда ослеплен")]
	public float FLASH_GAIN_SIGHT = 0.55f;

	[GAttribute4("коэфициент точности когда ослеплен")]
	public float FLASH_SCATTERING = 1.6f;

	[GAttribute4("коэфициент скорости прицеливания когда ослеплен")]
	public float FLASH_PRECICING = 1.6f;

	[GAttribute4("коэфициент слышимости дальности когда ослеплен")]
	public float FLASH_HEARING = 1f;

	[GAttribute4("коэфициент скорости прицеливания когда ослеплен")]
	public float FLASH_ACCURATY = 1.6f;

	[GAttribute4("коэфициент шанса лечь в случае опастности внезапной когда ослеплен")]
	public float FLASH_LAY_CHANCE = 1f;

	[GAttribute4("коэфициент дальности слышимости когда оглушен")]
	public float STUN_HEARING = 0.6f;

	[GAttribute4("Бот невидим на клиенте. Используется, например, для бота, управляющего башней БТРа")]
	public bool INVISIBLE_ON_CLIENT;

	public void Update()
	{
	}
}
