public abstract class GClass2377
{
	public const string HIDEOUT_FIRST_PERSON_INFO = "HideoutFirstPersonInfo";

	public const string QUEST_LIST_SHOW_LOCKED_KEY = "quests_list_ShowLocked";

	public const string CHAT_SOUND_STATE = "ChatSoundState";

	public const string BARTER_AUTO_FILL_KEY = "AutoFillRequirements";

	public const string OFFER_SAVED_POSITION_KEY = "AddOfferWindowPosition";

	public const string VOIP_FIRST_MESSAGE_SHOWN = "voip_first_message_shown";

	public const string PVE_FIRST_TIME = "pve_first_time";

	public const string TUE_LOGIN_FIRST_TIME = "tue_login_first_time";

	public static void RegisterKeysToReset()
	{
		PlayerPrefHelperClass.RegisterKeyToReset("voip_first_message_shown");
		PlayerPrefHelperClass.RegisterKeyToReset("pve_first_time");
	}
}
