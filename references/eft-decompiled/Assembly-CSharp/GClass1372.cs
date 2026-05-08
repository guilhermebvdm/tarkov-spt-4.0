using System.Diagnostics;

public abstract class GClass1372
{
	[Conditional("SERIALIZATION_VALIDATION")]
	public static void DefineDependedCheck(this ISerializer stream, uint checkNumber, string message = null)
	{
		stream.SerializeCheck(checkNumber, message);
	}
}
