using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[ExecuteAlways]
[DisallowMultipleComponent]
public class ScreenDistanceSwitcher : MonoBehaviour
{
	[SerializeField]
	private bool _enableSwitch;

	[SerializeField]
	private LODGroup _controlLodGroup;

	[NonSerialized]
	private float float_0;

	[NonSerialized]
	private Vector3 vector3_0;

	[NonSerialized]
	private float float_1;

	[SerializeField]
	private int _numRenderersSwitch = 10;

	[NonSerialized]
	private volatile bool bool_0;

	[NonSerialized]
	public volatile bool IsAutocullVisible;

	[NonSerialized]
	public volatile bool IsBakedAutocullVisible;

	private GInterface116[] ginterface116_0;

	private Renderer[] renderer_0;

	internal static readonly List<ScreenDistanceSwitcher> list_0 = new List<ScreenDistanceSwitcher>();

	[CompilerGenerated]
	private static bool bool_1;

	[HideInInspector]
	[SerializeField]
	private bool _firstSetupWasPerformed;

	public float Size
	{
		get
		{
			if (!(_controlLodGroup == null))
			{
				return GClass1244.GetWorldSpaceSize(_controlLodGroup);
			}
			return 1f;
		}
	}

	public float RelativeScreenSwitchHeight
	{
		get
		{
			if (_controlLodGroup == null)
			{
				return 0f;
			}
			LOD[] lODs = _controlLodGroup.GetLODs();
			if (lODs != null && lODs.Length != 0)
			{
				LOD obj = ((lODs.Length == 1) ? lODs[0] : lODs[^2]);
				return obj.screenRelativeTransitionHeight;
			}
			return 0f;
		}
	}

	public float RuntimeSize => float_0;

	public Vector3 RuntimeCenter => vector3_0;

	public float RuntimeRelativeScreenSwitchHeight => float_1;

	public int NumRenderersSwitch => _numRenderersSwitch;

	public bool IsVisible => bool_0;

	public static bool Boolean_0
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public static bool Boolean_1
	{
		get
		{
			if (Application.isPlaying && PerfectCullingCrossSceneSampler.Exists)
			{
				return PerfectCullingCrossSceneSampler.Instance.IsUpdatingVisibility;
			}
			return false;
		}
	}

	public Vector3 CenterPoint
	{
		get
		{
			if (_controlLodGroup == null)
			{
				return base.transform.position;
			}
			return _controlLodGroup.transform.TransformPoint(_controlLodGroup.localReferencePoint);
		}
	}

	public void Awake()
	{
		method_3();
	}

	public void OnDestroy()
	{
		method_4();
	}

	public void Update()
	{
		if (Application.isPlaying && _enableSwitch)
		{
			method_5();
		}
	}

	public void method_0()
	{
		vector3_0 = CenterPoint;
		float_0 = Size;
		float_1 = RelativeScreenSwitchHeight;
	}

	public void method_1(bool flag)
	{
		if (flag)
		{
			_enableSwitch = true;
			if (_controlLodGroup != null)
			{
				_controlLodGroup.gameObject.SetActive(value: true);
			}
			return;
		}
		_enableSwitch = false;
		Show(flag: false);
		if (_controlLodGroup != null)
		{
			_controlLodGroup.gameObject.SetActive(value: false);
		}
	}

	public MeshRenderer[] GetBakedLodRenderers()
	{
		if (_controlLodGroup == null)
		{
			return Array.Empty<MeshRenderer>();
		}
		return _controlLodGroup.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
	}

	public void method_2()
	{
		if (!_firstSetupWasPerformed && _controlLodGroup != null)
		{
			renderer_0 = _controlLodGroup.GetComponentsInChildren<Renderer>(includeInactive: true);
			if (_controlLodGroup != null)
			{
				_controlLodGroup.enabled = false;
			}
		}
		if (_firstSetupWasPerformed && _controlLodGroup == null)
		{
			_firstSetupWasPerformed = false;
		}
	}

	public void method_3()
	{
		IsBakedAutocullVisible = true;
		IsAutocullVisible = false;
		list_0.Add(this);
		ginterface116_0 = GetComponents<GInterface116>();
		if (_controlLodGroup != null)
		{
			renderer_0 = _controlLodGroup.GetComponentsInChildren<Renderer>(includeInactive: true);
			_controlLodGroup.enabled = false;
		}
		method_0();
		Show(flag: false);
	}

	public void method_4()
	{
		list_0.Remove(this);
		ginterface116_0 = null;
	}

	public void Show(bool flag)
	{
		bool_0 = flag;
		if (renderer_0 != null)
		{
			Renderer[] array = renderer_0;
			foreach (Renderer renderer in array)
			{
				if (renderer != null)
				{
					renderer.enabled = flag;
				}
			}
		}
		if (!Boolean_1)
		{
			MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].forceRenderingOff = flag;
			}
		}
		else if (ginterface116_0 != null)
		{
			GInterface116[] array2 = ginterface116_0;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i]?.OnBakedLODVisbilityChanged(this, flag);
			}
		}
	}

	public void method_5()
	{
		if (Boolean_0 && _enableSwitch && bool_0 != IsAutocullVisible)
		{
			Show(IsAutocullVisible);
		}
	}
}
