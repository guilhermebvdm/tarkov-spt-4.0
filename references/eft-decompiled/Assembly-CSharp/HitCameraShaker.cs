using UnityEngine;

[DisallowMultipleComponent]
public class HitCameraShaker : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve _positionCurve;

	[SerializeField]
	private AnimationCurve _hitBackCurve;

	[SerializeField]
	private float _maxOffset = 0.2f;

	[SerializeField]
	private float _goForwardTime = 0.5f;

	[SerializeField]
	private float _goBackTime = 1.5f;

	[SerializeField]
	private float _eulersNoiseXYMaxValue = 0.5f;

	[SerializeField]
	private float _eulersNoiseZMaxValue = 1f;

	[SerializeField]
	private float _positionXYNoise = 0.1f;

	[SerializeField]
	private float _positionZNoise = 0.02f;

	private float float_0;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	[SerializeField]
	private Transform _transform;

	private bool bool_0;

	private Vector3 vector3_2;

	private Vector3 vector3_3;

	private AnimationCurve animationCurve_0;

	private float float_1;

	public void Start()
	{
		if (_transform == null)
		{
			_transform = base.transform;
		}
		vector3_0 = _transform.localPosition;
		vector3_1 = _transform.localEulerAngles;
		vector3_3 = vector3_0;
		animationCurve_0 = _positionCurve;
	}

	public void Hit(Vector3 hitDirection, float hitPower)
	{
		if (hitDirection.sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon)
		{
			hitDirection = new Vector3(0f, 0f, -1f);
		}
		bool_0 = true;
		float_1 = Mathf.Clamp01(hitPower / 10f);
		hitDirection.z = Mathf.Max(0f, hitDirection.z);
		hitDirection = -hitDirection;
		hitDirection.Normalize();
		float num = float_1 * _maxOffset;
		hitDirection.Scale(new Vector3(num, num, num));
		vector3_2 = hitDirection + vector3_0;
		vector3_3 = _transform.localPosition;
		animationCurve_0 = _positionCurve;
	}

	public void LateUpdate()
	{
		Vector3 vector = Vector3.zero;
		if (bool_0)
		{
			float_0 = Mathf.Clamp01(float_0 + Time.deltaTime / (_goForwardTime * float_1));
			if (float_0 >= 1f)
			{
				bool_0 = false;
				animationCurve_0 = _hitBackCurve;
				vector3_3 = vector3_0;
			}
			_transform.localEulerAngles = new Vector3(Random.Range(0f - _eulersNoiseXYMaxValue, _eulersNoiseXYMaxValue) * float_1, Random.Range(0f - _eulersNoiseXYMaxValue, _eulersNoiseXYMaxValue) * float_1, Random.Range(0f - _eulersNoiseZMaxValue, _eulersNoiseZMaxValue) * float_1) + vector3_1;
			vector = new Vector3(Random.Range(0f - _positionXYNoise, _positionXYNoise) * float_1, Random.Range(0f - _positionXYNoise, _positionXYNoise) * float_1, Random.Range(0f - _positionZNoise, _positionZNoise)) * float_1;
		}
		else
		{
			float_0 = Mathf.Clamp01(float_0 - Time.deltaTime / (_goBackTime * float_1));
			_transform.localRotation = Quaternion.Slerp(Quaternion.Euler(vector3_1), Quaternion.Euler(_transform.localEulerAngles), float_0);
		}
		_transform.localPosition = Vector3.Lerp(vector3_3, vector3_2, Mathf.Clamp01(animationCurve_0.Evaluate(float_0))) + vector;
	}
}
