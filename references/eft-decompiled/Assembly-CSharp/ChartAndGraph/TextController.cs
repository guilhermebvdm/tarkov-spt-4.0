using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

[RequireComponent(typeof(ChartItem))]
[ExecuteInEditMode]
public class TextController : MonoBehaviour
{
	[CompilerGenerated]
	public class Class1083
	{
		public TextController textController_0;

		public float scale;

		public bool method_0(BillboardText x)
		{
			if (x == null)
			{
				return true;
			}
			if (!textController_0._invalidated || x.transform.hasChanged || textController_0._canvas.transform.hasChanged)
			{
				x.Rect.transform.position = x.transform.position;
				x.UIText.transform.localScale = new Vector3(x.Scale * scale, x.Scale * scale, 1f);
				x.transform.hasChanged = false;
			}
			return false;
		}
	}

	public Camera Camera;

	public float PlaneDistance = 4f;

	private const float PreviousScale = -1f;

	private Canvas _canvas;

	private bool _invalidated;

	private AnyChart _privateParent;

	private readonly List<BillboardText> _textList = new List<BillboardText>();

	public List<BillboardText> List_0 => _textList;

	public Canvas Canvas_0
	{
		get
		{
			method_1();
			return _canvas;
		}
	}

	public AnyChart AnyChart_0
	{
		get
		{
			return _privateParent;
		}
		set
		{
			_privateParent = value;
			if (_privateParent != null)
			{
				Camera = ((IInternalUse)_privateParent).InternalTextCamera;
				PlaneDistance = ((IInternalUse)_privateParent).InternalTextIdleDistance;
				Canvas_0.planeDistance = PlaneDistance;
			}
		}
	}

	public void Start()
	{
		method_1();
		Canvas.willRenderCanvases += method_0;
	}

	public void OnDestroy()
	{
		DestroyAll();
		Canvas.willRenderCanvases -= method_0;
	}

	public void method_0()
	{
		ApplyTextPosition();
	}

	public void method_1()
	{
		if (!(_canvas == null))
		{
			return;
		}
		_canvas = GetComponentInParent<Canvas>();
		if (_canvas == null)
		{
			_canvas = base.gameObject.AddComponent<Canvas>();
			base.gameObject.AddComponent<CanvasScaler>();
			base.gameObject.AddComponent<GraphicRaycaster>();
			if (AnyChart_0 != null && AnyChart_0.VRSpaceText)
			{
				_canvas.renderMode = RenderMode.WorldSpace;
			}
			else
			{
				_canvas.renderMode = RenderMode.ScreenSpaceCamera;
			}
			_canvas.planeDistance = PlaneDistance;
			Camera = method_2();
			GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		}
	}

	public void DestroyAll()
	{
		foreach (BillboardText text in _textList)
		{
			if (text != null && !text.Recycled)
			{
				if (text.UIText != null)
				{
					GClass1664.SafeDestroy(text.UIText.gameObject);
				}
				if (text.RectTransformOverride != null)
				{
					GClass1664.SafeDestroy(text.RectTransformOverride.gameObject);
				}
				text.UIText = null;
				text.RectTransformOverride = null;
				GClass1664.SafeDestroy(text.gameObject);
			}
		}
		_textList.Clear();
	}

	public void AddText(BillboardText billboard)
	{
		if (!(billboard.UIText == null))
		{
			_invalidated = false;
			_textList.Add(billboard);
			TextDirection component = billboard.Rect.GetComponent<TextDirection>();
			if (component != null)
			{
				component.SetTextController(this);
			}
			GameObject gameObject = GClass1664.smethod_1();
			RectTransform component2 = gameObject.GetComponent<RectTransform>();
			gameObject.AddComponent<Canvas>();
			gameObject.transform.SetParent(base.transform, worldPositionStays: false);
			billboard.parent = component2;
			billboard.Rect.SetParent(component2, worldPositionStays: false);
			if (AnyChart_0 != null)
			{
				gameObject.layer = AnyChart_0.gameObject.layer;
				billboard.Rect.gameObject.layer = AnyChart_0.gameObject.layer;
			}
			billboard.Rect.localRotation = Quaternion.identity;
			billboard.Rect.localPosition = Vector3.zero;
			billboard.Rect.localScale = new Vector3(1f, 1f, 1f);
			billboard.UIText.transform.localScale = new Vector3(billboard.Scale, billboard.Scale, 1f);
			RectTransform rect = billboard.Rect;
			if (component == null)
			{
				rect.anchorMin = Vector2.zero;
				rect.anchorMax = Vector2.zero;
			}
			billboard.parent.position = billboard.transform.position;
		}
	}

	public Camera method_2()
	{
		return method_3((Camera == null) ? Camera.main : Camera);
	}

	public Camera method_3(Camera cam)
	{
		Canvas canvas_ = Canvas_0;
		if (canvas_.worldCamera != cam)
		{
			canvas_.worldCamera = cam;
		}
		return cam;
	}

	public void ApplyTextPosition()
	{
		Camera = method_2();
		if (AnyChart_0 != null && AnyChart_0.VRSpaceText)
		{
			_canvas.transform.rotation = Quaternion.LookRotation(new Vector3(base.transform.position.x, 0f, base.transform.position.z) - new Vector3(Camera.transform.position.x, 0f, Camera.transform.position.z), Vector3.up);
			_canvas.transform.localScale = new Vector3(AnyChart_0.VRSpaceScale, AnyChart_0.VRSpaceScale, AnyChart_0.VRSpaceScale);
		}
		float scale = 1f;
		if (_privateParent != null && _privateParent.KeepOrthoSize && Camera != null && Camera.orthographic && Camera.orthographicSize > 0.1f)
		{
			scale = 5f / Camera.orthographicSize;
		}
		if (Mathf.Abs(-1f - scale) < 0.01f)
		{
			_invalidated = false;
		}
		_textList.RemoveAll(delegate(BillboardText x)
		{
			if (x == null)
			{
				return true;
			}
			if (!_invalidated || x.transform.hasChanged || _canvas.transform.hasChanged)
			{
				x.Rect.transform.position = x.transform.position;
				x.UIText.transform.localScale = new Vector3(x.Scale * scale, x.Scale * scale, 1f);
				x.transform.hasChanged = false;
			}
			return false;
		});
		_invalidated = true;
	}
}
