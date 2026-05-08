using System.Text;
using EFT;
using EFT.Game.Spawning;

public class GClass698
{
	public EPlayerSide? ProfileDataSide;

	public ISpawnPoint SpawnPoint;

	public float Time;

	public bool IsValid;

	public GClass698(ISpawnPoint sp)
	{
		SpawnPoint = sp;
	}

	public string DebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder("Id:" + SpawnPoint.Id);
		stringBuilder.Append($"Time:{Time} ");
		stringBuilder.Append($"ProfileDataSide:{ProfileDataSide} ");
		stringBuilder.Append($"IsValid:{IsValid} ");
		return stringBuilder.ToString();
	}
}
