using UnityEngine;
using UnityEngine.UI;

public class GridCellResizer : MonoBehaviour
{
	private GridLayoutGroup gridLayoutGroup_0;

	public bool stretchWidth;

	public bool stretchHeight;

	public void OnEnable()
	{
		gridLayoutGroup_0 = GetComponent<GridLayoutGroup>();
	}

	public void Update()
	{
		if (gridLayoutGroup_0 != null && (stretchWidth || stretchHeight))
		{
			gridLayoutGroup_0.cellSize = new Vector2(stretchWidth ? ((RectTransform)base.transform).rect.width : gridLayoutGroup_0.cellSize.x, stretchHeight ? ((RectTransform)base.transform).rect.height : gridLayoutGroup_0.cellSize.y);
		}
	}
}
