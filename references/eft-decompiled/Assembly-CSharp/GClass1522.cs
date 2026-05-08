using Diz.LanguageExtensions;

public class GClass1522 : Error
{
	public readonly string Error;

	public GClass1522(string error)
	{
		Error = error;
	}

	public override string ToString()
	{
		return Error;
	}
}
