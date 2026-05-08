using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Diz.DependencyManager;
using JetBrains.Annotations;
using Sirenix.Utilities;

public class DependencyGraphClass<T> where T : GInterface161
{
	public abstract class GClass1659
	{
		[NonSerialized]
		public TaskCompletionSource<object> TaskCompletionSource_0 = new TaskCompletionSource<object>();

		[NonSerialized]
		[CanBeNull]
		public IProgress<float> Iprogress_0;

		[NonSerialized]
		public bool Bool_0;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_1;

		[NonSerialized]
		[CompilerGenerated]
		public float Float_0;

		public Task LoadingJob => TaskCompletionSource_0.Task;

		public bool Finished
		{
			[CompilerGenerated]
			get
			{
				return Bool_1;
			}
			[CompilerGenerated]
			set
			{
				Bool_1 = value;
			}
		}

		public float Progress
		{
			[CompilerGenerated]
			get
			{
				return Float_0;
			}
			[CompilerGenerated]
			set
			{
				Float_0 = value;
			}
		}

		public bool Boolean_0
		{
			get
			{
				if (!Finished)
				{
					return !Bool_0;
				}
				return false;
			}
		}

		public GClass1659([CanBeNull] IProgress<float> progressReporter)
		{
			Iprogress_0 = progressReporter;
		}

		public void Release()
		{
			if (Bool_0)
			{
				GClass716.Instance.LogError("Trying to release Token that's already released: {0}", this);
			}
			else
			{
				Bool_0 = true;
				ReleaseInternal();
			}
		}

		public abstract void ReleaseInternal();

		public abstract void vmethod_0();
	}

	public class GClass1660 : GClass1659
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1052
		{
			public static readonly Class1052 class1052_0 = new Class1052();

			public static Action<GClass1661> action_0;

			public void method_0(GClass1661 x)
			{
				if (x.Boolean_0)
				{
					x.Release();
				}
			}
		}

		[NonSerialized]
		public CancellationToken CancellationToken_0;

		[NonSerialized]
		[CompilerGenerated]
		public Dictionary<string, GClass1661> Dictionary_0;

		[NonSerialized]
		public HashSet<GClass1662<T>> HashSet_0 = new HashSet<GClass1662<T>>();

		public Dictionary<string, GClass1661> TokenDict
		{
			[CompilerGenerated]
			get
			{
				return Dictionary_0;
			}
			[CompilerGenerated]
			set
			{
				Dictionary_0 = value;
			}
		}

		public GClass1660(Dictionary<string, GClass1661> tokens, [CanBeNull] IProgress<float> progressReporter, CancellationToken cancellationToken)
			: base(progressReporter)
		{
			CancellationToken_0 = cancellationToken;
			TokenDict = tokens;
			foreach (GClass1661 value in TokenDict.Values)
			{
				foreach (GClass1662<T> item in value.List_0)
				{
					HashSet_0.Add(item);
				}
			}
		}

		public override void ReleaseInternal()
		{
			TokenDict.Values.ForEach(delegate(GClass1661 x)
			{
				if (x.Boolean_0)
				{
					x.Release();
				}
			});
		}

