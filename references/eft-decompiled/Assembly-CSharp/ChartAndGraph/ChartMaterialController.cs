using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChartAndGraph;

public class ChartMaterialController : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	private static bool bool_0;

	public bool HandleEvents = true;

	[SerializeField]
	private ChartDynamicMaterial materials;

	internal ChartItemMaterialLerpEffect chartItemMaterialLerpEffect_0;

	private Color color_0 = Color.clear;

	private Color color_1;

	private Renderer renderer_0;

	private Material material_0;

	private Graphic graphic_0;

	private bool bool_1;

	private bool bool_2;

	private int? nullable_0;

	private float float_0;

	private bool bool_3;

	public ChartDynamicMaterial ChartDynamicMaterial_0
	{
		get
		{
			return materials;
		}
		set
		{
			materials = value;
			if (materials != null)
			{
				if (materials.Normal == null)
				{
					method_1();
				}
				else
				{
					method_0(materials.Normal, null);
				}
			}
		}
	}

	public void method_0(Material m, Material fallback)
	{
		if (renderer_0 == null)
		{
			renderer_0 = GetComponent<Renderer>();
		}
		if (renderer_0 != null)
		{
			GClass1664.smethod_12(renderer_0, m, fallback);
			return;
		}
		if (graphic_0 == null)
		{
			graphic_0 = GetComponent<Graphic>();
		}
		if (material_0 == null)
		{
			material_0 = new Material(materials.Normal);
			material_0.hideFlags = HideFlags.DontSave;
		}
		if (graphic_0 != null)
		{
			graphic_0.material = material_0;
		}
	}

	public void method_1()
	{
		if (!bool_0)
		{
			Debug.LogWarning("null material are not allowed");
			bool_0 = true;
		}
	}

	public void Start()
	{
		chartItemMaterialLerpEffect_0 = GetComponent<ChartItemMaterialLerpEffect>();
		Refresh();
	}

	public int method_2()
	{
		if (!nullable_0.HasValue)
		{
			nullable_0 = Shader.PropertyToID("_Combine");
		}
		return nullable_0.Value;
	}

	public void TriggerOn()
	{
		bool_1 = true;
		Refresh();
	}

	public void TriggerOff()
	{
		bool_1 = false;
		Refresh();
	}

	public void OnMouseEnter()
	{
		if (HandleEvents)
		{
			bool_1 = true;
			Refresh();
		}
	}

	public void OnMouseExit()
	{
		if (HandleEvents)
		{
			bool_1 = false;
			Refresh();
		}
	}

	public void OnMouseDown()
	{
		if (HandleEvents)
		{
			bool_2 = true;
			Refresh();
		}
	}

	public void OnMouseUp()
	{
		if (HandleEvents)
		{
			bool_2 = false;
			Refresh();
		}
	}

	public void Update()
	{
		if (chartItemMaterialLerpEffect_0 != null && bool_3)
		{
			float_0 += Time.deltaTime;
			if (float_0 > chartItemMaterialLerpEffect_0.LerpTime)
			{
				method_5(color_0);
			}
			else
			{
				method_5(Color.Lerp(color_1, color_0, float_0 / chartItemMaterialLerpEffect_0.LerpTime));
			}
		}
	}

	public Color method_3(Material m)
	{
		if (m.HasProperty(method_2()))
		{
			return m.GetColor(method_2());
		}
		return m.color;
	}

	public void method_4(Material m, Color c)
	{
		if (m.HasProperty(method_2()))
		{
			m.SetColor(method_2(), c);
		}
		else
		{
			m.color = c;
		}
	}

	public void method_5(Color c)
	{
		if (renderer_0 == null)
		{
			renderer_0 = GetComponent<Renderer>();
		}
		if (renderer_0 != null)
		{
			if (c == method_3(materials.Normal) && !bool_1 && !bool_2)
			{
				GClass1664.SafeDestroy(renderer_0.material);
				renderer_0.material = null;
				renderer_0.sharedMaterial = materials.Normal;
				bool_3 = false;
			}
			else
			{
				method_4(renderer_0.material, c);
			}
			return;
		}
		if (graphic_0 == null)
		{
			graphic_0 = GetComponent<Graphic>();
		}
		if (graphic_0 != null)
		{
			if (material_0 == null)
			{
				material_0 = new Material(materials.Normal);
				material_0.hideFlags = HideFlags.DontSave;
				graphic_0.material = material_0;
			}
			if (graphic_0.material != material_0)
			{
				graphic_0.material = material_0;
			}
			method_4(material_0, c);
			if (c == method_3(materials.Normal) && !bool_1 && !bool_2)
			{
				bool_3 = false;
				graphic_0.material = materials.Normal;
			}
		}
	}

	public void method_6(Color c)
	{
		if (GClass1664.Boolean_0)
		{
			return;
		}
		if (c == Color.clear)
		{
			c = method_3(materials.Normal);
		}
		if (chartItemMaterialLerpEffect_0 == null)
		{
			method_5(c);
			return;
		}
		if (material_0 == null)
		{
			material_0 = new Material(materials.Normal);
			material_0.hideFlags = HideFlags.DontSave;
			if (graphic_0 != null)
			{
				graphic_0.material = material_0;
			}
		}
		if (renderer_0 != null)
		{
			color_1 = method_3(renderer_0.material);
		}
		else if (graphic_0 != null && material_0 != null)
		{
			color_1 = method_3(material_0);
		}
		else
		{
			color_1 = method_3(materials.Normal);
		}
		color_0 = c;
		bool_3 = true;
		float_0 = 0f;
	}

	public void OnDestroy()
	{
		if (renderer_0 != null)
		{
			GClass1664.SafeDestroy(renderer_0.material);
		}
		GClass1664.SafeDestroy(material_0);
	}

	public void Refresh()
	{
		if (ChartDynamicMaterial_0 != null && !(materials.Normal == null))
		{
			if (!bool_1)
			{
				method_6(Color.clear);
			}
			else if (bool_2 && !(ChartDynamicMaterial_0.Selected == Color.clear))
			{
				method_6(materials.Selected);
			}
			else
			{
				method_6(materials.Hover);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnMouseEnter();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnMouseExit();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		OnMouseDown();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		OnMouseUp();
	}
}
