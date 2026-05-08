using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Systems.Effects;
using Comfort.Common;
using DG.Tweening;
using EFT;
using EFT.Ballistics;
using EFT.CameraControl;
using EFT.Character.Data;
using EFT.EnvironmentEffect;
using EFT.HealthSystem;
using UnityEngine;
using UnityEngine.Audio;
using UnityStandardAssets.ImageEffects;

public class EffectsController : MonoBehaviour
{
	public abstract class Class633
	{
		public List<IEffect> ActiveEffects = new List<IEffect>();

		public Type[] ValidTypes;

		public float MaxEffectValue;

		public bool Enabled;

		[NonSerialized]
		public EffectsController EffectsController_0;

		public virtual void AddEffect(IEffect effect)
		{
			if (ValidTypes.Contains(effect.Type))
			{
				ActiveEffects.Add(effect);
				Toggle(value: true);
			}
		}

		public virtual void DeleteEffect(IEffect effect)
		{
			if (ActiveEffects.Remove(effect) && ActiveEffects.Count == 0)
			{
				UpdateAmount();
				Toggle(value: false);
			}
		}

		public virtual void StimulatorBuff(IPlayerBuff buff)
		{
		}

		public virtual float vmethod_0()
		{
			float num = 0f;
			foreach (IEffect activeEffect in ActiveEffects)
			{
				num += activeEffect.CurrentStrength;
			}
			return num;
		}

		public virtual void Toggle(bool value)
		{
			Enabled = value;
		}

		public abstract void UpdateAmount();

		public Class633()
		{
		}
	}

	public interface Interface4
	{
		void ForceMaxValue(IEffect effect);
	}

	public class Class634 : Class633
	{
		[NonSerialized]
		public CC_FastVignette Cc_FastVignette_0;

		public Class634(EffectsController effectsController, CC_FastVignette cameraEffect, float maxEffectValue, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			Cc_FastVignette_0 = cameraEffect;
			ValidTypes = types;
		}

		public override void UpdateAmount()
		{
			float value = vmethod_0();
			Cc_FastVignette_0.darkness = Mathf.Clamp01(value) * MaxEffectValue * EffectsController_0.FastVineteFlicker.Evaluate(Time.realtimeSinceStartup);
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			Cc_FastVignette_0.enabled = value;
		}
	}

	public class Class635 : Class633
	{
		[NonSerialized]
		public CC_DoubleVision Cc_DoubleVision_0;

		public Class635(EffectsController effectsController, CC_DoubleVision cameraEffect, float maxEffectValue, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			Cc_DoubleVision_0 = cameraEffect;
			ValidTypes = types;
		}

		public override void UpdateAmount()
		{
			float value = vmethod_0();
			Cc_DoubleVision_0.amount = Mathf.Clamp01(value) * MaxEffectValue;
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			Cc_DoubleVision_0.enabled = value;
		}
	}

	public class Class636 : Class633
	{
		[NonSerialized]
		public CC_HueFocus Cc_HueFocus_0;

		public Class636(EffectsController effectsController, CC_HueFocus cameraEffect, float maxEffectValue, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			Cc_HueFocus_0 = cameraEffect;
			ValidTypes = types;
		}

		public override void UpdateAmount()
		{
			float value = vmethod_0();
			Cc_HueFocus_0.amount = Mathf.Clamp01(value) * MaxEffectValue;
		}

		public override void Toggle(bool value)
		{
			Cc_HueFocus_0.enabled = value;
		}
	}

	public class Class637 : Class633
	{
		[NonSerialized]
		public CC_RadialBlur Cc_RadialBlur_0;

		[NonSerialized]
		public float Float_0 = 1f;

		public Class637(EffectsController effectsController, CC_RadialBlur cameraEffect, float maxEffectValue, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			Cc_RadialBlur_0 = cameraEffect;
			ValidTypes = types;
		}

		public override void UpdateAmount()
		{
			bool flag = false;
			foreach (IEffect activeEffect in ActiveEffects)
			{
				if ((!(activeEffect is GInterface358) && !(activeEffect is GInterface372)) || activeEffect.State == EEffectState.Residued)
				{
					if (activeEffect is GInterface357 && activeEffect.State != EEffectState.Residued)
					{
						flag = true;
					}
					continue;
				}
				flag = false;
				break;
			}
			float value = vmethod_0();
			value = Mathf.Clamp01(value);
			value = ((!flag) ? Mathf.Lerp(Cc_RadialBlur_0.amount / MaxEffectValue, value, 0.15f) : (value + (Mathf.Cos(Time.time * 7f) + Mathf.Sin(Time.time * 3f)) * 0.2f));
			Cc_RadialBlur_0.amount = value * MaxEffectValue * Float_0;
		}

		public override float vmethod_0()
		{
			foreach (IEffect activeEffect in ActiveEffects)
			{
				if ((activeEffect is GInterface358 || activeEffect is GInterface372) && activeEffect.State != EEffectState.Residued)
				{
					return 0f;
				}
			}
			return base.vmethod_0();
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			Cc_RadialBlur_0.enabled = value;
			Float_0 = 1f - (float)EffectsController_0.player_0.Skills.StressPain;
		}
	}

	public class Class638 : Class633
	{
		[NonSerialized]
		public CC_Sharpen Cc_Sharpen_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public bool Bool_0;

		public Class638(EffectsController effectsController, CC_Sharpen cameraEffect, float maxEffectValue, float durationTime, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			Cc_Sharpen_0 = cameraEffect;
			ValidTypes = types;
			UpdateAmount();
		}

		public override void UpdateAmount()
		{
			float value = vmethod_0();
			Cc_Sharpen_0.strength = Float_0 + Mathf.Clamp(value, 0f, 2f) * (MaxEffectValue - Float_0);
			Cc_Sharpen_0.enabled = (double)Cc_Sharpen_0.strength >= 0.1 || (Cc_Sharpen_0.strength == 0f && Cc_Sharpen_0.DoDesaturate);
		}

