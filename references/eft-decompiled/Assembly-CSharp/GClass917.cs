using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass917 : IDisposable
{
	public enum EMode
	{
		Disabled,
		Hidden,
		Visible,
		Auto
	}

	public class Class560 : IDisposable
	{
		[NonSerialized]
		public const float Float_0 = 5f;

		[NonSerialized]
		public float Float_1;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		public bool Bool_1;

		[NonSerialized]
		public float Float_2;

		[NonSerialized]
		public bool Bool_2;

		[NonSerialized]
		public GClass999 Gclass999_0;

		public Action<bool> OnChange;

		public bool State => Bool_2;

		public Class560(GClass999 cullingObject, float cullingDistanceSqrt)
		{
			Gclass999_0 = cullingObject;
			Float_1 = cullingDistanceSqrt;
			Gclass999_0.OnVisibilityChanged += method_1;
		}

		public void ManualUpdate()
		{
			method_0();
			bool flag;
			if ((flag = Bool_0 || Bool_1) != Bool_2 && ((!flag && Time.time - Float_2 > 5f) || flag))
			{
				Bool_2 = flag;
				Float_2 = Time.time;
				OnChange?.Invoke(Bool_2);
			}
		}

		public void method_0()
		{
			if (Singleton<GameWorld>.Instantiated)
			{
				ISharedBallisticsCalculator sharedBallisticsCalculator = Singleton<GameWorld>.Instance.SharedBallisticsCalculator;
				if (sharedBallisticsCalculator != null)
				{
					Bool_1 = method_2(sharedBallisticsCalculator);
				}
			}
		}

		public void method_1(bool isVisible)
		{
			Bool_0 = isVisible;
		}

		public bool method_2(ISharedBallisticsCalculator ballisticsCalculator)
		{
			bool result = false;
			for (int i = 0; i < ballisticsCalculator.ActiveShotsCount; i++)
			{
				EftBulletClass activeShot = ballisticsCalculator.GetActiveShot(i);
				if (!((Gclass999_0.Position - activeShot.CurrentPosition).sqrMagnitude > Float_1) && (result = Mathf.Abs(smethod_0(activeShot.CurrentPosition, Gclass999_0.Position, activeShot.CurrentDirection)) < EFTHardSettings.Instance.CULLING_PLAYER_BALLISTICS_ANGLE))
				{
					break;
				}
			}
			return result;
		}

		public void Dispose()
		{
			Gclass999_0.OnVisibilityChanged -= method_1;
		}

		public static float smethod_0(Vector3 fromPoint, Vector3 toPoint, Vector3 zeroDirection)
		{
			Vector3 to = toPoint - fromPoint;
			float num = Mathf.Sign(zeroDirection.x * to.y - zeroDirection.y * to.x);
			return Vector3.Angle(zeroDirection, to) * num;
		}
	}

	[NonSerialized]
	public GClass999 Gclass999_0;

	[NonSerialized]
	public Class560 Class560_0;

	[NonSerialized]
	public List<Renderer> List_0 = new List<Renderer>(256);

	[NonSerialized]
	public EMode Emode_0 = EMode.Auto;

	public bool IsVisible
	{
		get
		{
			if (Emode_0 != EMode.Disabled && Emode_0 != EMode.Visible)
			{
				if (Emode_0 == EMode.Auto)
				{
					return Class560_0.State;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsCloseToMyPlayerCamera => Gclass999_0.SqrCameraDistance < Gclass999_0.CullDistanceSqr;

	public EMode Mode => Emode_0;

	public virtual bool SetMode(EMode mode)
	{
		if (Emode_0 == mode)
		{
			return false;
		}
		Emode_0 = mode;
		ApplyVisibleState();
		return true;
	}

	public void method_0(PlayerBones playerBones)
	{
		Gclass999_0 = new GClass999(playerBones.BodyTransform.Original, EFTHardSettings.Instance.CULLING_PLAYER_SPHERE_RADIUS, EFTHardSettings.Instance.CULLING_PLAYER_DISTANCE);
		Class560_0 = new Class560(Gclass999_0, EFTHardSettings.Instance.CULLING_PLAYER_DISTANCE * EFTHardSettings.Instance.CULLING_PLAYER_DISTANCE);
		Class560 class560_ = Class560_0;
		class560_.OnChange = (Action<bool>)Delegate.Combine(class560_.OnChange, new Action<bool>(method_1));
		Gclass999_0.Register();
		ApplyVisibleState();
	}

	public void method_1(bool state)
	{
		ApplyVisibleState();
	}

	public virtual void ApplyVisibleState()
	{
	}

	public void ManualUpdate(float dt)
	{
		if (Emode_0 != EMode.Disabled)
		{
			Class560_0.ManualUpdate();
		}
	}

	public void Enable()
	{
		SetMode(EMode.Auto);
	}

	public void Hide()
	{
		SetMode(EMode.Hidden);
	}

	public void Disable()
	{
		SetMode(EMode.Disabled);
	}

	public void DisableCullingOnDead()
	{
		Disable();
		if (Gclass999_0 != null)
		{
			Gclass999_0.Dispose();
		}
	}

	public virtual void Dispose()
	{
		if (Gclass999_0 != null)
		{
			Emode_0 = EMode.Disabled;
			Class560 class560_ = Class560_0;
			class560_.OnChange = (Action<bool>)Delegate.Remove(class560_.OnChange, new Action<bool>(method_1));
			Class560_0.Dispose();
			Gclass999_0.Dispose();
			Gclass999_0 = null;
			List_0.Clear();
		}
	}
}
