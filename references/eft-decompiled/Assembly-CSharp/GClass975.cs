using UnityEngine;

public abstract class GClass975
{
	public static void smethod_0(ref int value, int size)
	{
		value %= size;
		if (value < 0)
		{
			value += size;
		}
	}

	public static void smethod_1(ref int x, ref int y, ref int z, int size)
	{
		smethod_0(ref x, size);
		smethod_0(ref y, size);
		smethod_0(ref z, size);
	}

	public static float smethod_2(int x, int y, int z)
	{
		int num = x + y * 31 + z * 41;
		num = (num << 13) ^ num;
		return 1f - (float)((num * (num * num * 15731 + 789221) + 1376312589) & 0x7FFFFFFF) / 1.0737418E+09f;
	}

	public static float smethod_3(float x)
	{
		return x * x * (3f - 2f * x);
	}

	public static float smethod_4(float x, float y, float z, int size)
	{
		int x2 = (int)x;
		int y2 = (int)y;
		int z2 = (int)z;
		int x3 = x2 + 1;
		int y3 = y2 + 1;
		int z3 = z2 + 1;
		smethod_1(ref x2, ref y2, ref z2, size);
		smethod_1(ref x3, ref y3, ref z3, size);
		float t = smethod_3(x - (float)x2);
		float t2 = smethod_3(y - (float)y2);
		float t3 = smethod_3(z - (float)z2);
		float a = Mathf.Lerp(smethod_2(x2, y2, z2), smethod_2(x3, y2, z2), t);
		float a2 = Mathf.Lerp(smethod_2(x2, y2, z3), smethod_2(x3, y2, z3), t);
		float b = Mathf.Lerp(smethod_2(x2, y3, z2), smethod_2(x3, y3, z2), t);
		float b2 = Mathf.Lerp(smethod_2(x2, y3, z3), smethod_2(x3, y3, z3), t);
		float a3 = Mathf.Lerp(a, b, t2);
		float b3 = Mathf.Lerp(a2, b2, t2);
		return Mathf.Lerp(a3, b3, t3);
	}

	public static float[,,] smethod_5(int bSize, float roughness)
	{
		int num = 1 << bSize;
		float[,,] array = new float[num, num, num];
		float[] array2 = new float[bSize];
		int[] array3 = new int[bSize];
		float[] array4 = new float[bSize];
		float num2 = 0f;
		for (int i = 0; i < bSize; i++)
		{
			array3[i] = num / (1 << i);
			array2[i] = 1f / (float)array3[i];
			array4[i] = Mathf.Pow(roughness, bSize - i);
			num2 += array4[i];
		}
		num2 = 1f / num2;
		for (int j = 0; j < bSize; j++)
		{
			array4[j] *= num2;
		}
		for (int k = 0; k < num; k++)
		{
			for (int l = 0; l < num; l++)
			{
				for (int m = 0; m < num; m++)
				{
					float num3 = 0f;
					for (int n = 0; n < bSize; n++)
					{
						num3 += smethod_4((float)m * array2[n], (float)l * array2[n], (float)k * array2[n], array3[n]) * array4[n];
					}
					num3 = num3 * 0.5f + 0.5f;
					array[m, l, k] = num3;
				}
			}
		}
		return array;
	}

	public static Texture3D smethod_6(float[,,] map, int size)
	{
		Color32[] array = new Color32[size * size * size];
		int i = 0;
		int num = 0;
		for (; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				int num2 = 0;
				while (num2 < size)
				{
					byte b = (byte)(map[num2, j, i] * 256f);
					array[num] = new Color32(b, b, b, b);
					num2++;
					num++;
				}
			}
		}
		Texture3D texture3D = new Texture3D(size, size, size, TextureFormat.Alpha8, mipChain: false);
		texture3D.SetPixels32(array);
		texture3D.Apply();
		return texture3D;
	}

	public static Texture3D GetNoiseTexture3D(int bSize, float roughness)
	{
		return smethod_6(smethod_5(bSize, roughness), 1 << bSize);
	}
}
