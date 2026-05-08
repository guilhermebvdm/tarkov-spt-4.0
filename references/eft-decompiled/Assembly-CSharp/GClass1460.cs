using System;
using UnityEngine;

public abstract class GClass1460
{
	[Serializable]
	public enum BoneType
	{
		Unassigned,
		Spine,
		Head,
		Arm,
		Leg,
		Tail,
		Eye
	}

	[Serializable]
	public enum BoneSide
	{
		Center,
		Left,
		Right
	}

	public static string[] typeLeft = new string[9] { " L ", "_L_", "-L-", " l ", "_l_", "-l-", "Left", "left", "CATRigL" };

	public static string[] typeRight = new string[9] { " R ", "_R_", "-R-", " r ", "_r_", "-r-", "Right", "right", "CATRigR" };

	public static string[] typeSpine = new string[16]
	{
		"Spine", "spine", "Pelvis", "pelvis", "Root", "root", "Torso", "torso", "Body", "body",
		"Hips", "hips", "Neck", "neck", "Chest", "chest"
	};

	public static string[] typeHead = new string[2] { "Head", "head" };

	public static string[] typeArm = new string[10] { "Arm", "arm", "Hand", "hand", "Wrist", "Wrist", "Elbow", "elbow", "Palm", "palm" };

	public static string[] typeLeg = new string[16]
	{
		"Leg", "leg", "Thigh", "thigh", "Calf", "calf", "Femur", "femur", "Knee", "knee",
		"Foot", "foot", "Ankle", "ankle", "Hip", "hip"
	};

	public static string[] typeTail = new string[2] { "Tail", "tail" };

	public static string[] typeEye = new string[2] { "Eye", "eye" };

	public static string[] typeExclude = new string[6] { "Nub", "Dummy", "dummy", "Tip", "IK", "Mesh" };

	public static string[] typeExcludeSpine = new string[2] { "Head", "head" };

	public static string[] typeExcludeHead = new string[2] { "Top", "End" };

	public static string[] typeExcludeArm = new string[19]
	{
		"Collar", "collar", "Clavicle", "clavicle", "Finger", "finger", "Index", "index", "Mid", "mid",
		"Pinky", "pinky", "Ring", "Thumb", "thumb", "Adjust", "adjust", "Twist", "twist"
	};

	public static string[] typeExcludeLeg = new string[7] { "Toe", "toe", "Platform", "Adjust", "adjust", "Twist", "twist" };

	public static string[] typeExcludeTail = new string[0];

	public static string[] typeExcludeEye = new string[6] { "Lid", "lid", "Brow", "brow", "Lash", "lash" };

	public static string[] pelvis = new string[4] { "Pelvis", "pelvis", "Hip", "hip" };

	public static string[] hand = new string[6] { "Hand", "hand", "Wrist", "wrist", "Palm", "palm" };

	public static string[] foot = new string[4] { "Foot", "foot", "Ankle", "ankle" };

	public static Transform[] GetBonesOfType(BoneType boneType, Transform[] bones)
	{
		Transform[] array = new Transform[0];
		foreach (Transform transform in bones)
		{
			if (transform != null && GetBoneType(transform.name) == boneType)
			{
				Array.Resize(ref array, array.Length + 1);
				array[^1] = transform;
			}
		}
		return array;
	}

	public static Transform[] GetBonesOfSide(BoneSide boneSide, Transform[] bones)
	{
		Transform[] array = new Transform[0];
		foreach (Transform transform in bones)
		{
			if (transform != null && GetBoneSide(transform.name) == boneSide)
			{
				Array.Resize(ref array, array.Length + 1);
				array[^1] = transform;
			}
		}
		return array;
	}

	public static Transform[] GetBonesOfTypeAndSide(BoneType boneType, BoneSide boneSide, Transform[] bones)
	{
		Transform[] bonesOfType = GetBonesOfType(boneType, bones);
		return GetBonesOfSide(boneSide, bonesOfType);
	}

	public static Transform GetFirstBoneOfTypeAndSide(BoneType boneType, BoneSide boneSide, Transform[] bones)
	{
		Transform[] bonesOfTypeAndSide = GetBonesOfTypeAndSide(boneType, boneSide, bones);
		if (bonesOfTypeAndSide.Length == 0)
		{
			return null;
		}
		return bonesOfTypeAndSide[0];
	}

