public abstract class GClass779
{
	public static void CopyFields(ICharacterController characterController1, ICharacterController characterController2)
	{
		characterController1.isEnabled = characterController2.isEnabled;
		characterController1.center = characterController2.center;
		characterController1.height = characterController2.height;
		characterController1.skinWidth = characterController2.skinWidth;
		characterController1.radius = characterController2.radius;
		characterController1.slopeLimit = characterController2.slopeLimit;
		characterController1.stepOffset = characterController2.stepOffset;
		characterController1.contactOffset = characterController2.contactOffset;
		characterController1.SpeedLimit = characterController2.SpeedLimit;
	}

	public static void CopyFields(ICharacterController characterController1, CharacterStruct footprint)
	{
		characterController1.isEnabled = footprint.isEnabled;
		characterController1.center = footprint.center;
		characterController1.height = footprint.height;
		characterController1.skinWidth = footprint.skinWidth;
		characterController1.radius = footprint.radius;
		characterController1.slopeLimit = footprint.slopeLimit;
		characterController1.stepOffset = footprint.stepOffset;
		characterController1.contactOffset = footprint.contactOffset;
	}

	public static CharacterStruct GetFootprint(ICharacterController characterController)
	{
		return new CharacterStruct
		{
			isEnabled = characterController.isEnabled,
			center = characterController.center,
			height = characterController.height,
			skinWidth = characterController.skinWidth,
			radius = characterController.radius,
			slopeLimit = characterController.slopeLimit,
			stepOffset = characterController.stepOffset,
			contactOffset = characterController.contactOffset
		};
	}
}
