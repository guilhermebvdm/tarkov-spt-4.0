public struct GStruct17
{
	public bool Inited;

	public int HealthHead;

	public int HealthBody;

	public int HealthStomach;

	public int HealthLeftArm;

	public int HealthRightArm;

	public int HealthLeftLeg;

	public int HealthRightLeg;

	public int MaxHealthHead;

	public int MaxHealthBody;

	public int MaxHealthStomach;

	public int MaxHealthLeftArm;

	public int MaxHealthRightArm;

	public int MaxHealthLeftLeg;

	public int MaxHealthRightLeg;

	public bool HealthHeadBL;

	public bool HealthBodyBL;

	public bool HealthStomachBL;

	public bool HealthLeftArmBL;

	public bool HealthRightArmBL;

	public bool HealthLeftLegBL;

	public bool HealthRightLegBL;

	public bool HealthHeadBroken;

	public bool HealthBodyBroken;

	public bool HealthStomachBroken;

	public bool HealthLeftArmBroken;

	public bool HealthRightArmBroken;

	public bool HealthLeftLegBroken;

	public bool HealthRightLegBroken;

	public uint EffectsUint;

	public float Energy;

	public float Hydration;

	public EDebugBotEffect Effects
	{
		get
		{
			return (EDebugBotEffect)EffectsUint;
		}
		set
		{
			EffectsUint = (uint)value;
		}
	}
}
