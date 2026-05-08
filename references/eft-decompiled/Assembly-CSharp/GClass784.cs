using EFT;

public abstract class GClass784
{
	public static bool IsProne(this PlayerSpiritBones.PoseType poseType)
	{
		if (poseType != PlayerSpiritBones.PoseType.Prone && poseType != PlayerSpiritBones.PoseType.ProneSideStepLeft && poseType != PlayerSpiritBones.PoseType.ProneSideStepRight && poseType != PlayerSpiritBones.PoseType.ProneTiltLeft)
		{
			return poseType == PlayerSpiritBones.PoseType.ProneTiltRight;
		}
		return true;
	}

	public static bool IsTilt(this PlayerSpiritBones.PoseType poseType)
	{
		if (poseType != PlayerSpiritBones.PoseType.TiltLeft && poseType != PlayerSpiritBones.PoseType.TiltRight && poseType != PlayerSpiritBones.PoseType.CrouchTiltLeft && poseType != PlayerSpiritBones.PoseType.CrouchTiltRight && poseType != PlayerSpiritBones.PoseType.ProneTiltLeft)
		{
			return poseType == PlayerSpiritBones.PoseType.ProneTiltRight;
		}
		return true;
	}

	public static bool IsTiltRight(this PlayerSpiritBones.PoseType poseType)
	{
		if (poseType != PlayerSpiritBones.PoseType.TiltRight && poseType != PlayerSpiritBones.PoseType.CrouchTiltRight)
		{
			return poseType == PlayerSpiritBones.PoseType.ProneTiltRight;
		}
		return true;
	}
}
