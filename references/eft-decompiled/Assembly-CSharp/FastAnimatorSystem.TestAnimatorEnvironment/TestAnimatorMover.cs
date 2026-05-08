using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;
using UnityEngine.UI;

namespace FastAnimatorSystem.TestAnimatorEnvironment;

public class TestAnimatorMover : MonoBehaviour
{
	public TestAnimatorSkeleton[] skeletons;

	public Slider aimAngleSlider;

	public Slider speedSlider;

	public Slider levelSlider;

	public Slider tiltSlider;

	public Slider deltaRotationSlider;

	public Slider firstActionSlider;

	public Text frameLabel;

	public bool showDeltaLog;

	public bool thirdPersonParam = true;

	public bool replayRecord;

	public bool applyDeltaPositionOnRecord;

	public string recordFilePath;

	public bool autoMove;

	public bool autoProne;

	public bool autoDeltaRotation;

	public bool autoJump;

	public int targetFrameRate;

	private readonly int int_0 = Animator.StringToHash("Direct_X");

	private readonly int int_1 = Animator.StringToHash("Direct_Y");

	private readonly int int_2 = Animator.StringToHash("Inert");

	private readonly int int_3 = Animator.StringToHash("IsJumping");

	private readonly int int_4 = Animator.StringToHash("Landing");

	private readonly int int_5 = Animator.StringToHash("Tilt");

	private readonly int int_6 = Animator.StringToHash("Level");

	private readonly int int_7 = Animator.StringToHash("Aim_angle");

	private readonly int int_8 = Animator.StringToHash("Speed");

	private readonly int int_9 = Animator.StringToHash("Direct");

	private readonly int int_10 = Animator.StringToHash("ThirdPerson");

	private readonly int int_11 = Animator.StringToHash("Prone");

	private readonly int int_12 = Animator.StringToHash("ProneReload");

	private readonly int int_13 = Animator.StringToHash("DeltaRotation");

	private readonly int int_14 = Animator.StringToHash("Sprint");

	private readonly int int_15 = Animator.StringToHash("FirstAction");

	private readonly int int_16 = Animator.StringToHash("FootStep");

	private readonly int int_17 = Animator.StringToHash("FallingDown");

	private List<IAnimator> list_0 = new List<IAnimator>();

	private float float_0;

	private float float_1;

	private bool bool_0;

	private bool bool_1;

	private bool bool_2;

	private bool bool_3;

	private int int_18;

