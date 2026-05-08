using EFT;

public abstract class GClass1818 : GClass1817
{
	public abstract string BaseValueFormat { get; }

	public override string ValueFormat
	{
		get
		{
			HealthControllerClass.CalculateRegenBaseRatesPerSecond(out var health, out var energy, out var hydration);
			HealthControllerClass.CalculateRegenRatesPerSecond(out var health2, out var energy2, out var hydration2);
			float num = 0f;
			float num2 = 0f;
			switch (BonusType)
			{
			case EBonusType.EnergyRegeneration:
				num = energy2;
				num2 = (float)CalculateValue(energy) - energy;
				break;
			case EBonusType.HydrationRegeneration:
				num = hydration2;
				num2 = (float)CalculateValue(hydration) - hydration;
				break;
			case EBonusType.HealthRegeneration:
				num = health2;
				num2 = (float)CalculateValue(health) - health;
				break;
			}
			num *= 3600f;
			num2 *= 3600f;
			return string.Format(string.Format(GClass855.Positive(base.Value) ? "+{0}" : "{0}", GClass2348.Localized(BaseValueFormat)), num2.ToString("0"), num.ToString("0"));
		}
	}

	public GClass1818(ProfileBonusesClass descriptor)
		: base(descriptor)
	{
	}
}
