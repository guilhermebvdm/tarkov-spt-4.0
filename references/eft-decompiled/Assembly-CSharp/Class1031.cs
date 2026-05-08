using System;

public class Class1031 : Interface5
{
	[NonSerialized]
	public Func<Struct263, bool> Func_0;

	public Class1031(Func<Struct263, bool> condition)
	{
		Func_0 = condition;
	}

	public bool CanExecute(Struct263 continuation)
	{
		return Func_0(continuation);
	}

	public void Execute(ref Struct263 continuation)
	{
		continuation.Execute();
	}
}
