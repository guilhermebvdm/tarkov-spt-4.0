using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using EFT.Interactive;
using UnityEngine;

[ExecuteInEditMode]
public class DisablerCullingObject : DisablerCullingObjectBase
{
	[Serializable]
	public class ComponentState
	{
		public Component component;

		public bool isEnabled;
	}

	[SerializeField]
	protected List<Component> _componentsToTurnOff = new List<Component>();

	[SerializeField]
	protected List<Component> _compsToTurnOffWhoIgnoreInversedColliders = new List<Component>();

	[HideInInspector]
	[SerializeField]
	private List<ComponentState> _componentsToTurnOffDefaultState;

	[SerializeField]
	protected List<GameObject> _gameObjectsToTurnOff;

	[SerializeField]
	protected Bounds _componentsBounds;

	[SerializeField]
	public bool AllowLootCulling = true;

	[Header("Performance:")]
	[SerializeField]
	private int _componentsSwitchPerFrameOnEnable = 25;

	[SerializeField]
	private int _objectsSwitchPerFrameOnEnable = 25;

	[SerializeField]
	private int _componentsSwitchPerFrameOnDisable = 25;

	[SerializeField]
	private int _objectsSwitchPerFrameOnDisable = 25;

	[HideInInspector]
	public bool ExcludeLowPolyColliderLayer = true;

	[HideInInspector]
	public bool ExcludeDefaultLayerWithCollider;

	[HideInInspector]
	public bool ExcludeBallisticCollider;

	[HideInInspector]
	public bool IncludeInactive;

	[Header("View:")]
	public Color GizmosColor = new Color(0f, 1f, 0f, 0.16f);

	public Color GizmosInverseColor = new Color(1f, 0f, 0f, 0.1f);

	[SerializeField]
	private Color _unselectedGizmosColor = new Color(0.5f, 0.5f, 0.5f, 0.16f);

	private Stopwatch stopwatch_0 = new Stopwatch();

	private IEnumerator ienumerator_0;

	private IEnumerator ienumerator_1;

	private static Dictionary<Renderer, Bounds> dictionary_0;

	private bool bool_0;

	private static Type[] type_0 = new Type[7]
	{
		typeof(LODGroup),
		typeof(FogLight),
		typeof(BaseLight),
		typeof(Light),
		typeof(LampController),
		typeof(VolumetricLight),
		typeof(MeshRenderer)
	};

	private static Dictionary<Renderer, Bounds> dictionary_1;

	public int ComponentsSwitchPerFrameOnEnable
	{
		get
		{
			return _componentsSwitchPerFrameOnEnable;
		}
		set
		{
			_componentsSwitchPerFrameOnEnable = value;
		}
	}

	public int ComponentsSwitchPerFrameOnDisable
	{
		get
		{
			return _componentsSwitchPerFrameOnDisable;
		}
		set
		{
			_componentsSwitchPerFrameOnDisable = value;
		}
	}

	public List<Component> ComponentsToTurnOff => _componentsToTurnOff;

	public static void NullClean(List<Component> componentsToTurnOff, List<GameObject> gameObjectsToTurnOff)
	{
		if (componentsToTurnOff != null)
		{
			for (int num = componentsToTurnOff.Count - 1; num >= 0; num--)
			{
				Component component = componentsToTurnOff[num];
				if (component == null || component.hideFlags != HideFlags.None)
				{
					componentsToTurnOff.RemoveAt(num);
				}
			}
		}
		if (gameObjectsToTurnOff == null)
		{
			return;
		}
		for (int num2 = gameObjectsToTurnOff.Count - 1; num2 >= 0; num2--)
		{
			GameObject gameObject = gameObjectsToTurnOff[num2];
			if (gameObject == null || gameObject.hideFlags != HideFlags.None)
			{
				gameObjectsToTurnOff.RemoveAt(num2);
			}
		}
	}

	public virtual void NullClean()
	{
		NullClean(_componentsToTurnOff, _gameObjectsToTurnOff);
	}

