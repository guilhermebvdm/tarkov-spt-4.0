using System;

public abstract class GClass8
{
	public static void Shuffle<T>(T[] array)
	{
		Shuffle(new Random(), array);
	}

	public static void Shuffle<T>(this Random rng, T[] array)
	{
		int num = array.Length;
		while (num > 1)
		{
			int num2 = rng.Next(num--);
			int num3 = num;
			int num4 = num2;
			T val = array[num2];
			T val2 = array[num];
			array[num3] = val;
			array[num4] = val2;
		}
	}
}
