using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public abstract class PerfectCullingCrossSceneContent : MonoBehaviour, GInterface116
{
	public const int INVALID_CONTENT_GROUP_ID = int.MinValue;

	[HideInInspector]
	[SerializeField]
	protected int _contentGroupId = int.MinValue;

	[HideInInspector]
	[SerializeField]
	protected int _contentHash;

	[SerializeField]
	protected PerfectCullingBakeGroup[] _bakeGroups;

	[SerializeField]
	protected Vector2Int _indexSpan;

	[CompilerGenerated]
	private PerfectCullingCrossSceneGroup perfectCullingCrossSceneGroup_0;

	public int ContentGroupId => _contentGroupId;

	public int ContentHash => _contentHash;

	public PerfectCullingBakeGroup[] BakeGroups => _bakeGroups;

	public Vector2Int IndexSpan => _indexSpan;

	public PerfectCullingCrossSceneGroup RuntimeCullingGroup
	{
		[CompilerGenerated]
		get
		{
			return perfectCullingCrossSceneGroup_0;
		}
		[CompilerGenerated]
		set
		{
			perfectCullingCrossSceneGroup_0 = value;
		}
	}

	public virtual void OnBakedLODVisbilityChanged(ScreenDistanceSwitcher bakeLOD, bool isVisible)
	{
		if (ScreenDistanceSwitcher.Boolean_0 && !(RuntimeCullingGroup == null))
		{
			for (int i = 0; i < _bakeGroups.Length; i++)
			{
				_bakeGroups[i].RuntimeSetForceRenderingOff(isVisible);
			}
		}
	}

	public PerfectCullingCrossSceneContent()
	{
	}
}
