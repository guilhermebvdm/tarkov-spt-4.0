using Audio.Vehicles;

public class Class777 : Class774
{
	public override EVehicleMovementStatus Status => EVehicleMovementStatus.Idle;

	public Class777(VehicleMovementSoundContext context)
		: base(context)
	{
	}

	public override void Update()
	{
		VehicleMovementSoundContext_0.method_9();
		base.Update();
	}
}
