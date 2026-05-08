using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ChatShared;
using Comfort.Common;
using Diz.Binding;

public class DialogueClass : IUpdatable<DialogueClass>
{
	[Serializable]
	[CompilerGenerated]
	public class Class769
	{
		public static readonly Class769 class769_0 = new Class769();

		public static Func<ChatMessageClass, bool> func_0;

		public static Func<UpdatableChatMember, bool> func_1;

		public static Func<string, UpdatableChatMember> func_2;

		public bool method_0(ChatMessageClass x)
		{
			return x.DisplayRewardStatus;
		}

		public bool method_1(UpdatableChatMember x)
		{
			return x != null;
		}

		public UpdatableChatMember method_2(string id)
		{
			return new UpdatableChatMember(id);
		}
	}

	[NonSerialized]
	public global::OnEventClass<bool> OnDialogueSelected = new global::OnEventClass<bool>();

	[NonSerialized]
	public BindableEvent OnDialogueRemoved = new BindableEvent();

	[NonSerialized]
	public global::OnEventClass<bool> OnDialoguePinStatusChanged = new global::OnEventClass<bool>();

	[NonSerialized]
	public global::OnEventClass<ChatMessageClass> OnLastMessageUpdated = new global::OnEventClass<ChatMessageClass>();

	[NonSerialized]
	public global::OnEventClass<bool> OnDialogueLoadingStatus = new global::OnEventClass<bool>();

	[NonSerialized]
	public BindableEvent OnDialogueAttachmentsChanged = new BindableEvent();

	public string _id;

	public EMessageType Type;

	public ChatMessageClass Message;

	public DateTime? DeleteTime;

	public int New;

	public int AttachmentsNew;

	public UpdatableChatMember Profile;

	public string Owner;

	public string Name;

	public bool Pinned;

	public GClass1630<UpdatableChatMember> ActiveUsers;

	public LinkedList<UpdatableChatMember> SessionMentionedUsers = new LinkedList<UpdatableChatMember>();

	public GClass1630<ChatMessageClass> ChatMessages;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	public bool HasMessagesWithRewards
	{
		get
		{
			return Bool_0;
		}
		set
		{
			if (Bool_0 != value)
			{
				Bool_0 = value;
				OnDialogueAttachmentsChanged.Invoke();
			}
		}
	}

	public bool MessagesLoaded
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public bool FullyRead
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public bool Updating
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public bool AnyRewardMessage
	{
		get
		{
			if (!HasMessagesWithRewards)
			{
				return ChatMessages.Any((ChatMessageClass x) => x.DisplayRewardStatus);
			}
			return true;
		}
	}

	public bool IsMultiUsersDialog
	{
		get
		{
			if (ActiveUsers.Count > 0 && Type != EMessageType.UserMessage && Type != EMessageType.NpcTraderMessage && Type != EMessageType.SystemMessage)
			{
				return Type != EMessageType.FleamarketMessage;
			}
			return false;
		}
	}

	public DialogueClass()
	{
	}

	public DialogueClass(GClass1059 info, UpdatableChatMember playerMember, GClass1630<ChatMessageClass> messages = null)
	{
		_id = info.Id;
		Type = (EMessageType)info.type;
		DeleteTime = ((info.deleteTime > 0f) ? new DateTime?(EFTDateTimeClass.UniversalDateTimeFromUnixTime(info.deleteTime)) : ((DateTime?)null));
		Message = ((info.message == null) ? new ChatMessageClass() : new ChatMessageClass
		{
			_id = info.message._id,
			templateId = info.message.templateId,
			systemData = info.message.systemData,
			LocalDateTime = info.message.LocalDateTime,
			UtcDateTime = info.message.UtcDateTime,
			Text = ((DeleteTime.GetValueOrDefault(DateTime.MinValue) > info.message.UtcDateTime) ? string.Empty : info.message.Text),
			Type = info.message.Type,
			Params = info.message.Params,
			ReplyTo = info.message.ReplyTo,
			Member = info.message.Member
		});
		New = info.@new;
		AttachmentsNew = info.attachmentsNew;
		Owner = info.Owner;
		Name = info.Name;
		Pinned = info.pinned;
		List<UpdatableChatMember> list = ((info.Users == null) ? new List<UpdatableChatMember>() : info.Users.Where((UpdatableChatMember x) => x != null).ToList());
		foreach (UpdatableChatMember item in list)
		{
			SessionMentionedUsers.AddLast(item);
		}
		if (!list.Contains(playerMember))
		{
			list.Add(playerMember);
		}
		ActiveUsers = new GClass1630<UpdatableChatMember>();
		ActiveUsers.UpdateItems(list);
		Profile = ((ActiveUsers.Count > 0) ? ActiveUsers[0] : UpdatableChatMember.FindOrCreate(_id, (string id) => new UpdatableChatMember(id)));
		ChatMessages = messages ?? new GClass1630<ChatMessageClass>();
		MessagesLoaded = Type == EMessageType.GlobalChat;
	}

