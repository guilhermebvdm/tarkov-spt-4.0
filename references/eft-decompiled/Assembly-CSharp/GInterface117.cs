using System.Collections.Generic;
using GPUInstancer;
using UnityEngine;

public interface GInterface117
{
	Shader GetInstancedShader(List<ShaderInstance> shaderInstances, string shaderName);

	Material GetInstancedMaterial(List<ShaderInstance> shaderInstances, Material originalMaterial);

	bool ClearEmptyShaderInstances(List<ShaderInstance> shaderInstances);

	bool IsShadersInstancedVersionExists(List<ShaderInstance> shaderInstances, string shaderName);

	string GetExtensionCode();
}
