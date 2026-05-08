using System;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[Serializable]
public class BVH<T> where T : class, ISpatialItem
{
	[Serializable]
	public struct Node
	{
		public int Child1;

		public int Child2;

		public int Height;

		public int Parent;

		public int Next;

		public Bounds AABB;

		public T Object;

		public bool IsLeaf => Child1 == -1;
	}

	[NonSerialized]
	public const int EMPTY = -1;

	[NonSerialized]
	public const float MARGIN = 0f;

	[NonSerialized]
	public int Root = -1;

	[NonSerialized]
	public Node[] Nodes;

	[NonSerialized]
	public int NodeCount_1;

	[NonSerialized]
	public int NodeCapacity_1;

	[NonSerialized]
	public int FreeList;

	[NonSerialized]
	public int InCnt;

	[NonSerialized]
	public Stack<int> GrowableStack = new Stack<int>(256);

	public int NodeCount => NodeCount_1;

	public int NodeCapacity => NodeCapacity_1;

	public void Initialize(int capacity)
	{
		Root = -1;
		NodeCapacity_1 = capacity;
		NodeCount_1 = 0;
		Nodes = new Node[NodeCapacity_1];
		for (int i = 0; i < NodeCapacity_1 - 1; i++)
		{
			Nodes[i].Next = i + 1;
			Nodes[i].Height = -1;
		}
		Nodes[NodeCapacity_1 - 1].Next = -1;
		Nodes[NodeCapacity_1 - 1].Height = -1;
		FreeList = 0;
		InCnt = 0;
	}

	public int Add(T obj)
	{
		int num = method_0();
		Bounds spatialItemBounds = obj.SpatialItemBounds;
		spatialItemBounds.Expand(0f);
		Nodes[num].AABB = spatialItemBounds;
		Nodes[num].Height = 0;
		Nodes[num].Object = obj;
		method_2(num);
		return num;
	}

	public int OverlapNoAlloc(Bounds bounds, T[] outputArray)
	{
		int num = 0;
		if (NodeCount_1 == 0)
		{
			return 0;
		}
		GrowableStack.Clear();
		GrowableStack.Push(Root);
		while (true)
		{
			if (GrowableStack.Count > 0)
			{
				int num2 = GrowableStack.Pop();
				if (num2 == -1 || !Nodes[num2].AABB.Intersects(bounds))
				{
					continue;
				}
				if (Nodes[num2].IsLeaf)
				{
					outputArray[num] = Nodes[num2].Object;
					num++;
					if (num == outputArray.Length)
					{
						break;
					}
				}
				else
				{
					GrowableStack.Push(Nodes[num2].Child1);
					GrowableStack.Push(Nodes[num2].Child2);
				}
				continue;
			}
			return num;
		}
		return num;
	}

	public IEnumerable<T> EnumerateOverlappingLeafNodes(Bounds bounds)
	{
		if (NodeCount_1 == 0)
		{
			yield break;
		}
		GrowableStack.Clear();
		GrowableStack.Push(Root);
		while (GrowableStack.Count > 0)
		{
			int num = GrowableStack.Pop();
			if (num != -1 && Nodes[num].AABB.Intersects(bounds))
			{
				if (!Nodes[num].IsLeaf)
				{
					GrowableStack.Push(Nodes[num].Child1);
					GrowableStack.Push(Nodes[num].Child2);
				}
				else
				{
					yield return Nodes[num].Object;
				}
			}
		}
	}

	public T QueryNearest(Vector3 point, float radius)
	{
		if (NodeCount_1 == 0)
		{
			return null;
		}
		float num = -1f;
		T result = null;
		Bounds bounds = new Bounds(point, Vector3.one * radius);
		foreach (T item in EnumerateOverlappingLeafNodes(bounds))
		{
			if (!item.SpatialItemBounds.Contains(point))
			{
				float num2 = item.SpatialItemBounds.SqrDistance(point);
				if (num2 < num || num < 0f)
				{
					num = num2;
					result = item;
				}
				continue;
			}
			return item;
		}
		return result;
	}

	public T Query(Vector3 point)
	{
		if (NodeCount_1 == 0)
		{
			return null;
		}
		GrowableStack.Clear();
		GrowableStack.Push(Root);
		int num;
		while (true)
		{
			if (GrowableStack.Count > 0)
			{
				num = GrowableStack.Pop();
				if (num != -1 && Nodes[num].AABB.Contains(point))
				{
					if (Nodes[num].IsLeaf)
					{
						break;
					}
					GrowableStack.Push(Nodes[num].Child1);
					GrowableStack.Push(Nodes[num].Child2);
				}
				continue;
			}
			return null;
		}
		return Nodes[num].Object;
	}

