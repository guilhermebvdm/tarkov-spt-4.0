using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public abstract class GClass1255
{
	public class Class863
	{
		[NonSerialized]
		public const string String_0 = "m_Collider";

		[NonSerialized]
		public int Int_0;

		public Class863()
		{
			Int_0 = -1;
			FieldInfo[] fields = typeof(RaycastHit).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			int num = 0;
			FieldInfo fieldInfo;
			while (true)
			{
				if (num < fields.Length)
				{
					fieldInfo = fields[num];
					if (fieldInfo.Name == "m_Collider")
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			Int_0 = UnsafeUtility.GetFieldOffset(fieldInfo);
		}

		public unsafe int GetColliderID(ref RaycastHit hit)
		{
			return *(int*)((byte*)UnsafeUtility.AddressOf(ref hit) + Int_0);
		}
	}

	[NonSerialized]
	public static Class863 Class863_0 = new Class863();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetColliderID(this RaycastHit hit)
	{
		return Class863_0.GetColliderID(ref hit);
	}
}
