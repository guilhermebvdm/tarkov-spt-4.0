using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using ChatShared;
using Diz.Binding;
using EFT.Communications;
using EFT.InventoryLogic;
using UnityEngine;

public class ChatMessageClass : IUpdatable<ChatMessageClass>
{
	public class GClass1067
	{
		public EMessageParamsAction Action;

		public UpdatableChatMember Member;

		public UpdatableChatMember[] Users;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class767
	{
		public static readonly Class767 class767_0 = new Class767();

		public static Func<Item, bool> func_0;

		public static Func<Item, bool> func_1;

		public static Func<ProfileChangeEvent, bool> func_2;

		public bool method_0(Item item)
		{
			return !item.QuestItem;
		}

		public bool method_1(Item item)
		{
			return item.QuestItem;
		}

		public bool method_2(ProfileChangeEvent @event)
		{
			return !@event.Redeemed;
		}

		public string method_3(GClass1060 data)
		{
			return data.date;
		}

		public string method_4(GClass1060 data)
		{
			return data.time;
		}

		public string method_5(GClass1060 data)
		{
			return GClass2348.Localized(data.location);
		}

		public string method_6(GClass1060 data)
		{
			return data.buyerNickname;
		}

		public string method_7(GClass1060 data)
		{
			return GClass2348.Localized(data.soldItem + " Name");
		}

		public string method_8(GClass1060 data)
		{
			return "(x" + data.itemCount + ")";
		}
	}

	[CompilerGenerated]
	public class Class768
	{
		public GClass1060 data;

		public string method_0(Match match)
		{
			string value = match.Groups[1].Value;
			if (!IreadOnlyDictionary_0.TryGetValue(value, out var value2))
			{
				return match.Value;
			}
			return value2(data);
		}
	}

	public const string RICH_TEXT_TAG = "<.*?>";

	[NonSerialized]
	public const string String_0 = "#4db1d0";

	[NonSerialized]
	public static Regex Regex_0 = new Regex("\\{([^{}]+)\\}", RegexOptions.Compiled);

	[NonSerialized]
	public static IReadOnlyDictionary<string, Func<GClass1060, string>> IreadOnlyDictionary_0 = new Dictionary<string, Func<GClass1060, string>>
	{
		{
			"date",
			(GClass1060 data) => data.date
		},
		{
			"time",
			(GClass1060 data) => data.time
		},
		{
			"location",
			(GClass1060 data) => GClass2348.Localized(data.location)
		},
		{
			"buyerNickname",
			(GClass1060 data) => data.buyerNickname
		},
		{
			"soldItem",
			(GClass1060 data) => GClass2348.Localized(data.soldItem + " Name")
		},
		{
			"itemCount",
			(GClass1060 data) => "(x" + data.itemCount + ")"
		}
	};

	public string _id;

	public string templateId;

	public GClass1060 systemData;

	public EMessageType Type;

	public string Text;

	public DateTime LocalDateTime;

	public DateTime UtcDateTime;

	public UpdatableChatMember Member;

	public ChatMessageClass ReplyTo;

	public GClass1067 Params;

	public long MaxStorageTime;

	public GClass1065 Items;

	public ProfileChangeEvent[] ProfileChangeEvents;

	public bool HasRewards;

	[NonSerialized]
	public BindableEvent UpdateRewardStatus = new BindableEvent();

	public bool AnyItems
	{
		get
		{
			if (Items != null)
			{
				return Items.Stash.Grid.Items.Any();
			}
			return false;
		}
	}

	public bool AnyNonQuestItems
	{
		get
		{
			if (Items != null)
			{
				return Items.Stash.Grid.Items.Any((Item item) => !item.QuestItem);
			}
			return false;
		}
	}

	public bool AnyQuestItems
	{
		get
		{
			if (Items != null)
			{
				return Items.Stash.Grid.Items.Any((Item item) => item.QuestItem);
			}
			return false;
		}
	}

	public DateTime ExpirationTime => UtcDateTime.AddSeconds((MaxStorageTime > 0L) ? MaxStorageTime : 2147483647L);

	public bool HasProfileEvents => ProfileChangeEvents.Any((ProfileChangeEvent @event) => !@event.Redeemed);

	public bool DisplayRewardStatus
	{
		get
		{
			if (HasRewards && (MaxStorageTime <= 0L || UtcDateTime.AddSeconds(MaxStorageTime) > EFTDateTimeClass.UtcNow))
			{
				if (!AnyItems)
				{
					return HasProfileEvents;
				}
				return true;
			}
			return false;
		}
	}

	public string SystemText
	{
		get
		{
			if (Params?.Member?.Info == null)
			{
				Debug.LogError("> > Something went wrong getting system text!");
				return string.Empty;
			}
			return string.Format(GClass2348.Localized(Params.Action.ToString()), Params.Member.LocalizedNickname);
		}
	}

