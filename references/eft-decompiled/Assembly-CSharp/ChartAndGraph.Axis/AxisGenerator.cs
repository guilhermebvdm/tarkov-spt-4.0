using System;
using UnityEngine;

namespace ChartAndGraph.Axis;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class AxisGenerator : MonoBehaviour, GInterface164
{
	private MeshRenderer meshRenderer_0;

	private MeshFilter meshFilter_0;

	private Mesh mesh_0;

	private AxisBase axisBase_0;

	private Material material_0;

	private Material material_1;

	private float float_0 = 1f;

	private GClass1670 gclass1670_0;

	private Mesh mesh_1;

	private static readonly int int_0 = Shader.PropertyToID("_ChartTiling");

	public void OnDestroy()
	{
		GClass1664.smethod_13(null, ref mesh_0);
		GClass1664.SafeDestroy(material_0);
	}

	public float method_0(AnyChart parent, ChartOrientation orientation, ChartDivisionInfo inf)
	{
		MaterialTiling materialTiling = inf.MaterialTiling;
		if (materialTiling.EnableTiling && materialTiling.TileFactor > 0f)
		{
			float num = Math.Abs(GClass1664.smethod_6(parent, orientation, inf));
			float num2 = GClass1664.smethod_4(parent, orientation);
			float num3 = GClass1664.smethod_5(parent, orientation, inf);
			if (!inf.MarkBackLength.Automatic)
			{
				num2 = inf.MarkBackLength.Value;
			}
			if (num2 != 0f && num3 > 0f)
			{
				num += Math.Abs(num2) + Math.Abs(num3);
			}
			return num / materialTiling.TileFactor;
		}
		return 1f;
	}

	public void SetAxis(double scrollOffset, AnyChart parent, AxisBase axis, ChartOrientation axisOrientation, bool isSubDivisions)
	{
		axisBase_0 = axis;
		if (gclass1670_0 == null)
		{
			gclass1670_0 = new GClass1670(2);
			gclass1670_0.RecycleText = true;
		}
		gclass1670_0.Clear();
		gclass1670_0.Orientation = axisOrientation;
		axisBase_0 = axis;
		if (isSubDivisions)
		{
			axis.method_8(scrollOffset, parent, base.transform, gclass1670_0, axisOrientation);
		}
		else
		{
			axis.method_10(scrollOffset, parent, base.transform, gclass1670_0, axisOrientation);
		}
		if (gclass1670_0.TextObjects != null)
		{
			foreach (BillboardText textObject in gclass1670_0.TextObjects)
			{
				((IInternalUse)parent).InternalTextController.AddText(textObject);
			}
		}
		Mesh mesh = (mesh_1 = gclass1670_0.Generate(mesh_1));
		mesh.hideFlags = HideFlags.DontSave;
		if (meshFilter_0 == null)
		{
			meshFilter_0 = GetComponent<MeshFilter>();
		}
		meshFilter_0.sharedMesh = mesh;
		MeshCollider component = GetComponent<MeshCollider>();
		if (component != null)
		{
			component.sharedMesh = mesh;
		}
		GClass1664.smethod_13(mesh, ref mesh_0);
		MeshRenderer component2 = GetComponent<MeshRenderer>();
		if (component2 != null)
		{
			Material material = axisBase_0.MainDivisions.Material;
			float num = method_0(parent, axisOrientation, axisBase_0.MainDivisions);
			if (isSubDivisions)
			{
				material = axisBase_0.SubDivisions.Material;
				num = method_0(parent, axisOrientation, axisBase_0.SubDivisions);
			}
			material_1 = material;
			if (material != null)
			{
				GClass1664.SafeDestroy(material_0);
				material_0 = new Material(material);
				material_0.hideFlags = HideFlags.DontSave;
				component2.sharedMaterial = material_0;
				float_0 = num;
				if (material_0.HasProperty("_ChartTiling"))
				{
					material_0.SetFloat(int_0, float_0);
				}
			}
		}
		gclass1670_0.DestoryRecycled();
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

	public UnityEngine.Object This()
	{
		return this;
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}
}
