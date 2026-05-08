using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InteractionReciver : MonoBehaviour
{
	public class Class694
	{
		[NonSerialized]
		public int Int_0;

		[NonSerialized]
		public HashSet<InteractionReciver> HashSet_0 = new HashSet<InteractionReciver>();

		public bool IsEmpty => HashSet_0.Count == 0;

		public Class694(int coordinate)
		{
			Int_0 = coordinate;
		}

		public void Add(InteractionReciver reciver)
		{
			HashSet_0.Add(reciver);
		}

		public void Remove(InteractionReciver reciver)
		{
			HashSet_0.Remove(reciver);
			if (HashSet_0.Count == 0)
			{
				dictionary_0.Remove(Int_0);
			}
		}

		public void FillRecivers(LinkedList<InteractionReciver> recivers)
		{
			foreach (InteractionReciver item in HashSet_0)
			{
				recivers.AddLast(item);
			}
		}
	}

	public const float Size = 4f;

	private const float float_0 = 0.25f;

	private static Dictionary<int, Class694> dictionary_0 = new Dictionary<int, Class694>();

	[CompilerGenerated]
	private int int_0;

	[CompilerGenerated]
	private Vector3 vector3_0;

	public int Hash
	{
		[CompilerGenerated]
		get
		{
			return int_0;
		}
		[CompilerGenerated]
		set
		{
			int_0 = value;
		}
	}

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return vector3_0;
		}
		[CompilerGenerated]
		set
		{
			vector3_0 = value;
		}
	}

	public static int GetHash(Vector3 worldPosition)
	{
		int num = (int)(worldPosition.x * 0.25f);
		int num2 = (int)(worldPosition.y * 0.25f);
		int num3 = num & 0xFFFF;
		num2 &= 0xFFFF;
		return num3 | (num2 << 16);
	}

	public static int GetHash(XYCellSizeStruct worldPosition)
	{
		int x = worldPosition.X;
		int y = worldPosition.Y;
		int num = x & 0xFFFF;
		y &= 0xFFFF;
		return num | (y << 16);
	}

	public static XYCellSizeStruct GetCoordinates(Vector3 worldPosition)
	{
		return new XYCellSizeStruct((int)(worldPosition.x * 0.25f), (int)(worldPosition.y * 0.25f));
	}

	public static bool IsEmpty(XYCellSizeStruct worldPosition)
	{
		int hash = GetHash(worldPosition);
		if (!dictionary_0.TryGetValue(hash, out var value))
		{
			return true;
		}
		return value.IsEmpty;
	}

	public static void FillRecivers(XYCellSizeStruct worldPosition, LinkedList<InteractionReciver> recivers)
	{
		int hash = GetHash(worldPosition);
		if (dictionary_0.TryGetValue(hash, out var value) && !value.IsEmpty)
		{
			value.FillRecivers(recivers);
		}
	}

	public static Class694 smethod_0(int hash)
	{
		if (!dictionary_0.TryGetValue(hash, out var value))
		{
			value = new Class694(hash);
			dictionary_0.Add(hash, value);
		}
		return value;
	}

	public void Start()
	{
		Position = base.transform.position;
		Hash = GetHash(Position);
		smethod_0(Hash).Add(this);
	}

	public void method_0()
	{
		smethod_0(Hash).Remove(this);
	}
}
