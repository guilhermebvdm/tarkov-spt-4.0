using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
[GAttribute8(20000)]
public class CullingObject : MonoBehaviour, GInterface55, IBotController
{
	[SerializeField]
	protected float CullDistance = 80f;

	[SerializeField]
	protected float _radius = 1f;

	[SerializeField]
	protected Vector3 _shift;

	[SerializeField]
	private bool _drawSphere = true;

	[SerializeField]
	private bool _cullByDistanceOnly = true;

	[SerializeField]
	protected List<Component> _componentsToTurnOff = new List<Component>();

	[HideInInspector]
	[SerializeField]
	private List<DisablerCullingObject.ComponentState> _componentsToTurnOffDefaultState;

	[SerializeField]
	private List<GameObject> _gameObjectsToTurnOff;

	[SerializeField]
	private Transform _transform;

	[CompilerGenerated]
	private int int_0;

	private Vector3 vector3_0;

	protected bool _isVisible;

	[CompilerGenerated]
	private float float_0;

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private bool bool_1;

	[SerializeField]
	private int _componentsSwitchPerFrameOnEnable = 25;

	[SerializeField]
	private int _objectsSwitchPerFrameOnEnable = 25;

	[SerializeField]
	private int _componentsSwitchPerFrameOnDisable = 25;

	[SerializeField]
	private int _objectsSwitchPerFrameOnDisable = 25;

	private Stopwatch stopwatch_0 = new Stopwatch();

	private IEnumerator ienumerator_0;

	public int Index
	{
		[CompilerGenerated]
		get
		{
			return int_0;
		}
		[CompilerGenerated]
		set
		{
			int_0 = value;
		}
	}

	public float CullDistanceSqr => CullDistance * CullDistance;

	public bool CullByDistanceOnly
	{
		get
		{
			return _cullByDistanceOnly;
		}
		set
		{
			_cullByDistanceOnly = value;
		}
	}

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public Vector3 Shift
	{
		get
		{
			return _shift;
		}
		set
		{
			_shift = value;
		}
	}

	public Vector3 Position => GetTransform().position + _shift;

	public Vector3 SafeMultithreadedPosition => vector3_0;

	public Vector3 ClearTransformPosition => GetTransform().position;

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			_isVisible = value;
		}
	}

	public float SqrCameraDistance
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public bool IsVisibleByCamera
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public bool IsAutocullVisible
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

	public virtual void Awake()
	{
	}

	public virtual void OnValidate()
	{
		OnPreProcess();
	}

	public void OnPreProcess()
	{
		if (_transform == null)
		{
			_transform = base.transform;
		}
		DisablerCullingObject.NullClean(_componentsToTurnOff, _gameObjectsToTurnOff);
		DisablerCullingObject.SortComponentsToTurnOff(_componentsToTurnOff);
	}

	public void Start()
	{
		IsAutocullVisible = true;
		if (CullingManager.Instance == null)
		{
			CullingManager.AddEarlyObject(this);
		}
		else
		{
			Register();
		}
	}

	public virtual void CustomUpdate()
	{
		Transform transform = GetTransform();
		if (transform.hasChanged)
		{
			vector3_0 = ClearTransformPosition;
			CullingManager.Instance.UpdateSphere(this);
			transform.hasChanged = false;
		}
	}

	public void SetAutocullVisibility(bool flag)
	{
		IsAutocullVisible = flag;
		if (flag)
		{
			method_0(IsVisible);
		}
		else
		{
			method_0(isVisible: false);
		}
	}

	public virtual void SetVisibility(bool isVisible)
	{
		if (_isVisible != isVisible)
		{
			IsVisible = isVisible;
			method_0(isVisible);
		}
	}

	public void Register()
	{
		Index = CullingManager.Instance.Register(this);
	}

	public void method_0(bool isVisible)
	{
		GClass997.SetComponentsEnabled(base.gameObject, this, ref ienumerator_0, _componentsToTurnOff, _gameObjectsToTurnOff, _componentsSwitchPerFrameOnEnable, _objectsSwitchPerFrameOnEnable, _componentsSwitchPerFrameOnDisable, _objectsSwitchPerFrameOnDisable, isVisible, stopwatch_0);
	}

	public virtual void OnDestroy()
	{
		SetVisibility(isVisible: true);
		if (CullingManager.Instance != null)
		{
			CullingManager.Instance.Unregister(this);
		}
		else
		{
			CullingManager.RemoveEarlyObject(this);
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (_drawSphere)
		{
			Gizmos.color = (IsVisible ? Color.yellow : Color.red);
			Gizmos.DrawWireSphere(Position, _radius);
		}
	}

	public virtual Transform GetTransform()
	{
		return _transform;
	}

	public void SortComponentsToTurnOff()
	{
	}
}
