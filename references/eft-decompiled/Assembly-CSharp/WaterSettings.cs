using System;
using EFT.Weather;
using UnityEngine;

[Serializable]
public class WaterSettings
{
	[Serializable]
	public class TextureBlendSetting
	{
		public float Scale = 0.1f;

		public Vector2 MovementDirection = Vector2.one;

		[Range(0f, 1f)]
		public float Blend = 0.5f;

		public Vector4 Pack()
		{
			return new Vector4(MovementDirection.x, MovementDirection.y, Scale, Blend);
		}
	}

	[NonSerialized]
	public static int BumpMapID = Shader.PropertyToID("_BumpMap");

	[NonSerialized]
	public static int BumpMapDetailsID = Shader.PropertyToID("_BumpMapDetails");

	[NonSerialized]
	public static int FoamTexID = Shader.PropertyToID("_FoamTex");

	[NonSerialized]
	public static int DepthValsID = Shader.PropertyToID("_DepthVals");

	[NonSerialized]
	public static int BorderFadeDistValsID = Shader.PropertyToID("_BorderFadeDistVals");

	[NonSerialized]
	public static int BumpinessID = Shader.PropertyToID("_Bumpiness");

	[NonSerialized]
	public static int DepthColorDeepID = Shader.PropertyToID("_DepthColorDeep");

	[NonSerialized]
	public static int DepthColorShallowID = Shader.PropertyToID("_DepthColorShallow");

	[NonSerialized]
	public static int FoamValsID = Shader.PropertyToID("_FoamVals");

	[NonSerialized]
	public static int FoamColorID = Shader.PropertyToID("_FoamColor");

	[NonSerialized]
	public static int ReflectionColorID = Shader.PropertyToID("_ReflectionColor");

	[NonSerialized]
	public static int DiffuseColorID = Shader.PropertyToID("_DiffuseColor");

	[NonSerialized]
	public static int ValsNormAID = Shader.PropertyToID("_ValsNormA");

	[NonSerialized]
	public static int ValsNormBID = Shader.PropertyToID("_ValsNormB");

	[NonSerialized]
	public static int ValsNormDID = Shader.PropertyToID("_ValsNormD");

	[NonSerialized]
	public static int ValsFoamAID = Shader.PropertyToID("_ValsFoamA");

	[NonSerialized]
	public static int ValsFoamBID = Shader.PropertyToID("_ValsFoamB");

	[NonSerialized]
	public static int AdditionalCubemapReflectionID = Shader.PropertyToID("_AdditionalCubemapReflection");

	[NonSerialized]
	public static int RippleScaleID = Shader.PropertyToID("_RippleScale");

	[NonSerialized]
	public static int RippleBumpnessID = Shader.PropertyToID("_RippleBumpness");

	[NonSerialized]
	public static int FresnelValsID = Shader.PropertyToID("_FresnelVals");

	[NonSerialized]
	public static int RippleTexID = Shader.PropertyToID("_RippleTex");

	[Space]
	[Header("Textures")]
	[SerializeField]
	public Texture2D _rippleTexture;

	[SerializeField]
	public Texture _normals;

	[SerializeField]
	public Texture _normalsDetails;

	[SerializeField]
	public float _normalsDetailsMipMapBias;

	[SerializeField]
	public Texture _foam;

	[Space(32f)]
	[Header("Layers Settings")]
	[SerializeField]
	public TextureBlendSetting _normalsA = new TextureBlendSetting();

	[SerializeField]
	public TextureBlendSetting _normalsB = new TextureBlendSetting();

	[SerializeField]
	public TextureBlendSetting _normalsDetails0 = new TextureBlendSetting();

	[Space]
	[SerializeField]
	public TextureBlendSetting _foamA = new TextureBlendSetting();

	[SerializeField]
	public TextureBlendSetting _foamB = new TextureBlendSetting();

	[Space(32f)]
	[Header("Depth Settings")]
	[SerializeField]
	public float _borderFade;

	[SerializeField]
	public float _borderFadeDistStart;

	[SerializeField]
	public float _borderFadeDistRange;

	[SerializeField]
	public float _depthFade;

	[SerializeField]
	public float _depthColorFade;

	[SerializeField]
	public float _depthRefractions;

	[ColorUsage(false)]
	[SerializeField]
	public Color _depthColorDeep;

	[ColorUsage(false)]
	[SerializeField]
	public Color _depthColorShallow;

	[Space(32f)]
	[Header("Other")]
	[SerializeField]
	public float _bumpiness;

	[Space]
	[SerializeField]
	public float _rippleScale;

	[SerializeField]
	public float _rippleBumpness;

	[Space]
	[SerializeField]
	public float _foamSize;

	[SerializeField]
	public float _foamIntensity;

	[ColorUsage(false)]
	[SerializeField]
	public Color _foamColor;

