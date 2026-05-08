using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ChartAndGraph.Axis;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

[Serializable]
public abstract class AnyChart : MonoBehaviour, IInternalUse
{
	private bool mGenerating;

	private Dictionary<int, string> mHorizontalValueToStringMap = new Dictionary<int, string>();

	private Dictionary<int, string> mVerticalValueToStringMap = new Dictionary<int, string>();

	[SerializeField]
	private bool keepOrthoSize;

	[SerializeField]
	private bool vRSpaceText;

	[SerializeField]
	private float vRSpaceScale = 0.1f;

	private HashSet<object> mHovered = new HashSet<object>();

	protected ItemLabels mItemLabels;

	protected VerticalAxis mVerticalAxis;

	protected HorizontalAxis mHorizontalAxis;

	protected CategoryLabels mCategoryLabels;

	protected GroupLabels mGroupLabels;

	protected GameObject VerticalMainDevisions;

	protected GameObject VerticalSubDevisions;

	protected GameObject HorizontalMainDevisions;

	protected GameObject HorizontalSubDevisions;

	private bool mGenerateOnNextUpdate;

	protected bool hideHierarchy = true;

	private List<GInterface164> mAxis = new List<GInterface164>();

	[CompilerGenerated]
	private Action ChartGenerated;

	public Dictionary<int, string> VerticalValueToStringMap => mVerticalValueToStringMap;

	public Dictionary<int, string> HorizontalValueToStringMap => mHorizontalValueToStringMap;

	public virtual Camera TextCameraLink => null;

	public virtual float TextIdleDistanceLink => 20f;

	public bool KeepOrthoSize
	{
		get
		{
			return keepOrthoSize;
		}
		set
		{
			keepOrthoSize = value;
			GenerateChart();
		}
	}

	public bool VRSpaceText
	{
		get
		{
			return vRSpaceText;
		}
		set
		{
			vRSpaceText = value;
			GenerateChart();
		}
	}

	public float VRSpaceScale
	{
		get
		{
			return vRSpaceScale;
		}
		set
		{
			vRSpaceScale = value;
			GenerateChart();
		}
	}

	public bool IsUnderCanvas { get; set; }

	public bool CanvasChanged { get; set; }

	public abstract float TotalDepthLink { get; }

	public abstract float TotalHeightLink { get; }

	public abstract float TotalWidthLink { get; }

	bool IInternalUse.HideHierarchy => hideHierarchy;

	public bool Invalidating => mGenerateOnNextUpdate;

	public TextController TextController { get; set; }

	ItemLabels IInternalUse.ItemLabels
	{
		get
		{
			return mItemLabels;
		}
		set
		{
			if (mItemLabels != null)
			{
				((IInternalSettings)mItemLabels).InternalOnDataUpdate -= mGroupLabels_InternalOnDataUpdate;
				((IInternalSettings)mItemLabels).InternalOnDataChanged -= mGroupLabels_InternalOnDataChanged;
			}
			mItemLabels = value;
			if (mItemLabels != null)
			{
				((IInternalSettings)mItemLabels).InternalOnDataUpdate += mGroupLabels_InternalOnDataUpdate;
				((IInternalSettings)mItemLabels).InternalOnDataChanged += mGroupLabels_InternalOnDataChanged;
			}
			OnLabelSettingsSet();
		}
	}

	VerticalAxis IInternalUse.VerticalAxis
	{
		get
		{
			return mVerticalAxis;
		}
		set
		{
			if (mVerticalAxis != null)
			{
				((IInternalSettings)mVerticalAxis).InternalOnDataChanged -= mHorizontalAxis_InternalOnDataUpdate;
				((IInternalSettings)mVerticalAxis).InternalOnDataUpdate -= mHorizontalAxis_InternalOnDataUpdate;
			}
			mVerticalAxis = value;
			if (mVerticalAxis != null)
			{
				((IInternalSettings)mVerticalAxis).InternalOnDataChanged += mHorizontalAxis_InternalOnDataUpdate;
				((IInternalSettings)mVerticalAxis).InternalOnDataUpdate += mHorizontalAxis_InternalOnDataUpdate;
			}
			OnAxisValuesChanged();
		}
	}

