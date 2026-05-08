using System;
using System.Collections.Generic;
using System.IO;
using AnimationSystem.RootMotionTable;
using UnityEngine;

public class GClass1450 : GClass1448
{
	[NonSerialized]
	public GClass1452 Gclass1452_0;

	[NonSerialized]
	public float[] Float_1;

	[NonSerialized]
	public float[] Float_2;

	[NonSerialized]
	public float[][][] Float_3;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public RootMotionBlendTable.ParameterSettings ParameterSettings_0;

	[NonSerialized]
	public RootMotionBlendTable.ParameterSettings ParameterSettings_1;

	public override int ParameterXHash => Gclass1452_0.ParameterXHash;

	public override int ParameterYHash => Gclass1452_0.ParameterYHash;

	public override EComputedNodeType NodeType => EComputedNodeType.Tree;

	public GClass1450(GInterface135 blendParamsKeeper)
		: base(blendParamsKeeper)
	{
	}

	public override void StoreAllVariants(RootMotionBlendTable rootMotionTable)
	{
		base.StoreAllVariants(rootMotionTable);
		if (Gclass1452_0.Type == GClass1452.EBlendType.Simple1D || Gclass1452_0.Type == GClass1452.EBlendType.Direct)
		{
			return;
		}
		RootMotionBlendTable.ParameterSettings parameter = rootMotionTable.GetParameter(Gclass1452_0.ParameterXHash);
		RootMotionBlendTable.ParameterSettings parameter2 = rootMotionTable.GetParameter(Gclass1452_0.ParameterYHash);
		int totalSteps = parameter.TotalSteps;
		int totalSteps2 = parameter2.TotalSteps;
		Int_2 = totalSteps;
		Int_3 = totalSteps2;
		Int_4 = Gclass1452_0.ChildrenCount;
		method_0();
		for (int i = 0; i < Int_2; i++)
		{
			float value = parameter.GetValue(i);
			for (int j = 0; j < Int_3; j++)
			{
				float value2 = parameter2.GetValue(j);
				Gclass1452_0.ComputeWeights(value, value2, ref Float_3[i][j]);
			}
		}
	}

	public void method_0()
	{
		Float_3 = new float[Int_2][][];
		for (int i = 0; i < Int_2; i++)
		{
			Float_3[i] = new float[Int_3][];
			for (int j = 0; j < Int_3; j++)
			{
				Float_3[i][j] = new float[Int_4];
			}
		}
	}

	public GClass1450(GClass1452 customBlendTree, GInterface133 originalNode, GInterface135 blendParamsKeeper, bool storeRotations)
		: base(originalNode)
	{
		Gclass1452_0 = customBlendTree;
		Bool_1 = storeRotations;
		base.IsLooping = originalNode.IsLooping;
		String_0 = customBlendTree.Name;
		Float_1 = new float[Gclass1452_0.ChildrenCount];
		Float_2 = new float[Gclass1452_0.ChildrenCount];
		ParameterSettings_0 = blendParamsKeeper.GetParameter(Gclass1452_0.ParameterXHash);
		if (Gclass1452_0.Type == GClass1452.EBlendType.FreeformCartesian2D || Gclass1452_0.Type == GClass1452.EBlendType.FreeformDirectional2D || Gclass1452_0.Type == GClass1452.EBlendType.SimpleDirectional2D)
		{
			ParameterSettings_1 = blendParamsKeeper.GetParameter(Gclass1452_0.ParameterYHash);
		}
	}

	public override void ChildrensInitialized()
	{
		Gclass1452_0.InitChilds(Childs);
	}

	public override Vector3 ComputeRootPosition(float deltaTime, float stateNormalizedTime, GInterface134 allParameters)
	{
		if (Gclass1452_0.Type != GClass1452.EBlendType.Simple1D && Gclass1452_0.Type != GClass1452.EBlendType.Direct)
		{
			method_1(allParameters);
			return Gclass1452_0.ComputeRootPosition(deltaTime, stateNormalizedTime, allParameters, Float_1);
		}
		return Gclass1452_0.ComputeRootPosition(deltaTime, stateNormalizedTime, allParameters);
	}