	public bool CanSendMessage(SocialNetworkClass socialNetwork, out string error)
	{
		bool num = Type == EMessageType.GroupChatMessage;
		bool flag = SocialNetworkClass.IgnoringYouList.Contains(Profile);
		bool ignored = Profile.Info.Ignored;
		bool flag2 = socialNetwork.FriendsList.Contains(Profile);
		if (flag)
		{
			error = "You can't send message to this user. You are in ignore list.";
		}
		else if (ignored)
		{
			error = "You can't send message to this user. He is in ignore list.";
		}
		else if (!flag2)
		{
			error = "You can't send message to this user. He is not in your friends list.";
		}
		else
		{
			error = null;
		}
		if (!num)
		{
			return !flag && !ignored && flag2;
		}
		return true;
	}

	public void UpdateDialogMessages(IChatInteractions session, double time)
	{
		if (Updating || (MessagesLoaded && time <= 0.0) || Type == EMessageType.GlobalChat)
		{
			return;
		}
		Updating = true;
		OnDialogueLoadingStatus?.Invoke(arg: true);
		session.ChatGetDialogMessages(_id, (int)Type, 30, time, delegate(Result<GClass1061> result)
		{
			Updating = false;
			OnDialogueLoadingStatus?.Invoke(arg: false);
			if (string.IsNullOrEmpty(result.Error))
			{
				ChatMessageClass[] messages = result.Value.messages;
				IEnumerable<ChatMessageClass> enumerable2;
				if (ChatMessages.Count <= 0)
				{
					IEnumerable<ChatMessageClass> enumerable = messages;
					enumerable2 = enumerable;
				}
				else
				{
					enumerable2 = ChatMessages.Concat(messages);
				}
				IEnumerable<ChatMessageClass> source = enumerable2;
				ChatMessages.UpdateItems(source.ToArray());
				MessagesLoaded = true;
				HasMessagesWithRewards = result.Value.hasMessagesWithRewards;
				if (messages.Length < 30)
				{
					FullyRead = true;
				}
			}
		});
	}

	public void SelectDialogue(bool value)
	{
		OnDialogueSelected.Invoke(value);
	}

	public void RemoveDialogue()
	{
		OnDialogueRemoved.Invoke();
	}

	public bool Compare(DialogueClass other)
	{
		return other._id == _id;
	}

	public void UpdateFromAnotherItem(DialogueClass other)
	{
		Type = other.Type;
		Message = new ChatMessageClass
		{
			templateId = other.Message.templateId,
			systemData = other.Message.systemData,
			LocalDateTime = other.Message.LocalDateTime,
			UtcDateTime = other.Message.UtcDateTime,
			Text = other.Message.Text,
			Type = other.Message.Type,
			Params = other.Message.Params,
			Member = other.Message.Member,
			ReplyTo = other.Message.ReplyTo,
			_id = other.Message._id
		};
		New = other.New;
		AttachmentsNew = other.AttachmentsNew;
		HasMessagesWithRewards = other.HasMessagesWithRewards;
		Profile = other.Profile;
		Owner = other.Owner;
		Name = other.Name;
		Pinned = other.Pinned;
		ActiveUsers = other.ActiveUsers;
	}

	[CompilerGenerated]
	public void method_0(Result<GClass1061> result)
	{
		Updating = false;
		OnDialogueLoadingStatus?.Invoke(arg: false);
		if (string.IsNullOrEmpty(result.Error))
		{
			ChatMessageClass[] messages = result.Value.messages;
			IEnumerable<ChatMessageClass> enumerable2;
			if (ChatMessages.Count <= 0)
			{
				IEnumerable<ChatMessageClass> enumerable = messages;
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = ChatMessages.Concat(messages);
			}
			IEnumerable<ChatMessageClass> source = enumerable2;
			ChatMessages.UpdateItems(source.ToArray());
			MessagesLoaded = true;
			HasMessagesWithRewards = result.Value.hasMessagesWithRewards;
			if (messages.Length < 30)
			{
				FullyRead = true;
			}
		}
	}
}
