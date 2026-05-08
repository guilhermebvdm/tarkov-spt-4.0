using UnityEngine;

public class RainSplashController : MonoBehaviour
{
	[SerializeField]
	private DepthPhotograper _depthPhoto;

	[SerializeField]
	private ParticleSystem _splashes;

	[SerializeField]
	private float _splashLifetime = 0.3f;

	[SerializeField]
	private Vector2 _particleSizeRange = new Vector2(0.01f, 0.08f);

	[SerializeField]
	private float _splashesOffset;

	[SerializeField]
	private float _minDistance = 1.25f;

	[SerializeField]
	private float _maxDistance = 20f;

	[SerializeField]
	private float _maxGeneratedParticlesInFrame = 50f;

	[SerializeField]
	private AnimationCurve _falloffCurve;

	[SerializeField]
	private AnimationCurve _dispersionCurve;

	private Transform transform_0;

	private float float_0;

	private GClass3727 gclass3727_0;

	public float Intensity
	{
		get
		{
			return float_0;
		}
		set
		{
			float_0 = Mathf.Clamp01(value);
		}
	}

	public void Init(Camera targetCamera)
	{
		transform_0 = targetCamera.transform;
	}

	public void Awake()
	{
		gclass3727_0 = new GClass3727(300, 42);
	}

	public void Update()
	{
		if (!(transform_0 == null))
		{
			int num = Mathf.FloorToInt(_maxGeneratedParticlesInFrame * float_0);
			Color32 startColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
			{
				velocity = Vector3.zero
			};
			for (int i = 0; i < num; i++)
			{
				float num2 = (1f - _dispersionCurve.Evaluate(gclass3727_0.Next())) * (_maxDistance - _minDistance) + _minDistance;
				float num3 = 1f - _falloffCurve.Evaluate(num2 / _maxDistance);
				startColor.a = (byte)Mathf.RoundToInt((num3 * 0.7f + gclass3727_0.Next() * num3 * 0.3f) * 255f);
				emitParams.position = Vector3.up * _splashesOffset;
				emitParams.startSize = _particleSizeRange.x + gclass3727_0.Next() * (_particleSizeRange.y - _particleSizeRange.x);
				emitParams.startLifetime = _splashLifetime * 0.7f + gclass3727_0.Next() * _splashLifetime * 0.3f;
				emitParams.startColor = startColor;
				_splashes.Emit(emitParams, 1);
			}
		}
	}
}
