using UnityEngine;
using Object = UnityEngine.Object;

public class BFX_ManualAnimationUpdate : MonoBehaviour
{
	public BFX_BloodSettings BloodSettings;

	public AnimationCurve AnimationSpeed = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float FramesCount = 99f;

	public float TimeLimit = 3f;

	public float OffsetFrames = 0f;

	private float currentTime;

	private Renderer rend;

	private MaterialPropertyBlock propertyBlock;

	private void Awake()
	{
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		rend = ((Component)this).GetComponent<Renderer>();
	}

	private void OnEnable()
	{
		if (rend != null)
		{
			rend.enabled = true;
			if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
			rend.GetPropertyBlock(propertyBlock);
			propertyBlock.SetFloat("_UseCustomTime", 1f);
			propertyBlock.SetFloat("_TimeInFrames", 0f);
			rend.SetPropertyBlock(propertyBlock);
		}
		currentTime = 0f;
	}

	private void Update()
	{
		if (rend == null) return;

		float animSpeed = (BloodSettings != null) ? BloodSettings.AnimationSpeed : 1f;
		currentTime += Time.deltaTime * animSpeed;
		if ((double)(currentTime / TimeLimit) > 1.0)
		{
			if (rend.enabled)
			{
				rend.enabled = false;
			}
			return;
		}
		float num = AnimationSpeed.Evaluate(currentTime / TimeLimit);
		num = num * FramesCount + OffsetFrames + 1.1f;
		float num2 = Mathf.Ceil(0f - num) / (FramesCount + 1f) + 1f / (FramesCount + 1f);
		if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
		rend.GetPropertyBlock(propertyBlock);
		float lightIntensity = (BloodSettings != null) ? BloodSettings.LightIntensityMultiplier : 1f;
		propertyBlock.SetFloat("_LightIntencity", Mathf.Clamp(lightIntensity, 0.01f, 1f));
		propertyBlock.SetFloat("_TimeInFrames", num2);
		rend.SetPropertyBlock(propertyBlock);
	}
}
