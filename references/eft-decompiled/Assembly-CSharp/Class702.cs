using System;

public abstract class Class702
{
	public static bool ContainsKeyword(this Array array, string kwrd)
	{
		foreach (string item in array)
		{
			if (item == kwrd)
			{
				return true;
			}
		}
		return false;
	}
}
