using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerShaderBindings : ScriptableObject
{
	[CompilerGenerated]
	public class Class877
	{
		public string extensionCode;

		public bool method_0(GInterface117 ex)
		{
			return ex.GetExtensionCode().Equals(extensionCode);
		}
	}

	[CompilerGenerated]
	public class Class878
	{
		public GInterface117 extension;

		public bool method_0(GInterface117 ex)
		{
			return ex.GetExtensionCode().Equals(extension.GetExtensionCode());
		}
	}

	public List<ShaderInstance> shaderInstances;

	private static readonly List<string> list_0 = new List<string>
	{
		GClass1262.SHADER_UNITY_STANDARD,
		GClass1262.SHADER_UNITY_STANDARD_SPECULAR,
		GClass1262.SHADER_UNITY_STANDARD_ROUGHNESS,
		GClass1262.SHADER_UNITY_VERTEXLIT,
		GClass1262.SHADER_UNITY_SPEED_TREE,
		GClass1262.SHADER_UNITY_SPEED_TREE_8,
		GClass1262.SHADER_UNITY_TREE_CREATOR_BARK,
		GClass1262.SHADER_UNITY_TREE_CREATOR_BARK_OPTIMIZED,
		GClass1262.SHADER_UNITY_TREE_CREATOR_LEAVES,
		GClass1262.SHADER_UNITY_TREE_CREATOR_LEAVES_OPTIMIZED,
		GClass1262.SHADER_UNITY_TREE_CREATOR_LEAVES_FAST,
		GClass1262.SHADER_UNITY_TREE_CREATOR_LEAVES_FAST_OPTIMIZED,
		GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_BARK,
		GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_LEAVES
	};

	private static readonly List<string> list_1 = new List<string>
	{
		GClass1262.SHADER_GPUI_STANDARD,
		GClass1262.SHADER_GPUI_STANDARD_SPECULAR,
		GClass1262.SHADER_GPUI_STANDARD_ROUGHNESS,
		GClass1262.SHADER_GPUI_VERTEXLIT,
		GClass1262.SHADER_GPUI_SPEED_TREE,
		GClass1262.SHADER_GPUI_SPEED_TREE_8,
		GClass1262.SHADER_GPUI_TREE_CREATOR_BARK,
		GClass1262.SHADER_GPUI_TREE_CREATOR_BARK_OPTIMIZED,
		GClass1262.SHADER_GPUI_TREE_CREATOR_LEAVES,
		GClass1262.SHADER_GPUI_TREE_CREATOR_LEAVES_OPTIMIZED,
		GClass1262.SHADER_GPUI_TREE_CREATOR_LEAVES_FAST,
		GClass1262.SHADER_GPUI_TREE_CREATOR_LEAVES_FAST_OPTIMIZED,
		GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_BARK,
		GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_LEAVES
	};

	private static readonly List<string> list_2 = new List<string>
	{
		GClass1262.SHADER_GPUI_FOLIAGE,
		GClass1262.SHADER_GPUI_FOLIAGE_LWRP,
		GClass1262.SHADER_GPUI_SHADOWS_ONLY,
		GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_TREE,
		GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_TREECREATOR,
		GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_SOFTOCCLUSION,
		GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_STANDARD,
		GClass1262.SHADER_GPUI_HIZ_OCCLUSION_GENERATOR,
		GClass1262.SHADER_GPUI_TREE_PROXY
	};

	public List<GInterface117> shaderBindingsExtensions;

	public virtual bool HasExtension(string extensionCode)
	{
		if (shaderBindingsExtensions == null)
		{
			return false;
		}
		return shaderBindingsExtensions.Exists((GInterface117 ex) => ex.GetExtensionCode().Equals(extensionCode));
	}

	public virtual void AddExtension(GInterface117 extension)
	{
		if (shaderBindingsExtensions == null)
		{
			shaderBindingsExtensions = new List<GInterface117>();
		}
		if (!shaderBindingsExtensions.Exists((GInterface117 ex) => ex.GetExtensionCode().Equals(extension.GetExtensionCode())))
		{
			shaderBindingsExtensions.Add(extension);
		}
	}

	public virtual GInterface117 GetExtension(string extensionCode)
	{
		if (shaderBindingsExtensions != null && shaderBindingsExtensions.Count > 0)
		{
			foreach (GInterface117 shaderBindingsExtension in shaderBindingsExtensions)
			{
				if (shaderBindingsExtension.GetExtensionCode().Equals(extensionCode))
				{
					return shaderBindingsExtension;
				}
			}
		}
		Debug.LogError("GPU Instancer Shader Bindings Extension can not be found. ExtensionCode: " + extensionCode);
		return null;
	}

	public virtual Shader GetInstancedShader(string shaderName, string extensionCode = null)
	{
		if (!string.IsNullOrEmpty(extensionCode))
		{
			return GetExtension(extensionCode)?.GetInstancedShader(shaderInstances, shaderName);
		}
		if (string.IsNullOrEmpty(shaderName))
		{
			return null;
		}
		if (shaderInstances == null)
		{
			shaderInstances = new List<ShaderInstance>();
		}
		foreach (ShaderInstance shaderInstance in shaderInstances)
		{
			if (shaderInstance.name.Equals(shaderName) && string.IsNullOrEmpty(shaderInstance.extensionCode))
			{
				return shaderInstance.instancedShader;
			}
		}
		if (list_0.Contains(shaderName))
		{
			return Shader.Find(list_1[list_0.IndexOf(shaderName)]);
		}
		if (list_1.Contains(shaderName))
		{
			return Shader.Find(shaderName);
		}
		if (list_2.Contains(shaderName))
		{
			return Shader.Find(shaderName);
		}
		if (!shaderName.Equals(GClass1262.SHADER_UNITY_STANDARD))
		{
			if (Application.isPlaying)
			{
				Debug.LogWarning("Can not find instanced shader for : " + shaderName + ". Using Standard shader instead.");
			}
			return GetInstancedShader(GClass1262.SHADER_UNITY_STANDARD);
		}
		Debug.LogWarning("Can not find instanced shader for : " + shaderName);
		return null;
	}

	public virtual Material GetInstancedMaterial(Material originalMaterial, string extensionCode = null)
	{
		if (!string.IsNullOrEmpty(extensionCode))
		{
			return GetExtension(extensionCode)?.GetInstancedMaterial(shaderInstances, originalMaterial);
		}
		if (!(originalMaterial == null) && !(originalMaterial.shader == null))
		{
			Material material = new Material(GetInstancedShader(originalMaterial.shader.name));
			material.CopyPropertiesFromMaterial(originalMaterial);
			material.name = originalMaterial.name + "_GPUI";
			return material;
		}
		Debug.LogWarning("One of the GPU Instancer prototypes is missing material reference! Check the Material references in MeshRenderer.");
		return new Material(GetInstancedShader(GClass1262.SHADER_UNITY_STANDARD));
	}

	public virtual void ResetShaderInstances()
	{
		if (shaderInstances == null)
		{
			shaderInstances = new List<ShaderInstance>();
		}
		else
		{
			shaderInstances.Clear();
		}
	}

	public virtual void ClearEmptyShaderInstances()
	{
	}

	public virtual void AddShaderInstance(string name, Shader instancedShader, bool isOriginalInstanced = false, string extensionCode = null)
	{
		shaderInstances.Add(new ShaderInstance(name, instancedShader, isOriginalInstanced, extensionCode));
	}

	public virtual bool IsShadersInstancedVersionExists(string shaderName, string extensionCode = null)
	{
		if (!string.IsNullOrEmpty(extensionCode))
		{
			return GetExtension(extensionCode)?.IsShadersInstancedVersionExists(shaderInstances, shaderName) ?? false;
		}
		if (!list_0.Contains(shaderName) && !list_1.Contains(shaderName) && !list_2.Contains(shaderName))
		{
			foreach (ShaderInstance shaderInstance in shaderInstances)
			{
				if (shaderInstance.name.Equals(shaderName) && string.IsNullOrEmpty(shaderInstance.extensionCode))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}
}