	public int method_0()
	{
		if (FreeList == -1)
		{
			Node[] nodes = Nodes;
			NodeCapacity_1 *= 2;
			Nodes = new Node[NodeCapacity_1];
			for (int i = 0; i < nodes.Length; i++)
			{
				Nodes[i] = nodes[i];
			}
			for (int j = NodeCount_1; j < NodeCapacity_1 - 1; j++)
			{
				Nodes[j].Next = j + 1;
				Nodes[j].Height = -1;
			}
			Nodes[NodeCapacity_1 - 1].Next = -1;
			Nodes[NodeCapacity_1 - 1].Height = -1;
			FreeList = NodeCount_1;
		}
		int freeList = FreeList;
		FreeList = Nodes[freeList].Next;
		Nodes[freeList].Parent = -1;
		Nodes[freeList].Child1 = -1;
		Nodes[freeList].Child2 = -1;
		Nodes[freeList].Height = 0;
		Nodes[freeList].Next = 0;
		Nodes[freeList].Object = null;
		NodeCount_1++;
		return freeList;
	}

	public void method_1(int key)
	{
		Nodes[key].Next = FreeList;
		Nodes[key].Height = -1;
		FreeList = key;
		NodeCount_1--;
	}

	public void method_2(int leaf)
	{
		InCnt++;
		if (Root == -1)
		{
			Root = leaf;
			Nodes[Root].Parent = -1;
			return;
		}
		Bounds aABB = Nodes[leaf].AABB;
		int num = Root;
		while (!Nodes[num].IsLeaf)
		{
			int child = Nodes[num].Child1;
			int child2 = Nodes[num].Child2;
			float surfaceArea = GClass1242.GetSurfaceArea(Nodes[num].AABB);
			Bounds aABB2 = Nodes[num].AABB;
			aABB2.Encapsulate(aABB);
			float surfaceArea2 = GClass1242.GetSurfaceArea(aABB2);
			float num2 = 2f * surfaceArea2;
			float num3 = 2f * (surfaceArea2 - surfaceArea);
			float num4;
			if (Nodes[child].IsLeaf)
			{
				Bounds box = aABB;
				box.Encapsulate(Nodes[child].AABB);
				num4 = GClass1242.GetSurfaceArea(box) + num3;
			}
			else
			{
				Bounds box2 = aABB;
				box2.Encapsulate(Nodes[child].AABB);
				float surfaceArea3 = GClass1242.GetSurfaceArea(Nodes[child].AABB);
				num4 = GClass1242.GetSurfaceArea(box2) - surfaceArea3 + num3;
			}
			float num5;
			if (Nodes[child2].IsLeaf)
			{
				Bounds box3 = aABB;
				box3.Encapsulate(Nodes[child2].AABB);
				num5 = GClass1242.GetSurfaceArea(box3) + num3;
			}
			else
			{
				Bounds box4 = aABB;
				box4.Encapsulate(Nodes[child2].AABB);
				float surfaceArea4 = GClass1242.GetSurfaceArea(Nodes[child2].AABB);
				num5 = GClass1242.GetSurfaceArea(box4) - surfaceArea4 + num3;
			}
			if (num2 < num4 && num2 < num5)
			{
				break;
			}
			num = ((!(num4 < num5)) ? child2 : child);
		}
		int num6 = num;
		int parent = Nodes[num6].Parent;
		int num7 = method_0();
		Nodes[num7].Parent = parent;
		Nodes[num7].Object = null;
		Bounds aABB3 = aABB;
		aABB3.Encapsulate(Nodes[num6].AABB);
		Nodes[num7].AABB = aABB3;
		Nodes[num7].Height = Nodes[num6].Height + 1;
		if (parent != -1)
		{
			if (Nodes[parent].Child1 == num6)
			{
				Nodes[parent].Child1 = num7;
			}
			else
			{
				Nodes[parent].Child2 = num7;
			}
			Nodes[num7].Child1 = num6;
			Nodes[num7].Child2 = leaf;
			Nodes[num6].Parent = num7;
			Nodes[leaf].Parent = num7;
		}
		else
		{
			Nodes[num7].Child1 = num6;
			Nodes[num7].Child2 = leaf;
			Nodes[num6].Parent = num7;
			Nodes[leaf].Parent = num7;
			Root = num7;
		}
		for (num = Nodes[leaf].Parent; num != -1; num = Nodes[num].Parent)
		{
			num = method_3(num);
			int child3 = Nodes[num].Child1;
			int child4 = Nodes[num].Child2;
			Nodes[num].Height = 1 + Mathf.Max(Nodes[child3].Height, Nodes[child4].Height);
			Bounds aABB4 = Nodes[child3].AABB;
			aABB4.Encapsulate(Nodes[child4].AABB);
			Nodes[num].AABB = aABB4;
		}
	}