	[Space]
	[Range(0f, 1f)]
	[SerializeField]
	public float _fresnelIntensity = 0.9f;

	[SerializeField]
	public float _fresnelPower = 5f;

	[Space]
	[SerializeField]
	public float _additionalCubemapReflectionMinWetting = 0.05f;

	[SerializeField]
	public float _additionalCubemapReflectionMaxWetting = 0.35f;

	[SerializeField]
	public AnimationCurve _reflectionWettingFunc = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	public Color _reflectionColor;

	[SerializeField]
	public Color _diffuseColor;

	[field: NonSerialized]
	public bool IsDirty { get; set; } = true;

	public Texture2D RippleTexture
	{
		get
		{
			return _rippleTexture;
		}
		set
		{
			_rippleTexture = value;
			IsDirty = true;
		}
	}

	public Texture Normals
	{
		get
		{
			return _normals;
		}
		set
		{
			_normals = value;
			IsDirty = true;
		}
	}

	public Texture NormalsDetails
	{
		get
		{
			return _normalsDetails;
		}
		set
		{
			_normalsDetails = value;
			IsDirty = true;
		}
	}

	public float NormalsDetailsMipMapBias
	{
		get
		{
			return _normalsDetailsMipMapBias;
		}
		set
		{
			_normalsDetailsMipMapBias = value;
			IsDirty = true;
		}
	}

	public Texture Foam
	{
		get
		{
			return _foam;
		}
		set
		{
			_foam = value;
			IsDirty = true;
		}
	}

	public TextureBlendSetting NormalsA
	{
		get
		{
			return _normalsA;
		}
		set
		{
			_normalsA = value;
			IsDirty = true;
		}
	}

	public TextureBlendSetting NormalsB
	{
		get
		{
			return _normalsB;
		}
		set
		{
			_normalsB = value;
			IsDirty = true;
		}
	}

	public TextureBlendSetting NormalsDetails0
	{
		get
		{
			return _normalsDetails0;
		}
		set
		{
			_normalsDetails0 = value;
			IsDirty = true;
		}
	}

	public TextureBlendSetting FoamA
	{
		get
		{
			return _foamA;
		}
		set
		{
			_foamA = value;
			IsDirty = true;
		}
	}

	public TextureBlendSetting FoamB
	{
		get
		{
			return _foamB;
		}
		set
		{
			_foamB = value;
			IsDirty = true;
		}
	}

	public float BorderFade
	{
		get
		{
			return _borderFade;
		}
		set
		{
			_borderFade = value;
			IsDirty = true;
		}
	}

	public float BorderFadeDistStart
	{
		get
		{
			return _borderFadeDistStart;
		}
		set
		{
			_borderFadeDistStart = value;
			IsDirty = true;
		}
	}

	public float BorderFadeDistRange
	{
		get
		{
			return _borderFadeDistRange;
		}
		set
		{
			_borderFadeDistRange = value;
			IsDirty = true;
		}
	}

	public float DepthFade
	{
		get
		{
			return _depthFade;
		}
		set
		{
			_depthFade = value;
			IsDirty = true;
		}
	}

	public float DepthColorFade
	{
		get
		{
			return _depthColorFade;
		}
		set
		{
			_depthColorFade = value;
			IsDirty = true;
		}
	}

	public float DepthRefractions
	{
		get
		{
			return _depthRefractions;
		}
		set
		{
			_depthRefractions = value;
			IsDirty = true;
		}
	}

	public Color DepthColorDeep
	{
		get
		{
			return _depthColorDeep;
		}
		set
		{
			_depthColorDeep = value;
			IsDirty = true;
		}
	}

	public Color DepthColorShallow
	{
		get
		{
			return _depthColorShallow;
		}
		set
		{
			_depthColorShallow = value;
			IsDirty = true;
		}
	}

	public float Bumpiness
	{
		get
		{
			return _bumpiness;
		}
		set
		{
			_bumpiness = value;
			IsDirty = true;
		}
	}

	public float RippleScale
	{
		get
		{
			return _rippleScale;
		}
		set
		{
			_rippleScale = value;
			IsDirty = true;
		}
	}

	public float RippleBumpness
	{
		get
		{
			return _rippleBumpness;
		}
		set
		{
			_rippleBumpness = value;
			IsDirty = true;
		}
	}

	public float FoamSize
	{
		get
		{
			return _foamSize;
		}
		set
		{
			_foamSize = value;
			IsDirty = true;
		}
	}

	public float FoamIntensity
	{
		get
		{
			return _foamIntensity;
		}
		set
		{
			_foamIntensity = value;
			IsDirty = true;
		}
	}

	public Color FoamColor
	{
		get
		{
			return _foamColor;
		}
		set
		{
			_foamColor = value;
			IsDirty = true;
		}
	}

