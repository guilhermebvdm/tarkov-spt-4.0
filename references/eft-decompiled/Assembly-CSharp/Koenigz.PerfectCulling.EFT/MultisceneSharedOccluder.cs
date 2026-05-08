using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class MultisceneSharedOccluder : MonoBehaviour
{
	[Tooltip("Culling type.\n\nSharedOccluder\n-> not culled\n-> occludes in other scenes\n\nSharedOccludeeOccluder\n-> culled in volume\n-> occludes in other scenes\n\nOccludee\n-> culled in volume\n-> doesnt occlude in other scenes")]
	[SerializeField]
	private EOccludeMode _occludeMode;

	[Tooltip("Which LOD to select when generating multiscene shared occluder")]
	[SerializeField]
	private ESharedOccluderLODMode _LODMode;

	[Tooltip("Shadow culling mode on runtime")]
	[SerializeField]
	private EShadowMode _shadowMode;

	[Tooltip("How to treat transprent materials when generating shared occluder")]
	[SerializeField]
	private ETransparencyMode _transparencyMode;

	public EOccludeMode OccludeMode => _occludeMode;

	public ESharedOccluderLODMode SharedOccluderLODMode => _LODMode;

	public EShadowMode ShadowMode => _shadowMode;

	public ETransparencyMode TransparencyMode => _transparencyMode;
}
