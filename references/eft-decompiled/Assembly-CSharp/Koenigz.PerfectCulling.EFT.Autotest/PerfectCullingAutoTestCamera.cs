using UnityEngine;

namespace Koenigz.PerfectCulling.EFT.Autotest;

public class PerfectCullingAutoTestCamera : MonoBehaviour
{
	[SerializeField]
	private int _renderTextureSize = 2048;

	public int RenderTextureSize => _renderTextureSize;
}
