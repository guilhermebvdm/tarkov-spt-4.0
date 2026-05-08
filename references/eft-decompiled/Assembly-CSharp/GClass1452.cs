using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass1452 : GInterface133
{
	public enum EBlendType : byte
	{
		None,
		Simple1D,
		FreeformDirectional2D,
		FreeformCartesian2D,
		SimpleDirectional2D,
		Direct
	}

	[NonSerialized]
	public const float Float_0 = 1f / MathF.PI;

	[NonSerialized]
	public const float Float_1 = 0.01f;

	[NonSerialized]
	public static Vector3 Vector3_0 = Vector3.zero;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public List<GInterface133> List_0 = new List<GInterface133>();

	[NonSerialized]
	public List<GStruct141> List_1 = new List<GStruct141>(15);

	[NonSerialized]
	public EBlendType EblendType_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public float[] Float_2;

	[NonSerialized]
	public Vector2[] Vector2_0;

	[NonSerialized]
	public int[] Int_3;

	[NonSerialized]
	public float[] Float_3;

	[NonSerialized]
	public Vector3[] Vector3_1;

	[NonSerialized]
	public Quaternion[] Quaternion_0;

	[NonSerialized]
	public float[] Float_4;

	[NonSerialized]
	public Vector2[] Vector2_1;

	[NonSerialized]
	public float[] Float_5;

	[NonSerialized]
	public Vector2[] Vector2_2;

	[NonSerialized]
	public float[] Float_6;

	[NonSerialized]
	public int[] Int_4;

	[NonSerialized]
	public bool[] Bool_0;

	[NonSerialized]
	public int[][] Int_5;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public string Name => String_0;

	public EBlendType Type => EblendType_0;

	public int ParameterXHash => Int_0;

	public int ParameterYHash => Int_1;

	public float MaxClipTime => 0f;

	public bool IsLooping
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
	}

	public Vector3 PosMotion => Vector3.zero;

	public Quaternion PosRotation => Quaternion.identity;

	public int ChildrenCount => Int_2;

	public GClass1452()
	{
	}

	public GClass1452(string name, EBlendType blendType, int childrenLength, int parameterXHash, int parameterYHash)
	{
		String_0 = name;
		EblendType_0 = blendType;
		Int_2 = childrenLength;
		Int_0 = parameterXHash;
		Int_1 = parameterYHash;
		Float_3 = new float[childrenLength];
		Float_2 = new float[childrenLength];
		Vector3_1 = new Vector3[childrenLength];
		Quaternion_0 = new Quaternion[childrenLength];
		Float_4 = new float[childrenLength];
		Vector2_0 = new Vector2[childrenLength];
		Int_3 = new int[childrenLength];
		Vector2_1 = new Vector2[childrenLength];
		Float_5 = new float[childrenLength];
		Vector2_2 = new Vector2[childrenLength * childrenLength];
		Float_6 = new float[childrenLength * childrenLength];
		Int_4 = new int[childrenLength];
		Bool_0 = new bool[childrenLength * childrenLength];
		Int_5 = new int[childrenLength][];
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write((byte)EblendType_0);
		writer.Write(Int_0);
		writer.Write(Int_1);
		writer.Write(Int_2);
		writer.Write(Float_2.Length);
		for (int i = 0; i < Float_2.Length; i++)
		{
			writer.Write(Float_2[i]);
		}
		writer.Write(Vector2_0.Length);
		for (int j = 0; j < Vector2_0.Length; j++)
		{
			writer.Write(Vector2_0[j].x);
			writer.Write(Vector2_0[j].y);
		}
		writer.Write(Int_3.Length);
		for (int k = 0; k < Int_3.Length; k++)
		{
			writer.Write(Int_3[k]);
		}
	}

	public void Deserialize(BinaryReader reader)
	{
		EblendType_0 = (EBlendType)reader.ReadByte();
		Int_0 = reader.ReadInt32();
		Int_1 = reader.ReadInt32();
		Int_2 = reader.ReadInt32();
		int num = reader.ReadInt32();
		Float_2 = new float[num];
		for (int i = 0; i < Float_2.Length; i++)
		{
			Float_2[i] = reader.ReadSingle();
		}
		int num2 = reader.ReadInt32();
		Vector2_0 = new Vector2[num2];
		for (int j = 0; j < num2; j++)
		{
			Vector2_0[j] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
		}
		int num3 = reader.ReadInt32();
		Int_3 = new int[num3];
		for (int k = 0; k < num3; k++)
		{
			Int_3[k] = reader.ReadInt32();
		}
		Float_3 = new float[Int_2];
		Vector3_1 = new Vector3[Int_2];
		Quaternion_0 = new Quaternion[Int_2];
		Float_4 = new float[Int_2];
	}

	public void InitChilds(IEnumerable<GInterface133> nodes)
	{
		List_0.Clear();
		List_0.AddRange(nodes);
	}

	public void InitBlend1DParameters(float[] childrenThresholds)
	{
		Float_2 = childrenThresholds;
	}

	public void InitDirectBlendParameters(int[] directBlendHashes)
	{
		Int_3 = directBlendHashes;
	}

	public void InitBlend2DParameters(Vector2[] childrenPositions)
	{
		Vector2_0 = childrenPositions;
		method_0();
	}

	public void method_0()
	{
		if (EblendType_0 == EBlendType.FreeformDirectional2D)
		{
			for (int i = 0; i < Int_2; i++)
			{
				Float_5[i] = Vector2_0[i].magnitude;
			}
			for (int j = 0; j < Int_2; j++)
			{
				for (int k = 0; k < Int_2; k++)
				{
					int num = j + k * Int_2;
					float num2 = Float_5[k] + Float_5[j];
					if (num2 > 0f)
					{
						Float_6[num] = 2f / num2;
					}
					else
					{
						Float_6[num] = 2f / num2;
					}
					float y = (Float_5[k] - Float_5[j]) * Float_6[num];
					if (!(Math.Abs(Float_5[k]) < Mathf.Epsilon) && Math.Abs(Float_5[j]) >= Mathf.Epsilon)
					{
						float num3 = method_12(Vector2_0[j], Vector2_0[k]);
						if (Vector2_0[j].x * Vector2_0[k].y - Vector2_0[j].y * Vector2_0[k].x < 0f)
						{
							num3 = 0f - num3;
						}
						Vector2_2[num] = new Vector2(num3, y);
					}
					else
					{
						Vector2_2[num] = new Vector2(0f, y);
					}
				}
			}
		}
		else if (EblendType_0 == EBlendType.FreeformCartesian2D)
		{
			for (int l = 0; l < Int_2; l++)
			{
				for (int m = 0; m < Int_2; m++)
				{
					int num4 = l + m * Int_2;
					Float_6[num4] = 1f / Vector2.SqrMagnitude(Vector2_0[m] - Vector2_0[l]);
					Vector2_2[num4] = Vector2_0[m] - Vector2_0[l];
				}
			}
		}
		for (int n = 0; n < Int_2 * Int_2; n++)
		{
			Bool_0[n] = false;
		}
		float num5 = 10000f;
		float num6 = -10000f;
		float num7 = 10000f;
		float num8 = -10000f;
		for (int num9 = 0; num9 < Int_2; num9++)
		{
			num5 = Mathf.Min(num5, Vector2_0[num9].x);
			num6 = Mathf.Max(num6, Vector2_0[num9].x);
			num7 = Mathf.Min(num7, Vector2_0[num9].y);
			num8 = Mathf.Max(num8, Vector2_0[num9].y);
		}
		float num10 = (num6 - num5) * 0.5f;
		float num11 = (num8 - num7) * 0.5f;
		num5 -= num10;
		num6 += num10;
		num7 -= num11;
		num8 += num11;
		for (int num12 = 0; num12 <= 100; num12++)
		{
			for (int num13 = 0; num13 <= 100; num13++)
			{
				float num14 = (float)num12 * 0.01f;
				float num15 = (float)num13 * 0.01f;
				if (EblendType_0 == EBlendType.FreeformDirectional2D)
				{
					method_3(num5 * (1f - num14) + num6 * num14, num7 * (1f - num15) + num8 * num15, preCompute: true);
				}
				else if (EblendType_0 == EBlendType.FreeformCartesian2D)
				{
					method_4(num5 * (1f - num14) + num6 * num14, num7 * (1f - num15) + num8 * num15, preCompute: true);
				}
				for (int num16 = 0; num16 < Int_2; num16++)
				{
					if (Int_4[num16] >= 0)
					{
						Bool_0[num16 * Int_2 + Int_4[num16]] = true;
					}
				}
			}
		}
		for (int num17 = 0; num17 < Int_2; num17++)
		{
			List<int> list = new List<int>();
			for (int num18 = 0; num18 < Int_2; num18++)
			{
				if (Bool_0[num17 * Int_2 + num18])
				{
					list.Add(num18);
				}
			}
			Int_5[num17] = new int[list.Count];
			for (int num19 = 0; num19 < list.Count; num19++)
			{
				Int_5[num17][num19] = list[num19];
			}
		}
	}

	public Vector3 ComputeRootPosition(float deltaTime, float stateNormalizedTime, GInterface134 parametersCache)
	{
		method_1(parametersCache);
		return ComputeRootPosition(deltaTime, stateNormalizedTime, parametersCache, Float_3);
	}

	public Quaternion ComputeDeltaRotation(float deltaTime, GInterface134 parametersCache)
	{
		method_1(parametersCache);
		return ComputeDeltaRotation(deltaTime, parametersCache, Float_3);
	}

	public float ComputeClipTime(GInterface134 parametersCache)
	{
		method_1(parametersCache);
		return ComputeClipTime(parametersCache, Float_3);
	}

	public void ComputeActiveClips(List<GStruct141> nodes, GInterface134 parametersCache)
	{
		method_1(parametersCache);
		ComputeActiveNodes(nodes, parametersCache, Float_3);
	}

	public Vector3 ComputeRootPosition(float deltaTime, float stateNormalizedTime, GInterface134 parametersCache, float[] weights)
	{
		for (int i = 0; i < Vector3_1.Length; i++)
		{
			if (weights[i] < 0.01f)
			{
				Vector3 vector3_ = Vector3_0;
				Vector3_1[i] = vector3_;
			}
			else
			{
				Vector3_1[i] = List_0[i].ComputeRootPosition(deltaTime, stateNormalizedTime, parametersCache);
			}
		}
		return method_9(in weights, in Vector3_1);
	}

	public Quaternion ComputeDeltaRotation(float deltaTime, GInterface134 parametersCache, float[] weights)
	{
		for (int i = 0; i < Quaternion_0.Length; i++)
		{
			if (Math.Abs(weights[i]) < 0.01f)
			{
				Quaternion_0[i] = Quaternion.identity;
			}
			else
			{
				Quaternion_0[i] = List_0[i].ComputeDeltaRotation(deltaTime, parametersCache);
			}
		}
		return method_10(in weights, in Quaternion_0);
	}

	public float ComputeClipTime(GInterface134 parametersCache, float[] weights)
	{
		for (int i = 0; i < Float_4.Length; i++)
		{
			if (Math.Abs(weights[i]) < 0.01f)
			{
				Float_4[i] = 0f;
			}
			else
			{
				Float_4[i] = ((i < List_0.Count) ? List_0[i].ComputeClipTime(parametersCache) : 0f);
			}
		}
		return method_11(in weights, in Float_4);
	}

	public void ComputeActiveNodes(List<GStruct141> nodes, GInterface134 parametersCache, float[] weights)
	{
		for (int i = 0; i < Float_4.Length; i++)
		{
			if (!(weights[i] < 0.01f))
			{
				List_1.Clear();
				float num = weights[i];
				List_0[i].ComputeActiveClips(List_1, parametersCache);
				for (int j = 0; j < List_1.Count; j++)
				{
					GStruct141 value = List_1[j];
					value.Weight *= num;
					value.RawWeight = value.Weight;
					List_1[j] = value;
				}
				nodes.AddRange(List_1);
			}
		}
	}

	public void ComputeWeights(float blendValueX, float blendValueY, ref float[] weights)
	{
		switch (EblendType_0)
		{
		case EBlendType.Simple1D:
			method_6(blendValueX);
			goto default;
		case EBlendType.FreeformDirectional2D:
			method_3(blendValueX, blendValueY);
			goto default;
		case EBlendType.FreeformCartesian2D:
			method_4(blendValueX, blendValueY);
			goto default;
		case EBlendType.SimpleDirectional2D:
			method_5(blendValueX, blendValueY);
			goto default;
		default:
		{
			for (int i = 0; i < Int_2; i++)
			{
				weights[i] = Float_3[i];
			}
			break;
		}
		case EBlendType.Direct:
			throw new NotImplementedException();
		}
	}

	public void method_1(GInterface134 parametersCache)
	{
		switch (EblendType_0)
		{
		case EBlendType.Simple1D:
		{
			float parameter7 = parametersCache.GetParameter(ParameterXHash);
			method_6(parameter7);
			break;
		}
		case EBlendType.FreeformDirectional2D:
		{
			float parameter5 = parametersCache.GetParameter(ParameterXHash);
			float parameter6 = parametersCache.GetParameter(ParameterYHash);
			method_3(parameter5, parameter6);
			break;
		}
		case EBlendType.FreeformCartesian2D:
		{
			float parameter3 = parametersCache.GetParameter(ParameterXHash);
			float parameter4 = parametersCache.GetParameter(ParameterYHash);
			method_4(parameter3, parameter4);
			break;
		}
		case EBlendType.SimpleDirectional2D:
		{
			float parameter = parametersCache.GetParameter(ParameterXHash);
			float parameter2 = parametersCache.GetParameter(ParameterYHash);
			method_5(parameter, parameter2);
			break;
		}
		case EBlendType.Direct:
			method_8(parametersCache);
			break;
		}
	}

	public float method_2(int i, int j, Vector2 blendPosition)
	{
		int num = i + j * Int_2;
		Vector2 vector = Vector2_2[num];
		Vector2 rhs = Vector2_1[i];
		rhs.y *= Float_6[num];
		if (Vector2_0[i] == Vector2.zero)
		{
			vector.x = Vector2_1[j].x;
		}
		else if (Vector2_0[j] == Vector2.zero)
		{
			vector.x = Vector2_1[i].x;
		}
		else if (vector.x == 0f || blendPosition == Vector2.zero)
		{
			rhs.x = vector.x;
		}
		return 1f - Vector2.Dot(vector, rhs) / Vector2.SqrMagnitude(vector);
	}

	public void method_3(float blendValueX, float blendValueY, bool preCompute = false)
	{
		Vector2 vector = new Vector2(blendValueX, blendValueY);
		float magnitude = vector.magnitude;
		if (vector == Vector2.zero)
		{
			for (int i = 0; i < Int_2; i++)
			{
				Vector2_1[i] = new Vector2(0f, magnitude - Float_5[i]);
			}
		}
		else
		{
			for (int j = 0; j < Int_2; j++)
			{
				if (Vector2_0[j] == Vector2.zero)
				{
					Vector2_1[j] = new Vector2(0f, magnitude - Float_5[j]);
					continue;
				}
				float num = method_12(Vector2_0[j], vector);
				if (Vector2_0[j].x * vector.y - Vector2_0[j].y * vector.x < 0f)
				{
					num = 0f - num;
				}
				Vector2_1[j] = new Vector2(num, magnitude - Float_5[j]);
			}
		}
		if (preCompute)
		{
			for (int k = 0; k < Int_2; k++)
			{
				float num2 = 1f - Mathf.Abs(Vector2_1[k].x) * (1f / MathF.PI);
				Int_4[k] = -1;
				for (int l = 0; l < Int_2; l++)
				{
					if (k != l)
					{
						float num3 = method_2(k, l, vector);
						if (!(num3 > 0f))
						{
							Int_4[k] = -1;
							break;
						}
						if (num3 < num2)
						{
							Int_4[k] = l;
						}
						num2 = Mathf.Min(num2, num3);
					}
				}
			}
			return;
		}
		for (int m = 0; m < Int_2; m++)
		{
			float num4 = 1f - Mathf.Abs(Vector2_1[m].x) * (1f / MathF.PI);
			for (int n = 0; n < Int_5[m].Length; n++)
			{
				int j2 = Int_5[m][n];
				float num5 = method_2(m, j2, vector);
				if (num5 > 0f)
				{
					num4 = Mathf.Min(num4, num5);
					continue;
				}
				num4 = 0f;
				break;
			}
			Float_3[m] = num4;
		}
		float num6 = 0f;
		for (int num7 = 0; num7 < Int_2; num7++)
		{
			num6 += Float_3[num7];
		}
		if (num6 > 0f)
		{
			num6 = 1f / num6;
			for (int num8 = 0; num8 < Int_2; num8++)
			{
				Float_3[num8] *= num6;
			}
		}
		else
		{
			float num9 = 1f / (float)Int_2;
			for (int num10 = 0; num10 < Int_2; num10++)
			{
				Float_3[num10] = num9;
			}
		}
	}

	public void method_4(float blendValueX, float blendValueY, bool preCompute = false)
	{
		Vector2 vector = new Vector2(blendValueX, blendValueY);
		for (int i = 0; i < Int_2; i++)
		{
			Vector2_1[i] = vector - Vector2_0[i];
		}
		if (preCompute)
		{
			for (int j = 0; j < Int_2; j++)
			{
				Vector2 rhs = Vector2_1[j];
				float num = 1f;
				Int_4[j] = -1;
				for (int k = 0; k < Int_2; k++)
				{
					if (j != k)
					{
						int num2 = j + k * Int_2;
						Vector2 lhs = Vector2_2[num2];
						float num3 = 1f - Vector2.Dot(lhs, rhs) * Float_6[num2];
						if (!(num3 > 0f))
						{
							Int_4[j] = -1;
							break;
						}
						if (num3 < num)
						{
							Int_4[j] = k;
						}
						num = Mathf.Min(num, num3);
					}
				}
			}
			return;
		}
		for (int l = 0; l < Int_2; l++)
		{
			Vector2 rhs2 = Vector2_1[l];
			float num4 = 1f;
			for (int m = 0; m < Int_5[l].Length; m++)
			{
				int num5 = Int_5[l][m];
				if (l != num5)
				{
					int num6 = l + num5 * Int_2;
					Vector2 lhs2 = Vector2_2[num6];
					float num7 = 1f - Vector2.Dot(lhs2, rhs2) * Float_6[num6];
					if (!(num7 >= 0f))
					{
						num4 = 0f;
						break;
					}
					num4 = Mathf.Min(num4, num7);
				}
			}
			Float_3[l] = num4;
		}
		float num8 = 0f;
		for (int n = 0; n < Int_2; n++)
		{
			num8 += Float_3[n];
		}
		num8 = 1f / num8;
		for (int num9 = 0; num9 < Int_2; num9++)
		{
			Float_3[num9] *= num8;
		}
	}

	public void method_5(float blendValueX, float blendValueY)
	{
		Vector2 vector = new Vector2(blendValueX, blendValueY);
		for (int i = 0; i < Int_2; i++)
		{
			Float_3[i] = 0f;
		}
		if (Int_2 < 2)
		{
			if (Int_2 == 1)
			{
				Float_3[0] = 1f;
			}
			return;
		}
		if (vector == Vector2.zero)
		{
			int num = 0;
			while (true)
			{
				if (num < Int_2)
				{
					if (!(Vector2_0[num] != Vector2.zero))
					{
						break;
					}
					num++;
					continue;
				}
				float num2 = 1f / (float)Int_2;
				for (int j = 0; j < Int_2; j++)
				{
					Float_3[j] = num2;
				}
				return;
			}
			Float_3[num] = 1f;
			return;
		}
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		float num6 = -100000f;
		float num7 = -100000f;
		int num8 = 0;
		while (true)
		{
			if (num8 < Int_2)
			{
				if (Vector2_0[num8] == Vector2.zero)
				{
					if (num5 >= 0)
					{
						break;
					}
					num5 = num8;
				}
				else
				{
					Vector2 normalized = Vector2_0[num8].normalized;
					float num9 = Vector2.Dot(normalized, vector);
					if (normalized.x * vector.y - normalized.y * vector.x > 0f)
					{
						if (num9 > num7)
						{
							num7 = num9;
							num3 = num8;
						}
					}
					else if (num9 > num6)
					{
						num6 = num9;
						num4 = num8;
					}
				}
				num8++;
				continue;
			}
			float num10 = 0f;
			if (num3 >= 0 && num4 >= 0)
			{
				Vector2 vector2 = Vector2_0[num3];
				Vector2 vector3 = Vector2_0[num4];
				float num11 = vector3.y * vector2.x - vector3.x * vector2.y;
				float num12 = (vector3.y * blendValueX - vector3.x * blendValueY) / num11;
				float num13 = (vector2.x * blendValueY - vector2.y * blendValueX) / num11;
				num10 = 1f - num12 - num13;
				if (num10 < 0f)
				{
					num10 = 0f;
					float num14 = num12 + num13;
					num12 /= num14;
					num13 /= num14;
				}
				else if (num10 > 1f)
				{
					num10 = 1f;
					num12 = 0f;
					num13 = 0f;
				}
				Float_3[num3] = num12;
				Float_3[num4] = num13;
			}
			else
			{
				num10 = 1f;
			}
			if (num5 >= 0)
			{
				Float_3[num5] = num10;
				break;
			}
			float num15 = 1f / (float)Int_2;
			for (int k = 0; k < Int_2; k++)
			{
				Float_3[k] += num15 * num10;
			}
			break;
		}
	}

	public void method_6(float blendValue)
	{
		for (int i = 0; i < Int_2; i++)
		{
			Float_3[i] = method_7(i, blendValue);
		}
	}

	public float method_7(int index, float blend)
	{
		if (blend >= Float_2[index])
		{
			if (index + 1 == Int_2)
			{
				return 1f;
			}
			if (Float_2[index + 1] < blend)
			{
				return 0f;
			}
			if (Math.Abs(Float_2[index] - Float_2[index + 1]) > Mathf.Epsilon)
			{
				return (blend - Float_2[index + 1]) / (Float_2[index] - Float_2[index + 1]);
			}
			return 1f;
		}
		if (index == 0)
		{
			return 1f;
		}
		if (Float_2[index - 1] > blend)
		{
			return 0f;
		}
		if (Math.Abs(Float_2[index] - Float_2[index - 1]) > Mathf.Epsilon)
		{
			return (blend - Float_2[index - 1]) / (Float_2[index] - Float_2[index - 1]);
		}
		return 1f;
	}

	public void method_8(GInterface134 allParameters)
	{
		for (int i = 0; i < Int_2; i++)
		{
			Float_3[i] = allParameters.GetParameter(Int_3[i]);
		}
	}

	public Vector3 method_9(in float[] childBlends, in Vector3[] childRootPositions)
	{
		Vector3 vector3_ = Vector3_0;
		for (int i = 0; i < childBlends.Length; i++)
		{
			float num = childBlends[i];
			if (!(num < 0.01f))
			{
				Vector3 vector = childRootPositions[i];
				vector3_.x += vector.x * num;
				vector3_.y += vector.y * num;
				vector3_.z += vector.z * num;
			}
		}
		return vector3_;
	}

	public Quaternion method_10(in float[] childBlends, in Quaternion[] childRotations)
	{
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 0f);
		float num = 0f;
		for (int i = 0; i < childBlends.Length; i++)
		{
			float num2 = childBlends[i];
			if (!(num2 < 0.01f))
			{
				Quaternion b = childRotations[i];
				float num3 = ((Quaternion.Dot(quaternion, b) < 0f) ? (-1f) : 1f) * num2;
				num += num2;
				quaternion.x += b.x * num3;
				quaternion.y += b.y * num3;
				quaternion.z += b.z * num3;
				quaternion.w += b.w * num3;
			}
		}
		quaternion.w += Mathf.Clamp01(1f - num);
		return Quaternion.Normalize(quaternion);
	}

	public float method_11(in float[] childBlends, in float[] childClipTimes)
	{
		float num = 0f;
		for (int i = 0; i < childBlends.Length; i++)
		{
			num += childClipTimes[i] * childBlends[i];
		}
		return num;
	}

	public float method_12(Vector2 lhs, Vector2 rhs)
	{
		return Mathf.Acos(Mathf.Min(1f, Mathf.Max(-1f, Vector2.Dot(lhs, rhs) / (lhs.magnitude * rhs.magnitude))));
	}
}