	public void Start()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
		list_0.Clear();
		TestAnimatorSkeleton[] array = skeletons;
		foreach (TestAnimatorSkeleton testAnimatorSkeleton in array)
		{
			testAnimatorSkeleton.Init();
			list_0.Add(testAnimatorSkeleton.Animator);
		}
	}

	public void Update()
	{
		TestAnimatorSkeleton[] array;
		if (Input.GetKeyDown(KeyCode.R))
		{
			array = skeletons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].transform.position = Vector3.zero;
			}
		}
		float num = (autoMove ? 1f : 0f);
		float num2 = (autoMove ? 1f : 0f);
		float value = tiltSlider.value;
		float value2 = levelSlider.value;
		float value3 = aimAngleSlider.value;
		if (Input.GetKey(KeyCode.W))
		{
			num2 = 1f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			num2 = -1f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			num = 1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			num = -1f;
		}
		if (Input.GetKey(KeyCode.Q))
		{
			value = -5f;
		}
		if (Input.GetKey(KeyCode.E))
		{
			value = 5f;
		}
		if (Input.GetKey(KeyCode.LeftControl))
		{
			value2 = 0f;
		}
		bool_1 = Input.GetKey(KeyCode.LeftShift);
		if (Input.GetKeyDown(KeyCode.C))
		{
			bool_0 = !bool_0;
		}
		if (autoProne)
		{
			bool_0 = true;
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			bool_2 = !bool_2;
		}
		bool flag = Input.GetKey(KeyCode.Space) || autoJump;
		bool flag2 = Math.Abs(num) > Mathf.Epsilon || Math.Abs(num2) > Mathf.Epsilon || flag;
		EMovementDirection eMovementDirection = GClass2076.ConvertToMovementDirection(new Vector2(0f - num, num2));
		if (!flag2)
		{
			num = float_0;
			num2 = float_1;
		}
		foreach (IAnimator item in list_0)
		{
			if (!(item is FastAnimatorProcessorClass) || !autoMove || bool_3)
			{
				item.SetFloat(int_0, num);
				item.SetFloat(int_1, num2);
				item.SetBool(int_2, flag2);
				item.SetBool(int_3, flag);
				item.SetBool(int_4, !flag);
				item.SetFloat(int_8, speedSlider.value);
				item.SetInteger(int_15, (int)firstActionSlider.value);
				item.SetFloat(int_5, value);
				item.SetFloat(int_6, value2);
				item.SetFloat(int_7, value3);
				item.SetBool(int_14, bool_1);
				item.SetFloat(int_13, autoDeltaRotation ? (-87.42857f) : deltaRotationSlider.value);
				item.SetBool(int_11, bool_0);
				item.SetBool(int_12, bool_2);
				item.SetBool(int_10, thirdPersonParam);
				item.SetBool("Kick", Input.GetKey(KeyCode.K));
				if (Input.GetKeyDown(KeyCode.L))
				{
					item.SetBool("KickSucceed", value: true);
					item.SetBool("Approached", value: true);
				}
				item.SetBool(int_17, value: true);
				if (item is FastAnimatorProcessorClass)
				{
					item.SetFloat(int_16, 0.1032162f);
				}
				item.SetFloat(int_9, 2f);
				if (eMovementDirection != EMovementDirection.None && flag2)
				{
					float value4 = GClass2076.InvertDirection(eMovementDirection);
					item.SetFloat(int_9, value4);
				}
			}
		}
		bool_3 = autoMove;
		array = skeletons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Process(Time.deltaTime);
		}
		method_0();
	}

	public void method_0()
	{
		if (!showDeltaLog)
		{
			return;
		}
		string text = "delta " + Vector3.Distance(skeletons[0].transform.position, skeletons[1].transform.position) + " ";
		bool flag = false;
		TestAnimatorSkeleton[] array = skeletons;
		foreach (TestAnimatorSkeleton testAnimatorSkeleton in array)
		{
			AnimatorStateInfoWrapper currentState = testAnimatorSkeleton.GetCurrentState();
			if (testAnimatorSkeleton.Animator.IsInTransition(0))
			{
				flag = true;
			}
			float num = 0f;
			if (flag)
			{
				num = testAnimatorSkeleton.GetNextState().normalizedTime;
			}
			Vector3 deltaPosition = testAnimatorSkeleton.GetDeltaPosition();
			float deltaTime = Time.deltaTime;
			Quaternion deltaRotation = testAnimatorSkeleton.Animator.deltaRotation;
			new Quaternion(deltaRotation.x / deltaTime, deltaRotation.y / deltaTime, deltaRotation.z / deltaTime, deltaRotation.w / deltaTime);
			string[] obj = new string[8] { text, " | time ", null, null, null, null, null, null };
			float normalizedTime = currentState.normalizedTime;
			obj[2] = normalizedTime.ToString();
			obj[3] = " | nextTime ";
			obj[4] = num.ToString();
			obj[5] = " | deltaPos ";
			obj[6] = method_1(deltaPosition);
			obj[7] = " #### ";
			text = string.Concat(obj);
		}
		Debug.Log(text);
	}

	public string method_1(Vector3 vec)
	{
		return "(" + vec.x + ", " + vec.y + ", " + vec.z + ") ";
	}

	public string method_2(Quaternion vec)
	{
		return "(" + vec.x + ", " + vec.y + ", " + vec.z + ", " + vec.w + ") ";
	}
}
