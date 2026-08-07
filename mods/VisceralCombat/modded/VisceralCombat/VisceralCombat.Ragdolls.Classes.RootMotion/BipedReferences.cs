using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

[Serializable]
public class BipedReferences
{
	public struct AutoDetectParams
	{
		public bool legsParentInSpine;

		public bool includeEyes;

		public static AutoDetectParams Default => new AutoDetectParams(legsParentInSpine: true, includeEyes: true);

		public AutoDetectParams(bool legsParentInSpine, bool includeEyes)
		{
			this.legsParentInSpine = legsParentInSpine;
			this.includeEyes = includeEyes;
		}
	}

	public Transform root;

	public Transform pelvis;

	public Transform leftThigh;

	public Transform leftCalf;

	public Transform leftFoot;

	public Transform rightThigh;

	public Transform rightCalf;

	public Transform rightFoot;

	public Transform leftUpperArm;

	public Transform leftForearm;

	public Transform leftHand;

	public Transform rightUpperArm;

	public Transform rightForearm;

	public Transform rightHand;

	public Transform head;

	public Transform[] spine = (Transform[])(object)new Transform[0];

	public Transform[] eyes = (Transform[])(object)new Transform[0];

	public virtual bool isFilled
	{
		get
		{
			if ((Object)(object)root == (Object)null)
			{
				return false;
			}
			if ((Object)(object)pelvis == (Object)null)
			{
				return false;
			}
			if ((Object)(object)leftThigh == (Object)null || (Object)(object)leftCalf == (Object)null || (Object)(object)leftFoot == (Object)null)
			{
				return false;
			}
			if ((Object)(object)rightThigh == (Object)null || (Object)(object)rightCalf == (Object)null || (Object)(object)rightFoot == (Object)null)
			{
				return false;
			}
			if ((Object)(object)leftUpperArm == (Object)null || (Object)(object)leftForearm == (Object)null || (Object)(object)leftHand == (Object)null)
			{
				return false;
			}
			if ((Object)(object)rightUpperArm == (Object)null || (Object)(object)rightForearm == (Object)null || (Object)(object)rightHand == (Object)null)
			{
				return false;
			}
			Transform[] array = spine;
			foreach (Transform val in array)
			{
				if ((Object)(object)val == (Object)null)
				{
					return false;
				}
			}
			Transform[] array2 = eyes;
			foreach (Transform val2 in array2)
			{
				if ((Object)(object)val2 == (Object)null)
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool isEmpty => IsEmpty(includeRoot: true);

	public virtual bool IsEmpty(bool includeRoot)
	{
		if (includeRoot && (Object)(object)root != (Object)null)
		{
			return false;
		}
		if ((Object)(object)pelvis != (Object)null || (Object)(object)head != (Object)null)
		{
			return false;
		}
		if ((Object)(object)leftThigh != (Object)null || (Object)(object)leftCalf != (Object)null || (Object)(object)leftFoot != (Object)null)
		{
			return false;
		}
		if ((Object)(object)rightThigh != (Object)null || (Object)(object)rightCalf != (Object)null || (Object)(object)rightFoot != (Object)null)
		{
			return false;
		}
		if ((Object)(object)leftUpperArm != (Object)null || (Object)(object)leftForearm != (Object)null || (Object)(object)leftHand != (Object)null)
		{
			return false;
		}
		if ((Object)(object)rightUpperArm != (Object)null || (Object)(object)rightForearm != (Object)null || (Object)(object)rightHand != (Object)null)
		{
			return false;
		}
		Transform[] array = spine;
		foreach (Transform val in array)
		{
			if ((Object)(object)val != (Object)null)
			{
				return false;
			}
		}
		Transform[] array2 = eyes;
		foreach (Transform val2 in array2)
		{
			if ((Object)(object)val2 != (Object)null)
			{
				return false;
			}
		}
		return true;
	}

	public virtual bool Contains(Transform t, bool ignoreRoot = false)
	{
		if (!ignoreRoot && (Object)(object)root == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)pelvis == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftThigh == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftCalf == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)leftFoot == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightThigh == (Object)(object)t)
		{
			return true;
		}
		if ((Object)(object)rightCalf == (Object)(object)t)
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
		if ((Object)(object)leftForearm == (Object)(object)t)
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
		if ((Object)(object)rightForearm == (Object)(object)t)
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
		Transform[] array = spine;
		foreach (Transform val in array)
		{
			if ((Object)(object)val == (Object)(object)t)
			{
				return true;
			}
		}
		Transform[] array2 = eyes;
		foreach (Transform val2 in array2)
		{
			if ((Object)(object)val2 == (Object)(object)t)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AutoDetectReferences(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams)
	{
		if (references == null)
		{
			references = new BipedReferences();
		}
		references.root = root;
		Animator component = ((Component)root).GetComponent<Animator>();
		if ((Object)(object)component != (Object)null && component.isHuman)
		{
			AssignHumanoidReferences(ref references, component, autoDetectParams);
			return true;
		}
		DetectReferencesByNaming(ref references, root, autoDetectParams);
		Warning.logged = false;
		if (!references.isFilled)
		{
			Warning.Log("BipedReferences contains one or more missing Transforms.", root, logInEditMode: true);
			return false;
		}
		string errorMessage = "";
		if (SetupError(references, ref errorMessage))
		{
			Warning.Log(errorMessage, references.root, logInEditMode: true);
			return false;
		}
		if (SetupWarning(references, ref errorMessage))
		{
			Warning.Log(errorMessage, references.root, logInEditMode: true);
		}
		return true;
	}

	public static void DetectReferencesByNaming(ref BipedReferences references, Transform root, AutoDetectParams autoDetectParams)
	{
		if (references == null)
		{
			references = new BipedReferences();
		}
		Transform[] componentsInChildren = ((Component)root).GetComponentsInChildren<Transform>();
		DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Left, ref references.leftUpperArm, ref references.leftForearm, ref references.leftHand, componentsInChildren);
		DetectLimb(BipedNaming.BoneType.Arm, BipedNaming.BoneSide.Right, ref references.rightUpperArm, ref references.rightForearm, ref references.rightHand, componentsInChildren);
		DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Left, ref references.leftThigh, ref references.leftCalf, ref references.leftFoot, componentsInChildren);
		DetectLimb(BipedNaming.BoneType.Leg, BipedNaming.BoneSide.Right, ref references.rightThigh, ref references.rightCalf, ref references.rightFoot, componentsInChildren);
		references.head = BipedNaming.GetBone(componentsInChildren, BipedNaming.BoneType.Head, BipedNaming.BoneSide.Center);
		references.pelvis = BipedNaming.GetNamingMatch(componentsInChildren, BipedNaming.pelvis);
		if (((Object)(object)references.pelvis == (Object)null || !Hierarchy.IsAncestor(references.leftThigh, references.pelvis)) && (Object)(object)references.leftThigh != (Object)null)
		{
			references.pelvis = references.leftThigh.parent;
		}
		if ((Object)(object)references.leftUpperArm != (Object)null && (Object)(object)references.rightUpperArm != (Object)null && (Object)(object)references.pelvis != (Object)null && (Object)(object)references.leftThigh != (Object)null)
		{
			Transform firstCommonAncestor = Hierarchy.GetFirstCommonAncestor(references.leftUpperArm, references.rightUpperArm);
			if ((Object)(object)firstCommonAncestor != (Object)null)
			{
				Transform[] array = (Transform[])(object)new Transform[1] { firstCommonAncestor };
				Hierarchy.AddAncestors(array[0], references.pelvis, ref array);
				references.spine = (Transform[])(object)new Transform[0];
				for (int num = array.Length - 1; num > -1; num--)
				{
					if (AddBoneToSpine(array[num], ref references, autoDetectParams))
					{
						Array.Resize(ref references.spine, references.spine.Length + 1);
						references.spine[references.spine.Length - 1] = array[num];
					}
				}
				if ((Object)(object)references.head == (Object)null)
				{
					for (int i = 0; i < firstCommonAncestor.childCount; i++)
					{
						Transform child = firstCommonAncestor.GetChild(i);
						if (!Hierarchy.ContainsChild(child, references.leftUpperArm) && !Hierarchy.ContainsChild(child, references.rightUpperArm))
						{
							references.head = child;
							break;
						}
					}
				}
			}
		}
		Transform[] bonesOfType = BipedNaming.GetBonesOfType(BipedNaming.BoneType.Eye, componentsInChildren);
		references.eyes = (Transform[])(object)new Transform[0];
		if (!autoDetectParams.includeEyes)
		{
			return;
		}
		for (int j = 0; j < bonesOfType.Length; j++)
		{
			if (AddBoneToEyes(bonesOfType[j], ref references, autoDetectParams))
			{
				Array.Resize(ref references.eyes, references.eyes.Length + 1);
				references.eyes[references.eyes.Length - 1] = bonesOfType[j];
			}
		}
	}

	public static void AssignHumanoidReferences(ref BipedReferences references, Animator animator, AutoDetectParams autoDetectParams)
	{
		if (references == null)
		{
			references = new BipedReferences();
		}
		if (!((Object)(object)animator == (Object)null) && animator.isHuman)
		{
			references.spine = (Transform[])(object)new Transform[0];
			references.eyes = (Transform[])(object)new Transform[0];
			references.head = animator.GetBoneTransform((HumanBodyBones)10);
			references.leftThigh = animator.GetBoneTransform((HumanBodyBones)1);
			references.leftCalf = animator.GetBoneTransform((HumanBodyBones)3);
			references.leftFoot = animator.GetBoneTransform((HumanBodyBones)5);
			references.rightThigh = animator.GetBoneTransform((HumanBodyBones)2);
			references.rightCalf = animator.GetBoneTransform((HumanBodyBones)4);
			references.rightFoot = animator.GetBoneTransform((HumanBodyBones)6);
			references.leftUpperArm = animator.GetBoneTransform((HumanBodyBones)13);
			references.leftForearm = animator.GetBoneTransform((HumanBodyBones)15);
			references.leftHand = animator.GetBoneTransform((HumanBodyBones)17);
			references.rightUpperArm = animator.GetBoneTransform((HumanBodyBones)14);
			references.rightForearm = animator.GetBoneTransform((HumanBodyBones)16);
			references.rightHand = animator.GetBoneTransform((HumanBodyBones)18);
			references.pelvis = animator.GetBoneTransform((HumanBodyBones)0);
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform((HumanBodyBones)7));
			AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform((HumanBodyBones)8));
			if ((Object)(object)references.leftUpperArm != (Object)null && !IsNeckBone(animator.GetBoneTransform((HumanBodyBones)9), references.leftUpperArm))
			{
				AddBoneToHierarchy(ref references.spine, animator.GetBoneTransform((HumanBodyBones)9));
			}
			if (autoDetectParams.includeEyes)
			{
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform((HumanBodyBones)21));
				AddBoneToHierarchy(ref references.eyes, animator.GetBoneTransform((HumanBodyBones)22));
			}
		}
	}

	public static bool SetupError(BipedReferences references, ref string errorMessage)
	{
		if (!references.isFilled)
		{
			errorMessage = "BipedReferences contains one or more missing Transforms.";
			return true;
		}
		if (LimbError(references.leftThigh, references.leftCalf, references.leftFoot, ref errorMessage))
		{
			return true;
		}
		if (LimbError(references.rightThigh, references.rightCalf, references.rightFoot, ref errorMessage))
		{
			return true;
		}
		if (LimbError(references.leftUpperArm, references.leftForearm, references.leftHand, ref errorMessage))
		{
			return true;
		}
		if (LimbError(references.rightUpperArm, references.rightForearm, references.rightHand, ref errorMessage))
		{
			return true;
		}
		if (SpineError(references, ref errorMessage))
		{
			return true;
		}
		if (EyesError(references, ref errorMessage))
		{
			return true;
		}
		return false;
	}

	public static bool SetupWarning(BipedReferences references, ref string warningMessage)
	{
		if (LimbWarning(references.leftThigh, references.leftCalf, references.leftFoot, ref warningMessage))
		{
			return true;
		}
		if (LimbWarning(references.rightThigh, references.rightCalf, references.rightFoot, ref warningMessage))
		{
			return true;
		}
		if (LimbWarning(references.leftUpperArm, references.leftForearm, references.leftHand, ref warningMessage))
		{
			return true;
		}
		if (LimbWarning(references.rightUpperArm, references.rightForearm, references.rightHand, ref warningMessage))
		{
			return true;
		}
		if (SpineWarning(references, ref warningMessage))
		{
			return true;
		}
		if (EyesWarning(references, ref warningMessage))
		{
			return true;
		}
		if (RootHeightWarning(references, ref warningMessage))
		{
			return true;
		}
		if (FacingAxisWarning(references, ref warningMessage))
		{
			return true;
		}
		return false;
	}

	private static bool IsNeckBone(Transform bone, Transform leftUpperArm)
	{
		if ((Object)(object)leftUpperArm.parent != (Object)null && (Object)(object)leftUpperArm.parent == (Object)(object)bone)
		{
			return false;
		}
		if (Hierarchy.IsAncestor(leftUpperArm, bone))
		{
			return false;
		}
		return true;
	}

	private static bool AddBoneToEyes(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams)
	{
		if ((Object)(object)references.head != (Object)null && !Hierarchy.IsAncestor(bone, references.head))
		{
			return false;
		}
		if ((Object)(object)((Component)bone).GetComponent<SkinnedMeshRenderer>() != (Object)null)
		{
			return false;
		}
		return true;
	}

	private static bool AddBoneToSpine(Transform bone, ref BipedReferences references, AutoDetectParams autoDetectParams)
	{
		if ((Object)(object)bone == (Object)(object)references.root)
		{
			return false;
		}
		if ((Object)(object)bone == (Object)(object)references.leftThigh.parent && !autoDetectParams.legsParentInSpine)
		{
			return false;
		}
		if ((Object)(object)references.pelvis != (Object)null)
		{
			if ((Object)(object)bone == (Object)(object)references.pelvis)
			{
				return false;
			}
			if (Hierarchy.IsAncestor(references.pelvis, bone))
			{
				return false;
			}
		}
		return true;
	}

	private static void DetectLimb(BipedNaming.BoneType boneType, BipedNaming.BoneSide boneSide, ref Transform firstBone, ref Transform secondBone, ref Transform lastBone, Transform[] transforms)
	{
		Transform[] bonesOfTypeAndSide = BipedNaming.GetBonesOfTypeAndSide(boneType, boneSide, transforms);
		if (bonesOfTypeAndSide.Length >= 3)
		{
			if (bonesOfTypeAndSide.Length == 3)
			{
				firstBone = bonesOfTypeAndSide[0];
				secondBone = bonesOfTypeAndSide[1];
				lastBone = bonesOfTypeAndSide[2];
			}
			if (bonesOfTypeAndSide.Length > 3)
			{
				firstBone = bonesOfTypeAndSide[0];
				secondBone = bonesOfTypeAndSide[2];
				lastBone = bonesOfTypeAndSide[^1];
			}
		}
	}

	private static void AddBoneToHierarchy(ref Transform[] bones, Transform transform)
	{
		if (!((Object)(object)transform == (Object)null))
		{
			Array.Resize(ref bones, bones.Length + 1);
			bones[bones.Length - 1] = transform;
		}
	}

	private static bool LimbError(Transform bone1, Transform bone2, Transform bone3, ref string errorMessage)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)bone1 == (Object)null)
		{
			errorMessage = "Bone 1 of a BipedReferences limb is null.";
			return true;
		}
		if ((Object)(object)bone2 == (Object)null)
		{
			errorMessage = "Bone 2 of a BipedReferences limb is null.";
			return true;
		}
		if ((Object)(object)bone3 == (Object)null)
		{
			errorMessage = "Bone 3 of a BipedReferences limb is null.";
			return true;
		}
		Object[] objects = (Object[])(object)new Transform[3] { bone1, bone2, bone3 };
		Transform val = (Transform)Hierarchy.ContainsDuplicate(objects);
		if ((Object)(object)val != (Object)null)
		{
			errorMessage = ((Object)val).name + " is represented multiple times in the same BipedReferences limb.";
			return true;
		}
		if (bone2.position == bone1.position)
		{
			errorMessage = "Second bone's position equals first bone's position in the biped's limb.";
			return true;
		}
		if (bone3.position == bone2.position)
		{
			errorMessage = "Third bone's position equals second bone's position in the biped's limb.";
			return true;
		}
		if (!Hierarchy.HierarchyIsValid((Transform[])(object)new Transform[3] { bone1, bone2, bone3 }))
		{
			errorMessage = "BipedReferences limb hierarchy is invalid. Bone transforms in a limb do not belong to the same ancestry. Please make sure the bones are parented to each other. Bones: " + ((Object)bone1).name + ", " + ((Object)bone2).name + ", " + ((Object)bone3).name;
			return true;
		}
		return false;
	}

	private static bool LimbWarning(Transform bone1, Transform bone2, Transform bone3, ref string warningMessage)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(bone2.position - bone1.position, bone3.position - bone1.position);
		if (val == Vector3.zero)
		{
			warningMessage = "BipedReferences limb is completely stretched out in the initial pose. IK solver can not calculate the default bend plane for the limb. Please make sure you character's limbs are at least slightly bent in the initial pose. First bone: " + ((Object)bone1).name + ", second bone: " + ((Object)bone2).name + ".";
			return true;
		}
		return false;
	}

	private static bool SpineError(BipedReferences references, ref string errorMessage)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (references.spine.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < references.spine.Length; i++)
		{
			if ((Object)(object)references.spine[i] == (Object)null)
			{
				errorMessage = "BipedReferences spine bone at index " + i + " is null.";
				return true;
			}
		}
		Object[] objects = (Object[])(object)references.spine;
		Transform val = (Transform)Hierarchy.ContainsDuplicate(objects);
		if ((Object)(object)val != (Object)null)
		{
			errorMessage = ((Object)val).name + " is represented multiple times in BipedReferences spine.";
			return true;
		}
		if (!Hierarchy.HierarchyIsValid(references.spine))
		{
			errorMessage = "BipedReferences spine hierarchy is invalid. Bone transforms in the spine do not belong to the same ancestry. Please make sure the bones are parented to each other.";
			return true;
		}
		for (int j = 0; j < references.spine.Length; j++)
		{
			bool flag = false;
			if (j == 0 && references.spine[j].position == references.pelvis.position)
			{
				flag = true;
			}
			if (j != 0 && references.spine.Length > 1 && references.spine[j].position == references.spine[j - 1].position)
			{
				flag = true;
			}
			if (flag)
			{
				errorMessage = "Biped's spine bone nr " + j + " position is the same as its parent spine/pelvis bone's position. Please remove this bone from the spine.";
				return true;
			}
		}
		return false;
	}

	private static bool SpineWarning(BipedReferences references, ref string warningMessage)
	{
		return false;
	}

	private static bool EyesError(BipedReferences references, ref string errorMessage)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		if (references.eyes.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < references.eyes.Length; i++)
		{
			if ((Object)(object)references.eyes[i] == (Object)null)
			{
				errorMessage = "BipedReferences eye bone at index " + i + " is null.";
				return true;
			}
		}
		Object[] objects = (Object[])(object)references.eyes;
		Transform val = (Transform)Hierarchy.ContainsDuplicate(objects);
		if ((Object)(object)val != (Object)null)
		{
			errorMessage = ((Object)val).name + " is represented multiple times in BipedReferences eyes.";
			return true;
		}
		return false;
	}

	private static bool EyesWarning(BipedReferences references, ref string warningMessage)
	{
		return false;
	}

	private static bool RootHeightWarning(BipedReferences references, ref string warningMessage)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)references.head == (Object)null)
		{
			return false;
		}
		float verticalOffset = GetVerticalOffset(references.head.position, references.leftFoot.position, references.root.rotation);
		float verticalOffset2 = GetVerticalOffset(references.root.position, references.leftFoot.position, references.root.rotation);
		if (verticalOffset2 / verticalOffset > 0.2f)
		{
			warningMessage = "Biped's root Transform's position should be at ground level relative to the character (at the character's feet not at its pelvis).";
			return true;
		}
		return false;
	}

	private static bool FacingAxisWarning(BipedReferences references, ref string warningMessage)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = references.rightHand.position - references.leftHand.position;
		Vector3 val2 = references.rightFoot.position - references.leftFoot.position;
		float num = Vector3.Dot(val.normalized, references.root.right);
		float num2 = Vector3.Dot(val2.normalized, references.root.right);
		if (num < 0f || num2 < 0f)
		{
			warningMessage = "Biped does not seem to be facing its forward axis. Please make sure that in the initial pose the character is facing towards the positive Z axis of the Biped root gameobject.";
			return true;
		}
		return false;
	}

	private static float GetVerticalOffset(Vector3 p1, Vector3 p2, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Quaternion.Inverse(rotation) * (p1 - p2);
		return val.y;
	}
}
