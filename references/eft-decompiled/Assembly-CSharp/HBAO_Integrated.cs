using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/HBAO Integrated")]
[RequireComponent(typeof(Camera))]
public class HBAO_Integrated : HBAO_Core
{
	private CommandBuffer commandBuffer_0;

	private IntegrationStage integrationStage_0;

	private Resolution resolution_0;

	private DisplayMode displayMode_0;

	private RenderingPath renderingPath_0;

	private bool bool_0;

	private int int_2;

	private int int_3;

	private Quality quality_1;

	private Deinterleaving deinterleaving_0;

	private bool bool_1;

	private bool bool_2;

	private Blur blur_0;

	private bool bool_3;

	public override void OnEnable()
	{
		base.OnEnable();
		if (commandBuffer_0 == null)
		{
			commandBuffer_0 = new CommandBuffer();
			commandBuffer_0.name = "HBAO";
		}
		bool_3 = true;
	}

	public override void OnDisable()
	{
		method_6();
		base.OnDisable();
	}

	public override void CheckParameters()
	{
		base.CheckParameters();
		CameraEvent cameraEvent = method_7();
		if (cameraEvent != CameraEvent.BeforeImageEffectsOpaque && !IsDeferredShading())
		{
			GeneralSettings generalSettings = base.generalSettings;
			generalSettings.integrationStage = IntegrationStage.BeforeImageEffectsOpaque;
			base.generalSettings = generalSettings;
		}
		if (cameraEvent == CameraEvent.BeforeImageEffectsOpaque && base.aoSettings.perPixelNormals == PerPixelNormals.GBuffer)
		{
			AOSettings aOSettings = base.aoSettings;
			aOSettings.perPixelNormals = PerPixelNormals.Camera;
			base.aoSettings = aOSettings;
		}
		else if (cameraEvent != CameraEvent.BeforeImageEffectsOpaque && base.aoSettings.perPixelNormals == PerPixelNormals.Camera)
		{
			AOSettings aOSettings2 = base.aoSettings;
			aOSettings2.perPixelNormals = PerPixelNormals.GBuffer;
			base.aoSettings = aOSettings2;
		}
	}

	public void OnPreRender()
	{
		if (!(hbaoShader == null) && !(_hbaoCamera == null))
		{
			_hbaoCamera.depthTextureMode |= DepthTextureMode.Depth;
			if (base.aoSettings.perPixelNormals == PerPixelNormals.Camera)
			{
				_hbaoCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
			}
			CheckParameters();
			UpdateShaderProperties();
			UpdateShaderKeywords();
			bool flag = false;
			if (integrationStage_0 != base.generalSettings.integrationStage || resolution_0 != base.generalSettings.resolution || displayMode_0 != base.generalSettings.displayMode || renderingPath_0 != _renderTarget.renderingPath || bool_0 != _renderTarget.hdr || int_2 != _renderTarget.fullWidth || int_3 != _renderTarget.fullHeight || quality_1 != base.generalSettings.quality || deinterleaving_0 != base.generalSettings.deinterleaving || bool_1 != base.aoSettings.useMultiBounce || bool_2 != base.colorBleedingSettings.enabled || blur_0 != base.blurSettings.amount)
			{
				integrationStage_0 = base.generalSettings.integrationStage;
				resolution_0 = base.generalSettings.resolution;
				displayMode_0 = base.generalSettings.displayMode;
				renderingPath_0 = _renderTarget.renderingPath;
				bool_0 = _renderTarget.hdr;
				int_2 = _renderTarget.fullWidth;
				int_3 = _renderTarget.fullHeight;
				quality_1 = base.generalSettings.quality;
				deinterleaving_0 = base.generalSettings.deinterleaving;
				bool_1 = base.aoSettings.useMultiBounce;
				bool_2 = base.colorBleedingSettings.enabled;
				blur_0 = base.blurSettings.amount;
				flag = true;
			}
			if (flag || bool_3)
			{
				method_6();
				CameraEvent cameraEvent = method_7();
				if (base.generalSettings.deinterleaving == Deinterleaving._2x)
				{
					method_3(cameraEvent);
				}
				else if (base.generalSettings.deinterleaving == Deinterleaving._4x)
				{
					method_4(cameraEvent);
				}
				else
				{
					method_2(cameraEvent);
				}
				_hbaoCamera.AddCommandBuffer(cameraEvent, commandBuffer_0);
				bool_3 = false;
			}
		}
		else
		{
			Shader.SetGlobalTexture(ShaderProperties.hbaoTex, Texture2D.whiteTexture);
		}
	}

