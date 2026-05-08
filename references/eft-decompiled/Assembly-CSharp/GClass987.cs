using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass987
{
	public AnimationCurve ScaleCurve;

	public float Lifetime = 1f;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public Action<GClass987> Action_0;

	public static float _maxScale = 0.9f;

	public float _initialScale;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	public bool _isActive;

	public Vector2 Scale;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_1;

	[NonSerialized]
	[CompilerGenerated]
	public static bool Bool_1;

	public Vector3 CurrentScale
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public Vector3 CurrentPosition
	{
		[CompilerGenerated]
		get
		{
			return Vector3_1;
		}
		[CompilerGenerated]
		set
		{
			Vector3_1 = value;
		}
	}

	public static bool IsDropsShouldMove
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public GClass987(AnimationCurve scaleCurve, Action<GClass987> callback, Material dropMateral, float aspect)
	{
		ScaleCurve = scaleCurve;
		Action_0 = callback;
		Material_0 = dropMateral;
		Float_3 = 1f / aspect;
	}

	public void Use(float amount, float intensity, Vector3 localPosition)
	{
		_isActive = true;
		Float_0 = 0f;
		_initialScale = UnityEngine.Random.Range(Scale.x, Scale.y);
		Bool_0 = IsDropsShouldMove && UnityEngine.Random.Range(0.1f, 1f) < 0.35f;
		Float_1 = amount;
		Float_2 = intensity;
		CurrentPosition = localPosition;
	}

	public void UpdateDrop(float dt)
	{
		if (_isActive)
		{
			Vector2 vector = Vector2.one * ScaleCurve.Evaluate(Float_0) * _initialScale * Float_1 * Float_2 * 0.5f;
			CurrentScale = new Vector3(vector.x * Float_3, vector.y, 1f);
			Float_0 += dt / Lifetime;
			if (Bool_0 && Float_0 > 0.55f)
			{
				CurrentPosition += Vector3.up * ((0f - Time.deltaTime) / UnityEngine.Random.Range(2f, 5f));
				CurrentScale = new Vector3(CurrentScale.x * 0.75f, CurrentScale.y, 1f);
			}
			if (Float_0 > 1f && Action_0 != null && _isActive)
			{
				_isActive = false;
				Action_0(this);
			}
		}
	}

	public void Draw()
	{
		Material_0.SetPass(0);
		float x = CurrentPosition.x - CurrentScale.x;
		float x2 = CurrentPosition.x + CurrentScale.x;
		float y = 0f - CurrentPosition.y - CurrentScale.y;
		float y2 = 0f - CurrentPosition.y + CurrentScale.y;
		GL.Begin(7);
		Material_0.SetPass(0);
		GL.Begin(7);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(x, y, 0f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(x2, y, 0f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(x2, y2, 0f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(x, y2, 0f);
		GL.End();
	}
}
