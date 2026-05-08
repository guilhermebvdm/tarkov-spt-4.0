using System;
using System.Collections.Generic;
using System.IO;
using AnimationSystem.RootMotionTable;
using UnityEngine;

public class GClass1449 : GClass1448
{
	[NonSerialized]
	public const int Int_2 = 7;

	[NonSerialized]
	public Vector3 Vector3_0 = Vector3.zero;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public GStruct141 Gstruct141_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public AnimationCurve[] AnimationCurve_0;

	public override EComputedNodeType NodeType => EComputedNodeType.Clip;

	public GClass1449(GInterface135 blendParametersKeeper)
		: base(blendParametersKeeper)
	{
	}

	public GClass1449(GInterface133 originalNode, int clipIndex, float timeScale, bool useCurvesForDP, AnimationCurve[] rootCurves)
		: base(originalNode)
	{
		Int_3 = clipIndex;
		Float_1 = timeScale;
		Bool_1 = useCurvesForDP;
		Bool_2 = rootCurves.Length == 7;
		if (Bool_2)
		{
			if (Bool_1)
			{
				AnimationCurve_0 = new AnimationCurve[7]
				{
					rootCurves[0],
					rootCurves[1],
					rootCurves[2],
					rootCurves[3],
					rootCurves[4],
					rootCurves[5],
					rootCurves[6]
				};
			}
			else
			{
				AnimationCurve_0 = new AnimationCurve[4]
				{
					rootCurves[3],
					rootCurves[4],
					rootCurves[5],
					rootCurves[6]
				};
			}
		}
		else if (Bool_1)
		{
			AnimationCurve_0 = new AnimationCurve[3]
			{
				rootCurves[0],
				rootCurves[1],
				rootCurves[2]
			};
		}
	}

	public override void StoreAllVariants(RootMotionBlendTable rootMotionTable)
	{
		base.StoreAllVariants(rootMotionTable);
		String_0 = Ginterface133_0.Name;
		Vector3_0 = Ginterface133_0.PosMotion / Ginterface133_0.MaxClipTime;
		Float_2 = Ginterface133_0.MaxClipTime;
		base.IsLooping = Ginterface133_0.IsLooping;
	}

	public override Vector3 ComputeRootPosition(float deltaTime, float stateNormalizedTime, GInterface134 parametersCache)
	{
		if (Bool_1)
		{
			if (!base.IsLooping && !(stateNormalizedTime < 1f))
			{
				return Vector3.zero;
			}
			if (stateNormalizedTime >= 1f)
			{
				stateNormalizedTime -= Mathf.Floor(stateNormalizedTime);
			}
			float time = stateNormalizedTime * Float_2;
			float num = stateNormalizedTime * Float_2 - Time.deltaTime;
			float num2 = AnimationCurve_0[0].Evaluate(time);
			float num3 = AnimationCurve_0[1].Evaluate(time);
			float num4 = AnimationCurve_0[2].Evaluate(time);
			if (num < 0f)
			{
				return new Vector3(num2, num3, num4);
			}
			float num5 = AnimationCurve_0[0].Evaluate(num);
			float num6 = AnimationCurve_0[1].Evaluate(num);
			float num7 = AnimationCurve_0[2].Evaluate(num);
			return new Vector3(num2 - num5, num3 - num6, num4 - num7);
		}
		if (!base.IsLooping && !(stateNormalizedTime < 1f))
		{
			return Vector3.zero;
		}
		return Vector3_0 * deltaTime;
	}

	public override Quaternion ComputeDeltaRotation(float deltaTime, GInterface134 parametersCache)
	{
		if (!Bool_2)
		{
			return Quaternion.identity;
		}
		if (Bool_1)
		{
			float x = AnimationCurve_0[3].Evaluate(deltaTime);
			float y = AnimationCurve_0[4].Evaluate(deltaTime);
			float z = AnimationCurve_0[5].Evaluate(deltaTime);
			float w = AnimationCurve_0[6].Evaluate(deltaTime);
			return new Quaternion(x, y, z, w);
		}
		float x2 = AnimationCurve_0[0].Evaluate(deltaTime);
		float y2 = AnimationCurve_0[1].Evaluate(deltaTime);
		float z2 = AnimationCurve_0[2].Evaluate(deltaTime);
		float w2 = AnimationCurve_0[3].Evaluate(deltaTime);
		return new Quaternion(x2, y2, z2, w2);
	}

	public override float ComputeClipTime(GInterface134 parametersCache)
	{
		return Float_2;
	}

	public override void ComputeActiveClips(List<GStruct141> nodes, GInterface134 parametersCache)
	{
		Gstruct141_0.Weight = 1f;
		Gstruct141_0.RawWeight = 1f;
		Gstruct141_0.TimeScale = Float_1;
		nodes.Add(Gstruct141_0);
	}

	public override void Serialize(BinaryWriter writer)
	{
		base.Serialize(writer);
		writer.Write(String_0);
		writer.Write(Float_1);
		writer.Write(Float_2);
		writer.Write(Int_3);
		writer.Write(Vector3_0.x);
		writer.Write(Vector3_0.y);
		writer.Write(Vector3_0.z);
		writer.Write(Bool_2);
		writer.Write(Bool_1);
		if (Bool_2 || Bool_1)
		{
			writer.Write((short)AnimationCurve_0.Length);
			for (int i = 0; i < AnimationCurve_0.Length; i++)
			{
				method_0(writer, AnimationCurve_0[i]);
			}
		}
	}

	public override void Deserialize(BinaryReader reader)
	{
		base.Deserialize(reader);
		String_0 = reader.ReadString();
		Float_1 = reader.ReadSingle();
		Float_2 = reader.ReadSingle();
		Int_3 = reader.ReadInt32();
		Vector3_0 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		Bool_2 = reader.ReadBoolean();
		Bool_1 = reader.ReadBoolean();
		if (Bool_2 || Bool_1)
		{
			short num = reader.ReadInt16();
			AnimationCurve_0 = new AnimationCurve[num];
			for (int i = 0; i < num; i++)
			{
				method_1(reader, out AnimationCurve_0[i]);
			}
		}
		Gstruct141_0 = new GStruct141
		{
			ClipIndex = Int_3,
			Weight = 1f,
			RawWeight = 1f,
			TimeScale = Float_1
		};
	}

	public void method_0(BinaryWriter writer, AnimationCurve curve)
	{
		writer.Write(curve.length);
		writer.Write((short)curve.postWrapMode);
		writer.Write((short)curve.preWrapMode);
		Keyframe[] keys = curve.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			Keyframe keyframe = keys[i];
			writer.Write(keyframe.time);
			writer.Write(keyframe.value);
			writer.Write(keyframe.inTangent);
			writer.Write(keyframe.outTangent);
			writer.Write(keyframe.inWeight);
			writer.Write(keyframe.outWeight);
			writer.Write((short)keyframe.weightedMode);
		}
	}

	public void method_1(BinaryReader reader, out AnimationCurve curve)
	{
		int num = reader.ReadInt32();
		Keyframe[] array = new Keyframe[num];
		WrapMode postWrapMode = (WrapMode)reader.ReadInt16();
		WrapMode preWrapMode = (WrapMode)reader.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			Keyframe keyframe = new Keyframe(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			keyframe.weightedMode = (WeightedMode)reader.ReadInt16();
			array[i] = keyframe;
		}
		curve = new AnimationCurve(array)
		{
			postWrapMode = postWrapMode,
			preWrapMode = preWrapMode
		};
	}
}
