using EFT.UI.DragAndDrop;

public class ArenaEftItemTransferStashGridView : GridView
{
	public override bool CanAccept(ItemContextClass itemContext, ItemContextAbstractClass targetItemContext, out GStruct153 operation)
	{
		if (targetItemContext is GClass3461 { IsSelected: not false })
		{
			operation = default(GStruct153);
			return false;
		}
		return base.CanAccept(itemContext, targetItemContext, out operation);
	}
}
