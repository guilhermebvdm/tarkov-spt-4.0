using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Bsg.GameSettings;
using Comfort.Common;
using EFT.CameraControl;
using EFT.InputSystem;
using EFT.InventoryLogic;
using UnityEngine;

public class ScopeZoomHandler : MonoBehaviour
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct113
	{
		public GameSetting<bool> inverseAxis;

		public float scroll;

		public ControlSettingsClass settings;
	}

	[CompilerGenerated]
	private Action<ESmoothScopeState> action_0;

	[CompilerGenerated]
	private Action<float> action_1;

	private const float float_0 = 0.0001f;

	public Transform ScopeSwitcher;

	public Transform StartPivot;

	public Transform EndPivot;

	[Space]
	public bool InverseRotation;

	[CompilerGenerated]
	private ScopePrefabCache scopePrefabCache_0;

	[CompilerGenerated]
	private ScopeSmoothCameraData scopeSmoothCameraData_0;

	private float float_1;

	private float float_2;

	private float float_3;

	private float float_4;

	private float float_5 = 1f;

	private float float_6;

	private float float_7;

	private bool bool_0;

	private bool bool_1;

	private ESmoothScopeState esmoothScopeState_0;

	private ESmoothScopeState esmoothScopeState_1;

	private IAdjustableOpticData iadjustableOpticData_0;

	private float float_8;

	private SightComponent sightComponent_0;

	private CompositeDisposableClass compositeDisposableClass = new CompositeDisposableClass();

	private KeyGroup keyGroup_0;

	private KeyGroup keyGroup_1;

	public ScopePrefabCache ScopeCache
	{
		[CompilerGenerated]
		get
		{
			return scopePrefabCache_0;
		}
		[CompilerGenerated]
		set
		{
			scopePrefabCache_0 = value;
		}
	}

	public ScopeSmoothCameraData CameraData
	{
		[CompilerGenerated]
		get
		{
			return scopeSmoothCameraData_0;
		}
		[CompilerGenerated]
		set
		{
			scopeSmoothCameraData_0 = value;
		}
	}

	public float FiledOfView => float_1;

	public float BlendFactor => method_13();

	public float Single_0 => iadjustableOpticData_0.MinMaxFov.x;

	public float Single_1 => iadjustableOpticData_0.MinMaxFov.y;

	public float Single_2 => iadjustableOpticData_0.MinMaxFov.z;

	public event Action<ESmoothScopeState> OnSmoothScopeStateChanged
	{
		[CompilerGenerated]
		add
		{
			Action<ESmoothScopeState> action = action_0;
			Action<ESmoothScopeState> action2;
			do
			{
				action2 = action;
				Action<ESmoothScopeState> value2 = (Action<ESmoothScopeState>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<ESmoothScopeState> action = action_0;
			Action<ESmoothScopeState> action2;
			do
			{
				action2 = action;
				Action<ESmoothScopeState> value2 = (Action<ESmoothScopeState>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<float> OnSmoothSensetivityChange
	{
		[CompilerGenerated]
		add
		{
			Action<float> action = action_1;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<float> action = action_1;
			Action<float> action2;
			do
			{
				action2 = action;
				Action<float> value2 = (Action<float>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		ScopeCache = GetComponent<ScopePrefabCache>();
		CameraData = GetComponent<ScopeSmoothCameraData>();
		CameraData.ScopeZoomHandler = this;
		OpticSight[] componentsInChildren = GetComponentsInChildren<OpticSight>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CameraData = CameraData;
		}
	}

	public void Init(SightComponent sightComponent)
	{
		if (sightComponent.AdjustableOpticData.IsAdjustableOptic)
		{
			sightComponent_0 = sightComponent;
			iadjustableOpticData_0 = sightComponent.AdjustableOpticData;
			if (sightComponent_0.ScopeZoomValue == 0f)
			{
				sightComponent_0.ScopeZoomValue = Single_0;
			}
			float_1 = Mathf.Clamp(sightComponent_0.ScopeZoomValue, Single_1, Single_0);
			method_9();
			float_6 = Mathf.Abs(Single_0 - Single_1);
			float_4 = float_1;
			float_2 = float_1;
			esmoothScopeState_1 = esmoothScopeState_0;
			method_11();
		}
	}

	public void OnEnable()
	{
		ControlSettingsClass settings = Singleton<SharedGameSettingsClass>.Instance.Control.Settings;
		compositeDisposableClass.AddDisposable(settings.UserKeyBindings.Bind(method_0));
	}

	public void OnDisable()
	{
		compositeDisposableClass.Dispose();
	}

	public void OnDestroy()
	{
		compositeDisposableClass.Dispose();
	}

	public void method_0(List<KeyGroup> userKeyBindings)
	{
		foreach (KeyGroup userKeyBinding in userKeyBindings)
		{
			if (userKeyBinding.keyName == EGameKey.ScopeZoomIn)
			{
				keyGroup_0 = userKeyBinding;
			}
			if (userKeyBinding.keyName == EGameKey.ScopeZoomOut)
			{
				keyGroup_1 = userKeyBinding;
			}
		}
	}

	public void SetUpdateEnable(bool updateEnable)
	{
		bool_1 = updateEnable;
	}

	public void Update()
	{
		if (bool_1)
		{
			method_4();
			method_1();
			method_10();
		}
	}

	public void UpdateScope()
	{
		method_4();
		method_1();
		method_2();
		method_10();
	}

	public void method_1()
	{
		OpticRetrice opticRetrice = CameraClass.Instance.OpticCameraManager.OpticRetrice;
		if (opticRetrice.CanUpdate)
		{
			float value = Mathf.Clamp((1f - BlendFactor) * 100f, 0f, 100f);
			opticRetrice.Renderer.SetBlendShapeWeight(0, value);
			opticRetrice.UpdateTransform(ScopeCache.CurrentModOpticSight);
		}
	}

	public void method_2()
	{
		float adjustableOpticSensitivity = iadjustableOpticData_0.AdjustableOpticSensitivity;
		float adjustableOpticSensitivityMax = iadjustableOpticData_0.AdjustableOpticSensitivityMax;
		float obj = Mathf.Lerp(adjustableOpticSensitivity, adjustableOpticSensitivityMax, method_12());
		action_1?.Invoke(obj);
	}

	public float method_3()
	{
		Struct113 struct113_ = default(Struct113);
		struct113_.settings = Singleton<SharedGameSettingsClass>.Instance.Control.Settings;
		struct113_.inverseAxis = struct113_.settings.InvertedYAxis;
		struct113_.scroll = Input.mouseScrollDelta.y * (float)struct113_.settings.OpticSensitivity;
		if (smethod_0(keyGroup_0, positive: true, out var value, ref struct113_))
		{
			return value;
		}
		if (smethod_0(keyGroup_1, positive: false, out var value2, ref struct113_))
		{
			return value2;
		}
		return 0f;
	}

	public void method_4()
	{
		float_8 = method_3();
		if (!GClass855.IsZero(float_8))
		{
			method_5();
			float_4 = Mathf.Clamp(float_4, Single_1, Single_0);
			float_5 = Mathf.InverseLerp(Single_1, Single_0, float_4);
			float num = CameraData.FieldOfViewCurve.Evaluate(float_5);
			float_2 = float_6 * num + Single_1;
			float_2 = Mathf.Clamp(float_2, Single_1, Single_0);
			float_7 = float_8;
		}
		bool_0 = Mathf.Abs(float_1 - float_2) < 0.0001f;
		if (!bool_0)
		{
			float maxDelta = Single_2 * Mathf.Abs(float_3) * Time.deltaTime;
			float_1 = Mathf.MoveTowards(float_1, float_2, maxDelta);
			float_1 = Mathf.Clamp(float_1, Single_1, Single_0);
			sightComponent_0.ScopeZoomValue = float_1;
			method_9();
		}
		else
		{
			float_3 = 0f;
		}
	}

	public void method_5()
	{
		if ((!(float_8 > 0f) || float_7 >= 0f) && (!(float_8 < 0f) || !(float_7 > 0f)))
		{
			method_7();
		}
		else if (!bool_0 && esmoothScopeState_0 == ESmoothScopeState.SmoothValue)
		{
			method_6();
		}
		else
		{
			method_7();
		}
	}

	public void method_6()
	{
		float_3 = 0f;
		float_4 = float_1;
	}

	public void method_7()
	{
		float_3 += float_8 * iadjustableOpticData_0.ZoomSensitivity;
		float_4 -= float_3;
	}

	public void method_8()
	{
		float_3 = 0f;
		float_8 = 0f;
		float_7 = 0f;
	}

	public void ForceChangeScopeState()
	{
		if (sightComponent_0 != null && iadjustableOpticData_0 != null)
		{
			float num = Mathf.Abs(float_1 - Single_0);
			float num2 = Mathf.Abs(float_1 - Single_1);
			if (num > num2)
			{
				float_1 = Single_0;
				float_2 = Single_0;
				esmoothScopeState_1 = ESmoothScopeState.Max;
			}
			else
			{
				float_1 = Single_1;
				float_2 = Single_1;
				esmoothScopeState_1 = ESmoothScopeState.Min;
			}
			float_4 = float_1;
			sightComponent_0.ScopeZoomValue = float_1;
			method_11();
			method_9();
		}
	}

	public void method_9()
	{
		ESmoothScopeState num = esmoothScopeState_0;
		if (GClass855.ApproxEquals(float_1, Single_0))
		{
			esmoothScopeState_0 = ESmoothScopeState.Min;
			esmoothScopeState_1 = ESmoothScopeState.Min;
			method_8();
		}
		else if (GClass855.ApproxEquals(float_1, Single_1))
		{
			esmoothScopeState_0 = ESmoothScopeState.Max;
			esmoothScopeState_1 = ESmoothScopeState.Max;
			method_8();
		}
		else
		{
			esmoothScopeState_0 = ESmoothScopeState.SmoothValue;
		}
		if (num != esmoothScopeState_0)
		{
			action_0?.Invoke(esmoothScopeState_0);
		}
	}

	public void method_10()
	{
		if (!bool_0)
		{
			method_11();
		}
	}

	public void method_11()
	{
		float t = Mathf.InverseLerp(Single_0, Single_1, float_1);
		ScopeSwitcher.rotation = method_14(StartPivot.rotation, EndPivot.rotation, t, !InverseRotation);
	}

	public float method_12()
	{
		return Mathf.InverseLerp(Single_0, Single_1, float_1);
	}

	public float method_13()
	{
		float time = method_12();
		return CameraData.ReticleBlendCurve.Evaluate(time);
	}

	public Quaternion method_14(Quaternion p, Quaternion q, float t, bool shortWay)
	{
		float num = Quaternion.Dot(p, q);
		if (shortWay && num < 0f)
		{
			return method_14(method_15(p, -1f), q, t, shortWay: true);
		}
		float num2 = Mathf.Acos(num);
		Quaternion p2 = method_15(p, Mathf.Sin((1f - t) * num2));
		Quaternion q2 = method_15(q, Mathf.Sin(t * num2));
		float scalar = 1f / Mathf.Sin(num2);
		return method_15(method_16(p2, q2), scalar);
	}

	public Quaternion method_15(Quaternion input, float scalar)
	{
		return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
	}

	public Quaternion method_16(Quaternion p, Quaternion q)
	{
		return new Quaternion(p.x + q.x, p.y + q.y, p.z + q.z, p.w + q.w);
	}

	[CompilerGenerated]
	public static bool smethod_0(KeyGroup keyGroup, bool positive, out float value, ref Struct113 struct113_0)
	{
		value = 0f;
		foreach (InputSource variant in keyGroup.variants)
		{
			if (variant.isAxis)
			{
				positive = (struct113_0.inverseAxis ? variant.positiveAxis : (!variant.positiveAxis));
				if (GClass855.IsZero(struct113_0.scroll) || GClass855.Positive(struct113_0.scroll) != positive)
				{
					continue;
				}
				if (smethod_1(variant))
				{
					value = (struct113_0.inverseAxis ? (0f - struct113_0.scroll) : struct113_0.scroll);
					return true;
				}
			}
			if (variant.keyCode.Count > 0 && smethod_1(variant))
			{
				value = (positive ? (0f - (float)struct113_0.settings.OpticSensitivity) : ((float)struct113_0.settings.OpticSensitivity));
				return true;
			}
		}
		return false;
	}

	[CompilerGenerated]
	public static bool smethod_1(InputSource variant)
	{
		foreach (KeyCode item in variant.keyCode)
		{
			if (!Input.GetKey(item))
			{
				return false;
			}
		}
		return true;
	}
}