		public void ChangeDefaultSharpenValue(float defaultValue)
		{
			Float_0 = defaultValue;
			UpdateAmount();
		}
	}

	public class Class639 : Class633
	{
		[NonSerialized]
		public const float Float_0 = 0.325f;

		[NonSerialized]
		public const float Float_1 = 1f;

		[NonSerialized]
		public FrostbiteEffect FrostbiteEffect_0;

		[NonSerialized]
		public bool Bool_0;

		public Class639(EffectsController controller, FrostbiteEffect effect, params Type[] types)
		{
			ValidTypes = types;
			EffectsController_0 = controller;
			FrostbiteEffect_0 = effect;
			if (FrostbiteEffect_0 != null)
			{
				FrostbiteEffect_0.enabled = false;
				FrostbiteEffect_0.Radius = 1f;
			}
			Toggle(value: false);
		}

		public override void AddEffect(IEffect effect)
		{
			if (ValidTypes.Contains(effect.Type))
			{
				ActiveEffects.Add(effect);
				Toggle(value: true);
				FrostbiteEffect_0.enabled = true;
				method_0();
			}
		}

		public override void DeleteEffect(IEffect effect)
		{
			int num = ActiveEffects.IndexOf(effect);
			if (num != -1)
			{
				ActiveEffects.RemoveAt(num);
				Bool_0 = true;
				method_0();
			}
		}

		public void method_0()
		{
			if (!(FrostbiteEffect_0 == null))
			{
				MaxEffectValue = ((ActiveEffects.Count > 0) ? 0.325f : 1f);
			}
		}

		public override void UpdateAmount()
		{
			if (!(FrostbiteEffect_0 == null))
			{
				float maxDelta = FrostbiteEffect_0.Speed * Time.deltaTime;
				FrostbiteEffect_0.Radius = Mathf.MoveTowards(FrostbiteEffect_0.Radius, MaxEffectValue, maxDelta);
				if (Bool_0 && FrostbiteEffect_0.Radius >= 1f)
				{
					Bool_0 = false;
					FrostbiteEffect_0.Radius = 1f;
					Toggle(value: false);
					FrostbiteEffect_0.enabled = false;
				}
			}
		}
	}

	public class Class640 : Class633
	{
		[NonSerialized]
		public const float Float_0 = 1f;

		[NonSerialized]
		public const float Float_1 = 0f;

		[NonSerialized]
		public CC_Sharpen Cc_Sharpen_0;

		[NonSerialized]
		public float Float_2 = 1f;

		[NonSerialized]
		public float Float_3;

		[NonSerialized]
		public float Float_4;

		[NonSerialized]
		public bool Bool_0;

		public Class640(EffectsController effectsController, CC_Sharpen cameraEffect, params Type[] types)
		{
			ValidTypes = types;
			EffectsController_0 = effectsController;
			Cc_Sharpen_0 = cameraEffect;
			method_0();
		}

		public override void AddEffect(IEffect effect)
		{
			if (ValidTypes.Contains(effect.Type))
			{
				ActiveEffects.Add(effect);
				Toggle(value: true);
				method_0();
			}
		}

		public override void DeleteEffect(IEffect effect)
		{
			int num = ActiveEffects.IndexOf(effect);
			if (num != -1)
			{
				ActiveEffects.RemoveAt(num);
				Bool_0 = true;
				method_0();
			}
		}

		public void method_0()
		{
			if (!(Cc_Sharpen_0 == null))
			{
				MaxEffectValue = ((ActiveEffects.Count > 0) ? 1f : 0f);
				bool flag = !Cc_Sharpen_0.FailedToGetDesaturate && (bool)Cc_Sharpen_0.DesaturateEffectSettingsProvider;
				Float_3 = (flag ? Cc_Sharpen_0.DesaturateEffectSettingsProvider.MinMaxRadius.x : Cc_Sharpen_0.MinMaxRadius.x);
				Float_4 = (flag ? Cc_Sharpen_0.DesaturateEffectSettingsProvider.MinMaxRadius.y : Cc_Sharpen_0.MinMaxRadius.y);
				Float_2 = ((ActiveEffects.Count > 0) ? Float_3 : Float_4);
			}
		}

		public override void UpdateAmount()
		{
			if (Cc_Sharpen_0 == null)
			{
				return;
			}
			bool flag;
			float num = ((flag = !Cc_Sharpen_0.FailedToGetDesaturate && (bool)Cc_Sharpen_0.DesaturateEffectSettingsProvider) ? Cc_Sharpen_0.DesaturateEffectSettingsProvider.Radius : Cc_Sharpen_0.Radius);
			float num2 = (flag ? Cc_Sharpen_0.DesaturateEffectSettingsProvider.MaskDesaturate : Cc_Sharpen_0.MaskDesaturate);
			method_2(flag);
			if (!Bool_0)
			{
				method_1(flag);
			}
			else
			{
				if (!(num >= Float_4))
				{
					return;
				}
				method_1(flag);
				if (num2 <= 0f)
				{
					if (flag)
					{
						Cc_Sharpen_0.DesaturateEffectSettingsProvider.Radius = Float_4;
					}
					Cc_Sharpen_0.Radius = Float_4;
					Bool_0 = false;
					Toggle(value: false);
				}
			}
		}

		public void method_1(bool desaturateProvider)
		{
			if (desaturateProvider)
			{
				Cc_Sharpen_0.DesaturateEffectSettingsProvider.MaskDesaturate = Mathf.MoveTowards(Cc_Sharpen_0.DesaturateEffectSettingsProvider.MaskDesaturate, MaxEffectValue, Time.deltaTime * 0.5f);
			}
			Cc_Sharpen_0.MaskDesaturate = Mathf.MoveTowards(Cc_Sharpen_0.MaskDesaturate, MaxEffectValue, Time.deltaTime * 0.5f);
		}

