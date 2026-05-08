using System;
using EFT;
using Newtonsoft.Json;

namespace ChatShared;

public class ChatRoomMember
{
	public class MemberInfo
	{
		public string Nickname;

		public EChatMemberSide Side;

		public int Level;

		public int PrestigeLevel;

		public EMemberCategory MemberCategory;

		public EMemberCategory SelectedMemberCategory;

		public DateTime GlobalMuteTime;
	}

	[JsonProperty("_id")]
	public string Id;

	[JsonProperty("aid")]
	public string Aid;

	public MemberInfo Info;
}
