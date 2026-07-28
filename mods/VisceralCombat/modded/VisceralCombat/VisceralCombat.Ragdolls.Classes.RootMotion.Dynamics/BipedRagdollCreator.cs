using System;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[HelpURL("https://www.youtube.com/watch?v=y-luLRVmL7E&index=1&list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL")]
[AddComponentMenu("Scripts/RootMotion.Dynamics/Ragdoll Manager/Biped Ragdoll Creator")]
public class BipedRagdollCreator : RagdollCreator
{
	[Serializable]
	public struct Options
	{
		public float weight;

		[Header("Optional Bones")]
		public bool spine;

		public bool chest;

		public bool hands;

		public bool feet;

		[Header("Joints")]
		public JointType joints;

		public float jointRange;

		[Header("Colliders")]
		public float colliderLengthOverlap;

		public ColliderType torsoColliders;

		public ColliderType headCollider;

		public ColliderType armColliders;

		public ColliderType handColliders;

		public ColliderType legColliders;

		public ColliderType footColliders;

		public bool fixFootColliderRotation;

		public static Options Default
		{
			get
			{
				Options result = default(Options);
				result.weight = 75f;
				result.colliderLengthOverlap = 0.1f;
				result.jointRange = 1f;
				result.chest = true;
				result.headCollider = ColliderType.Capsule;
				result.armColliders = ColliderType.Capsule;
				result.hands = true;
				result.handColliders = ColliderType.Capsule;
				result.legColliders = ColliderType.Capsule;
				result.feet = true;
				result.fixFootColliderRotation = true;
				return result;
			}
		}
	}

	public bool canBuild;

	public BipedRagdollReferences references;

