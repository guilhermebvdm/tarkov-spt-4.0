using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[Serializable]
public struct BipedRagdollReferences
{
	public Transform root;

	public Transform hips;

	public Transform spine;

	public Transform chest;

	public Transform head;

	public Transform leftUpperLeg;

	public Transform leftLowerLeg;

	public Transform leftFoot;

	public Transform rightUpperLeg;

	public Transform rightLowerLeg;

	public Transform rightFoot;

	public Transform leftUpperArm;

	public Transform leftLowerArm;

	public Transform leftHand;

	public Transform rightUpperArm;

	public Transform rightLowerArm;

	public Transform rightHand;

	public bool IsValid(ref string msg)
	{
		if ((Object)(object)root == (Object)null || (Object)(object)hips == (Object)null || (Object)(object)head == (Object)null || (Object)(object)leftUpperArm == (Object)null || (Object)(object)leftLowerArm == (Object)null || (Object)(object)leftHand == (Object)null || (Object)(object)rightUpperArm == (Object)null || (Object)(object)rightLowerArm == (Object)null || (Object)(object)rightHand == (Object)null || (Object)(object)leftUpperLeg == (Object)null || (Object)(object)leftLowerLeg == (Object)null || (Object)(object)leftFoot == (Object)null || (Object)(object)rightUpperLeg == (Object)null || (Object)(object)rightLowerLeg == (Object)null || (Object)(object)rightFoot == (Object)null)
		{
			msg = "Invalid References, one or more Transforms missing.";
			return false;
		}
		Transform[] array = (Transform[])(object)new Transform[15]
		{
			root, hips, head, leftUpperArm, leftLowerArm, leftHand, rightUpperArm, rightLowerArm, rightHand, leftUpperLeg,
			leftLowerLeg, leftFoot, rightUpperLeg, rightLowerLeg, rightFoot
		};
		for (int i = 1; i < array.Length; i++)
		{
			if (!IsChildRecursive(array[i], root))
			{
				msg = "Invalid References, " + ((Object)array[i]).name + " is not in the Root's hierarchy.";
				return false;
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			for (int k = 0; k < array.Length; k++)
			{
				if (j != k && (Object)(object)array[j] == (Object)(object)array[k])
				{
					msg = "Invalid References, " + ((Object)array[j]).name + " is represented more than once.";
					return false;
				}
			}
		}
		return true;
	}

	private bool IsChildRecursive(Transform t, Transform parent)
	{
		if ((Object)(object)t.parent == (Object)(object)parent)
		{
			return true;
		}
		if ((Object)(object)t.parent != (Object)null)
		{
			return IsChildRecursive(t.parent, parent);
		}
		return false;
	}

	public bool IsEmpty(bool considerRoot)
	{
		if (considerRoot && (Object)(object)root != (Object)null)
		{
			return false;
		}
		if ((Object)(object)hips != (Object)null || (Object)(object)head != (Object)null || (Object)(object)spine != (Object)null || (Object)(object)chest != (Object)null || (Object)(object)leftUpperArm != (Object)null || (Object)(object)leftLowerArm != (Object)null || (Object)(object)leftHand != (Object)null || (Object)(object)rightUpperArm != (Object)null || (Object)(object)rightLowerArm != (Object)null || (Object)(object)rightHand != (Object)null || (Object)(object)leftUpperLeg != (Object)null || (Object)(object)leftLowerLeg != (Object)null || (Object)(object)leftFoot != (Object)null || (Object)(object)rightUpperLeg != (Object)null || (Object)(object)rightLowerLeg != (Object)null || (Object)(object)rightFoot != (Object)null)
		{
			return false;
		}
		return true;
	}

	public bool Contains(Transform t, bool ignoreRoot = false)
	{
		if (!ignoreRoot && (Object)(object)root == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)hips == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)spine == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)chest == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftUpperLeg == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftLowerLeg == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftFoot == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightUpperLeg == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightLowerLeg == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightFoot == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftUpperArm == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftLowerArm == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftHand == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightUpperArm == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightLowerArm == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightHand == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)head == (Object)(object)t)
		{
			return true;
		}
		return false;
	}

	public Transform[] GetRagdollTransforms()
	{
		return (Transform[])(object)new Transform[16]
		{
			hips, spine, chest, head, leftUpperArm, leftLowerArm, leftHand, rightUpperArm, rightLowerArm, rightHand,
			leftUpperLeg, leftLowerLeg, leftFoot, rightUpperLeg, rightLowerLeg, rightFoot
		};
	}

	public static BipedRagdollReferences FromAvatar(Animator animator)
	{
		BipedRagdollReferences result = default(BipedRagdollReferences);
		if (!animator.isHuman)
		{
			return result;
		}
		result.root = ((Component)animator).transform;
		result.hips = animator.GetBoneTransform((HumanBodyBones)0);
		result.spine = animator.GetBoneTransform((HumanBodyBones)7);
		result.chest = animator.GetBoneTransform((HumanBodyBones)8);
		result.head = animator.GetBoneTransform((HumanBodyBones)10);
		result.leftUpperArm = animator.GetBoneTransform((HumanBodyBones)13);
		result.leftLowerArm = animator.GetBoneTransform((HumanBodyBones)15);
		result.leftHand = animator.GetBoneTransform((HumanBodyBones)17);
		result.rightUpperArm = animator.GetBoneTransform((HumanBodyBones)14);
		result.rightLowerArm = animator.GetBoneTransform((HumanBodyBones)16);
		result.rightHand = animator.GetBoneTransform((HumanBodyBones)18);
		result.leftUpperLeg = animator.GetBoneTransform((HumanBodyBones)1);
		result.leftLowerLeg = animator.GetBoneTransform((HumanBodyBones)3);
		result.leftFoot = animator.GetBoneTransform((HumanBodyBones)5);
		result.rightUpperLeg = animator.GetBoneTransform((HumanBodyBones)2);
		result.rightLowerLeg = animator.GetBoneTransform((HumanBodyBones)4);
		result.rightFoot = animator.GetBoneTransform((HumanBodyBones)6);
		return result;
	}

	public static BipedRagdollReferences FromBipedReferences(BipedReferences biped)
	{
		BipedRagdollReferences result = default(BipedRagdollReferences);
		result.root = biped.root;
		result.hips = biped.pelvis;
		if (biped.spine != null && biped.spine.Length != 0)
		{
			result.spine = biped.spine[0];
			if (biped.spine.Length > 1)
			{
				result.chest = biped.spine[biped.spine.Length - 1];
			}
		}
		result.head = biped.head;
		result.leftUpperArm = biped.leftUpperArm;
		result.leftLowerArm = biped.leftForearm;
		result.leftHand = biped.leftHand;
		result.rightUpperArm = biped.rightUpperArm;
		result.rightLowerArm = biped.rightForearm;
		result.rightHand = biped.rightHand;
		result.leftUpperLeg = biped.leftThigh;
		result.leftLowerLeg = biped.leftCalf;
		result.leftFoot = biped.leftFoot;
		result.rightUpperLeg = biped.rightThigh;
		result.rightLowerLeg = biped.rightCalf;
		result.rightFoot = biped.rightFoot;
		return result;
	}

	public static BipedRagdollReferences BSG_PlayerSetup(PlayerBones pb)
	{
		BipedRagdollReferences result = default(BipedRagdollReferences);
		result.root = ((Component)pb).transform;
		result.hips = ((Component)pb.Pelvis.Original).transform;
		result.spine = ((Component)pb.Spine1).transform;
		result.chest = ((Component)pb.Spine3.Original).transform;
		result.head = ((Component)pb.Head.Original).transform;
		result.leftUpperArm = ((Component)pb.Upperarms[0]).transform;
		result.leftLowerArm = ((Component)pb.Forearms[0]).transform;
		result.leftHand = ((Component)((Component)pb.Forearms[0]).transform.GetChild(0).GetChild(0).GetChild(0)).transform;
		result.rightUpperArm = ((Component)pb.Upperarms[1]).transform;
		result.rightLowerArm = ((Component)pb.Forearms[1]).transform;
		result.rightHand = ((Component)((Component)pb.Forearms[1]).transform.GetChild(0).GetChild(0).GetChild(0)).transform;
		result.leftUpperLeg = ((Component)pb.LeftThigh1.Original).transform;
		result.leftLowerLeg = ((Component)((Component)pb.LeftThigh2.Original).transform.GetChild(0)).transform;
		result.leftFoot = ((Component)((Component)pb.LeftThigh2.Original).transform.GetChild(0).GetChild(0)).transform;
		result.rightUpperLeg = ((Component)pb.RightThigh1.Original).transform;
		result.rightLowerLeg = ((Component)((Component)pb.RightThigh2.Original).transform.GetChild(0)).transform;
		result.rightFoot = ((Component)((Component)pb.RightThigh2.Original).transform.GetChild(0).GetChild(0)).transform;
		return result;
	}
}
