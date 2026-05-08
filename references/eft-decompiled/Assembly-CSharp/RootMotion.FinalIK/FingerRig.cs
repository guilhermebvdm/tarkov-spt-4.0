using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RootMotion.FinalIK;

public class FingerRig : SolverManager
{
	[Tooltip("The master weight for all fingers.")]
	[Range(0f, 1f)]
	public float weight = 1f;

	public Finger[] fingers = new Finger[0];

	[CompilerGenerated]
	private bool bool_3;

	public bool initiated
	{
		[CompilerGenerated]
		get
		{
			return bool_3;
		}
		[CompilerGenerated]
		set
		{
			bool_3 = value;
		}
	}

	public bool IsValid(ref string errorMessage)
	{
		Finger[] array = fingers;
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				if (!array[num].IsValid(ref errorMessage))
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		return false;
	}

	[ContextMenu("Auto-detect")]
	public void AutoDetect()
	{
		fingers = new Finger[0];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform[] array = new Transform[0];
			method_3(base.transform.GetChild(i), ref array);
			if (array.Length == 3 || array.Length == 4)
			{
				Finger finger = new Finger();
				finger.bone1 = array[0];
				finger.bone2 = array[1];
				if (array.Length == 3)
				{
					finger.tip = array[2];
				}
				else
				{
					finger.bone3 = array[2];
					finger.tip = array[3];
				}
				finger.weight = 1f;
				Array.Resize(ref fingers, fingers.Length + 1);
				fingers[fingers.Length - 1] = finger;
			}
		}
	}

	public void AddFinger(Transform bone1, Transform bone2, Transform bone3, Transform tip, Transform target = null)
	{
		Finger finger = new Finger();
		finger.bone1 = bone1;
		finger.bone2 = bone2;
		finger.bone3 = bone3;
		finger.tip = tip;
		finger.target = target;
		Array.Resize(ref fingers, fingers.Length + 1);
		fingers[fingers.Length - 1] = finger;
		initiated = false;
		finger.Initiate(base.transform, fingers.Length - 1);
		if (fingers[fingers.Length - 1].initiated)
		{
			initiated = true;
		}
	}

	public void RemoveFinger(int index)
	{
		if (!((float)index < 0f) && index < fingers.Length)
		{
			if (fingers.Length == 1)
			{
				fingers = new Finger[0];
				return;
			}
			Finger[] array = new Finger[fingers.Length - 1];
			int num = 0;
			for (int i = 0; i < fingers.Length; i++)
			{
				if (i != index)
				{
					array[num] = fingers[i];
					num++;
				}
			}
			fingers = array;
		}
		else
		{
			GClass1465.Log("RemoveFinger index out of bounds.", base.transform);
		}
	}

	public void method_3(Transform parent, ref Transform[] array)
	{
		Array.Resize(ref array, array.Length + 1);
		array[array.Length - 1] = parent;
		if (parent.childCount == 1)
		{
			method_3(parent.GetChild(0), ref array);
		}
	}

	public override void InitiateSolver()
	{
		initiated = true;
		for (int i = 0; i < fingers.Length; i++)
		{
			fingers[i].Initiate(base.transform, i);
			if (!fingers[i].initiated)
			{
				initiated = false;
			}
		}
	}

	public void UpdateFingerSolvers()
	{
		if (!(weight <= 0f))
		{
			Finger[] array = fingers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Update(weight);
			}
		}
	}

	public void FixFingerTransforms()
	{
		Finger[] array = fingers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FixTransforms();
		}
	}

	public override void UpdateSolver()
	{
		UpdateFingerSolvers();
	}

	public override void FixTransforms()
	{
		FixFingerTransforms();
	}
}