		public void method_2(bool desaturateProvider)
		{
			if (desaturateProvider)
			{
				Cc_Sharpen_0.DesaturateEffectSettingsProvider.Radius = Mathf.MoveTowards(Cc_Sharpen_0.DesaturateEffectSettingsProvider.Radius, Float_2, Time.deltaTime * 0.2f);
			}
			Cc_Sharpen_0.Radius = Mathf.MoveTowards(Cc_Sharpen_0.Radius, Float_2, Time.deltaTime * 0.2f);
		}
	}

	public class Class641 : Class633
	{
		[Serializable]
		[CompilerGenerated]
		public class Class648
		{
			public static readonly Class648 class648_0 = new Class648();

			public static Func<IEffect, float> func_0;

			public float method_0(IEffect x)
			{
				return Dictionary_0[x.Type];
			}
		}

		[NonSerialized]
		public static Dictionary<Type, float> Dictionary_0 = new Dictionary<Type, float>
		{
			{
				typeof(GInterface376),
				-0.15f
			},
			{
				typeof(GInterface344),
				0.15f
			},
			{
				typeof(GInterface357),
				0.3f
			}
		};

		[NonSerialized]
		public const float Float_0 = 0f;

		[NonSerialized]
		public CC_Sharpen Cc_Sharpen_0;

		public static bool smethod_0(IEffect effect)
		{
			if (!(effect is GInterface376 gInterface) || !(gInterface.MedItem is MedKitItemClass))
			{
				return effect is GInterface344;
			}
			return true;
		}

		public Class641(EffectsController effectsController, CC_Sharpen cameraEffect)
		{
			EffectsController_0 = effectsController;
			Cc_Sharpen_0 = cameraEffect;
			method_0();
			Enabled = true;
		}

		public override void AddEffect(IEffect effect)
		{
			if (smethod_0(effect))
			{
				ActiveEffects.Add(effect);
				method_0();
			}
		}

		public override void DeleteEffect(IEffect effect)
		{
			int num = ActiveEffects.IndexOf(effect);
			if (num != -1)
			{
				ActiveEffects.RemoveAt(num);
				method_0();
			}
		}

		public void method_0()
		{
			MaxEffectValue = ((ActiveEffects.Count > 0) ? ActiveEffects.Max((IEffect x) => Dictionary_0[x.Type]) : 0f);
		}

		public override void UpdateAmount()
		{
			method_1();
		}

		public void method_1()
		{
			if (Cc_Sharpen_0.FailedToGetDesaturate)
			{
				Cc_Sharpen_0.HealthDesaturate = Mathf.MoveTowards(Cc_Sharpen_0.HealthDesaturate, MaxEffectValue, Time.deltaTime * 0.1f);
			}
			else if ((bool)Cc_Sharpen_0.DesaturateEffectSettingsProvider)
			{
				Cc_Sharpen_0.DesaturateEffectSettingsProvider.HealthDesaturate = Mathf.MoveTowards(Cc_Sharpen_0.DesaturateEffectSettingsProvider.HealthDesaturate, MaxEffectValue, Time.deltaTime * 0.1f);
			}
		}
	}

	public class Class642 : Class633
	{
		[NonSerialized]
		public CC_Blend Cc_Blend_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public float Float_1;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public float Float_2;

		public Class642(EffectsController effectsController, CC_Blend cameraEffect, float maxEffectValue, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			Cc_Blend_0 = cameraEffect;
			ValidTypes = types;
		}

		public override void AddEffect(IEffect effect)
		{
			if (ValidTypes.Contains(effect.Type))
			{
				if (ActiveEffects.Count == 0)
				{
					Float_0 = Time.time;
					Float_1 = Time.time + 10f;
				}
				ActiveEffects.Add(effect);
				Toggle(value: true);
				Bool_0 = false;
			}
		}

		public override void DeleteEffect(IEffect effect)
		{
			int num = ActiveEffects.IndexOf(effect);
			if (num != -1)
			{
				ActiveEffects.RemoveAt(num);
				if (ActiveEffects.Count == 0)
				{
					Float_0 = Time.time;
					Float_1 = Time.time + 10f;
					Bool_0 = true;
				}
			}
		}

		public override void UpdateAmount()
		{
			Float_2 = (Bool_0 ? Mathf.InverseLerp(Float_1, Float_0, Time.time) : Mathf.InverseLerp(Float_0, Float_1, Time.time));
			Cc_Blend_0.amount = Float_2 * MaxEffectValue;
			if (Float_2 <= 0f && ActiveEffects.Count < 1)
			{
				Toggle(value: false);
			}
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			Cc_Blend_0.enabled = value;
			Cc_Blend_0.amount = 0f;
		}
	}

	public class Class643 : Class633
	{
		[NonSerialized]
		public GrenadeFlashScreenEffect GrenadeFlashScreenEffect_0;

		[NonSerialized]
		public List<IEffect> List_0 = new List<IEffect>();

		public Class643(EffectsController effectsController, GrenadeFlashScreenEffect screenScreenEffect, params Type[] types)
		{
			ValidTypes = types;
			EffectsController_0 = effectsController;
			GrenadeFlashScreenEffect_0 = screenScreenEffect;
		}

		public override void AddEffect(IEffect effect)
		{
			if (ValidTypes.Contains(effect.Type))
			{
				ActiveEffects.Add(effect);
				Toggle(value: true);
				if (EnvironmentManager.Instance.Environment == EnvironmentType.Indoor)
				{
					List_0.Add(effect);
				}
				GrenadeFlashScreenEffect_0.Explode(effect.Strength);
			}
		}

		public override void DeleteEffect(IEffect effect)
		{
			int num = ActiveEffects.IndexOf(effect);
			if (num != -1)
			{
				ActiveEffects.RemoveAt(num);
				if (ActiveEffects.Count == 0)
				{
					Toggle(value: false);
				}
				List_0.Remove(effect);
			}
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			GrenadeFlashScreenEffect_0.enabled = value;
		}

		public override void UpdateAmount()
		{
			GrenadeFlashScreenEffect_0.EffectStrength = (float)Math.Sqrt(ActiveEffects.Max((IEffect e) => (!List_0.Contains(e)) ? e.CurrentStrength : Mathf.Clamp01(e.CurrentStrength * 1.25f)));
		}

