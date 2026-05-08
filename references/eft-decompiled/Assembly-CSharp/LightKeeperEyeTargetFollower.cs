using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LightKeeperEyeTargetFollower : MonoBehaviour
{
	public enum EEditorToggleEye
	{
		Left,
		Right
	}

	[SerializeField]
	private Transform _transformToLookAt;

	[SerializeField]
	private SkinnedMeshRenderer _headMesh;

	[SerializeField]
	private float _lerpToTargetRotationSpeed;

	[SerializeField]
	private float _angleDifForStartRotation;

	[SerializeField]
	private float _angleDifForStopRotation = 1f;

	[SerializeField]
	private bool _returnStartRotationOnEnd = true;

	[SerializeField]
	private int angleToBlendShapeMultiplier = 10;

	[SerializeField]
	private EEditorToggleEye _eyeToShow;

	[SerializeField]
	private Transform _leftEye;

	[SerializeField]
	private string _leBlendOnRight;

	[SerializeField]
	private string _leBlendOnLeft;

	[SerializeField]
	private string _leBlendOnUp;

	[SerializeField]
	private string _leBlendOnDown;

	[SerializeField]
	private Vector3 _leAdditionalRotation = new Vector3(-90f, 0f, 0f);

	[SerializeField]
	private Space _leAdditionalRotSpace = Space.Self;

	private Vector3 vector3_0;

	private Quaternion quaternion_0;

	private Vector3 vector3_1;

	private Vector3 vector3_2;

	private float float_0;

	[SerializeField]
	private Transform _rightEye;

	[SerializeField]
	private string _reBlendOnRight;

	[SerializeField]
	private string _reBlendOnLeft;

	[SerializeField]
	private string _reBlendOnUp;

	[SerializeField]
	private string _reBlendOnDown;

	[SerializeField]
	private Vector3 _reAdditionalRotation = new Vector3(-90f, 0f, 0f);

	[SerializeField]
	private Space _reAdditionalRotSpace = Space.Self;

	private Vector3 vector3_3;

	private Quaternion quaternion_1;

	private Vector3 vector3_4;

	private Vector3 vector3_5;

	private float float_1;

	private Dictionary<string, int> dictionary_0 = new Dictionary<string, int>();

	private Action action_0;

	private Coroutine coroutine_0;

	private bool bool_0 = true;

	private Transform transform_0;

	public float LeftEyeUpBlendLastValue => float_0;

	public string LefttEyeBlendOnUp => _leBlendOnUp;

	public string RightEyeBlendOnUp => _reBlendOnUp;

	public float RightEyeUpBlendLastValue => float_1;

	public Vector3 Vector3_0 => _leftEye.up * -1f;

	public Vector3 Vector3_1 => _rightEye.up * -1f;

	public void Awake()
	{
		quaternion_0 = _leftEye.rotation;
		quaternion_1 = _rightEye.rotation;
		action_0 = GlobalEventHandlerClass.Instance.SubscribeOnEvent(delegate(GClass3555 interactEvent)
		{
			method_0(interactEvent);
		});
		dictionary_0.Clear();
		for (int num = 0; num < _headMesh.sharedMesh.blendShapeCount; num++)
		{
			int num2 = num;
			string blendShapeName = _headMesh.sharedMesh.GetBlendShapeName(num2);
			dictionary_0[blendShapeName] = num2;
		}
	}

	public void OnDestroy()
	{
		action_0?.Invoke();
		action_0 = null;
		ToggleWorking(enable: false);
	}

	public void method_0(GClass3555 invokedEvent)
	{
		switch (invokedEvent.InteractState)
		{
		case GClass3555.EInteractState.Exited:
			ToggleWorking(enable: false);
			break;
		case GClass3555.EInteractState.IsEntering:
			_transformToLookAt = invokedEvent.InteractingPlayer.CameraContainer.transform;
			ToggleWorking(enable: true);
			break;
		}
	}

	public void ToggleWorking(bool enable)
	{
		if (enable)
		{
			if (coroutine_0 == null)
			{
				vector3_0 = _leftEye.up * -1f;
				vector3_3 = _rightEye.up * -1f;
				vector3_1 = _leftEye.position + _leftEye.up * -1f - _transformToLookAt.position;
				vector3_4 = _rightEye.position + _rightEye.up * -1f - _transformToLookAt.position;
				vector3_2 = vector3_1;
				vector3_5 = vector3_4;
				coroutine_0 = StartCoroutine(method_1());
			}
		}
		else if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
			if (_returnStartRotationOnEnd)
			{
				_leftEye.rotation = quaternion_0;
				_rightEye.rotation = quaternion_1;
			}
		}
	}

	public IEnumerator method_1()
	{
		bool flag = false;
		bool flag2 = false;
		while (!(_transformToLookAt == null))
		{
			vector3_1 = _transformToLookAt.position - _leftEye.position;
			Quaternion b = Quaternion.LookRotation(vector3_1);
			float num = Quaternion.Angle(_leftEye.rotation, b);
			if (!flag && num > _angleDifForStartRotation)
			{
				vector3_2 = Vector3.Lerp(vector3_2, vector3_1, Time.deltaTime * _lerpToTargetRotationSpeed);
				_leftEye.rotation = Quaternion.LookRotation(vector3_2);
				flag = true;
			}
			else if (flag)
			{
				vector3_2 = Vector3.Lerp(vector3_2, vector3_1, Time.deltaTime * _lerpToTargetRotationSpeed);
				_leftEye.rotation = Quaternion.LookRotation(vector3_2);
				if (num <= _angleDifForStopRotation)
				{
					flag = false;
				}
			}
			_leftEye.Rotate(_leAdditionalRotation, _leAdditionalRotSpace);
			Vector3 eulerAngles = Quaternion.FromToRotation(vector3_0, Vector3_0).eulerAngles;
			float signedAngle = GetSignedAngle(eulerAngles.x);
			float signedAngle2 = GetSignedAngle(eulerAngles.y);
			if (signedAngle > 0f)
			{
				_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnDown], Mathf.Clamp(signedAngle * (float)angleToBlendShapeMultiplier, 0f, 100f));
				if (!bool_0)
				{
					_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnUp], 0f);
				}
				float_0 = 0f;
			}
			else
			{
				float_0 = Mathf.Clamp(Mathf.Abs(signedAngle) * (float)angleToBlendShapeMultiplier, 0f, 100f);
				if (!bool_0)
				{
					_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnUp], float_0);
				}
				_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnDown], 0f);
			}
			if (signedAngle2 > 0f)
			{
				_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnRight], Mathf.Clamp(signedAngle2 * (float)angleToBlendShapeMultiplier, 0f, 100f));
				_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnLeft], 0f);
			}
			else
			{
				_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnLeft], Mathf.Clamp(Mathf.Abs(signedAngle2) * (float)angleToBlendShapeMultiplier, 0f, 100f));
				_headMesh.SetBlendShapeWeight(dictionary_0[_leBlendOnRight], 0f);
			}
			vector3_4 = _transformToLookAt.position - _rightEye.position;
			b = Quaternion.LookRotation(vector3_4);
			float num2 = Quaternion.Angle(_rightEye.rotation, b);
			if (!flag2 && num2 > _angleDifForStartRotation)
			{
				vector3_5 = Vector3.Lerp(vector3_5, vector3_4, Time.deltaTime * _lerpToTargetRotationSpeed);
				_rightEye.rotation = Quaternion.LookRotation(vector3_5);
				flag2 = true;
			}
			else if (flag2)
			{
				vector3_5 = Vector3.Lerp(vector3_5, vector3_4, Time.deltaTime * _lerpToTargetRotationSpeed);
				_rightEye.rotation = Quaternion.LookRotation(vector3_5);
				if (num2 <= _angleDifForStopRotation)
				{
					flag2 = false;
				}
			}
			_rightEye.Rotate(_reAdditionalRotation, _reAdditionalRotSpace);
			eulerAngles = Quaternion.FromToRotation(vector3_3, Vector3_1).eulerAngles;
			signedAngle = GetSignedAngle(eulerAngles.x);
			signedAngle2 = GetSignedAngle(eulerAngles.y);
			if (signedAngle > 0f)
			{
				_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnDown], Mathf.Clamp(signedAngle * (float)angleToBlendShapeMultiplier, 0f, 100f));
				if (!bool_0)
				{
					_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnUp], 0f);
				}
				float_1 = 0f;
			}
			else
			{
				float_1 = Mathf.Clamp(Mathf.Abs(signedAngle) * (float)angleToBlendShapeMultiplier, 0f, 100f);
				if (!bool_0)
				{
					_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnUp], float_1);
				}
				_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnDown], 0f);
			}
			if (signedAngle2 > 0f)
			{
				_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnRight], Mathf.Clamp(signedAngle2 * (float)angleToBlendShapeMultiplier, 0f, 100f));
				_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnLeft], 0f);
			}
			else
			{
				_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnLeft], Mathf.Clamp(Mathf.Abs(signedAngle2) * (float)angleToBlendShapeMultiplier, 0f, 100f));
				_headMesh.SetBlendShapeWeight(dictionary_0[_reBlendOnRight], 0f);
			}
			yield return null;
		}
		ToggleWorking(enable: false);
	}

	public void ToggleBlockBlendShapes(bool blockIt)
	{
		bool_0 = blockIt;
	}

	public float GetSignedAngle(float angle)
	{
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	public void Update()
	{
		if (_transformToLookAt != transform_0)
		{
			ToggleWorking(enable: false);
			if (_transformToLookAt != null)
			{
				ToggleWorking(enable: true);
			}
			transform_0 = _transformToLookAt;
		}
	}

	[CompilerGenerated]
	public void method_2(GClass3555 interactEvent)
	{
		method_0(interactEvent);
	}
}
