using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1157
{
	[NonSerialized]
	public static Material Material_0_1;

	public static Material Material_0
	{
		get
		{
			if (Material_0_1 == null)
			{
				Material_0_1 = smethod_0();
			}
			return Material_0_1;
		}
	}

	public static void DrawLineWorld(List<Vector3> lineVertices, Color color)
	{
		if (lineVertices.Count > 1)
		{
			Material_0.SetPass(0);
			GL.Begin(1);
			GL.Color(color);
			for (int i = 0; i < lineVertices.Count - 1; i++)
			{
				Vector3 vector = lineVertices[i];
				Vector3 vector2 = lineVertices[i + 1];
				GL.Vertex3(vector.x, vector.y, vector.z);
				GL.Vertex3(vector2.x, vector2.y, vector2.z);
			}
			GL.End();
		}
	}

	public static void DrawLineWorld(Vector3 startPoint, Vector3 endPoint, Color color)
	{
		Material_0.SetPass(0);
		GL.Begin(1);
		GL.Color(color);
		GL.Vertex3(startPoint.x, startPoint.y, startPoint.z);
		GL.Vertex3(endPoint.x, endPoint.y, endPoint.z);
		GL.End();
	}

	public static void DrawWiredSphere(Vector3 position, float radius, Color color, int segments = 20)
	{
		Material_0.SetPass(0);
		GL.PushMatrix();
		GL.MultMatrix(Matrix4x4.Translate(position));
		GL.Begin(1);
		GL.Color(color);
		if (!(radius <= 0f) && segments > 0)
		{
			for (int i = 0; i <= segments; i++)
			{
				float f = MathF.PI * 2f * (float)i / (float)segments;
				float num = Mathf.Cos(f);
				float num2 = Mathf.Sin(f);
				for (int j = 0; j < segments; j++)
				{
					float f2 = MathF.PI * (float)j / (float)segments - MathF.PI / 2f;
					float f3 = MathF.PI * (float)(j + 1) / (float)segments - MathF.PI / 2f;
					float num3 = Mathf.Cos(f2);
					float y = Mathf.Sin(f2);
					float num4 = Mathf.Cos(f3);
					float y2 = Mathf.Sin(f3);
					Vector3 v = new Vector3(num3 * num, y, num3 * num2) * radius;
					Vector3 v2 = new Vector3(num4 * num, y2, num4 * num2) * radius;
					GL.Vertex(v);
					GL.Vertex(v2);
				}
			}
			for (int k = 0; k <= segments; k++)
			{
				float f4 = MathF.PI * (float)k / (float)segments - MathF.PI / 2f;
				float num5 = Mathf.Cos(f4);
				float y3 = Mathf.Sin(f4);
				for (int l = 0; l < segments; l++)
				{
					float f5 = MathF.PI * 2f * (float)l / (float)segments;
					float f6 = MathF.PI * 2f * (float)(l + 1) / (float)segments;
					float num6 = Mathf.Cos(f5);
					float num7 = Mathf.Sin(f5);
					float num8 = Mathf.Cos(f6);
					float num9 = Mathf.Sin(f6);
					Vector3 v3 = new Vector3(num5 * num6, y3, num5 * num7) * radius;
					Vector3 v4 = new Vector3(num5 * num8, y3, num5 * num9) * radius;
					GL.Vertex(v3);
					GL.Vertex(v4);
				}
			}
			GL.End();
			GL.PopMatrix();
		}
		else
		{
			GL.End();
			GL.PopMatrix();
			Debug.LogError($"Invalid params! Radius : {radius}, Segments : {segments}");
		}
	}

	public static Material smethod_0()
	{
		return new Material(Shader.Find("Hidden/Internal-Colored"))
		{
			color = Color.white
		};
	}
}