		[CompilerGenerated]
		public float method_0(IEffect e)
		{
			if (!List_0.Contains(e))
			{
				return e.CurrentStrength;
			}
			return Mathf.Clamp01(e.CurrentStrength * 1.25f);
		}
	}

	public class Class644 : Class633
	{
		[NonSerialized]
		public float Float_0 = 17f;

		[NonSerialized]
		public float Float_1 = 5f;

		[NonSerialized]
		public float Float_2;

		public override void UpdateAmount()
		{
			float num = vmethod_0();
			CameraClass.Instance.Camera.fieldOfView = CameraClass.Instance.Fov + num * num * num + (Mathf.Cos(Time.time * Float_0) + Mathf.Cos(Time.time * Float_1)) * (Mathf.Clamp01(num - 1.5f) / 3f);
		}

		public Class644(EffectsController effectsController, params Type[] types)
		{
			EffectsController_0 = effectsController;
			ValidTypes = types;
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			if (!value)
			{
				CameraClass.Instance.Camera.fieldOfView = CameraClass.Instance.Fov;
			}
		}
	}

	public class Class645 : Class633
	{
		[Serializable]
		[CompilerGenerated]
		public class Class649
		{
			public static readonly Class649 class649_0 = new Class649();

			public static Func<IEffect, bool> func_0;

			public static Func<IPlayerBuff, float> func_1;

			public static Func<IPlayerBuff, float> func_2;

			public static Func<IPlayerBuff, float> func_3;

			public bool method_0(IEffect e)
			{
				return e is GInterface350;
			}

			public float method_1(IPlayerBuff b)
			{
				return b.Value;
			}

			public float method_2(IPlayerBuff b)
			{
				return b.Value;
			}

			public float method_3(IPlayerBuff buff)
			{
				return buff.Value;
			}
		}

		[NonSerialized]
		public CC_Wiggle Cc_Wiggle_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public float Float_1;

		[NonSerialized]
		public List<IPlayerBuff> List_0;

		[NonSerialized]
		public Dictionary<Type, float> Dictionary_0 = new Dictionary<Type, float>
		{
			{
				typeof(GInterface352),
				0.8f
			},
			{
				typeof(GInterface356),
				0.4f
			},
			{
				typeof(GInterface350),
				4f
			}
		};

		[NonSerialized]
		public Dictionary<Type, float> Dictionary_1 = new Dictionary<Type, float>
		{
			{
				typeof(GInterface352),
				8f
			},
			{
				typeof(GInterface356),
				5f
			},
			{
				typeof(GInterface350),
				1.5f
			}
		};

		public Class645(EffectsController effectsController, CC_Wiggle cameraEffect, float maxEffectValue, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			Cc_Wiggle_0 = cameraEffect;
			ValidTypes = types;
			List_0 = new List<IPlayerBuff>();
		}

		public override void AddEffect(IEffect effect)
		{
			if (ValidTypes.Contains(effect.Type))
			{
				ActiveEffects.Add(effect);
				Toggle(value: true);
				Float_0 = 0f;
				Float_1 = 0f;
				method_0();
				Cc_Wiggle_0.str = 1f;
			}
		}

		public override void DeleteEffect(IEffect effect)
		{
			int num = ActiveEffects.IndexOf(effect);
			if (num != -1)
			{
				ActiveEffects.RemoveAt(num);
				method_0();
			}
		}

		public override void StimulatorBuff(IPlayerBuff buff)
		{
			if (buff.Settings.BuffType == EStimulatorBuffType.ContusionWiggle)
			{
				if (buff.Active)
				{
					List_0.Add(buff);
				}
				else
				{
					List_0.Remove(buff);
				}
				Toggle(ActiveEffects.Count > 0 || List_0.Count > 0);
				method_0();
			}
		}

		public void method_0()
		{
			Float_0 = 0f;
			Float_1 = 0f;
			if (ActiveEffects.Any((IEffect e) => e is GInterface350))
			{
				Float_0 = Dictionary_0[typeof(GInterface350)];
				Float_1 = Dictionary_1[typeof(GInterface350)];
			}
			else if (ActiveEffects.Count > 0)
			{
				Float_0 = ActiveEffects.Max((IEffect e) => Dictionary_0[e.Type]);
				Float_1 = ActiveEffects.Max((IEffect e) => Dictionary_1[e.Type]);
			}
			if (List_0.Count > 0)
			{
				Float_0 = Mathf.Max(Float_0, List_0.Max((IPlayerBuff b) => b.Value) * Dictionary_0[typeof(GInterface352)]);
				Float_1 = Mathf.Max(Float_1, List_0.Max((IPlayerBuff b) => b.Value) * Dictionary_1[typeof(GInterface352)]);
			}
		}

		public override void UpdateAmount()
		{
			float value = vmethod_0();
			Cc_Wiggle_0.speed = Mathf.Clamp01(value) * Float_0;
			Cc_Wiggle_0.scale = Mathf.Clamp01(value) * Float_1;
			if (ActiveEffects.Count + List_0.Count == 0)
			{
				Cc_Wiggle_0.str = Mathf.Lerp(Cc_Wiggle_0.str, 0f, Time.deltaTime);
				if (Cc_Wiggle_0.str < 0.01f)
				{
					Toggle(value: false);
				}
			}
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			Cc_Wiggle_0.enabled = value;
		}

		public override float vmethod_0()
		{
			return base.vmethod_0() + List_0.Sum((IPlayerBuff buff) => buff.Value);
		}

		[CompilerGenerated]
		public float method_1(IEffect e)
		{
			return Dictionary_0[e.Type];
		}

		[CompilerGenerated]
		public float method_2(IEffect e)
		{
			return Dictionary_1[e.Type];
		}
	}

	public class Class646 : Class633
	{
		[Serializable]
		[CompilerGenerated]
		public class Class650
		{
			public static readonly Class650 class650_0 = new Class650();

			public static Func<IPlayerBuff, float> func_0;

			public static Func<IPlayerBuff, float> func_1;

