using System;
using UnityEngine;

[Serializable]
public class ShotsGroupSettings
{
	public int StartShotIndex;

	public int EndShotIndex;

	[Tooltip("Разброс силы вращения, который добавляется к общим расчетам")]
	public Vector2 ShotRecoilRotationStrength;

	[Tooltip("Разброс смещения позиции, который добавляется к общим расчетам")]
	public Vector2 ShotRecoilPositionStrength;

	[Tooltip("Разброс угла вращения, который добавляется к общим расчетам")]
	public Vector2 ShotRecoilRadianRange;

	public bool IsShotIndexInRange(int shotIndex)
	{
		if (shotIndex >= StartShotIndex && shotIndex <= EndShotIndex)
		{
			return true;
		}
		return false;
	}
}
