using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using Nexus.BundleLoader;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using VisceralCombat.Combined.Patches;
using VisceralCombat.Dismemberment.Classes;

namespace VisceralCombat.Dismemberment.Patches;

public class PlayerDetonationPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Grenade).GetMethod("Explosion", BindingFlags.Static | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(IExplosiveItem grenadeItem, Vector3 grenadePosition)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(grenadeItem.MinExplosionDistance, grenadeItem.MaxExplosionDistance);
		RaycastHit[] array = Physics.SphereCastAll(new Ray(grenadePosition, Vector3.up), num, grenadeItem.MaxExplosionDistance, LayerMasksDataAbstractClass.HitMask);
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit val = array2[i];
			Rigidbody component = ((Component)val.collider).GetComponent<Rigidbody>();
			if ((Object)(object)component != (Object)null)
			{
				float num2 = Vector3.Distance(grenadePosition, ((Component)val.collider).transform.position);
				if (num2 < 2f && grenadeItem.GetStrength >= 300f)
				{
					Player componentInParentRecursive = Utils.GetComponentInParentRecursive<Player>(((Component)component).gameObject);
					componentInParentRecursive.KillMe((EBodyPartColliderType)0, 10000000f);
					KillPatch.SpawnOldVolumetricBlood(componentInParentRecursive.PlayerBones.Head.Original, Vector3.zero, 3f);
					FuckingCrazyGoreyExplosion(((Component)componentInParentRecursive.PlayerBones.Head.Original).gameObject, 5f, 5f);
					Object.Destroy((Object)(object)componentInParentRecursive);
				}
			}
		}
	}

	public static void FuckingCrazyGoreyExplosion(GameObject go, float time, float scale)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = BundleLoaderPlugin.Instance.GetAssetBundle("blood_3dfx").LoadAllAssets<GameObject>()[20];
		GameObject ExtremeGoreObject = Object.Instantiate<GameObject>(val);
		ExtremeGoreObject.transform.parent = go.transform;
		ExtremeGoreObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		ExtremeGoreObject.transform.position = go.transform.position;
		ExtremeGoreObject.GetComponent<ParticleSystem>().scalingMode = (ParticleSystemScalingMode)1;
		ParticleSystem[] componentsInChildren = ExtremeGoreObject.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem val2 in array)
		{
			val2.loop = false;
			Traverse.Create((object)val2.main).Property<float>("duration", (object[])null).Value = time;
			Traverse.Create((object)val2.collision).Property<bool>("sendCollisionMessages", (object[])null).Value = true;
			val2.Play();
			val2.scalingMode = (ParticleSystemScalingMode)0;
		}
		GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, time + 1f, (Action)delegate
		{
			Object.Destroy((Object)(object)ExtremeGoreObject);
		});
	}
}
