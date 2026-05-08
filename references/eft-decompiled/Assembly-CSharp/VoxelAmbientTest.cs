using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VoxelAmbientTest : MonoBehaviour
{
	public class Class620
	{
		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public int Int_1;

		[NonSerialized]
		public int Int_2;

		[NonSerialized]
		public int Int_3;

		public Class620(int pixelsX, int pixelsY, int pixelsZ)
		{
			Int_0 = pixelsX;
			Int_1 = pixelsY;
			Int_2 = pixelsZ;
			Int_3 = Int_0 * Int_1;
		}

		public bool[] GetObstacles(Transform gridPosition)
		{
			bool[] array = new bool[Int_0 * Int_1 * Int_2];
			Vector3 b = new Vector3(1f / (float)Int_0, 1f / (float)Int_1, 1f / (float)Int_2);
			Vector3 lossyScale = gridPosition.lossyScale;
			Vector3 halfExtents = new Vector3(lossyScale.x / (float)Int_0, lossyScale.y / (float)Int_1, lossyScale.z / (float)Int_2) * 0.5f;
			Quaternion rotation = gridPosition.rotation;
			int i = 0;
			int num = 0;
			for (; i < Int_2; i++)
			{
				for (int j = 0; j < Int_1; j++)
				{
					int num2 = 0;
					while (num2 < Int_0)
					{
						Vector3 position = Vector3.Scale(new Vector3((float)num2 + 0.5f, (float)j + 0.5f, (float)i + 0.5f), b);
						Vector3 center = gridPosition.TransformPoint(position);
						array[num] = Physics.CheckBox(center, halfExtents, rotation);
						num2++;
						num++;
					}
				}
			}
			return array;
		}

		public LinkedList<int> GetSunLight(bool[] obstacles)
		{
			int x = Int_0 - 1;
			int num = Int_1 - 1;
			int z = Int_2 - 1;
			HashSet<int> hashSet = new HashSet<int>();
			for (int i = 0; i < Int_0; i++)
			{
				for (int j = 0; j < Int_2; j++)
				{
					for (int num2 = num; num2 >= 0; num2--)
					{
						int num3 = method_0(i, num2, j);
						if (obstacles[num3])
						{
							break;
						}
						hashSet.Add(num3);
					}
				}
			}
			for (int k = 0; k < Int_0; k++)
			{
				for (int l = 0; l < Int_1; l++)
				{
					int num4 = method_0(k, l, 0);
					int num5 = method_0(k, l, z);
					if (!obstacles[num4])
					{
						hashSet.Add(num4);
					}
					if (!obstacles[num5])
					{
						hashSet.Add(num5);
					}
				}
			}
			for (int m = 0; m < Int_0; m++)
			{
				for (int n = 0; n < Int_2; n++)
				{
					int num6 = method_0(m, 0, n);
					int num7 = method_0(m, num, n);
					if (!obstacles[num6])
					{
						hashSet.Add(num6);
					}
					if (!obstacles[num7])
					{
						hashSet.Add(num7);
					}
				}
			}
			for (int num8 = 0; num8 < Int_1; num8++)
			{
				for (int num9 = 0; num9 < Int_2; num9++)
				{
					int num10 = method_0(0, num8, num9);
					int num11 = method_0(x, num8, num9);
					if (!obstacles[num10])
					{
						hashSet.Add(num10);
					}
					if (!obstacles[num11])
					{
						hashSet.Add(num11);
					}
				}
			}
			return new LinkedList<int>(hashSet);
		}

		public float[] GetLight(bool[] obstacles, LinkedList<int> lights, int[][] axes, float[] intensities)
		{
			int num = Int_0 - 1;
			int num2 = Int_1 - 1;
			int num3 = Int_2 - 1;
			float[] array = new float[obstacles.Length];
			bool[] array2 = new bool[obstacles.Length];
			foreach (int light in lights)
			{
				array[light] = 1f;
				array2[light] = true;
			}
			List<int> list = new List<int>(26);
			for (LinkedListNode<int> first = lights.First; first != null; first = lights.First)
			{
				int value = first.Value;
				lights.Remove(first);
				method_1(value, out var x, out var y, out var z);
				int num4 = ((x == 0) ? (-1) : ((x == num) ? 1 : 666));
				int num5 = ((y == 0) ? (-1) : ((y == num2) ? 1 : 666));
				int num6 = ((z == 0) ? (-1) : ((z == num3) ? 1 : 666));
				float num7 = 0f;
				list.Clear();
				for (int i = 0; i < 26; i++)
				{
					int[] array3 = axes[i];
					if (array3[0] == num4 || array3[1] == num5 || array3[2] == num6)
					{
						continue;
					}
					int num8 = method_0(x + array3[0], y + array3[1], z + array3[2]);
					if (!obstacles[num8])
					{
						if (!array2[num8])
						{
							array2[num8] = true;
							list.Add(num8);
						}
						else
						{
							num7 = Math.Max(num7, array[num8] * (1f - intensities[i]));
						}
					}
				}
				if (num7 > 0.0001f)
				{
					foreach (int item in list)
					{
						lights.AddLast(item);
					}
					array[value] = num7;
				}
			}
			return array;
		}

		public Color[] GetDirected(float[] array, int[][] axes, Vector3[] vectors, float NormalContrast)
		{
			Color[] array2 = new Color[Int_0 * Int_1 * Int_2];
			int num = Int_0 - 1;
			int num2 = Int_1 - 1;
			int num3 = Int_2 - 1;
			for (int i = 0; i < array.Length; i++)
			{
				method_1(i, out var x, out var y, out var z);
				int num4 = ((x == 0) ? (-1) : ((x == num) ? 1 : 666));
				int num5 = ((y == 0) ? (-1) : ((y == num2) ? 1 : 666));
				int num6 = ((z == 0) ? (-1) : ((z == num3) ? 1 : 666));
				float num7 = array[i];
				int num8 = 0;
				Vector3 vector = default(Vector3);
				for (int j = 0; j < 26; j++)
				{
					int[] array3 = axes[j];
					if (array3[0] != num4 && array3[1] != num5 && array3[2] != num6)
					{
						int num9 = method_0(x + array3[0], y + array3[1], z + array3[2]);
						float num10 = array[num9] - num7;
						num10 = ((!(num10 > 0f)) ? (0f - Mathf.Sqrt(0f - num10)) : Mathf.Sqrt(num10));
						vector += vectors[j] * num10;
						num8++;
					}
				}
				vector /= (float)num8;
				vector *= NormalContrast;
				array2[i] = new Color(vector.x * 0.5f + 0.5f, vector.y * 0.5f + 0.5f, vector.z * 0.5f + 0.5f, num7);
			}
			return array2;
		}

		public Color[] BlurImage(Color[] pixels, int size)
		{
			if (size < 1)
			{
				return pixels;
			}
			Color[] array = new Color[Int_0 * Int_1 * Int_2];
			int i = 0;
			int num = 0;
			for (; i < Int_2; i++)
			{
				for (int j = 0; j < Int_1; j++)
				{
					int num2 = 0;
					while (num2 < Int_0)
					{
						Color color = pixels[num];
						int num3 = 1;
						for (int k = 1; k < size; k++)
						{
							int num4 = num2 + k;
							if (num4 >= Int_0)
							{
								break;
							}
							color += pixels[method_0(num4, j, i)];
							num3++;
						}
						for (int l = 1; l < size; l++)
						{
							int num5 = num2 - l;
							if (num5 < 0)
							{
								break;
							}
							color += pixels[method_0(num5, j, i)];
							num3++;
						}
						array[num] = color / num3;
						num2++;
						num++;
					}
				}
			}
			int m = 0;
			int num6 = 0;
			for (; m < Int_2; m++)
			{
				for (int n = 0; n < Int_1; n++)
				{
					int num7 = 0;
					while (num7 < Int_0)
					{
						Color color2 = array[num6];
						int num8 = 1;
						for (int num9 = 1; num9 < size; num9++)
						{
							int num10 = n + num9;
							if (num10 >= Int_1)
							{
								break;
							}
							color2 += array[method_0(num7, num10, m)];
							num8++;
						}
						for (int num11 = 1; num11 < size; num11++)
						{
							int num12 = n - num11;
							if (num12 < 0)
							{
								break;
							}
							color2 += array[method_0(num7, num12, m)];
							num8++;
						}
						pixels[num6] = color2 / num8;
						num7++;
						num6++;
					}
				}
			}
			int num13 = 0;
			int num14 = 0;
			for (; num13 < Int_2; num13++)
			{
				for (int num15 = 0; num15 < Int_1; num15++)
				{
					int num16 = 0;
					while (num16 < Int_0)
					{
						Color color3 = pixels[num14];
						int num17 = 1;
						for (int num18 = 1; num18 < size; num18++)
						{
							int num19 = num13 + num18;
							if (num19 >= Int_2)
							{
								break;
							}
							color3 += pixels[method_0(num16, num15, num19)];
							num17++;
						}
						for (int num20 = 1; num20 < size; num20++)
						{
							int num21 = num13 - num20;
							if (num21 < 0)
							{
								break;
							}
							color3 += pixels[method_0(num16, num15, num21)];
							num17++;
						}
						array[num14] = color3 / num17;
						num16++;
						num14++;
					}
				}
			}
			return array;
		}

		public Color[] Downsample(Color[] pixels, int samples, out Class620 im)
		{
			im = new Class620(Int_0 >> samples, Int_1 >> samples, Int_2 >> samples);
			if (samples < 1)
			{
				return pixels;
			}
			Color[] array = new Color[im.Int_0 * im.Int_1 * im.Int_2];
			int i = 0;
			int num = 0;
			for (; i < Int_2; i++)
			{
				for (int j = 0; j < Int_1; j++)
				{
					int num2 = 0;
					while (num2 < Int_0)
					{
						int num3 = im.method_0(num2 >> samples, j >> samples, i >> samples);
						Color color = array[num3];
						color += pixels[num];
						array[num3] = color;
						num2++;
						num++;
					}
				}
			}
			float num4 = 1f / Mathf.Pow(1 << samples, 3f);
			for (int k = 0; k < array.Length; k++)
			{
				array[k] *= num4;
			}
			return array;
		}

		public int method_0(int x, int y, int z)
		{
			return x + Int_0 * y + Int_3 * z;
		}

		public void method_1(int id, out int x, out int y, out int z)
		{
			z = Math.DivRem(id, Int_3, out var result);
			y = Math.DivRem(result, Int_0, out result);
			x = result;
		}

		public Texture3D GetTex(Color[] pixels)
		{
			Texture3D texture3D = new Texture3D(Int_0, Int_1, Int_2, TextureFormat.ARGB32, mipChain: false);
			texture3D.SetPixels(pixels);
			texture3D.Apply(updateMipmaps: false);
			return texture3D;
		}
	}

	public int PixelsX;

	public int PixelsY;

	public int PixelsZ;

	[Space]
	public int Upsample;

	[Space]
	public float LightRange = 1f;

	public float NormalsContrast = 1f;

	public float WallLightness;

	[Space]
	public Transform GridPosition;

	public Material Material;

	public Texture3D Tex;

	public Vector4 DebugVec;

	public bool DebugDrawGrid;

	private static readonly int int_0 = Shader.PropertyToID("_MainTex");

	public void OnEnable()
	{
		Class620 @class = new Class620(PixelsX << Upsample, PixelsY << Upsample, PixelsZ << Upsample);
		bool[] obstacles = @class.GetObstacles(GridPosition);
		LinkedList<int> sunLight = @class.GetSunLight(obstacles);
		method_1(LightRange, out var axes, out var intensities, out var vectors);
		float[] light = @class.GetLight(obstacles, sunLight, axes, intensities);
		Color[] directed = @class.GetDirected(light, axes, vectors, NormalsContrast);
		for (int i = 0; i < obstacles.Length; i++)
		{
			if (obstacles[i])
			{
				directed[i].a = WallLightness;
			}
		}
		directed = @class.Downsample(directed, Upsample, out var im);
		Texture3D tex = im.GetTex(directed);
		if (Material != null)
		{
			Material.SetTexture(int_0, tex);
		}
		Tex = tex;
	}

	public void OnValidate()
	{
	}

	public void OnDrawGizmosSelected()
	{
		if (DebugDrawGrid)
		{
			Gizmos.matrix = GridPosition.localToWorldMatrix;
			Vector3 vector = new Vector3(1f / (float)PixelsX, 1f / (float)PixelsY, 1f / (float)PixelsZ);
			Vector3 vector2 = vector * 0.5f;
			for (int i = 0; i < PixelsX; i += 2)
			{
				Gizmos.DrawWireCube(new Vector3((float)i * vector.x + vector2.x, 0.5f, 0.5f), new Vector3(vector.x, 1f, 1f));
			}
			for (int j = 0; j < PixelsY; j += 2)
			{
				Gizmos.DrawWireCube(new Vector3(0.5f, (float)j * vector.y + vector2.y, 0.5f), new Vector3(1f, vector.y, 1f));
			}
			for (int k = 0; k < PixelsZ; k += 2)
			{
				Gizmos.DrawWireCube(new Vector3(0.5f, 0.5f, (float)k * vector.z + vector2.z), new Vector3(1f, 1f, vector.z));
			}
		}
	}

	public bool[] method_0()
	{
		if (GridPosition == null)
		{
			return null;
		}
		bool[] array = new bool[PixelsX * PixelsY * PixelsZ];
		Vector3 b = new Vector3(1f / (float)PixelsX, 1f / (float)PixelsY, 1f / (float)PixelsZ);
		Vector3 lossyScale = GridPosition.lossyScale;
		Vector3 halfExtents = new Vector3(lossyScale.x / (float)PixelsX, lossyScale.y / (float)PixelsY, lossyScale.z / (float)PixelsZ) * 0.5f;
		Quaternion rotation = GridPosition.rotation;
		int i = 0;
		int num = 0;
		for (; i < PixelsZ; i++)
		{
			for (int j = 0; j < PixelsY; j++)
			{
				int num2 = 0;
				while (num2 < PixelsX)
				{
					Vector3 position = Vector3.Scale(new Vector3((float)num2 + 0.5f, (float)j + 0.5f, (float)i + 0.5f), b);
					Vector3 center = GridPosition.TransformPoint(position);
					array[num] = Physics.CheckBox(center, halfExtents, rotation);
					num2++;
					num++;
				}
			}
		}
		return array;
	}

	public void method_1(float lightRange, out int[][] axes, out float[] intensities, out Vector3[] vectors)
	{
		axes = new int[26][];
		intensities = new float[26];
		vectors = new Vector3[26];
		if (GridPosition == null)
		{
			return;
		}
		Vector3 lossyScale = GridPosition.lossyScale;
		int i = -1;
		int num = 0;
		for (; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				for (int k = -1; k <= 1; k++)
				{
					if (i != 0 || j != 0 || k != 0)
					{
						axes[num] = new int[3] { i, j, k };
						Vector3 vector = Vector3.Scale(new Vector3(i, j, k), lossyScale);
						intensities[num] = vector.magnitude / lightRange;
						float magnitude = vector.magnitude;
						vectors[num] = vector / (magnitude * magnitude);
						num++;
					}
				}
			}
		}
	}
}
