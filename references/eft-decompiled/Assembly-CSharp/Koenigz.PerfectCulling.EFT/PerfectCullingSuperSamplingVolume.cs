using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingSuperSamplingVolume : MonoBehaviour, GClass879.GInterface33
{
	public static readonly Color GIZMO_VOLUME_COLOR = new Color(0f, 0f, 1f, 0.25f);

	[SerializeField]
	private GuidReference _crossGroupReference;

	[SerializeField]
	private int _cellSize = 1;

	[SerializeField]
	private Vector3 _volumeSize = new Vector3(5f, 5f, 5f);

	public GuidReference CrossGroupReference => _crossGroupReference;

	public PerfectCullingCrossSceneGroup CrossGroup => _crossGroupReference?.gameObject?.GetComponent<PerfectCullingCrossSceneGroup>();

	public int CellSize
	{
		get
		{
			return _cellSize;
		}
		set
		{
			_cellSize = value;
		}
	}

	public Vector3 VolumeSize => _volumeSize;

	public Bounds VolumeBakeBounds
	{
		get
		{
			return new Bounds(base.transform.position, _volumeSize);
		}
		set
		{
			base.transform.position = value.center;
			_volumeSize = new Vector3(Mathf.Max(1f, value.size.x), Mathf.Max(1f, value.size.y), Mathf.Max(1f, value.size.z));
		}
	}

	public Vector3 BakeCellSize => Vector3.one * PerfectCullingCrossSceneVolume.BAKE_CELL_SIZES[_cellSize];

	public int CellCount => GClass1225.CalculateNumberOfCells(_volumeSize, BakeCellSize);

	public Vector3 HandleSized
	{
		get
		{
			return VolumeBakeBounds.size;
		}
		set
		{
			VolumeBakeBounds = new Bounds(base.transform.position, value);
		}
	}

	public static PerfectCullingSuperSamplingVolume Create(PerfectCullingCrossSceneVolume volume, PerfectCullingCrossSceneGroup group, int cellSizeType)
	{
		PerfectCullingSuperSamplingVolume perfectCullingSuperSamplingVolume = new GameObject().AddComponent<PerfectCullingSuperSamplingVolume>();
		perfectCullingSuperSamplingVolume._crossGroupReference = new GuidReference(group.GetComponent<GuidComponent>());
		perfectCullingSuperSamplingVolume._cellSize = cellSizeType;
		perfectCullingSuperSamplingVolume.VolumeBakeBounds = volume.volumeBakeBounds;
		return perfectCullingSuperSamplingVolume;
	}

	public List<Vector3> GetSamplingPositions(Space space = Space.Self)
	{
		List<Vector3> list = new List<Vector3>(CellCount);
		for (int i = 0; i < CellCount; i++)
		{
			list.Add(method_0(i, BakeCellSize, space));
		}
		return list;
	}

	public Vector3 method_0(int index, Vector3 cellSize, Space space = Space.Self)
	{
		Vector3 gridSize = new Vector3(_volumeSize.x / cellSize.x, _volumeSize.y / cellSize.y, _volumeSize.z / cellSize.z);
		return method_1(index, gridSize, cellSize, space);
	}

	public Vector3 method_1(int index, Vector3 gridSize, Vector3 cellSize, Space space = Space.Self)
	{
		Vector3 vector = gridSize * 0.5f;
		GClass1225.UnflattenToXYZ(index, out var x, out var y, out var z, gridSize);
		Vector3 vector2 = cellSize / 2f + new Vector3((float)x * cellSize.x, (float)y * cellSize.y, (float)z * cellSize.z) - new Vector3(vector.x * cellSize.x, vector.y * cellSize.y, vector.z * cellSize.z);
		if (space == Space.World)
		{
			return base.transform.position + base.transform.rotation * vector2;
		}
		return vector2;
	}

	public void FixVolumeSize()
	{
		Bounds volumeBakeBounds = VolumeBakeBounds;
		volumeBakeBounds.size = GClass1228.Snap(volumeBakeBounds.size, PerfectCullingCrossSceneVolume.BAKE_CELL_SIZES[_cellSize]);
		VolumeBakeBounds = volumeBakeBounds;
	}
}
