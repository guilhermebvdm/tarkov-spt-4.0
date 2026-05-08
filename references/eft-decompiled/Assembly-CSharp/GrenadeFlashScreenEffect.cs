using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GrenadeFlashScreenEffect : MonoBehaviour
{
	private const string string_0 = "_BlindnessCoef";

	public PrismEffects PrismEffects;

	public AnimationCurve WhiteBlack;

	public Material FlashMaterial;

	[CompilerGenerated]
	private float float_0;

	private Vector3 vector3_0 = new Vector3(7f, 0.3f, 0.3f);

	private float float_1;

	private float float_2 = 1f;

	private float float_3 = 1f;

	private static readonly int int_0 = Shader.PropertyToID("_Coef");

	private SSAAPropagator ssaapropagator_0;

	public float EffectStrength
	{
		[CompilerGenerated]
		get
		{
			return float_0;
		}
		[CompilerGenerated]
		set
		{
			float_0 = value;
		}
	}

	public void Awake()
	{
		vector3_0 = PrismEffects.toneValues;
		Shader.SetGlobalFloat("_BlindnessCoef", 0f);
		ssaapropagator_0 = GetComponent<SSAAPropagator>();
	}

	public void OnEnable()
	{
		Awake();
	}

	public void OnDisable()
	{
		PrismEffects.toneValues = vector3_0;
	}

	public void Update()
	{
		PrismEffects.toneValues = vector3_0 * (1f - EffectStrength);
		Shader.SetGlobalFloat("_BlindnessCoef", EffectStrength);
	}

	public void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.GetSourceDestination(out src, out dest);
		}
		float num = (Time.time - float_1) / float_3;
		num = ((num > 1f) ? 0f : Mathf.Clamp01(num));
		num = WhiteBlack.Evaluate(num);
		FlashMaterial.SetFloat(int_0, num * float_2);
		Graphics.Blit(src, dest, FlashMaterial);
		if (ssaapropagator_0 != null)
		{
			ssaapropagator_0.ReleaseSourceDestination(src, dest);
		}
	}

	public void Explode(float flashStrength)
	{
		float_1 = Time.time;
		float_2 = flashStrength;
		PrismEffects.toneValues = vector3_0;
	}

	public void OnDestroy()
	{
		Shader.SetGlobalFloat("_BlindnessCoef", 0f);
	}
}
