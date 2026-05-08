using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph.Axis;

[RequireComponent(typeof(CanvasRenderer))]
[ExecuteInEditMode]
public class CanvasAxisGenerator : Image, GInterface164
{
	private MeshRenderer meshRenderer_0;

	private MeshFilter meshFilter_0;

	private Mesh mesh_0;

	private AxisBase axisBase_0;

	private AnyChart anyChart_0;

	private ChartOrientation chartOrientation_0;

	private bool bool_0;

	private Material material_0;

	private Material material_1;

	private Class1082 class1082_0;

	private float float_0 = 1f;

	private double double_0;

	private static readonly int int_0 = Shader.PropertyToID("_ChartTiling");

	public override void OnPopulateMesh(VertexHelper vh)
	{
		base.OnPopulateMesh(vh);
		vh.Clear();
		if (!(axisBase_0 == null) && !(anyChart_0 == null))
		{
			Class1082 mesh = new Class1082(vh);
			method_0(mesh);
		}
	}

	public void method_0(Class1082 mesh)
	{
		mesh.Orientation = chartOrientation_0;
		if (bool_0)
		{
			axisBase_0.method_8(double_0, anyChart_0, base.transform, mesh, chartOrientation_0);
		}
		else
		{
			axisBase_0.method_10(double_0, anyChart_0, base.transform, mesh, chartOrientation_0);
		}
	}

	public override void OnPopulateMesh(Mesh m)
	{
		m.Clear();
		if (!(axisBase_0 == null) && !(anyChart_0 == null))
		{
			GClass1670 gClass = new GClass1670(isCanvas: true);
			gClass.Orientation = chartOrientation_0;
			if (bool_0)
			{
				axisBase_0.method_10(double_0, anyChart_0, base.transform, gClass, chartOrientation_0);
			}
			else
			{
				axisBase_0.method_8(double_0, anyChart_0, base.transform, gClass, chartOrientation_0);
			}
			gClass.ApplyToMesh(m);
		}
	}

	public override void UpdateMaterial()
	{
		base.UpdateMaterial();
		if (!(material == null))
		{
			if (material.mainTexture != null)
			{
				base.canvasRenderer.SetTexture(material.mainTexture);
			}
			base.canvasRenderer.SetColor(Color.white);
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (material_0 != null)
		{
			GClass1664.SafeDestroy(material_0);
		}
	}

	public float method_1(MaterialTiling tiling)
	{
		if (tiling.EnableTiling && tiling.TileFactor > 0f)
		{
			ChartDivisionInfo info = axisBase_0.MainDivisions;
			if (bool_0)
			{
				info = axisBase_0.SubDivisions;
			}
			return GClass1664.smethod_6(anyChart_0, chartOrientation_0, info) / tiling.TileFactor;
		}
		return 1f;
	}

	public void SetAxis(double scrollOffset, AnyChart parent, AxisBase axis, ChartOrientation axisOrientation, bool isSubDivisions)
	{
		double_0 = scrollOffset;
		raycastTarget = false;
		color = Color.white;
		axisBase_0 = axis;
		anyChart_0 = parent;
		bool_0 = isSubDivisions;
		chartOrientation_0 = axisOrientation;
		if (class1082_0 == null)
		{
			class1082_0 = new Class1082(forText: true);
			class1082_0.RecycleText = true;
		}
		class1082_0.Clear();
		if (bool_0)
		{
			axisBase_0.method_10(double_0, anyChart_0, base.transform, class1082_0, chartOrientation_0);
		}
		else
		{
			axisBase_0.method_8(double_0, anyChart_0, base.transform, class1082_0, chartOrientation_0);
		}
		if (class1082_0.TextObjects != null)
		{
			foreach (BillboardText textObject in class1082_0.TextObjects)
			{
				((IInternalUse)parent).InternalTextController.AddText(textObject);
			}
		}
		base.canvasRenderer.materialCount = 1;
		if (material_0 != null)
		{
			GClass1664.SafeDestroy(material_0);
		}
		float value = 1f;
		if (isSubDivisions)
		{
			if (axis.SubDivisions.Material != null)
			{
				material_0 = new Material(material_1 = axis.SubDivisions.Material);
				material_0.hideFlags = HideFlags.DontSave;
				material = material_0;
				value = method_1(axis.SubDivisions.MaterialTiling);
			}
		}
		else if (axis.MainDivisions.Material != null)
		{
			material_0 = new Material(material_1 = axis.MainDivisions.Material);
			material_0.hideFlags = HideFlags.DontSave;
			material = material_0;
			value = method_1(axis.MainDivisions.MaterialTiling);
		}
		float_0 = value;
		if (material_0 != null && material_0.HasProperty("_ChartTiling"))
		{
			material_0.SetFloat(int_0, value);
		}
		SetAllDirty();
		Rebuild(CanvasUpdate.PreRender);
		class1082_0.DestoryRecycled();
	}

	public virtual void Update()
	{
		if (material_1 != null && material_0 != null && material_0.HasProperty("_ChartTiling"))
		{
			if (material_0 != material_1)
			{
				material_0.CopyPropertiesFromMaterial(material_1);
			}
			material_0.SetFloat(int_0, float_0);
		}
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public Object This()
	{
		return this;
	}
}
