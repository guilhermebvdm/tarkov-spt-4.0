using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

[RequireComponent(typeof(ScreenDistanceSwitcher))]
public class BakedLodContent : PerfectCullingCrossSceneContent
{
	[SerializeField]
	private ScreenDistanceSwitcher _screenDistanceSwitcher;

	public ScreenDistanceSwitcher Switcher => _screenDistanceSwitcher;

	public override void OnBakedLODVisbilityChanged(ScreenDistanceSwitcher bakeLOD, bool isVisible)
	{
	}
}