	public void method_2(CameraEvent cameraEvent)
	{
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.mainTex);
		RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(ShaderProperties.hbaoTex);
		commandBuffer_0.GetTemporaryRT(ShaderProperties.hbaoTex, _renderTarget.fullWidth / _renderTarget.downsamplingFactor, _renderTarget.fullHeight / _renderTarget.downsamplingFactor);
		commandBuffer_0.SetRenderTarget(renderTargetIdentifier2);
		commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
		commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, renderTargetIdentifier2, _hbaoMaterial, GetAoPass());
		if (base.blurSettings.amount != Blur.None)
		{
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor);
			commandBuffer_0.Blit(renderTargetIdentifier2, renderTargetIdentifier, _hbaoMaterial, GetBlurXPass());
			commandBuffer_0.Blit(renderTargetIdentifier, renderTargetIdentifier2, _hbaoMaterial, GetBlurYPass());
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mainTex);
		}
		commandBuffer_0.SetGlobalTexture(ShaderProperties.hbaoTex, renderTargetIdentifier2);
		method_5(cameraEvent);
		commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.hbaoTex);
	}

	public void method_3(CameraEvent cameraEvent)
	{
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.mainTex);
		RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(ShaderProperties.hbaoTex);
		RenderTargetIdentifier[] array = new RenderTargetIdentifier[4]
		{
			ShaderProperties.mrtDepthTex[0],
			ShaderProperties.mrtDepthTex[1],
			ShaderProperties.mrtDepthTex[2],
			ShaderProperties.mrtDepthTex[3]
		};
		RenderTargetIdentifier[] array2 = new RenderTargetIdentifier[4]
		{
			ShaderProperties.mrtNrmTex[0],
			ShaderProperties.mrtNrmTex[1],
			ShaderProperties.mrtNrmTex[2],
			ShaderProperties.mrtNrmTex[3]
		};
		RenderTargetIdentifier[] array3 = new RenderTargetIdentifier[4]
		{
			ShaderProperties.mrtHBAOTex[0],
			ShaderProperties.mrtHBAOTex[1],
			ShaderProperties.mrtHBAOTex[2],
			ShaderProperties.mrtHBAOTex[3]
		};
		for (int i = 0; i < 4; i++)
		{
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mrtDepthTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.RFloat);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mrtNrmTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mrtHBAOTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
		}
		commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[0], new Vector2(0f, 0f));
		commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[1], new Vector2(1f, 0f));
		commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[2], new Vector2(0f, 1f));
		commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[3], new Vector2(1f, 1f));
		commandBuffer_0.SetRenderTarget(array, array[0]);
		commandBuffer_0.DrawMesh(quadMesh, Matrix4x4.identity, _hbaoMaterial, 0, 10);
		commandBuffer_0.SetRenderTarget(array2, array2[0]);
		commandBuffer_0.DrawMesh(quadMesh, Matrix4x4.identity, _hbaoMaterial, 0, 12);
		for (int j = 0; j < 4; j++)
		{
			commandBuffer_0.SetGlobalTexture(ShaderProperties.depthTex, array[j]);
			commandBuffer_0.SetGlobalTexture(ShaderProperties.normalsTex, array2[j]);
			commandBuffer_0.SetGlobalVector(ShaderProperties.jitter, _jitter[j]);
			commandBuffer_0.SetRenderTarget(array3[j]);
			commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, array3[j], _hbaoMaterial, GetAoDeinterleavedPass());
		}
		commandBuffer_0.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		for (int k = 0; k < 4; k++)
		{
			commandBuffer_0.SetGlobalVector(ShaderProperties.layerOffset, new Vector2((k & 1) * _renderTarget.layerWidth, (k >> 1) * _renderTarget.layerHeight));
			commandBuffer_0.Blit(array3[k], renderTargetIdentifier, _hbaoMaterial, 14);
		}
		commandBuffer_0.GetTemporaryRT(ShaderProperties.hbaoTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		commandBuffer_0.Blit(renderTargetIdentifier, renderTargetIdentifier2, _hbaoMaterial, 15);
		if (base.blurSettings.amount != Blur.None)
		{
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mainTex);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.blurDownsamplingFactor);
			commandBuffer_0.Blit(renderTargetIdentifier2, renderTargetIdentifier, _hbaoMaterial, GetBlurXPass());
			commandBuffer_0.Blit(renderTargetIdentifier, renderTargetIdentifier2, _hbaoMaterial, GetBlurYPass());
		}
		commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mainTex);
		for (int l = 0; l < 4; l++)
		{
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mrtHBAOTex[l]);
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mrtNrmTex[l]);
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mrtDepthTex[l]);
		}
		commandBuffer_0.SetGlobalTexture(ShaderProperties.hbaoTex, renderTargetIdentifier2);
		method_5(cameraEvent);
		commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.hbaoTex);
	}

	public void method_4(CameraEvent cameraEvent)
	{
		RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.mainTex);
		RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(ShaderProperties.hbaoTex);
		RenderTargetIdentifier[] array = new RenderTargetIdentifier[16];
		RenderTargetIdentifier[] array2 = new RenderTargetIdentifier[16];
		RenderTargetIdentifier[] array3 = new RenderTargetIdentifier[16];
		for (int i = 0; i < 16; i++)
		{
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mrtDepthTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.RFloat);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mrtNrmTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mrtHBAOTex[i], _renderTarget.layerWidth, _renderTarget.layerHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
			array[i] = ShaderProperties.mrtDepthTex[i];
			array2[i] = ShaderProperties.mrtNrmTex[i];
			array3[i] = ShaderProperties.mrtHBAOTex[i];
		}
		for (int j = 0; j < 4; j++)
		{
			int num = (j & 1) << 1;
			int num2 = j >> 1 << 1;
			commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[0], new Vector2(num, num2));
			commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[1], new Vector2(num + 1, num2));
			commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[2], new Vector2(num, num2 + 1));
			commandBuffer_0.SetGlobalVector(ShaderProperties.deinterleavingOffset[3], new Vector2(num + 1, num2 + 1));
			RenderTargetIdentifier[] array4 = new RenderTargetIdentifier[4]
			{
				array[j << 2],
				array[(j << 2) + 1],
				array[(j << 2) + 2],
				array[(j << 2) + 3]
			};
			RenderTargetIdentifier[] array5 = new RenderTargetIdentifier[4]
			{
				array2[j << 2],
				array2[(j << 2) + 1],
				array2[(j << 2) + 2],
				array2[(j << 2) + 3]
			};
			commandBuffer_0.SetRenderTarget(array4, array4[0]);
			commandBuffer_0.DrawMesh(quadMesh, Matrix4x4.identity, _hbaoMaterial, 0, 11);
			commandBuffer_0.SetRenderTarget(array5, array5[0]);
			commandBuffer_0.DrawMesh(quadMesh, Matrix4x4.identity, _hbaoMaterial, 0, 13);
		}
		for (int k = 0; k < 16; k++)
		{
			commandBuffer_0.SetGlobalTexture(ShaderProperties.depthTex, array[k]);
			commandBuffer_0.SetGlobalTexture(ShaderProperties.normalsTex, array2[k]);
			commandBuffer_0.SetGlobalVector(ShaderProperties.jitter, _jitter[k]);
			commandBuffer_0.SetRenderTarget(array3[k]);
			commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, array3[k], _hbaoMaterial, GetAoDeinterleavedPass());
		}
		commandBuffer_0.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		for (int l = 0; l < 16; l++)
		{
			commandBuffer_0.SetGlobalVector(ShaderProperties.layerOffset, new Vector2(((l & 1) + ((l & 7) >> 2 << 1)) * _renderTarget.layerWidth, (((l & 3) >> 1) + (l >> 3 << 1)) * _renderTarget.layerHeight));
			commandBuffer_0.Blit(array3[l], renderTargetIdentifier, _hbaoMaterial, 14);
		}
		commandBuffer_0.GetTemporaryRT(ShaderProperties.hbaoTex, _renderTarget.fullWidth, _renderTarget.fullHeight);
		commandBuffer_0.Blit(renderTargetIdentifier, renderTargetIdentifier2, _hbaoMaterial, 16);
		if (base.blurSettings.amount != Blur.None)
		{
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mainTex);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.fullWidth / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.blurDownsamplingFactor);
			commandBuffer_0.Blit(renderTargetIdentifier2, renderTargetIdentifier, _hbaoMaterial, GetBlurXPass());
			commandBuffer_0.Blit(renderTargetIdentifier, renderTargetIdentifier2, _hbaoMaterial, GetBlurYPass());
		}
		commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mainTex);
		for (int m = 0; m < 16; m++)
		{
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mrtHBAOTex[m]);
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mrtNrmTex[m]);
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mrtDepthTex[m]);
		}
		commandBuffer_0.SetGlobalTexture(ShaderProperties.hbaoTex, renderTargetIdentifier2);
		method_5(cameraEvent);
		commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.hbaoTex);
	}

	public void method_5(CameraEvent cameraEvent)
	{
		if (base.generalSettings.displayMode == DisplayMode.Normal)
		{
			if (cameraEvent == CameraEvent.BeforeReflections)
			{
				RenderTargetIdentifier[] colors = new RenderTargetIdentifier[2]
				{
					BuiltinRenderTextureType.GBuffer0,
					_renderTarget.hdr ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3
				};
				if (_renderTarget.hdr)
				{
					RenderTargetIdentifier dest = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
					commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
					commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, dest);
					commandBuffer_0.SetRenderTarget(colors, BuiltinRenderTextureType.CameraTarget);
					commandBuffer_0.DrawMesh(quadMesh, Matrix4x4.identity, _hbaoMaterial, 0, 37);
					if (base.colorBleedingSettings.enabled)
					{
						commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, 42);
					}
					commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
				}
				else
				{
					RenderTargetIdentifier dest2 = new RenderTargetIdentifier(ShaderProperties.rt0Tex);
					RenderTargetIdentifier dest3 = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
					commandBuffer_0.GetTemporaryRT(ShaderProperties.rt0Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
					commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
					commandBuffer_0.Blit(BuiltinRenderTextureType.GBuffer0, dest2);
					commandBuffer_0.Blit(BuiltinRenderTextureType.GBuffer3, dest3);
					commandBuffer_0.SetRenderTarget(colors, BuiltinRenderTextureType.GBuffer3);
					commandBuffer_0.DrawMesh(quadMesh, Matrix4x4.identity, _hbaoMaterial, 0, 36);
					commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
					commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt0Tex);
				}
			}
			switch (cameraEvent)
			{
			case CameraEvent.AfterLighting:
				if (_renderTarget.hdr)
				{
					if (bool_1)
					{
						RenderTargetIdentifier dest4 = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
						commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
						commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, dest4);
					}
					commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, bool_1 ? 41 : 40);
					if (base.colorBleedingSettings.enabled)
					{
						commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, 42);
					}
					if (bool_1)
					{
						commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
					}
				}
				else
				{
					RenderTargetIdentifier renderTargetIdentifier2 = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
					commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
					commandBuffer_0.Blit(BuiltinRenderTextureType.GBuffer3, renderTargetIdentifier2);
					commandBuffer_0.Blit(renderTargetIdentifier2, BuiltinRenderTextureType.GBuffer3, _hbaoMaterial, bool_1 ? 39 : 38);
					commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
				}
				break;
			case CameraEvent.BeforeImageEffectsOpaque:
				if (bool_1)
				{
					RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
					commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
					commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, renderTargetIdentifier);
					commandBuffer_0.Blit(renderTargetIdentifier, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, 41);
				}
				else
				{
					commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, 40);
				}
				if (base.colorBleedingSettings.enabled)
				{
					commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, 42);
				}
				if (bool_1)
				{
					commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
				}
				break;
			}
		}
		else if (base.generalSettings.displayMode == DisplayMode.AOOnly)
		{
			if (bool_1)
			{
				RenderTargetIdentifier dest5 = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
				commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
				commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, dest5);
			}
			RenderTargetIdentifier renderTargetIdentifier3 = new RenderTargetIdentifier(ShaderProperties.mainTex);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.width, _renderTarget.height);
			commandBuffer_0.SetRenderTarget(renderTargetIdentifier3);
			commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, renderTargetIdentifier3, _hbaoMaterial, bool_1 ? 41 : 40);
			commandBuffer_0.Blit(renderTargetIdentifier3, BuiltinRenderTextureType.CameraTarget);
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mainTex);
			if (bool_1)
			{
				commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
			}
		}
		else if (base.generalSettings.displayMode == DisplayMode.ColorBleedingOnly)
		{
			RenderTargetIdentifier renderTargetIdentifier4 = new RenderTargetIdentifier(ShaderProperties.mainTex);
			commandBuffer_0.GetTemporaryRT(ShaderProperties.mainTex, _renderTarget.width, _renderTarget.height);
			commandBuffer_0.SetRenderTarget(renderTargetIdentifier4);
			commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.black);
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, renderTargetIdentifier4, _hbaoMaterial, 42);
			commandBuffer_0.Blit(renderTargetIdentifier4, BuiltinRenderTextureType.CameraTarget);
			commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.mainTex);
		}
		else if (base.generalSettings.displayMode == DisplayMode.SplitWithAOAndAOOnly)
		{
			if (bool_1)
			{
				RenderTargetIdentifier dest6 = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
				commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
				commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, dest6);
			}
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, bool_1 ? 41 : 40);
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, bool_1 ? 44 : 43);
			if (bool_1)
			{
				commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
			}
		}
		else if (base.generalSettings.displayMode == DisplayMode.SplitWithoutAOAndAOOnly)
		{
			if (bool_1)
			{
				RenderTargetIdentifier dest7 = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
				commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
				commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, dest7);
			}
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, bool_1 ? 44 : 43);
			if (bool_1)
			{
				commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
			}
		}
		else if (base.generalSettings.displayMode == DisplayMode.SplitWithoutAOAndWithAO)
		{
			if (bool_1)
			{
				RenderTargetIdentifier dest8 = new RenderTargetIdentifier(ShaderProperties.rt3Tex);
				commandBuffer_0.GetTemporaryRT(ShaderProperties.rt3Tex, _renderTarget.fullWidth, _renderTarget.fullHeight, 0, FilterMode.Point, RuntimeUtilities.defaultHDRRenderTextureFormat);
				commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, dest8);
			}
			commandBuffer_0.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, _hbaoMaterial, bool_1 ? 46 : 45);
			if (bool_1)
			{
				commandBuffer_0.ReleaseTemporaryRT(ShaderProperties.rt3Tex);
			}
		}
	}

	public void method_6()
	{
		if (commandBuffer_0 != null)
		{
			if (_hbaoCamera != null)
			{
				_hbaoCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, commandBuffer_0);
				_hbaoCamera.RemoveCommandBuffer(CameraEvent.AfterLighting, commandBuffer_0);
				_hbaoCamera.RemoveCommandBuffer(CameraEvent.BeforeReflections, commandBuffer_0);
			}
			commandBuffer_0.Clear();
		}
	}

	public CameraEvent method_7()
	{
		if (base.generalSettings.displayMode != DisplayMode.Normal)
		{
			return CameraEvent.BeforeImageEffectsOpaque;
		}
		return base.generalSettings.integrationStage switch
		{
			IntegrationStage.AfterLighting => CameraEvent.AfterLighting, 
			IntegrationStage.BeforeReflections => CameraEvent.BeforeReflections, 
			_ => CameraEvent.BeforeImageEffectsOpaque, 
		};
	}
}
