using System;
using Comfort.Common;
using EFT.Bots;

public abstract class GClass1895
{
	[NonSerialized]
	public const int Int_0 = 0;

	[NonSerialized]
	public const int Int_1 = 15;

	[NonSerialized]
	public const int Int_2 = 20;

	[NonSerialized]
	public const int Int_3 = 25;

	[NonSerialized]
	public const int Int_4 = 35;

	public static int ToMaxBotsCount(this EBotAmount botAmount)
	{
		return botAmount switch
		{
			EBotAmount.NoBots => 0, 
			EBotAmount.Low => 15, 
			EBotAmount.Medium => 20, 
			EBotAmount.High => 25, 
			EBotAmount.Horde => 35, 
			_ => 15, 
		};
	}

	public static int ToBotAmountSlots(this EBotAmount botDifficulty, int slotsMin, int slotsMax)
	{
		switch (botDifficulty)
		{
		case EBotAmount.NoBots:
		case EBotAmount.Low:
		{
			float num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_LOW : LocalBotSettingsProviderClass.Core.WAVE_COEF_LOW);
			return (int)(0.5f + (float)slotsMin * num);
		}
		default:
		{
			float num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_MID : LocalBotSettingsProviderClass.Core.WAVE_COEF_MID);
			return (int)(0.5f + num * (float)(slotsMax - slotsMin) / 2f);
		}
		case EBotAmount.High:
		{
			float num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HIGH : LocalBotSettingsProviderClass.Core.WAVE_COEF_HIGH);
			return (int)(0.5f + (float)slotsMax * num);
		}
		case EBotAmount.Horde:
		{
			float num = ((Singleton<BackendConfigSettingsClass>.Instance != null) ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HORDE : LocalBotSettingsProviderClass.Core.WAVE_COEF_HORDE);
			return (int)(0.5f + (float)slotsMax * num);
		}
		}
	}
}
