public class GClass1335 : GClass1332<PlayerStateContainer, GClass1331>
{
	public override GClass1331 InternalCreate(PlayerStateContainer behaviour, GClass1312 state)
	{
		GClass1331 gClass = base.InternalCreate(behaviour, state);
		gClass.IsDefaultState = behaviour.IsDefaultState;
		gClass.Name = behaviour.Name;
		gClass.RotationSpeedClamp = behaviour.RotationSpeedClamp;
		gClass.StateSensitivity = behaviour.StateSensitivity;
		gClass.CanInteract = behaviour.CanInteract;
		gClass.DisableRootMotion = behaviour.DisableRootMotion;
		gClass.CreateUniqueMovementStateObject = behaviour.CreateUniqueMovementStateObject;
		gClass.AnimationAuthority = behaviour.AnimationAuthority;
		gClass.AuthoritySpeed = behaviour.AuthoritySpeed;
		gClass.StateFullName = behaviour.StateFullName;
		gClass.StateLength = behaviour.StateLength;
		gClass.Type = behaviour.Type;
		gClass.StateFullNameHash = behaviour.StateFullNameHash;
		return gClass;
	}
}
