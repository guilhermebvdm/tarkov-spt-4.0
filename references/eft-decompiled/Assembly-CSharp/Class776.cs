using Audio.Vehicles;

public class Class776 : Class774
{
	public override EVehicleMovementStatus Status => EVehicleMovementStatus.Stopped;

	public Class776(VehicleMovementSoundContext context)
		: base(context)
	{
	}

	public override EVehicleMovementStatus Run()
	{
		VehicleMovementSoundContext_0.method_11();
		return base.Run();
	}
}
