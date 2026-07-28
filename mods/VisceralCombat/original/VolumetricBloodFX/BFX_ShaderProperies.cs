using System;
using UnityEngine;

public class BFX_ShaderProperies : MonoBehaviour
{
	public BFX_BloodSettings BloodSettings;

	public AnimationCurve FloatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public float GraphTimeMultiplier = 1f;

	public float GraphIntensityMultiplier = 1f;

	public float TimeDelay = 0f;

	private bool canUpdate;

	private bool isFrized;

	private float startTime;

	private int cutoutPropertyID;

	private int forwardDirPropertyID;

	private float timeLapsed;

	private MaterialPropertyBlock props;

	private Renderer rend;

	public event Action OnAnimationFinished;

	private void Awake()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		props = new MaterialPropertyBlock();
		rend = ((Component)this).GetComponent<Renderer>();
		cutoutPropertyID = Shader.PropertyToID("_Cutout");
		forwardDirPropertyID = Shader.PropertyToID("_DecalForwardDir");
		OnEnable();
	}

	private void OnEnable()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		startTime = Time.time + TimeDelay;
		canUpdate = true;
		((Component)this).GetComponent<Renderer>().enabled = true;
		rend.GetPropertyBlock(props);
		float num = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
		props.SetFloat(cutoutPropertyID, num);
		props.SetVector(forwardDirPropertyID, Vector4.op_Implicit(((Component)this).transform.up));
		rend.SetPropertyBlock(props);
	}

	private void OnDisable()
	{
		rend.GetPropertyBlock(props);
		float num = FloatCurve.Evaluate(0f) * GraphIntensityMultiplier;
		props.SetFloat(cutoutPropertyID, num);
		rend.SetPropertyBlock(props);
		timeLapsed = 0f;
	}

	private void Update()
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		if (canUpdate)
		{
			rend.GetPropertyBlock(props);
			float num = (((Object)(object)BloodSettings == (Object)null) ? Time.deltaTime : (Time.deltaTime * BloodSettings.AnimationSpeed));
			if (!((Object)(object)BloodSettings != (Object)null) || !BloodSettings.FreezeDecalDisappearance || !(timeLapsed / GraphTimeMultiplier > 0.3f))
			{
				timeLapsed += num;
			}
			float num2 = FloatCurve.Evaluate(timeLapsed / GraphTimeMultiplier) * GraphIntensityMultiplier;
			props.SetFloat(cutoutPropertyID, num2);
			if ((Object)(object)BloodSettings != (Object)null)
			{
				props.SetFloat("_LightIntencity", Mathf.Clamp(BloodSettings.LightIntensityMultiplier, 0.01f, 1f));
			}
			if (timeLapsed >= GraphTimeMultiplier)
			{
				canUpdate = false;
				this.OnAnimationFinished?.Invoke();
			}
			props.SetVector(forwardDirPropertyID, Vector4.op_Implicit(((Component)this).transform.up));
			rend.SetPropertyBlock(props);
		}
	}
}
