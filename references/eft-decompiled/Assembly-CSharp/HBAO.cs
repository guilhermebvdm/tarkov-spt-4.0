using EFT.BlitDebug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/HBAO")]
[RequireComponent(typeof(Camera))]
public class HBAO : HBAO_Core
{
	private RenderTexture[] renderTexture_0 = new RenderTexture[16];

	private RenderTexture[] renderTexture_1 = new RenderTexture[16];

	private RenderTexture[] renderTexture_2 = new RenderTexture[16];

	private RenderBuffer[][] renderBuffer_0 = new RenderBuffer[4][]
	{
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4]
	};

	private RenderBuffer[][] renderBuffer_1 = new RenderBuffer[4][]
	{
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4],
		new RenderBuffer[4]
	};

	private RenderTexture renderTexture_3;

	private RenderTexture renderTexture_4;

	private CommandBuffer commandBuffer_0;

	public bool useTriangleBlit = true;

	public override void OnEnable()
	{
		commandBuffer_0 = new CommandBuffer();
		commandBuffer_0.name = "HBAO";
		base.OnEnable();
		method_3();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (renderTexture_3 != null)
		{
			Object.DestroyImmediate(renderTexture_3);
		}
		if (renderTexture_4 != null)
		{
			Object.DestroyImmediate(renderTexture_4);
		}
		commandBuffer_0?.Dispose();
		commandBuffer_0 = null;
	}

	[ImageEffectOpaque]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
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
			if (method_2())
			{
				method_3();
			}
			if (base.generalSettings.deinterleaving == Deinterleaving._2x)
			{
				method_5(source, destination);
			}
			else if (base.generalSettings.deinterleaving == Deinterleaving._4x)
			{
				method_6(source, destination);
			}
			else
			{
				method_4(source, destination);
			}
		}
		else
		{
			Shader.SetGlobalTexture(ShaderProperties.hbaoTex, Texture2D.whiteTexture);
			GClass860.BlitOrCopy(source, destination);
		}
	}

	public bool method_2()
	{
		if (!(renderTexture_3 == null) && !(renderTexture_4 == null))
		{
			if (renderTexture_4.width == _renderTarget.width && renderTexture_4.height == _renderTarget.height)
			{
				if (renderTexture_3.width == _renderTarget.fullWidth / _renderTarget.downsamplingFactor && renderTexture_3.height == _renderTarget.fullHeight / _renderTarget.downsamplingFactor)
				{
					return false;
				}
				return true;
			}
			return true;
		}
		return true;
	}

	public void method_3()
	{
		if (_renderTarget != null)
		{
			if (renderTexture_4 != null)
			{
				Object.DestroyImmediate(renderTexture_4);
			}
			renderTexture_4 = new RenderTexture(_renderTarget.fullWidth, _renderTarget.fullHeight, 0);
			if (renderTexture_3 != null)
			{
				Object.DestroyImmediate(renderTexture_3);
			}
			if (_renderTarget.downsamplingFactor != 0)
			{
				renderTexture_3 = new RenderTexture(_renderTarget.fullWidth / _renderTarget.downsamplingFactor, _renderTarget.fullHeight / _renderTarget.downsamplingFactor, 0);
			}
		}
	}

	public void method_4(RenderTexture source, RenderTexture destination)
	{
		if (useTriangleBlit)
		{
			commandBuffer_0.Clear();
			RenderTexture renderTexture = null;
			RenderTexture active = RenderTexture.active;
			commandBuffer_0.SetRenderTarget(renderTexture_3);
			commandBuffer_0.ClearRenderTarget(clearDepth: false, clearColor: true, Color.white);
			commandBuffer_0.SetRenderTarget(active);
			commandBuffer_0.BlitFullscreenTriangle(source, renderTexture_3, _hbaoMaterial, GetAoPass());
			if (base.blurSettings.amount != Blur.None)
			{
				renderTexture = RenderTexture.GetTemporary(_renderTarget.fullWidth / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor);
				commandBuffer_0.BlitFullscreenTriangle(renderTexture_3, renderTexture, _hbaoMaterial, GetBlurXPass());
				commandBuffer_0.BlitFullscreenTriangle(renderTexture, renderTexture_3, _hbaoMaterial, GetBlurYPass());
			}
			_hbaoMaterial.SetTexture(ShaderProperties.hbaoTex, renderTexture_3);
			Shader.SetGlobalTexture(ShaderProperties.hbaoTex, renderTexture_3);
			commandBuffer_0.BlitFullscreenTriangle(source, destination, _hbaoMaterial, GetFinalPass());
			Graphics.ExecuteCommandBuffer(commandBuffer_0);
			if (renderTexture != null)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
		}
		else
		{
			RenderTexture active2 = RenderTexture.active;
			RenderTexture.active = renderTexture_3;
			GL.Clear(clearDepth: false, clearColor: true, Color.white);
			RenderTexture.active = active2;
			DebugGraphics.Blit(source, renderTexture_3, _hbaoMaterial, GetAoPass());
			if (base.blurSettings.amount != Blur.None)
			{
				RenderTexture temporary = RenderTexture.GetTemporary(_renderTarget.fullWidth / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.downsamplingFactor / _renderTarget.blurDownsamplingFactor);
				DebugGraphics.Blit(renderTexture_3, temporary, _hbaoMaterial, GetBlurXPass());
				renderTexture_3.DiscardContents();
				DebugGraphics.Blit(temporary, renderTexture_3, _hbaoMaterial, GetBlurYPass());
				RenderTexture.ReleaseTemporary(temporary);
			}
			_hbaoMaterial.SetTexture(ShaderProperties.hbaoTex, renderTexture_3);
			Shader.SetGlobalTexture(ShaderProperties.hbaoTex, renderTexture_3);
			DebugGraphics.Blit(source, destination, _hbaoMaterial, GetFinalPass());
		}
	}

	public void method_5(RenderTexture source, RenderTexture destination)
	{
		RenderTexture active = RenderTexture.active;
		for (int i = 0; i < 4; i++)
		{
			renderTexture_0[i] = RenderTexture.GetTemporary(_renderTarget.layerWidth, _renderTarget.layerHeight, 0, RenderTextureFormat.RFloat);
			renderTexture_1[i] = RenderTexture.GetTemporary(_renderTarget.layerWidth, _renderTarget.layerHeight, 0, RenderTextureFormat.ARGB2101010);
			renderTexture_2[i] = RenderTexture.GetTemporary(_renderTarget.layerWidth, _renderTarget.layerHeight);
			renderBuffer_0[0][i] = renderTexture_0[i].colorBuffer;
			renderBuffer_1[0][i] = renderTexture_1[i].colorBuffer;
			RenderTexture.active = renderTexture_2[i];
			GL.Clear(clearDepth: false, clearColor: true, Color.white);
		}
		_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[0], new Vector2(0f, 0f));
		_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[1], new Vector2(1f, 0f));
		_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[2], new Vector2(0f, 1f));
		_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[3], new Vector2(1f, 1f));
		Graphics.SetRenderTarget(renderBuffer_0[0], renderTexture_0[0].depthBuffer);
		_hbaoMaterial.SetPass(10);
		Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
		Graphics.SetRenderTarget(renderBuffer_1[0], renderTexture_1[0].depthBuffer);
		_hbaoMaterial.SetPass(12);
		Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
		RenderTexture.active = active;
		for (int j = 0; j < 4; j++)
		{
			_hbaoMaterial.SetTexture(ShaderProperties.depthTex, renderTexture_0[j]);
			_hbaoMaterial.SetTexture(ShaderProperties.normalsTex, renderTexture_1[j]);
			_hbaoMaterial.SetVector(ShaderProperties.jitter, _jitter[j]);
			DebugGraphics.Blit(source, renderTexture_2[j], _hbaoMaterial, GetAoDeinterleavedPass());
			renderTexture_0[j].DiscardContents();
			renderTexture_1[j].DiscardContents();
		}
		RenderTexture temporary = RenderTexture.GetTemporary(_renderTarget.fullWidth, _renderTarget.fullHeight);
		for (int k = 0; k < 4; k++)
		{
			_hbaoMaterial.SetVector(ShaderProperties.layerOffset, new Vector2((k & 1) * _renderTarget.layerWidth, (k >> 1) * _renderTarget.layerHeight));
			DebugGraphics.Blit(renderTexture_2[k], temporary, _hbaoMaterial, 14);
			RenderTexture.ReleaseTemporary(renderTexture_2[k]);
			RenderTexture.ReleaseTemporary(renderTexture_1[k]);
			RenderTexture.ReleaseTemporary(renderTexture_0[k]);
		}
		DebugGraphics.Blit(temporary, renderTexture_4, _hbaoMaterial, 15);
		temporary.DiscardContents();
		if (base.blurSettings.amount != Blur.None)
		{
			if (base.blurSettings.downsample)
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(_renderTarget.fullWidth / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.blurDownsamplingFactor);
				DebugGraphics.Blit(renderTexture_4, temporary2, _hbaoMaterial, GetBlurXPass());
				renderTexture_4.DiscardContents();
				DebugGraphics.Blit(temporary2, renderTexture_4, _hbaoMaterial, GetBlurYPass());
				RenderTexture.ReleaseTemporary(temporary2);
			}
			else
			{
				DebugGraphics.Blit(renderTexture_4, temporary, _hbaoMaterial, GetBlurXPass());
				renderTexture_4.DiscardContents();
				DebugGraphics.Blit(temporary, renderTexture_4, _hbaoMaterial, GetBlurYPass());
			}
		}
		RenderTexture.ReleaseTemporary(temporary);
		_hbaoMaterial.SetTexture(ShaderProperties.hbaoTex, renderTexture_4);
		Shader.SetGlobalTexture(ShaderProperties.hbaoTex, renderTexture_4);
		DebugGraphics.Blit(source, destination, _hbaoMaterial, GetFinalPass());
	}

	public void method_6(RenderTexture source, RenderTexture destination)
	{
		RenderTexture active = RenderTexture.active;
		for (int i = 0; i < 16; i++)
		{
			renderTexture_0[i] = RenderTexture.GetTemporary(_renderTarget.layerWidth, _renderTarget.layerHeight, 0, RenderTextureFormat.RFloat);
			renderTexture_1[i] = RenderTexture.GetTemporary(_renderTarget.layerWidth, _renderTarget.layerHeight, 0, RenderTextureFormat.ARGB2101010);
			renderTexture_2[i] = RenderTexture.GetTemporary(_renderTarget.layerWidth, _renderTarget.layerHeight);
			RenderTexture.active = renderTexture_2[i];
			GL.Clear(clearDepth: false, clearColor: true, Color.white);
		}
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < 4; k++)
			{
				renderBuffer_0[j][k] = renderTexture_0[k + 4 * j].colorBuffer;
				renderBuffer_1[j][k] = renderTexture_1[k + 4 * j].colorBuffer;
			}
		}
		for (int l = 0; l < 4; l++)
		{
			int num = (l & 1) << 1;
			int num2 = l >> 1 << 1;
			_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[0], new Vector2(num, num2));
			_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[1], new Vector2(num + 1, num2));
			_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[2], new Vector2(num, num2 + 1));
			_hbaoMaterial.SetVector(ShaderProperties.deinterleavingOffset[3], new Vector2(num + 1, num2 + 1));
			Graphics.SetRenderTarget(renderBuffer_0[l], renderTexture_0[4 * l].depthBuffer);
			_hbaoMaterial.SetPass(11);
			Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
			Graphics.SetRenderTarget(renderBuffer_1[l], renderTexture_1[4 * l].depthBuffer);
			_hbaoMaterial.SetPass(13);
			Graphics.DrawMeshNow(quadMesh, Matrix4x4.identity);
		}
		RenderTexture.active = active;
		for (int m = 0; m < 16; m++)
		{
			_hbaoMaterial.SetTexture(ShaderProperties.depthTex, renderTexture_0[m]);
			_hbaoMaterial.SetTexture(ShaderProperties.normalsTex, renderTexture_1[m]);
			_hbaoMaterial.SetVector(ShaderProperties.jitter, _jitter[m]);
			DebugGraphics.Blit(source, renderTexture_2[m], _hbaoMaterial, GetAoDeinterleavedPass());
			renderTexture_0[m].DiscardContents();
			renderTexture_1[m].DiscardContents();
		}
		RenderTexture temporary = RenderTexture.GetTemporary(_renderTarget.fullWidth, _renderTarget.fullHeight);
		for (int n = 0; n < 16; n++)
		{
			_hbaoMaterial.SetVector(ShaderProperties.layerOffset, new Vector2(((n & 1) + ((n & 7) >> 2 << 1)) * _renderTarget.layerWidth, (((n & 3) >> 1) + (n >> 3 << 1)) * _renderTarget.layerHeight));
			DebugGraphics.Blit(renderTexture_2[n], temporary, _hbaoMaterial, 14);
			RenderTexture.ReleaseTemporary(renderTexture_2[n]);
			RenderTexture.ReleaseTemporary(renderTexture_1[n]);
			RenderTexture.ReleaseTemporary(renderTexture_0[n]);
		}
		DebugGraphics.Blit(temporary, renderTexture_4, _hbaoMaterial, 16);
		temporary.DiscardContents();
		if (base.blurSettings.amount != Blur.None)
		{
			if (base.blurSettings.downsample)
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(_renderTarget.fullWidth / _renderTarget.blurDownsamplingFactor, _renderTarget.fullHeight / _renderTarget.blurDownsamplingFactor);
				DebugGraphics.Blit(renderTexture_4, temporary2, _hbaoMaterial, GetBlurXPass());
				renderTexture_4.DiscardContents();
				DebugGraphics.Blit(temporary2, renderTexture_4, _hbaoMaterial, GetBlurYPass());
				RenderTexture.ReleaseTemporary(temporary2);
			}
			else
			{
				DebugGraphics.Blit(renderTexture_4, temporary, _hbaoMaterial, GetBlurXPass());
				renderTexture_4.DiscardContents();
				DebugGraphics.Blit(temporary, renderTexture_4, _hbaoMaterial, GetBlurYPass());
			}
		}
		RenderTexture.ReleaseTemporary(temporary);
		_hbaoMaterial.SetTexture(ShaderProperties.hbaoTex, renderTexture_4);
		Shader.SetGlobalTexture(ShaderProperties.hbaoTex, renderTexture_4);
		DebugGraphics.Blit(source, destination, _hbaoMaterial, GetFinalPass());
	}
}
