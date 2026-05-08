using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using AnimationSystem.RootMotionTable;
using UnityEngine;

public abstract class GClass1448 : GInterface133
{
	public enum EComputedNodeType : byte
	{
		None,
		Clip,
		Tree
	}

	public List<GClass1448> Childs = new List<GClass1448>();

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	public GInterface133 Ginterface133_0;

	[NonSerialized]
	public GInterface135 Ginterface135_0;

	[NonSerialized]
	public string String_0 = string.Empty;

	public string Name => String_0;

	public Vector3 PosMotion => Vector3.zero;

	public Quaternion PosRotation => Quaternion.identity;

	public virtual int ParameterXHash
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public virtual int ParameterYHash
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
	}

	public float MaxClipTime
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
	}

	public bool IsLooping
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public virtual EComputedNodeType NodeType => EComputedNodeType.None;

	public static void ProcessParameterRec(GClass1448 node, Action<int> paramProcessor)
	{
		if (node.ParameterXHash != 0)
		{
			paramProcessor(node.ParameterXHash);
		}
		if (node.ParameterYHash != 0)
		{
			paramProcessor(node.ParameterYHash);
		}
		for (int i = 0; i < node.Childs.Count; i++)
		{
			ProcessParameterRec(node.Childs[i], paramProcessor);
		}
	}

	public static Vector3 ComputeRootMotion(Animator animator, float deltaTime, float stateNormalizedTime, GInterface134 allParameters, Dictionary<int, GClass1448> _cachedNodes)
	{
		bool flag = animator.IsInTransition(0);
		int fullPathHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
		GClass1448 gClass = (_cachedNodes.ContainsKey(fullPathHash) ? _cachedNodes[fullPathHash] : null);
		if (gClass == null)
		{
			return Vector3.zero;
		}
		Vector3 vector = gClass.ComputeRootPosition(deltaTime, stateNormalizedTime, allParameters);
		if (!flag)
		{
			return animator.targetRotation * vector;
		}
		int fullPathHash2 = animator.GetNextAnimatorStateInfo(0).fullPathHash;
		AnimatorTransitionInfo animatorTransitionInfo = animator.GetAnimatorTransitionInfo(0);
		Vector3 b = ((!flag || !_cachedNodes.ContainsKey(fullPathHash2)) ? null : _cachedNodes[fullPathHash2]).ComputeRootPosition(deltaTime, stateNormalizedTime, allParameters);
		return animator.targetRotation * Vector3.Lerp(vector, b, animatorTransitionInfo.normalizedTime);
	}

	public GClass1448(GInterface135 blendParamsKeeper)
	{
		Ginterface135_0 = blendParamsKeeper;
	}

	public GClass1448(GInterface133 originalNode)
	{
		Ginterface133_0 = originalNode;
	}

	public virtual void StoreAllVariants(RootMotionBlendTable rootMotionTable)
	{
	}

	public virtual void ChildrensInitialized()
	{
	}

	public virtual void Serialize(BinaryWriter writer)
	{
		writer.Write(String_0);
		writer.Write(IsLooping);
	}

	public virtual void Deserialize(BinaryReader reader)
	{
		String_0 = reader.ReadString();
		IsLooping = reader.ReadBoolean();
	}

	public virtual Vector3 ComputeRootPosition(float deltaTime, float stateNormalizedTime, GInterface134 parametersCache)
	{
		throw new NotImplementedException();
	}

	public virtual Quaternion ComputeDeltaRotation(float deltaTime, GInterface134 parametersCache)
	{
		throw new NotImplementedException();
	}

	public virtual float ComputeClipTime(GInterface134 parametersCache)
	{
		throw new NotImplementedException();
	}

	public virtual void ComputeActiveClips(List<GStruct141> nodes, GInterface134 parametersCache)
	{
		throw new NotImplementedException();
	}
}
