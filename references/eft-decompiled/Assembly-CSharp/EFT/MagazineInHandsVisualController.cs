using EFT.AssetsManager;

namespace EFT;

public class MagazineInHandsVisualController : WeaponModPoolObject
{
	private GClass2088 gclass2088_0;

	public void InitializeMagazine(GClass2088 magazineInHandsVisual, MagazineItemClass magazine, bool isAnimated = false)
	{
		gclass2088_0 = magazineInHandsVisual;
		gclass2088_0.Init(base.gameObject, magazine, isAnimated);
	}

	public override void ReturnToPool()
	{
		gclass2088_0?.OnReturnToPool();
		base.ReturnToPool();
	}

	public void Update()
	{
		gclass2088_0?.Update();
	}
}