	public Options options = Options.Default;

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page1.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_biped_ragdoll_creator.html#details");
	}

	[ContextMenu("TUTORIAL VIDEO")]
	private void OpenTutorial()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=y-luLRVmL7E&index=1&list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL");
	}

	public static Options AutodetectOptions(BipedRagdollReferences r)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Options @default = Options.Default;
		if ((Object)(object)r.spine == (Object)null)
		{
			@default.spine = false;
		}
		if ((Object)(object)r.chest == (Object)null)
		{
			@default.chest = false;
		}
		if (@default.chest && Vector3.Dot(r.root.up, r.chest.position - GetUpperArmCentroid(r)) > 0f)
		{
			@default.chest = false;
			if ((Object)(object)r.spine != (Object)null)
			{
				@default.spine = true;
			}
		}
		return @default;
	}

	public void Create(BipedRagdollReferences r, Options options)
	{
		string msg = string.Empty;
		if (r.IsValid(ref msg))
		{
			RagdollCreator.ClearAll(r.root);
			CreateColliders(r, options);
			MassDistribution(r, options);
			CreateJoints(r, options);
		}
	}

	private static void CreateColliders(BipedRagdollReferences r, Options options)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0471: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		Vector3 upperArmToHeadCentroid = GetUpperArmToHeadCentroid(r);
		if ((Object)(object)r.spine == (Object)null)
		{
			options.spine = false;
		}
		if ((Object)(object)r.chest == (Object)null)
		{
			options.chest = false;
		}
		Vector3 widthDirection = r.rightUpperArm.position - r.leftUpperArm.position;
		float magnitude = ((Vector3)(ref widthDirection)).magnitude;
		float proportionAspect = 0.6f;
		Vector3 val = r.hips.position;
		float num = Vector3.Distance(r.head.position, r.root.position);
		float num2 = Vector3.Distance(r.hips.position, r.root.position);
		if (num2 < num * 0.2f)
		{
			val = Vector3.Lerp(r.leftUpperLeg.position, r.rightUpperLeg.position, 0.5f);
		}
		Vector3 val2 = (options.spine ? r.spine.position : (options.chest ? r.chest.position : upperArmToHeadCentroid));
		val += (val - upperArmToHeadCentroid) * 0.1f;
		float width = ((options.spine || options.chest) ? (magnitude * 0.8f) : magnitude);
		RagdollCreator.CreateCollider(r.hips, val, val2, options.torsoColliders, options.colliderLengthOverlap, width, proportionAspect, widthDirection);
		if (options.spine)
		{
			Vector3 startPoint = val2;
			val2 = (options.chest ? r.chest.position : upperArmToHeadCentroid);
			float width2 = (options.chest ? (magnitude * 0.75f) : magnitude);
			RagdollCreator.CreateCollider(r.spine, startPoint, val2, options.torsoColliders, options.colliderLengthOverlap, width2, proportionAspect, widthDirection);
		}
		if (options.chest)
		{
			Vector3 startPoint2 = val2;
			val2 = upperArmToHeadCentroid;
			RagdollCreator.CreateCollider(r.chest, startPoint2, val2, options.torsoColliders, options.colliderLengthOverlap, magnitude, proportionAspect, widthDirection);
		}
		Vector3 val3 = val2;
		Vector3 val4 = val3 + (val3 - val) * 0.45f;
		Vector3 val5 = r.head.TransformVector(AxisTools.GetAxisVectorToDirection(r.head, val4 - val3));
		Vector3 val6 = Vector3.Project(val4 - val3, val5);
		Vector3 normalized = ((Vector3)(ref val6)).normalized;
		val6 = val4 - val3;
		val4 = val3 + normalized * ((Vector3)(ref val6)).magnitude;
		RagdollCreator.CreateCollider(r.head, val3, val4, options.headCollider, options.colliderLengthOverlap, Vector3.Distance(val3, val4) * 0.8f);
		float num3 = 0.4f;
		float num4 = Vector3.Distance(r.leftUpperArm.position, r.leftLowerArm.position) * num3;
		RagdollCreator.CreateCollider(r.leftUpperArm, r.leftUpperArm.position, r.leftLowerArm.position, options.armColliders, options.colliderLengthOverlap, num4);
		RagdollCreator.CreateCollider(r.leftLowerArm, r.leftLowerArm.position, r.leftHand.position, options.armColliders, options.colliderLengthOverlap, num4 * 0.9f);
		float num5 = Vector3.Distance(r.rightUpperArm.position, r.rightLowerArm.position) * num3;
		RagdollCreator.CreateCollider(r.rightUpperArm, r.rightUpperArm.position, r.rightLowerArm.position, options.armColliders, options.colliderLengthOverlap, num5);
		RagdollCreator.CreateCollider(r.rightLowerArm, r.rightLowerArm.position, r.rightHand.position, options.armColliders, options.colliderLengthOverlap, num5 * 0.9f);
		float num6 = 0.3f;
		float num7 = Vector3.Distance(r.leftUpperLeg.position, r.leftLowerLeg.position) * num6;
		RagdollCreator.CreateCollider(r.leftUpperLeg, r.leftUpperLeg.position, r.leftLowerLeg.position, options.legColliders, options.colliderLengthOverlap, num7);
		RagdollCreator.CreateCollider(r.leftLowerLeg, r.leftLowerLeg.position, r.leftFoot.position, options.legColliders, options.colliderLengthOverlap, num7 * 0.9f);
		float num8 = Vector3.Distance(r.rightUpperLeg.position, r.rightLowerLeg.position) * num6;
		RagdollCreator.CreateCollider(r.rightUpperLeg, r.rightUpperLeg.position, r.rightLowerLeg.position, options.legColliders, options.colliderLengthOverlap, num8);
		RagdollCreator.CreateCollider(r.rightLowerLeg, r.rightLowerLeg.position, r.rightFoot.position, options.legColliders, options.colliderLengthOverlap, num8 * 0.9f);
		if (options.hands)
		{
			CreateHandCollider(r.leftHand, r.leftLowerArm, r.root, options);
			CreateHandCollider(r.rightHand, r.rightLowerArm, r.root, options);
		}
		if (options.feet)
		{
			CreateFootCollider(r.leftFoot, r.leftLowerLeg, r.leftUpperLeg, r.root, options);
			CreateFootCollider(r.rightFoot, r.rightLowerLeg, r.rightUpperLeg, r.root, options);
		}
	}

	private static Collider CopyCollider(Collider c, GameObject destination)
	{
		if (c is CapsuleCollider)
		{
			return (Collider)(object)CopyCapsuleCollider((CapsuleCollider)(object)((c is CapsuleCollider) ? c : null), destination);
		}
		if (c is SphereCollider)
		{
			return (Collider)(object)CopySphereCollider((SphereCollider)(object)((c is SphereCollider) ? c : null), destination);
		}
		if (c is BoxCollider)
		{
			return (Collider)(object)CopyBoxCollider((BoxCollider)(object)((c is BoxCollider) ? c : null), destination);
		}
		return null;
	}

	private static CapsuleCollider CopyCapsuleCollider(CapsuleCollider o, GameObject destination)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		CapsuleCollider val = destination.GetComponent<CapsuleCollider>();
		if ((Object)(object)val == (Object)null)
		{
			val = destination.AddComponent<CapsuleCollider>();
		}
		((Collider)val).isTrigger = ((Collider)o).isTrigger;
		((Collider)val).sharedMaterial = ((Collider)o).sharedMaterial;
		val.center = o.center;
		val.radius = o.radius;
		val.height = o.height;
		val.direction = o.direction;
		return val;
	}

	private static SphereCollider CopySphereCollider(SphereCollider o, GameObject destination)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		SphereCollider val = destination.GetComponent<SphereCollider>();
		if ((Object)(object)val == (Object)null)
		{
			val = destination.AddComponent<SphereCollider>();
		}
		((Collider)val).isTrigger = ((Collider)o).isTrigger;
		((Collider)val).sharedMaterial = ((Collider)o).sharedMaterial;
		val.center = o.center;
		val.radius = o.radius;
		return val;
	}

	private static BoxCollider CopyBoxCollider(BoxCollider o, GameObject destination)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider val = destination.GetComponent<BoxCollider>();
		if ((Object)(object)val == (Object)null)
		{
			val = destination.AddComponent<BoxCollider>();
		}
		((Collider)val).isTrigger = ((Collider)o).isTrigger;
		((Collider)val).sharedMaterial = ((Collider)o).sharedMaterial;
		val.center = o.center;
		val.size = o.size;
		return val;
	}

	private static void CreateHandCollider(Transform hand, Transform lowerArm, Transform root, Options options)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = hand.TransformVector(AxisTools.GetAxisVectorToPoint(hand, GetChildCentroid(hand, lowerArm.position)));
		Vector3 val2 = hand.position - (lowerArm.position - hand.position) * 0.75f;
		Vector3 position = hand.position;
		Vector3 val3 = Vector3.Project(val2 - hand.position, val);
		Vector3 normalized = ((Vector3)(ref val3)).normalized;
		val3 = val2 - hand.position;
		val2 = position + normalized * ((Vector3)(ref val3)).magnitude;
		RagdollCreator.CreateCollider(hand, hand.position, val2, options.handColliders, options.colliderLengthOverlap, Vector3.Distance(val2, hand.position) * 0.5f);
	}

	private static void CreateFootCollider(Transform foot, Transform lowerLeg, Transform upperLeg, Transform root, Options options)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = upperLeg.position - foot.position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		Vector3 val2 = foot.TransformVector(AxisTools.GetAxisVectorToPoint(foot, GetChildCentroid(foot, foot.position + root.forward) + root.forward * magnitude * 0.2f));
		Vector3 val3 = foot.position + root.forward * magnitude * 0.25f;
		Vector3 position = foot.position;
		val = Vector3.Project(val3 - foot.position, val2);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = val3 - foot.position;
		val3 = position + normalized * ((Vector3)(ref val)).magnitude;
		float num = Vector3.Distance(val3, foot.position) * 0.5f;
		Vector3 position2 = foot.position;
		Vector3 val4 = ((Vector3.Dot(root.up, foot.position - root.position) < 0f) ? Vector3.zero : Vector3.Project(position2 - root.up * num * 0.5f - root.position, root.up));
		Vector3 val5 = val3 - position2;
		position2 -= val5 * 0.2f;
		if (options.fixFootColliderRotation)
		{
			Vector3 val6 = AxisTools.GetAxisVectorToDirection(foot, root.forward);
			if (Vector3.Dot(foot.rotation * val6, root.forward) < 0f)
			{
				val6 = -val6;
			}
			Vector3 up = Vector3.up;
			Vector3 val7 = foot.rotation * val6;
			Vector3.OrthoNormalize(ref up, ref val7);
			Vector3 val8 = foot.position + val7;
			Vector3 childCentroidRecursive = GetChildCentroidRecursive(foot, val8);
			Vector3 val9 = childCentroidRecursive - foot.position;
			Transform transform = new GameObject("Foot Collider").transform;
			transform.parent = foot;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			Collider c = RagdollCreator.CreateCollider(transform, position2 - val4, val3 - val4, options.footColliders, options.colliderLengthOverlap, num, foot);
			transform.rotation = Quaternion.FromToRotation(transform.rotation * val6, childCentroidRecursive - transform.position) * transform.rotation;
			Orthogonize(transform, root.forward, root.up);
			Orthogonize(transform, root.right, root.up);
			if (childCentroidRecursive != val8)
			{
				Vector3 val10 = Vector3.Lerp(foot.position, childCentroidRecursive, 0.5f);
				Vector3 colliderCenter = GetColliderCenter(c);
				transform.position += val10 - colliderCenter;
				float colliderBottom = GetColliderBottom(c, root.up);
				transform.position += Vector3.up * (root.position.y - colliderBottom);
			}
		}
		else
		{
			RagdollCreator.CreateCollider(foot, position2 - val4, val3 - val4, options.footColliders, options.colliderLengthOverlap, num);
		}
	}

	public static Collider FixFootCollider(Transform foot, Transform root)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = AxisTools.GetAxisVectorToDirection(foot, root.forward);
		if (Vector3.Dot(foot.rotation * val, root.forward) < 0f)
		{
			val = -val;
		}
		Vector3 up = Vector3.up;
		Vector3 val2 = foot.rotation * val;
		Vector3.OrthoNormalize(ref up, ref val2);
		Vector3 val3 = foot.position + val2;
		Vector3 childCentroidRecursive = GetChildCentroidRecursive(foot, val3);
		Vector3 val4 = childCentroidRecursive - foot.position;
		Transform transform = new GameObject("Foot Collider").transform;
		transform.parent = foot;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		Collider component = ((Component)foot).GetComponent<Collider>();
		Collider val5 = CopyCollider(component, ((Component)transform).gameObject);
		if (Application.isPlaying)
		{
			Object.Destroy((Object)(object)component);
		}
		else
		{
			Object.DestroyImmediate((Object)(object)component);
		}
		transform.rotation = Quaternion.FromToRotation(transform.rotation * val, childCentroidRecursive - transform.position) * transform.rotation;
		Orthogonize(transform, root.forward, root.up);
		Orthogonize(transform, root.right, root.up);
		if (childCentroidRecursive != val3)
		{
			Vector3 val6 = Vector3.Lerp(foot.position, childCentroidRecursive, 0.5f);
			Vector3 colliderCenter = GetColliderCenter(val5);
			transform.position += val6 - colliderCenter;
			float colliderBottom = GetColliderBottom(val5, root.up);
			transform.position += Vector3.up * (root.position.y - colliderBottom);
		}
		return val5;
	}

	private static Vector3 GetColliderCenter(Collider c)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (c is BoxCollider)
		{
			return ((Component)c).transform.TransformPoint(((BoxCollider)((c is BoxCollider) ? c : null)).center);
		}
		if (c is CapsuleCollider)
		{
			return ((Component)c).transform.TransformPoint(((CapsuleCollider)((c is CapsuleCollider) ? c : null)).center);
		}
		return ((Component)c).transform.position;
	}

	private static float GetColliderBottom(Collider c, Vector3 up)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)c).transform;
		if (c is BoxCollider)
		{
			BoxCollider val = (BoxCollider)(object)((c is BoxCollider) ? c : null);
			Vector3 val2 = AxisTools.GetAxisVectorToDirection(transform, -up);
			if (Vector3.Dot(transform.rotation * val2, -up) < 0f)
			{
				val2 = -val2;
			}
			Vector3 val3 = Vector3.Scale(val.size, val2 * 0.5f);
			return (transform.TransformPoint(val.center) + transform.rotation * val3).y;
		}
		if (c is CapsuleCollider)
		{
			CapsuleCollider val4 = (CapsuleCollider)(object)((c is CapsuleCollider) ? c : null);
			Vector3 val5 = AxisTools.GetAxisVectorToDirection(transform, -up);
			if (Vector3.Dot(transform.rotation * val5, -up) < 0f)
			{
				val5 = -val5;
			}
			Vector3 val6 = val4.radius * val5 * 0.5f;
			return (transform.TransformPoint(val4.center) + transform.rotation * val6).y;
		}
		return GetColliderCenter(c).y;
	}

	private static void Orthogonize(Transform t, Vector3 direction, Vector3 normal)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = AxisTools.GetAxisVectorToDirection(t, direction);
		if (Vector3.Dot(t.rotation * val, direction) < 0f)
		{
			val = -val;
		}
		Vector3 val2 = t.rotation * val;
		Vector3.OrthoNormalize(ref normal, ref val2);
		t.rotation = Quaternion.FromToRotation(t.rotation * val, val2) * t.rotation;
	}

	private static Vector3 GetChildCentroidRecursive(Transform t, Vector3 fallback)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Transform[] componentsInChildren = ((Component)t).GetComponentsInChildren<Transform>();
		if (componentsInChildren.Length < 2)
		{
			return fallback;
		}
		Vector3 val = Vector3.zero;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			val += componentsInChildren[i].position;
		}
		return val / (float)(componentsInChildren.Length - 1);
	}

	private static Vector3 GetChildCentroid(Transform t, Vector3 fallback)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (t.childCount == 0)
		{
			return fallback;
		}
		Vector3 val = Vector3.zero;
		for (int i = 0; i < t.childCount; i++)
		{
			val += t.GetChild(i).position;
		}
		return val / (float)t.childCount;
	}

	private static void MassDistribution(BipedRagdollReferences r, Options o)
	{
		int num = 3;
		if ((Object)(object)r.spine == (Object)null)
		{
			o.spine = false;
			num--;
		}
		if ((Object)(object)r.chest == (Object)null)
		{
			o.chest = false;
			num--;
		}
		float num2 = 0.508f / (float)num;
		float num3 = 0.0732f;
		float num4 = 0.027f;
		float num5 = 0.016f;
		float num6 = 0.0066f;
		float num7 = 0.0988f;
		float num8 = 0.0465f;
		float num9 = 0.0145f;
		((Component)r.hips).GetComponent<Rigidbody>().mass = num2 * o.weight;
		if (o.spine)
		{
			((Component)r.spine).GetComponent<Rigidbody>().mass = num2 * o.weight;
		}
		if (o.chest)
		{
			((Component)r.chest).GetComponent<Rigidbody>().mass = num2 * o.weight;
		}
		((Component)r.head).GetComponent<Rigidbody>().mass = num3 * o.weight;
		((Component)r.leftUpperArm).GetComponent<Rigidbody>().mass = num4 * o.weight;
		((Component)r.rightUpperArm).GetComponent<Rigidbody>().mass = ((Component)r.leftUpperArm).GetComponent<Rigidbody>().mass;
		((Component)r.leftLowerArm).GetComponent<Rigidbody>().mass = num5 * o.weight;
		((Component)r.rightLowerArm).GetComponent<Rigidbody>().mass = ((Component)r.leftLowerArm).GetComponent<Rigidbody>().mass;
		if (o.hands)
		{
			((Component)r.leftHand).GetComponent<Rigidbody>().mass = num6 * o.weight;
			((Component)r.rightHand).GetComponent<Rigidbody>().mass = ((Component)r.leftHand).GetComponent<Rigidbody>().mass;
		}
		((Component)r.leftUpperLeg).GetComponent<Rigidbody>().mass = num7 * o.weight;
		((Component)r.rightUpperLeg).GetComponent<Rigidbody>().mass = ((Component)r.leftUpperLeg).GetComponent<Rigidbody>().mass;
		((Component)r.leftLowerLeg).GetComponent<Rigidbody>().mass = num8 * o.weight;
		((Component)r.rightLowerLeg).GetComponent<Rigidbody>().mass = ((Component)r.leftLowerLeg).GetComponent<Rigidbody>().mass;
		if (o.feet)
		{
			((Component)r.leftFoot).GetComponent<Rigidbody>().mass = num9 * o.weight;
			((Component)r.rightFoot).GetComponent<Rigidbody>().mass = ((Component)r.leftFoot).GetComponent<Rigidbody>().mass;
		}
	}

	private static void CreateJoints(BipedRagdollReferences r, Options o)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)r.spine == (Object)null)
		{
			o.spine = false;
		}
		if ((Object)(object)r.chest == (Object)null)
		{
			o.chest = false;
		}
		float minSwing = -30f * o.jointRange;
		float maxSwing = 10f * o.jointRange;
		float swing = 25f * o.jointRange;
		float twist = 25f * o.jointRange;
		RagdollCreator.CreateJoint(new CreateJointParams(((Component)r.hips).GetComponent<Rigidbody>(), null, o.spine ? r.spine : (o.chest ? r.chest : r.head), r.root.right, new CreateJointParams.Limits(0f, 0f, 0f, 0f), o.joints));
		if (o.spine)
		{
			RagdollCreator.CreateJoint(new CreateJointParams(((Component)r.spine).GetComponent<Rigidbody>(), ((Component)r.hips).GetComponent<Rigidbody>(), o.chest ? r.chest : r.head, r.root.right, new CreateJointParams.Limits(minSwing, maxSwing, swing, twist), o.joints));
		}
		if (o.chest)
		{
			RagdollCreator.CreateJoint(new CreateJointParams(((Component)r.chest).GetComponent<Rigidbody>(), o.spine ? ((Component)r.spine).GetComponent<Rigidbody>() : ((Component)r.hips).GetComponent<Rigidbody>(), r.head, r.root.right, new CreateJointParams.Limits(minSwing, maxSwing, swing, twist), o.joints));
		}
		Transform val = (o.chest ? r.chest : (o.spine ? r.spine : r.hips));
		RagdollCreator.CreateJoint(new CreateJointParams(((Component)r.head).GetComponent<Rigidbody>(), ((Component)val).GetComponent<Rigidbody>(), null, r.root.right, new CreateJointParams.Limits(-30f, 30f, 30f, 85f), o.joints));
		CreateJointParams.Limits limits = new CreateJointParams.Limits(-35f * o.jointRange, 120f * o.jointRange, 85f * o.jointRange, 45f * o.jointRange);
		CreateJointParams.Limits limits2 = new CreateJointParams.Limits(0f, 140f * o.jointRange, 10f * o.jointRange, 45f * o.jointRange);
		CreateJointParams.Limits limits3 = new CreateJointParams.Limits(-50f * o.jointRange, 50f * o.jointRange, 50f * o.jointRange, 25f * o.jointRange);
		CreateLimbJoints(val, r.leftUpperArm, r.leftLowerArm, r.leftHand, r.root, -r.root.right, o.joints, limits, limits2, limits3);
		CreateLimbJoints(val, r.rightUpperArm, r.rightLowerArm, r.rightHand, r.root, r.root.right, o.joints, limits, limits2, limits3);
		CreateJointParams.Limits limits4 = new CreateJointParams.Limits(-120f * o.jointRange, 35f * o.jointRange, 85f * o.jointRange, 45f * o.jointRange);
		CreateJointParams.Limits limits5 = new CreateJointParams.Limits(0f, 140f * o.jointRange, 10f * o.jointRange, 45f * o.jointRange);
		CreateJointParams.Limits limits6 = new CreateJointParams.Limits(-50f * o.jointRange, 50f * o.jointRange, 50f * o.jointRange, 25f * o.jointRange);
		CreateLimbJoints(r.hips, r.leftUpperLeg, r.leftLowerLeg, r.leftFoot, r.root, -r.root.up, o.joints, limits4, limits5, limits6);
		CreateLimbJoints(r.hips, r.rightUpperLeg, r.rightLowerLeg, r.rightFoot, r.root, -r.root.up, o.joints, limits4, limits5, limits6);
	}

	private static void CreateLimbJoints(Transform connectedBone, Transform bone1, Transform bone2, Transform bone3, Transform root, Vector3 defaultWorldDirection, JointType jointType, CreateJointParams.Limits limits1, CreateJointParams.Limits limits2, CreateJointParams.Limits limits3)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		Quaternion localRotation = bone1.localRotation;
		bone1.rotation = Quaternion.FromToRotation(bone1.rotation * (bone2.position - bone1.position), defaultWorldDirection) * bone1.rotation;
		Vector3 val = bone2.position - bone1.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = bone3.position - bone2.position;
		Vector3 normalized2 = ((Vector3)(ref val)).normalized;
		Vector3 worldSwingAxis = -Vector3.Cross(normalized, normalized2);
		float num = Vector3.Angle(normalized, normalized2);
		bool flag = Mathf.Abs(Vector3.Dot(normalized, root.up)) > 0.5f;
		float num2 = (flag ? 100f : 1f);
		if (num < 0.01f * num2)
		{
			worldSwingAxis = ((!flag) ? ((Vector3.Dot(normalized, root.right) > 0f) ? root.up : (-root.up)) : ((Vector3.Dot(normalized, root.up) > 0f) ? root.right : (-root.right)));
		}
		RagdollCreator.CreateJoint(new CreateJointParams(((Component)bone1).GetComponent<Rigidbody>(), ((Component)connectedBone).GetComponent<Rigidbody>(), bone2, worldSwingAxis, limits1, jointType));
		RagdollCreator.CreateJoint(new CreateJointParams(((Component)bone2).GetComponent<Rigidbody>(), ((Component)bone1).GetComponent<Rigidbody>(), bone3, worldSwingAxis, new CreateJointParams.Limits(limits2.minSwing - num, limits2.maxSwing - num, limits2.swing2, limits2.twist), jointType));
		if ((Object)(object)((Component)bone3).GetComponent<Rigidbody>() != (Object)null)
		{
			RagdollCreator.CreateJoint(new CreateJointParams(((Component)bone3).GetComponent<Rigidbody>(), ((Component)bone2).GetComponent<Rigidbody>(), null, worldSwingAxis, limits3, jointType));
		}
		bone1.localRotation = localRotation;
	}

	public static void ClearBipedRagdoll(BipedRagdollReferences r)
	{
		Transform[] ragdollTransforms = r.GetRagdollTransforms();
		Transform[] array = ragdollTransforms;
		foreach (Transform transform in array)
		{
			RagdollCreator.ClearTransform(transform);
		}
	}

	public static bool IsClear(BipedRagdollReferences r)
	{
		Transform[] ragdollTransforms = r.GetRagdollTransforms();
		Transform[] array = ragdollTransforms;
		foreach (Transform val in array)
		{
			if ((Object)(object)((Component)val).GetComponent<Rigidbody>() != (Object)null)
			{
				return false;
			}
		}
		return true;
	}

	private static Vector3 GetUpperArmToHeadCentroid(BipedRagdollReferences r)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(GetUpperArmCentroid(r), r.head.position, 0.5f);
	}

	private static Vector3 GetUpperArmCentroid(BipedRagdollReferences r)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(r.leftUpperArm.position, r.rightUpperArm.position, 0.5f);
	}
}