			public float method_0(IPlayerBuff buff)
			{
				return buff.Value;
			}

			public float method_1(IPlayerBuff b)
			{
				return b.Value;
			}
		}

		[NonSerialized]
		public MotionBlur MotionBlur_0;

		[NonSerialized]
		public Dictionary<Type, float> Dictionary_0 = new Dictionary<Type, float>
		{
			{
				typeof(GInterface352),
				0.8f
			},
			{
				typeof(GInterface356),
				0.3f
			},
			{
				typeof(GInterface350),
				1.4f
			}
		};

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public List<IPlayerBuff> List_0;

		public Class646(EffectsController effectsController, MotionBlur cameraEffect, float maxEffectValue, params Type[] types)
		{
			EffectsController_0 = effectsController;
			MaxEffectValue = maxEffectValue;
			MotionBlur_0 = cameraEffect;
			ValidTypes = types;
			List_0 = new List<IPlayerBuff>();
		}

		public override float vmethod_0()
		{
			return base.vmethod_0() + List_0.Sum((IPlayerBuff buff) => buff.Value);
		}

		public override void UpdateAmount()
		{
			float value = vmethod_0();
			MaxEffectValue = Mathf.Lerp(MaxEffectValue, Float_0, Time.deltaTime);
			MotionBlur_0.blurAmount = Mathf.Clamp01(value) * MaxEffectValue;
		}

		public override void AddEffect(IEffect effect)
		{
			base.AddEffect(effect);
			method_0();
		}

		public override void DeleteEffect(IEffect effect)
		{
			base.DeleteEffect(effect);
			method_0();
		}

		public override void StimulatorBuff(IPlayerBuff buff)
		{
			if (buff.Settings.BuffType == EStimulatorBuffType.ContusionBlur)
			{
				if (buff.Active)
				{
					List_0.Add(buff);
				}
				else
				{
					List_0.Remove(buff);
				}
				Toggle(ActiveEffects.Count > 0 || List_0.Count > 0);
				method_0();
			}
		}

		public void method_0()
		{
			Float_0 = 0f;
			foreach (IEffect activeEffect in ActiveEffects)
			{
				if (Float_0 <= Dictionary_0[activeEffect.Type])
				{
					Float_0 = Dictionary_0[activeEffect.Type];
				}
			}
			if (List_0.Count > 0)
			{
				Float_0 = Mathf.Max(Float_0, List_0.Max((IPlayerBuff b) => b.Value) * Dictionary_0[typeof(GInterface352)]);
			}
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			MotionBlur_0.enabled = value;
		}
	}

	public class Class647 : Class633, Interface4
	{
		[NonSerialized]
		public InfectionEffect InfectionEffect_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public float Float_1;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public float Float_2;

		[NonSerialized]
		public FirstPersonPlayerHearingSettings FirstPersonPlayerHearingSettings_0;

		public Class647(EffectsController effectsController, InfectionEffect cameraEffect, params Type[] types)
		{
			InfectionEffect_0 = cameraEffect;
			EffectsController_0 = effectsController;
			ValidTypes = types;
			GClass3019.ZombieInfectionSettings zombieInfection = Singleton<BackendConfigSettingsClass>.Instance.Health.Effects.ZombieInfection;
			Float_0 = zombieInfection.float_0;
			Float_1 = zombieInfection.HearingDebuffPercentage;
			FirstPersonPlayerHearingSettings_0 = GClass1857.GetAsset<FirstPersonPlayerHearingSettings>(Singleton<IEasyAssets>.Instance, "assets/content/audio/prefabs/character/firstpersonplayerhearingsettings.bundle");
		}

		public override void UpdateAmount()
		{
			if (!Bool_0)
			{
				Float_2 += Time.deltaTime;
				if (Float_2 >= Float_0)
				{
					Float_2 = Float_0;
					Bool_0 = true;
				}
				InfectionEffect_0.EffectValue = Float_2 / Float_0;
			}
		}

		public override void Toggle(bool value)
		{
			base.Toggle(value);
			InfectionEffect_0.EffectValue = 0f;
			Float_2 = 0f;
			Bool_0 = false;
			if (value)
			{
				SetHearingDebuff(Float_1, Float_0);
			}
			else
			{
				ResetHearingDebuff();
			}
		}

		public void ForceMaxValue(IEffect effect)
		{
			if (ValidTypes.Contains(effect.Type))
			{
				ActiveEffects.Add(effect);
				InfectionEffect_0.EffectValue = 1f;
				Bool_0 = true;
				ResetHearingDebuff();
				SetHearingDebuff(Float_1);
				base.Toggle(value: true);
			}
		}

		public void SetHearingDebuff(float debuffPercentage, float fadeTime = 1f)
		{
			if (GClass3670.TryGetData<GClass1174>(out var dataContainer))
			{
				BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
				if (!(instance == null))
				{
					float value = 1f - debuffPercentage;
					float volumeDB = FirstPersonPlayerHearingSettings_0.GetVolumeDB(value);
					float lowpassFreq = FirstPersonPlayerHearingSettings_0.GetLowpassFreq(value);
					float reverbLevel = FirstPersonPlayerHearingSettings_0.GetReverbLevel(value);
					AudioMixer audioMixer = instance.WorldMixer.audioMixer;
					audioMixer.DOKill();
					audioMixer.DOSetFloat(dataContainer.WorldMixerVolume, volumeDB, fadeTime);
					audioMixer.DOSetFloat(dataContainer.WorldMixerLowpassFreq, lowpassFreq, fadeTime);
					audioMixer.DOSetFloat(dataContainer.WorldMixerReverbLevel, reverbLevel, fadeTime);
				}
			}
		}

		public void ResetHearingDebuff()
		{
			if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
			{
				MonoBehaviourSingleton<BetterAudio>.Instance.ResetWorldMixerValues();
			}
		}
	}

	private const float float_0 = 5.2f;

	private const float float_1 = 15f;

	private const float float_2 = 11.5f;

	[SerializeField]
	private GameObject _effectsPrefab;

