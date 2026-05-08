using System;
using UnityEngine;

[Serializable]
public class CoverPointDefenceInfo
{
	[SerializeField]
	public float _defenceLevel;

	[SerializeField]
	public int distanceCheckSum;

	public float DefenceLevel => _defenceLevel;

	public int DangerCoeff => distanceCheckSum;

	public CoverPointDefenceInfo(int defLevel)
	{
		_defenceLevel = defLevel;
		distanceCheckSum = 999;
	}

	public CoverPointDefenceInfo(Vector3 position)
	{
		float num = (float)GClass369.AllBaseSides.Count * 8f;
		float num2 = 0f;
		float num3 = 1.26f;
		Vector3 headPos = position + Vector3.up * num3;
		distanceCheckSum = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		for (int i = 0; i < GClass369.AllBaseSides.Count; i++)
		{
			Vector3 dir = GClass369.AllBaseSides[i];
			if (!GClass369.TestDir(headPos, dir, 8f, out var outPos) && outPos.HasValue)
			{
				float magnitude = (outPos.Value - position).magnitude;
				num2 += magnitude;
				if (magnitude <= 2f)
				{
					num4++;
				}
				else if (magnitude <= 6f)
				{
					distanceCheckSum++;
					num5++;
				}
				else
				{
					distanceCheckSum += 2;
					num6++;
				}
			}
			else
			{
				num2 += 8f;
				distanceCheckSum += 4;
				num7++;
			}
		}
		_defenceLevel = num - num2;
	}

	public void OverrideData(float defenceLevel, int dangerCoeff)
	{
		_defenceLevel = defenceLevel;
		distanceCheckSum = dangerCoeff;
	}

	public bool IsSafe()
	{
		return distanceCheckSum < 8;
	}

	public void SetDefenceLevel(float deffCoeff)
	{
		_defenceLevel = deffCoeff;
	}
}
