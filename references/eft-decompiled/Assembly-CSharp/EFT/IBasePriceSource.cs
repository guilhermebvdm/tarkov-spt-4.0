namespace EFT;

public interface IBasePriceSource
{
	double GetBasePrice(MongoID itemId);
}
