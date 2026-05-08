using System.Collections.Generic;
using UnityEngine;

public class AIMinesPositionsHolder : MonoBehaviour
{
	public List<AIMinePoint> Mines;

	private List<AIMinePoint> list_0 = new List<AIMinePoint>();

	private List<AIMinePoint> list_1 = new List<AIMinePoint>();

	public List<AIMinePoint> MinesSimple => list_0;

	public List<AIMinePoint> MinesForRewarm => list_1;

	public void InGameRestore(AICoversData data)
	{
		list_0 = new List<AIMinePoint>();
		list_1 = new List<AIMinePoint>();
		if (Mines == null)
		{
			return;
		}
		foreach (AIMinePoint mine in Mines)
		{
			mine.Restore(data.AICorePointsHolder);
			if (mine.IsForPrewarm)
			{
				list_1.Add(mine);
			}
			else
			{
				list_0.Add(mine);
			}
		}
	}

	public void SetMines(List<AIMinePoint> minePoints)
	{
		Mines = minePoints;
	}
}
