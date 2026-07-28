using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class GrenadeDeadBodiesPatch : ModulePatch
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
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(grenadeItem.MinExplosionDistance, grenadeItem.MaxExplosionDistance);
		RaycastHit[] array = Physics.SphereCastAll(new Ray(grenadePosition, Vector3.up), num, grenadeItem.MaxExplosionDistance, LayerMask.op_Implicit(LayerMasksDataAbstractClass.HitMask));
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit val = array2[i];
			Rigidbody component = ((Component)((RaycastHit)(ref val)).collider).GetComponent<Rigidbody>();
			if ((Object)(object)component != (Object)null)
			{
				component.AddExplosionForce(grenadeItem.GetStrength * 0.5f * VisceralEntry.Instance.GrenadeExplIntensity.Value, grenadePosition, num);
			}
		}
	}
}