	[SerializeField]
	public AnimationCurve FastVineteFlicker;

	[CompilerGenerated]
	private RainScreenDrops rainScreenDrops_0;

	[CompilerGenerated]
	private ScreenWater screenWater_0;

	private CC_FastVignette cc_FastVignette_0;

	private CC_DoubleVision cc_DoubleVision_0;

	private CC_HueFocus cc_HueFocus_0;

	private CC_RadialBlur cc_RadialBlur_0;

	private CC_Sharpen cc_Sharpen_0;

	private CC_Blend cc_Blend_0;

	private CC_Blend cc_Blend_1;

	private CC_Wiggle cc_Wiggle_0;

	private MotionBlur motionBlur_0;

	private BloodOnScreen bloodOnScreen_0;

	private GrenadeFlashScreenEffect grenadeFlashScreenEffect_0;

	private EyeBurn eyeBurn_0;

	private FastBlur fastBlur_0;

	private DepthOfField depthOfField_0;

	private readonly List<Class633> list_0 = new List<Class633>();

	private Class638 class638_0;

	private ChromaticAberration chromaticAberration_0;

	private ThermalVision thermalVision_0;

	private FrostbiteEffect frostbiteEffect_0;

	private float float_3;

	private float float_4;

	private float float_5;

	private float float_6;

	private float float_7;

	private float float_8;

	private float float_9;

	private DeathFade deathFade_0;

	private Player player_0;

	private Action action_0;

	private Action action_1;

	private static readonly MaterialType[] materialType_0 = new MaterialType[3]
	{
		MaterialType.GlassVisor,
		MaterialType.Helmet,
		MaterialType.HelmetRicochet
	};

	public RainScreenDrops RainScreenDrops
	{
		[CompilerGenerated]
		get
		{
			return rainScreenDrops_0;
		}
		[CompilerGenerated]
		set
		{
			rainScreenDrops_0 = value;
		}
	}

	public ScreenWater ScreenWater
	{
		[CompilerGenerated]
		get
		{
			return screenWater_0;
		}
		[CompilerGenerated]
		set
		{
			screenWater_0 = value;
		}
	}

	public void Awake()
	{
		method_3();
		action_0 = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.Sharpen.Bind(method_10);
		PlayerCameraController.OnPlayerCameraControllerCreated += method_0;
		PlayerCameraController.OnPlayerCameraControllerDestroyed += method_2;
		action_1 = GlobalEventHandlerClass.Instance.SubscribeOnEvent(delegate(GClass3546 invokedEvent)
		{
			method_1(invokedEvent);
		});
	}

	public void method_0(PlayerCameraController playerCameraController, Camera camera)
	{
		player_0 = playerCameraController.Player;
		foreach (IEffect allActiveEffect in player_0.HealthController.GetAllActiveEffects())
		{
			foreach (Class633 item in list_0)
			{
				if (item is Interface4 @interface)
				{
					@interface.ForceMaxValue(allActiveEffect);
				}
				else
				{
					item.AddEffect(allActiveEffect);
				}
			}
		}
		player_0.HealthController.EffectStartedEvent += method_11;
		player_0.HealthController.EffectRemovedEvent += method_12;
		player_0.OnDamageReceived += method_7;
		player_0.HealthController.ApplyDamageEvent += method_6;
		player_0.HealthController.DiedEvent += method_9;
		player_0.HealthController.BurnEyesEvent += method_5;
		player_0.HealthController.StimulatorBuffEvent += method_13;
		player_0.OnGlassesChanged += method_4;
	}

	public void method_1(GClass3546 invokedEvent)
	{
		if (invokedEvent.PlayerProfileID == player_0.ProfileId)
		{
			eyeBurn_0.IsPaused = invokedEvent.IsPaused;
			if (eyeBurn_0.IsPaused)
			{
				eyeBurn_0.ClearEyeBurnRT();
			}
		}
	}

	public void method_2()
	{
		player_0 = null;
	}

