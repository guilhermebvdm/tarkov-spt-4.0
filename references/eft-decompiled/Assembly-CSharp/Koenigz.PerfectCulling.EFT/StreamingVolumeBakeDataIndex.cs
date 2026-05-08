using System.IO;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[PreferBinarySerialization]
public class StreamingVolumeBakeDataIndex : ScriptableObject
{
	public string volumeGuid;

	public string groupGuid;

	public Vector3 cellCount;

	public Vector3 cellSize;

	public Vector3 originalCellSize;

	public Quaternion orientation;

	public bool bakeCompleted;

	public int bakeHash;

	public int bakeDataVersion;

	public VisibilitySetIndex[] indices;

	public const string StreamingIndexFilePostfix = "_index";

	public const string StreamingRawDataFilePostfix = "_cull";

	public const string PlayerBuildCullingDataFolder = "Culling_Data";

	public bool IsValid
	{
		get
		{
			if (bakeCompleted && bakeHash != 0 && !string.IsNullOrEmpty(volumeGuid))
			{
				return !string.IsNullOrEmpty(groupGuid);
			}
			return false;
		}
	}

	public unsafe int IndexSizeBytes => sizeof(VisibilitySetIndex) * indices.Length;

	public static string GetStreamingDataFilePath(PerfectCullingCrossSceneVolume vol, PerfectCullingCrossSceneVolume.BakeDescriptor desc, bool isIndexFile = false)
	{
		string path = Path.Combine(Application.streamingAssetsPath, "Culling_Data");
		if (isIndexFile)
		{
			return Path.Combine(path, vol.VolumeGuid + "_" + desc.crossSceneGroup.ObjectGuidString + "_index.asset");
		}
		return Path.Combine(path, vol.VolumeGuid + "_" + desc.crossSceneGroup.ObjectGuidString + "_cull.bytes");
	}

	public static StreamingVolumeBakeDataIndex Convert(PerfectCullingCrossSceneVolume vol, PerfectCullingCrossSceneVolume.BakeDescriptor desc, PerfectCullingVolumeBakeData data)
	{
		return null;
	}
}
