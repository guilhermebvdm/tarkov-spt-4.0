using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1224
{
	public const int MaxRenderers = 65535;

	public const int SampleBatchCount = 2048;

	public const int CamBakeLayer = 30;

	public const bool SafetyChecks = true;

	public static readonly HashSet<Type> SupportedRendererTypes = new HashSet<Type>
	{
		typeof(MeshRenderer),
		typeof(SkinnedMeshRenderer)
	};

	public static Color ClearColor = Color.black;

	public static bool AllowSceneReload = true;

	public static bool EFTSceneBakeMode = true;

	public static readonly string MultiSceneTempPath = "Assets/PerfectCulling_Temp.unity";

	public static readonly string ResourcesFolder = "Perfect Culling";
}
