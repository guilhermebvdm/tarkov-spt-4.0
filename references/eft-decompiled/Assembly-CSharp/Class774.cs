using System;
using Audio.Vehicles;

public abstract class Class774
{
	[NonSerialized]
	public VehicleMovementSoundContext VehicleMovementSoundContext_0;

	public abstract EVehicleMovementStatus Status { get; }

	public bool IsCurrent => Status == VehicleMovementSoundContext_0.CurrentStatus;

	public Class774(VehicleMovementSoundContext context)
	{
		VehicleMovementSoundContext_0 = context;
	}

	public virtual EVehicleMovementStatus Run()
	{
		return Status;
	}

	public virtual void Update()
	{
	}

	public virtual void Exit()
	{
	}
}