	HorizontalAxis IInternalUse.HorizontalAxis
	{
		get
		{
			return mHorizontalAxis;
		}
		set
		{
			if (mHorizontalAxis != null)
			{
				((IInternalSettings)mHorizontalAxis).InternalOnDataChanged -= mHorizontalAxis_InternalOnDataUpdate;
				((IInternalSettings)mHorizontalAxis).InternalOnDataUpdate -= mHorizontalAxis_InternalOnDataUpdate;
			}
			mHorizontalAxis = value;
			if (mHorizontalAxis != null)
			{
				((IInternalSettings)mHorizontalAxis).InternalOnDataChanged += mHorizontalAxis_InternalOnDataUpdate;
				((IInternalSettings)mHorizontalAxis).InternalOnDataUpdate += mHorizontalAxis_InternalOnDataUpdate;
			}
			OnAxisValuesChanged();
		}
	}

	CategoryLabels IInternalUse.CategoryLabels
	{
		get
		{
			return mCategoryLabels;
		}
		set
		{
			if (mCategoryLabels != null)
			{
				((IInternalSettings)mCategoryLabels).InternalOnDataUpdate -= mGroupLabels_InternalOnDataUpdate;
				((IInternalSettings)mCategoryLabels).InternalOnDataChanged -= mGroupLabels_InternalOnDataChanged;
			}
			mCategoryLabels = value;
			if (mCategoryLabels != null)
			{
				((IInternalSettings)mCategoryLabels).InternalOnDataUpdate += mGroupLabels_InternalOnDataUpdate;
				((IInternalSettings)mCategoryLabels).InternalOnDataChanged += mGroupLabels_InternalOnDataChanged;
			}
			OnLabelSettingsSet();
		}
	}

	GroupLabels IInternalUse.GroupLabels
	{
		get
		{
			return mGroupLabels;
		}
		set
		{
			if (mGroupLabels != null)
			{
				((IInternalSettings)mGroupLabels).InternalOnDataUpdate -= mGroupLabels_InternalOnDataUpdate;
				((IInternalSettings)mGroupLabels).InternalOnDataChanged -= mGroupLabels_InternalOnDataChanged;
			}
			mGroupLabels = value;
			if (mGroupLabels != null)
			{
				((IInternalSettings)mGroupLabels).InternalOnDataUpdate += mGroupLabels_InternalOnDataUpdate;
				((IInternalSettings)mGroupLabels).InternalOnDataChanged += mGroupLabels_InternalOnDataChanged;
			}
			OnLabelSettingsSet();
		}
	}

	TextController IInternalUse.InternalTextController => TextController;

	Camera IInternalUse.InternalTextCamera => TextCameraLink;

	float IInternalUse.InternalTextIdleDistance => TextIdleDistanceLink;

	float IInternalUse.InternalTotalDepth => TotalDepthLink;

	float IInternalUse.InternalTotalWidth => TotalWidthLink;

	float IInternalUse.InternalTotalHeight => TotalHeightLink;

	public abstract bool SupportsCategoryLabels { get; }

	public abstract bool SupportsItemLabels { get; }

	public abstract bool SupportsGroupLables { get; }

	bool IInternalUse.InternalSupportsCategoryLables => SupportsCategoryLabels;

	bool IInternalUse.InternalSupportsGroupLabels => SupportsGroupLables;

	bool IInternalUse.InternalSupportsItemLabels => SupportsItemLabels;