	public void method_3()
	{
		EffectsCurveData component = _effectsPrefab.GetComponent<EffectsCurveData>();
		if (component != null)
		{
			FastVineteFlicker = component.FastVineteFlicker;
		}
		else
		{
			Debug.LogError("Can't get EffectsController component");
		}
		RainScreenDrops = GetComponent<RainScreenDrops>();
		bloodOnScreen_0 = GetComponent<BloodOnScreen>();
		ScreenWater = GetComponent<ScreenWater>();
		FastVineteFlicker.postWrapMode = WrapMode.Loop;
		fastBlur_0 = GetComponent<FastBlur>();
		deathFade_0 = GetComponent<DeathFade>();
		if (deathFade_0 != null)
		{
			deathFade_0.enabled = false;
		}
		DepthOfField component2 = _effectsPrefab.GetComponent<DepthOfField>();
		depthOfField_0 = base.gameObject.GetComponent<DepthOfField>() ?? GClass6.AddComponentCopy(base.gameObject, component2);
		depthOfField_0.aperture = component2.aperture;
		depthOfField_0.blurSampleCount = component2.blurSampleCount;
		depthOfField_0.blurType = component2.blurType;
		depthOfField_0.focalLength = component2.focalLength;
		depthOfField_0.focalSize = component2.focalSize;
		depthOfField_0.focalTransform = component2.focalTransform;
		depthOfField_0.foregroundOverlap = component2.foregroundOverlap;
		depthOfField_0.highResolution = component2.highResolution;
		depthOfField_0.maxBlurSize = component2.maxBlurSize;
		depthOfField_0.nearBlur = component2.nearBlur;
		CC_FastVignette component3 = _effectsPrefab.GetComponent<CC_FastVignette>();
		cc_FastVignette_0 = base.gameObject.GetComponent<CC_FastVignette>() ?? GClass6.AddComponentCopy(base.gameObject, component3);
		cc_FastVignette_0.sharpness = component3.sharpness;
		cc_FastVignette_0.center = component3.center;
		cc_FastVignette_0.shader = component3.shader;
		float_6 = component3.darkness;
		cc_FastVignette_0.enabled = false;
		CC_DoubleVision component4 = _effectsPrefab.GetComponent<CC_DoubleVision>();
		cc_DoubleVision_0 = base.gameObject.GetComponent<CC_DoubleVision>() ?? GClass6.AddComponentCopy(base.gameObject, component4);
		cc_DoubleVision_0.shader = component4.shader;
		cc_DoubleVision_0.enabled = false;
		cc_DoubleVision_0.displace = component4.displace;
		float_5 = component4.amount;
		CC_HueFocus component5 = _effectsPrefab.GetComponent<CC_HueFocus>();
		cc_HueFocus_0 = base.gameObject.GetComponent<CC_HueFocus>() ?? GClass6.AddComponentCopy(base.gameObject, component5);
		cc_HueFocus_0.hue = component5.hue;
		cc_HueFocus_0.range = component5.range;
		cc_HueFocus_0.boost = component5.boost;
		cc_HueFocus_0.shader = component5.shader;
		cc_HueFocus_0.enabled = false;
		grenadeFlashScreenEffect_0 = base.gameObject.GetComponent<GrenadeFlashScreenEffect>();
		eyeBurn_0 = base.gameObject.GetComponent<EyeBurn>();
		CC_RadialBlur component6 = _effectsPrefab.GetComponent<CC_RadialBlur>();
		cc_RadialBlur_0 = base.gameObject.GetComponent<CC_RadialBlur>() ?? GClass6.AddComponentCopy(base.gameObject, component6);
		cc_RadialBlur_0.shader = component6.shader;
		cc_RadialBlur_0.enabled = false;
		cc_RadialBlur_0.quality = component6.quality;
		cc_RadialBlur_0.center = component6.center;
		float_3 = component6.amount;
		CC_Blend cC_Blend = _effectsPrefab.GetComponents<CC_Blend>()[0];
		cc_Blend_1 = GClass6.AddComponentCopy(base.gameObject, cC_Blend);
		cc_Blend_1.shader = cC_Blend.shader;
		cc_Blend_1.enabled = false;
		float_4 = cC_Blend.amount;
		CC_Blend cC_Blend2 = _effectsPrefab.GetComponents<CC_Blend>()[1];
		cc_Blend_0 = GClass6.AddComponentCopy(base.gameObject, cC_Blend2);
		cc_Blend_0.shader = cC_Blend2.shader;
		cc_Blend_0.enabled = false;
		float_4 = cC_Blend2.amount;
		CC_Wiggle component7 = _effectsPrefab.GetComponent<CC_Wiggle>();
		cc_Wiggle_0 = base.gameObject.GetComponent<CC_Wiggle>() ?? GClass6.AddComponentCopy(base.gameObject, component7);
		cc_Wiggle_0.shader = component7.shader;
		cc_Wiggle_0.enabled = false;
		float_7 = component7.scale;
		MotionBlur component8 = _effectsPrefab.GetComponent<MotionBlur>();
		motionBlur_0 = base.gameObject.GetComponent<MotionBlur>() ?? GClass6.AddComponentCopy(base.gameObject, component8);
		motionBlur_0.shader = component8.shader;
		motionBlur_0.enabled = false;
		float_8 = component8.blurAmount;
		CC_Sharpen component9 = _effectsPrefab.GetComponent<CC_Sharpen>();
		cc_Sharpen_0 = base.gameObject.GetComponent<CC_Sharpen>() ?? GClass6.AddComponentCopy(base.gameObject, component9);
		cc_Sharpen_0.shader = component9.shader;
		cc_Sharpen_0.enabled = false;
		float_9 = component9.strength;
		ChromaticAberration component10 = _effectsPrefab.GetComponent<ChromaticAberration>();
		chromaticAberration_0 = base.gameObject.GetComponent<ChromaticAberration>() ?? GClass6.AddComponentCopy(base.gameObject, component10);
		ThermalVision component11 = _effectsPrefab.GetComponent<ThermalVision>();
		thermalVision_0 = base.gameObject.GetComponent<ThermalVision>() ?? GClass6.AddComponentCopy(base.gameObject, component11);
		frostbiteEffect_0 = base.gameObject.GetComponent<FrostbiteEffect>();
		frostbiteEffect_0.enabled = false;
		class638_0 = new Class638(this, cc_Sharpen_0, float_9, 15f, typeof(GInterface350));
		list_0.Clear();
		list_0.Add(class638_0);
		list_0.Add(new Class641(this, cc_Sharpen_0));
		list_0.Add(new Class640(this, cc_Sharpen_0, typeof(GInterface358)));
		list_0.Add(new Class637(this, cc_RadialBlur_0, float_3, typeof(GInterface343), typeof(GInterface357), typeof(GInterface358)));
		list_0.Add(new Class642(this, cc_Blend_0, float_4, typeof(GInterface356)));
		list_0.Add(new Class634(this, cc_FastVignette_0, float_6, typeof(GInterface344), typeof(GInterface363)));
		list_0.Add(new Class645(this, cc_Wiggle_0, float_7, typeof(GInterface352), typeof(GInterface356)));
		list_0.Add(new Class646(this, motionBlur_0, float_8, typeof(GInterface352), typeof(GInterface356)));
		list_0.Add(new Class635(this, cc_DoubleVision_0, float_5, typeof(GInterface352), typeof(GInterface360)));
		list_0.Add(new Class647(this, base.gameObject.GetComponent<InfectionEffect>(), typeof(GInterface355)));
		list_0.Add(new Class643(this, grenadeFlashScreenEffect_0, typeof(GInterface360)));
		list_0.Add(new Class639(this, frostbiteEffect_0, typeof(GInterface372)));
		base.gameObject.AddComponent<SSAAImpl>();
	}

	public void method_4(VisorsItemClass glasses, bool isActive)
	{
		if ((bool)RainScreenDrops)
		{
			RainScreenDrops.ChangeGlassesState(isActive);
		}
		if ((bool)bloodOnScreen_0)
		{
			bloodOnScreen_0.ChangeGlassesState(isActive);
		}
		if ((bool)ScreenWater)
		{
			ScreenWater.ChangeGlassesState(isActive);
		}
	}

