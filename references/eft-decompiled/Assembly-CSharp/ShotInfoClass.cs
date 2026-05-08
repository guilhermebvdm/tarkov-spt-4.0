using EFT;
using EFT.Ballistics;

public class ShotInfoClass
{
	public bool Penetrated;

	public MongoID? HitArmorID;

	public EPointOfView PoV;

	public MaterialType Material;

	public bool Silent;
}
