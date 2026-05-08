using EFT.Ballistics;
using UnityEngine;

public class BaseBallistic : MonoBehaviour
{
	public enum ESurfaceSound
	{
		Concrete,
		Metal,
		Wood,
		Soil,
		Grass,
		Asphalt,
		Glass,
		Gravel,
		MetalThin,
		Puddle,
		Slate,
		Tile,
		WoodThick,
		Swamp,
		Garbage,
		GarbageMetal,
		Plastic,
		WholeGlass,
		WoodThin,
		Snow
	}

	[HideInInspector]
	public ESurfaceSound SurfaceSound;

	public virtual ESurfaceSound GetSurfaceSound(Vector3 position)
	{
		return SurfaceSound;
	}

	public virtual BallisticCollider Get(Vector3 pos)
	{
		return null;
	}

	public void Associate(MaterialType typeOfMaterial)
	{
		switch (typeOfMaterial)
		{
		case MaterialType.Asphalt:
			SurfaceSound = ESurfaceSound.Asphalt;
			break;
		case MaterialType.GarbageMetal:
			SurfaceSound = ESurfaceSound.GarbageMetal;
			break;
		case MaterialType.Cardboard:
		case MaterialType.GarbagePaper:
			SurfaceSound = ESurfaceSound.Wood;
			break;
		case MaterialType.Glass:
		case MaterialType.GlassShattered:
			SurfaceSound = ESurfaceSound.Glass;
			break;
		case MaterialType.Gravel:
			SurfaceSound = ESurfaceSound.Gravel;
			break;
		case MaterialType.MetalThin:
			SurfaceSound = ESurfaceSound.MetalThin;
			break;
		case MaterialType.Chainfence:
		case MaterialType.Grate:
		case MaterialType.MetalThick:
			SurfaceSound = ESurfaceSound.Metal;
			break;
		case MaterialType.Pebbles:
			SurfaceSound = ESurfaceSound.Slate;
			break;
		case MaterialType.Plastic:
			SurfaceSound = ESurfaceSound.Plastic;
			break;
		case MaterialType.Concrete:
		case MaterialType.Stone:
			SurfaceSound = ESurfaceSound.Concrete;
			break;
		case MaterialType.Mud:
		case MaterialType.Soil:
		case MaterialType.SoilForest:
			SurfaceSound = ESurfaceSound.Soil;
			break;
		case MaterialType.Tile:
			SurfaceSound = ESurfaceSound.Tile;
			break;
		case MaterialType.Water:
		case MaterialType.WaterPuddle:
			SurfaceSound = ESurfaceSound.Puddle;
			break;
		case MaterialType.WoodThin:
			SurfaceSound = ESurfaceSound.WoodThin;
			break;
		case MaterialType.WoodThick:
			SurfaceSound = ESurfaceSound.WoodThick;
			break;
		case MaterialType.Swamp:
			SurfaceSound = ESurfaceSound.Swamp;
			break;
		case MaterialType.Body:
		case MaterialType.Fabric:
		case MaterialType.GenericSoft:
		case MaterialType.Tyre:
		case MaterialType.Rubber:
		case MaterialType.GenericHard:
		case MaterialType.BodyArmor:
		case MaterialType.Helmet:
		case MaterialType.GlassVisor:
		case MaterialType.HelmetRicochet:
		case MaterialType.MetalNoDecal:
			break;
		case MaterialType.GrassHigh:
		case MaterialType.GrassLow:
		case MaterialType.Snow:
			SurfaceSound = ESurfaceSound.Grass;
			break;
		}
	}

	public virtual void TakeSettingsFrom(BaseBallistic collider)
	{
		SurfaceSound = collider.SurfaceSound;
	}
}
