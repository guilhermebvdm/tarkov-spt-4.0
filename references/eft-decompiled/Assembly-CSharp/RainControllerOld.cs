using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class RainControllerOld : MonoBehaviour
{
	[SerializeField]
	private AudioSource _waterAudio;

	[SerializeField]
	private float _maxAudioVolume = 0.5f;

	[SerializeField]
	private float _minAudioVolume;

	[SerializeField]
	private AnimationCurve _fadeCurve;

	[SerializeField]
	private float _evaluationTime = 2f;

	private float float_0;

	private int int_0 = 1;

	public void OnTriggerEnter(Collider col)
	{
		int_0 = -1;
	}

	public void OnTriggerExit(Collider col)
	{
		int_0 = 1;
	}

	public void Update()
	{
		float num = 1f / _evaluationTime * Time.deltaTime;
		float_0 = Mathf.Clamp01(float_0 - num * (float)int_0);
		float t = _fadeCurve.Evaluate(float_0);
		_waterAudio.volume = Mathf.Lerp(_minAudioVolume, _maxAudioVolume, t);
	}
}