	public void SetDoFFocalDistance(int fov, float minFov, float maxFov)
	{
		depthOfField_0.focalLength = 5.2f - Mathf.InverseLerp(minFov, maxFov, fov) * (minFov / (maxFov - minFov));
	}

	public void method_5(Vector3 position, float str, float time)
	{
		eyeBurn_0.enabled = true;
		eyeBurn_0.Burn(position, str, player_0.MovementContext.PreviousRotation - player_0.MovementContext.Rotation);
	}

	public void method_6(EBodyPart bodyPart, float damage, DamageInfoStruct damageInfo)
	{
		if (GClass3051.IsBleeding(damageInfo.DamageType))
		{
			Vector3 vector = UnityEngine.Random.insideUnitSphere / 3f;
			vector += EFTHardSettings.Instance.BleedingCenters[(int)bodyPart];
			Vector3 direction = new Vector3(vector.x, 0f, vector.z);
			bloodOnScreen_0.HitBleeding(direction, (damageInfo.DamageType & EDamageType.HeavyBleeding) != 0);
		}
	}

	public void method_7(float damage, EBodyPart bodyPart, EDamageType type, float damageReducedByArmor, MaterialType special = MaterialType.None)
	{
		if (!base.enabled || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (materialType_0.Contains(special))
		{
			method_8(special);
		}
		if (GClass3051.IsSelfInflicted(type) || type == EDamageType.Undefined)
		{
			return;
		}
		Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
		Vector3 direction = new Vector3(insideUnitSphere.x, 0f, insideUnitSphere.z);
		direction = base.transform.InverseTransformDirection(direction).normalized;
		float num = damage + damageReducedByArmor;
		float strength = Mathf.Sqrt(num) / 11.5f;
		float hands = 0.05f;
		float camera = 0.4f;
		switch (bodyPart)
		{
		case EBodyPart.Head:
			hands = 0.1f;
			camera = 1.3f;
			break;
		case EBodyPart.LeftArm:
		case EBodyPart.RightArm:
			hands = 0.15f;
			camera = 0.5f;
			break;
		case EBodyPart.LeftLeg:
		case EBodyPart.RightLeg:
			camera = 0.3f;
			break;
		}
		player_0.ProceduralWeaponAnimation.ForceReact.AddForce(strength, hands, camera);
		float power = (player_0.MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) ? num : ((bodyPart == EBodyPart.Head) ? (num * 6f) : (num * 3f)));
		fastBlur_0.enabled = true;
		fastBlur_0.Hit(power);
		if (type != EDamageType.Fall && damage >= 0.1f)
		{
			bloodOnScreen_0.Hit(direction);
			if (bodyPart == EBodyPart.Head)
			{
				insideUnitSphere = UnityEngine.Random.insideUnitSphere / 1.5f;
				insideUnitSphere += EFTHardSettings.Instance.BleedingCenters[(int)bodyPart];
				direction = new Vector3(insideUnitSphere.x, 0f, insideUnitSphere.z);
				bloodOnScreen_0.Hit(direction);
			}
		}
	}

	public void method_8(MaterialType specialMaterialType)
	{
		if (Singleton<Effects>.Instantiated)
		{
			AudioMixerGroup mixerGroup = null;
			if (MonoBehaviourSingleton<BetterAudio>.Instantiated)
			{
				mixerGroup = (GClass2078.IsFirstPerson(player_0.PointOfView) ? MonoBehaviourSingleton<BetterAudio>.Instance.ClientPlayerSpeechMixer : MonoBehaviourSingleton<BetterAudio>.Instance.ObservedPlayerSpeechMixer);
			}
			Singleton<Effects>.Instance.EmitPlayerSoundOnly(specialMaterialType, player_0, 1f, mixerGroup);
		}
	}

	public void method_9(EDamageType damageType)
	{
		if (base.enabled && base.gameObject.activeInHierarchy)
		{
			deathFade_0.enabled = true;
			deathFade_0.EnableEffect();
			fastBlur_0.enabled = true;
			fastBlur_0.Die();
		}
	}

	public void OnDisable()
	{
		foreach (Class633 item in list_0)
		{
			item.ActiveEffects.Clear();
		}
	}

	public void OnDestroy()
	{
		PlayerCameraController.OnPlayerCameraControllerCreated -= method_0;
		PlayerCameraController.OnPlayerCameraControllerDestroyed -= method_2;
		action_0?.Invoke();
		action_0 = null;
		action_1?.Invoke();
		action_1 = null;
		if (!(player_0 == null))
		{
			player_0.HealthController.EffectStartedEvent -= method_11;
			player_0.HealthController.EffectRemovedEvent -= method_12;
			player_0.OnDamageReceived -= method_7;
			player_0.HealthController.ApplyDamageEvent -= method_6;
			player_0.HealthController.DiedEvent -= method_9;
			player_0.HealthController.BurnEyesEvent -= method_5;
			player_0.HealthController.StimulatorBuffEvent -= method_13;
			player_0.OnGlassesChanged -= method_4;
			player_0 = null;
		}
	}

	public void method_10(float value)
	{
		class638_0?.ChangeDefaultSharpenValue(value);
	}

	public void method_11(IEffect effect)
	{
		foreach (Class633 item in list_0)
		{
			item.AddEffect(effect);
		}
	}

	public void method_12(IEffect effect)
	{
		foreach (Class633 item in list_0)
		{
			item.DeleteEffect(effect);
		}
	}

	public void method_13(IPlayerBuff buff)
	{
		foreach (Class633 item in list_0)
		{
			item.StimulatorBuff(buff);
		}
	}

	public void Update()
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			if (list_0[i].Enabled)
			{
				list_0[i].UpdateAmount();
			}
		}
	}

	[CompilerGenerated]
	public void method_14(GClass3546 invokedEvent)
	{
		method_1(invokedEvent);
	}
}
