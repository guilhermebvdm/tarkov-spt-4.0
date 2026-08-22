using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public class BFX_ShaderProperies : MonoBehaviour
{
	public BFX_BloodSettings BloodSettings;

	public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public float TimeDelay = 0f;

	private bool canUpdate;

	private float startTime;

	private int cutoutPropertyID;

	private int forwardDirPropertyID;

	private float timeLapsed;

	private MaterialPropertyBlock props;

	private Renderer rend;

	public event Action OnAnimationFinished;

	private void Awake()
	{
		props = new MaterialPropertyBlock();
		rend = ((Component)this).GetComponent<Renderer>();
		cutoutPropertyID = Shader.PropertyToID("_Cutout");
		forwardDirPropertyID = Shader.PropertyToID("_DecalForwardDir");
	}

	private void OnEnable()
	{
		startTime = Time.time + TimeDelay;
		canUpdate = true;
		if (rend != null)
		{
			rend.enabled = true;
			if (props == null) props = new MaterialPropertyBlock();
			rend.GetPropertyBlock(props);
			float num = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
			props.SetFloat(cutoutPropertyID, num);
			props.SetVector(forwardDirPropertyID, (Vector4)(((Component)this).transform.up));
			rend.SetPropertyBlock(props);
		}
	}

	private void OnDisable()
	{
		if (rend != null)
		{
			if (props == null) props = new MaterialPropertyBlock();
			rend.GetPropertyBlock(props);
			float num = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
			props.SetFloat(cutoutPropertyID, num);
			rend.SetPropertyBlock(props);
		}
		timeLapsed = 0f;
	}

	private void Update()
	{
		if (canUpdate && rend != null)
		{
			if (props == null) props = new MaterialPropertyBlock();
			rend.GetPropertyBlock(props);
			float animSpeed = (BloodSettings != null) ? BloodSettings.AnimationSpeed : 1f;
			float num = Time.deltaTime * animSpeed;
			if (BloodSettings == null || !BloodSettings.FreezeDecalDisappearance || !(timeLapsed / GraphTimeMultiplier > 0.3f))
			{
				timeLapsed += num;
			}
			float num2 = FloatCurve.Evaluate(timeLapsed / GraphTimeMultiplier) * GraphIntensityMultiplier;
			props.SetFloat(cutoutPropertyID, num2);
			if (BloodSettings != null)
			{
				props.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));
			}
			if (timeLapsed >= GraphTimeMultiplier)
			{
				canUpdate = false;
				this.OnAnimationFinished?.Invoke();
			}
			props.SetVector(forwardDirPropertyID, (Vector4)(((Component)this).transform.up));
			rend.SetPropertyBlock(props);
		}
	}
}