	public float FresnelIntensity
	{
		get
		{
			return _fresnelIntensity;
		}
		set
		{
			_fresnelIntensity = value;
			IsDirty = true;
		}
	}

	public float FresnelPower
	{
		get
		{
			return _fresnelPower;
		}
		set
		{
			_fresnelPower = value;
			IsDirty = true;
		}
	}

	public Color ReflectionColor
	{
		get
		{
			return _reflectionColor;
		}
		set
		{
			_reflectionColor = value;
			IsDirty = true;
		}
	}

	public Color DiffuseColor
	{
		get
		{
			return _diffuseColor;
		}
		set
		{
			_diffuseColor = value;
			IsDirty = true;
		}
	}

	public float AdditionalCubemapReflectionMinWetting
	{
		get
		{
			return _additionalCubemapReflectionMinWetting;
		}
		set
		{
			_additionalCubemapReflectionMinWetting = value;
		}
	}

	public float AdditionalCubemapReflectionMaxWetting
	{
		get
		{
			return _additionalCubemapReflectionMaxWetting;
		}
		set
		{
			_additionalCubemapReflectionMaxWetting = value;
		}
	}

	public AnimationCurve ReflectionWettingFunc
	{
		get
		{
			return _reflectionWettingFunc;
		}
		set
		{
			_reflectionWettingFunc = value;
		}
	}

	public void Wet(MaterialPropertyBlock block)
	{
		float t = 0f;
		if (WeatherController.Instance != null)
		{
			t = ReflectionWettingFunc.Evaluate(RainController.Intensity);
		}
		block.SetFloat(AdditionalCubemapReflectionID, Mathf.Lerp(AdditionalCubemapReflectionMinWetting, AdditionalCubemapReflectionMaxWetting, t));
	}

	public void Update(MaterialPropertyBlock block)
	{
		NormalsDetails.mipMapBias = NormalsDetailsMipMapBias;
		block.SetTexture(BumpMapID, Normals);
		block.SetTexture(BumpMapDetailsID, NormalsDetails);
		block.SetTexture(FoamTexID, Foam);
		block.SetVector(DepthValsID, new Vector4(DepthFade, DepthRefractions, DepthColorFade, BorderFade));
		block.SetVector(BorderFadeDistValsID, new Vector4(BorderFadeDistStart, 1f / BorderFadeDistRange));
		block.SetFloat(BumpinessID, Bumpiness);
		block.SetColor(DepthColorDeepID, DepthColorDeep);
		block.SetColor(DepthColorShallowID, DepthColorShallow);
		block.SetVector(FoamValsID, new Vector4(FoamSize, 1f / FoamSize, FoamIntensity));
		block.SetColor(FoamColorID, FoamColor);
		block.SetColor(ReflectionColorID, ReflectionColor);
		block.SetColor(DiffuseColorID, DiffuseColor);
		block.SetVector(ValsNormAID, NormalsA.Pack());
		block.SetVector(ValsNormBID, NormalsB.Pack());
		block.SetVector(ValsNormDID, NormalsDetails0.Pack());
		block.SetVector(ValsFoamAID, FoamA.Pack());
		block.SetVector(ValsFoamBID, FoamB.Pack());
		block.SetFloat(RippleScaleID, RippleScale);
		block.SetFloat(RippleBumpnessID, RippleBumpness);
		block.SetVector(FresnelValsID, new Vector4(FresnelIntensity, FresnelPower));
		block.SetTexture(RippleTexID, RippleTexture);
		IsDirty = false;
	}

	public void Validate()
	{
		if (float.IsNaN(_normalsA.Blend) || float.IsNaN(_normalsB.Blend) || float.IsNaN(_normalsDetails0.Blend))
		{
			TextureBlendSetting normalsA = _normalsA;
			TextureBlendSetting normalsB = _normalsB;
			TextureBlendSetting normalsDetails = _normalsDetails0;
			float num = 0.5f;
			normalsDetails.Blend = 0.5f;
			float blend = num;
			num = 0.5f;
			normalsB.Blend = blend;
			normalsA.Blend = num;
		}
		float num2 = _normalsA.Blend + _normalsB.Blend + _normalsDetails0.Blend;
		num2 = 1f / num2;
		_normalsA.Blend *= num2;
		_normalsB.Blend *= num2;
		_normalsDetails0.Blend *= num2;
		if (float.IsNaN(_foamA.Blend) || float.IsNaN(_foamB.Blend))
		{
			TextureBlendSetting foamA = _foamA;
			TextureBlendSetting foamB = _foamB;
			float num = 0.5f;
			foamB.Blend = 0.5f;
			foamA.Blend = num;
		}
		num2 = _foamA.Blend + _foamB.Blend;
		num2 = 1f / num2;
		_foamA.Blend *= num2;
		_foamB.Blend *= num2;
		IsDirty = true;
	}
}
