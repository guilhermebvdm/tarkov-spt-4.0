using System;
using System.Globalization;
using UnityEngine;

namespace GPUInstancer;

[Serializable]
public class ShaderInstance
{
	public string name;

	public Shader instancedShader;

	public string modifiedDate;

	public bool isOriginalInstanced;

	public string extensionCode;

	public ShaderInstance(string name, Shader instancedShader, bool isOriginalInstanced, string extensionCode = null)
	{
		this.name = name;
		this.instancedShader = instancedShader;
		modifiedDate = EFTDateTimeClass.Now.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
		this.isOriginalInstanced = isOriginalInstanced;
		this.extensionCode = extensionCode;
	}

	public virtual void Regenerate()
	{
		if (isOriginalInstanced)
		{
			instancedShader = GClass1274.CreateInstancedShader(instancedShader, useOriginal: true);
			return;
		}
		Shader shader = Shader.Find(name);
		if (shader != null)
		{
			instancedShader = GClass1274.CreateInstancedShader(shader);
			modifiedDate = EFTDateTimeClass.Now.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
		}
	}
}
