using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass956
{
	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public Vector2 Vector2_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public static int Int_0 = Shader.PropertyToID("_Lifetime");

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	public float Float_0;

	public int DropNumber
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public GClass956(Material material, float aspect)
	{
		Material_0 = new Material(material);
		Float_0 = aspect;
	}

	public void Setup(Vector2 position, Vector3 scale, int dropNumber)
	{
		DropNumber = dropNumber;
		Vector2_0 = position;
		Vector3_0 = scale;
	}

	public void SetLifetime(float lifetime)
	{
		Material_0.SetFloat(Int_0, lifetime);
	}

	public void Draw()
	{
		float x = 0f - Vector3_0.x;
		float x2 = Vector3_0.x;
		float y = 0f - Vector3_0.y;
		float y2 = Vector3_0.y;
		Vector2 vector = new Vector2(x, y);
		Vector2 vector2 = new Vector2(x2, y);
		Vector2 vector3 = new Vector2(x2, y2);
		Vector2 vector4 = new Vector2(x, y2);
		Vector2_0.y *= -1f;
		vector += Vector2_0;
		vector2 += Vector2_0;
		vector3 += Vector2_0;
		vector4 += Vector2_0;
		vector.y *= Float_0;
		vector2.y *= Float_0;
		vector3.y *= Float_0;
		vector4.y *= Float_0;
		Material_0.SetPass(0);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(vector.x, vector.y, 0f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(vector2.x, vector2.y, 0f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(vector3.x, vector3.y, 0f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(vector4.x, vector4.y, 0f);
		GL.End();
	}
}
