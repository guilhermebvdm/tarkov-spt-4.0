using System;
using System.Collections;
using EFT;
using UnityEngine;

public class PedometerClass
{
	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public Player Player_0;

	[NonSerialized]
	public float[] Float_1;

	[NonSerialized]
	public float[] Float_2;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public IEnumerator Ienumerator_0;

	public EPlayerState CurrentState = EPlayerState.Idle;

	public PedometerClass(Player player)
	{
		Player_0 = player;
		Array values = Enum.GetValues(typeof(EPlayerState));
		Float_1 = new float[values.Length];
		Float_2 = new float[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			Float_1[i] = 0f;
			Float_2[i] = 0f;
		}
	}

	public IEnumerator method_0()
	{
		while (true)
		{
			if (Vector3_0 != Player_0.Transform.position)
			{
				Vector3 position = Player_0.Transform.position;
				Vector3 vector = Vector3_0 - position;
				Float_1[(uint)CurrentState] += Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);
				Vector3_0 = position;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public void Start()
	{
		for (int i = 0; i < Float_1.Length; i++)
		{
			Float_1[i] = 0f;
		}
		for (int j = 0; j < Float_2.Length; j++)
		{
			Float_2[j] = 0f;
		}
		Vector3_0 = Vector3.zero;
		if (Ienumerator_0 != null)
		{
			Stop();
		}
		Ienumerator_0 = method_0();
		Player_0.StartCoroutine(Ienumerator_0);
	}

	public void Stop()
	{
		Player_0.StopCoroutine(Ienumerator_0);
	}

	public float GetDistance()
	{
		float num = 0f;
		for (int i = 0; i < Float_1.Length; i++)
		{
			num += Float_1[i];
		}
		return num;
	}

	public void MakeMark(EPlayerState state)
	{
		Float_2[(uint)state] = Float_1[(uint)state];
	}

	public float GetDistanceFromMark(EPlayerState state)
	{
		float num = Float_2[(uint)state];
		return Float_1[(uint)state] - num;
	}
}
