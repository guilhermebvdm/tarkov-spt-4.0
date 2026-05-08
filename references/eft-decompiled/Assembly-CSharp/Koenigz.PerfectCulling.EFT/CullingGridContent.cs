using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class CullingGridContent : MonoBehaviour
{
	[SerializeField]
	private CullingGridCellContent[] _cellContent;

	public CullingGridCellContent[] CellContent => _cellContent;
}