	public override void PrepareCullingObject()
	{
		base.PrepareCullingObject();
		if (dictionary_0 == null)
		{
			dictionary_0 = new Dictionary<Renderer, Bounds>();
		}
		for (int i = 0; i < _componentsToTurnOff.Count; i++)
		{
			if (_componentsToTurnOff[i] is Renderer renderer && renderer != null && !dictionary_0.ContainsKey(renderer))
			{
				dictionary_0.Add(renderer, renderer.bounds);
			}
		}
		NullClean();
		SortComponentsToTurnOff();
		method_3();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void method_3()
	{
		bool flag = true;
		for (int i = 0; i < _componentsToTurnOff.Count; i++)
		{
			if (_componentsToTurnOff[i] is Renderer renderer)
			{
				Bounds value;
				Bounds bounds = ((dictionary_0 == null || !dictionary_0.TryGetValue(renderer, out value)) ? renderer.bounds : value);
				if (flag)
				{
					_componentsBounds = bounds;
					flag = false;
				}
				else
				{
					_componentsBounds.Encapsulate(bounds);
				}
			}
		}
		if (flag)
		{
			_componentsBounds = default(Bounds);
		}
	}

	public override void SetComponentsEnabled(bool isVisible)
	{
		if (!bool_0)
		{
			GClass997.SetComponentsEnabled(base.gameObject, this, ref ienumerator_0, _componentsToTurnOff, _gameObjectsToTurnOff, _componentsSwitchPerFrameOnEnable, _objectsSwitchPerFrameOnEnable, _componentsSwitchPerFrameOnDisable, _objectsSwitchPerFrameOnDisable, isVisible, stopwatch_0);
			GClass997.SetComponentsEnabled(base.gameObject, this, ref ienumerator_1, _compsToTurnOffWhoIgnoreInversedColliders, null, _componentsSwitchPerFrameOnEnable, _objectsSwitchPerFrameOnEnable, _componentsSwitchPerFrameOnDisable, _objectsSwitchPerFrameOnDisable, _enteredColliders.Count > 0, stopwatch_0);
		}
	}

	public void ForceEnable(bool value)
	{
		bool flag = bool_0;
		bool_0 = false;
		SetComponentsEnabled(value);
		bool_0 = flag;
	}

	public void ForceUpdate()
	{
		UpdateComponentsStatusOnUpdate();
	}

	public void LockState(bool value)
	{
		bool_0 = value;
		_updateComponentsStatus = true;
	}

	public override void ManualUpdate(float dt)
	{
		base.ManualUpdate(dt);
		if (dictionary_0 != null)
		{
			dictionary_0 = null;
		}
	}

	public void RegisterComponents<T>(IEnumerable<T> components, bool ignoreInverseColliders = false) where T : Component
	{
		bool flag = false;
		foreach (T component in components)
		{
			if (ignoreInverseColliders)
			{
				_compsToTurnOffWhoIgnoreInversedColliders.Add(component);
			}
			else
			{
				_componentsToTurnOff.Add(component);
			}
			flag = true;
		}
		if (flag)
		{
			_updateComponentsStatus = true;
		}
	}

	public void UnregisterComponents<T>(IEnumerable<T> components, bool ignoreInverseColliders = false) where T : Component
	{
		foreach (T component in components)
		{
			if (ignoreInverseColliders)
			{
				_compsToTurnOffWhoIgnoreInversedColliders.Remove(component);
			}
			else
			{
				_componentsToTurnOff.Remove(component);
			}
		}
	}

	public static void SortComponentsToTurnOff(List<Component> componentsToTurnOff, Dictionary<Renderer, Bounds> cache = null)
	{
		dictionary_1 = cache;
		componentsToTurnOff.Sort(smethod_1);
		dictionary_1 = null;
	}

	public static int smethod_0(Component component)
	{
		Type type = component.GetType();
		int num = 0;
		while (true)
		{
			if (num < type_0.Length)
			{
				Type type2 = type_0[num];
				if (type == type2 || type.IsSubclassOf(type2))
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}

	public static int smethod_1(Component x, Component y)
	{
		int num = smethod_0(x);
		int num2 = smethod_0(y);
		if (num < num2)
		{
			return -1;
		}
		if (num > num2)
		{
			return 1;
		}
		Renderer renderer = x as Renderer;
		Renderer renderer2 = y as Renderer;
		if (!(renderer == null) && !(renderer2 == null))
		{
			Bounds value;
			Vector3 vector = ((dictionary_1 == null || !dictionary_1.TryGetValue(renderer, out value)) ? renderer.bounds.size : value.size);
			Bounds value2;
			Vector3 vector2 = ((dictionary_1 == null || !dictionary_1.TryGetValue(renderer2, out value2)) ? renderer2.bounds.size : value2.size);
			float value3 = vector.x * vector.y * vector.z;
			return (vector2.x * vector2.y * vector2.z).CompareTo(value3);
		}
		return 0;
	}

	public void SortComponentsToTurnOff()
	{
		SortComponentsToTurnOff(_componentsToTurnOff, dictionary_0);
	}

	public Bounds GetBounds()
	{
		return _componentsBounds;
	}
}
