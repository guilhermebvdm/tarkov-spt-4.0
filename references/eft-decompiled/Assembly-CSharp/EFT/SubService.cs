using System;

namespace EFT;

[Serializable]
public class SubService
{
	public string Id;

	public int Price;

	public SubService(string id, int price)
	{
		Id = id;
		Price = price;
	}
}
