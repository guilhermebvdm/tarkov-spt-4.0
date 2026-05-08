using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass933 : GInterface40
{
	public struct Struct133
	{
		public Renderer Renderer;

		public int MaterialIndex;

		public Shader OriginalShader;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class574
	{
		public static readonly Class574 class574_0 = new Class574();

		public static Func<Renderer, bool> func_0;

		public bool method_0(Renderer renderer)
		{
			return renderer.enabled;
		}
	}

	[NonSerialized]
	public List<Struct133> List_0 = new List<Struct133>();

	public void Replace(GameObject model)
	{
		List_0.Clear();
		foreach (Renderer item in from renderer in model.GetComponentsInChildren<Renderer>()
			where renderer.enabled
			select renderer)
		{
			for (int num = 0; num < item.sharedMaterials.Length; num++)
			{
				if (!(item.sharedMaterials[num] == null) && !(item.sharedMaterials[num].shader.name == "Diffuse") && !(item.sharedMaterials[num].shader.name == "Standard") && !item.sharedMaterials[num].shader.name.Contains("_Icon"))
				{
					Material material = item.materials[num];
					List_0.Add(new Struct133
					{
						Renderer = item,
						MaterialIndex = num,
						OriginalShader = material.shader
					});
					Shader shader = GClass872.Find(material.shader.name + "_Icon");
					material.shader = ((shader != null) ? shader : GClass872.Find("p0/Reflective/Bumped Specular SMap_Icon"));
				}
			}
		}
	}

	public void Restore()
	{
		foreach (Struct133 item in List_0)
		{
			item.Renderer.materials[item.MaterialIndex].shader = item.OriginalShader;
		}
	}
}