	public int method_3(int iA)
	{
		if (!Nodes[iA].IsLeaf && Nodes[iA].Height >= 2)
		{
			int child = Nodes[iA].Child1;
			int child2 = Nodes[iA].Child2;
			int num = Nodes[child2].Height - Nodes[child].Height;
			if (num > 1)
			{
				int child3 = Nodes[child2].Child1;
				int child4 = Nodes[child2].Child2;
				Nodes[child2].Child1 = iA;
				Nodes[child2].Parent = Nodes[iA].Parent;
				Nodes[iA].Parent = child2;
				if (Nodes[child2].Parent != -1)
				{
					if (Nodes[Nodes[child2].Parent].Child1 == iA)
					{
						Nodes[Nodes[child2].Parent].Child1 = child2;
					}
					else
					{
						Nodes[Nodes[child2].Parent].Child2 = child2;
					}
				}
				else
				{
					Root = child2;
				}
				if (Nodes[child3].Height > Nodes[child4].Height)
				{
					Nodes[child2].Child2 = child3;
					Nodes[iA].Child2 = child4;
					Nodes[child4].Parent = iA;
					Bounds aABB = Nodes[child].AABB;
					aABB.Encapsulate(Nodes[child4].AABB);
					Nodes[iA].AABB = aABB;
					aABB = Nodes[iA].AABB;
					aABB.Encapsulate(Nodes[child3].AABB);
					Nodes[child2].AABB = aABB;
					Nodes[iA].Height = 1 + Mathf.Max(Nodes[child].Height, Nodes[child4].Height);
					Nodes[child2].Height = 1 + Mathf.Max(Nodes[iA].Height, Nodes[child3].Height);
				}
				else
				{
					Nodes[child2].Child2 = child4;
					Nodes[iA].Child2 = child3;
					Nodes[child3].Parent = iA;
					Bounds aABB2 = Nodes[child].AABB;
					aABB2.Encapsulate(Nodes[child3].AABB);
					Nodes[iA].AABB = aABB2;
					aABB2 = Nodes[iA].AABB;
					aABB2.Encapsulate(Nodes[child4].AABB);
					Nodes[child2].AABB = aABB2;
					Nodes[iA].Height = 1 + Mathf.Max(Nodes[child].Height, Nodes[child3].Height);
					Nodes[child2].Height = 1 + Mathf.Max(Nodes[iA].Height, Nodes[child4].Height);
				}
				return child2;
			}
			if (num < -1)
			{
				int child5 = Nodes[child].Child1;
				int child6 = Nodes[child].Child2;
				Nodes[child].Child1 = iA;
				Nodes[child].Parent = Nodes[iA].Parent;
				Nodes[iA].Parent = child;
				if (Nodes[child].Parent != -1)
				{
					if (Nodes[Nodes[child].Parent].Child1 == iA)
					{
						Nodes[Nodes[child].Parent].Child1 = child;
					}
					else
					{
						Nodes[Nodes[child].Parent].Child2 = child;
					}
				}
				else
				{
					Root = child;
				}
				if (Nodes[child5].Height > Nodes[child6].Height)
				{
					Nodes[child].Child2 = child5;
					Nodes[iA].Child1 = child6;
					Nodes[child6].Parent = iA;
					Bounds aABB3 = Nodes[child2].AABB;
					aABB3.Encapsulate(Nodes[child6].AABB);
					Nodes[iA].AABB = aABB3;
					aABB3 = Nodes[iA].AABB;
					aABB3.Encapsulate(Nodes[child5].AABB);
					Nodes[child].AABB = aABB3;
					Nodes[iA].Height = 1 + Mathf.Max(Nodes[child2].Height, Nodes[child6].Height);
					Nodes[child].Height = 1 + Mathf.Max(Nodes[iA].Height, Nodes[child5].Height);
				}
				else
				{
					Nodes[child].Child2 = child6;
					Nodes[iA].Child1 = child5;
					Nodes[child5].Parent = iA;
					Bounds aABB4 = Nodes[child2].AABB;
					aABB4.Encapsulate(Nodes[child5].AABB);
					Nodes[iA].AABB = aABB4;
					aABB4 = Nodes[iA].AABB;
					aABB4.Encapsulate(Nodes[child6].AABB);
					Nodes[child].AABB = aABB4;
					Nodes[iA].Height = 1 + Mathf.Max(Nodes[child2].Height, Nodes[child5].Height);
					Nodes[child].Height = 1 + Mathf.Max(Nodes[iA].Height, Nodes[child6].Height);
				}
				return child;
			}
			return iA;
		}
		return iA;
	}

	public int method_4(int nodeId)
	{
		if (Nodes[nodeId].IsLeaf)
		{
			return 0;
		}
		int a = method_4(Nodes[nodeId].Child1);
		int b = method_4(Nodes[nodeId].Child2);
		return 1 + Mathf.Max(a, b);
	}

	public void method_5(int index)
	{
		if (index != -1)
		{
			int child = Nodes[index].Child1;
			int child2 = Nodes[index].Child2;
			if (!Nodes[index].IsLeaf)
			{
				method_5(child);
				method_5(child2);
			}
		}
	}

	public void method_6(int index)
	{
		if (index != -1)
		{
			int child = Nodes[index].Child1;
			int child2 = Nodes[index].Child2;
			if (!Nodes[index].IsLeaf)
			{
				int height = Nodes[child].Height;
				int height2 = Nodes[child2].Height;
				Mathf.Max(height, height2);
				Bounds aABB = Nodes[child].AABB;
				aABB.Encapsulate(Nodes[child2].AABB);
				method_6(child);
				method_6(child2);
			}
		}
	}
}