	public bool ParseLocalizationForMessage
	{
		get
		{
			if (Type != EMessageType.InsuranceReturn && Type != EMessageType.BtrItemsDelivery && Type != EMessageType.NpcTraderMessage && Type != EMessageType.SystemMessage && Type != EMessageType.FleamarketMessage && Type != EMessageType.QuestFail && Type != EMessageType.QuestStart && Type != EMessageType.QuestSuccess && Type != EMessageType.MessageWithItems && Type != EMessageType.InitialSupport)
			{
				return Type == EMessageType.AuctionMessage;
			}
			return true;
		}
	}

	public string ParsedText()
	{
		return ParsedText(Text, EViewRule.MessageText);
	}

	public string ParsedText(string startingValue, EViewRule viewRule)
	{
		string text = startingValue;
		GClass1060 gClass = systemData;
		if (Type == EMessageType.SystemMessage)
		{
			if (Params == null)
			{
				string text2 = Text;
				text = text2;
			}
			else
			{
				string text2 = ((Params.Member.Info == null) ? GClass2348.Localized("Player") : Params.Member.LocalizedNickname);
				text = string.Format(GClass2348.Localized(GClass867.ToStringNoBox(Params.Action)), text2);
			}
		}
		string text3 = ((!string.IsNullOrEmpty(templateId)) ? GClass2348.Localized(templateId) : (ParseLocalizationForMessage ? GClass2348.ParseLocalization(text) : text));
		if (string.IsNullOrEmpty(text3))
		{
			if (!string.IsNullOrEmpty(Text))
			{
				Debug.LogError("Localized string is null or empty in (" + _id + ")");
			}
			return string.Empty;
		}
		if (GClass867.HasFlagNoBox(viewRule, EViewRule.RemoveRichText))
		{
			text3 = Regex.Replace(text3, "<.*?>", string.Empty);
		}
		if (GClass867.HasFlagNoBox(viewRule, EViewRule.AddHyperlink))
		{
			foreach (Match item in new Regex("#\\d+").Matches(text3))
			{
				text3 = GClass1673.ReplaceFirst(text3, item.Value, smethod_1("#4db1d0", item.Value));
			}
		}
		if (GClass867.HasFlagNoBox(viewRule, EViewRule.ParseTags) && gClass != null && Regex_0.IsMatch(text3))
		{
			return smethod_0(text3, gClass);
		}
		return text3;
	}

	public static string smethod_0(string text, GClass1060 data)
	{
		return Regex_0.Replace(text, delegate(Match match)
		{
			string value = match.Groups[1].Value;
			Func<GClass1060, string> value2;
			return (!IreadOnlyDictionary_0.TryGetValue(value, out value2)) ? match.Value : value2(data);
		});
	}

	public static string smethod_1(string color, string link)
	{
		return "<color=" + color + "><u><link=" + link.Substring(1) + ">" + link + "</link></u></color>";
	}

	public bool Compare(ChatMessageClass other)
	{
		return other._id == _id;
	}

	public static ChatMessageClass FindOrCreate(string id, Func<ChatMessageClass> constructor)
	{
		ChatMessageClass chatMessageClass = constructor();
		if (string.IsNullOrEmpty(id))
		{
			return chatMessageClass;
		}
		if (SocialNetworkClass.AllMessages.TryGetValue(id, out var value))
		{
			value.UpdateFromAnotherItem(chatMessageClass);
		}
		else
		{
			value = chatMessageClass;
			SocialNetworkClass.AllMessages[id] = value;
		}
		return value;
	}

	public void UpdateFromAnotherItem(ChatMessageClass other)
	{
		Type = other.Type;
		Text = other.Text;
		LocalDateTime = other.LocalDateTime;
		UtcDateTime = other.UtcDateTime;
		Member = other.Member;
		if (other.ReplyTo != null)
		{
			ReplyTo = new ChatMessageClass
			{
				_id = other.ReplyTo._id,
				LocalDateTime = other.ReplyTo.LocalDateTime,
				UtcDateTime = other.ReplyTo.UtcDateTime,
				Member = other.ReplyTo.Member,
				Params = other.ReplyTo.Params,
				ReplyTo = other.ReplyTo.ReplyTo,
				Text = other.ReplyTo.Text,
				Type = other.ReplyTo.Type
			};
		}
		if (other.Params != null)
		{
			Params = new GClass1067
			{
				Action = other.Params.Action,
				Member = other.Params.Member,
				Users = other.Params.Users
			};
		}
	}

	public ChatMessageClass ToSystemMessage()
	{
		return new ChatMessageClass
		{
			_id = _id,
			templateId = templateId,
			systemData = systemData,
			Text = Text,
			LocalDateTime = LocalDateTime,
			UtcDateTime = UtcDateTime,
			Type = EMessageType.SystemMessage,
			MaxStorageTime = MaxStorageTime,
			Items = Items,
			ProfileChangeEvents = ProfileChangeEvents,
			HasRewards = HasRewards
		};
	}

	public ChatMessageClass ToSystemMessageWithParams()
	{
		return new ChatMessageClass
		{
			_id = _id,
			Text = SystemText,
			LocalDateTime = LocalDateTime,
			UtcDateTime = UtcDateTime,
			Type = EMessageType.SystemMessage,
			Params = new GClass1067
			{
				Action = Params.Action,
				Member = Params.Member,
				Users = Params.Users
			}
		};
	}
}
