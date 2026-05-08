using System;
using UnityEngine;

namespace GPUInstancer;

[Serializable]
public class GPUInstancerCameraData
{
	public Camera mainCamera;

	public SSAA mainCameraSSAA;

	public GPUInstancerHiZOcclusionGenerator hiZOcclusionGenerator;

	public float[] mvpMatrixFloats;

	public float[] mvpMatrix2Floats;

	public Vector3 cameraPosition = Vector3.zero;

	public bool hasOcclusionGenerator;

	public float halfAngle;

	[NonSerialized]
	public Quaternion PreviousRotation = Quaternion.identity;

	[field: NonSerialized]
	public bool IsOptic { get; }

	[field: NonSerialized]
	public float LastAngularFrustumOffset { get; set; }

	public GPUInstancerCameraData(Camera mainCamera = null, bool _isOptic = false)
	{
		this.mainCamera = mainCamera;
		mainCameraSSAA = mainCamera?.GetComponent<SSAA>();
		IsOptic = _isOptic;
		mvpMatrixFloats = new float[16];
		CalculateHalfAngle();
	}

	public void SetCamera(Camera mainCamera)
	{
		this.mainCamera = mainCamera;
		if (mainCamera != null)
		{
			mainCameraSSAA = mainCamera.GetComponent<SSAA>();
		}
		else
		{
			mainCameraSSAA = null;
		}
		CalculateHalfAngle();
	}

	public void CalculateCameraData()
	{
		if (mainCamera == null)
		{
			return;
		}
		hasOcclusionGenerator = hiZOcclusionGenerator != null && hiZOcclusionGenerator.hiZDepthTexture != null;
		Matrix4x4 matrix4x = ((!hasOcclusionGenerator || !hiZOcclusionGenerator.isVREnabled) ? mainCamera.projectionMatrix : mainCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left)) * mainCamera.worldToCameraMatrix;
		if (mvpMatrixFloats == null || mvpMatrixFloats.Length != 16)
		{
			mvpMatrixFloats = new float[16];
		}
		GClass1274.Matrix4x4ToFloatArray(matrix4x, mvpMatrixFloats);
		if (hasOcclusionGenerator && hiZOcclusionGenerator.isVREnabled && GClass1262.gpuiSettings.testBothEyesForVROcclusion)
		{
			Matrix4x4 matrix4x2 = mainCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right) * mainCamera.worldToCameraMatrix;
			if (mvpMatrix2Floats == null || mvpMatrix2Floats.Length != 16)
			{
				mvpMatrix2Floats = new float[16];
			}
			GClass1274.Matrix4x4ToFloatArray(matrix4x2, mvpMatrix2Floats);
		}
		cameraPosition = mainCamera.transform.position;
	}

	public void CalculateHalfAngle()
	{
		if (mainCamera != null)
		{
			halfAngle = Mathf.Tan(MathF.PI / 180f * mainCamera.fieldOfView * 0.25f);
		}
	}

	public void CalculateAngularFrustumOffset(float frustumOffset)
	{
		Quaternion quaternion = mainCamera.transform.rotation * Quaternion.Inverse(PreviousRotation);
		PreviousRotation = mainCamera.transform.rotation;
		Vector3 axis = Vector3.zero;
		quaternion.ToAngleAxis(out var angle, out axis);
		angle *= MathF.PI / 180f;
		float num = Mathf.Clamp((axis * angle * (1f / Time.deltaTime)).y, -10f, 10f);
		LastAngularFrustumOffset = num * frustumOffset;
	}
}