	public static Transform GetNamingMatch(Transform[] transforms, params string[][] namings)
	{
		int num = 0;
		Transform transform;
		while (true)
		{
			if (num < transforms.Length)
			{
				transform = transforms[num];
				bool flag = true;
				foreach (string[] namingConvention in namings)
				{
					if (!smethod_9(transform.name, namingConvention))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return transform;
	}

	public static BoneType GetBoneType(string boneName)
	{
		if (smethod_2(boneName))
		{
			return BoneType.Spine;
		}
		if (smethod_3(boneName))
		{
			return BoneType.Head;
		}
		if (smethod_4(boneName))
		{
			return BoneType.Arm;
		}
		if (smethod_5(boneName))
		{
			return BoneType.Leg;
		}
		if (smethod_6(boneName))
		{
			return BoneType.Tail;
		}
		if (smethod_7(boneName))
		{
			return BoneType.Eye;
		}
		return BoneType.Unassigned;
	}

	public static BoneSide GetBoneSide(string boneName)
	{
		if (smethod_0(boneName))
		{
			return BoneSide.Left;
		}
		if (smethod_1(boneName))
		{
			return BoneSide.Right;
		}
		return BoneSide.Center;
	}

	public static Transform GetBone(Transform[] transforms, BoneType boneType, BoneSide boneSide = BoneSide.Center, params string[][] namings)
	{
		return GetNamingMatch(GetBonesOfTypeAndSide(boneType, boneSide, transforms), namings);
	}

	public static bool smethod_0(string boneName)
	{
		if (!smethod_9(boneName, typeLeft) && !(smethod_14(boneName) == "L"))
		{
			return smethod_13(boneName) == "L";
		}
		return true;
	}

	public static bool smethod_1(string boneName)
	{
		if (!smethod_9(boneName, typeRight) && !(smethod_14(boneName) == "R"))
		{
			return smethod_13(boneName) == "R";
		}
		return true;
	}

	public static bool smethod_2(string boneName)
	{
		if (smethod_9(boneName, typeSpine))
		{
			return !smethod_10(boneName, typeExcludeSpine);
		}
		return false;
	}

	public static bool smethod_3(string boneName)
	{
		if (smethod_9(boneName, typeHead))
		{
			return !smethod_10(boneName, typeExcludeHead);
		}
		return false;
	}

	public static bool smethod_4(string boneName)
	{
		if (smethod_9(boneName, typeArm))
		{
			return !smethod_10(boneName, typeExcludeArm);
		}
		return false;
	}

	public static bool smethod_5(string boneName)
	{
		if (smethod_9(boneName, typeLeg))
		{
			return !smethod_10(boneName, typeExcludeLeg);
		}
		return false;
	}

	public static bool smethod_6(string boneName)
	{
		if (smethod_9(boneName, typeTail))
		{
			return !smethod_10(boneName, typeExcludeTail);
		}
		return false;
	}

	public static bool smethod_7(string boneName)
	{
		if (smethod_9(boneName, typeEye))
		{
			return !smethod_10(boneName, typeExcludeEye);
		}
		return false;
	}

	public static bool smethod_8(string boneName)
	{
		return smethod_9(boneName, typeExclude);
	}

	public static bool smethod_9(string boneName, string[] namingConvention)
	{
		if (smethod_10(boneName, typeExclude))
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			if (num < namingConvention.Length)
			{
				string value = namingConvention[num];
				if (boneName.Contains(value))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public static bool smethod_10(string boneName, string[] namingConvention)
	{
		int num = 0;
		while (true)
		{
			if (num < namingConvention.Length)
			{
				string value = namingConvention[num];
				if (boneName.Contains(value))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public static bool smethod_11(string boneName, string[] namingConvention)
	{
		int num = 0;
		while (true)
		{
			if (num < namingConvention.Length)
			{
				string letter = namingConvention[num];
				if (smethod_12(boneName, letter))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public static bool smethod_12(string boneName, string letter)
	{
		return boneName.Substring(boneName.Length - 1, 1) == letter;
	}

	public static string smethod_13(string boneName)
	{
		if (boneName.Length > 0)
		{
			return boneName.Substring(0, 1);
		}
		return "";
	}

	public static string smethod_14(string boneName)
	{
		if (boneName.Length > 0)
		{
			return boneName.Substring(boneName.Length - 1, 1);
		}
		return "";
	}
}
