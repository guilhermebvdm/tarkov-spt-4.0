using System.Collections.Generic;
using System.Linq;
using Nexus.BundleLoader;
using UnityEngine;

namespace VisceralCombat.Dismemberment.Classes;

public class EffectContainer : MonoBehaviour
{
	public GameObject lightBleedEffect;

	public GameObject heavyBleedEffect;

	public GameObject squirtEffect1;

	public GameObject squirtEffect2;

	public GameObject brainParticles;

	public List<GameObject> bloodParticles = new List<GameObject>();

	public List<GameObject> goreCaps = new List<GameObject>();

	public List<GameObject> bloodSFX = new List<GameObject>();

	public GameObject limbSquirter;

	public GameObject activeRagdollBase;

	public List<GameObject> blood3dFxEffects = new List<GameObject>();

	public void Start()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		AssetBundle assetBundle = BundleLoaderPlugin.Instance.GetAssetBundle("blood_3dfx");
		AssetBundle assetBundle2 = BundleLoaderPlugin.Instance.GetAssetBundle("bloodfx");
		AssetBundle assetBundle3 = BundleLoaderPlugin.Instance.GetAssetBundle("bloodsfx");
		AssetBundle assetBundle4 = BundleLoaderPlugin.Instance.GetAssetBundle("gorecaps");
		AssetBundle assetBundle5 = BundleLoaderPlugin.Instance.GetAssetBundle("active_ragdoll_base");
		GameObject[] array = assetBundle.LoadAllAssets<GameObject>();
		GameObject[] array2 = assetBundle2.LoadAllAssets<GameObject>();
		GameObject[] source = assetBundle3.LoadAllAssets<GameObject>();
		GameObject[] source2 = assetBundle4.LoadAllAssets<GameObject>();
		GameObject[] array3 = assetBundle5.LoadAllAssets<GameObject>();
		GameObject val = new GameObject("Visceral Combat Effects Container");
		val.transform.parent = ((Component)this).transform;
		val.gameObject.SetActive(false);
		blood3dFxEffects = array.Where((GameObject prefab) => (Object)(object)prefab != (Object)null).ToList();
		if (array.Length > 17 && (Object)(object)array[17] != (Object)null)
		{
			heavyBleedEffect = Object.Instantiate<GameObject>(array[17], val.transform);
		}
		if (array.Length > 18 && (Object)(object)array[18] != (Object)null)
		{
			lightBleedEffect = Object.Instantiate<GameObject>(array[18], val.transform);
		}
		if (array.Length > 10 && (Object)(object)array[10] != (Object)null)
		{
			squirtEffect1 = Object.Instantiate<GameObject>(array[10], val.transform);
		}
		if (array.Length > 11 && (Object)(object)array[11] != (Object)null)
		{
			squirtEffect2 = Object.Instantiate<GameObject>(array[11], val.transform);
		}
		if (array.Length > 9 && (Object)(object)array[9] != (Object)null)
		{
			limbSquirter = Object.Instantiate<GameObject>(array[9], val.transform);
		}
		bloodParticles = (from prefab in array2.Skip(1).Take(17)
			where (Object)(object)prefab != (Object)null
			select prefab).ToList();
		if (array3.Length != 0 && (Object)(object)array3[0] != (Object)null)
		{
			GameObject val2 = Object.Instantiate<GameObject>(array3[0]);
			activeRagdollBase = Object.Instantiate<GameObject>(val2, val.transform);
		}
		bloodSFX = (from prefab in source.Take(3)
			where (Object)(object)prefab != (Object)null
			select prefab).ToList();
		if (array2.Length > 18 && (Object)(object)array2[18] != (Object)null)
		{
			brainParticles = Object.Instantiate<GameObject>(array2[18], val.transform);
		}
		goreCaps = source2.Where((GameObject prefab) => (Object)(object)prefab != (Object)null).ToList();
	}
}
