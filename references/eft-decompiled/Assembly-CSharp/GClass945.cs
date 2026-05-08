using Comfort.Common;
using EFT;

public abstract class GClass945
{
	public static bool IsLocalStreamer(this GInterface214 profileData)
	{
		return Singleton<SharedGameSettingsClass>.Instance.Game.Controller.IsLocalStreamer(profileData.ProfileId);
	}

	public static string GetCorrectedNickname(this GInterface214 profileData)
	{
		if (profileData.Side == EPlayerSide.Savage)
		{
			return GClass953.Transliterate(profileData.Nickname);
		}
		return Singleton<SharedGameSettingsClass>.Instance.Game.Controller.GetCorrectedProfileNickname(profileData.ProfileId, profileData.Nickname);
	}
}
