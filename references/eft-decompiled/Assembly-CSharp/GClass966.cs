using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass966 : GClass965<StaticDeferredDecal, GClass963>
{
	public class Class604
	{
		public GClass966 tree;
	}

	public class Class605
	{
		public GClass2380 frustum = new GClass2380();

		public int maxDepth;

		public List<CommandBuffer> threadResults = new List<CommandBuffer>();
	}

	[NonSerialized]
	public Thread Thread_0;

	[NonSerialized]
	public static CancellationTokenSource CancellationTokenSource_0;

	[NonSerialized]
	public static CancellationToken CancellationToken_0;

	public static int ThreadSleepTime = 100;

	public static bool ThreadSkipJob = false;

	[NonSerialized]
	public object Object_0 = new object();

	[NonSerialized]
	public object Object_1 = new object();

	[NonSerialized]
	public static Class605 Class605_0 = new Class605();

	public void Dispose()
	{
		if (CancellationTokenSource_0 != null)
		{
			CancellationTokenSource_0.Cancel();
		}
		if (Thread_0 != null)
		{
			Thread_0.Join();
		}
		Thread_0 = null;
		CancellationTokenSource_0.Dispose();
		CancellationTokenSource_0 = null;
		if (Gclass962_0 != null)
		{
			(Gclass962_0 as GClass963).Dispose();
		}
	}

	public GClass966()
	{
		CancellationTokenSource_0 = new CancellationTokenSource();
		CancellationToken_0 = CancellationTokenSource_0.Token;
	}

	public override GClass962<StaticDeferredDecal> CreateQuadrant(GClass962<StaticDeferredDecal> parent, Rect bounds)
	{
		return new GClass963(parent as GClass963, bounds);
	}

	public void Insert(StaticDeferredDecal decal, Mesh mesh)
	{
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;
		GClass963.ComputeDecalMinMaxPoints(decal, ref minPoint, ref maxPoint, mesh);
		Bounds bounds = new Bounds((minPoint + maxPoint) * 0.5f, maxPoint - minPoint);
		float x = bounds.min.x;
		float z = bounds.min.z;
		float x2 = bounds.size.x;
		float z2 = bounds.size.z;
		Insert(decal, new Rect(x, z, x2, z2));
	}

	public Dictionary<Texture2D, GClass967> ConstructCommandBuffersAndTexturesTrees(Mesh mesh, GClass963.GDelegate34 preDrawCallback)
	{
		List<GClass963.GStruct65> item = (Gclass962_0 as GClass963).ConstructSubtreeCommandBuffers(mesh, preDrawCallback).Item2;
		Dictionary<Texture2D, List<GClass971>> dictionary = new Dictionary<Texture2D, List<GClass971>>();
		foreach (GClass963.GStruct65 item2 in item)
		{
			if (!dictionary.ContainsKey(item2.Texture))
			{
				dictionary.Add(item2.Texture, new List<GClass971>());
			}
			dictionary[item2.Texture].Add(new GClass971
			{
				maxSize = item2.MaxSize,
				pos = item2.Position,
				uvStartEnd = item2.uvStartEnd
			});
		}
		Dictionary<Texture2D, GClass967> dictionary2 = new Dictionary<Texture2D, GClass967>();
		foreach (KeyValuePair<Texture2D, List<GClass971>> item3 in dictionary)
		{
			GClass967 gClass = new GClass967();
			gClass.SizeLimit = base.SizeLimit;
			List<GClass971> infosList = item3.Value;
			gClass.Insert(ref infosList, item3.Key);
			dictionary2.Add(item3.Key, gClass);
		}
		return dictionary2;
	}

	public CommandBuffer GetRootCommandBuffer()
	{
		if (Gclass962_0 != null)
		{
			return (Gclass962_0 as GClass963)._subtreeCommandBuffer;
		}
		return null;
	}

	public void GetCommandBuffers(GClass2380 frustum, ref List<CommandBuffer> commandBuffers, int maxDepth)
	{
		commandBuffers.Clear();
		if (Gclass962_0 != null)
		{
			(Gclass962_0 as GClass963).GetCommandBuffers(frustum, ref commandBuffers, maxDepth);
		}
	}

	public void RecordCommandBuffer(GClass2380 frustum, CommandBuffer commandBuffer, int maxDepth, GClass963.GDelegate34 preDrawCallback, Mesh mesh)
	{
		commandBuffer.Clear();
		preDrawCallback?.Invoke(commandBuffer);
		if (Gclass962_0 != null)
		{
			(Gclass962_0 as GClass963).RecordCommandBuffer(frustum, commandBuffer, maxDepth, mesh);
		}
	}

	public void SetThreadParams(GClass2380 frustum, int maxDepth)
	{
		if (Monitor.TryEnter(Object_0))
		{
			frustum.CopyTo(Class605_0.frustum);
			Class605_0.maxDepth = maxDepth;
			if (Thread_0 == null)
			{
				Class604 @class = new Class604();
				@class.tree = this;
				Thread_0 = new Thread(smethod_0);
				Thread_0.Start(@class);
			}
			Monitor.Exit(Object_0);
		}
	}

	public bool GetThreadResults(ref List<CommandBuffer> commandBuffers)
	{
		bool result = false;
		if (Monitor.TryEnter(Object_1))
		{
			result = true;
			commandBuffers.Clear();
			commandBuffers.AddRange(Class605_0.threadResults);
			Monitor.Exit(Object_1);
		}
		return result;
	}

	public static void smethod_0(object p)
	{
		GClass2380 gClass = new GClass2380();
		int maxDepth = 0;
		GClass966 tree = (p as Class604).tree;
		List<CommandBuffer> commandBuffers = new List<CommandBuffer>();
		while (!CancellationToken_0.IsCancellationRequested)
		{
			if (!ThreadSkipJob)
			{
				bool flag = false;
				if (Monitor.TryEnter(tree.Object_0))
				{
					Class605_0.frustum.CopyTo(gClass);
					maxDepth = Class605_0.maxDepth;
					flag = true;
					Monitor.Exit(tree.Object_0);
				}
				if (flag)
				{
					tree.GetCommandBuffers(gClass, ref commandBuffers, maxDepth);
					Monitor.Enter(tree.Object_1);
					Class605_0.threadResults.Clear();
					Class605_0.threadResults.AddRange(commandBuffers);
					Monitor.Exit(tree.Object_1);
				}
			}
			Thread.Sleep(ThreadSleepTime);
		}
	}
}
