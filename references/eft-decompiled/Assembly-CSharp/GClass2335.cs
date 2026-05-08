using System;
using EFT.InventoryLogic;
using Newtonsoft.Json;

public class GClass2335
{
	public string _tpl;

	public double count;

	public int level;

	public EDogtagExchangeSide side;

	public bool onlyFunctional;

	[JsonIgnore]
	public int IntCount => (int)Math.Ceiling(count);
}
