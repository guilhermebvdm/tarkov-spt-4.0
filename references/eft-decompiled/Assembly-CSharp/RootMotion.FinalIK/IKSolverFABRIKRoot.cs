using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverFABRIKRoot : IKSolver
{
	public int iterations = 4;

	[Range(0f, 1f)]
	public float rootPin;

	public FABRIKChain[] chains = new FABRIKChain[0];

	[NonSerialized]
	public bool ZeroWeightApplied;

	[NonSerialized]
	public bool[] IsRoot;

	[NonSerialized]
	public Vector3 RootDefaultPosition;

	public override bool IsValid(ref string message)
	{
		if (chains.Length == 0)
		{
			message = "IKSolverFABRIKRoot contains no chains.";
			return false;
		}
		FABRIKChain[] array = chains;
		int num = 0;
		while (true)
		{
			if (num < array.Length)
			{
				if (!array[num].IsValid(ref message))
				{
					break;
				}
				num++;
				continue;
			}
			for (int i = 0; i < chains.Length; i++)
			{
				for (int j = 0; j < chains.Length; j++)
				{
					if (i != j && chains[i].ik == chains[j].ik)
					{
						message = chains[i].ik.name + " is represented more than once in IKSolverFABRIKRoot chain.";
						return false;
					}
				}
			}
			for (int k = 0; k < chains.Length; k++)
			{
				for (int l = 0; l < chains[k].children.Length; l++)
				{
					int num2 = chains[k].children[l];
					if (num2 >= 0)
					{
						if (num2 != k)
						{
							if (num2 < chains.Length)
							{
								for (int m = 0; m < chains.Length; m++)
								{
									if (num2 != m)
									{
										continue;
									}
									for (int n = 0; n < chains[m].children.Length; n++)
									{
										if (chains[m].children[n] == k)
										{
											message = "Circular parenting. " + chains[m].ik.name + " already has " + chains[k].ik.name + " listed as it's child.";
											return false;
										}
									}
								}
								for (int num3 = 0; num3 < chains[k].children.Length; num3++)
								{
									if (l != num3 && chains[k].children[num3] == num2)
									{
										message = "Chain number " + num2 + " is represented more than once in the children of " + chains[k].ik.name;
										return false;
									}
								}
								continue;
							}
							message = chains[k].ik.name + "IKSolverFABRIKRoot chain at index " + k + " has invalid children array. Child index > number of chains";
							return false;
						}
						message = chains[k].ik.name + "IKSolverFABRIKRoot chain at index " + k + " has invalid children array. Child index is referencing to itself.";
						return false;
					}
					message = chains[k].ik.name + "IKSolverFABRIKRoot chain at index " + k + " has invalid children array. Child index is < 0.";
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public override void StoreDefaultLocalState()
	{
		RootDefaultPosition = root.localPosition;
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].ik.solver.StoreDefaultLocalState();
		}
	}

	public override void FixTransforms()
	{
		root.localPosition = RootDefaultPosition;
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].ik.solver.FixTransforms();
		}
	}

	public override void OnInitiate()
	{
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].Initiate();
		}
		IsRoot = new bool[chains.Length];
		for (int j = 0; j < chains.Length; j++)
		{
			IsRoot[j] = method_0(j);
		}
	}

	public bool method_0(int index)
	{
		for (int i = 0; i < chains.Length; i++)
		{
			for (int j = 0; j < chains[i].children.Length; j++)
			{
				if (chains[i].children[j] == index)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void OnUpdate()
	{
		if (IKPositionWeight <= 0f && ZeroWeightApplied)
		{
			return;
		}
		IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
		for (int i = 0; i < chains.Length; i++)
		{
			chains[i].ik.solver.IKPositionWeight = IKPositionWeight;
		}
		if (IKPositionWeight <= 0f)
		{
			ZeroWeightApplied = true;
			return;
		}
		ZeroWeightApplied = false;
		for (int j = 0; j < iterations; j++)
		{
			for (int k = 0; k < chains.Length; k++)
			{
				if (IsRoot[k])
				{
					chains[k].Stage1(chains);
				}
			}
			Vector3 vector = method_2();
			root.position = vector;
			for (int l = 0; l < chains.Length; l++)
			{
				if (IsRoot[l])
				{
					chains[l].Stage2(vector, chains);
				}
			}
		}
	}

	public override Point[] GetPoints()
	{
		Point[] array = new Point[0];
		for (int i = 0; i < chains.Length; i++)
		{
			method_1(ref array, chains[i]);
		}
		return array;
	}

	public override Point GetPoint(Transform transform)
	{
		Point point = null;
		int num = 0;
		while (true)
		{
			if (num < chains.Length)
			{
				point = chains[num].ik.solver.GetPoint(transform);
				if (point != null)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return point;
	}

	public void method_1(ref Point[] array, FABRIKChain chain)
	{
		Point[] points = chain.ik.solver.GetPoints();
		Array.Resize(ref array, array.Length + points.Length);
		int num = 0;
		for (int i = array.Length - points.Length; i < array.Length; i++)
		{
			array[i] = points[num];
			num++;
		}
	}

	public Vector3 method_2()
	{
		Vector3 position = root.position;
		if (rootPin >= 1f)
		{
			return position;
		}
		float num = 0f;
		for (int i = 0; i < chains.Length; i++)
		{
			if (IsRoot[i])
			{
				num += chains[i].pull;
			}
		}
		for (int j = 0; j < chains.Length; j++)
		{
			if (IsRoot[j] && num > 0f)
			{
				position += (chains[j].ik.solver.bones[0].solverPosition - root.position) * (chains[j].pull / Mathf.Clamp(num, 1f, num));
			}
		}
		return Vector3.Lerp(position, root.position, rootPin);
	}
}
