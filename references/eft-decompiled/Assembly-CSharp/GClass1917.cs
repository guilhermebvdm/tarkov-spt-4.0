using System.Collections.Generic;
using EFT;

[GAttribute29]
public class GClass1917
{
	public byte Malfunction;

	public float LastShotOverheat;

	public float LastShotTime;

	public bool SlideOnOverheatReached;

	public List<MongoID> PlayersWhoKnowAboutMalfunction;

	public List<MongoID> PlayersWhoKnowMalfType;

	public Dictionary<MongoID, byte> PlayersReducedMalfChances;

	public MongoID? AmmoToFireTemplateId;

	public MongoID? AmmoWillBeLoadedToChamberTemplateId;

	public MongoID? AmmoMalfunctionedTemplateId;
}
