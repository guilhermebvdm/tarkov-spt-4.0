using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class StaticDeferredDecal : MonoBehaviour
{
	[HideInInspector]
	public Vector2 LowerLeftPixel = Vector2.zero;

	[HideInInspector]
	public Vector2 UpperRightPixel = Vector2.zero;

	[SerializeField]
	public Material _material;

	[SerializeField]
	[Range(0f, 9f)]
	private int _decalQueue;

	[SerializeField]
	[Range(-1f, 1f)]
	private float _normalClip = 0.8f;

	[SerializeField]
	private float _normalAlphaFadeoff = 10f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _edgeClip;

	[SerializeField]
	private float _edgeAlphaFadeoff = 1f;

	[SerializeField]
	private Color _tint = Color.white;

	private Action action_0;

	private Action action_1;

	private Vector3 vector3_0;

	private Quaternion quaternion_0;

	private Vector3 vector3_1;

	[CompilerGenerated]
	private static Action<StaticDeferredDecal, bool> action_2;

	[CompilerGenerated]
	private static Action<StaticDeferredDecal, bool> action_3;

	private static readonly int int_0 = Shader.PropertyToID("_MainTex");

	private static readonly int int_1 = Shader.PropertyToID("uvStartEnd");

	private static readonly int int_2 = Shader.PropertyToID("tint");

	[CompilerGenerated]
	private Texture2D texture2D_0;

	[HideInInspector]
	public Vector2 LastTexSize = Vector2.zero;

	public int DecalQueue => _decalQueue;

	public Material DecalMaterial
	{
		get
		{
			return _material;
		}
		set
		{
			_material = value;
		}
	}

	public Texture2D Tex
	{
		[CompilerGenerated]
		get
		{
			return texture2D_0;
		}
		[CompilerGenerated]
		set
		{
			texture2D_0 = value;
		}
	}

	public Color Tint => _tint;

	public static event Action<StaticDeferredDecal, bool> OnDecalRegister
	{
		[CompilerGenerated]
		add
		{
			Action<StaticDeferredDecal, bool> action = action_2;
			Action<StaticDeferredDecal, bool> action2;
			do
			{
				action2 = action;
				Action<StaticDeferredDecal, bool> value2 = (Action<StaticDeferredDecal, bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<StaticDeferredDecal, bool> action = action_2;
			Action<StaticDeferredDecal, bool> action2;
			do
			{
				action2 = action;
				Action<StaticDeferredDecal, bool> value2 = (Action<StaticDeferredDecal, bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static event Action<StaticDeferredDecal, bool> OnDecalUnregister
	{
		[CompilerGenerated]
		add
		{
			Action<StaticDeferredDecal, bool> action = action_3;
			Action<StaticDeferredDecal, bool> action2;
			do
			{
				action2 = action;
				Action<StaticDeferredDecal, bool> value2 = (Action<StaticDeferredDecal, bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<StaticDeferredDecal, bool> action = action_3;
			Action<StaticDeferredDecal, bool> action2;
			do
			{
				action2 = action;
				Action<StaticDeferredDecal, bool> value2 = (Action<StaticDeferredDecal, bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		if (_material == null)
		{
			base.enabled = false;
			return;
		}
		method_0();
		if ((double)(UpperRightPixel - LowerLeftPixel).sqrMagnitude < 0.0001)
		{
			UpperRightPixel = new Vector2(Tex.width - 1, Tex.height - 1);
			LowerLeftPixel = Vector2.zero;
		}
	}

	public void method_0()
	{
		Tex = _material.GetTexture(int_0) as Texture2D;
	}

	public Vector4 GetUVStartEnd()
	{
		if (Tex == null)
		{
			method_0();
		}
		if (Tex == null)
		{
			return Vector4.zero;
		}
		int width = Tex.width;
		int height = Tex.height;
		return new Vector4(LowerLeftPixel.x / (float)width, LowerLeftPixel.y / (float)height, UpperRightPixel.x / (float)width, UpperRightPixel.y / (float)height);
	}

	public Vector4 GetNormalEdgeClipAndFadeoff()
	{
		return new Vector4(_normalClip, _normalAlphaFadeoff, _edgeClip * 0.5f, _edgeAlphaFadeoff);
	}

	public void OnEnable()
	{
		action_2?.Invoke(this, arg2: true);
	}

	public void OnDisable()
	{
		action_3?.Invoke(this, arg2: true);
	}

	public void Refresh()
	{
		Awake();
	}
}
