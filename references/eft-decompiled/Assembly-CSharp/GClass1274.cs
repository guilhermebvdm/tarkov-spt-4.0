using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using GPUInstancer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public abstract class GClass1274
{
	[Serializable]
	[CompilerGenerated]
	public class Class879
	{
		public static readonly Class879 class879_0 = new Class879();

		public static Func<MeshRenderer, bool> func_0;

		public static Func<MeshRenderer, bool> func_1;

		public static Func<Material, bool> func_2;

		public static Func<MeshRenderer, bool> func_3;

		public static Func<Renderer, bool> func_4;

		public static Func<Renderer, bool> func_5;

		public static Func<MeshRenderer, bool> func_6;

		public bool method_0(MeshRenderer r)
		{
			if (!(r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE))
			{
				return r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE_LWRP;
			}
			return true;
		}

		public bool method_1(MeshRenderer r)
		{
			if (!(r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE))
			{
				return r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE_LWRP;
			}
			return true;
		}

		public bool method_2(MeshRenderer mr)
		{
			return mr.sharedMaterials.Where((Material m) => m.shader.name == GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_BARK || m.shader.name == GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_BARK || m.shader.name == GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_LEAVES || m.shader.name == GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_LEAVES).FirstOrDefault();
		}

		public bool method_3(Material m)
		{
			if (!(m.shader.name == GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_BARK) && !(m.shader.name == GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_BARK) && !(m.shader.name == GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_LEAVES))
			{
				return m.shader.name == GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_LEAVES;
			}
			return true;
		}

		public bool method_4(Renderer r)
		{
			return r.GetComponent<BillboardRenderer>() != null;
		}

		public bool method_5(Renderer r)
		{
			return r.sharedMaterials[0].IsKeywordEnabled("EFFECT_BILLBOARD");
		}

		public bool method_6(MeshRenderer r)
		{
			if (r.sharedMaterials != null && r.sharedMaterials.Length != 0)
			{
				if (!(r.sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE) && !(r.sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE) && !(r.sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE_8))
				{
					return r.sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE_8;
				}
				return true;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class880
	{
		public GClass1273<GClass1258> spData;

		public bool method_0(int d)
		{
			return d > spData.cellRowAndCollumnCountPerTerrain;
		}
	}

	[CompilerGenerated]
	public class Class881
	{
		public GameObject go;

		public bool method_0(GPUInstancerPrototype p)
		{
			return p.prefabObject == go;
		}
	}

	[CompilerGenerated]
	public class Class882
	{
		public int n;

		public bool method_0(int a)
		{
			return n % a == 0;
		}
	}

	[NonSerialized]
	public static int Int_0 = Shader.PropertyToID("_HealthyDryNoiseTexture");

	[NonSerialized]
	public static int Int_1 = Shader.PropertyToID("_NoiseSpread");

	[NonSerialized]
	public static int Int_2 = Shader.PropertyToID("_WindWaveNormalTexture");

	[NonSerialized]
	public static int Int_3 = Shader.PropertyToID("_WindVector");

	[NonSerialized]
	public static int Int_4 = Shader.PropertyToID("_HeightMap");

	[NonSerialized]
	public static int Int_5 = Shader.PropertyToID("_HeightResolution");

	[NonSerialized]
	public static int Int_6 = Shader.PropertyToID("_TerrainWorldPos");

	[NonSerialized]
	public static int Int_7 = Shader.PropertyToID("_TerrainSize");

	[NonSerialized]
	public static int Int_8 = Shader.PropertyToID("_TerrainNormalMap");

	[NonSerialized]
	public static int Int_9 = Shader.PropertyToID("_GradientNormalHeight");

	[NonSerialized]
	public static int Int_10 = Shader.PropertyToID("_NormalMap");

	[NonSerialized]
	public static int Int_11 = Shader.PropertyToID("_AlphaMapArray");

	[NonSerialized]
	public static int Int_12 = Shader.PropertyToID("_AlphaChannelMask");

	[NonSerialized]
	public static int Int_13 = Shader.PropertyToID("_DensityMapArray");

	[NonSerialized]
	public static int Int_14 = Shader.PropertyToID("_DensityMapIndex");

	[NonSerialized]
	public static int Int_15 = Shader.PropertyToID("_DensityChanelMask");

	[NonSerialized]
	public static int Int_16 = Shader.PropertyToID("_DensityMinMax");

	[NonSerialized]
	public static int Int_17 = Shader.PropertyToID("_CutoffFade");

	[NonSerialized]
	public static int Int_18 = Shader.PropertyToID("_AmbientOcclusion");

	[NonSerialized]
	public static int Int_19 = Shader.PropertyToID("_GradientPower");

	[NonSerialized]
	public static int Int_20 = Shader.PropertyToID("_WindWaveTintColor");

	[NonSerialized]
	public static int Int_21 = Shader.PropertyToID("_WindIdleSway");

	[NonSerialized]
	public static int Int_22 = Shader.PropertyToID("_WindWavesOn");

	[NonSerialized]
	public static int Int_23 = Shader.PropertyToID("_WindWaveSize");

	[NonSerialized]
	public static int Int_24 = Shader.PropertyToID("_WindWaveTint");

	[NonSerialized]
	public static int Int_25 = Shader.PropertyToID("_WindWaveSway");

	[NonSerialized]
	public static int Int_26 = Shader.PropertyToID("_IsBillboard");

	[NonSerialized]
	public static int Int_27 = Shader.PropertyToID("_MainTex");

	[NonSerialized]
	public static int Int_28 = Shader.PropertyToID("_HealthyColor");

	[NonSerialized]
	public static int Int_29 = Shader.PropertyToID("_DryColor");

	[NonSerialized]
	public static int Int_30 = Shader.PropertyToID("_MainTexture");

	[NonSerialized]
	public static int Int_31 = Shader.PropertyToID("_GPUIBillboardBrightness");

	[NonSerialized]
	public static int Int_32 = Shader.PropertyToID("_GPUIBillboardCutoffOverride");

	[NonSerialized]
	public static int Int_33 = Shader.PropertyToID("_IsLinearSpace");

	[NonSerialized]
	public static int Int_34 = Shader.PropertyToID("_UseSPDHueVariation");

	[NonSerialized]
	public static int Int_35 = Shader.PropertyToID("_SPDHueVariation");

	[NonSerialized]
	public static int Int_36 = Shader.PropertyToID("_HueVariation");

	[NonSerialized]
	public static int Int_37 = Shader.PropertyToID("_HueVariationColor");

	[NonSerialized]
	public static int Int_38 = Shader.PropertyToID("_TranslucencyColor");

	[NonSerialized]
	public static int Int_39 = Shader.PropertyToID("_TranslucencyViewDependency");

	[NonSerialized]
	public static int Int_40 = Shader.PropertyToID("_ShadowStrength");

	[NonSerialized]
	public static int Int_41 = Shader.PropertyToID("_AlbedoAtlas");

	[NonSerialized]
	public static int Int_42 = Shader.PropertyToID("_NormalAtlas");

	[NonSerialized]
	public static int Int_43 = Shader.PropertyToID("_FrameCount");

	[NonSerialized]
	public static int Int_44 = Shader.PropertyToID("_CutOff");

	[NonSerialized]
	public static int Int_45 = Shader.PropertyToID("bufferSize");

	[NonSerialized]
	public static int Int_46 = Shader.PropertyToID("maxTextureSize");

	public static Texture2D dummyHiZTex;

	public static GPUIMatrixHandlingType matrixHandlingType;

	[NonSerialized]
	public static Dictionary<GPUInstancerEventType, UnityEvent> Dictionary_0;

	public static void InitializeGPUBuffers<T>(List<T> runtimeDataList) where T : GClass1270
	{
		if (runtimeDataList != null && runtimeDataList.Count != 0)
		{
			for (int i = 0; i < runtimeDataList.Count; i++)
			{
				InitializeGPUBuffer(runtimeDataList[i]);
			}
		}
	}

	public static void InitializeGPUBuffer<T>(T runtimeData) where T : GClass1270
	{
		if (runtimeData == null || runtimeData.bufferSize == 0)
		{
			return;
		}
		if (runtimeData.instanceLODs != null && runtimeData.instanceLODs.Count != 0)
		{
			if (dummyHiZTex == null)
			{
				dummyHiZTex = new Texture2D(1, 1);
			}
			if (runtimeData.transformationMatrixVisibilityBuffer == null || runtimeData.transformationMatrixVisibilityBuffer.count != runtimeData.bufferSize)
			{
				runtimeData.transformationMatrixVisibilityBuffer?.Release();
				runtimeData.transformationMatrixVisibilityBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_MATRIX4X4);
				if (runtimeData.instanceDataArray != null)
				{
					runtimeData.transformationMatrixVisibilityBuffer.SetData(runtimeData.instanceDataArray);
				}
			}
			if (runtimeData.instanceLODDataBuffer == null || runtimeData.instanceLODDataBuffer.count != runtimeData.bufferSize)
			{
				runtimeData.instanceLODDataBuffer?.Release();
				runtimeData.instanceLODDataBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_FLOAT4);
			}
			if (runtimeData.argsBuffer == null)
			{
				int num = 0;
				for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
				{
					for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
					{
						num += runtimeData.instanceLODs[i].renderers[j].mesh.subMeshCount;
					}
				}
				runtimeData.args = new uint[5 * num];
				int argsBufferOffset = 0;
				for (int k = 0; k < runtimeData.instanceLODs.Count; k++)
				{
					for (int l = 0; l < runtimeData.instanceLODs[k].renderers.Count; l++)
					{
						runtimeData.instanceLODs[k].renderers[l].argsBufferOffset = argsBufferOffset;
						for (int m = 0; m < runtimeData.instanceLODs[k].renderers[l].mesh.subMeshCount; m++)
						{
							runtimeData.args[argsBufferOffset++] = runtimeData.instanceLODs[k].renderers[l].mesh.GetIndexCount(m);
							runtimeData.args[argsBufferOffset++] = 0u;
							runtimeData.args[argsBufferOffset++] = runtimeData.instanceLODs[k].renderers[l].mesh.GetIndexStart(m);
							runtimeData.args[argsBufferOffset++] = 0u;
							runtimeData.args[argsBufferOffset++] = 0u;
						}
					}
				}
				if (runtimeData.args.Length != 0)
				{
					runtimeData.argsBuffer = new ComputeBuffer(runtimeData.args.Length, 4, ComputeBufferType.DrawIndirect);
					runtimeData.argsBuffer.SetData(runtimeData.args);
					if (runtimeData.hasShadowCasterBuffer)
					{
						runtimeData.shadowArgs = runtimeData.args.ToArray();
						runtimeData.shadowArgsBuffer?.Release();
						runtimeData.shadowArgsBuffer = new ComputeBuffer(runtimeData.args.Length, 4, ComputeBufferType.DrawIndirect);
						runtimeData.shadowArgsBuffer.SetData(runtimeData.args);
					}
				}
			}
			SetAppendBuffers(runtimeData);
			runtimeData.InitializeData();
		}
		else
		{
			Debug.LogError("instance prototype with an empty LOD list detected. There must be at least one LOD defined per instance prototype.");
		}
	}

	public static void SetAppendBuffers<T>(T runtimeData) where T : GClass1270
	{
		switch (matrixHandlingType)
		{
		default:
			smethod_0(runtimeData);
			break;
		case GPUIMatrixHandlingType.CopyToTexture:
			smethod_2(runtimeData);
			break;
		case GPUIMatrixHandlingType.MatrixAppend:
			smethod_1(runtimeData);
			break;
		}
	}

	public static void smethod_0<T>(T runtimeData) where T : GClass1270
	{
		int num = 0;
		foreach (GClass1271 instanceLOD in runtimeData.instanceLODs)
		{
			if (instanceLOD.transformationMatrixAppendBuffer == null || instanceLOD.transformationMatrixAppendBuffer.count != runtimeData.bufferSize)
			{
				instanceLOD.transformationMatrixAppendBuffer?.Release();
				instanceLOD.transformationMatrixAppendBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_INT, ComputeBufferType.Append);
				if (runtimeData.hasShadowCasterBuffer)
				{
					instanceLOD.shadowAppendBuffer?.Release();
					instanceLOD.shadowAppendBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_INT, ComputeBufferType.Append);
				}
			}
			foreach (GClass1272 renderer in instanceLOD.renderers)
			{
				renderer.mpb.SetBuffer(GClass1262.GClass1264.TRANSFORMATION_MATRIX_BUFFER, instanceLOD.transformationMatrixAppendBuffer);
				renderer.mpb.SetBuffer(GClass1262.GClass1264.INSTANCE_DATA_BUFFER, runtimeData.transformationMatrixVisibilityBuffer);
				renderer.mpb.SetBuffer(GClass1262.GClass1264.INSTANCE_LOD_BUFFER, runtimeData.instanceLODDataBuffer);
				renderer.mpb.SetMatrix(GClass1262.GClass1264.RENDERER_TRANSFORM_OFFSET, renderer.transformOffset);
				renderer.mpb.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_LOD_LEVEL, runtimeData.prototype.isLODCrossFade ? num : (-1));
				if (runtimeData.prototype.isLODCrossFade)
				{
					if (runtimeData.prototype.isLODCrossFadeAnimate)
					{
						renderer.mpb.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_FADE_LEVEL_MULTIPLIER, 0.01f);
					}
					else
					{
						renderer.mpb.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_FADE_LEVEL_MULTIPLIER, 1f);
					}
				}
				if (runtimeData.hasShadowCasterBuffer)
				{
					renderer.shadowMPB.SetBuffer(GClass1262.GClass1264.TRANSFORMATION_MATRIX_BUFFER, instanceLOD.shadowAppendBuffer);
					renderer.shadowMPB.SetBuffer(GClass1262.GClass1264.INSTANCE_DATA_BUFFER, runtimeData.transformationMatrixVisibilityBuffer);
					renderer.shadowMPB.SetMatrix(GClass1262.GClass1264.RENDERER_TRANSFORM_OFFSET, renderer.transformOffset);
					renderer.shadowMPB.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_LOD_LEVEL, -1f);
				}
			}
			num++;
		}
	}

	public static void smethod_1<T>(T runtimeData) where T : GClass1270
	{
		for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
		{
			if (runtimeData.instanceLODs[i].transformationMatrixAppendBuffer == null || runtimeData.instanceLODs[i].transformationMatrixAppendBuffer.count != runtimeData.bufferSize)
			{
				if (runtimeData.instanceLODs[i].transformationMatrixAppendBuffer != null)
				{
					runtimeData.instanceLODs[i].transformationMatrixAppendBuffer.Release();
				}
				runtimeData.instanceLODs[i].transformationMatrixAppendBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_MATRIX4X4, ComputeBufferType.Append);
				if (runtimeData.hasShadowCasterBuffer)
				{
					if (runtimeData.instanceLODs[i].shadowAppendBuffer != null)
					{
						runtimeData.instanceLODs[i].shadowAppendBuffer.Release();
					}
					runtimeData.instanceLODs[i].shadowAppendBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_MATRIX4X4, ComputeBufferType.Append);
				}
			}
			for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
			{
				runtimeData.instanceLODs[i].renderers[j].mpb.SetBuffer(GClass1262.GClass1264.TRANSFORMATION_MATRIX_BUFFER, runtimeData.instanceLODs[i].transformationMatrixAppendBuffer);
				runtimeData.instanceLODs[i].renderers[j].mpb.SetMatrix(GClass1262.GClass1264.RENDERER_TRANSFORM_OFFSET, runtimeData.instanceLODs[i].renderers[j].transformOffset);
				if (runtimeData.hasShadowCasterBuffer)
				{
					runtimeData.instanceLODs[i].renderers[j].shadowMPB.SetBuffer(GClass1262.GClass1264.TRANSFORMATION_MATRIX_BUFFER, runtimeData.instanceLODs[i].shadowAppendBuffer);
					runtimeData.instanceLODs[i].renderers[j].shadowMPB.SetMatrix(GClass1262.GClass1264.RENDERER_TRANSFORM_OFFSET, runtimeData.instanceLODs[i].renderers[j].transformOffset);
				}
			}
		}
	}

	public static void smethod_2<T>(T runtimeData) where T : GClass1270
	{
		for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
		{
			if (runtimeData.instanceLODs[i].transformationMatrixAppendBuffer == null || runtimeData.instanceLODs[i].transformationMatrixAppendBuffer.count != runtimeData.bufferSize)
			{
				if (runtimeData.instanceLODs[i].transformationMatrixAppendBuffer != null)
				{
					runtimeData.instanceLODs[i].transformationMatrixAppendBuffer.Release();
				}
				runtimeData.instanceLODs[i].transformationMatrixAppendBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_INT, ComputeBufferType.Append);
			}
			if (runtimeData.instanceLODs[i].transformationMatrixAppendTexture == null || runtimeData.instanceLODs[i].transformationMatrixAppendTexture.width != runtimeData.bufferSize)
			{
				if (runtimeData.instanceLODs[i].transformationMatrixAppendTexture != null)
				{
					UnityEngine.Object.DestroyImmediate(runtimeData.instanceLODs[i].transformationMatrixAppendTexture);
				}
				int num = Mathf.CeilToInt((float)runtimeData.bufferSize / (float)GClass1262.TEXTURE_MAX_SIZE);
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture = new RenderTexture((num == 1) ? runtimeData.bufferSize : GClass1262.TEXTURE_MAX_SIZE, 4 * num, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture.isPowerOfTwo = false;
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture.enableRandomWrite = true;
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture.filterMode = FilterMode.Point;
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture.useMipMap = false;
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture.autoGenerateMips = false;
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture.Create();
			}
			if (runtimeData.hasShadowCasterBuffer)
			{
				if (runtimeData.instanceLODs[i].shadowAppendBuffer != null)
				{
					runtimeData.instanceLODs[i].shadowAppendBuffer.Release();
				}
				runtimeData.instanceLODs[i].shadowAppendBuffer = new ComputeBuffer(runtimeData.bufferSize, GClass1262.STRIDE_SIZE_INT, ComputeBufferType.Append);
				if (runtimeData.instanceLODs[i].shadowAppendTexture != null)
				{
					UnityEngine.Object.Destroy(runtimeData.instanceLODs[i].shadowAppendTexture);
				}
				int num2 = Mathf.CeilToInt((float)runtimeData.bufferSize / (float)GClass1262.TEXTURE_MAX_SIZE);
				runtimeData.instanceLODs[i].shadowAppendTexture = new RenderTexture((num2 == 1) ? runtimeData.bufferSize : GClass1262.TEXTURE_MAX_SIZE, 4 * num2, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
				runtimeData.instanceLODs[i].shadowAppendTexture.isPowerOfTwo = false;
				runtimeData.instanceLODs[i].shadowAppendTexture.enableRandomWrite = true;
				runtimeData.instanceLODs[i].shadowAppendTexture.filterMode = FilterMode.Point;
				runtimeData.instanceLODs[i].shadowAppendTexture.useMipMap = false;
				runtimeData.instanceLODs[i].shadowAppendTexture.autoGenerateMips = false;
				runtimeData.instanceLODs[i].shadowAppendTexture.Create();
			}
			for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
			{
				runtimeData.instanceLODs[i].renderers[j].mpb.SetTexture(GClass1262.GClass1263.TRANSFORMATION_MATRIX_TEXTURE, runtimeData.instanceLODs[i].transformationMatrixAppendTexture);
				runtimeData.instanceLODs[i].renderers[j].mpb.SetMatrix(GClass1262.GClass1264.RENDERER_TRANSFORM_OFFSET, runtimeData.instanceLODs[i].renderers[j].transformOffset);
				runtimeData.instanceLODs[i].renderers[j].mpb.SetFloat(Int_45, runtimeData.bufferSize);
				runtimeData.instanceLODs[i].renderers[j].mpb.SetFloat(Int_46, GClass1262.TEXTURE_MAX_SIZE);
				if (runtimeData.hasShadowCasterBuffer)
				{
					runtimeData.instanceLODs[i].renderers[j].shadowMPB.SetTexture(GClass1262.GClass1263.TRANSFORMATION_MATRIX_TEXTURE, runtimeData.instanceLODs[i].shadowAppendTexture);
					runtimeData.instanceLODs[i].renderers[j].shadowMPB.SetMatrix(GClass1262.GClass1264.RENDERER_TRANSFORM_OFFSET, runtimeData.instanceLODs[i].renderers[j].transformOffset);
					runtimeData.instanceLODs[i].renderers[j].shadowMPB.SetFloat(Int_45, runtimeData.bufferSize);
					runtimeData.instanceLODs[i].renderers[j].shadowMPB.SetFloat(Int_46, GClass1262.TEXTURE_MAX_SIZE);
				}
			}
		}
	}

	public static void UpdateGPUBuffers<T>(ComputeShader cameraComputeShader, int[] cameraComputeKernelIDs, ComputeShader visibilityComputeShader, int[] instanceVisibilityComputeKernelIDs, List<T> runtimeDataList, GPUInstancerTerrainSettings terrainSetting, GPUInstancerCameraData cameraData, bool isManagerFrustumCulling, bool isManagerOcclusionCulling, bool showRenderedAmount, bool isInitial) where T : GClass1270
	{
		if (runtimeDataList != null)
		{
			float frustumOffset = 0.6f;
			if (terrainSetting != null)
			{
				frustumOffset = terrainSetting.frustumOffset;
			}
			cameraData.CalculateAngularFrustumOffset(frustumOffset);
			for (int i = 0; i < runtimeDataList.Count; i++)
			{
				UpdateGPUBuffer(cameraComputeShader, cameraComputeKernelIDs, visibilityComputeShader, instanceVisibilityComputeKernelIDs, runtimeDataList[i], terrainSetting, cameraData, isManagerFrustumCulling, isManagerOcclusionCulling, showRenderedAmount, isInitial);
			}
		}
	}

	public static void UpdateGPUBuffer<T>(ComputeShader cameraComputeShader, int[] cameraComputeKernelIDs, ComputeShader visibilityComputeShader, int[] instanceVisibilityComputeKernelIDs, T runtimeData, GPUInstancerTerrainSettings terrainSetting, GPUInstancerCameraData cameraData, bool isManagerFrustumCulling, bool isManagerOcclusionCulling, bool showRenderedAmount, bool isInitial) where T : GClass1270
	{
		if (runtimeData == null)
		{
			return;
		}
		if (runtimeData.transformationMatrixVisibilityBuffer != null && runtimeData.bufferSize != 0 && runtimeData.instanceCount != 0)
		{
			bool flag = Singleton<SharedGameSettingsClass>.Instantiated && (bool)Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.GrassShadow;
			DispatchCSInstancedCameraCalculation(cameraComputeShader, cameraComputeKernelIDs, runtimeData, terrainSetting, cameraData, isManagerFrustumCulling, isManagerOcclusionCulling, isInitial);
			int count = runtimeData.instanceLODs.Count;
			int instanceVisibilityComputeKernelId = instanceVisibilityComputeKernelIDs[(count > GClass1262.COMPUTE_MAX_LOD_BUFFER) ? (GClass1262.COMPUTE_MAX_LOD_BUFFER - 1) : (count - 1)];
			DispatchCSInstancedVisibilityCalculation(visibilityComputeShader, instanceVisibilityComputeKernelId, runtimeData, isShadow: false, 0, 0);
			if (runtimeData.hasShadowCasterBuffer && flag)
			{
				DispatchCSInstancedVisibilityCalculation(visibilityComputeShader, instanceVisibilityComputeKernelId, runtimeData, isShadow: true, 0, 1);
			}
			if (!isInitial && runtimeData.prototype.isLODCrossFade)
			{
				DispatchCSInstancedVisibilityCalculation(visibilityComputeShader, instanceVisibilityComputeKernelId, runtimeData, isShadow: false, 0, 2);
			}
			if (count > GClass1262.COMPUTE_MAX_LOD_BUFFER)
			{
				instanceVisibilityComputeKernelId = instanceVisibilityComputeKernelIDs[count - GClass1262.COMPUTE_MAX_LOD_BUFFER - 1];
				DispatchCSInstancedVisibilityCalculation(visibilityComputeShader, instanceVisibilityComputeKernelId, runtimeData, isShadow: false, GClass1262.COMPUTE_MAX_LOD_BUFFER, 0);
				if (runtimeData.hasShadowCasterBuffer && flag)
				{
					DispatchCSInstancedVisibilityCalculation(visibilityComputeShader, instanceVisibilityComputeKernelId, runtimeData, isShadow: true, GClass1262.COMPUTE_MAX_LOD_BUFFER, 1);
				}
				if (!isInitial && runtimeData.prototype.isLODCrossFade)
				{
					DispatchCSInstancedVisibilityCalculation(visibilityComputeShader, instanceVisibilityComputeKernelId, runtimeData, isShadow: false, GClass1262.COMPUTE_MAX_LOD_BUFFER, 2);
				}
			}
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				GClass1271 gClass = runtimeData.instanceLODs[i];
				for (int j = 0; j < gClass.renderers.Count; j++)
				{
					GClass1272 gClass2 = gClass.renderers[j];
					for (int k = 0; k < gClass2.mesh.subMeshCount; k++)
					{
						num = gClass2.argsBufferOffset * GClass1262.STRIDE_SIZE_INT + k * GClass1262.STRIDE_SIZE_INT * 5 + GClass1262.STRIDE_SIZE_INT;
						ComputeBuffer.CopyCount(gClass.transformationMatrixAppendBuffer, runtimeData.argsBuffer, num);
						if (runtimeData.hasShadowCasterBuffer && flag)
						{
							ComputeBuffer.CopyCount(gClass.shadowAppendBuffer, runtimeData.shadowArgsBuffer, num);
						}
					}
				}
			}
			if (showRenderedAmount && runtimeData.argsBuffer != null && runtimeData.args != null && runtimeData.args.Length != 0)
			{
				runtimeData.argsBuffer.GetData(runtimeData.args);
				if (runtimeData.hasShadowCasterBuffer)
				{
					runtimeData.shadowArgsBuffer.GetData(runtimeData.shadowArgs);
				}
			}
		}
		else
		{
			if (!showRenderedAmount || runtimeData.args == null)
			{
				return;
			}
			for (int l = 0; l < runtimeData.instanceLODs.Count; l++)
			{
				runtimeData.args[runtimeData.instanceLODs[l].argsBufferOffset + 1] = 0u;
				if (runtimeData.hasShadowCasterBuffer && runtimeData.shadowArgs != null)
				{
					runtimeData.shadowArgs[runtimeData.instanceLODs[l].argsBufferOffset + 1] = 0u;
				}
			}
		}
	}

	public static void DispatchCSInstancedCameraCalculation<T>(ComputeShader cameraComputeShader, int[] cameraComputeKernelIDs, T runtimeData, GPUInstancerTerrainSettings terrainSettings, GPUInstancerCameraData cameraData, bool isManagerFrustumCulling, bool isManagerOcclusionCulling, bool isInitial) where T : GClass1270
	{
		bool flag = false;
		if (terrainSettings == null)
		{
			flag = cameraData.IsOptic;
		}
		int count = runtimeData.instanceLODs.Count;
		int kernelIndex = cameraComputeKernelIDs[(!isInitial && runtimeData.prototype.isLODCrossFade) ? 1 : 0];
		cameraComputeShader.SetBuffer(kernelIndex, GClass1262.GClass1264.INSTANCE_LOD_BUFFER, runtimeData.instanceLODDataBuffer);
		cameraComputeShader.SetBuffer(kernelIndex, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, runtimeData.transformationMatrixVisibilityBuffer);
		cameraComputeShader.SetFloats(GClass1262.GClass1264.BUFFER_PARAMETER_MVP_MATRIX, cameraData.mvpMatrixFloats);
		cameraComputeShader.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_CENTER, runtimeData.instanceBounds.center);
		cameraComputeShader.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_EXTENTS, runtimeData.instanceBounds.extents);
		cameraComputeShader.SetBool(GClass1262.GClass1264.BUFFER_PARAMETER_FRUSTUM_CULL_SWITCH, isManagerFrustumCulling && runtimeData.prototype.isFrustumCulling);
		cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_MIN_VIEW_DISTANCE, flag ? runtimeData.prototype.minDistanceOptic : runtimeData.prototype.minDistance);
		cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_MAX_VIEW_DISTANCE, flag ? runtimeData.prototype.maxDistanceOptic : runtimeData.prototype.maxDistance);
		cameraComputeShader.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_CAMERA_POSITION, cameraData.cameraPosition);
		cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_OCCLUSION_OFFSET, runtimeData.prototype.occlusionOffset);
		cameraComputeShader.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_OCCLUSION_ACCURACY, runtimeData.prototype.occlusionAccuracy);
		cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_MIN_CULLING_DISTANCE, runtimeData.prototype.isShadowCasting ? runtimeData.prototype.shadowDistance : runtimeData.prototype.minCullingDistance);
		cameraComputeShader.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, runtimeData.instanceCount);
		cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_FRUSTUM_OFFSET, cameraData.LastAngularFrustumOffset);
		float val = -1f;
		if (runtimeData.hasShadowCasterBuffer)
		{
			runtimeData.prototype.shadowDistance = (runtimeData.prototype.useCustomShadowDistance ? runtimeData.prototype.shadowDistance : QualitySettings.shadowDistance);
			val = runtimeData.prototype.shadowDistance;
			cameraComputeShader.SetFloats(GClass1262.GClass1264.BUFFER_PARAMETER_SHADOW_LOD_MAP, runtimeData.prototype.shadowLODMap);
			cameraComputeShader.SetBool(GClass1262.GClass1264.BUFFER_PARAMETER_CULL_SHADOW, runtimeData.prototype.cullShadows);
		}
		cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_SHADOW_DISTANCE, val);
		cameraComputeShader.SetFloats(GClass1262.GClass1264.BUFFER_PARAMETER_LOD_SIZES, runtimeData.lodSizes);
		cameraComputeShader.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_LOD_COUNT, count);
		cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_HALF_ANGLE, cameraData.halfAngle);
		if (!isInitial && runtimeData.prototype.isLODCrossFade)
		{
			cameraComputeShader.SetBool(GClass1262.GClass1264.BUFFER_PARAMETER_ANIMATE_CROSS_FADE, runtimeData.prototype.isLODCrossFadeAnimate);
			if (runtimeData.prototype.isLODCrossFadeAnimate)
			{
				cameraComputeShader.SetFloat(GClass1262.GClass1264.BUFFER_PARAMETER_DELTA_TIME, GPUInstancerManager.timeSinceLastDrawCall);
			}
		}
		if (isManagerOcclusionCulling && cameraData.hasOcclusionGenerator)
		{
			cameraComputeShader.SetBool(GClass1262.GClass1264.BUFFER_PARAMETER_OCCLUSION_CULL_SWITCH, runtimeData.prototype.isOcclusionCulling);
			if (cameraData.hiZOcclusionGenerator.isVREnabled && GClass1262.gpuiSettings.testBothEyesForVROcclusion)
			{
				cameraComputeShader.SetFloats(GClass1262.GClass1264.BUFFER_PARAMETER_MVP_MATRIX2, cameraData.mvpMatrix2Floats);
			}
			cameraComputeShader.SetTexture(kernelIndex, GClass1262.GClass1264.BUFFER_PARAMETER_HIERARCHICAL_Z_TEXTURE_MAP, cameraData.hiZOcclusionGenerator.hiZDepthTexture);
			cameraComputeShader.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_HIERARCHICAL_Z_TEXTURE_SIZE, cameraData.hiZOcclusionGenerator.hiZTextureSize);
		}
		else
		{
			cameraComputeShader.SetBool(GClass1262.GClass1264.BUFFER_PARAMETER_OCCLUSION_CULL_SWITCH, val: false);
			cameraComputeShader.SetTexture(kernelIndex, GClass1262.GClass1264.BUFFER_PARAMETER_HIERARCHICAL_Z_TEXTURE_MAP, dummyHiZTex);
		}
		cameraComputeShader.Dispatch(kernelIndex, Mathf.CeilToInt((float)runtimeData.instanceCount / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
	}

	public static void DispatchCSInstancedVisibilityCalculation<T>(ComputeShader visibilityComputeShader, int instanceVisibilityComputeKernelId, T runtimeData, bool isShadow, int lodShift, int lodAppendIndex) where T : GClass1270
	{
		int count = runtimeData.instanceLODs.Count;
		visibilityComputeShader.SetBuffer(instanceVisibilityComputeKernelId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, runtimeData.transformationMatrixVisibilityBuffer);
		visibilityComputeShader.SetBuffer(instanceVisibilityComputeKernelId, GClass1262.GClass1264.INSTANCE_LOD_BUFFER, runtimeData.instanceLODDataBuffer);
		for (int i = 0; i < count - lodShift && i < GClass1262.COMPUTE_MAX_LOD_BUFFER; i++)
		{
			GClass1271 gClass = runtimeData.instanceLODs[i + lodShift];
			if (isShadow)
			{
				gClass.shadowAppendBuffer.SetCounterValue(0u);
				visibilityComputeShader.SetBuffer(instanceVisibilityComputeKernelId, GClass1262.GClass1264.TRANSFORMATION_MATRIX_APPEND_BUFFERS[i], gClass.shadowAppendBuffer);
				continue;
			}
			if (lodAppendIndex == 0)
			{
				gClass.transformationMatrixAppendBuffer.SetCounterValue(0u);
			}
			visibilityComputeShader.SetBuffer(instanceVisibilityComputeKernelId, GClass1262.GClass1264.TRANSFORMATION_MATRIX_APPEND_BUFFERS[i], gClass.transformationMatrixAppendBuffer);
		}
		visibilityComputeShader.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, runtimeData.instanceCount);
		visibilityComputeShader.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_LOD_SHIFT, lodShift);
		visibilityComputeShader.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_LOD_APPEND_INDEX, lodAppendIndex);
		visibilityComputeShader.Dispatch(instanceVisibilityComputeKernelId, Mathf.CeilToInt((float)runtimeData.instanceCount / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
	}

	public static void DrawPrePass<T>(CommandBuffer motionVectorsCb, List<T> runtimeDataList, Bounds instancingBounds, GPUInstancerCameraData cameraData, bool generateMotionVectors, int layerMask = -1) where T : GClass1270
	{
		if (runtimeDataList == null || motionVectorsCb == null)
		{
			return;
		}
		for (int i = 0; i < runtimeDataList.Count; i++)
		{
			T val = runtimeDataList[i];
			if (val?.transformationMatrixVisibilityBuffer == null || val.bufferSize == 0 || val.instanceCount == 0)
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			for (int j = 0; j < val.instanceLODs.Count; j++)
			{
				GClass1271 gClass = val.instanceLODs[j];
				for (int k = 0; k < gClass.renderers.Count; k++)
				{
					GClass1272 gClass2 = gClass.renderers[k];
					if (!IsInLayer(layerMask, gClass2.layer))
					{
						continue;
					}
					for (int l = 0; l < gClass2.materials.Count; l++)
					{
						Material material = gClass2.materials[l];
						int num3 = material.FindPass(generateMotionVectors ? "MotionVectors" : "DepthPrePass");
						if (num3 >= 0)
						{
							num2 = Math.Min(l, gClass2.mesh.subMeshCount - 1);
							num = (gClass2.argsBufferOffset + 5 * num2) * GClass1262.STRIDE_SIZE_INT;
							motionVectorsCb.DrawMeshInstancedIndirect(gClass2.mesh, num2, material, num3, val.argsBuffer, num, gClass2.mpb);
						}
					}
				}
			}
		}
	}

	public static void GPUIDrawMeshInstancedIndirect<T>(List<T> runtimeDataList, Bounds instancingBounds, GPUInstancerCameraData cameraData, int layerMask = -1) where T : GClass1270
	{
		if (runtimeDataList == null)
		{
			return;
		}
		bool flag = Singleton<SharedGameSettingsClass>.Instantiated && (bool)Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.GrassShadow;
		Camera mainCamera = cameraData.mainCamera;
		for (int i = 0; i < runtimeDataList.Count; i++)
		{
			T val = runtimeDataList[i];
			if (val?.transformationMatrixVisibilityBuffer == null || val.bufferSize == 0 || val.instanceCount == 0)
			{
				continue;
			}
			int num = 0;
			int num2 = 0;
			for (int j = 0; j < val.instanceLODs.Count; j++)
			{
				GClass1271 gClass = val.instanceLODs[j];
				for (int k = 0; k < gClass.renderers.Count; k++)
				{
					GClass1272 gClass2 = gClass.renderers[k];
					if (!IsInLayer(layerMask, gClass2.layer))
					{
						continue;
					}
					for (int l = 0; l < gClass2.materials.Count; l++)
					{
						Material material = gClass2.materials[l];
						num2 = Math.Min(l, gClass2.mesh.subMeshCount - 1);
						num = (gClass2.argsBufferOffset + 5 * num2) * GClass1262.STRIDE_SIZE_INT;
						Graphics.DrawMeshInstancedIndirect(gClass2.mesh, num2, material, instancingBounds, val.argsBuffer, num, gClass2.mpb, ShadowCastingMode.Off, receiveShadows: true, gClass2.layer, mainCamera);
						if (val.hasShadowCasterBuffer && val.prototype.isShadowCasting && gClass2.castShadows && flag)
						{
							Graphics.DrawMeshInstancedIndirect(gClass2.mesh, num2, val.prototype.useOriginalShaderForShadow ? material : val.shadowCasterMaterial, instancingBounds, val.shadowArgsBuffer, num, gClass2.shadowMPB, ShadowCastingMode.ShadowsOnly, receiveShadows: false, gClass2.layer, mainCamera);
						}
					}
				}
			}
		}
	}

	public static void DispatchBufferToTexture<T>(List<T> runtimeDataList, ComputeShader bufferToTextureComputeShader, int bufferToTextureComputeKernelID) where T : GClass1270
	{
		if (runtimeDataList == null)
		{
			return;
		}
		foreach (T runtimeData in runtimeDataList)
		{
			if (runtimeData == null || runtimeData.args == null || runtimeData.transformationMatrixVisibilityBuffer == null || runtimeData.bufferSize == 0)
			{
				continue;
			}
			for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
			{
				bufferToTextureComputeShader.SetBuffer(bufferToTextureComputeKernelID, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, runtimeData.transformationMatrixVisibilityBuffer);
				bufferToTextureComputeShader.SetBuffer(bufferToTextureComputeKernelID, GClass1262.GClass1264.TRANSFORMATION_MATRIX_BUFFER, runtimeData.instanceLODs[i].transformationMatrixAppendBuffer);
				bufferToTextureComputeShader.SetTexture(bufferToTextureComputeKernelID, GClass1262.GClass1263.TRANSFORMATION_MATRIX_TEXTURE, runtimeData.instanceLODs[i].transformationMatrixAppendTexture);
				bufferToTextureComputeShader.SetBuffer(bufferToTextureComputeKernelID, "argsBuffer", runtimeData.argsBuffer);
				bufferToTextureComputeShader.SetInt("argsBufferIndex", runtimeData.instanceLODs[i].argsBufferOffset + 1);
				bufferToTextureComputeShader.SetInt("maxTextureSize", GClass1262.TEXTURE_MAX_SIZE);
				bufferToTextureComputeShader.Dispatch(bufferToTextureComputeKernelID, Mathf.CeilToInt((float)runtimeData.bufferSize / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
				if (runtimeData.hasShadowCasterBuffer)
				{
					bufferToTextureComputeShader.SetBuffer(bufferToTextureComputeKernelID, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, runtimeData.transformationMatrixVisibilityBuffer);
					bufferToTextureComputeShader.SetBuffer(bufferToTextureComputeKernelID, GClass1262.GClass1264.TRANSFORMATION_MATRIX_BUFFER, runtimeData.instanceLODs[i].shadowAppendBuffer);
					bufferToTextureComputeShader.SetTexture(bufferToTextureComputeKernelID, GClass1262.GClass1263.TRANSFORMATION_MATRIX_TEXTURE, runtimeData.instanceLODs[i].shadowAppendTexture);
					bufferToTextureComputeShader.SetBuffer(bufferToTextureComputeKernelID, "argsBuffer", runtimeData.argsBuffer);
					bufferToTextureComputeShader.SetInt("argsBufferIndex", runtimeData.instanceLODs[i].argsBufferOffset + 1);
					bufferToTextureComputeShader.SetInt("maxTextureSize", GClass1262.TEXTURE_MAX_SIZE);
					bufferToTextureComputeShader.Dispatch(bufferToTextureComputeKernelID, Mathf.CeilToInt((float)runtimeData.bufferSize / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
				}
			}
		}
	}

	public static bool IsInLayer(int layerMask, int layer)
	{
		return layerMask == (layerMask | (1 << layer));
	}

	public static void ReleaseInstanceBuffers<T>(List<T> runtimeDataList) where T : GClass1270
	{
		if (runtimeDataList != null)
		{
			for (int i = 0; i < runtimeDataList.Count; i++)
			{
				ReleaseInstanceBuffers(runtimeDataList[i]);
			}
		}
	}

	public static void ReleaseInstanceBuffers<T>(T runtimeData) where T : GClass1270
	{
		if (runtimeData == null)
		{
			return;
		}
		if (runtimeData.instanceLODs != null)
		{
			for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
			{
				if (runtimeData.instanceLODs[i].transformationMatrixAppendBuffer != null)
				{
					runtimeData.instanceLODs[i].transformationMatrixAppendBuffer.Release();
				}
				runtimeData.instanceLODs[i].transformationMatrixAppendBuffer = null;
				if (runtimeData.instanceLODs[i].transformationMatrixAppendTexture != null)
				{
					UnityEngine.Object.DestroyImmediate(runtimeData.instanceLODs[i].transformationMatrixAppendTexture);
				}
				runtimeData.instanceLODs[i].transformationMatrixAppendTexture = null;
				if (runtimeData.instanceLODs[i].shadowAppendTexture != null)
				{
					UnityEngine.Object.DestroyImmediate(runtimeData.instanceLODs[i].shadowAppendTexture);
				}
				runtimeData.instanceLODs[i].shadowAppendTexture = null;
				runtimeData.instanceLODs[i].shadowAppendBuffer?.Release();
				runtimeData.instanceLODs[i].shadowAppendBuffer = null;
			}
		}
		runtimeData.instanceLODDataBuffer?.Release();
		runtimeData.instanceLODDataBuffer = null;
		runtimeData.transformationMatrixVisibilityBuffer?.Release();
		runtimeData.transformationMatrixVisibilityBuffer = null;
		runtimeData.argsBuffer?.Release();
		runtimeData.argsBuffer = null;
		runtimeData.shadowArgsBuffer?.Release();
		runtimeData.shadowArgsBuffer = null;
		runtimeData.ReleaseBuffers();
	}

	public static void ReleaseSPBuffers(GClass1273<GClass1258> spData)
	{
		if (spData != null && spData.activeCellList != null)
		{
			for (int i = 0; i < spData.activeCellList.Count; i++)
			{
				ReleaseSPCell(spData.activeCellList[i]);
			}
		}
	}

	public static void ReleaseSPCell(GClass1258 spCell)
	{
		if (spCell == null || !(spCell is GClass1259))
		{
			return;
		}
		GClass1259 gClass = (GClass1259)spCell;
		if (gClass.detailInstanceBuffers == null)
		{
			return;
		}
		foreach (ComputeBuffer value in gClass.detailInstanceBuffers.Values)
		{
			value?.Release();
		}
		gClass.detailInstanceBuffers = null;
	}

	public static void ClearInstanceData<T>(List<T> runtimeDataList) where T : GClass1270
	{
		if (runtimeDataList != null)
		{
			for (int i = 0; i < runtimeDataList.Count; i++)
			{
				runtimeDataList[i].instanceDataArray = null;
			}
		}
	}

	public static void SetDetailInstancePrototypes(GPUInstancerDetailManager detailManager, List<GPUInstancerPrototype> detailInstancePrototypes, DetailPrototype[] detailPrototypes, int quadCount, GPUInstancerTerrainSettings terrainSettings, bool forceNew, Terrain terrain)
	{
	}

	public static void AddDetailInstancePrototypeFromTerrainPrototype(GPUInstancerDetailManager detailManager, List<GPUInstancerPrototype> detailInstancePrototypes, DetailPrototype terrainDetailPrototype, int detailIndex, int quadCount, GPUInstancerTerrainSettings terrainSettings, GameObject replacementPrefab = null, Terrain terrain = null)
	{
		if (replacementPrefab == null && terrainDetailPrototype.prototype != null)
		{
			replacementPrefab = terrainDetailPrototype.prototype;
			while (replacementPrefab.transform.parent != null)
			{
				replacementPrefab = replacementPrefab.transform.parent.gameObject;
			}
		}
		GPUInstancerDetailPrototype gPUInstancerDetailPrototype = ScriptableObject.CreateInstance<GPUInstancerDetailPrototype>();
		gPUInstancerDetailPrototype.prototypeIndex = detailIndex;
		gPUInstancerDetailPrototype.detailRenderMode = terrainDetailPrototype.renderMode;
		gPUInstancerDetailPrototype.usePrototypeMesh = terrainDetailPrototype.usePrototypeMesh;
		gPUInstancerDetailPrototype.prefabObject = replacementPrefab;
		gPUInstancerDetailPrototype.prototypeTexture = terrainDetailPrototype.prototypeTexture;
		gPUInstancerDetailPrototype.useCrossQuads = quadCount > 1 && !terrainDetailPrototype.usePrototypeMesh;
		gPUInstancerDetailPrototype.quadCount = quadCount;
		gPUInstancerDetailPrototype.useVertexFit = !terrainDetailPrototype.usePrototypeMesh;
		gPUInstancerDetailPrototype.useTerrainNormal = !terrainDetailPrototype.usePrototypeMesh && gPUInstancerDetailPrototype.useTerrainNormal;
		gPUInstancerDetailPrototype.detailHealthyColor = terrainDetailPrototype.healthyColor;
		gPUInstancerDetailPrototype.detailDryColor = terrainDetailPrototype.dryColor;
		gPUInstancerDetailPrototype.noiseSpread = terrainDetailPrototype.noiseSpread;
		gPUInstancerDetailPrototype.detailScale = new Vector4(terrainDetailPrototype.minWidth, terrainDetailPrototype.maxWidth, terrainDetailPrototype.minHeight, terrainDetailPrototype.maxHeight);
		gPUInstancerDetailPrototype.windWaveTintColor = terrainSettings.wavingGrassTint;
		string text = ((terrainDetailPrototype.prototype != null) ? GetAssetGUID(terrainDetailPrototype.prototype) : GetAssetGUID(terrainDetailPrototype.prototypeTexture)).Substring(0, 6);
		gPUInstancerDetailPrototype.name = "Detail_" + detailIndex + "_" + ((terrainDetailPrototype.prototype != null) ? terrainDetailPrototype.prototype.name : terrainDetailPrototype.prototypeTexture.name) + "_" + text;
		gPUInstancerDetailPrototype.maxDistance = terrainSettings.maxDetailDistance;
		gPUInstancerDetailPrototype.detailDensity = terrainSettings.detailDensity;
		gPUInstancerDetailPrototype.isBillboardDisabled = !terrainDetailPrototype.usePrototypeMesh;
		gPUInstancerDetailPrototype.windWavesOn = !terrainDetailPrototype.usePrototypeMesh && gPUInstancerDetailPrototype.windWavesOn;
		if (replacementPrefab != null)
		{
			DetermineTreePrototypeType(gPUInstancerDetailPrototype);
		}
		if (gPUInstancerDetailPrototype.treeType != GPUInstancerTreeType.None || GClass1262.gpuiSettings.isLWRP || GClass1262.gpuiSettings.isHDRP)
		{
			gPUInstancerDetailPrototype.useOriginalShaderForShadow = true;
		}
		gPUInstancerDetailPrototype.isShadowCasting = GClass1262.gpuiSettings.DEFAULT_GRASS_SHADOW_DISTANCE > 0f && !terrainDetailPrototype.usePrototypeMesh && !detailManager.IsOptic;
		gPUInstancerDetailPrototype.useCustomShadowDistance = gPUInstancerDetailPrototype.isShadowCasting;
		gPUInstancerDetailPrototype.shadowDistance = GClass1262.gpuiSettings.DEFAULT_GRASS_SHADOW_DISTANCE;
		gPUInstancerDetailPrototype.cullShadows = true;
		if (!GClass1262.gpuiSettings.disableAutoGenerateBillboards && IsBillboardGeneratedByDefault(gPUInstancerDetailPrototype))
		{
			gPUInstancerDetailPrototype.isLODCrossFade = true;
			gPUInstancerDetailPrototype.useGeneratedBillboard = true;
			if (gPUInstancerDetailPrototype.billboard == null)
			{
				gPUInstancerDetailPrototype.billboard = new GPUInstancerBillboard();
			}
			GeneratePrototypeBillboard(gPUInstancerDetailPrototype, GClass1262.gpuiSettings);
		}
		AddObjectToAsset(terrainSettings, gPUInstancerDetailPrototype);
		detailInstancePrototypes.Add(gPUInstancerDetailPrototype);
		if (terrainDetailPrototype.usePrototypeMesh)
		{
			GenerateInstancedShadersForGameObject(gPUInstancerDetailPrototype);
		}
		else if (GClass1262.gpuiSettings.isLWRP)
		{
			if (Shader.Find(GClass1262.SHADER_GPUI_FOLIAGE_LWRP) == null)
			{
				ImportFoliageLWRPShader();
			}
			else
			{
				GClass1262.gpuiSettings.AddShaderVariantToCollection(GClass1262.SHADER_GPUI_FOLIAGE_LWRP);
			}
		}
		else
		{
			GClass1262.gpuiSettings.AddShaderVariantToCollection(GClass1262.SHADER_GPUI_FOLIAGE);
		}
	}

	public static void ImportFoliageLWRPShader()
	{
	}

	public static void ImportFoliageLWRPShaderPopup()
	{
	}

	public static void OnFoliageLWRPShaderImportCompleted(string foliageShaderPackageName)
	{
	}

	public static void AddDetailInstanceRuntimeDataToList(Terrain terrain, List<GClass1270> runtimeDataList, List<GPUInstancerPrototype> detailPrototypes, GPUInstancerTerrainSettings terrainSettings, int detailLayer)
	{
		foreach (GPUInstancerPrototype detailPrototype in detailPrototypes)
		{
			if (detailPrototype == null)
			{
				continue;
			}
			GClass1270 gClass = new GClass1270(detailPrototype);
			GPUInstancerDetailPrototype gPUInstancerDetailPrototype = (GPUInstancerDetailPrototype)detailPrototype;
			if (gPUInstancerDetailPrototype.usePrototypeMesh)
			{
				if (!gClass.CreateRenderersFromGameObject(detailPrototype))
				{
					continue;
				}
				AddBillboardToRuntimeData(gClass);
				if (gPUInstancerDetailPrototype.treeType == GPUInstancerTreeType.SpeedTree || gPUInstancerDetailPrototype.treeType == GPUInstancerTreeType.SpeedTree8 || gPUInstancerDetailPrototype.treeType == GPUInstancerTreeType.TreeCreatorTree)
				{
					GPUInstancerManager.AddTreeProxy(gPUInstancerDetailPrototype, gClass);
				}
				Material material = detailPrototype.prefabObject.GetComponentsInChildren<MeshRenderer>().FirstOrDefault((MeshRenderer r) => r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE || r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE_LWRP)?.sharedMaterial;
				if ((bool)material)
				{
					if (detailPrototype.isShadowCasting)
					{
						gClass.hasShadowCasterBuffer = true;
						if (!detailPrototype.useOriginalShaderForShadow)
						{
							gClass.shadowCasterMaterial = material;
						}
					}
					foreach (GClass1271 instanceLOD in gClass.instanceLODs)
					{
						foreach (GClass1272 renderer in instanceLOD.renderers)
						{
							renderer.mpb.SetTexture(Int_0, terrainSettings.GetHealthyDryNoiseTexture(gPUInstancerDetailPrototype));
							renderer.mpb.SetFloat(Int_1, gPUInstancerDetailPrototype.noiseSpread);
							renderer.mpb.SetTexture(Int_2, terrainSettings.windWaveNormalTexture);
							renderer.mpb.SetTexture(Int_4, terrain.terrainData.heightmapTexture);
							renderer.mpb.SetInt(Int_5, terrain.terrainData.heightmapResolution);
							renderer.mpb.SetVector(Int_6, terrain.transform.position);
							renderer.mpb.SetVector(Int_7, terrain.terrainData.size);
							renderer.mpb.SetTexture(Int_8, terrain.normalmapTexture);
							renderer.mpb.SetFloat(Int_9, gPUInstancerDetailPrototype.gradientNormalHeight);
							if ((bool)gPUInstancerDetailPrototype.bumpMap)
							{
								renderer.mpb.SetTexture(Int_10, gPUInstancerDetailPrototype.bumpMap);
							}
							renderer.mpb.SetTexture(Int_11, gPUInstancerDetailPrototype.CreateTextureArray(terrain.terrainData.alphamapTextures));
							renderer.mpb.SetFloatArray(Int_12, gPUInstancerDetailPrototype.CreateAlphaMaskArray());
							if ((bool)terrainSettings.densityMapArray && terrainSettings.densityMapArray != null)
							{
								renderer.mpb.SetTexture(Int_13, terrainSettings.densityMapArray);
								renderer.mpb.SetInt(Int_14, gPUInstancerDetailPrototype.densityMapIndex);
								renderer.mpb.SetVector(Int_15, gPUInstancerDetailPrototype.densityChanelMask * gPUInstancerDetailPrototype.densityFadeFactor);
								renderer.mpb.SetVector(Int_16, gPUInstancerDetailPrototype.densityMinMax);
							}
							renderer.mpb.SetVector(Int_17, new Vector4(gPUInstancerDetailPrototype.maxDistance - 5f, gPUInstancerDetailPrototype.maxDistance, gPUInstancerDetailPrototype.shadowDistance - 5f, gPUInstancerDetailPrototype.shadowDistance));
							material.DisableKeyword("_VERTEXFIT_ON");
							if (gPUInstancerDetailPrototype.useVertexFit)
							{
								material.EnableKeyword("_VERTEXFIT_ON");
							}
							material.DisableKeyword("_UseTerrainNormal_ON");
							if (gPUInstancerDetailPrototype.useTerrainNormal)
							{
								material.EnableKeyword("_UseTerrainNormal_ON");
							}
							material.DisableKeyword("_UseAlphaMask_ON");
							if (gPUInstancerDetailPrototype.useAlphaMask)
							{
								material.EnableKeyword("_UseAlphaMask_ON");
							}
							material.DisableKeyword("_UseDensityMask_ON");
							if (gPUInstancerDetailPrototype.useDensityMask && terrainSettings.densityMapArray != null)
							{
								material.EnableKeyword("_UseDensityMask_ON");
							}
						}
					}
				}
			}
			else
			{
				Material instancedMaterial;
				if (gPUInstancerDetailPrototype.useCustomMaterialForTextureDetail && gPUInstancerDetailPrototype.textureDetailCustomMaterial != null)
				{
					instancedMaterial = GClass1262.gpuiSettings.shaderBindings.GetInstancedMaterial(gPUInstancerDetailPrototype.textureDetailCustomMaterial);
					instancedMaterial.name = "InstancedMaterial_" + gPUInstancerDetailPrototype.prototypeTexture.name;
					if (detailPrototype.isShadowCasting)
					{
						gClass.hasShadowCasterBuffer = true;
						if (!detailPrototype.useOriginalShaderForShadow)
						{
							gClass.shadowCasterMaterial = instancedMaterial;
						}
					}
					instancedMaterial.SetFloat(Int_1, gPUInstancerDetailPrototype.noiseSpread);
					instancedMaterial.SetFloat(Int_18, gPUInstancerDetailPrototype.ambientOcclusion);
					instancedMaterial.SetFloat(Int_19, gPUInstancerDetailPrototype.gradientPower);
					instancedMaterial.SetColor(Int_20, gPUInstancerDetailPrototype.windWaveTintColor);
					instancedMaterial.SetFloat(Int_21, gPUInstancerDetailPrototype.windIdleSway);
					instancedMaterial.SetFloat(Int_22, gPUInstancerDetailPrototype.windWavesOn ? 1f : 0f);
					instancedMaterial.SetFloat(Int_24, gPUInstancerDetailPrototype.windWaveTint);
					instancedMaterial.SetFloat(Int_25, gPUInstancerDetailPrototype.windWaveSway);
					instancedMaterial.DisableKeyword("_BILLBOARDFACECAMPOS_ON");
					if (gPUInstancerDetailPrototype.billboardFaceCamPos)
					{
						instancedMaterial.EnableKeyword("_BILLBOARDFACECAMPOS_ON");
					}
					instancedMaterial.DisableKeyword("_VERTEXFIT_ON");
					if (gPUInstancerDetailPrototype.useVertexFit)
					{
						instancedMaterial.EnableKeyword("_VERTEXFIT_ON");
					}
					instancedMaterial.DisableKeyword("_UseTerrainNormal_ON");
					if (gPUInstancerDetailPrototype.useTerrainNormal)
					{
						instancedMaterial.EnableKeyword("_UseTerrainNormal_ON");
					}
					instancedMaterial.DisableKeyword("_UseAlphaMask_ON");
					if (gPUInstancerDetailPrototype.useAlphaMask)
					{
						instancedMaterial.EnableKeyword("_UseAlphaMask_ON");
					}
					instancedMaterial.DisableKeyword("_UseDensityMask_ON");
					if (gPUInstancerDetailPrototype.useDensityMask)
					{
						instancedMaterial.EnableKeyword("_UseDensityMask_ON");
					}
					instancedMaterial.SetTexture(Int_4, terrain.terrainData.heightmapTexture);
					instancedMaterial.SetInt(Int_5, terrain.terrainData.heightmapResolution);
					instancedMaterial.SetVector(Int_6, terrain.transform.position);
					instancedMaterial.SetVector(Int_7, terrain.terrainData.size);
					instancedMaterial.SetTexture(Int_8, terrain.normalmapTexture);
					instancedMaterial.SetFloat(Int_9, gPUInstancerDetailPrototype.gradientNormalHeight);
					instancedMaterial.SetVector(Int_17, new Vector4(gPUInstancerDetailPrototype.maxDistance - 5f, gPUInstancerDetailPrototype.maxDistance, gPUInstancerDetailPrototype.shadowDistance - 5f, gPUInstancerDetailPrototype.shadowDistance));
					if ((bool)gPUInstancerDetailPrototype.bumpMap)
					{
						instancedMaterial.SetTexture(Int_10, gPUInstancerDetailPrototype.bumpMap);
					}
					if (gPUInstancerDetailPrototype.useAlphaMask)
					{
						instancedMaterial.SetTexture(Int_11, gPUInstancerDetailPrototype.CreateTextureArray(terrain.terrainData.alphamapTextures));
						instancedMaterial.SetFloatArray(Int_12, gPUInstancerDetailPrototype.CreateAlphaMaskArray());
					}
					if (gPUInstancerDetailPrototype.useDensityMask && terrainSettings.densityMapArray != null)
					{
						instancedMaterial.SetTexture(Int_13, terrainSettings.densityMapArray);
						instancedMaterial.SetInt(Int_14, gPUInstancerDetailPrototype.densityMapIndex);
						instancedMaterial.SetVector(Int_15, gPUInstancerDetailPrototype.densityChanelMask * gPUInstancerDetailPrototype.densityFadeFactor);
						instancedMaterial.SetVector(Int_16, gPUInstancerDetailPrototype.densityMinMax);
					}
					gClass.AddLodAndRenderer(CreateCrossQuadsMeshForDetailGrass(1f, 1f, gPUInstancerDetailPrototype.prototypeTexture.name, gPUInstancerDetailPrototype.quadCount), new List<Material> { instancedMaterial }, new MaterialPropertyBlock(), castShadows: true, 0f, new MaterialPropertyBlock(), excludeBounds: false, detailLayer);
					runtimeDataList.Add(gClass);
					continue;
				}
				instancedMaterial = new Material(Shader.Find(GClass1262.gpuiSettings.isLWRP ? GClass1262.SHADER_GPUI_FOLIAGE_LWRP : GClass1262.SHADER_GPUI_FOLIAGE));
				if (detailPrototype.isShadowCasting)
				{
					gClass.hasShadowCasterBuffer = true;
					if (!detailPrototype.useOriginalShaderForShadow)
					{
						gClass.shadowCasterMaterial = instancedMaterial;
					}
				}
				instancedMaterial.SetTexture(Int_0, terrainSettings.GetHealthyDryNoiseTexture(gPUInstancerDetailPrototype));
				instancedMaterial.SetTexture(Int_2, terrainSettings.windWaveNormalTexture);
				instancedMaterial.SetFloat(Int_26, gPUInstancerDetailPrototype.useCrossQuads ? 0f : (gPUInstancerDetailPrototype.isBillboard ? 1f : 0f));
				instancedMaterial.DisableKeyword("_BILLBOARDFACECAMPOS_ON");
				if (gPUInstancerDetailPrototype.billboardFaceCamPos)
				{
					instancedMaterial.EnableKeyword("_BILLBOARDFACECAMPOS_ON");
				}
				instancedMaterial.DisableKeyword("_VERTEXFIT_ON");
				if (gPUInstancerDetailPrototype.useVertexFit)
				{
					instancedMaterial.EnableKeyword("_VERTEXFIT_ON");
				}
				instancedMaterial.DisableKeyword("_UseTerrainNormal_ON");
				if (gPUInstancerDetailPrototype.useTerrainNormal)
				{
					instancedMaterial.EnableKeyword("_UseTerrainNormal_ON");
				}
				instancedMaterial.DisableKeyword("_UseAlphaMask_ON");
				if (gPUInstancerDetailPrototype.useAlphaMask)
				{
					instancedMaterial.EnableKeyword("_UseAlphaMask_ON");
				}
				instancedMaterial.DisableKeyword("_UseDensityMask_ON");
				if (gPUInstancerDetailPrototype.useDensityMask && terrainSettings.densityMapArray != null)
				{
					instancedMaterial.EnableKeyword("_UseDensityMask_ON");
				}
				instancedMaterial.SetTexture(Int_27, gPUInstancerDetailPrototype.prototypeTexture);
				instancedMaterial.SetColor(Int_28, gPUInstancerDetailPrototype.detailHealthyColor);
				instancedMaterial.SetColor(Int_29, gPUInstancerDetailPrototype.detailDryColor);
				instancedMaterial.SetFloat(Int_1, gPUInstancerDetailPrototype.noiseSpread);
				instancedMaterial.SetFloat(Int_18, gPUInstancerDetailPrototype.ambientOcclusion);
				instancedMaterial.SetFloat(Int_19, gPUInstancerDetailPrototype.gradientPower);
				instancedMaterial.SetColor(Int_20, gPUInstancerDetailPrototype.windWaveTintColor);
				instancedMaterial.SetFloat(Int_21, gPUInstancerDetailPrototype.windIdleSway);
				instancedMaterial.SetFloat(Int_22, gPUInstancerDetailPrototype.windWavesOn ? 1f : 0f);
				instancedMaterial.SetFloat(Int_24, gPUInstancerDetailPrototype.windWaveTint);
				instancedMaterial.SetFloat(Int_25, gPUInstancerDetailPrototype.windWaveSway);
				instancedMaterial.SetTexture(Int_4, terrain.terrainData.heightmapTexture);
				instancedMaterial.SetInt(Int_5, terrain.terrainData.heightmapResolution);
				instancedMaterial.SetVector(Int_6, terrain.transform.position);
				instancedMaterial.SetVector(Int_7, terrain.terrainData.size);
				instancedMaterial.SetTexture(Int_8, terrain.normalmapTexture);
				instancedMaterial.SetFloat(Int_9, gPUInstancerDetailPrototype.gradientNormalHeight);
				instancedMaterial.SetVector(Int_17, new Vector4(gPUInstancerDetailPrototype.maxDistance - 5f, gPUInstancerDetailPrototype.maxDistance, gPUInstancerDetailPrototype.shadowDistance - 5f, gPUInstancerDetailPrototype.shadowDistance));
				if ((bool)gPUInstancerDetailPrototype.bumpMap)
				{
					instancedMaterial.SetTexture(Int_10, gPUInstancerDetailPrototype.bumpMap);
				}
				if (gPUInstancerDetailPrototype.useAlphaMask)
				{
					instancedMaterial.SetTexture(Int_11, gPUInstancerDetailPrototype.CreateTextureArray(terrain.terrainData.alphamapTextures));
					instancedMaterial.SetFloatArray(Int_12, gPUInstancerDetailPrototype.CreateAlphaMaskArray());
				}
				if (gPUInstancerDetailPrototype.useDensityMask)
				{
					instancedMaterial.SetTexture(Int_13, terrainSettings.densityMapArray);
					instancedMaterial.SetInt(Int_14, gPUInstancerDetailPrototype.densityMapIndex);
					instancedMaterial.SetVector(Int_15, gPUInstancerDetailPrototype.densityChanelMask * gPUInstancerDetailPrototype.densityFadeFactor);
					instancedMaterial.SetVector(Int_16, gPUInstancerDetailPrototype.densityMinMax);
				}
				instancedMaterial.name = "InstancedMaterial_" + gPUInstancerDetailPrototype.prototypeTexture.name;
				gClass.AddLodAndRenderer(CreateCrossQuadsMeshForDetailGrass(1f, 1f, gPUInstancerDetailPrototype.prototypeTexture.name, (!gPUInstancerDetailPrototype.useCrossQuads) ? 1 : gPUInstancerDetailPrototype.quadCount), new List<Material> { instancedMaterial }, new MaterialPropertyBlock(), castShadows: true, gPUInstancerDetailPrototype.useCrossQuads ? GetDistanceRelativeHeight(gPUInstancerDetailPrototype) : 0f, new MaterialPropertyBlock(), excludeBounds: false, detailLayer);
				if (gPUInstancerDetailPrototype.useCrossQuads)
				{
					Material material2 = instancedMaterial;
					material2.SetFloat(Int_26, 0f);
					material2.DisableKeyword("_BILLBOARDFACECAMPOS_ON");
					if (gPUInstancerDetailPrototype.billboardFaceCamPos)
					{
						material2.EnableKeyword("_BILLBOARDFACECAMPOS_ON");
					}
					if (gPUInstancerDetailPrototype.billboardDistanceDebug)
					{
						material2.SetColor(Int_28, gPUInstancerDetailPrototype.billboardDistanceDebugColor);
						material2.SetColor(Int_29, gPUInstancerDetailPrototype.billboardDistanceDebugColor);
					}
					if (detailPrototype.isShadowCasting)
					{
						gClass.hasShadowCasterBuffer = true;
						if (!detailPrototype.useOriginalShaderForShadow)
						{
							gClass.shadowCasterMaterial = material2;
						}
					}
				}
			}
			runtimeDataList.Add(gClass);
		}
	}

	public static void UpdateDetailInstanceRuntimeDataList(Terrain terrain, List<GClass1270> runtimeDataList, GPUInstancerTerrainSettings terrainSettings, bool updateMeshes = false, int detailLayer = 0)
	{
		foreach (GClass1270 runtimeData in runtimeDataList)
		{
			GPUInstancerDetailPrototype gPUInstancerDetailPrototype = (GPUInstancerDetailPrototype)runtimeData.prototype;
			if ((bool)runtimeData.shadowCasterMaterial)
			{
				runtimeData.shadowCasterMaterial.SetVector(Int_17, new Vector4(gPUInstancerDetailPrototype.maxDistance - 5f, gPUInstancerDetailPrototype.maxDistance, gPUInstancerDetailPrototype.shadowDistance - 5f, gPUInstancerDetailPrototype.shadowDistance));
			}
			if (gPUInstancerDetailPrototype.usePrototypeMesh)
			{
				if (!(gPUInstancerDetailPrototype.prefabObject.GetComponentsInChildren<MeshRenderer>().FirstOrDefault((MeshRenderer r) => r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE || r.sharedMaterial.shader.name == GClass1262.SHADER_GPUI_FOLIAGE_LWRP)?.sharedMaterial))
				{
					continue;
				}
				foreach (GClass1271 instanceLOD in runtimeData.instanceLODs)
				{
					foreach (GClass1272 renderer in instanceLOD.renderers)
					{
						renderer.mpb.SetTexture(Int_0, terrainSettings.GetHealthyDryNoiseTexture(gPUInstancerDetailPrototype));
						renderer.mpb.SetFloat(Int_1, gPUInstancerDetailPrototype.noiseSpread);
						renderer.mpb.SetTexture(Int_2, terrainSettings.windWaveNormalTexture);
						renderer.mpb.SetTexture(Int_4, terrain.terrainData.heightmapTexture);
						renderer.mpb.SetInt(Int_5, terrain.terrainData.heightmapResolution);
						renderer.mpb.SetVector(Int_6, terrain.transform.position);
						renderer.mpb.SetVector(Int_7, terrain.terrainData.size);
						renderer.mpb.SetTexture(Int_8, terrain.normalmapTexture);
						renderer.mpb.SetFloat(Int_9, gPUInstancerDetailPrototype.gradientNormalHeight);
						renderer.mpb.SetVector(Int_17, new Vector4(gPUInstancerDetailPrototype.maxDistance - 5f, gPUInstancerDetailPrototype.maxDistance, gPUInstancerDetailPrototype.shadowDistance - 5f, gPUInstancerDetailPrototype.shadowDistance));
						if ((bool)gPUInstancerDetailPrototype.bumpMap)
						{
							renderer.mpb.SetTexture(Int_10, gPUInstancerDetailPrototype.bumpMap);
						}
						if (gPUInstancerDetailPrototype.useAlphaMask)
						{
							renderer.mpb.SetTexture(Int_11, gPUInstancerDetailPrototype.CreateTextureArray(terrain.terrainData.alphamapTextures));
							renderer.mpb.SetFloatArray(Int_12, gPUInstancerDetailPrototype.CreateAlphaMaskArray());
						}
						if (gPUInstancerDetailPrototype.useDensityMask && terrainSettings.densityMapArray != null)
						{
							renderer.mpb.SetTexture(Int_13, terrainSettings.densityMapArray);
							renderer.mpb.SetInt(Int_14, gPUInstancerDetailPrototype.densityMapIndex);
							renderer.mpb.SetVector(Int_15, gPUInstancerDetailPrototype.densityChanelMask * gPUInstancerDetailPrototype.densityFadeFactor);
							renderer.mpb.SetVector(Int_16, gPUInstancerDetailPrototype.densityMinMax);
						}
						Material material = renderer.materials.First();
						material.DisableKeyword("_VERTEXFIT_ON");
						if (gPUInstancerDetailPrototype.useVertexFit)
						{
							material.EnableKeyword("_VERTEXFIT_ON");
						}
						material.DisableKeyword("_UseTerrainNormal_ON");
						if (gPUInstancerDetailPrototype.useTerrainNormal)
						{
							material.EnableKeyword("_UseTerrainNormal_ON");
						}
						material.DisableKeyword("_UseAlphaMask_ON");
						if (gPUInstancerDetailPrototype.useAlphaMask)
						{
							material.EnableKeyword("_UseAlphaMask_ON");
						}
						material.DisableKeyword("_UseDensityMask_ON");
						if (gPUInstancerDetailPrototype.useDensityMask && terrainSettings.densityMapArray != null)
						{
							material.EnableKeyword("_UseDensityMask_ON");
						}
					}
				}
				continue;
			}
			if (!gPUInstancerDetailPrototype.useCustomMaterialForTextureDetail || (gPUInstancerDetailPrototype.useCustomMaterialForTextureDetail && gPUInstancerDetailPrototype.textureDetailCustomMaterial != null))
			{
				if (updateMeshes)
				{
					if (gPUInstancerDetailPrototype.useCrossQuads)
					{
						GClass1271 gClass = runtimeData.instanceLODs[runtimeData.instanceLODs.Count - 1];
						runtimeData.instanceLODs[0].transformationMatrixAppendBuffer?.Release();
						runtimeData.instanceLODs[0].transformationMatrixAppendBuffer = null;
						runtimeData.instanceLODs.Clear();
						runtimeData.AddLodAndRenderer(CreateCrossQuadsMeshForDetailGrass(1f, 1f, gPUInstancerDetailPrototype.prototypeTexture.name, gPUInstancerDetailPrototype.quadCount), new List<Material> { gClass.renderers[0].materials[0] }, new MaterialPropertyBlock(), castShadows: true, 1f, new MaterialPropertyBlock(), excludeBounds: false, detailLayer);
						runtimeData.instanceLODs.Add(gClass);
						runtimeData.lodSizes[1] = 0f;
						runtimeData.argsBuffer?.Release();
						runtimeData.argsBuffer = null;
						InitializeGPUBuffer(runtimeData);
					}
					else if (runtimeData.instanceLODs.Count == 2)
					{
						runtimeData.instanceLODs[0].transformationMatrixAppendBuffer?.Release();
						runtimeData.instanceLODs[0].transformationMatrixAppendBuffer = null;
						runtimeData.instanceLODs.RemoveAt(0);
						runtimeData.argsBuffer?.Release();
						runtimeData.argsBuffer = null;
						runtimeData.lodSizes[0] = 0f;
						runtimeData.lodSizes[1] = -1f;
						InitializeGPUBuffer(runtimeData);
					}
				}
				for (int num = 0; num < runtimeData.instanceLODs.Count; num++)
				{
					MaterialPropertyBlock mpb = runtimeData.instanceLODs[num].renderers[0].mpb;
					mpb.SetTexture(Int_0, terrainSettings.GetHealthyDryNoiseTexture(gPUInstancerDetailPrototype));
					mpb.SetTexture(Int_2, terrainSettings.windWaveNormalTexture);
					mpb.SetColor(Int_28, gPUInstancerDetailPrototype.detailHealthyColor);
					mpb.SetColor(Int_29, gPUInstancerDetailPrototype.detailDryColor);
					mpb.SetFloat(Int_1, gPUInstancerDetailPrototype.noiseSpread);
					mpb.SetFloat(Int_18, gPUInstancerDetailPrototype.ambientOcclusion);
					mpb.SetFloat(Int_19, gPUInstancerDetailPrototype.gradientPower);
					mpb.SetColor(Int_20, gPUInstancerDetailPrototype.windWaveTintColor);
					mpb.SetFloat(Int_21, gPUInstancerDetailPrototype.windIdleSway);
					mpb.SetFloat(Int_22, gPUInstancerDetailPrototype.windWavesOn ? 1f : 0f);
					mpb.SetFloat(Int_24, gPUInstancerDetailPrototype.windWaveTint);
					mpb.SetFloat(Int_25, gPUInstancerDetailPrototype.windWaveSway);
					mpb.SetFloat(Int_26, (gPUInstancerDetailPrototype.useCrossQuads && num == 0) ? 0f : ((gPUInstancerDetailPrototype.isBillboard || gPUInstancerDetailPrototype.useCrossQuads) ? 1f : 0f));
					mpb.SetTexture(Int_4, terrain.terrainData.heightmapTexture);
					mpb.SetInt(Int_5, terrain.terrainData.heightmapResolution);
					mpb.SetVector(Int_6, terrain.transform.position);
					mpb.SetVector(Int_7, terrain.terrainData.size);
					mpb.SetTexture(Int_8, terrain.normalmapTexture);
					mpb.SetFloat(Int_9, gPUInstancerDetailPrototype.gradientNormalHeight);
					mpb.SetVector(Int_17, new Vector4(gPUInstancerDetailPrototype.maxDistance - 5f, gPUInstancerDetailPrototype.maxDistance, gPUInstancerDetailPrototype.shadowDistance - 5f, gPUInstancerDetailPrototype.shadowDistance));
					if ((bool)gPUInstancerDetailPrototype.bumpMap)
					{
						mpb.SetTexture(Int_10, gPUInstancerDetailPrototype.bumpMap);
					}
					if (gPUInstancerDetailPrototype.useAlphaMask)
					{
						mpb.SetTexture(Int_11, gPUInstancerDetailPrototype.CreateTextureArray(terrain.terrainData.alphamapTextures));
						mpb.SetFloatArray(Int_12, gPUInstancerDetailPrototype.CreateAlphaMaskArray());
					}
					if (gPUInstancerDetailPrototype.useDensityMask && terrainSettings.densityMapArray != null)
					{
						mpb.SetTexture(Int_13, terrainSettings.densityMapArray);
						mpb.SetInt(Int_14, gPUInstancerDetailPrototype.densityMapIndex);
						mpb.SetVector(Int_15, gPUInstancerDetailPrototype.densityChanelMask * gPUInstancerDetailPrototype.densityFadeFactor);
						mpb.SetVector(Int_16, gPUInstancerDetailPrototype.densityMinMax);
					}
					Material material2 = runtimeData.instanceLODs[num].renderers[0].materials.First();
					material2.DisableKeyword("_VERTEXFIT_ON");
					if (gPUInstancerDetailPrototype.useVertexFit)
					{
						material2.EnableKeyword("_VERTEXFIT_ON");
					}
					material2.DisableKeyword("_UseTerrainNormal_ON");
					if (gPUInstancerDetailPrototype.useTerrainNormal)
					{
						material2.EnableKeyword("_UseTerrainNormal_ON");
					}
					material2.DisableKeyword("_UseAlphaMask_ON");
					if (gPUInstancerDetailPrototype.useAlphaMask)
					{
						material2.EnableKeyword("_UseAlphaMask_ON");
					}
					material2.DisableKeyword("_UseDensityMask_ON");
					if (gPUInstancerDetailPrototype.useDensityMask)
					{
						material2.EnableKeyword("_UseDensityMask_ON");
					}
				}
			}
			if (gPUInstancerDetailPrototype.useCrossQuads)
			{
				runtimeData.lodSizes[0] = GetDistanceRelativeHeight(gPUInstancerDetailPrototype);
				if (gPUInstancerDetailPrototype.billboardDistanceDebug)
				{
					MaterialPropertyBlock mpb2 = runtimeData.instanceLODs[1].renderers[0].mpb;
					mpb2.SetColor(Int_28, gPUInstancerDetailPrototype.billboardDistanceDebugColor);
					mpb2.SetColor(Int_29, gPUInstancerDetailPrototype.billboardDistanceDebugColor);
				}
			}
		}
	}

	public static void UpdateTerrainNormalMapDetailInstance(Terrain terrain, List<GClass1270> runtimeDataList)
	{
		if (runtimeDataList == null)
		{
			return;
		}
		foreach (GClass1270 runtimeData in runtimeDataList)
		{
			foreach (GClass1271 instanceLOD in runtimeData.instanceLODs)
			{
				foreach (GClass1272 renderer in instanceLOD.renderers)
				{
					renderer.mpb.SetTexture(Int_8, terrain.normalmapTexture);
				}
			}
		}
	}

	public static float GetDistanceRelativeHeight(GPUInstancerDetailPrototype detailPrototype)
	{
		return 1f - detailPrototype.billboardDistance;
	}

	public static void SetPrefabInstancePrototypes(GameObject gameObject, List<GPUInstancerPrototype> prototypeList, List<GameObject> prefabList, bool forceNew)
	{
		if (prefabList == null)
		{
			return;
		}
		foreach (GameObject go in prefabList)
		{
			if (forceNew || !prototypeList.Exists((GPUInstancerPrototype p) => p.prefabObject == go))
			{
				prototypeList.Add(GeneratePrefabPrototype(go, forceNew));
			}
		}
	}

	public static GPUInstancerPrefabPrototype GeneratePrefabPrototype(GameObject go, bool forceNew)
	{
		GPUInstancerPrefab gPUInstancerPrefab = go.GetComponent<GPUInstancerPrefab>();
		if (gPUInstancerPrefab == null)
		{
			gPUInstancerPrefab = go.AddComponent<GPUInstancerPrefab>();
		}
		if (gPUInstancerPrefab == null)
		{
			return null;
		}
		GPUInstancerPrefabPrototype gPUInstancerPrefabPrototype = gPUInstancerPrefab.prefabPrototype;
		if (gPUInstancerPrefabPrototype == null)
		{
			gPUInstancerPrefabPrototype = (gPUInstancerPrefab.prefabPrototype = ScriptableObject.CreateInstance<GPUInstancerPrefabPrototype>());
			gPUInstancerPrefabPrototype.prefabObject = go;
			gPUInstancerPrefabPrototype.name = go.name + "_" + go.GetInstanceID();
			DetermineTreePrototypeType(gPUInstancerPrefabPrototype);
			if (gPUInstancerPrefabPrototype.treeType != GPUInstancerTreeType.None || GClass1262.gpuiSettings.isLWRP || GClass1262.gpuiSettings.isHDRP)
			{
				gPUInstancerPrefabPrototype.useOriginalShaderForShadow = true;
			}
			if (go.GetComponent<Rigidbody>() != null)
			{
				gPUInstancerPrefabPrototype.enableRuntimeModifications = true;
				gPUInstancerPrefabPrototype.autoUpdateTransformData = true;
			}
			if (!gPUInstancerPrefabPrototype.useOriginalShaderForShadow)
			{
				MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
					foreach (Material material in sharedMaterials)
					{
						if (material.shader.name.Contains("HDRenderPipeline") || material.shader.name.Contains("LWRenderPipeline") || material.shader.name.Contains("Lightweight Render Pipeline"))
						{
							gPUInstancerPrefabPrototype.useOriginalShaderForShadow = true;
							break;
						}
					}
					if (gPUInstancerPrefabPrototype.useOriginalShaderForShadow)
					{
						break;
					}
				}
			}
			if (!GClass1262.gpuiSettings.disableAutoGenerateBillboards && IsBillboardGeneratedByDefault(gPUInstancerPrefabPrototype))
			{
				gPUInstancerPrefabPrototype.isLODCrossFade = true;
				gPUInstancerPrefabPrototype.useGeneratedBillboard = true;
				if (gPUInstancerPrefabPrototype.billboard == null)
				{
					gPUInstancerPrefabPrototype.billboard = new GPUInstancerBillboard();
				}
				GeneratePrototypeBillboard(gPUInstancerPrefabPrototype);
			}
			GenerateInstancedShadersForGameObject(gPUInstancerPrefabPrototype);
		}
		return gPUInstancerPrefabPrototype;
	}

	public static void SetTreeInstancePrototypes(GameObject gameObject, List<GPUInstancerPrototype> treeIntancePrototypes, TreePrototype[] treePrototypes, GPUInstancerTerrainSettings terrainSettings, bool forceNew)
	{
		if (forceNew)
		{
			RemoveAssetsOfType(terrainSettings, typeof(GPUInstancerTreePrototype));
		}
		for (int i = 0; i < treePrototypes.Length; i++)
		{
			if (forceNew || treeIntancePrototypes.Count <= i)
			{
				AddTreeInstancePrototypeFromTerrainPrototype(gameObject, treeIntancePrototypes, treePrototypes[i], i, terrainSettings);
			}
		}
		RemoveUnusedAssets(terrainSettings, treeIntancePrototypes, typeof(GPUInstancerTreePrototype));
	}

	public static void AddTreeInstancePrototypeFromTerrainPrototype(GameObject gameObject, List<GPUInstancerPrototype> treeInstancePrototypes, TreePrototype terrainTreePrototype, int treeIndex, GPUInstancerTerrainSettings terrainSettings)
	{
		GPUInstancerTreePrototype gPUInstancerTreePrototype = ScriptableObject.CreateInstance<GPUInstancerTreePrototype>();
		gPUInstancerTreePrototype.prototypeIndex = treeIndex;
		gPUInstancerTreePrototype.prefabObject = terrainTreePrototype.prefab;
		gPUInstancerTreePrototype.name = "Tree_" + treeIndex + "_" + terrainTreePrototype.prefab.name;
		gPUInstancerTreePrototype.maxDistance = terrainSettings.maxTreeDistance;
		gPUInstancerTreePrototype.useOriginalShaderForShadow = true;
		DetermineTreePrototypeType(gPUInstancerTreePrototype);
		if (gPUInstancerTreePrototype.treeType == GPUInstancerTreeType.None)
		{
			gPUInstancerTreePrototype.treeType = GPUInstancerTreeType.MeshTree;
		}
		gPUInstancerTreePrototype.isLODCrossFade = true;
		if (!GClass1262.gpuiSettings.disableAutoGenerateBillboards && gPUInstancerTreePrototype.treeType != GPUInstancerTreeType.SpeedTree8)
		{
			gPUInstancerTreePrototype.useGeneratedBillboard = true;
			if (gPUInstancerTreePrototype.billboard == null)
			{
				gPUInstancerTreePrototype.billboard = new GPUInstancerBillboard();
			}
			GeneratePrototypeBillboard(gPUInstancerTreePrototype);
		}
		AddObjectToAsset(terrainSettings, gPUInstancerTreePrototype);
		treeInstancePrototypes.Add(gPUInstancerTreePrototype);
		GenerateInstancedShadersForGameObject(gPUInstancerTreePrototype);
	}

	public static void AddTreeInstanceRuntimeDataToList(List<GClass1270> runtimeDataList, List<GPUInstancerPrototype> treePrototypes, GPUInstancerTerrainSettings terrainSettings)
	{
		for (int i = 0; i < treePrototypes.Count; i++)
		{
			GPUInstancerTreePrototype gPUInstancerTreePrototype = (GPUInstancerTreePrototype)treePrototypes[i];
			if (GClass1262.gpuiSettings.isLWRP || GClass1262.gpuiSettings.isHDRP)
			{
				gPUInstancerTreePrototype.useOriginalShaderForShadow = true;
			}
			GClass1270 gClass = new GClass1270(gPUInstancerTreePrototype);
			if (gClass.CreateRenderersFromGameObject(gPUInstancerTreePrototype))
			{
				AddBillboardToRuntimeData(gClass);
				if (gPUInstancerTreePrototype.treeType == GPUInstancerTreeType.SpeedTree || gPUInstancerTreePrototype.treeType == GPUInstancerTreeType.SpeedTree8 || gPUInstancerTreePrototype.treeType == GPUInstancerTreeType.TreeCreatorTree)
				{
					GPUInstancerManager.AddTreeProxy(gPUInstancerTreePrototype, gClass);
				}
				gClass.hasShadowCasterBuffer = gPUInstancerTreePrototype.isShadowCasting;
				runtimeDataList.Add(gClass);
			}
		}
	}

	public static void DetermineTreePrototypeType(GPUInstancerPrototype prototype)
	{
		if (prototype.prefabObject != null)
		{
			if (prototype.prefabObject.GetComponent<MeshFilter>() != null && prototype.prefabObject.GetComponent<MeshRenderer>() != null && prototype.prefabObject.GetComponent<MeshRenderer>().sharedMaterials != null && prototype.prefabObject.GetComponent<MeshRenderer>().sharedMaterials.Length != 0)
			{
				if (prototype.prefabObject.GetComponent<MeshRenderer>().sharedMaterials[0].shader.name.Contains("Tree Creator"))
				{
					prototype.treeType = GPUInstancerTreeType.TreeCreatorTree;
					return;
				}
				if (prototype.prefabObject.GetComponent<MeshRenderer>().sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE || prototype.prefabObject.GetComponent<MeshRenderer>().sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE)
				{
					prototype.treeType = GPUInstancerTreeType.SpeedTree;
					return;
				}
				if (prototype.prefabObject.GetComponent<MeshRenderer>().sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE_8 || prototype.prefabObject.GetComponent<MeshRenderer>().sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE_8)
				{
					prototype.treeType = GPUInstancerTreeType.SpeedTree8;
					ImportSpeedTree8Shader();
					return;
				}
			}
			if (prototype.prefabObject.GetComponent<LODGroup>() != null && prototype.prefabObject.GetComponent<LODGroup>().GetLODs() != null && prototype.prefabObject.GetComponent<LODGroup>().GetLODs().Length != 0 && prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers != null && prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers.Length != 0 && prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers[0].sharedMaterials != null && prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers[0].sharedMaterials.Length != 0)
			{
				if (prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers[0].sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE || prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers[0].sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE)
				{
					prototype.treeType = GPUInstancerTreeType.SpeedTree;
					return;
				}
				if (prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers[0].sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE_8 || prototype.prefabObject.GetComponent<LODGroup>().GetLODs()[0].renderers[0].sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE_8)
				{
					prototype.treeType = GPUInstancerTreeType.SpeedTree8;
					ImportSpeedTree8Shader();
					return;
				}
			}
			if (prototype.prefabObject.GetComponentsInChildren<MeshRenderer>().Any((MeshRenderer mr) => mr.sharedMaterials.Where((Material m) => m.shader.name == GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_BARK || m.shader.name == GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_BARK || m.shader.name == GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_LEAVES || m.shader.name == GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_LEAVES).FirstOrDefault()))
			{
				prototype.treeType = GPUInstancerTreeType.SoftOcclusionTree;
				return;
			}
		}
		prototype.treeType = GPUInstancerTreeType.None;
	}

	public static void ImportSpeedTree8Shader()
	{
	}

	public static void ImportSpeedTree8ShaderPopup()
	{
	}

	public static Mesh CreateCrossQuadsMeshForDetailGrass(float width, float height, string name, int quality)
	{
		GameObject gameObject = new GameObject(name, typeof(MeshFilter));
		gameObject.transform.position = Vector3.zero;
		CombineInstance[] array = new CombineInstance[quality];
		for (int i = 0; i < quality; i++)
		{
			GameObject gameObject2 = new GameObject("quadToCombine_" + i, typeof(MeshFilter));
			Mesh mesh = GenerateQuadMesh(width, height, new Rect(0f, 0f, 1f, 1f), centerPivotAtBottom: true);
			for (int j = 0; j < mesh.normals.Length; j++)
			{
				mesh.normals[i] = Vector3.up;
			}
			gameObject2.GetComponent<MeshFilter>().sharedMesh = mesh;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity * Quaternion.AngleAxis(180f / (float)quality * (float)i, Vector3.up);
			gameObject2.transform.localScale = Vector3.one;
			array[i] = new CombineInstance
			{
				mesh = gameObject2.GetComponent<MeshFilter>().sharedMesh,
				transform = gameObject2.transform.localToWorldMatrix
			};
		}
		gameObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
		gameObject.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: true);
		Mesh sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		sharedMesh.name = name;
		UnityEngine.Object.DestroyImmediate(gameObject);
		return sharedMesh;
	}

	public static Mesh GenerateQuadMesh(float width, float height, Rect? uvRect = null, bool centerPivotAtBottom = false, float pivotOffsetX = 0f, float pivotOffsetY = 0f)
	{
		Mesh mesh = new Mesh();
		mesh.name = "QuadMesh";
		mesh.vertices = new Vector3[4]
		{
			new Vector3(centerPivotAtBottom ? ((0f - width) / 2f - pivotOffsetX) : (0f - pivotOffsetX), 0f - pivotOffsetY, 0f),
			new Vector3(centerPivotAtBottom ? ((0f - width) / 2f - pivotOffsetX) : (0f - pivotOffsetX), height - pivotOffsetY, 0f),
			new Vector3(centerPivotAtBottom ? (width / 2f - pivotOffsetX) : (width - pivotOffsetX), height - pivotOffsetY, 0f),
			new Vector3(centerPivotAtBottom ? (width / 2f - pivotOffsetX) : (width - pivotOffsetX), 0f - pivotOffsetY, 0f)
		};
		if (uvRect.HasValue)
		{
			mesh.uv = new Vector2[4]
			{
				new Vector2(uvRect.Value.x, uvRect.Value.y),
				new Vector2(uvRect.Value.x, uvRect.Value.y + uvRect.Value.height),
				new Vector2(uvRect.Value.x + uvRect.Value.width, uvRect.Value.y + uvRect.Value.height),
				new Vector2(uvRect.Value.x + uvRect.Value.width, uvRect.Value.y)
			};
		}
		mesh.triangles = new int[6] { 0, 1, 3, 1, 2, 3 };
		Vector3 vector = new Vector3(0f, 0f, -1f);
		Vector4 vector2 = new Vector4(1f, 0f, 0f, -1f);
		mesh.normals = new Vector3[4] { vector, vector, vector, vector };
		mesh.tangents = new Vector4[4] { vector2, vector2, vector2, vector2 };
		Color[] array = new Color[mesh.vertices.Length];
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			array[i] = Color.Lerp(Color.clear, Color.red, mesh.vertices[i].y);
		}
		mesh.colors = array;
		return mesh;
	}

	public static List<int[]> GetDetailMapsFromTerrain(Terrain terrain, List<GPUInstancerPrototype> detailPrototypeList)
	{
		List<int[]> list = new List<int[]>();
		for (int i = 0; i < detailPrototypeList.Count; i++)
		{
			int[,] detailLayer = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailResolution, terrain.terrainData.detailResolution, i);
			list.Add(new int[detailLayer.GetLength(0) * detailLayer.GetLength(1)]);
			for (int j = 0; j < detailLayer.GetLength(0); j++)
			{
				for (int k = 0; k < detailLayer.GetLength(1); k++)
				{
					list[i][k + j * detailLayer.GetLength(0)] = detailLayer[j, k];
				}
			}
		}
		return list;
	}

	public static Bounds GenerateBoundsFromTerrainPositionAndSize(Vector3 position, Vector3 size)
	{
		return new Bounds(new Vector3(position.x + size.x / 2f, position.y + size.y / 2f, position.z + size.z / 2f), size);
	}

	public static float SampleTerrainHeight(float px, float py, float leftBottomH, float leftTopH, float rightBottomH, float rightTopH)
	{
		return Mathf.Lerp(Mathf.Lerp(leftBottomH, rightBottomH, px), Mathf.Lerp(leftTopH, rightTopH, px), py);
	}

	public static Vector3 ComputeTerrainNormal(float leftBottomH, float leftTopH, float rightBottomH, float scale)
	{
		Vector3 vector = new Vector3(0f, leftBottomH * scale, 0f);
		Vector3 vector2 = new Vector3(0f, leftTopH * scale, 1f);
		Vector3 vector3 = new Vector3(1f, rightBottomH * scale, 0f);
		return Vector3.Cross(vector2 - vector3, vector3 - vector).normalized;
	}

	public static int GCD(int[] numbers)
	{
		return numbers.Aggregate(GCD);
	}

	public static int GCD(int a, int b)
	{
		if (b != 0)
		{
			return GCD(b, a % b);
		}
		return a;
	}

	public static IEnumerable<int> GetDivisors(int n)
	{
		return from a in Enumerable.Range(2, n / 2)
			where n % a == 0
			select a;
	}

	public static void AssignBillboardBinding(GPUInstancerPrototype prototype)
	{
		if (prototype.billboard == null)
		{
			prototype.billboard = new GPUInstancerBillboard();
		}
		if (prototype.billboard.albedoAtlasTexture == null)
		{
			BillboardAtlasBinding billboardAtlasBinding = GClass1262.gpuiSettings.billboardAtlasBindings.GetBillboardAtlasBinding(prototype.prefabObject, prototype.billboard.atlasResolution, prototype.billboard.frameCount);
			if (billboardAtlasBinding != null)
			{
				prototype.billboard.albedoAtlasTexture = billboardAtlasBinding.albedoAtlasTexture;
				prototype.billboard.normalAtlasTexture = billboardAtlasBinding.normalAtlasTexture;
				prototype.billboard.quadSize = billboardAtlasBinding.quadSize;
				prototype.billboard.yPivotOffset = billboardAtlasBinding.yPivotOffset;
			}
		}
	}

	public static void GeneratePrototypeBillboard(GPUInstancerPrototype prototype, bool forceRegenerate = false)
	{
		if (prototype.billboard == null)
		{
			prototype.billboard = new GPUInstancerBillboard();
		}
		if (prototype.billboard.useCustomBillboard || GClass1262.gpuiSettings.isLWRP || GClass1262.gpuiSettings.isHDRP)
		{
			return;
		}
		DetermineTreePrototypeType(prototype);
		BillboardAtlasBinding billboardAtlasBinding = GClass1262.gpuiSettings.billboardAtlasBindings.GetBillboardAtlasBinding(prototype.prefabObject, prototype.billboard.atlasResolution, prototype.billboard.frameCount);
		if (billboardAtlasBinding != null)
		{
			if (!forceRegenerate)
			{
				prototype.billboard.albedoAtlasTexture = billboardAtlasBinding.albedoAtlasTexture;
				prototype.billboard.normalAtlasTexture = billboardAtlasBinding.normalAtlasTexture;
				prototype.billboard.quadSize = billboardAtlasBinding.quadSize;
				prototype.billboard.yPivotOffset = billboardAtlasBinding.yPivotOffset;
				return;
			}
			GClass1262.gpuiSettings.billboardAtlasBindings.RemoveBillboardAtlas(billboardAtlasBinding);
		}
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		try
		{
			RenderTexture active = RenderTexture.active;
			int num = prototype.billboard.atlasResolution / prototype.billboard.frameCount;
			prototype.billboard.albedoAtlasTexture = new Texture2D(prototype.billboard.atlasResolution, num);
			prototype.billboard.normalAtlasTexture = new Texture2D(prototype.billboard.atlasResolution, num);
			RenderTexture temporary = RenderTexture.GetTemporary(num, num, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			temporary.enableRandomWrite = true;
			temporary.Create();
			gameObject = UnityEngine.Object.Instantiate(prototype.prefabObject, Vector3.zero, Quaternion.identity);
			gameObject.transform.localScale = Vector3.one;
			gameObject.hideFlags = HideFlags.DontSave;
			int num2 = 31;
			MeshRenderer[] componentsInChildren = gameObject.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				Debug.LogError("Cannot create GPU Instancer billboard for " + prototype.name + " : no mesh renderers found in prototype prefab!");
				UnityEngine.Object.DestroyImmediate(gameObject);
				prototype.useGeneratedBillboard = false;
				return;
			}
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = num2;
				for (int j = 0; j < componentsInChildren[i].sharedMaterials.Length; j++)
				{
					if (componentsInChildren[i].sharedMaterials[j].HasProperty("_MainTexture"))
					{
						componentsInChildren[i].sharedMaterials[j].SetTexture(Int_27, componentsInChildren[i].sharedMaterials[j].GetTexture(Int_30));
					}
				}
				if (!componentsInChildren[i].enabled)
				{
					continue;
				}
				MeshFilter component = componentsInChildren[i].GetComponent<MeshFilter>();
				if (!(component == null) && !(component.sharedMesh == null) && component.sharedMesh.vertices != null)
				{
					Vector3[] vertices = component.sharedMesh.vertices;
					for (int k = 0; k < vertices.Length; k++)
					{
						bounds.Encapsulate(componentsInChildren[i].transform.localToWorldMatrix.MultiplyPoint3x4(vertices[k]));
					}
				}
			}
			float a = Mathf.Max(bounds.size.x, bounds.size.z) * 2f;
			a = Mathf.Max(a, bounds.size.y);
			Shader shader = Shader.Find(GClass1262.SHADER_GPUI_BILLBOARD_ALBEDO_BAKER);
			Shader shader2 = Shader.Find(GClass1262.SHADER_GPUI_BILLBOARD_NORMAL_BAKER);
			Shader.SetGlobalFloat(Int_31, prototype.billboard.billboardBrightness);
			Shader.SetGlobalFloat(Int_32, prototype.billboard.cutoffOverride);
			gameObject2 = new GameObject("GPUI_BillboardCameraPivot");
			Camera camera = new GameObject().AddComponent<Camera>();
			camera.transform.SetParent(gameObject2.transform);
			camera.gameObject.hideFlags = HideFlags.DontSave;
			camera.cullingMask = 1 << num2;
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = Color.clear;
			camera.orthographic = true;
			camera.nearClipPlane = 0.05f;
			camera.farClipPlane = a;
			camera.orthographicSize = a * 0.5f;
			camera.allowMSAA = false;
			camera.enabled = false;
			camera.renderingPath = RenderingPath.Forward;
			camera.targetTexture = temporary;
			camera.transform.localPosition = new Vector3(0f, bounds.center.y, (0f - a) / 2f);
			float num3 = 360f / (float)prototype.billboard.frameCount;
			for (int l = 0; l < prototype.billboard.frameCount; l++)
			{
				gameObject2.transform.rotation = Quaternion.AngleAxis(num3 * (float)l, Vector3.up);
				RenderTexture.active = temporary;
				camera.RenderWithShader(shader, string.Empty);
				prototype.billboard.albedoAtlasTexture.ReadPixels(new Rect(0f, 0f, num, num), l * num, 0);
				camera.RenderWithShader(shader2, string.Empty);
				prototype.billboard.normalAtlasTexture.ReadPixels(new Rect(0f, 0f, num, num), l * num, 0);
			}
			prototype.billboard.albedoAtlasTexture.Apply();
			prototype.billboard.normalAtlasTexture.Apply();
			prototype.billboard.albedoAtlasTexture = DilateBillboardTexture(prototype.billboard.albedoAtlasTexture, prototype.billboard.frameCount, isNormal: false);
			prototype.billboard.normalAtlasTexture = DilateBillboardTexture(prototype.billboard.normalAtlasTexture, prototype.billboard.frameCount, isNormal: true);
			prototype.billboard.quadSize = a;
			prototype.billboard.yPivotOffset = gameObject.transform.position.y + (a / 2f - bounds.extents.y - bounds.min.y);
			GClass1262.gpuiSettings.billboardAtlasBindings.AddBillboardAtlas(prototype.prefabObject, prototype.billboard.atlasResolution, prototype.billboard.frameCount, prototype.billboard.albedoAtlasTexture, prototype.billboard.normalAtlasTexture, prototype.billboard.quadSize, prototype.billboard.yPivotOffset);
			RenderTexture.active = active;
		}
		catch (Exception ex)
		{
			Debug.LogError("Error on billboard generation for: " + prototype);
			if ((bool)gameObject)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			if ((bool)gameObject2)
			{
				UnityEngine.Object.DestroyImmediate(gameObject2);
			}
			throw ex;
		}
		UnityEngine.Object.DestroyImmediate(gameObject);
		UnityEngine.Object.DestroyImmediate(gameObject2);
	}

	public static Texture2D DilateBillboardTexture(Texture2D billboardTexture, int frameCount, bool isNormal)
	{
		ComputeShader computeShader = (ComputeShader)Resources.Load(GClass1262.COMPUTE_BILLBOARD_RESOURCE_PATH);
		int kernelIndex = computeShader.FindKernel(GClass1262.COMPUTE_BILLBOARD_DILATION_KERNEL);
		RenderTexture temporary = RenderTexture.GetTemporary(billboardTexture.width, billboardTexture.height, 32, RenderTextureFormat.ARGB32);
		temporary.enableRandomWrite = true;
		temporary.Create();
		computeShader.SetTexture(kernelIndex, "result", temporary);
		computeShader.SetTexture(kernelIndex, "billboardSource", billboardTexture);
		computeShader.SetInts("billboardSize", billboardTexture.width, billboardTexture.height);
		computeShader.SetInt("frameCount", frameCount);
		computeShader.SetBool("isNormal", isNormal);
		computeShader.Dispatch(kernelIndex, Mathf.CeilToInt((float)billboardTexture.width / (GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D * (float)frameCount)), Mathf.CeilToInt((float)billboardTexture.height / GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D), frameCount);
		RenderTexture.active = temporary;
		Texture2D texture2D = new Texture2D(billboardTexture.width, billboardTexture.height);
		texture2D.ReadPixels(new Rect(0f, 0f, billboardTexture.width, billboardTexture.height), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		temporary.Release();
		return texture2D;
	}

	public static void AddBillboardToRuntimeData(GClass1270 runtimeData)
	{
		if (!runtimeData.prototype.useGeneratedBillboard || runtimeData.prototype.billboard == null)
		{
			return;
		}
		bool castShadows = false;
		if (runtimeData.prototype.billboard.useCustomBillboard && runtimeData.prototype.billboard.customBillboardInLODGroup)
		{
			return;
		}
		Mesh mesh;
		Material item;
		if (runtimeData.prototype.billboard.useCustomBillboard && runtimeData.prototype.billboard.customBillboardMesh != null && runtimeData.prototype.billboard.customBillboardMaterial != null)
		{
			mesh = runtimeData.prototype.billboard.customBillboardMesh;
			item = GClass1262.gpuiSettings.shaderBindings.GetInstancedMaterial(runtimeData.prototype.billboard.customBillboardMaterial);
			castShadows = runtimeData.prototype.billboard.isBillboardShadowCasting;
		}
		else
		{
			if (runtimeData.prototype.billboard.albedoAtlasTexture == null || runtimeData.prototype.billboard.normalAtlasTexture == null)
			{
				return;
			}
			mesh = GenerateQuadMesh(runtimeData.prototype.billboard.quadSize, runtimeData.prototype.billboard.quadSize, new Rect(0f, 0f, 1f, 1f), centerPivotAtBottom: true, 0f, runtimeData.prototype.billboard.yPivotOffset);
			item = GetBillboardMaterial(runtimeData.prototype);
		}
		if (runtimeData.prototype.treeType == GPUInstancerTreeType.SpeedTree || runtimeData.prototype.treeType == GPUInstancerTreeType.SpeedTree8)
		{
			LODGroup component = runtimeData.prototype.prefabObject.GetComponent<LODGroup>();
			if (component != null)
			{
				int num = 0;
				while (true)
				{
					if (num < component.GetLODs().Length)
					{
						bool flag = false;
						if (runtimeData.prototype.treeType == GPUInstancerTreeType.SpeedTree)
						{
							flag = component.GetLODs()[num].renderers.Any((Renderer r) => r.GetComponent<BillboardRenderer>() != null);
						}
						if (runtimeData.prototype.treeType == GPUInstancerTreeType.SpeedTree8)
						{
							flag = component.GetLODs()[num].renderers.Any((Renderer r) => r.sharedMaterials[0].IsKeywordEnabled("EFFECT_BILLBOARD"));
						}
						if (flag)
						{
							break;
						}
						num++;
						continue;
					}
					runtimeData.AddLodAndRenderer(mesh, new List<Material> { item }, new MaterialPropertyBlock(), castShadows, 0f, new MaterialPropertyBlock(), excludeBounds: true, runtimeData.prototype.prefabObject.layer);
					return;
				}
				runtimeData.AddLodAndRenderer(mesh, new List<Material> { item }, new MaterialPropertyBlock(), castShadows, component.GetLODs()[num].screenRelativeTransitionHeight, new MaterialPropertyBlock(), excludeBounds: true, runtimeData.prototype.prefabObject.layer);
				return;
			}
		}
		if (runtimeData.prototype.prefabObject.GetComponent<LODGroup>() != null && runtimeData.prototype.billboard.replaceLODCullWithBillboard)
		{
			runtimeData.AddLodAndRenderer(mesh, new List<Material> { item }, new MaterialPropertyBlock(), castShadows, 0f, new MaterialPropertyBlock(), excludeBounds: true, runtimeData.prototype.prefabObject.layer);
			return;
		}
		float num2 = (1f - runtimeData.prototype.billboard.billboardDistance) / QualitySettings.lodBias;
		int num3 = (runtimeData.instanceLODs.Count - 1) * 4;
		if (num2 > runtimeData.lodSizes[num3])
		{
			runtimeData.lodSizes[num3] = num2;
			if (runtimeData.prototype.isLODCrossFade && !runtimeData.prototype.isLODCrossFadeAnimate)
			{
				runtimeData.lodSizes[num3 + 2] = num2 + (1f - num2) * runtimeData.prototype.lodFadeTransitionWidth;
			}
		}
		runtimeData.AddLodAndRenderer(mesh, new List<Material> { item }, new MaterialPropertyBlock(), castShadows, 0f, new MaterialPropertyBlock(), excludeBounds: true, runtimeData.prototype.prefabObject.layer);
	}

	public static Material GetBillboardMaterial(GPUInstancerPrototype prototype)
	{
		Material material = null;
		switch (prototype.treeType)
		{
		case GPUInstancerTreeType.TreeCreatorTree:
		{
			material = new Material(Shader.Find(GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_TREECREATOR));
			MeshRenderer[] componentsInChildren = prototype.prefabObject.GetComponentsInChildren<MeshRenderer>();
			bool flag = false;
			for (int num = 0; num < componentsInChildren.Length; num++)
			{
				for (int num2 = 0; num2 < componentsInChildren[num].sharedMaterials.Length; num2++)
				{
					if (componentsInChildren[num].sharedMaterials[num2].shader.name == GClass1262.SHADER_UNITY_TREE_CREATOR_LEAVES_OPTIMIZED || componentsInChildren[num].sharedMaterials[num2].shader.name == GClass1262.SHADER_GPUI_TREE_CREATOR_LEAVES_OPTIMIZED)
					{
						material.SetColor(Int_38, componentsInChildren[num].sharedMaterials[num2].GetColor(Int_38));
						material.SetFloat(Int_39, componentsInChildren[num].sharedMaterials[num2].GetFloat(Int_39));
						material.SetFloat(Int_40, componentsInChildren[num].sharedMaterials[num2].GetFloat(Int_40));
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			break;
		}
		case GPUInstancerTreeType.SoftOcclusionTree:
			material = new Material(Shader.Find(GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_SOFTOCCLUSION));
			break;
		case GPUInstancerTreeType.SpeedTree:
		case GPUInstancerTreeType.SpeedTree8:
		{
			material = new Material(Shader.Find(GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_TREE));
			Renderer renderer = prototype.prefabObject.GetComponentsInChildren<MeshRenderer>().FirstOrDefault((MeshRenderer r) => r.sharedMaterials != null && r.sharedMaterials.Length != 0 && (r.sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE || r.sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE || r.sharedMaterials[0].shader.name == GClass1262.SHADER_UNITY_SPEED_TREE_8 || r.sharedMaterials[0].shader.name == GClass1262.SHADER_GPUI_SPEED_TREE_8));
			if (!(renderer != null))
			{
				break;
			}
			if (renderer.sharedMaterial.IsKeywordEnabled("EFFECT_HUE_VARIATION"))
			{
				material.EnableKeyword("SPDTREE_HUE_VARIATION");
				material.SetFloat(Int_34, 1f);
				if (renderer.sharedMaterial.HasProperty("_HueVariation"))
				{
					material.SetVector(Int_35, renderer.sharedMaterial.GetVector(Int_36));
				}
				if (renderer.sharedMaterial.HasProperty("_HueVariationColor"))
				{
					material.SetVector(Int_35, renderer.sharedMaterial.GetVector(Int_37));
				}
			}
			else
			{
				material.DisableKeyword("SPDTREE_HUE_VARIATION");
			}
			break;
		}
		}
		if (material == null)
		{
			MeshRenderer[] componentsInChildren2 = prototype.prefabObject.GetComponentsInChildren<MeshRenderer>();
			for (int num3 = 0; num3 < componentsInChildren2.Length; num3++)
			{
				for (int num4 = 0; num4 < componentsInChildren2[num3].sharedMaterials.Length; num4++)
				{
					if (componentsInChildren2[num3].sharedMaterials[num4].shader.name == GClass1262.SHADER_UNITY_STANDARD || componentsInChildren2[num3].sharedMaterials[num4].shader.name == GClass1262.SHADER_UNITY_STANDARD_SPECULAR || componentsInChildren2[num3].sharedMaterials[num4].shader.name == GClass1262.SHADER_GPUI_STANDARD || componentsInChildren2[num3].sharedMaterials[num4].shader.name == GClass1262.SHADER_GPUI_STANDARD_SPECULAR)
					{
						material = new Material(Shader.Find(GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_STANDARD));
						break;
					}
				}
				if (material != null)
				{
					break;
				}
			}
		}
		if (material == null)
		{
			material = new Material(Shader.Find(GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_TREE));
			material.DisableKeyword("SPDTREE_HUE_VARIATION");
		}
		material.SetTexture(Int_41, prototype.billboard.albedoAtlasTexture);
		material.SetTexture(Int_42, prototype.billboard.normalAtlasTexture);
		material.SetFloat(Int_43, prototype.billboard.frameCount);
		material.SetFloat(Int_44, 0.3f);
		material.DisableKeyword("_BILLBOARDFACECAMPOS_ON");
		if (prototype.billboard.billboardFaceCamPos)
		{
			material.EnableKeyword("_BILLBOARDFACECAMPOS_ON");
		}
		return material;
	}

	public static string GetBillboardShaderName(GPUInstancerPrototype prototype)
	{
		if (prototype.billboard == null)
		{
			return null;
		}
		if (prototype.billboard.useCustomBillboard && prototype.billboard.customBillboardMaterial != null && prototype.billboard.customBillboardMaterial.shader != null)
		{
			return prototype.billboard.customBillboardMaterial.shader.name;
		}
		string text = null;
		MeshRenderer[] componentsInChildren = prototype.prefabObject.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			for (int j = 0; j < componentsInChildren[i].sharedMaterials.Length; j++)
			{
				if (!(componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_UNITY_STANDARD) && !(componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_UNITY_STANDARD_SPECULAR) && !(componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_GPUI_STANDARD) && !(componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_GPUI_STANDARD_SPECULAR))
				{
					if (!(componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_UNITY_TREE_CREATOR_LEAVES_OPTIMIZED) && !(componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_GPUI_TREE_CREATOR_LEAVES_OPTIMIZED))
					{
						if (componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_UNITY_TREE_SOFT_OCCLUSION_LEAVES || componentsInChildren[i].sharedMaterials[j].shader.name == GClass1262.SHADER_GPUI_TREE_SOFT_OCCLUSION_LEAVES)
						{
							text = GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_SOFTOCCLUSION;
							break;
						}
						continue;
					}
					text = GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_TREECREATOR;
					break;
				}
				text = GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_STANDARD;
				break;
			}
			if (text != null)
			{
				break;
			}
		}
		if (text == null)
		{
			text = GClass1262.SHADER_GPUI_BILLBOARD_2D_RENDERER_TREE;
		}
		return text;
	}

	public static bool IsBillboardGeneratedByDefault(GPUInstancerPrototype prototype)
	{
		if (prototype.treeType != GPUInstancerTreeType.SpeedTree && prototype.treeType != GPUInstancerTreeType.TreeCreatorTree)
		{
			if (prototype.billboard != null)
			{
				return prototype.billboard.useCustomBillboard;
			}
			return false;
		}
		return true;
	}

	public static void ShowBillboardQuad(GPUInstancerPrototype prototype, Vector3 quadPos)
	{
		if (prototype.billboard.useCustomBillboard)
		{
			if (prototype.billboard.customBillboardMesh != null && prototype.billboard.customBillboardMaterial != null)
			{
				GameObject obj = new GameObject
				{
					name = "GPUI Billboard (" + prototype.name + ")"
				};
				MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
				meshRenderer.shadowCastingMode = (prototype.billboard.isBillboardShadowCasting ? ShadowCastingMode.On : ShadowCastingMode.Off);
				meshFilter.mesh = prototype.billboard.customBillboardMesh;
				meshRenderer.sharedMaterial = prototype.billboard.customBillboardMaterial;
			}
		}
		else
		{
			GameObject obj2 = new GameObject
			{
				name = "GPUI Billboard (" + prototype.name + ")"
			};
			MeshFilter meshFilter2 = obj2.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer2 = obj2.AddComponent<MeshRenderer>();
			meshRenderer2.shadowCastingMode = ShadowCastingMode.Off;
			meshFilter2.mesh = GenerateQuadMesh(prototype.billboard.quadSize, prototype.billboard.quadSize, new Rect(0f, 0f, 1f, 1f), centerPivotAtBottom: true, 0f, prototype.billboard.yPivotOffset);
			meshRenderer2.sharedMaterial = GetBillboardMaterial(prototype);
		}
	}

	public static void RemoveAssetsOfType(UnityEngine.Object baseAsset, Type type)
	{
	}

	public static void RemoveUnusedAssets<T>(UnityEngine.Object baseAsset, List<T> prototypeList, Type prototypeType) where T : GPUInstancerPrototype
	{
	}

	public static void AddObjectToAsset(UnityEngine.Object baseAsset, UnityEngine.Object objectToAdd)
	{
	}

	public static void SetPrototypeListFromAssets<T>(UnityEngine.Object baseAsset, List<T> prototypeList, Type prototypeType) where T : GPUInstancerPrototype
	{
	}

	public static string GetAssetGUID(UnityEngine.Object assetObject)
	{
		return null;
	}

	public static void CalculateSpatialPartitioningValuesFromTerrain(GClass1273<GClass1258> spData, Terrain terrain, float maxDetailDistance, float preferedCellSize = 0f)
	{
		if (preferedCellSize == 0f)
		{
			preferedCellSize = maxDetailDistance / 2f;
		}
		float num = Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z);
		spData.cellRowAndCollumnCountPerTerrain = Mathf.FloorToInt(num / preferedCellSize);
		if (spData.cellRowAndCollumnCountPerTerrain == 0)
		{
			spData.cellRowAndCollumnCountPerTerrain = 1;
		}
		else if (terrain.terrainData.detailResolution % spData.cellRowAndCollumnCountPerTerrain != 0 || (terrain.terrainData.heightmapResolution - 1) % spData.cellRowAndCollumnCountPerTerrain != 0)
		{
			int num2 = GCD(terrain.terrainData.detailResolution, terrain.terrainData.heightmapResolution - 1);
			List<int> list = GetDivisors(num2).ToList();
			list.Add(num2);
			list.RemoveAll((int d) => d > spData.cellRowAndCollumnCountPerTerrain);
			if (list.Any())
			{
				spData.cellRowAndCollumnCountPerTerrain = list.Last();
			}
		}
		float num3 = terrain.terrainData.size.x / (float)spData.cellRowAndCollumnCountPerTerrain;
		float y = terrain.terrainData.size.y;
		float num4 = terrain.terrainData.size.z / (float)spData.cellRowAndCollumnCountPerTerrain;
		float num5 = maxDetailDistance * 2.5f;
		for (int num6 = 0; num6 < spData.cellRowAndCollumnCountPerTerrain; num6++)
		{
			for (int num7 = 0; num7 < spData.cellRowAndCollumnCountPerTerrain; num7++)
			{
				GClass1259 gClass = new GClass1259(num7, num6);
				gClass.cellBounds = new Bounds(new Vector3(terrain.transform.position.x + (float)num7 * num3 + num3 / 2f, terrain.transform.position.y + y / 2f, terrain.transform.position.z + (float)num6 * num4 + num4 / 2f), new Vector3(num3 + num5, y + num5, num4 + num5));
				gClass.cellInnerBounds = new Bounds(new Vector3(terrain.transform.position.x + (float)num7 * num3 + num3 / 2f, terrain.transform.position.y + y / 2f, terrain.transform.position.z + (float)num6 * num4 + num4 / 2f), new Vector3(num3, y, num4));
				gClass.instanceStartPosition = new Vector3(terrain.transform.position.x + (float)num7 * num3, terrain.transform.position.y, terrain.transform.position.z + (float)num6 * num4);
				spData.AddCell(gClass);
			}
		}
	}

	public static void GenerateInstancedShadersForGameObject(GPUInstancerPrototype prototype)
	{
		if (prototype.prefabObject == null)
		{
			return;
		}
		MeshRenderer[] componentsInChildren = prototype.prefabObject.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				if (sharedMaterials[j] == null || sharedMaterials[j].shader == null)
				{
					continue;
				}
				if (GClass1262.gpuiSettings.shaderBindings.IsShadersInstancedVersionExists(sharedMaterials[j].shader.name))
				{
					if (!GClass1262.gpuiSettings.disableAutoVariantHandling)
					{
						GClass1262.gpuiSettings.AddShaderVariantToCollection(sharedMaterials[j]);
					}
				}
				else
				{
					if (Application.isPlaying)
					{
						continue;
					}
					if (IsShaderInstanced(sharedMaterials[j].shader))
					{
						GClass1262.gpuiSettings.shaderBindings.AddShaderInstance(sharedMaterials[j].shader.name, sharedMaterials[j].shader, isOriginalInstanced: true);
						if (!GClass1262.gpuiSettings.disableAutoVariantHandling)
						{
							GClass1262.gpuiSettings.AddShaderVariantToCollection(sharedMaterials[j]);
						}
					}
					else
					{
						if (GClass1262.gpuiSettings.disableAutoShaderConversion)
						{
							continue;
						}
						Shader shader = CreateInstancedShader(sharedMaterials[j].shader);
						if (shader != null)
						{
							GClass1262.gpuiSettings.shaderBindings.AddShaderInstance(sharedMaterials[j].shader.name, shader);
							if (!GClass1262.gpuiSettings.disableAutoVariantHandling)
							{
								GClass1262.gpuiSettings.AddShaderVariantToCollection(sharedMaterials[j]);
							}
						}
					}
				}
			}
		}
		if (prototype.useGeneratedBillboard && prototype.billboard != null && !GClass1262.gpuiSettings.disableAutoVariantHandling)
		{
			GClass1262.gpuiSettings.AddShaderVariantToCollection(GetBillboardMaterial(prototype));
		}
	}

	public static bool IsShaderInstanced(Shader shader)
	{
		return false;
	}

	public static Shader CreateInstancedShader(Shader originalShader, bool useOriginal = false)
	{
		return null;
	}

	public static T[] MirrorAndFlatten<T>(this T[,] array2D)
	{
		T[] array = new T[array2D.GetLength(0) * array2D.GetLength(1)];
		for (int i = 0; i < array2D.GetLength(0); i++)
		{
			for (int j = 0; j < array2D.GetLength(1); j++)
			{
				array[j + i * array2D.GetLength(0)] = array2D[i, j];
			}
		}
		return array;
	}

	public static T[] MirrorAndFlatten<T>(this T[,] array2D, int xBase, int yBase, int width, int height)
	{
		T[] array = new T[width * height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				array[j + i * width] = array2D[i + yBase, j + xBase];
			}
		}
		return array;
	}

	public static float Range(this System.Random prng, float min, float max)
	{
		return (float)((double)min + prng.NextDouble() * (double)(max - min));
	}

	public static void Matrix4x4ToFloatArray(this Matrix4x4 matrix4x4, float[] floatArray)
	{
		floatArray[0] = matrix4x4[0, 0];
		floatArray[1] = matrix4x4[1, 0];
		floatArray[2] = matrix4x4[2, 0];
		floatArray[3] = matrix4x4[3, 0];
		floatArray[4] = matrix4x4[0, 1];
		floatArray[5] = matrix4x4[1, 1];
		floatArray[6] = matrix4x4[2, 1];
		floatArray[7] = matrix4x4[3, 1];
		floatArray[8] = matrix4x4[0, 2];
		floatArray[9] = matrix4x4[1, 2];
		floatArray[10] = matrix4x4[2, 2];
		floatArray[11] = matrix4x4[3, 2];
		floatArray[12] = matrix4x4[0, 3];
		floatArray[13] = matrix4x4[1, 3];
		floatArray[14] = matrix4x4[2, 3];
		floatArray[15] = matrix4x4[3, 3];
	}

	public static Matrix4x4 Matrix4x4FromString(string matrixStr)
	{
		Matrix4x4 result = default(Matrix4x4);
		string[] array = matrixStr.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			result[i / 4, i % 4] = float.Parse(array[i]);
		}
		return result;
	}

	public static string Matrix4x4ToString(Matrix4x4 matrix4x4)
	{
		string text = matrix4x4.ToString().Replace("\n", ";").Replace("\t", ";");
		return text.Substring(0, text.Length - 1);
	}

	public static void SetMatrix4x4ToTransform(this Transform transform, Matrix4x4 matrix)
	{
		transform.position = matrix.GetColumn(3);
		transform.localScale = new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
		transform.rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
	}

	public static float[] Matrix4x4ToFloatArray(this Matrix4x4 matrix4x4)
	{
		float[] array = new float[16];
		Matrix4x4ToFloatArray(matrix4x4, array);
		return array;
	}

	public static void SetDataSingle(this ComputeBuffer computeBuffer, Matrix4x4[] data, int managedBufferStartIndex, int computeBufferStartIndex)
	{
		computeBuffer.SetData(data, managedBufferStartIndex, computeBufferStartIndex, 1);
	}

	public static void SetDataPartial(this ComputeBuffer computeBuffer, Matrix4x4[] data, int managedBufferStartIndex, int computeBufferStartIndex, int count, ComputeBuffer managedBuffer = null, Matrix4x4[] managedData = null)
	{
		if (managedBufferStartIndex == 0 && computeBufferStartIndex == 0 && count == data.Length)
		{
			computeBuffer.SetData(data);
		}
		computeBuffer.SetData(data, managedBufferStartIndex, computeBufferStartIndex, count);
	}

	public static void CopyComputeBuffer(this ComputeBuffer computeBuffer, int computeBufferStartIndex, int count, ComputeBuffer managedBuffer)
	{
		GClass1262.computeBufferSetDataPartial.SetBuffer(GClass1262.computeBufferSetDataPartialKernelId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, computeBuffer);
		GClass1262.computeBufferSetDataPartial.SetBuffer(GClass1262.computeBufferSetDataPartialKernelId, GClass1262.GClass1265.BUFFER_PARAMETER_MANAGED_BUFFER_DATA, managedBuffer);
		GClass1262.computeBufferSetDataPartial.SetInt(GClass1262.GClass1265.BUFFER_PARAMETER_COMPUTE_BUFFER_START_INDEX, computeBufferStartIndex);
		GClass1262.computeBufferSetDataPartial.SetInt(GClass1262.GClass1265.BUFFER_PARAMETER_COUNT, count);
		GClass1262.computeBufferSetDataPartial.Dispatch(GClass1262.computeBufferSetDataPartialKernelId, Mathf.CeilToInt((float)count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
	}

	public static ComputeBuffer MergeComputeBuffers(this ComputeBuffer computeBuffer, ComputeBuffer bufferToMerge, bool releaseMergedBuffers)
	{
		ComputeBuffer computeBuffer2 = new ComputeBuffer(computeBuffer.count + bufferToMerge.count, computeBuffer.stride);
		GClass1262.computeBufferSetDataPartial.SetBuffer(GClass1262.computeBufferSetDataPartialKernelId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, computeBuffer2);
		GClass1262.computeBufferSetDataPartial.SetBuffer(GClass1262.computeBufferSetDataPartialKernelId, GClass1262.GClass1265.BUFFER_PARAMETER_MANAGED_BUFFER_DATA, computeBuffer);
		GClass1262.computeBufferSetDataPartial.SetInt(GClass1262.GClass1265.BUFFER_PARAMETER_COMPUTE_BUFFER_START_INDEX, 0);
		GClass1262.computeBufferSetDataPartial.SetInt(GClass1262.GClass1265.BUFFER_PARAMETER_COUNT, computeBuffer.count);
		GClass1262.computeBufferSetDataPartial.Dispatch(GClass1262.computeBufferSetDataPartialKernelId, Mathf.CeilToInt((float)computeBuffer.count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
		GClass1262.computeBufferSetDataPartial.SetBuffer(GClass1262.computeBufferSetDataPartialKernelId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, computeBuffer2);
		GClass1262.computeBufferSetDataPartial.SetBuffer(GClass1262.computeBufferSetDataPartialKernelId, GClass1262.GClass1265.BUFFER_PARAMETER_MANAGED_BUFFER_DATA, bufferToMerge);
		GClass1262.computeBufferSetDataPartial.SetInt(GClass1262.GClass1265.BUFFER_PARAMETER_COMPUTE_BUFFER_START_INDEX, computeBuffer.count);
		GClass1262.computeBufferSetDataPartial.SetInt(GClass1262.GClass1265.BUFFER_PARAMETER_COUNT, bufferToMerge.count);
		GClass1262.computeBufferSetDataPartial.Dispatch(GClass1262.computeBufferSetDataPartialKernelId, Mathf.CeilToInt((float)bufferToMerge.count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
		if (releaseMergedBuffers)
		{
			computeBuffer.Release();
			bufferToMerge.Release();
		}
		return computeBuffer2;
	}

	public static void SetGlobalPositionOffset(GPUInstancerManager manager, Vector3 offsetPosition)
	{
		if (manager.runtimeDataList == null)
		{
			return;
		}
		manager.SetGlobalPositionOffset(offsetPosition);
		foreach (GClass1270 runtimeData in manager.runtimeDataList)
		{
			if (runtimeData != null)
			{
				if (runtimeData.instanceCount != 0 && runtimeData.bufferSize != 0)
				{
					if (runtimeData.transformationMatrixVisibilityBuffer != null)
					{
						GClass1262.computeRuntimeModification.SetBuffer(GClass1262.computeBufferTransformOffsetId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, runtimeData.transformationMatrixVisibilityBuffer);
						GClass1262.computeRuntimeModification.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, runtimeData.bufferSize);
						GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1269.BUFFER_PARAMETER_POSITION_OFFSET, offsetPosition);
						GClass1262.computeRuntimeModification.Dispatch(GClass1262.computeBufferTransformOffsetId, Mathf.CeilToInt((float)runtimeData.bufferSize / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
						continue;
					}
					Debug.LogWarning("SetGlobalPositionOffset called before buffers are initialized. Offset will not be applied.");
					break;
				}
				break;
			}
			Debug.LogWarning("SetGlobalPositionOffset called before manager initialization. Offset will not be applied.");
			break;
		}
	}

	public static void RemoveInstancesInsideBounds(ComputeBuffer instanceDataBuffer, Vector3 center, Vector3 extents, float offset)
	{
		if (instanceDataBuffer != null)
		{
			GClass1262.computeRuntimeModification.SetBuffer(GClass1262.computeRemoveInsideBoundsId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, instanceDataBuffer);
			GClass1262.computeRuntimeModification.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, instanceDataBuffer.count);
			GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_CENTER, center);
			GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_EXTENTS, extents + Vector3.one * offset);
			GClass1262.computeRuntimeModification.Dispatch(GClass1262.computeRemoveInsideBoundsId, Mathf.CeilToInt((float)instanceDataBuffer.count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
		}
	}

	public static void RemoveInstancesInsideBoxCollider(ComputeBuffer instanceDataBuffer, BoxCollider boxCollider, float offset)
	{
		if (instanceDataBuffer != null)
		{
			GClass1262.computeRuntimeModification.SetBuffer(GClass1262.computeRemoveInsideBoxId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, instanceDataBuffer);
			GClass1262.computeRuntimeModification.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, instanceDataBuffer.count);
			GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_CENTER, boxCollider.center);
			GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_EXTENTS, boxCollider.size / 2f + Vector3.one * offset);
			GClass1262.computeRuntimeModification.SetMatrix(GClass1262.GClass1269.BUFFER_PARAMETER_MODIFIER_TRANSFORM, boxCollider.transform.localToWorldMatrix);
			GClass1262.computeRuntimeModification.Dispatch(GClass1262.computeRemoveInsideBoxId, Mathf.CeilToInt((float)instanceDataBuffer.count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
		}
	}

	public static void RemoveInstancesInsideSphereCollider(ComputeBuffer instanceDataBuffer, SphereCollider sphereCollider, float offset)
	{
		if (instanceDataBuffer != null)
		{
			GClass1262.computeRuntimeModification.SetBuffer(GClass1262.computeRemoveInsideSphereId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, instanceDataBuffer);
			GClass1262.computeRuntimeModification.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, instanceDataBuffer.count);
			GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_CENTER, sphereCollider.center + sphereCollider.transform.position);
			GClass1262.computeRuntimeModification.SetFloat(GClass1262.GClass1269.BUFFER_PARAMETER_MODIFIER_RADIUS, sphereCollider.radius * Mathf.Max(Mathf.Max(sphereCollider.transform.localScale.x, sphereCollider.transform.localScale.y), sphereCollider.transform.localScale.z) + offset);
			GClass1262.computeRuntimeModification.Dispatch(GClass1262.computeRemoveInsideSphereId, Mathf.CeilToInt((float)instanceDataBuffer.count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
		}
	}

	public static void RemoveInstancesInsideCapsuleCollider(ComputeBuffer instanceDataBuffer, CapsuleCollider capsuleCollider, float offset)
	{
		if (instanceDataBuffer != null)
		{
			GClass1262.computeRuntimeModification.SetBuffer(GClass1262.computeRemoveInsideCapsuleId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, instanceDataBuffer);
			GClass1262.computeRuntimeModification.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, instanceDataBuffer.count);
			GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1264.BUFFER_PARAMETER_BOUNDS_CENTER, capsuleCollider.center);
			GClass1262.computeRuntimeModification.SetFloat(GClass1262.GClass1269.BUFFER_PARAMETER_MODIFIER_RADIUS, capsuleCollider.radius * Mathf.Max(Mathf.Max((capsuleCollider.direction == 0) ? 0f : capsuleCollider.transform.localScale.x, (capsuleCollider.direction == 1) ? 0f : capsuleCollider.transform.localScale.y), (capsuleCollider.direction == 2) ? 0f : capsuleCollider.transform.localScale.z) + offset);
			GClass1262.computeRuntimeModification.SetFloat(GClass1262.GClass1269.BUFFER_PARAMETER_MODIFIER_HEIGHT, capsuleCollider.height * ((capsuleCollider.direction == 0) ? capsuleCollider.transform.localScale.x : ((capsuleCollider.direction == 1) ? capsuleCollider.transform.localScale.y : ((capsuleCollider.direction == 2) ? capsuleCollider.transform.localScale.z : 0f))));
			GClass1262.computeRuntimeModification.SetMatrix(GClass1262.GClass1269.BUFFER_PARAMETER_MODIFIER_TRANSFORM, capsuleCollider.transform.localToWorldMatrix);
			GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1269.BUFFER_PARAMETER_MODIFIER_AXIS, (capsuleCollider.direction == 0) ? Vector3.right : ((capsuleCollider.direction == 1) ? Vector3.up : Vector3.forward));
			GClass1262.computeRuntimeModification.Dispatch(GClass1262.computeRemoveInsideCapsuleId, Mathf.CeilToInt((float)instanceDataBuffer.count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
		}
	}

	public static void InitializeWithMatrix4x4Array(GPUInstancerPrefabManager prefabManager, GPUInstancerPrefabPrototype prototype, Matrix4x4[] matrix4x4Array)
	{
		prototype.enableRuntimeModifications = false;
		prefabManager.InitializeRuntimeDataAndBuffers(forceNew: false);
		GClass1270 gClass = prefabManager.GetRuntimeData(prototype);
		if (gClass == null)
		{
			gClass = prefabManager.InitializeRuntimeDataForPrefabPrototype(prototype);
			if (gClass == null)
			{
				Debug.LogError("Can not find runtime data for prototype: " + prototype?.ToString() + ". Please check if the prototype was added to the Prefab Manager.");
				return;
			}
		}
		gClass.instanceDataArray = matrix4x4Array;
		gClass.bufferSize = matrix4x4Array.Length;
		gClass.instanceCount = matrix4x4Array.Length;
		ReleaseInstanceBuffers(gClass);
		if (prototype.treeType == GPUInstancerTreeType.SpeedTree || prototype.treeType == GPUInstancerTreeType.SpeedTree8 || prototype.treeType == GPUInstancerTreeType.TreeCreatorTree)
		{
			GPUInstancerManager.AddTreeProxy(prototype, gClass);
		}
		InitializeGPUBuffer(gClass);
	}

	public static void UpdateVisibilityBufferWithMatrix4x4Array(GPUInstancerPrefabManager prefabManager, GPUInstancerPrefabPrototype prototype, Matrix4x4[] matrix4x4Array, int arrayStartIndex = 0, int bufferStartIndex = 0, int count = 0)
	{
		GClass1270 runtimeData = prefabManager.GetRuntimeData(prototype, logError: true);
		if (runtimeData != null)
		{
			if (runtimeData.bufferSize == 0)
			{
				Debug.LogError("Can not find runtime data for prototype: " + prototype?.ToString() + ". Please check if the prototype was added to the Prefab Manager and the initialize method was called before update.");
			}
			else if (count > 0)
			{
				runtimeData.transformationMatrixVisibilityBuffer.SetData(matrix4x4Array, arrayStartIndex, bufferStartIndex, count);
			}
			else
			{
				runtimeData.transformationMatrixVisibilityBuffer.SetData(matrix4x4Array);
			}
		}
	}

	public static void CopyTextureWithComputeShader(Texture source, Texture destination, int offsetX, int sourceMip = 0, int destinationMip = 0, bool reverseZ = true)
	{
		GClass1262.computeTextureUtils.SetTexture(GClass1262.computeTextureUtilsCopyTextureId, GClass1262.GClass1266.SOURCE_TEXTURE, source, sourceMip);
		GClass1262.computeTextureUtils.SetTexture(GClass1262.computeTextureUtilsCopyTextureId, GClass1262.GClass1266.DESTINATION_TEXTURE, destination, destinationMip);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.OFFSET_X, offsetX);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.SOURCE_SIZE_X, source.width);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.SOURCE_SIZE_Y, source.height);
		GClass1262.computeTextureUtils.SetBool(GClass1262.GClass1266.REVERSE_Z, reverseZ);
		GClass1262.computeTextureUtils.Dispatch(GClass1262.computeTextureUtilsCopyTextureId, Mathf.CeilToInt((float)source.width / GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D), Mathf.CeilToInt((float)source.height / GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D), 1);
	}

	public static void ReduceTextureWithComputeShader(Texture source, Texture destination, int offsetX)
	{
		GClass1262.computeTextureUtils.SetTexture(1, GClass1262.GClass1266.SOURCE_TEXTURE, source);
		GClass1262.computeTextureUtils.SetTexture(1, GClass1262.GClass1266.DESTINATION_TEXTURE, destination);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.OFFSET_X, offsetX);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.SOURCE_SIZE_X, source.width);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.SOURCE_SIZE_Y, source.height);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.DESTINATION_SIZE_X, destination.width);
		GClass1262.computeTextureUtils.SetInt(GClass1262.GClass1266.DESTINATION_SIZE_Y, destination.height);
		GClass1262.computeTextureUtils.Dispatch(1, Mathf.CeilToInt((float)destination.width / GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D), Mathf.CeilToInt((float)destination.height / GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D), 1);
	}

	public static void StartListening(GPUInstancerEventType eventType, UnityAction listener)
	{
		if (Dictionary_0 == null)
		{
			Dictionary_0 = new Dictionary<GPUInstancerEventType, UnityEvent>();
		}
		UnityEvent value = null;
		if (Dictionary_0.TryGetValue(eventType, out value))
		{
			value.RemoveListener(listener);
			value.AddListener(listener);
		}
		else
		{
			value = new UnityEvent();
			value.AddListener(listener);
			Dictionary_0.Add(eventType, value);
		}
	}

	public static void StopListening(GPUInstancerEventType eventType, UnityAction listener)
	{
		if (Dictionary_0 != null)
		{
			UnityEvent value = null;
			if (Dictionary_0.TryGetValue(eventType, out value))
			{
				value.RemoveListener(listener);
			}
		}
	}

	public static void TriggerEvent(GPUInstancerEventType eventType)
	{
		if (Dictionary_0 != null && Dictionary_0.ContainsKey(eventType))
		{
			UnityEvent value = null;
			if (Dictionary_0.TryGetValue(eventType, out value))
			{
				value.Invoke();
			}
		}
	}

	public static void VersionControlCheckout(UnityEngine.Object assetObject)
	{
	}

	public static void VersionControlCheckout(string path)
	{
	}

	public static void SetPlatformDependentVariables()
	{
		GPUIPlatform platform = DeterminePlatform();
		matrixHandlingType = GClass1262.gpuiSettings.GetMatrixHandlingType(platform);
		switch (GClass1262.gpuiSettings.GetComputeThreadCount(platform))
		{
		default:
			GClass1262.COMPUTE_SHADER_THREAD_COUNT = 512f;
			GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D = 16f;
			break;
		case GPUIComputeThreadCount.x64:
			GClass1262.COMPUTE_SHADER_THREAD_COUNT = 64f;
			GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D = 8f;
			break;
		case GPUIComputeThreadCount.x128:
			GClass1262.COMPUTE_SHADER_THREAD_COUNT = 128f;
			GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D = 8f;
			break;
		case GPUIComputeThreadCount.x256:
			GClass1262.COMPUTE_SHADER_THREAD_COUNT = 256f;
			GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D = 16f;
			break;
		case GPUIComputeThreadCount.x512:
			GClass1262.COMPUTE_SHADER_THREAD_COUNT = 512f;
			GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D = 16f;
			break;
		case GPUIComputeThreadCount.x1024:
			GClass1262.COMPUTE_SHADER_THREAD_COUNT = 1024f;
			GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D = 32f;
			break;
		}
	}

	public static GPUIPlatform DeterminePlatform()
	{
		return SystemInfo.graphicsDeviceType switch
		{
			GraphicsDeviceType.OpenGLES3 => GPUIPlatform.GLES31, 
			GraphicsDeviceType.PlayStation4 => GPUIPlatform.PS4, 
			GraphicsDeviceType.XboxOne => GPUIPlatform.XBoxOne, 
			GraphicsDeviceType.Metal => GPUIPlatform.Metal, 
			GraphicsDeviceType.OpenGLCore => GPUIPlatform.OpenGLCore, 
			GraphicsDeviceType.Vulkan => GPUIPlatform.Vulkan, 
			_ => GPUIPlatform.Default, 
		};
	}

	public static void UpdatePlatformDependentFiles()
	{
	}
}
