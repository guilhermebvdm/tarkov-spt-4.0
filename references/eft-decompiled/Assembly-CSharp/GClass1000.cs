using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass1000
{
	public struct Struct166
	{
		public Vector4 color;

		public Vector4 pos;

		public Vector4 uv0;

		public Vector4 uv1;

		public Vector2 distortAndFog;

		public Vector4 projPos;
	}

	public struct Struct167
	{
		public Vector4 _MainTex_ST;

		public Vector4 _TintColor;

		public Vector4 _IndoorTintColor;

		public float _AnimationSpeed;

		public float _Distortion;

		public float _InvFade;

		public uint _MainTexArrayOffset;

		public uint _NoiseTexArrayOffset;

		public float _Indoor;

		public void SetIndoorPropertiesForce(Vector4 indoorTintColor, float indoor)
		{
			_IndoorTintColor = indoorTintColor;
			_Indoor = indoor;
		}
	}

	public struct Struct168
	{
		public int vertexCountPerInstance;

		public int instanceCount;

		public int startVertex;

		public int startInstance;
	}

	public struct Struct169
	{
		public int groupsX;

		public int groupsY;

		public int groupsZ;
	}

	public struct Struct170
	{
		public uint DrawParamsOffset;

		public uint ShaderIndex;
	}

	public struct Struct171
	{
		public ParticleSystem ParticleSystem;

		public int MaxParticles;
	}

	public struct Struct172
	{
		public int DrawArgsOffset;

		public Material Material;

		public HashSet<ParticleSystem> ParticleSystems;

		public Class701 ParticlesBuffers;
	}

	public struct Struct173
	{
		public Texture2D noiseTexture;

		public Texture2D mainTexture;

		public int drawParamsIndex;
	}

	public class Class700
	{
		[NonSerialized]
		public RenderTexture RenderTexture_0;

		[NonSerialized]
		public List<bool> List_0;

		[NonSerialized]
		public ComputeShader ComputeShader_0;

		[NonSerialized]
		public Action Action_0;

		[NonSerialized]
		public List<RenderTexture> List_1 = new List<RenderTexture>();

		[NonSerialized]
		public Dictionary<Texture2D, int> Dictionary_0 = new Dictionary<Texture2D, int>();

		public RenderTexture Storage => RenderTexture_0;

		public Class700(int resolution, int initialLayers, ComputeShader copyShader, Action textureIsRecreated)
		{
			int num = 0;
			for (int num2 = resolution; num2 > 0; num2 >>= 1)
			{
				num++;
			}
			RenderTexture_0 = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32, num);
			RenderTexture_0.volumeDepth = initialLayers;
			RenderTexture_0.enableRandomWrite = true;
			RenderTexture_0.dimension = TextureDimension.Tex2DArray;
			RenderTexture_0.Create();
			List_0 = new List<bool>();
			for (int i = 0; i < initialLayers; i++)
			{
				List_0.Add(item: true);
			}
			ComputeShader_0 = copyShader;
			Action_0 = textureIsRecreated;
		}

		public void ReserveTextureSpace(int count, CommandBuffer cmdBuf)
		{
			int num = 0;
			for (int i = 0; i < List_0.Count; i++)
			{
				num += (List_0[i] ? 1 : 0);
			}
			if (num >= count)
			{
				return;
			}
			int num2 = count - num;
			for (int j = 0; j < num2; j++)
			{
				List_0.Add(item: true);
			}
			RenderTexture renderTexture = new RenderTexture(RenderTexture_0.width, RenderTexture_0.height, 0, RenderTexture_0.format, RenderTexture_0.mipmapCount);
			renderTexture.volumeDepth = RenderTexture_0.volumeDepth + num2;
			renderTexture.enableRandomWrite = true;
			renderTexture.dimension = TextureDimension.Tex2DArray;
			renderTexture.Create();
			int mipmapCount = RenderTexture_0.mipmapCount;
			int volumeDepth = RenderTexture_0.volumeDepth;
			for (int k = 0; k < volumeDepth; k++)
			{
				for (int l = 0; l < mipmapCount; l++)
				{
					cmdBuf.CopyTexture(RenderTexture_0, k, l, 0, 0, RenderTexture_0.width, RenderTexture_0.height, renderTexture, k, l, 0, 0);
				}
			}
			List_1.Add(RenderTexture_0);
			RenderTexture_0 = renderTexture;
			Action_0?.Invoke();
		}

		public int ContainsTexture(Texture2D texture)
		{
			if (texture == null)
			{
				texture = Texture2D.whiteTexture;
			}
			if (Dictionary_0.ContainsKey(texture))
			{
				return Dictionary_0[texture];
			}
			return -1;
		}

		public int AddTexture(CommandBuffer cmdBuf, Texture2D texture)
		{
			if (texture == null)
			{
				texture = Texture2D.whiteTexture;
			}
			if (Dictionary_0.ContainsKey(texture))
			{
				return Dictionary_0[texture];
			}
			int num = -1;
			for (int i = 0; i < List_0.Count; i++)
			{
				if (List_0[i])
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				throw new IndexOutOfRangeException("Reserve before adding texture");
			}
			int threadGroupsX = RenderTexture_0.width / 8 + 1;
			int threadGroupsY = RenderTexture_0.height / 8 + 1;
			int[] values = new int[2] { RenderTexture_0.width, RenderTexture_0.height };
			cmdBuf.SetComputeIntParams(ComputeShader_0, "resolution", values);
			cmdBuf.SetComputeIntParam(ComputeShader_0, "copyTo", num);
			cmdBuf.SetComputeTextureParam(ComputeShader_0, 0, "_Src", texture);
			cmdBuf.SetComputeTextureParam(ComputeShader_0, 0, "_Dst", RenderTexture_0);
			cmdBuf.DispatchCompute(ComputeShader_0, 0, threadGroupsX, threadGroupsY, 1);
			List_0[num] = false;
			Dictionary_0.Add(texture, num);
			return num;
		}

		public int CopyTexture(CommandBuffer cmdBuf, Texture2D texture, int copyTo)
		{
			if (texture == null)
			{
				texture = Texture2D.whiteTexture;
			}
			int threadGroupsX = RenderTexture_0.width / 8 + 1;
			int threadGroupsY = RenderTexture_0.height / 8 + 1;
			int[] values = new int[2] { RenderTexture_0.width, RenderTexture_0.height };
			cmdBuf.SetComputeIntParams(ComputeShader_0, "resolution", values);
			cmdBuf.SetComputeIntParam(ComputeShader_0, "copyTo", copyTo);
			cmdBuf.SetComputeTextureParam(ComputeShader_0, 0, "_Src", texture);
			cmdBuf.SetComputeTextureParam(ComputeShader_0, 0, "_Dst", RenderTexture_0);
			cmdBuf.DispatchCompute(ComputeShader_0, 0, threadGroupsX, threadGroupsY, 1);
			List_0[copyTo] = false;
			return copyTo;
		}

		public void RemoveTexture(int index)
		{
			List_0[index] = true;
		}

		public void ProcessDelayedJobs()
		{
			for (int i = 0; i < List_1.Count; i++)
			{
				if (List_1[i] != null)
				{
					List_1[i].Release();
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(List_1[i]);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(List_1[i]);
					}
				}
			}
			List_1.Clear();
		}
	}

	public class Class701
	{
		public struct Struct174
		{
			public int offset;

			public int count;
		}

		[NonSerialized]
		public ComputeBuffer ComputeBuffer_0;

		[NonSerialized]
		public ComputeBuffer ComputeBuffer_1;

		[NonSerialized]
		public List<Struct174> List_0 = new List<Struct174>();

		[NonSerialized]
		public Dictionary<ParticleSystem, Struct174> Dictionary_0 = new Dictionary<ParticleSystem, Struct174>();

		[NonSerialized]
		public int Int_0 = 200;

		[NonSerialized]
		public string String_0 = "offsets__";

		[NonSerialized]
		public string String_1 = "vertices__";

		public ComputeBuffer ParticlesVertices => ComputeBuffer_0;

		public ComputeBuffer ParticlesOffsets => ComputeBuffer_1;

		public Class701(int initialSize, int incrementSize, int vertexStride, int offsetStride, string offsetsName, string verticesName)
		{
			ComputeBuffer_0 = new ComputeBuffer(initialSize + 1, vertexStride, ComputeBufferType_0);
			ComputeBuffer_0.name = (String_1 = verticesName);
			ComputeBuffer_1 = new ComputeBuffer(initialSize, offsetStride, ComputeBufferType_0);
			ComputeBuffer_1.name = (String_0 = offsetsName);
			List_0.Add(new Struct174
			{
				offset = 0,
				count = initialSize
			});
			Int_0 = incrementSize;
		}

		public bool HasData()
		{
			return Dictionary_0.Count != 0;
		}

		public void Cleanup()
		{
			ComputeBuffer_0.Release();
			ComputeBuffer_1.Release();
		}

		public void AddParticleSystem(ParticleSystem ps, out int outOffset, out int outCount, out bool outNeedToRebingBuffers)
		{
			int num = ps.main.maxParticles * 6;
			Struct174 value = new Struct174
			{
				offset = -1,
				count = -1
			};
			int num2 = -1;
			for (int i = 0; i < List_0.Count; i++)
			{
				Struct174 @struct = List_0[i];
				if (@struct.count >= num)
				{
					Struct174 item = new Struct174
					{
						offset = @struct.offset + num,
						count = @struct.count - num
					};
					if (item.count > 0)
					{
						List_0.Add(item);
					}
					value.offset = @struct.offset;
					value.count = num;
					Dictionary_0[ps] = value;
					num2 = i;
					break;
				}
			}
			if (num2 > -1)
			{
				List_0.RemoveAt(num2);
			}
			bool flag = false;
			if (value.offset == -1)
			{
				int num3 = Mathf.Max(Int_0, num);
				bool flag2 = false;
				if (List_0.Count > 0)
				{
					Struct174 struct2 = List_0[List_0.Count - 1];
					flag2 = struct2.offset + struct2.count == ComputeBuffer_0.count;
				}
				int count = ComputeBuffer_0.count;
				ComputeBuffer_0.Release();
				int stride = Marshal.SizeOf<Struct166>();
				ComputeBuffer_0 = new ComputeBuffer(count + num3, stride, ComputeBufferType_0);
				ComputeBuffer_0.name = String_1;
				ComputeBuffer_1 = new ComputeBuffer(count + num3 + 1, 4);
				ComputeBuffer_1.name = String_0;
				flag = true;
				if (flag2)
				{
					Struct174 value2 = List_0[List_0.Count - 1];
					value2.count += num3;
					List_0[List_0.Count - 1] = value2;
				}
				else
				{
					List_0.Add(new Struct174
					{
						offset = count,
						count = num3
					});
				}
				Struct174 struct3 = List_0[List_0.Count - 1];
				Struct174 item2 = new Struct174
				{
					offset = struct3.offset + num,
					count = struct3.count - num
				};
				List_0.RemoveAt(List_0.Count - 1);
				if (item2.count > 0)
				{
					List_0.Add(item2);
				}
				value.offset = struct3.offset;
				value.count = num;
				Dictionary_0[ps] = value;
			}
			Struct174 struct4 = Dictionary_0[ps];
			outOffset = struct4.offset;
			outCount = struct4.count;
			outNeedToRebingBuffers = flag;
		}

		public void RemoveParticleSystem(ParticleSystem ps)
		{
			Struct174 item = Dictionary_0[ps];
			int num = List_0.Count;
			for (int i = 0; i < List_0.Count; i++)
			{
				Struct174 @struct = List_0[i];
				if (item.offset < @struct.offset)
				{
					num = i;
					break;
				}
			}
			List_0.Insert(num, item);
			Dictionary_0.Remove(ps);
			if (num < List_0.Count - 1)
			{
				Struct174 struct2 = List_0[num + 1];
				if (item.offset + item.count == struct2.offset)
				{
					Struct174 value = new Struct174
					{
						offset = item.offset,
						count = item.count + struct2.count
					};
					List_0[num] = value;
					List_0.RemoveAt(num + 1);
				}
			}
			if (num > 0)
			{
				Struct174 struct3 = List_0[num - 1];
				Struct174 struct4 = List_0[num];
				if (struct3.offset + struct3.count == struct4.offset)
				{
					Struct174 value2 = new Struct174
					{
						offset = struct3.offset,
						count = struct3.count + struct4.count
					};
					List_0.RemoveAt(num);
					List_0[num - 1] = value2;
				}
			}
		}
	}

	public struct Struct175
	{
		public ParticleSystemRenderer renderer;

		public Material material;

		public int renderArgsOffset;

		public int drawParamsOffset;
	}

	[NonSerialized]
	public Class701 Class701_0;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_0;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_1;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_2;

	[NonSerialized]
	public ComputeBuffer ComputeBuffer_3;

	[NonSerialized]
	public List<Struct168> List_0 = new List<Struct168>();

	[NonSerialized]
	public List<int> List_1 = new List<int>();

	[NonSerialized]
	public List<Struct167> List_2 = new List<Struct167>();

	[NonSerialized]
	public List<int> List_3 = new List<int>();

	[NonSerialized]
	public List<Struct171> List_4 = new List<Struct171>();

	[NonSerialized]
	public int Int_0 = 200;

	[NonSerialized]
	public ComputeShader ComputeShader_0;

	[NonSerialized]
	public ComputeShader ComputeShader_1;

	[NonSerialized]
	public Class700 Class700_0;

	[NonSerialized]
	public Class700 Class700_1;

	[NonSerialized]
	public Dictionary<string, Struct172> Dictionary_0 = new Dictionary<string, Struct172>();

	[NonSerialized]
	public List<Struct172> List_5 = new List<Struct172>();

	[NonSerialized]
	public List<Struct173> List_6 = new List<Struct173>();

	[NonSerialized]
	public static ComputeBufferType ComputeBufferType_0 = ComputeBufferType.Structured;

	[NonSerialized]
	public Dictionary<ParticleSystem, Struct175> Dictionary_1 = new Dictionary<ParticleSystem, Struct175>();

	[NonSerialized]
	public int Int_1 = 100;

	[NonSerialized]
	public const string String_0 = "MBOIT Particles Before Render";

	[NonSerialized]
	public const string String_1 = "Clear Particles Indirect Data";

	public ComputeBuffer ParticlesVertices => Class701_0.ParticlesVertices;

	public ComputeBuffer ParticlesOffsets => Class701_0.ParticlesOffsets;

	public ComputeBuffer IndirectDataBuffer => ComputeBuffer_0;

	public GClass1000(int initialSize, int incrementSize, ComputeShader particlesIndirectArgsCleaner, ComputeShader texturesCopyShader, ComputeShader renderDataCopyShader)
	{
		ComputeShader_0 = particlesIndirectArgsCleaner;
		ComputeShader_1 = renderDataCopyShader;
		Int_1 = initialSize;
		int vertexStride = Marshal.SizeOf<Struct166>();
		int offsetStride = Marshal.SizeOf<Struct170>();
		Class701_0 = new Class701(initialSize, incrementSize, vertexStride, offsetStride, "offsetsGlobal", "verticesGlobal");
		int stride = Marshal.SizeOf<Struct168>();
		int stride2 = Marshal.SizeOf<Struct167>();
		ComputeBuffer_0 = new ComputeBuffer(initialSize, stride, ComputeBufferType.DrawIndirect);
		ComputeBuffer_0.name = "indirectDataGlobal";
		ComputeBuffer_1 = new ComputeBuffer(initialSize, stride, ComputeBufferType.DrawIndirect);
		ComputeBuffer_1.name = "indirectDataLocal";
		ComputeBuffer_2 = new ComputeBuffer(initialSize, stride2, ComputeBufferType_0);
		Struct168[] array = new Struct168[initialSize];
		Struct168 @struct = new Struct168
		{
			vertexCountPerInstance = 0,
			instanceCount = 0,
			startVertex = 0,
			startInstance = 0
		};
		for (int i = 0; i < initialSize; i++)
		{
			array[i] = @struct;
		}
		ComputeBuffer_0.SetData(array);
		ComputeBuffer_1.SetData(array);
		Int_0 = incrementSize;
		Class700_1 = new Class700(1024, 14, texturesCopyShader, method_7);
		Class700_0 = new Class700(512, 4, texturesCopyShader, method_8);
		int stride3 = Marshal.SizeOf<Struct169>();
		ComputeBuffer_3 = new ComputeBuffer(1, stride3, ComputeBufferType.DrawIndirect);
	}

	public void Cleanup()
	{
		Class701_0.Cleanup();
		ComputeBuffer_0.Release();
		ComputeBuffer_1.Release();
		foreach (KeyValuePair<string, Struct172> item in Dictionary_0)
		{
			item.Value.ParticlesBuffers.Cleanup();
		}
	}

	public void AddParticleSystem(ParticleSystem ps, bool isDynamic = false)
	{
		if (Dictionary_1.ContainsKey(ps))
		{
			return;
		}
		ParticleSystemRenderer component = ps.GetComponent<ParticleSystemRenderer>();
		if (!(component == null))
		{
			if (isDynamic)
			{
				List_4.Add(new Struct171
				{
					ParticleSystem = ps,
					MaxParticles = ps.main.maxParticles
				});
			}
			Class701_0.AddParticleSystem(ps, out var outOffset, out var outCount, out var outNeedToRebingBuffers);
			if (outNeedToRebingBuffers)
			{
				method_5();
			}
			Material material = component.material;
			string name = material.shader.name;
			int num = 0;
			if (Dictionary_0.TryGetValue(name, out var value))
			{
				num = value.DrawArgsOffset;
			}
			else
			{
				num = method_1(outOffset, outCount);
				int vertexStride = Marshal.SizeOf<Struct166>();
				int offsetStride = Marshal.SizeOf<uint>();
				value = new Struct172
				{
					DrawArgsOffset = num,
					Material = material,
					ParticleSystems = new HashSet<ParticleSystem> { ps },
					ParticlesBuffers = new Class701(Int_1, Int_0, vertexStride, offsetStride, "offsets" + num, "vertices" + num)
				};
				Dictionary_0.Add(name, value);
				List_5 = Dictionary_0.Values.ToList();
			}
			value.ParticlesBuffers.AddParticleSystem(ps, out var _, out var _, out var outNeedToRebingBuffers2);
			if (outNeedToRebingBuffers2)
			{
				method_6();
			}
			int num2 = method_2(material);
			material.SetInt("_VerticesOffset", outOffset);
			material.SetInt("_DrawArgsOffset", num2);
			material.SetInt("_ShaderIndex", method_4(num));
			material.SetBuffer("_Vertices", ParticlesVertices);
			material.SetBuffer("_LocalVertices", value.ParticlesBuffers.ParticlesVertices);
			material.SetBuffer("_ParticlesOffsetsLocal", value.ParticlesBuffers.ParticlesOffsets);
			material.SetBuffer("_DrawArgs", ComputeBuffer_0);
			material.SetTexture("_MainTexArray", Class700_1.Storage);
			material.SetTexture("_NoiseTexArray", Class700_0.Storage);
			material.SetBuffer("_ParticlesOffsetsGlobal", ParticlesOffsets);
			material.SetBuffer("_DrawParams", ComputeBuffer_2);
			Dictionary_1[ps] = new Struct175
			{
				renderer = component,
				material = material,
				renderArgsOffset = num,
				drawParamsOffset = num2
			};
		}
	}

	public void UpdateParticleSystemMaterial(ParticleSystem ps, Material material)
	{
		if (Dictionary_1.TryGetValue(ps, out var value))
		{
			int drawParamsOffset = value.drawParamsOffset;
			value.material = material;
			method_0(material, drawParamsOffset);
			ComputeBuffer_2.SetData(List_2, drawParamsOffset, drawParamsOffset, 1);
		}
	}

	public void method_0(Material material, int index)
	{
		Vector4 indoorTintColor = (material.HasProperty("_IndoorTintColor") ? material.GetVector("_IndoorTintColor") : Vector4.zero);
		float indoor = (material.HasProperty("_Indoor") ? material.GetFloat("_Indoor") : 0f);
		List_2[index].SetIndoorPropertiesForce(indoorTintColor, indoor);
	}

	public int method_1(int offset, int count)
	{
		int stride = Marshal.SizeOf<Struct168>();
		if (List_0.Count == ComputeBuffer_0.count)
		{
			ComputeBuffer_0 = new ComputeBuffer(ComputeBuffer_0.count + Int_0, stride, ComputeBufferType.DrawIndirect);
			ComputeBuffer_0.name = "indirectDataGlobal";
			ComputeBuffer_0.SetData(List_0);
			ComputeBuffer_1 = new ComputeBuffer(ComputeBuffer_1.count + Int_0, stride, ComputeBufferType.DrawIndirect);
			ComputeBuffer_1.name = "indirectDataLocal";
			ComputeBuffer_1.SetData(List_0);
			ReAssignDrawArgsBufferToMPBs();
		}
		int num = List_0.Count;
		if (List_1.Count > 0)
		{
			num = List_1[List_1.Count - 1];
			List_1.RemoveAt(List_1.Count - 1);
		}
		List_0.Add(new Struct168
		{
			vertexCountPerInstance = count,
			instanceCount = 1,
			startVertex = offset,
			startInstance = 0
		});
		ComputeBuffer_0.SetData(List_0, num, num, 1);
		ComputeBuffer_1.SetData(List_0, num, num, 1);
		return num;
	}

	public int method_2(Material material)
	{
		int num = 0;
		if (List_3.Count > 0)
		{
			num = List_3[List_3.Count - 1];
			List_3.RemoveAt(List_3.Count - 1);
		}
		else
		{
			int stride = Marshal.SizeOf<Struct167>();
			List_2.Add(default(Struct167));
			num = List_2.Count - 1;
			ComputeBuffer_2 = new ComputeBuffer(List_2.Count, stride, ComputeBufferType_0);
			ComputeBuffer_2.SetData(List_2, 0, 0, List_2.Count - 1);
			ReAssignDrawParamsBuffersToMPBs();
		}
		Struct167 value = method_3(material);
		List_2[num] = value;
		ComputeBuffer_2.SetData(List_2, num, num, 1);
		Texture2D mainTexture = (material.HasProperty("_MainTex") ? (material.GetTexture("_MainTex") as Texture2D) : null);
		Texture2D noiseTexture = (material.HasProperty("_NoiseTex") ? (material.GetTexture("_NoiseTex") as Texture2D) : null);
		Struct173 item = new Struct173
		{
			drawParamsIndex = num,
			mainTexture = mainTexture,
			noiseTexture = noiseTexture
		};
		List_6.Add(item);
		return num;
	}

	public Struct167 method_3(Material material)
	{
		Texture2D texture2D = (material.HasProperty("_MainTex") ? (material.GetTexture("_MainTex") as Texture2D) : null);
		if (!material.HasProperty("_TintColor"))
		{
			Debug.LogError("Add particleSystem material: " + material.name);
		}
		Color color = material.GetColor("_TintColor");
		float animationSpeed = (material.HasProperty("_AnimationSpeed") ? material.GetFloat("_AnimationSpeed") : 0f);
		float distortion = (material.HasProperty("_Distortion") ? material.GetFloat("_Distortion") : 0f);
		float invFade = (material.HasProperty("_InvFade") ? material.GetFloat("_InvFade") : 0f);
		Vector4 indoorTintColor = (material.HasProperty("_IndoorTintColor") ? material.GetVector("_IndoorTintColor") : Vector4.zero);
		float indoor = (material.HasProperty("_Indoor") ? material.GetFloat("_Indoor") : 0f);
		Vector4 mainTex_ST = new Vector4(texture2D.width, texture2D.height, 1f / (float)texture2D.width, 1f / (float)texture2D.height);
		return new Struct167
		{
			_MainTex_ST = mainTex_ST,
			_TintColor = color,
			_AnimationSpeed = animationSpeed,
			_Distortion = distortion,
			_InvFade = invFade,
			_IndoorTintColor = indoorTintColor,
			_Indoor = indoor,
			_MainTexArrayOffset = 0u,
			_NoiseTexArrayOffset = 0u
		};
	}

	public int method_4(int argsOffset)
	{
		return argsOffset + 1;
	}

	public void BeforeRendering(CommandBuffer cmdBuf)
	{
		cmdBuf.BeginSample("MBOIT Particles Before Render");
		if (List_5.Count == 0)
		{
			cmdBuf.EndSample("MBOIT Particles Before Render");
			return;
		}
		cmdBuf.SetComputeBufferParam(ComputeShader_1, 0, "_GlobalDrawArgs", ComputeBuffer_0);
		cmdBuf.SetComputeBufferParam(ComputeShader_1, 0, "_IndirectDispatchArgs", ComputeBuffer_3);
		cmdBuf.DispatchCompute(ComputeShader_1, 0, 1, 1, 1);
		int num;
		for (int i = 0; i < List_5.Count; i += num)
		{
			int kernelIndex = 1;
			if (List_5.Count > i + 1)
			{
				kernelIndex = 2;
				if (List_5.Count > i + 2)
				{
					kernelIndex = 3;
				}
			}
			num = 1;
			cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_GlobalDrawArgs", ComputeBuffer_0);
			cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_GlobalVertices", ParticlesVertices);
			cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_GlobalOffsets", ParticlesOffsets);
			Class701 particlesBuffers = List_5[i].ParticlesBuffers;
			cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_LocalVertices0", particlesBuffers.ParticlesVertices);
			cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_LocalOffsets0", particlesBuffers.ParticlesOffsets);
			cmdBuf.SetComputeIntParam(ComputeShader_1, "_ShaderIndex0", method_4(List_5[i].DrawArgsOffset));
			if (List_5.Count > i + 1)
			{
				num = 2;
				Class701 particlesBuffers2 = List_5[i + 1].ParticlesBuffers;
				cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_LocalVertices1", particlesBuffers2.ParticlesVertices);
				cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_LocalOffsets1", particlesBuffers2.ParticlesOffsets);
				cmdBuf.SetComputeIntParam(ComputeShader_1, "_ShaderIndex1", method_4(List_5[i + 1].DrawArgsOffset));
				if (List_5.Count > i + 2)
				{
					num = 3;
					Class701 particlesBuffers3 = List_5[i + 2].ParticlesBuffers;
					cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_LocalVertices2", particlesBuffers3.ParticlesVertices);
					cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_LocalOffsets2", particlesBuffers3.ParticlesOffsets);
					cmdBuf.SetComputeIntParam(ComputeShader_1, "_ShaderIndex2", method_4(List_5[i + 2].DrawArgsOffset));
				}
			}
			cmdBuf.SetComputeIntParam(ComputeShader_1, "_ShaderIndexOffset", i);
			cmdBuf.SetComputeIntParam(ComputeShader_1, "_DrawArgsOffset", 0);
			cmdBuf.SetComputeBufferParam(ComputeShader_1, kernelIndex, "_DrawArgs", ComputeBuffer_1);
			cmdBuf.DispatchCompute(ComputeShader_1, kernelIndex, ComputeBuffer_3, 0u);
		}
		cmdBuf.EndSample("MBOIT Particles Before Render");
	}

	public void ProcessJobs()
	{
		Class700_1.ProcessDelayedJobs();
		Class700_0.ProcessDelayedJobs();
		if (List_6.Count > 0)
		{
			CommandBuffer commandBuffer = new CommandBuffer();
			commandBuffer.name = "MBOITParticlesTmpBuffer";
			HashSet<Texture2D> hashSet = new HashSet<Texture2D>();
			HashSet<Texture2D> hashSet2 = new HashSet<Texture2D>();
			for (int i = 0; i < List_6.Count; i++)
			{
				Struct173 @struct = List_6[i];
				if (Class700_1.ContainsTexture(@struct.mainTexture) < 0)
				{
					if (@struct.mainTexture != null)
					{
						hashSet.Add(@struct.mainTexture);
					}
					else
					{
						hashSet.Add(null);
					}
				}
				if (Class700_0.ContainsTexture(@struct.noiseTexture) < 0)
				{
					if (@struct.noiseTexture != null)
					{
						hashSet2.Add(@struct.noiseTexture);
					}
					else
					{
						hashSet2.Add(null);
					}
				}
			}
			Class700_1.ReserveTextureSpace(hashSet.Count, commandBuffer);
			Class700_0.ReserveTextureSpace(hashSet2.Count, commandBuffer);
			for (int j = 0; j < List_6.Count; j++)
			{
				Struct173 struct2 = List_6[j];
				int drawParamsIndex = struct2.drawParamsIndex;
				Struct167 value = List_2[drawParamsIndex];
				int num = Class700_1.ContainsTexture(struct2.mainTexture);
				value._MainTexArrayOffset = (uint)((num > -1) ? num : Class700_1.AddTexture(commandBuffer, struct2.mainTexture));
				int num2 = Class700_0.ContainsTexture(struct2.noiseTexture);
				value._NoiseTexArrayOffset = (uint)((num2 > -1) ? num2 : Class700_0.AddTexture(commandBuffer, struct2.noiseTexture));
				List_2[drawParamsIndex] = value;
				ComputeBuffer_2.SetData(List_2, drawParamsIndex, drawParamsIndex, 1);
			}
			List_6.Clear();
			Graphics.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Release();
		}
		for (int k = 0; k < List_4.Count; k++)
		{
			Struct171 value2 = List_4[k];
			if (value2.ParticleSystem.main.maxParticles > value2.MaxParticles)
			{
				RemoveParticleSystem(value2.ParticleSystem);
				AddParticleSystem(value2.ParticleSystem);
				value2.MaxParticles = value2.ParticleSystem.main.maxParticles;
				List_4[k] = value2;
			}
		}
	}

	public void method_5()
	{
		foreach (Struct175 value in Dictionary_1.Values)
		{
			value.material.SetBuffer("_Vertices", ParticlesVertices);
			value.material.SetBuffer("_ParticlesOffsetsGlobal", ParticlesOffsets);
		}
	}

	public void method_6()
	{
		foreach (KeyValuePair<string, Struct172> item in Dictionary_0)
		{
			item.Value.Material.SetBuffer("_LocalVertices", item.Value.ParticlesBuffers.ParticlesVertices);
			item.Value.Material.SetBuffer("_ParticlesOffsetsLocal", item.Value.ParticlesBuffers.ParticlesOffsets);
		}
	}

	public void ReAssignDrawArgsBufferToMPBs()
	{
		foreach (Struct175 value in Dictionary_1.Values)
		{
			value.material.SetBuffer("_DrawArgs", ComputeBuffer_0);
		}
	}

	public void ReAssignDrawParamsBuffersToMPBs()
	{
		foreach (Struct175 value in Dictionary_1.Values)
		{
			value.material.SetBuffer("_DrawParams", ComputeBuffer_2);
		}
	}

	public void method_7()
	{
		foreach (Struct175 value in Dictionary_1.Values)
		{
			value.material.SetTexture("_MainTexArray", Class700_1.Storage);
		}
	}

	public void method_8()
	{
		foreach (Struct175 value in Dictionary_1.Values)
		{
			value.material.SetTexture("_NoiseTexArray", Class700_0.Storage);
		}
	}

	public void RemoveParticleSystem(ParticleSystem ps)
	{
		if (Dictionary_1.ContainsKey(ps))
		{
			Class701_0.RemoveParticleSystem(ps);
			string name = ps.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.name;
			Struct172 @struct = Dictionary_0[name];
			@struct.ParticleSystems.Remove(ps);
			Struct175 struct2 = Dictionary_1[ps];
			if (@struct.ParticleSystems.Count == 0)
			{
				int item = struct2.renderArgsOffset >> 2;
				List_1.Add(item);
			}
			List_3.Add(struct2.drawParamsOffset);
			Dictionary_1.Remove(ps);
		}
	}

	public void RenderParticles(CommandBuffer cmdBuf, Camera camera)
	{
		if (Class701_0.HasData())
		{
			for (int i = 0; i < List_5.Count; i++)
			{
				Struct172 @struct = List_5[i];
				int val = @struct.Material.FindPass("MBOIT_Pass");
				val = Math.Max(val, 0);
				int drawArgsOffset = @struct.DrawArgsOffset;
				cmdBuf.DrawProceduralIndirect(Matrix4x4.identity, @struct.Material, val, MeshTopology.Triangles, ComputeBuffer_1, drawArgsOffset * 16);
			}
		}
	}

	public void ClearIndirectDrawData(CommandBuffer cmdBuf)
	{
		cmdBuf.BeginSample("Clear Particles Indirect Data");
		int threadGroupsX = List_0.Count / 64 + 1;
		cmdBuf.SetComputeBufferParam(ComputeShader_0, 0, "_DrawArgs", ComputeBuffer_0);
		cmdBuf.SetComputeIntParam(ComputeShader_0, "_DrawArgsCount", List_0.Count);
		cmdBuf.DispatchCompute(ComputeShader_0, 0, threadGroupsX, 1, 1);
		cmdBuf.SetComputeBufferParam(ComputeShader_0, 0, "_DrawArgs", ComputeBuffer_1);
		cmdBuf.SetComputeIntParam(ComputeShader_0, "_DrawArgsCount", List_0.Count);
		cmdBuf.DispatchCompute(ComputeShader_0, 0, threadGroupsX, 1, 1);
		cmdBuf.EndSample("Clear Particles Indirect Data");
	}
}
