using System;

public class GClass2090 : GClass2089
{
	[NonSerialized]
	public int Int_4;

	public override int MagazineCount => MagazineItemClass.ObservedMagazineCount;

	public override void InitializeVisualOnStart()
	{
		Int_4 = MagazineItemClass.Count;
		MagazineItemClass.ObservedMagazineCount = Int_4;
		base.InitializeVisualOnStart();
	}

	public override int GetBulletPositionForSpawnInBelt()
	{
		return MagazineItemClass.ObservedMagazineCount - 1;
	}
}
