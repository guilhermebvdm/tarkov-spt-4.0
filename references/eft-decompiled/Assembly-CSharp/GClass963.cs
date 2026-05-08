using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass963 : GClass962<StaticDeferredDecal>
{
	public struct GStruct65
	{
		public Texture2D Texture;

		public float MaxSize;

		public Vector3 Position;

		public Vector4 uvStartEnd;
	}

	public delegate void GDelegate34(CommandBuffer cbBuf);

	public CommandBuffer _subtreeCommandBuffer;

	public CommandBuffer _ownDecalsCommandBuffer;

	[NonSerialized]
	public SortedList<string, GClass969> SortedList_0;

	[NonSerialized]
	public SortedList<string, GClass969> SortedList_1;

	[NonSerialized]
	public Bounds Bounds_0;

	[NonSerialized]
	public static Bounds Bounds_1 = smethod_2();

	public GStruct65 method_4(StaticDeferredDecal decal)
	{
		Vector3 localScale = decal.transform.localScale;
		return new GStruct65
		{
			Texture = (decal.DecalMaterial.mainTexture as Texture2D),
			MaxSize = Mathf.Max(Mathf.Max(localScale.x, localScale.y), localScale.z),
			Position = decal.transform.position,
			uvStartEnd = decal.GetUVStartEnd()
		};
	}

	public GClass963(GClass963 parent, Rect bounds)
		: base((GClass962<StaticDeferredDecal>)parent, bounds)
	{
	}

	public override GClass962<StaticDeferredDecal> CreateQuadrant(GClass962<StaticDeferredDecal> parent, Rect bounds)
	{
		return new GClass963(parent as GClass963, bounds);
	}

	public static Bounds smethod_2()
	{
		return new Bounds
		{
			min = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
			max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity)
		};
	}

	public void method_5(GClass963 quadrant, Mesh mesh, GDelegate34 preDrawCallback, ref List<StaticDeferredDecal> decalsList, ref List<GStruct65> texturesList, ref Bounds boundsToSet)
	{
		if (quadrant == null)
		{
			return;
		}
		var (collection, collection2, b) = quadrant.ConstructSubtreeCommandBuffers(mesh, preDrawCallback);
		decalsList.AddRange(collection);
		texturesList.AddRange(collection2);
		if (!method_6(ref b))
		{
			if (method_6(ref boundsToSet))
			{
				boundsToSet = new Bounds(b.center, b.size);
			}
			else
			{
				boundsToSet.Encapsulate(b);
			}
		}
	}

	public bool method_6(ref Bounds b)
	{
		if (!float.IsNaN(b.size.x) && !float.IsNaN(b.size.y))
		{
			return float.IsNaN(b.size.z);
		}
		return true;
	}

	public (List<StaticDeferredDecal>, List<GStruct65>, Bounds) ConstructSubtreeCommandBuffers(Mesh mesh, GDelegate34 preDrawCallback)
	{
		Bounds_0 = Bounds_1;
		List<StaticDeferredDecal> decalsList = new List<StaticDeferredDecal>();
		List<StaticDeferredDecal> decalsList2 = new List<StaticDeferredDecal>();
		List<StaticDeferredDecal> decalsList3 = new List<StaticDeferredDecal>();
		List<StaticDeferredDecal> decalsList4 = new List<StaticDeferredDecal>();
		List<StaticDeferredDecal> list = new List<StaticDeferredDecal>();
		List<GStruct65> texturesList = new List<GStruct65>();
		List<GStruct65> texturesList2 = new List<GStruct65>();
		List<GStruct65> texturesList3 = new List<GStruct65>();
		List<GStruct65> texturesList4 = new List<GStruct65>();
		List<GStruct65> list2 = new List<GStruct65>();
		method_5(Gclass962_1 as GClass963, mesh, preDrawCallback, ref decalsList, ref texturesList, ref Bounds_0);
		method_5(Gclass962_2 as GClass963, mesh, preDrawCallback, ref decalsList2, ref texturesList2, ref Bounds_0);
		method_5(Gclass962_3 as GClass963, mesh, preDrawCallback, ref decalsList3, ref texturesList3, ref Bounds_0);
		method_5(Gclass962_4 as GClass963, mesh, preDrawCallback, ref decalsList4, ref texturesList4, ref Bounds_0);
		list.AddRange(decalsList);
		list.AddRange(decalsList2);
		list.AddRange(decalsList3);
		list.AddRange(decalsList4);
		list2.AddRange(texturesList);
		list2.AddRange(texturesList2);
		list2.AddRange(texturesList3);
		list2.AddRange(texturesList4);
		if (List_0 != null)
		{
			List<StaticDeferredDecal> list3 = new List<StaticDeferredDecal>();
			Vector3 minPoint = Vector3.zero;
			Vector3 maxPoint = Vector3.zero;
			ComputeDecalMinMaxPoints(List_0[0].Node, ref minPoint, ref maxPoint, mesh);
			Bounds b = new Bounds((minPoint + maxPoint) * 0.5f, maxPoint - minPoint);
			foreach (GClass961<StaticDeferredDecal> item in List_0)
			{
				list.Add(item.Node);
				list3.Add(item.Node);
				ComputeDecalMinMaxPoints(item.Node, ref minPoint, ref maxPoint, mesh);
				b.Encapsulate(new Bounds((minPoint + maxPoint) * 0.5f, maxPoint - minPoint));
				list2.Add(method_4(item.Node));
			}
			GenerateOwnCommandBufferForQuadrant(this, list3, mesh, preDrawCallback);
			_ownDecalsCommandBuffer.name = "Own_" + base.Rect_0.min.ToString() + "_" + base.Rect_0.max.ToString();
			if (!method_6(ref b))
			{
				if (method_6(ref Bounds_0))
				{
					Bounds_0 = new Bounds(b.center, b.size);
				}
				else
				{
					Bounds_0.Encapsulate(b);
				}
			}
		}
		if (list.Count > 0)
		{
			GenerateCommandBufferForQuadrant(this, list, mesh, preDrawCallback);
			_subtreeCommandBuffer.name = "Subtree_" + base.Rect_0.min.ToString() + "_" + base.Rect_0.max.ToString();
		}
		return (list, list2, Bounds_0);
	}

	public void GetCommandBuffers(GClass2380 frustum, ref List<CommandBuffer> commandBuffers, int maxDepth)
	{
		GClass2380.IntersectionType boxIntersection = frustum.GetBoxIntersection(ref Bounds_0);
		bool flag = maxDepth > 0 && (Gclass962_1 != null || Gclass962_2 != null || Gclass962_3 != null || Gclass962_4 != null);
		switch (boxIntersection)
		{
		case GClass2380.IntersectionType.Contains:
			if (_subtreeCommandBuffer != null)
			{
				commandBuffers.Add(_subtreeCommandBuffer);
			}
			break;
		case GClass2380.IntersectionType.Intersects:
			if (flag)
			{
				if (Gclass962_1 != null)
				{
					(Gclass962_1 as GClass963).GetCommandBuffers(frustum, ref commandBuffers, maxDepth - 1);
				}
				if (Gclass962_2 != null)
				{
					(Gclass962_2 as GClass963).GetCommandBuffers(frustum, ref commandBuffers, maxDepth - 1);
				}
				if (Gclass962_3 != null)
				{
					(Gclass962_3 as GClass963).GetCommandBuffers(frustum, ref commandBuffers, maxDepth - 1);
				}
				if (Gclass962_4 != null)
				{
					(Gclass962_4 as GClass963).GetCommandBuffers(frustum, ref commandBuffers, maxDepth - 1);
				}
			}
			else if (_subtreeCommandBuffer != null)
			{
				commandBuffers.Add(_subtreeCommandBuffer);
			}
			break;
		}
	}

	public void RecordCommandBuffer(GClass2380 frustum, CommandBuffer commandBuffer, int maxDepth, Mesh mesh)
	{
		GClass2380.IntersectionType boxIntersection = frustum.GetBoxIntersection(ref Bounds_0);
		bool flag = maxDepth > 0 && (Gclass962_1 != null || Gclass962_2 != null || Gclass962_3 != null || Gclass962_4 != null);
		switch (boxIntersection)
		{
		case GClass2380.IntersectionType.Contains:
			RecordCommandBufferForQuadrant(this, commandBuffer, mesh);
			break;
		case GClass2380.IntersectionType.Intersects:
			if (flag)
			{
				if (Gclass962_1 != null)
				{
					(Gclass962_1 as GClass963).RecordCommandBuffer(frustum, commandBuffer, maxDepth - 1, mesh);
				}
				if (Gclass962_2 != null)
				{
					(Gclass962_2 as GClass963).RecordCommandBuffer(frustum, commandBuffer, maxDepth - 1, mesh);
				}
				if (Gclass962_3 != null)
				{
					(Gclass962_3 as GClass963).RecordCommandBuffer(frustum, commandBuffer, maxDepth - 1, mesh);
				}
				if (Gclass962_4 != null)
				{
					(Gclass962_4 as GClass963).RecordCommandBuffer(frustum, commandBuffer, maxDepth - 1, mesh);
				}
			}
			else
			{
				RecordCommandBufferForQuadrant(this, commandBuffer, mesh);
			}
			break;
		}
	}

	public static string smethod_3(StaticDeferredDecal decal)
	{
		return decal.DecalQueue.ToString() + decal.DecalMaterial.GetHashCode();
	}

	public static Dictionary<string, List<StaticDeferredDecal>> smethod_4(List<StaticDeferredDecal> decals)
	{
		Dictionary<string, List<StaticDeferredDecal>> dictionary = new Dictionary<string, List<StaticDeferredDecal>>();
		foreach (StaticDeferredDecal decal in decals)
		{
			string key = smethod_3(decal);
			if (dictionary.ContainsKey(key))
			{
				dictionary[key].Add(decal);
				continue;
			}
			List<StaticDeferredDecal> list = new List<StaticDeferredDecal>();
			list.Add(decal);
			dictionary.Add(key, list);
		}
		return dictionary;
	}

	public static void GenerateCommandBufferForQuadrant(GClass963 q, List<StaticDeferredDecal> decals, Mesh mesh, GDelegate34 preDrawCallback)
	{
		Dictionary<string, List<StaticDeferredDecal>> dictionary = smethod_4(decals);
		q.method_7();
		q.SortedList_0 = new SortedList<string, GClass969>();
		if (q._subtreeCommandBuffer == null)
		{
			q._subtreeCommandBuffer = new CommandBuffer();
		}
		q._subtreeCommandBuffer.Clear();
		preDrawCallback?.Invoke(q._subtreeCommandBuffer);
		foreach (string key in dictionary.Keys)
		{
			List<StaticDeferredDecal> list = dictionary[key];
			GClass969 gClass = new GClass969();
			q.SortedList_0.Add(key, gClass);
			Material material = new Material(list[0].DecalMaterial);
			gClass.InitBuffers(material, list.Count, mesh);
			gClass.UpdateBuffers(list, mesh, q.Bounds_0);
		}
		foreach (KeyValuePair<string, GClass969> item in q.SortedList_0)
		{
			q._subtreeCommandBuffer.DrawMeshInstancedIndirect(mesh, 0, item.Value.Material, -1, item.Value.ArgsBuffer);
		}
	}

	public static void GenerateOwnCommandBufferForQuadrant(GClass963 q, List<StaticDeferredDecal> decals, Mesh mesh, GDelegate34 preDrawCallback)
	{
		Dictionary<string, List<StaticDeferredDecal>> dictionary = smethod_4(decals);
		q.method_8();
		q.SortedList_1 = new SortedList<string, GClass969>();
		if (q._ownDecalsCommandBuffer == null)
		{
			q._ownDecalsCommandBuffer = new CommandBuffer();
		}
		q._ownDecalsCommandBuffer.Clear();
		preDrawCallback?.Invoke(q._ownDecalsCommandBuffer);
		foreach (string key in dictionary.Keys)
		{
			List<StaticDeferredDecal> list = dictionary[key];
			GClass969 gClass = new GClass969();
			q.SortedList_1.Add(key, gClass);
			Material material = new Material(list[0].DecalMaterial);
			gClass.InitBuffers(material, list.Count, mesh);
			gClass.UpdateBuffers(list, mesh, q.Bounds_0);
		}
		foreach (KeyValuePair<string, GClass969> item in q.SortedList_1)
		{
			q._ownDecalsCommandBuffer.DrawMeshInstancedIndirect(mesh, 0, item.Value.Material, -1, item.Value.ArgsBuffer);
		}
	}

	public static void RecordCommandBufferForQuadrant(GClass963 q, CommandBuffer cmdBuf, Mesh mesh)
	{
		if (q.SortedList_0 != null)
		{
			foreach (KeyValuePair<string, GClass969> item in q.SortedList_0)
			{
				cmdBuf.DrawMeshInstancedIndirect(mesh, 0, item.Value.Material, -1, item.Value.ArgsBuffer);
			}
		}
		if (q.SortedList_1 == null)
		{
			return;
		}
		foreach (KeyValuePair<string, GClass969> item2 in q.SortedList_1)
		{
			cmdBuf.DrawMeshInstancedIndirect(mesh, 0, item2.Value.Material, -1, item2.Value.ArgsBuffer);
		}
	}

	public static void ComputeDecalMinMaxPoints(StaticDeferredDecal decal, ref Vector3 minPoint, ref Vector3 maxPoint, Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		Matrix4x4 localToWorldMatrix = decal.transform.localToWorldMatrix;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = localToWorldMatrix.MultiplyPoint(vertices[i]);
		}
		Vector3 vector = vertices[0];
		Vector3 vector2 = vertices[0];
		Vector3[] array = vertices;
		foreach (Vector3 rhs in array)
		{
			vector = Vector3.Min(vector, rhs);
			vector2 = Vector3.Max(vector2, rhs);
		}
		minPoint = vector;
		maxPoint = vector2;
	}

	public void method_7()
	{
		if (SortedList_0 == null)
		{
			return;
		}
		foreach (KeyValuePair<string, GClass969> item in SortedList_0)
		{
			item.Value.DestroyBuffers();
		}
		SortedList_0.Clear();
		SortedList_0 = null;
	}

	public void method_8()
	{
		if (SortedList_1 == null)
		{
			return;
		}
		foreach (KeyValuePair<string, GClass969> item in SortedList_1)
		{
			item.Value.DestroyBuffers();
		}
		SortedList_1.Clear();
		SortedList_1 = null;
	}

	public void Dispose()
	{
		if (Gclass962_1 != null)
		{
			(Gclass962_1 as GClass963).Dispose();
		}
		if (Gclass962_2 != null)
		{
			(Gclass962_2 as GClass963).Dispose();
		}
		if (Gclass962_3 != null)
		{
			(Gclass962_3 as GClass963).Dispose();
		}
		if (Gclass962_4 != null)
		{
			(Gclass962_4 as GClass963).Dispose();
		}
		method_7();
		method_8();
		if (_subtreeCommandBuffer != null)
		{
			_subtreeCommandBuffer.Release();
			_subtreeCommandBuffer = null;
		}
		if (_ownDecalsCommandBuffer != null)
		{
			_ownDecalsCommandBuffer.Release();
			_ownDecalsCommandBuffer = null;
		}
	}
}
