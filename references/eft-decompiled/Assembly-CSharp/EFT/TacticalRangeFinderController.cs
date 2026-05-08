using System;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

namespace EFT;

public class TacticalRangeFinderController : MonoBehaviour
{
	public enum DistanceOutputFormat
	{
		FourDigits,
		ThreeDigitsAndDot
	}

	[SerializeField]
	private DistanceOutputFormat _distanceOutputFormat;

	[SerializeField]
	private string _noDistanceText = "----";

	[SerializeField]
	private float _delayInSecsBetweenCasts = 0.5f;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private TMP_Text _textOnDisplay;

	[SerializeField]
	private Transform _boneToCastRay;

	[SerializeField]
	private float _rayStartOffset = 0.1f;

	[SerializeField]
	private float _maxCastDistance = 2500f;

	[SerializeField]
	private float _cullDistance = 1f;

	[SerializeField]
	private LayerMask _mask;

	private float float_0;

	private readonly CustomSampler customSampler_0 = CustomSampler.Create("TacticalRangeFinderController.MeasureDistance");

	public void OnEnable()
	{
		float_0 = 0f;
		if (_canvas != null)
		{
			_canvas.worldCamera = CameraClass.Instance.OpticCameraManager.Camera;
			_canvas.planeDistance = 2f;
		}
		method_0();
	}

	public void OnDisable()
	{
		if (_canvas != null)
		{
			_canvas.worldCamera = null;
		}
	}

	public void Update()
	{
		float_0 += Time.deltaTime;
		if (!(float_0 < _delayInSecsBetweenCasts))
		{
			float_0 = 0f;
			bool flag = CameraClass.Instance.Distance(base.transform.parent.position) < _cullDistance;
			if (_canvas != null)
			{
				_canvas.enabled = flag;
			}
			if (flag)
			{
				method_0();
			}
		}
	}

	public void method_0()
	{
		Vector3 position = _boneToCastRay.position;
		Vector3 forward = _boneToCastRay.forward;
		if (Physics.Raycast(position + forward * _rayStartOffset, forward, out var hitInfo, _maxCastDistance, _mask))
		{
			switch (_distanceOutputFormat)
			{
			default:
				throw new ArgumentOutOfRangeException();
			case DistanceOutputFormat.ThreeDigitsAndDot:
			{
				float num2 = Mathf.Clamp(hitInfo.distance, 0f, 999f);
				GClass1673.SetMonospaceText(_textOnDisplay, num2.ToString("000.0"));
				break;
			}
			case DistanceOutputFormat.FourDigits:
			{
				int num = Mathf.RoundToInt(hitInfo.distance);
				GClass1673.SetMonospaceText(_textOnDisplay, num.ToString("D4"));
				break;
			}
			}
		}
		else
		{
			GClass1673.SetMonospaceText(_textOnDisplay, _noDistanceText);
		}
	}
}
