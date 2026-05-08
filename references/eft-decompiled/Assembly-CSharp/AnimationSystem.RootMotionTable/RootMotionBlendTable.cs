using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AnimationSystem.RootMotionTable;

[CreateAssetMenu]
public class RootMotionBlendTable : ScriptableObject, GInterface135
{
	[Serializable]
	public class ParameterSettings
	{
		public string ParamName;

		public float MinValue;

		public float MaxValue;

		public float StepDensity;

		[NonSerialized]
		public int CachedTotalSteps;

		public float Range => Mathf.Abs(MaxValue - MinValue);

		public int TotalSteps
		{
			get
			{
				CachedTotalSteps = Mathf.CeilToInt(Range / StepDensity) + 1;
				return CachedTotalSteps;
			}
		}

		public float GetValue(int step)
		{
			return MinValue + (float)step * StepDensity;
		}

		public void GetStep(out int indexA, out int indexB, out float t, float value)
		{
			indexA = 0;
			indexB = 0;
			t = 0f;
			if (value < MinValue)
			{
				value = MinValue;
			}
			else if (value > MaxValue)
			{
				value = MaxValue;
			}
			int totalSteps = TotalSteps;
			if (totalSteps != 1)
			{
				indexA = (int)Math.Floor((value - MinValue) / StepDensity);
				if (indexA >= totalSteps)
				{
					indexA = totalSteps - 1;
					indexB = totalSteps - 1;
				}
				t = (value - (MinValue + (float)indexA * StepDensity)) / StepDensity;
				indexB = ((indexA + 1 < totalSteps) ? (indexA + 1) : indexA);
			}
		}
	}

	[Serializable]
	public struct ParameterRelatedCurve
	{
		public AnimationCurve curve;

		public int parameterHash;
	}

	public TextAsset GeneratedData;

	public string AnimatorPath;

	public string ClipsKeeperPath;

	public string RootBoneName;

	public List<ParameterSettings> Parameters;

	public List<string> StoreRotationStatesWhitelist;

	public List<string> UsingCurvesForDPClipNames;

	[NonSerialized]
	public Dictionary<int, GClass1448> _loadedNodes;

	public void LoadNodes()
	{
		if (_loadedNodes != null)
		{
			return;
		}
		_loadedNodes = new Dictionary<int, GClass1448>();
		using (MemoryStream input = new MemoryStream(GeneratedData.bytes))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
			{
				binaryReader.ReadString();
				int key = binaryReader.ReadInt32();
				GClass1448 value = GClass1451.DeserializeNode(binaryReader, this);
				_loadedNodes.Add(key, value);
			}
		}
		UnityEngine.Object.Destroy(GeneratedData);
		GeneratedData = null;
	}

	public bool IsValidStateToStoreRotation(string stateName)
	{
		foreach (string item in StoreRotationStatesWhitelist)
		{
			if (string.Equals(stateName, item))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsValidClipForCurvesDP(string clipName)
	{
		foreach (string usingCurvesForDPClipName in UsingCurvesForDPClipNames)
		{
			if (string.Equals(usingCurvesForDPClipName, clipName))
			{
				return true;
			}
		}
		return false;
	}

	public ParameterSettings GetParameter(int paramHash)
	{
		int num = 0;
		while (true)
		{
			if (num < Parameters.Count)
			{
				if (Animator.StringToHash(Parameters[num].ParamName) == paramHash)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return Parameters[num];
	}

	public ParameterSettings GetParameter(string paramName)
	{
		int num = 0;
		while (true)
		{
			if (num < Parameters.Count)
			{
				if (Parameters[num].ParamName.Equals(paramName))
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return Parameters[num];
	}
}
