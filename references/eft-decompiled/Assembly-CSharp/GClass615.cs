using System;
using System.Runtime.CompilerServices;
using EFT;

public class GClass615
{
	[CompilerGenerated]
	public class Class288
	{
		public GClass615 gclass615_0;

		public BotLastBlindEffectModifierClass modif;

		public void method_0()
		{
			gclass615_0.Dismiss(modif);
		}
	}

	public float _precicingSpeedCoef = 1f;

	public float _accuratySpeedCoef = 1f;

	public float _layChanceDangerCoef = 1f;

	public float _visibleDistCoef = 1f;

	public float _runtimeVisionEffectK = 1f;

	public float _scatteringCoef = 1f;

	public float _priorityScatteringCoef = 1f;

	public float _hearingDistCoef = 1f;

	public float _waitInCoverCoef = 1f;

	public float _triggerDownDelay = 1f;

	[NonSerialized]
	public ScatteringSettingsClass ScatteringSettingsClass;

	[NonSerialized]
	[CompilerGenerated]
	public BotSettingsComponents BotSettingsComponents_0_1;

	public BotSettingsComponents BotSettingsComponents_0
	{
		[CompilerGenerated]
		get
		{
			return BotSettingsComponents_0_1;
		}
	}

	public float PriorityScatter1meter => ScatteringSettingsClass.CurrentScattering.PriorityScatter1meter * _priorityScatteringCoef;

	public float PriorityScatter10meter => ScatteringSettingsClass.CurrentScattering.PriorityScatter10meter * _priorityScatteringCoef;

	public float PriorityScatter100meter => ScatteringSettingsClass.CurrentScattering.PriorityScatter100meter * _priorityScatteringCoef;

	public float CurrentMaxScatter => BotSettingsComponents_0.Scattering.MaxScatter * _scatteringCoef;

	public float CurrentWorkingScatter => BotSettingsComponents_0.Scattering.WorkingScatter * _scatteringCoef;

	public float CurrentMinScatter => BotSettingsComponents_0.Scattering.MinScatter * _scatteringCoef;

	public float CurrentPrecicingSpeed => BotSettingsComponents_0.Aiming.BETTER_PRECICING_COEF * _precicingSpeedCoef;

	public float CurrentScatteringUp => BotSettingsComponents_0.Scattering.SpeedUp * _accuratySpeedCoef;

	public float CurrentAccuratySpeed => BotSettingsComponents_0.Core.AccuratySpeed * _accuratySpeedCoef;

	public float CurrentLayChance => BotSettingsComponents_0.Mind.PANIC_LAY_WEIGHT * _layChanceDangerCoef;

	public float CurrentVisibleDistance => BotSettingsComponents_0.Core.VisibleDistance * _visibleDistCoef;

	public float RuntimeVisionEffectsK => BotSettingsComponents_0.Look.BASE_RUNTIME_EFFECT_K * _runtimeVisionEffectK;

	public float CurrentScattering => BotSettingsComponents_0.Core.ScatteringPerMeter * _scatteringCoef;

	public float CurrentScatteringClose => BotSettingsComponents_0.Core.ScatteringClosePerMeter * _scatteringCoef;

	public float CurrentHearingSense => BotSettingsComponents_0.Core.HearingSense * _hearingDistCoef;

	public float CurrentHoldDownAutoFire => BotSettingsComponents_0.Shoot.BASE_AUTOMATIC_TIME * _triggerDownDelay;

	public float CurrentHoldDownSingleShot => BotSettingsComponents_0.Shoot.FINGER_HOLD_SINGLE_SHOT * _triggerDownDelay;

	public float CurrentHoldDownStationaryBullet => BotSettingsComponents_0.Shoot.FINGER_HOLD_STATIONARY_BULLET * _triggerDownDelay;

	public float CurrentHoldDownStationaryGrenade => BotSettingsComponents_0.Shoot.FINGER_HOLD_STATIONARY_GRENADE * _triggerDownDelay;

	public float CloseHearingSense => BotSettingsComponents_0.Hearing.CLOSE_DIST * _hearingDistCoef;

	public float FarHearingSense => BotSettingsComponents_0.Hearing.FAR_DIST * _hearingDistCoef;

	public float WaitInCoverBetweenShotsSecCurrent => BotSettingsComponents_0.Core.WaitInCoverBetweenShotsSec * _waitInCoverCoef;

	public GClass615(BotSettingsComponents mainSettings, ScatteringSettingsClass scatteringSettingSettings)
	{
		ScatteringSettingsClass = scatteringSettingSettings;
		BotSettingsComponents_0_1 = mainSettings;
	}

	public void Apply(BotLastBlindEffectModifierClass modif, float duractionSec = -1f)
	{
		if (modif.IsApplyed)
		{
			return;
		}
		modif.IsApplyed = true;
		_precicingSpeedCoef *= modif.PrecicingSpeedCoef;
		_accuratySpeedCoef *= modif.AccuratySpeedCoef;
		_layChanceDangerCoef *= modif.LayChanceDangerCoef;
		_visibleDistCoef *= modif.VisibleDistCoef;
		_runtimeVisionEffectK *= modif.RuntimeVisionEffectK;
		_scatteringCoef *= modif.ScatteringCoef;
		_priorityScatteringCoef *= modif.PriorityScatteringCoef;
		_hearingDistCoef *= modif.HearingDistCoef;
		_triggerDownDelay *= modif.TriggerDownDelay;
		_waitInCoverCoef *= modif.WaitInCoverCoef;
		if (duractionSec > 0f)
		{
			StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(duractionSec)).OnTimer += delegate
			{
				Dismiss(modif);
			};
		}
	}

	public void Dismiss(BotLastBlindEffectModifierClass modif)
	{
		if (modif.IsApplyed)
		{
			modif.IsApplyed = false;
			_precicingSpeedCoef /= modif.PrecicingSpeedCoef;
			_accuratySpeedCoef /= modif.AccuratySpeedCoef;
			_layChanceDangerCoef /= modif.LayChanceDangerCoef;
			_visibleDistCoef /= modif.VisibleDistCoef;
			_runtimeVisionEffectK /= modif.RuntimeVisionEffectK;
			_priorityScatteringCoef /= modif.PriorityScatteringCoef;
			_scatteringCoef /= modif.ScatteringCoef;
			_hearingDistCoef /= modif.HearingDistCoef;
			_triggerDownDelay /= modif.TriggerDownDelay;
			_waitInCoverCoef /= modif.WaitInCoverCoef;
		}
	}

	public bool IsParametersChanged()
	{
		float num = _precicingSpeedCoef * _accuratySpeedCoef * _layChanceDangerCoef * _visibleDistCoef * _runtimeVisionEffectK * _scatteringCoef * _priorityScatteringCoef * _hearingDistCoef * _triggerDownDelay * _waitInCoverCoef;
		if (!(num > 1.01f))
		{
			return num < 0.99f;
		}
		return true;
	}
}
