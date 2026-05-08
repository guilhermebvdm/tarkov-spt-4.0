using Comfort.Common;
using EFT.InventoryLogic;

public interface GInterface205<T> : GInterface204, IHandsController where T : Item
{
	new T Item { get; }

	void SetOnUsedCallback(Callback<GInterface205<T>> callback);
}
