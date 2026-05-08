using System;
using System.Collections;
using Comfort.Common;
using EFT.Animations;
using UnityEngine;

[Serializable]
public class ForceEffector : IEffector
{
	[Serializable]
	public class Val
	{
		public ComponentType From;

		public Target To;

		public ComponentType Component;

		public float Intensity;

		[NonSerialized]
		public Spring Target;

		[NonSerialized]
		public int Component_1;

		[NonSerialized]
		public int FromComponent;

		public void Initialize(Spring rootRotation, Spring handsPosition, Spring handsRotation)
		{
			switch (To)
			{
			case global::Target.CameraRotation:
				Target = rootRotation;
				break;
			case global::Target.HandsPosition:
				Target = handsPosition;
				break;
			case global::Target.HandsRotation:
				Target = handsRotation;
				break;
			}
			Component_1 = (int)Component;
			FromComponent = (int)From;
		}

		public void Process(Vector3 force)
		{
			Target.AddAccelerationLimitless(Component_1, force[FromComponent] * Intensity);
		}
	}

	public float Intensity = 1f;

	[GAttribute7("From 60;To 120;Component 60;Intensity 80", false)]
	public Val[] Vals;

	[NonSerialized]
	public bool Initialized;

	[NonSerialized]
	public Vector3 Force;

	[NonSerialized]
	public Vector3 ForceSum;

	[NonSerialized]
	public int FramesLeft;

	[NonSerialized]
	public Spring CameraRotationSpring;

	[NonSerialized]
	public Spring CameraPositionSpring;

	[NonSerialized]
	public Spring HandsRotationSpring;

	[SerializeField]
	public float _eulersNoiseXYMaxValue = 0.5f;

	[SerializeField]
	public float _eulersNoiseZMaxValue = 1f;

	[SerializeField]
	public float _deltaRandom = 0.3f;

	public float WiggleMagnitude;

	public void Initialize(PlayerSpring playerSpring)
	{
		CameraRotationSpring = playerSpring.CameraRotation;
		CameraPositionSpring = playerSpring.CameraPosition;
		HandsRotationSpring = playerSpring.HandsRotation;
		WiggleMagnitude = Singleton<BackendConfigSettingsClass>.Instance.AimPunchMagnitude;
		Val[] vals = Vals;
		for (int i = 0; i < vals.Length; i++)
		{
			vals[i].Initialize(playerSpring.CameraRotation, playerSpring.CameraPosition, playerSpring.HandsRotation);
		}
	}

	public void Process(float deltaTime)
	{
		if (FramesLeft >= 0)
		{
			FramesLeft--;
			ForceSum = Vector3.Slerp(ForceSum, Force, 1f / 6f);
			Force = ForceSum;
			for (int i = 0; i < Vals.Length; i++)
			{
				Vals[i].Process(Force);
			}
			Force = Vector3.zero;
		}
	}

	public string DebugOutput()
	{
		throw new NotImplementedException();
	}

	public void AddForce(Vector3 direction, float strength = 1f, float hands = 1f, float camera = 1f)
	{
		strength = Mathf.Clamp01(strength);
		if (CameraRotationSpring != null && camera > 0f)
		{
			CameraRotationSpring.AddAcceleration(direction * camera * WiggleMagnitude * strength);
		}
		if (HandsRotationSpring != null && hands > 0f)
		{
			HandsRotationSpring.AddAcceleration(direction * hands * WiggleMagnitude * strength);
		}
	}

	public void AddForce(float strength = 1f, float hands = 1f, float camera = 1f)
	{
		Vector3 vector = new Vector3(UnityEngine.Random.Range(0f - _eulersNoiseXYMaxValue, _eulersNoiseXYMaxValue), UnityEngine.Random.Range(0f - _eulersNoiseXYMaxValue, _eulersNoiseXYMaxValue), UnityEngine.Random.Range(0f - _eulersNoiseZMaxValue, _eulersNoiseZMaxValue));
		float num = Vector3.Magnitude(vector);
		if ((double)num > 9.99999974737875E-06)
		{
			vector /= num / UnityEngine.Random.Range(1f - _deltaRandom, 1f);
		}
		AddForce(vector, strength, hands, camera);
	}

	public void method_0(Vector3 force, float hitPower = 1f)
	{
		Force += force.normalized * Mathf.Clamp(Intensity * hitPower * 0.0005f, 0.0003f, 0.0006f);
		FramesLeft = 6;
		Vector3 acceleration = new Vector3(UnityEngine.Random.Range(0f - _eulersNoiseXYMaxValue, _eulersNoiseXYMaxValue), UnityEngine.Random.Range(0f - _eulersNoiseXYMaxValue, _eulersNoiseXYMaxValue), UnityEngine.Random.Range(0f - _eulersNoiseZMaxValue, _eulersNoiseZMaxValue)) * hitPower;
		if (CameraRotationSpring != null)
		{
			CameraRotationSpring.AddAcceleration(acceleration);
		}
		else
		{
			Debug.LogError("CameraRotationSpring is null");
		}
	}

	public IEnumerator GrenadeShake_CO(float strength)
	{
		float num = strength * 0.75f;
		float num2 = strength * 1.2f;
		float num3 = 0.1f;
		while (num > 0f)
		{
			method_0(UnityEngine.Random.onUnitSphere, num2 + UnityEngine.Random.Range(0f, 0.15f));
			yield return new WaitForSeconds(num3);
			num -= num3;
			num2 *= 0.75f;
			num3 *= 1.1f;
		}
	}

	public void HardShake(float strength)
	{
		CameraRotationSpring?.AddAccelerationLimitless(UnityEngine.Random.onUnitSphere.normalized * strength);
		CameraPositionSpring?.AddAccelerationLimitless(UnityEngine.Random.onUnitSphere.normalized * (strength * 0.01f));
	}
}
