using Audio.Vehicles;

public class Class775 : Class774
{
	public override EVehicleMovementStatus Status => EVehicleMovementStatus.Started;

	public Class775(VehicleMovementSoundContext context)
		: base(context)
	{
	}

	public override EVehicleMovementStatus Run()
	{
		VehicleMovementSoundContext_0.method_2();
		return VehicleMovementSoundContext_0.method_7(EVehicleMovementStatus.Idle);
	}
}