	public override Quaternion ComputeDeltaRotation(float deltaTime, GInterface134 allParameters)
	{
		if (!Bool_1)
		{
			return Quaternion.identity;
		}
		if (Gclass1452_0.Type != GClass1452.EBlendType.Simple1D && Gclass1452_0.Type != GClass1452.EBlendType.Direct)
		{
			method_1(allParameters);
			return Gclass1452_0.ComputeDeltaRotation(deltaTime, allParameters, Float_1);
		}
		return Gclass1452_0.ComputeDeltaRotation(deltaTime, allParameters);
	}

	public override float ComputeClipTime(GInterface134 allParameters)
	{
		if (Gclass1452_0.Type != GClass1452.EBlendType.Simple1D && Gclass1452_0.Type != GClass1452.EBlendType.Direct)
		{
			method_1(allParameters);
			return Gclass1452_0.ComputeClipTime(allParameters, Float_1);
		}
		return Gclass1452_0.ComputeClipTime(allParameters);
	}

	public override void ComputeActiveClips(List<GStruct141> nodes, GInterface134 parametersCache)
	{
		if (Gclass1452_0.Type != GClass1452.EBlendType.Simple1D && Gclass1452_0.Type != GClass1452.EBlendType.Direct)
		{
			method_1(parametersCache);
			Gclass1452_0.ComputeActiveNodes(nodes, parametersCache, Float_1);
		}
		else
		{
			Gclass1452_0.ComputeActiveClips(nodes, parametersCache);
		}
	}

	public void method_1(GInterface134 allParameters)
	{
		float parameter = allParameters.GetParameter(Gclass1452_0.ParameterXHash);
		float parameter2 = allParameters.GetParameter(Gclass1452_0.ParameterYHash);
		ParameterSettings_0.GetStep(out var indexA, out var indexB, out var t, parameter);
		ParameterSettings_1.GetStep(out var indexA2, out var indexB2, out var t2, parameter2);
		method_2(indexA, indexA2, indexB2, t2, Float_1);
		method_2(indexB, indexA2, indexB2, t2, Float_2);
		if (t < 0f)
		{
			t = 0f;
		}
		else if (t > 1f)
		{
			t = 1f;
		}
		for (int i = 0; i < Int_4; i++)
		{
			float num = Float_1[i];
			Float_1[i] = num + (Float_2[i] - num) * t;
		}
	}

	public void method_2(int stepX, int stepY1, int stepY2, float t, float[] weightsBuffer)
	{
		if (t < 0f)
		{
			t = 0f;
		}
		if (t > 1f)
		{
			t = 1f;
		}
		for (int i = 0; i < Int_4; i++)
		{
			float num = Float_3[stepX][stepY1][i];
			float num2 = Float_3[stepX][stepY2][i];
			weightsBuffer[i] = num + (num2 - num) * t;
		}
	}

	public override void Serialize(BinaryWriter writer)
	{
		base.Serialize(writer);
		Gclass1452_0.Serialize(writer);
		writer.Write(Int_2);
		writer.Write(Int_3);
		writer.Write(Int_4);
		writer.Write(Bool_1);
		for (int i = 0; i < Int_2; i++)
		{
			for (int j = 0; j < Int_3; j++)
			{
				for (int k = 0; k < Int_4; k++)
				{
					writer.Write(Float_3[i][j][k]);
				}
			}
		}
	}

	public override void Deserialize(BinaryReader reader)
	{
		base.Deserialize(reader);
		Gclass1452_0 = new GClass1452();
		Gclass1452_0.Deserialize(reader);
		Int_2 = reader.ReadInt32();
		Int_3 = reader.ReadInt32();
		Int_4 = reader.ReadInt32();
		Bool_1 = reader.ReadBoolean();
		ParameterSettings_0 = Ginterface135_0.GetParameter(Gclass1452_0.ParameterXHash);
		if (Gclass1452_0.Type == GClass1452.EBlendType.FreeformCartesian2D || Gclass1452_0.Type == GClass1452.EBlendType.FreeformDirectional2D || Gclass1452_0.Type == GClass1452.EBlendType.SimpleDirectional2D)
		{
			ParameterSettings_1 = Ginterface135_0.GetParameter(Gclass1452_0.ParameterYHash);
		}
		Float_1 = new float[Int_4];
		Float_2 = new float[Int_4];
		method_0();
		for (int i = 0; i < Int_2; i++)
		{
			for (int j = 0; j < Int_3; j++)
			{
				for (int k = 0; k < Int_4; k++)
				{
					Float_3[i][j][k] = reader.ReadSingle();
				}
			}
		}
	}
}