	public event Action Event_0
	{
		[CompilerGenerated]
		add
		{
			Action action = ChartGenerated;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref ChartGenerated, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = ChartGenerated;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref ChartGenerated, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action ChartAndGraph_002EIInternalUse_002EGenerated
	{
		add
		{
			Event_0 += value;
		}
		remove
		{
			Event_0 -= value;
		}
	}

	public void mHorizontalAxis_InternalOnDataUpdate(object sender, EventArgs e)
	{
		GenerateChart();
	}

	public virtual void OnPropertyUpdated()
	{
		ValidateProperties();
	}

	public void mGroupLabels_InternalOnDataChanged(object sender, EventArgs e)
	{
		OnLabelSettingsSet();
	}

	public void mGroupLabels_InternalOnDataUpdate(object sender, EventArgs e)
	{
		OnLabelSettingChanged();
	}

	public virtual void OnLabelSettingChanged()
	{
	}

	public virtual double GetScrollOffset(int axis)
	{
		return 0.0;
	}

	public virtual void OnAxisValuesChanged()
	{
	}

	public virtual void OnLabelSettingsSet()
	{
	}

	public virtual void Start()
	{
		if (base.gameObject.activeInHierarchy)
		{
			method_0(start: true);
			method_2();
		}
	}

	public void method_0(bool start)
	{
		Canvas componentInParent = GetComponentInParent<Canvas>();
		bool isUnderCanvas = IsUnderCanvas;
		IsUnderCanvas = componentInParent != null;
		if (IsUnderCanvas && !start && IsUnderCanvas != isUnderCanvas)
		{
			CanvasChanged = true;
			GenerateChart();
			CanvasChanged = false;
		}
	}

	public void method_1()
	{
		GameObject gameObject = new GameObject("textController", typeof(RectTransform));
		GClass1664.smethod_3(gameObject, hideHierarchy);
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localPosition = Vector3.zero;
		TextController = gameObject.AddComponent<TextController>();
		TextController.AnyChart_0 = this;
	}

	public void method_2()
	{
		if (!(TextController != null))
		{
			method_1();
		}
	}

	public virtual void Invalidate()
	{
		if (!mGenerating)
		{
			mGenerateOnNextUpdate = true;
		}
	}

	public virtual void Update()
	{
		if (base.gameObject.activeInHierarchy)
		{
			method_0(start: false);
			if (mGenerateOnNextUpdate)
			{
				mGenerateOnNextUpdate = false;
				GenerateChart();
			}
		}
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnValidate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			ValidateProperties();
			method_0(start: true);
			method_2();
		}
	}

	public abstract bool HasValues(AxisBase axis);

	public abstract double MaxValue(AxisBase axis);

	public abstract double MinValue(AxisBase axis);

	public virtual void OnEnable()
	{
		ChartItem[] componentsInChildren = GetComponentsInChildren<ChartItem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null && componentsInChildren[i].gameObject != base.gameObject)
			{
				componentsInChildren[i].gameObject.SetActive(value: true);
			}
		}
	}

	public virtual void OnDisable()
	{
		ChartItem[] componentsInChildren = GetComponentsInChildren<ChartItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null && componentsInChildren[i].gameObject != base.gameObject)
			{
				componentsInChildren[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void GenerateChart()
	{
		if (mGenerating)
		{
			return;
		}
		mGenerating = true;
		InternalGenerateChart();
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == null) && !(transform.gameObject == null))
			{
				transform.gameObject.layer = base.gameObject.layer;
			}
		}
		mGenerating = false;
	}

	public virtual void InternalGenerateChart()
	{
		if (base.gameObject.activeInHierarchy)
		{
			mGenerateOnNextUpdate = false;
			if (ChartGenerated != null)
			{
				ChartGenerated();
			}
		}
	}

	public virtual void ClearChart()
	{
		mHovered.Clear();
		if (TextController != null)
		{
			TextController.DestroyAll();
			TextController.transform.SetParent(base.transform, worldPositionStays: false);
		}
		ChartItem[] componentsInChildren = GetComponentsInChildren<ChartItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null)
			{
				RectMask2D component = componentsInChildren[i].GetComponent<RectMask2D>();
				if (component != null)
				{
					Debug.Log(component.gameObject);
				}
				if ((!(TextController != null) || !(componentsInChildren[i].gameObject == TextController.gameObject)) && componentsInChildren[i].gameObject != base.gameObject)
				{
					GClass1664.SafeDestroy(componentsInChildren[i].gameObject);
				}
			}
		}
		method_2();
		for (int j = 0; j < mAxis.Count; j++)
		{
			if (mAxis[j] != null && mAxis[j].This() != null)
			{
				GClass1664.SafeDestroy(mAxis[j].GetGameObject());
			}
		}
		mAxis.Clear();
	}

	public virtual void OnNonHoverted()
	{
	}

	public virtual void OnItemLeave(object userData)
	{
		if (mHovered.Count != 0)
		{
			mHovered.Remove(userData);
			if (mHovered.Count == 0)
			{
				OnNonHoverted();
			}
		}
	}

	public virtual void OnItemSelected(object userData)
	{
	}

	public virtual void OnItemHoverted(object userData)
	{
		mHovered.Add(userData);
	}

	public virtual GInterface164 InternalUpdateAxis(ref GameObject axisObject, AxisBase axisBase, ChartOrientation axisOrientation, bool isSubDiv, bool forceRecreate, double scrollOffset)
	{
		GInterface164 gInterface = null;
		if (!(axisObject == null || forceRecreate) && !CanvasChanged)
		{
			gInterface = ((!IsUnderCanvas) ? ((GInterface164)axisObject.GetComponent<AxisGenerator>()) : ((GInterface164)axisObject.GetComponent<CanvasAxisGenerator>()));
		}
		else
		{
			GClass1664.SafeDestroy(axisObject);
			GameObject gameObject = null;
			gameObject = ((!IsUnderCanvas) ? GClass1664.smethod_2() : GClass1664.smethod_1());
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localPosition = default(Vector3);
			gameObject.layer = base.gameObject.layer;
			GClass1664.smethod_3(gameObject, hideHierarchy);
			axisObject = gameObject;
			gInterface = ((!IsUnderCanvas) ? ((GInterface164)gameObject.AddComponent<AxisGenerator>()) : ((GInterface164)gameObject.AddComponent<CanvasAxisGenerator>()));
		}
		gInterface.SetAxis(scrollOffset, this, axisBase, axisOrientation, isSubDiv);
		return gInterface;
	}

	public virtual void ValidateProperties()
	{
		if (mItemLabels != null)
		{
			mItemLabels.ValidateProperties();
		}
		if (mCategoryLabels != null)
		{
			mCategoryLabels.ValidateProperties();
		}
		if (mGroupLabels != null)
		{
			mGroupLabels.ValidateProperties();
		}
		if (mHorizontalAxis != null)
		{
			mHorizontalAxis.ValidateProperties();
		}
		if (mVerticalAxis != null)
		{
			mVerticalAxis.ValidateProperties();
		}
	}

	public void GenerateAxis(bool force)
	{
		mAxis.Clear();
		if ((bool)mVerticalAxis)
		{
			double scrollOffset = GetScrollOffset(1);
			GInterface164 gInterface = InternalUpdateAxis(ref VerticalMainDevisions, mVerticalAxis, ChartOrientation.Vertical, isSubDiv: false, force, scrollOffset);
			GInterface164 gInterface2 = InternalUpdateAxis(ref VerticalSubDevisions, mVerticalAxis, ChartOrientation.Vertical, isSubDiv: true, force, scrollOffset);
			if (gInterface != null)
			{
				mAxis.Add(gInterface);
			}
			if (gInterface2 != null)
			{
				mAxis.Add(gInterface2);
			}
		}
		if ((bool)mHorizontalAxis)
		{
			double scrollOffset2 = GetScrollOffset(0);
			GInterface164 gInterface3 = InternalUpdateAxis(ref HorizontalMainDevisions, mHorizontalAxis, ChartOrientation.Horizontal, isSubDiv: false, force, scrollOffset2);
			GInterface164 gInterface4 = InternalUpdateAxis(ref HorizontalSubDevisions, mHorizontalAxis, ChartOrientation.Horizontal, isSubDiv: true, force, scrollOffset2);
			if (gInterface3 != null)
			{
				mAxis.Add(gInterface3);
			}
			if (gInterface4 != null)
			{
				mAxis.Add(gInterface4);
			}
		}
	}

	public void InternalItemSelected(object userData)
	{
		OnItemSelected(userData);
	}

	void IInternalUse.InternalItemSelected(object userData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in InternalItemSelected
		this.InternalItemSelected(userData);
	}

	public void InternalItemLeave(object userData)
	{
		OnItemLeave(userData);
	}

	void IInternalUse.InternalItemLeave(object userData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in InternalItemLeave
		this.InternalItemLeave(userData);
	}

	public void InternalItemHovered(object userData)
	{
		OnItemHoverted(userData);
	}

	void IInternalUse.InternalItemHovered(object userData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in InternalItemHovered
		this.InternalItemHovered(userData);
	}

	public void CallOnValidate()
	{
		OnValidate();
	}

	void IInternalUse.CallOnValidate()
	{
		//ILSpy generated this explicit interface implementation from .override directive in CallOnValidate
		this.CallOnValidate();
	}

	public bool InternalHasValues(AxisBase axis)
	{
		return HasValues(axis);
	}

	bool IInternalUse.InternalHasValues(AxisBase axis)
	{
		//ILSpy generated this explicit interface implementation from .override directive in InternalHasValues
		return this.InternalHasValues(axis);
	}

	public double InternalMaxValue(AxisBase axis)
	{
		return MaxValue(axis);
	}

	double IInternalUse.InternalMaxValue(AxisBase axis)
	{
		//ILSpy generated this explicit interface implementation from .override directive in InternalMaxValue
		return this.InternalMaxValue(axis);
	}

	public double InternalMinValue(AxisBase axis)
	{
		return MinValue(axis);
	}

	double IInternalUse.InternalMinValue(AxisBase axis)
	{
		//ILSpy generated this explicit interface implementation from .override directive in InternalMinValue
		return this.InternalMinValue(axis);
	}

	public AnyChart()
	{
	}
}
