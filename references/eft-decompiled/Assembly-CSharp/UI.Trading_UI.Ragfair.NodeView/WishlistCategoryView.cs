using EFT.HandBook;
using UnityEngine;

namespace UI.Trading_UI.Ragfair.NodeView;

public class WishlistCategoryView : HandbookCategoryView
{
	[SerializeField]
	private Color _defaultBackgroundColor;

	[SerializeField]
	private Color _hoverBackgroundColor;

	public override Color DefaultBackgroundColor => _defaultBackgroundColor;

	public override Color HoverBackgroundColor => _hoverBackgroundColor;
}
