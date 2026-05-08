using UnityEngine;

public abstract class LightOverride : MonoBehaviour
{
	public enum Type
	{
		None,
		Point,
		Tube,
		Area,
		Directional
	}

	[Header("Overrides")]
	public float m_IntensityMult = 1f;

	[GAttribute13(0f)]
	public float m_RangeMult = 1f;

	private Type type_0;

	private bool bool_0;

	private Light light_0;

	private TubeLight tubeLight_0;

	private AreaLight areaLight_0;

	public bool isOn
	{
		get
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			method_0();
			switch (type_0)
			{
			default:
				return false;
			case Type.Point:
				if (!light_0.enabled)
				{
					return GetForceOn();
				}
				return true;
			case Type.Tube:
				if (!tubeLight_0.enabled)
				{
					return GetForceOn();
				}
				return true;
			case Type.Area:
				if (!areaLight_0.enabled)
				{
					return GetForceOn();
				}
				return true;
			case Type.Directional:
				if (!light_0.enabled)
				{
					return GetForceOn();
				}
				return true;
			}
		}
		set
		{
		}
	}

	public Light light
	{
		get
		{
			method_0();
			return light_0;
		}
		set
		{
		}
	}

	public TubeLight tubeLight
	{
		get
		{
			method_0();
			return tubeLight_0;
		}
		set
		{
		}
	}

	public AreaLight areaLight
	{
		get
		{
			method_0();
			return areaLight_0;
		}
		set
		{
		}
	}

	public Type type
	{
		get
		{
			method_0();
			return type_0;
		}
		set
		{
		}
	}

	public void Update()
	{
	}

	public abstract bool GetForceOn();

	public void method_0()
	{
		if (bool_0)
		{
			return;
		}
		if ((light_0 = GetComponent<Light>()) != null)
		{
			switch (light_0.type)
			{
			case LightType.Point:
				type_0 = Type.Point;
				break;
			default:
				type_0 = Type.None;
				break;
			case LightType.Directional:
				type_0 = Type.Directional;
				break;
			}
		}
		else if ((tubeLight_0 = GetComponent<TubeLight>()) != null)
		{
			type_0 = Type.Tube;
		}
		else if ((areaLight_0 = GetComponent<AreaLight>()) != null)
		{
			type_0 = Type.Area;
		}
		bool_0 = true;
	}

	public LightOverride()
	{
	}
}
