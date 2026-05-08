using System;
using System.Text;

public abstract class GClass1371
{
	[NonSerialized]
	public const string String_0 = "0123456789ABCDEF";

	[NonSerialized]
	public static int[] Int_0 = new int[23]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		0, 0, 0, 0, 0, 0, 0, 10, 11, 12,
		13, 14, 15
	};

	public static string ToHexString(this byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
		foreach (byte b in bytes)
		{
			stringBuilder.Append("0123456789ABCDEF"[b >> 4]);
			stringBuilder.Append("0123456789ABCDEF"[b & 0xF]);
		}
		return stringBuilder.ToString();
	}

	public static byte[] FromHexString(this string hex)
	{
		byte[] array = new byte[hex.Length / 2];
		int num = 0;
		int num2 = 0;
		while (num2 < hex.Length)
		{
			array[num] = (byte)((Int_0[char.ToUpper(hex[num2]) - 48] << 4) | Int_0[char.ToUpper(hex[num2 + 1]) - 48]);
			num2 += 2;
			num++;
		}
		return array;
	}
}
