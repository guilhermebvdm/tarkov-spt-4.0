using System;
using Comfort.Common;
using Diz.Binding;
using EFT;
using EFT.UI.Ragfair;

namespace ChatShared;

[Serializable]
public class UpdatableChatMember : IUpdatable<UpdatableChatMember>
{
	public class UpdatableChatMemberInfo
	{
		public string Nickname;

		public EChatMemberSide Side;

		public int Level;

		public int PrestigeLevel;

		public EMemberCategory MemberCategory;

		public EMemberCategory SelectedMemberCategory;

		public bool Ignored;

		public bool Banned;

		public bool IsTrader
		{
			get
			{
				if (Side != EChatMemberSide.Trader)
				{
					return MemberCategory == EMemberCategory.Trader;
				}
				return true;
			}
		}
	}

	[NonSerialized]
	public string Id_1;

	public string AccountId;

	[NonSerialized]
	public UpdatableChatMemberInfo Info_1 = new UpdatableChatMemberInfo();

	[NonSerialized]
	public BindableEvent OnIgnoreStatusChanged = new BindableEvent();

	[NonSerialized]
	public global::OnEventClass<bool> OnPlayerBanStatusChanged = new global::OnEventClass<bool>();

	public string Id => Id_1;

	public UpdatableChatMemberInfo Info => Info_1;

	public bool HasNickname => !string.IsNullOrEmpty(Info.Nickname);

	public string LocalizedNickname
	{
		get
		{
			if (Info.Side != EChatMemberSide.Trader && Info.MemberCategory != EMemberCategory.Trader)
			{
				return Singleton<SharedGameSettingsClass>.Instance.Game.Controller.GetCorrectedProfileNickname(Id, Info.Nickname);
			}
			return GClass2348.Localized(Info.Nickname);
		}
	}

	public static UpdatableChatMember FindOrCreate(string id, Func<string, UpdatableChatMember> constructor)
	{
		if (!SocialNetworkClass.AllMembers.TryGetValue(id, out var value))
		{
			value = constructor(id);
			SocialNetworkClass.AllMembers[id] = value;
		}
		return value;
	}

	public UpdatableChatMember(string id)
	{
		Id_1 = id;
		if (Singleton<BackendConfigSettingsClass>.Instance.TradersSettings.ContainsKey(Id))
		{
			Info_1.MemberCategory = EMemberCategory.Trader;
			Info_1.Side = EChatMemberSide.Trader;
		}
	}

	public void UpdateFromAnotherItem(UpdatableChatMember other)
	{
		if (other.Info_1 != null)
		{
			AccountId = other.AccountId;
			Info_1.Level = other.Info_1.Level;
			Info_1.PrestigeLevel = other.Info_1.PrestigeLevel;
			Info_1.Nickname = other.Info_1.Nickname;
			Info_1.Side = other.Info_1.Side;
			Info_1.MemberCategory = other.Info_1.MemberCategory;
			Info_1.SelectedMemberCategory = other.Info_1.SelectedMemberCategory;
			SetIgnoreStatus(other.Info_1.Ignored);
			SetBanStatus(other.Info_1.Banned);
		}
	}

	public void UpdateFromChatMember(ChatRoomMember member)
	{
		if (member.Aid != null)
		{
			AccountId = member.Aid;
		}
		Info_1.Level = (member?.Info?.Level).GetValueOrDefault();
		Info_1.PrestigeLevel = (member?.Info?.PrestigeLevel).GetValueOrDefault();
		UpdatableChatMemberInfo info_ = Info_1;
		object obj;
		if (member == null)
		{
			obj = null;
		}
		else
		{
			ChatRoomMember.MemberInfo info = member.Info;
			if (info == null)
			{
				obj = null;
			}
			else
			{
				obj = info.Nickname;
				if (obj != null)
				{
					goto IL_00bb;
				}
			}
		}
		obj = "Trader";
		goto IL_00bb;
		IL_00bb:
		info_.Nickname = (string)obj;
		Info_1.MemberCategory = member?.Info?.MemberCategory ?? EMemberCategory.Trader;
		Info_1.SelectedMemberCategory = member?.Info?.SelectedMemberCategory ?? EMemberCategory.Trader;
		Info_1.Side = member?.Info?.Side ?? EChatMemberSide.Trader;
		SetBanStatus(member?.Info?.GlobalMuteTime.ToLocalTime() > EFTDateTimeClass.Now);
	}

	public void UpdateFromMerchant(Offer.GClass3939 merchant)
	{
		AccountId = merchant.Aid.ToString();
		Info_1.Nickname = merchant.CorrectedNickname;
		Info_1.MemberCategory = merchant.MemberType;
		Info_1.SelectedMemberCategory = merchant.SelectedMemberType;
	}

	public void UpdateFromTrader(BackendConfigSettingsClass.TraderSettings traderSettings)
	{
		Info_1.Nickname = traderSettings.Nickname;
		Info_1.Side = EChatMemberSide.Trader;
		Info_1.MemberCategory = EMemberCategory.Trader;
	}

	public void SetIgnoreStatus(bool status)
	{
		if (status != Info_1.Ignored)
		{
			Info_1.Ignored = status;
			OnIgnoreStatusChanged?.Invoke();
		}
	}

	public void SetBanStatus(bool status)
	{
		if (status != Info_1.Banned)
		{
			Info_1.Banned = status;
			OnPlayerBanStatusChanged?.Invoke(status);
		}
	}

	public void SetNickname(string nickname)
	{
		Info_1.Nickname = nickname;
	}

	public void SetCategory(EMemberCategory category)
	{
		Info_1.MemberCategory = category;
	}

	public bool Compare(UpdatableChatMember other)
	{
		return Id_1 == other.Id_1;
	}
}
