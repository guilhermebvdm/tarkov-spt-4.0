using Audio.Vehicles;

public class Class778 : Class774
{
	public override EVehicleMovementStatus Status => EVehicleMovementStatus.Running;

	public Class778(VehicleMovementSoundContext context)
		: base(context)
	{
	}

	public override EVehicleMovementStatus Run()
	{
		VehicleMovementSoundContext_0.method_3();
		return base.Run();
	}

	public override void Update()
	{
		VehicleMovementSoundContext_0.method_4();
		VehicleMovementSoundContext_0.method_9();
		VehicleMovementSoundContext_0.method_6();
		base.Update();
	}

	public override void Exit()
	{
		VehicleMovementSoundContext_0.method_5();
		base.Exit();
	}
}
