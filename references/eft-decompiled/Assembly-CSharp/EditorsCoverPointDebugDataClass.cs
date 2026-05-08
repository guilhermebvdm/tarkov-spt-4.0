using System.Collections.Generic;
using UnityEngine;

public class EditorsCoverPointDebugDataClass
{
	public List<CustomNavigationPoint> NearGroups = new List<CustomNavigationPoint>();

	public List<CustomNavigationPoint> DeleteOnHide = new List<CustomNavigationPoint>();

	public List<CustomNavigationPoint> DeletOnCantShoot = new List<CustomNavigationPoint>();

	public List<CustomNavigationPoint> DeletOnBehind = new List<CustomNavigationPoint>();

	public List<CustomNavigationPoint> Good = new List<CustomNavigationPoint>();

	public List<CustomNavigationPoint> GoodWithShoot = new List<CustomNavigationPoint>();

	public CustomNavigationPoint Final;

	public string FinalString;

	public ShootPointClass _pointToShoot;

	public Vector3 StartPoint;

	public Vector3 BotPos;

	public EditorsCoverPointDebugDataClass(ShootPointClass pointToShoot, Vector3 startPoint, Vector3 botPos)
	{
		BotPos = botPos;
		_pointToShoot = pointToShoot;
		StartPoint = startPoint;
	}

	public void DebugAll()
	{
		DrawBehind();
		DrawHide();
		DrawShoot();
		DrawGood();
	}

	public void DebugHide()
	{
		DrawHide();
		DrawShoot();
		DrawGood();
	}

	public void DebugShoot()
	{
		DrawShoot();
		DrawGood();
	}

	public void DebugAllChecked()
	{
		Debug.LogError($"AllChecked:{NearGroups.Count}");
		foreach (CustomNavigationPoint nearGroup in NearGroups)
		{
			Debug.DrawLine(nearGroup.BasePosition + Vector3.up * 1f, StartPoint, Color.white, 5f);
		}
	}

	public void DrawGood()
	{
		Debug.LogError($"Good:{Good.Count}  GoodWithShoot:{GoodWithShoot.Count}  ");
		if (Final != null)
		{
			Debug.LogError($"Final id:{Final.Id} CoveringWeight:{Final.CoveringWeight}   CanIShootToEnemy:{Final.CanIShootToEnemy}");
			Debug.LogError(FinalString);
		}
		foreach (CustomNavigationPoint item in Good)
		{
			Debug.DrawLine(item.BasePosition + Vector3.up * 1f, StartPoint, Color.green, 5f);
		}
	}

	public void DrawHide()
	{
		Debug.LogError($"DrawHide:{DeleteOnHide.Count}");
		foreach (CustomNavigationPoint item in DeleteOnHide)
		{
			Debug.DrawLine(item.BasePosition + Vector3.up * 1f, StartPoint, Color.magenta, 5f);
		}
	}

	public void DrawShoot()
	{
		Debug.LogError($"DrawShoot:{DeletOnCantShoot.Count}");
		foreach (CustomNavigationPoint item in DeletOnCantShoot)
		{
			Debug.DrawLine(item.BasePosition + Vector3.up * 1f, StartPoint, Color.red, 5f);
			if (_pointToShoot != null)
			{
				Debug.DrawLine(item.FirePosition, _pointToShoot.Point, Color.red, 5f);
			}
		}
	}

	public void DrawBehind()
	{
		Debug.LogError($"DrawBehind:{DeletOnBehind.Count}");
		foreach (CustomNavigationPoint item in DeletOnBehind)
		{
			Debug.DrawLine(item.BasePosition + Vector3.up * 1f, StartPoint, Color.blue, 5f);
		}
	}

	public void Dispose()
	{
		DeleteOnHide.Clear();
		DeletOnCantShoot.Clear();
		DeletOnBehind.Clear();
		Good.Clear();
		GoodWithShoot.Clear();
	}
}
