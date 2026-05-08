using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingAdaptiveGrid : MonoBehaviour
{
	[CompilerGenerated]
	public class Class851
	{
		public PerfectCullingAdaptiveGrid perfectCullingAdaptiveGrid_0;

		public string filePath;

		public void method_0()
		{
			perfectCullingAdaptiveGrid_0.gclass1250_0.LoadSync(filePath);
		}
	}

	[CompilerGenerated]
	public class Class852
	{
		public PerfectCullingAdaptiveGrid perfectCullingAdaptiveGrid_0;

		public CancellationToken token;

		public IProgress<float> progress;

		public void method_0()
		{
			perfectCullingAdaptiveGrid_0.gclass1233_0 = perfectCullingAdaptiveGrid_0.method_5(token, progress);
		}
	}

	[CompilerGenerated]
	private static PerfectCullingAdaptiveGrid perfectCullingAdaptiveGrid_0;

	[SerializeField]
	private PerfectCullingAdaptiveGridData _gridData;

	[SerializeField]
	private string _gridHash;

	private GClass1233 gclass1233_0;

	private List<PerfectCullingCrossSceneGroup> list_0;

	private GClass1250 gclass1250_0;

	private List<OrientedBounds> list_1;

	private List<OrientedPoint> list_2;

	private GClass1250.GClass1251 gclass1251_0;

	public static PerfectCullingAdaptiveGrid Instance
	{
		[CompilerGenerated]
		get
		{
			return perfectCullingAdaptiveGrid_0;
		}
		[CompilerGenerated]
		set
		{
			perfectCullingAdaptiveGrid_0 = value;
		}
	}

	public PerfectCullingAdaptiveGridData GridData => _gridData;

	public string GridHash => _gridHash;

	public GClass1233 RuntimeBVH => gclass1233_0;

	public List<PerfectCullingCrossSceneGroup> RuntimeGroupMapping => list_0;

	public GClass1250 PackedData => gclass1250_0;

	public List<CullingCellData> AdaptiveCells => _gridData.CellData;

	public GClass1250.GClass1251 IndexDecompressor => gclass1251_0;

	public int GetGroupIndex(GuidReference reference)
	{
		if (_gridData != null)
		{
			return GridData.GetGroupIndex(reference);
		}
		if (gclass1250_0 != null)
		{
			return gclass1250_0.GetGroupIndex(reference);
		}
		return -1;
	}

	public void Awake()
	{
		Instance = this;
	}

	public async Task method_0(CancellationToken token, IProgress<float> progress = null)
	{
		GClass712.Debug("Initializing culling grid");
		await method_1(token, progress);
		if (!token.IsCancellationRequested)
		{
			method_3();
			await method_4(token, progress);
		}
	}

	public async Task method_1(CancellationToken token, IProgress<float> progress = null)
	{
		try
		{
			string filePath = GClass1250.GetPackedFilePathForGrid(this);
			GClass712.Debug("Loading packed culling file " + filePath);
			gclass1250_0 = new GClass1250();
			await Task.Factory.StartNew(delegate
			{
				gclass1250_0.LoadSync(filePath);
			}, token);
			if (!gclass1250_0.IsValid)
			{
				GClass712.Error("Packed culling file is not loaded " + filePath);
				return;
			}
			gclass1250_0.PostLoadInitialize();
			gclass1251_0 = gclass1250_0.AllocateDecompressor();
		}
		catch (Exception ex)
		{
			throw new Exception("Packed culling data file failed to load. " + ex.Message + " | " + ex.StackTrace);
		}
	}

	public void method_2()
	{
		if (Instance == this)
		{
			if (gclass1251_0 != null)
			{
				gclass1251_0.Dispose();
				gclass1251_0 = null;
			}
			if (gclass1250_0 != null)
			{
				gclass1250_0.Dispose();
				gclass1250_0 = null;
			}
			Instance = null;
			gclass1233_0 = null;
			_gridData = null;
			if (list_0 != null)
			{
				list_0.Clear();
				list_0 = null;
			}
		}
	}

	public void OnDestroy()
	{
		method_2();
	}

	public void method_3()
	{
		list_0 = new List<PerfectCullingCrossSceneGroup>();
		List<GuidReference> list = null;
		list = gclass1250_0.GroupMapping;
		if (list == null)
		{
			return;
		}
		foreach (GuidReference item in list)
		{
			if (item.gameObject == null)
			{
				GClass712.Throw("Culling group is not loaded " + item.ObjectGuidString);
			}
			PerfectCullingCrossSceneGroup component = item.gameObject.GetComponent<PerfectCullingCrossSceneGroup>();
			if (component == null)
			{
				GClass712.Throw("Culling group failed to init " + item.gameObject.name);
			}
			list_0.Add(component);
		}
	}

	public async Task method_4(CancellationToken token, IProgress<float> progress = null)
	{
		await Task.Factory.StartNew(delegate
		{
			gclass1233_0 = method_5(token, progress);
		});
	}

	public GClass1233 InitializeSpatialTreeFromGridData(CancellationToken token, IProgress<float> progress = null)
	{
		GClass1233 gClass = new GClass1233();
		if (_gridData != null && _gridData.CellData != null && _gridData.CellData.Count > 0)
		{
			gClass.Initialize(_gridData.CellData.Count);
			int num = _gridData.CellData.Count / 10;
			int num2 = 0;
			foreach (CullingCellData cellDatum in _gridData.CellData)
			{
				gClass.Add(cellDatum);
				cellDatum.RuntimeCellId = num2;
				num2++;
				if (num2 % num == 0)
				{
					progress?.Report((float)num2 / (float)_gridData.CellData.Count);
					if (token.IsCancellationRequested)
					{
						break;
					}
				}
			}
		}
		return gClass;
	}

	public GClass1233 method_5(CancellationToken token, IProgress<float> progress = null)
	{
		GClass1233 gClass = new GClass1233();
		if (gclass1250_0 != null)
		{
			if (!gclass1250_0.IsValid)
			{
				GClass712.Error("Packed culling file not loaded on runtime");
				return gClass;
			}
			int numCells = gclass1250_0.NumCells;
			if (numCells == 0)
			{
				GClass712.Error("Packed culling data contains no cells.");
				return gClass;
			}
			gClass.Initialize(numCells);
			int num = numCells / 10;
			for (int i = 0; i < numCells; i++)
			{
				GClass1250.GStruct112 cellBounds = gclass1250_0.GetCellBounds(i);
				CullingCellData obj = new CullingCellData
				{
					CellBounds = new OrientedBounds
					{
						bounds = new Bounds(cellBounds.center, cellBounds.size),
						rotation = cellBounds.rotation
					},
					RuntimeCellId = i
				};
				gClass.Add(obj);
				if (i % num == 0)
				{
					progress?.Report((float)i / (float)numCells);
					if (token.IsCancellationRequested)
					{
						break;
					}
				}
			}
			GClass712.Debug($"BVH init {numCells}");
		}
		return gClass;
	}
}
