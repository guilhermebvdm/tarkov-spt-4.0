using System;
using System.Collections.Generic;

public class GClass690
{
	public struct GStruct28(double width, double height, int calcAccuracy)
	{
		public double Width = width;

		public double Height = height;

		public double PowTwoWidth = width * width;

		public double PowTwoHeight = height * height;

		public double PowTwoEccentricity = 1.0 - PowTwoHeight / PowTwoWidth;

		public int CalcAccuracy = calcAccuracy;

		public double Length = 0.0;

		public double[] CachedLengths = new double[90];

		public void CalculateLengthCash()
		{
			for (int i = 0; i < 90; i++)
			{
				double angle = 0.01745329 * (double)(i + 1);
				double startAngle = 0.01745329 * (double)i;
				double curveLength = GetCurveLength(startAngle, angle);
				double num = ((i > 0) ? CachedLengths[i - 1] : 0.0);
				CachedLengths[i] = num + curveLength;
			}
			Length = CachedLengths[89] * 4.0;
		}

		public double GetCurveLength(double startAngle, double angle)
		{
			if (Math.Abs(startAngle - angle) < 1E-10)
			{
				return 0.0;
			}
			double num = Math.Cos(angle);
			double num2 = Math.Cos(startAngle);
			double num3 = Height / Math.Sqrt(1.0 - PowTwoEccentricity * (num * num));
			double num4 = Height / Math.Sqrt(1.0 - PowTwoEccentricity * (num2 * num2));
			double num5 = num3 * num;
			double num6 = num4 * num2;
			double num7 = Math.Abs(num5 - num6) / (double)CalcAccuracy;
			double num8 = num6;
			double num9 = Math.Sqrt(PowTwoHeight * (1.0 - num8 * num8 / PowTwoWidth));
			double num10 = 0.0;
			if (double.IsNaN(num9))
			{
				num9 = 0.0;
			}
			for (int i = 0; i < CalcAccuracy; i++)
			{
				int num11 = ((!(angle > Math.PI)) ? 1 : (-1));
				double num12 = num8 - num7 * (double)num11;
				double num13 = Math.Sqrt(PowTwoHeight * (1.0 - num12 * num12 / PowTwoWidth));
				double num14 = num8 - num12;
				double num15 = num9 - num13;
				num10 += Math.Sqrt(num14 * num14 + num15 * num15);
				num8 = num12;
				num9 = num13;
			}
			return num10;
		}

		public float GetAngle(float length)
		{
			if (Math.Abs(length) < 0.001f)
			{
				return 0f;
			}
			if (length < 0f)
			{
				throw new Exception("Length must be positive or zero");
			}
			float num = length / (float)CachedLengths[89];
			if (num < 1f)
			{
				return method_0(length);
			}
			if (num > 1f && num < 2f)
			{
				return 90f + (90f - method_0(2f * (float)CachedLengths[89] - length));
			}
			if (num > 2f && num < 3f)
			{
				return 180f + method_0(length - 2f * (float)CachedLengths[89]);
			}
			if (num > 3f && num < 4f)
			{
				return 270f + (90f - method_0(4f * (float)CachedLengths[89] - length));
			}
			return num * 90f;
		}

		public double GetCashLength(int angle)
		{
			if (angle == 0)
			{
				return 0.0;
			}
			if (angle < 0)
			{
				throw new Exception("Angle must be positive or zero");
			}
			float num = (float)angle / 90f;
			if (num < 1f)
			{
				return CachedLengths[angle - 1];
			}
			if (num > 1f && num < 2f)
			{
				return CachedLengths[89] + (CachedLengths[89] - CachedLengths[179 - angle]);
			}
			if (num > 2f && num < 3f)
			{
				return CachedLengths[89] * 2.0 + CachedLengths[angle - 180];
			}
			if (num > 3f && num < 4f)
			{
				return CachedLengths[89] * 3.0 + (CachedLengths[89] - CachedLengths[359 - angle]);
			}
			return (double)num * CachedLengths[89];
		}

		public float method_0(float length)
		{
			int num = 0;
			for (int i = 0; i < CachedLengths.Length; i++)
			{
				if (!(length - (float)CachedLengths[i] >= 0f))
				{
					num = i;
					break;
				}
			}
			int num2 = num - 1;
			double num3 = ((num2 < 0) ? 0.0 : CachedLengths[num2]);
			double num4 = CachedLengths[num] - num3;
			double num5 = ((double)length - num3) / num4;
			return (float)((double)num + num5);
		}
	}

	[NonSerialized]
	public List<GStruct28> List_0 = new List<GStruct28>(20);

	public float LengthFromAngle(double widthHalf, double heightHalf, float angle)
	{
		if (angle < 0f)
		{
			angle += 360f;
		}
		else if (angle > 360f)
		{
			angle -= 360f;
		}
		GStruct28 gStruct = method_0(widthHalf, heightHalf);
		int num = (int)Math.Floor(angle);
		double cashLength = gStruct.GetCashLength(num);
		double angle2 = 0.01745329 * (double)angle;
		double startAngle = 0.01745329 * (double)num;
		double curveLength = gStruct.GetCurveLength(startAngle, angle2);
		return (float)(cashLength + curveLength);
	}

	public float AngleFromLength(float widthHalf, float heightHalf, float length)
	{
		GStruct28 gStruct = method_0(widthHalf, heightHalf);
		if (length < 0f)
		{
			length += (float)gStruct.Length;
		}
		else if ((double)length > gStruct.Length)
		{
			length -= (float)gStruct.Length;
		}
		return gStruct.GetAngle(length);
	}

	public float GetLength(double widthHalf, double heightHalf)
	{
		return (float)method_0(widthHalf, heightHalf).Length;
	}

	public GStruct28 method_0(double widthHalf, double heightHalf)
	{
		GStruct28 gStruct = default(GStruct28);
		bool flag = false;
		for (int i = 0; i < List_0.Count; i++)
		{
			if ((int)List_0[i].Width == (int)widthHalf && (int)List_0[i].Height == (int)heightHalf)
			{
				gStruct = List_0[i];
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			gStruct = new GStruct28(widthHalf, heightHalf, 40);
			gStruct.CalculateLengthCash();
			List_0.Add(gStruct);
		}
		return gStruct;
	}
}
