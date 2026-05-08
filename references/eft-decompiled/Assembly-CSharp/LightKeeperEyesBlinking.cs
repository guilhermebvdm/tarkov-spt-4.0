using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightKeeperEyesBlinking : MonoBehaviour
{
	[SerializeField]
	private SkinnedMeshRenderer _headMesh;

	[SerializeField]
	private LightKeeperEyeTargetFollower _eyeTargetFollower;

	[SerializeField]
	private float minDelayBetweenBlinking;

	[SerializeField]
	private float maxDelayBetweenBlinking;

	[SerializeField]
	private float minBlinkTime;

	[SerializeField]
	private float maxBlinkTime;

	[SerializeField]
	private float minTimeForClosedEyes;

	[SerializeField]
	private float maxTimeForClosedEyes;

	[SerializeField]
	private string leftEyeBlinkShape;

	[SerializeField]
	private string rightEyeBlinkShape;

	private Dictionary<string, int> dictionary_0 = new Dictionary<string, int>();

	private Coroutine coroutine_0;

	private WaitForSeconds waitForSeconds_0 = new WaitForSeconds(1f);

	public void Awake()
	{
		dictionary_0.Clear();
		for (int i = 0; i < _headMesh.sharedMesh.blendShapeCount; i++)
		{
			int num = i;
			string blendShapeName = _headMesh.sharedMesh.GetBlendShapeName(num);
			dictionary_0[blendShapeName] = num;
		}
		coroutine_0 = StartCoroutine(method_0());
	}

	public void OnDestroy()
	{
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
	}

	public IEnumerator method_0()
	{
		yield return new WaitForSeconds(Random.Range(minDelayBetweenBlinking, maxDelayBetweenBlinking));
		while (true)
		{
			_eyeTargetFollower.ToggleBlockBlendShapes(blockIt: true);
			float num = Random.Range(minBlinkTime, maxBlinkTime) / 2f;
			float num2 = 0f;
			float leftEyeUpBlendLastValue = _eyeTargetFollower.LeftEyeUpBlendLastValue;
			float rightEyeUpBlendLastValue = _eyeTargetFollower.RightEyeUpBlendLastValue;
			float num3 = leftEyeUpBlendLastValue / 100f;
			float num4 = rightEyeUpBlendLastValue / 100f;
			while (num2 <= 1f)
			{
				float num5 = num2 * 100f;
				_headMesh.SetBlendShapeWeight(dictionary_0[leftEyeBlinkShape], num5);
				_headMesh.SetBlendShapeWeight(dictionary_0[rightEyeBlinkShape], num5);
				float value = leftEyeUpBlendLastValue - num5 * num3;
				float value2 = rightEyeUpBlendLastValue - num5 * num4;
				_headMesh.SetBlendShapeWeight(dictionary_0[_eyeTargetFollower.LefttEyeBlendOnUp], value);
				_headMesh.SetBlendShapeWeight(dictionary_0[_eyeTargetFollower.RightEyeBlendOnUp], value2);
				num2 += Time.deltaTime / num;
				yield return null;
			}
			_headMesh.SetBlendShapeWeight(dictionary_0[leftEyeBlinkShape], 100f);
			_headMesh.SetBlendShapeWeight(dictionary_0[rightEyeBlinkShape], 100f);
			_headMesh.SetBlendShapeWeight(dictionary_0[_eyeTargetFollower.LefttEyeBlendOnUp], 0f);
			_headMesh.SetBlendShapeWeight(dictionary_0[_eyeTargetFollower.RightEyeBlendOnUp], 0f);
			num2 = 1f;
			yield return new WaitForSeconds(Random.Range(minTimeForClosedEyes, maxTimeForClosedEyes));
			float num6 = 0f;
			while (num2 >= 0f)
			{
				float num5 = num2 * 100f;
				_headMesh.SetBlendShapeWeight(dictionary_0[leftEyeBlinkShape], num5);
				_headMesh.SetBlendShapeWeight(dictionary_0[rightEyeBlinkShape], num5);
				float value = num6 * _eyeTargetFollower.LeftEyeUpBlendLastValue;
				float value2 = num6 * _eyeTargetFollower.RightEyeUpBlendLastValue;
				_headMesh.SetBlendShapeWeight(dictionary_0[_eyeTargetFollower.LefttEyeBlendOnUp], value);
				_headMesh.SetBlendShapeWeight(dictionary_0[_eyeTargetFollower.RightEyeBlendOnUp], value2);
				num2 -= Time.deltaTime / num;
				num6 += Time.deltaTime / num;
				yield return null;
			}
			_headMesh.SetBlendShapeWeight(dictionary_0[leftEyeBlinkShape], 0f);
			_headMesh.SetBlendShapeWeight(dictionary_0[rightEyeBlinkShape], 0f);
			_eyeTargetFollower.ToggleBlockBlendShapes(blockIt: false);
			yield return new WaitForSeconds(Random.Range(minDelayBetweenBlinking, maxDelayBetweenBlinking));
		}
	}
}
