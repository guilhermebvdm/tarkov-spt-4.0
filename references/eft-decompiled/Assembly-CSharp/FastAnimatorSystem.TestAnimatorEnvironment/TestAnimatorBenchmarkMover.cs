using System;
using System.Collections.Generic;
using UnityEngine;

namespace FastAnimatorSystem.TestAnimatorEnvironment;

public class TestAnimatorBenchmarkMover : MonoBehaviour
{
	public TestAnimatorSkeleton fastSkeletonPrefab;

	public TestAnimatorSkeleton origSkeletonPrefab;

	public float speed = 1.5f;

	public float deltaTreshhold = 0.1f;

	public float actualDelta;

	public int spawnCount = 60;

	private int int_0 = Animator.StringToHash("Direct_X");

	private int int_1 = Animator.StringToHash("Direct_Y");

	private int int_2 = Animator.StringToHash("Inert");

	private int int_3 = Animator.StringToHash("IsJumping");

	private List<IAnimator> list_0 = new List<IAnimator>();

	private List<TestAnimatorSkeleton> list_1 = new List<TestAnimatorSkeleton>();

	private float float_0;

	private float float_1;

	public void Update()
	{
		float value = 0f;
		float value2 = 0f;
		if (Input.GetKey(KeyCode.W))
		{
			value2 = 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			value2 = -1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			value = 1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			value = -1f;
		}
		if (Input.GetKeyDown(KeyCode.F1))
		{
			method_0(spawnCount, origSkeletonPrefab);
		}
		else if (Input.GetKeyDown(KeyCode.F2))
		{
			method_0(spawnCount, fastSkeletonPrefab);
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			foreach (TestAnimatorSkeleton item in list_1)
			{
				item.transform.position = Vector3.zero;
			}
		}
		bool key = Input.GetKey(KeyCode.Space);
		bool value3;
		if (!(value3 = Math.Abs(value) > Mathf.Epsilon || Math.Abs(value2) > Mathf.Epsilon || key))
		{
			value = float_0;
			value2 = float_1;
		}
		foreach (IAnimator item2 in list_0)
		{
			item2.SetFloat(int_0, value);
			item2.SetFloat(int_1, value2);
			item2.SetBool(int_2, value3);
			item2.SetBool(int_3, key);
		}
		foreach (TestAnimatorSkeleton item3 in list_1)
		{
			item3.Process(Time.deltaTime);
		}
		float_0 = value;
		float_1 = value2;
	}

	public void method_0(int count, TestAnimatorSkeleton skeletonPrefab)
	{
		foreach (TestAnimatorSkeleton item in list_1)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		list_1.Clear();
		list_0.Clear();
		for (int i = 0; i < count; i++)
		{
			GameObject obj = UnityEngine.Object.Instantiate(skeletonPrefab.gameObject);
			TestAnimatorSkeleton component = obj.GetComponent<TestAnimatorSkeleton>();
			list_1.Add(component);
			obj.SetActive(value: true);
			component.Init();
			list_0.Add(component.Animator);
		}
		StopAllCoroutines();
	}
}