		public override void vmethod_0()
		{
			if (Bool_0 || base.Finished)
			{
				return;
			}
			if (CancellationToken_0.IsCancellationRequested)
			{
				Release();
				TaskCompletionSource_0.SetException(new OperationCanceledException());
				return;
			}
			float num = 0f;
			bool flag = true;
			foreach (GClass1662<T> item in HashSet_0)
			{
				num += item.Data.Progress;
				ELoadState value = item.Data.LoadState.Value;
				if (value != ELoadState.Loaded && value != ELoadState.Failed)
				{
					flag = false;
				}
			}
			base.Progress = num / (float)HashSet_0.Count;
			Iprogress_0?.Report(base.Progress);
			if (flag)
			{
				base.Finished = true;
				TaskCompletionSource_0.SetResult(true);
			}
		}
	}

	public class GClass1661 : GClass1659
	{
		[Serializable]
		[CompilerGenerated]
		public class Class1053
		{
			public static readonly Class1053 class1053_0 = new Class1053();

			public static Func<GClass1662<T>, string> func_0;

			public string method_0(GClass1662<T> node)
			{
				return node.Data.Key;
			}
		}

		[NonSerialized]
		public List<GClass1662<T>> List_0;

		[NonSerialized]
		public CancellationToken CancellationToken_0;

		[NonSerialized]
		public global::DependencyGraphClass<T> Gclass1658_0;

		public GClass1661(global::DependencyGraphClass<T> graph, List<GClass1662<T>> nodes, [CanBeNull] IProgress<float> progressReporter, CancellationToken cancellationToken)
			: base(progressReporter)
		{
			Gclass1658_0 = graph;
			List_0 = new List<GClass1662<T>>(nodes);
			CancellationToken_0 = cancellationToken;
		}

		public override void ReleaseInternal()
		{
			Gclass1658_0.method_0(List_0);
		}

		public override void vmethod_0()
		{
			if (Bool_0 || base.Finished)
			{
				return;
			}
			CancellationToken cancellationToken_ = CancellationToken_0;
			if (cancellationToken_.IsCancellationRequested)
			{
				Release();
				TaskCompletionSource_0.SetException(new OperationCanceledException());
				return;
			}
			float num = 0f;
			bool flag = true;
			foreach (GClass1662<T> item in List_0)
			{
				num += item.Data.Progress;
				ELoadState value = item.Data.LoadState.Value;
				if (value != ELoadState.Loaded && value != ELoadState.Failed)
				{
					flag = false;
				}
			}
			base.Progress = num / (float)List_0.Count;
			Iprogress_0?.Report(base.Progress);
			if (flag)
			{
				base.Finished = true;
				TaskCompletionSource_0.SetResult(true);
			}
		}

		public override string ToString()
		{
			return "Token: " + string.Join(", ", List_0.Select((GClass1662<T> node) => node.Data.Key));
		}
	}

	[CompilerGenerated]
	public class Class1054
	{
		public Func<string, bool> shouldExclude;

		public global::DependencyGraphClass<T> gclass1658_0;
	}

	[CompilerGenerated]
	public class Class1055
	{
		public T loadable;

		public Class1054 class1054_0;

		public IEnumerable<GClass1662<T>> method_0(string x)
		{
			if (class1054_0.shouldExclude != null && class1054_0.shouldExclude(x))
			{
				return Enumerable.Empty<GClass1662<T>>();
			}
			if (!class1054_0.gclass1658_0.Nodes.ContainsKey(x))
			{
				GClass716.Instance.LogError("Bundle \"" + loadable.Key + "\" has dependency \"" + x + "\" that was not found in manifest");
				return Enumerable.Empty<GClass1662<T>>();
			}
			return Enumerable.Repeat(class1054_0.gclass1658_0.Nodes[x], 1);
		}
	}

	public readonly Dictionary<string, GClass1662<T>> Nodes;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public static int Int_0 = 55;

	[NonSerialized]
	public static int Int_1 = 0;

	[NonSerialized]
	public List<GClass1659> List_0 = new List<GClass1659>(1024);

	[NonSerialized]
	public List<GClass1659> List_1 = new List<GClass1659>(1024);

	[NonSerialized]
	public List<GClass1662<T>> List_2 = new List<GClass1662<T>>(1024);

	[NonSerialized]
	public List<GClass1662<T>> List_3 = new List<GClass1662<T>>(1024);

	[NonSerialized]
	public List<GClass1662<T>> List_4 = new List<GClass1662<T>>(1024);

	[NonSerialized]
	public List<GClass1662<T>> List_5 = new List<GClass1662<T>>(1024);

	[NonSerialized]
	public List<GClass1662<T>> List_6 = new List<GClass1662<T>>(1024);

	public DependencyGraphClass(T[] loadables, string defaultKey, [CanBeNull] Func<string, bool> shouldExclude)
	{
		global::DependencyGraphClass<T> gclass1658_0 = this;
		String_0 = defaultKey;
		Nodes = new Dictionary<string, GClass1662<T>>();
		T[] array = loadables;
		for (int i = 0; i < array.Length; i++)
		{
			T data = array[i];
			Nodes[data.Key] = new GClass1662<T>(data);
		}
		array = loadables;
		for (int i = 0; i < array.Length; i++)
		{
			T loadable = array[i];
			Nodes[loadable.Key].Dependencies = loadable.DependencyKeys.SelectMany(delegate(string x)
			{
				if (shouldExclude != null && shouldExclude(x))
				{
					return Enumerable.Empty<GClass1662<T>>();
				}
				if (!gclass1658_0.Nodes.ContainsKey(x))
				{
					GClass716.Instance.LogError("Bundle \"" + loadable.Key + "\" has dependency \"" + x + "\" that was not found in manifest");
					return Enumerable.Empty<GClass1662<T>>();
				}
				return Enumerable.Repeat(gclass1658_0.Nodes[x], 1);
			}).ToArray();
		}
	}

	public GClass1661 Retain(string key, [CanBeNull] IProgress<float> progress = null)
	{
		return Retain(new string[1] { key }, progress);
	}

	public static void smethod_0(GClass1662<T> node, List<GClass1662<T>> nodes)
	{
		if (!nodes.Contains(node))
		{
			nodes.Add(node);
			GClass1662<T>[] dependencies = node.Dependencies;
			for (int i = 0; i < dependencies.Length; i++)
			{
				smethod_0(dependencies[i], nodes);
			}
		}
	}

	public static async Task smethod_1(GClass1662<T> node, List<GClass1662<T>> nodes, CancellationToken ct)
	{
		if (ct.IsCancellationRequested)
		{
			return;
		}
		if (Int_1 >= Int_0)
		{
			Int_1 = 0;
			await Task.Yield();
		}
		Int_1++;
		if (!nodes.Contains(node))
		{
			nodes.Add(node);
			for (int i = 0; i < node.Dependencies.Length; i++)
			{
				await smethod_1(node.Dependencies[i], nodes, ct);
			}
		}
	}

	public GClass1661 Retain(IEnumerable<string> keys, [CanBeNull] IProgress<float> progress, CancellationToken ct = default(CancellationToken))
	{
		List<GClass1662<T>> list = new List<GClass1662<T>>();
		foreach (string key in keys)
		{
			smethod_0(GetNode(key), list);
		}
		List_2.AddRange(list);
		GClass1661 gClass = new GClass1661(this, list, progress, ct);
		List_1.Add(gClass);
		return gClass;
	}

	public GClass1660 RetainSeparate(IEnumerable<string> keys, [CanBeNull] IProgress<float> progress, CancellationToken ct = default(CancellationToken))
	{
		List<GClass1662<T>> list = new List<GClass1662<T>>();
		Dictionary<string, GClass1661> dictionary = new Dictionary<string, GClass1661>();
		foreach (string key in keys)
		{
			list.Clear();
			smethod_0(GetNode(key), list);
			GClass1661 value = new GClass1661(this, list, null, ct);
			dictionary.Add(key, value);
			List_2.AddRange(list);
		}
		GClass1660 gClass = new GClass1660(dictionary, progress, ct);
		List_1.AddRange(dictionary.Values);
		List_1.Add(gClass);
		return gClass;
	}

	public async Task<GClass1660> RetainSeparateAsync(IEnumerable<string> keys, [CanBeNull] IProgress<float> progress, CancellationToken ct = default(CancellationToken))
	{
		List<GClass1662<T>> list = new List<GClass1662<T>>();
		Dictionary<string, GClass1661> dictionary = new Dictionary<string, GClass1661>();
		float num = 0f;
		float num2 = keys.Count() * 2;
		Int_1 = 0;
		foreach (string key in keys)
		{
			if (!ct.IsCancellationRequested)
			{
				list.Clear();
				await smethod_1(GetNode(key), list, ct);
				GClass1661 value = new GClass1661(this, list, null, ct);
				dictionary.Add(key, value);
				List_2.AddRange(list);
				num += 1f;
				float value2 = num / num2;
				progress?.Report(value2);
				continue;
			}
			break;
		}
		Int_1 = 0;
		GClass1660 gClass = new GClass1660(dictionary, progress, ct);
		List_1.AddRange(dictionary.Values);
		List_1.Add(gClass);
		return gClass;
	}

	public void method_0(ICollection<GClass1662<T>> nodes)
	{
		List_3.AddRange(nodes);
	}

	public static void smethod_2(List<GClass1662<T>> input, List<GClass1662<T>> temp, List<GClass1662<T>> output, [CanBeNull] Func<string, bool> shouldExclude)
	{
		while (input.Count > 0)
		{
			temp.AddRange(input);
			input.Clear();
			foreach (GClass1662<T> item in temp)
			{
				if (shouldExclude == null || !shouldExclude(item.Data.Key))
				{
					output.Add(item);
					input.AddRange(item.Dependencies);
				}
			}
			temp.Clear();
		}
	}

	public static void smethod_3(List<GClass1662<T>> retains, List<GClass1662<T>> releases, List<GClass1662<T>> refCountChanges)
	{
		foreach (GClass1662<T> retain in retains)
		{
			retain.RefCount++;
		}
		foreach (GClass1662<T> release in releases)
		{
			release.RefCount--;
		}
		refCountChanges.AddRange(retains);
		refCountChanges.AddRange(releases);
	}

	public static void smethod_4(List<GClass1662<T>> refCountChanges, List<GClass1662<T>> pendingLoads, List<GClass1662<T>> pendingUnloads)
	{
		foreach (GClass1662<T> refCountChange in refCountChanges)
		{
			int refCount = refCountChange.RefCount;
			ELoadState value = refCountChange.Data.LoadState.Value;
			if (refCount > 0)
			{
				if ((value == ELoadState.Unloaded || value == ELoadState.Unloading || value == ELoadState.Failed) && !pendingLoads.Contains(refCountChange))
				{
					pendingLoads.Add(refCountChange);
				}
			}
			else if (refCount == 0)
			{
				pendingLoads.Remove(refCountChange);
				if (value != ELoadState.Unloaded && value != ELoadState.Unloading)
				{
					refCountChange.Data.Unload();
					if (refCountChange.Dependencies.Length != 0)
					{
						pendingUnloads.Add(refCountChange);
					}
				}
			}
			else if (refCountChange.RefCount < 0)
			{
				throw new Exception($"Ref less than zero. {refCountChange.Data.Key}: {refCountChange.RefCount}");
			}
		}
	}

	public static void smethod_5(List<GClass1662<T>> pendingLoads)
	{
		for (int num = pendingLoads.Count - 1; num >= 0; num--)
		{
			GClass1662<T> gClass = pendingLoads[num];
			if (gClass.Data.LoadState.Value == ELoadState.Unloaded)
			{
				bool flag = true;
				GClass1662<T>[] dependencies = gClass.Dependencies;
				for (int i = 0; i < dependencies.Length; i++)
				{
					ELoadState value = dependencies[i].Data.LoadState.Value;
					if (value == ELoadState.Unloaded || value == ELoadState.Loading)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					gClass.Data.Load();
					pendingLoads.RemoveAt(num);
					dependencies = gClass.Dependencies;
					for (int i = 0; i < dependencies.Length; i++)
					{
						dependencies[i].RefCount++;
					}
				}
			}
		}
	}

	public static void smethod_6(List<GClass1662<T>> pendingUnloads, List<GClass1662<T>> refCountChanges)
	{
		for (int num = pendingUnloads.Count - 1; num >= 0; num--)
		{
			GClass1662<T> gClass = pendingUnloads[num];
			if (gClass.Data.LoadState.Value == ELoadState.Unloaded)
			{
				GClass1662<T>[] dependencies = gClass.Dependencies;
				foreach (GClass1662<T> gClass2 in dependencies)
				{
					gClass2.RefCount--;
					refCountChanges.Add(gClass2);
				}
				pendingUnloads.RemoveAt(num);
			}
		}
	}

	public void Update()
	{
		foreach (GClass1659 item in List_1)
		{
			List_0.Add(item);
		}
		List_1.Clear();
		if (List_0.Count > 0)
		{
			foreach (GClass1659 item2 in List_0)
			{
				item2.vmethod_0();
			}
			for (int num = List_0.Count - 1; num >= 0; num--)
			{
				if (!List_0[num].Boolean_0)
				{
					List_0.RemoveAt(num);
				}
			}
		}
		if (List_4.Count > 0 || List_2.Count > 0 || List_3.Count > 0)
		{
			smethod_3(List_2, List_3, List_4);
			smethod_4(List_4, List_5, List_6);
			List_2.Clear();
			List_3.Clear();
			List_4.Clear();
		}
		smethod_5(List_5);
		smethod_6(List_6, List_4);
	}

	public GClass1662<T> GetNode([NotNull] string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			GClass716.Instance.LogError("Bundle key is null or empty");
			return GetDefaultNode();
		}
		if (Nodes.TryGetValue(key, out var value))
		{
			return value;
		}
		GClass716.Instance.LogError("Error: Bundle {0} not found in manifest", key);
		return GetDefaultNode();
	}

	public GClass1662<T> GetDefaultNode()
	{
		return Nodes[String_0];
	}
}
