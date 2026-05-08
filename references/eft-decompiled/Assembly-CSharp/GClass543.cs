using System;
using System.Globalization;
using System.Runtime.CompilerServices;

public class GClass543
{
	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool CanShoot;

	public float VisibilityLevel;

	[NonSerialized]
	public EEnemyPartVisibleType EenemyPartVisibleType_0;

	public bool Visible
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

	public EEnemyPartVisibleType VisibleType
	{
		get
		{
			return EenemyPartVisibleType_0;
		}
		set
		{
			EenemyPartVisibleType_0 = value;
			Visible = EenemyPartVisibleType_0 != EEnemyPartVisibleType.NotVisible;
		}
	}

	public GClass543(EEnemyPartVisibleType visibleType, bool canShoot, float visibilityLevel)
	{
		VisibleType = visibleType;
		CanShoot = canShoot;
		VisibilityLevel = visibilityLevel;
	}

	public override string ToString()
	{
		return string.Format("vis: {0,-5} | type: {1} | shoot: {2,-5} | lvl: {3,-6}", Visible, GClass856.GetSymbol(VisibleType), CanShoot, VisibilityLevel.ToString("F2", CultureInfo.InvariantCulture));
	}
}
