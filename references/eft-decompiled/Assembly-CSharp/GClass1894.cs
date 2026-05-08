using EFT;

public abstract class GClass1894
{
	public static float ToTimeFlow(this ETimeFlowType timeFlowType)
	{
		return timeFlowType switch
		{
			ETimeFlowType.x0 => 0f, 
			ETimeFlowType.x0_14 => 1f, 
			ETimeFlowType.x0_25 => 1.75f, 
			ETimeFlowType.x0_5 => 3.5f, 
			ETimeFlowType.x1 => 7f, 
			ETimeFlowType.x2 => 14f, 
			ETimeFlowType.x4 => 28f, 
			ETimeFlowType.x8 => 56f, 
			_ => 1f, 
		};
	}
}
