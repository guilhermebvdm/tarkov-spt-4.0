using AnimationEventSystem;

public abstract class GClass1453
{
	public static string GetCSharpParamName(this EAnimationEventParamType paramType)
	{
		return paramType switch
		{
			EAnimationEventParamType.None => null, 
			EAnimationEventParamType.Boolean => "bool", 
			EAnimationEventParamType.Int32 => "int", 
			EAnimationEventParamType.Float => "float", 
			EAnimationEventParamType.String => "string", 
			_ => null, 
		};
	}
}
