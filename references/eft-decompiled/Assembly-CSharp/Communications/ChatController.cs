using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ChatShared;
using Comfort.Common;
using UnityEngine;

namespace Communications;

[RequireComponent(typeof(ChatClient))]
public class ChatController : MonoBehaviour, IChatMember
{
	[Serializable]
	[CompilerGenerated]
	public class Class755
	{
		public static readonly Class755 class755_0 = new Class755();

		public static Func<string, UpdatableChatMember> func_0;

		public static Func<ChatRoomMember, UpdatableChatMember> func_1;

		public static Func<UpdatableChatMember, bool> func_2;

		public static Func<UpdatableChatMember, bool> func_3;

		public static Func<UpdatableChatMember, bool> func_4;

		public static Func<UpdatableChatMember, bool> func_5;

		public static Func<UpdatableChatMember, bool> func_6;

		public static Func<UpdatableChatMember, bool> func_7;

		public static Action action_0;

		public UpdatableChatMember method_0(ChatRoomMember info)
		{
			UpdatableChatMember updatableChatMember = UpdatableChatMember.FindOrCreate(info.Id, (string id) => new UpdatableChatMember(id));
			updatableChatMember.UpdateFromChatMember(info);
			return updatableChatMember;
		}

		public UpdatableChatMember method_1(string id)
		{
			return new UpdatableChatMember(id);
		}

		public bool method_2(UpdatableChatMember x)
		{
			return x != null;
		}

		public bool method_3(UpdatableChatMember x)
		{
			return x != null;
		}

		public bool method_4(UpdatableChatMember x)
		{
			return x != null;
		}

		public bool method_5(UpdatableChatMember x)
		{
			return x != null;
		}

		public bool method_6(UpdatableChatMember x)
		{
			return x != null;
		}

		public bool method_7(UpdatableChatMember x)
		{
			return x != null;
		}

		public void method_8()
		{
		}
	}

	[CompilerGenerated]
	public class Class756
	{
		public Message message;

		public bool method_0(UpdatableChatMember member)
		{
			return member.Id == message.SenderId;
		}
	}

	[CompilerGenerated]
	public class Class757
	{
		public Message replayMessage;

		public Message message;

		public bool method_0(UpdatableChatMember member)
		{
			return member.Id == replayMessage.SenderId;
		}

		public bool method_1(UpdatableChatMember member)
		{
			return member.Id == message.SenderId;
		}
	}

	[CompilerGenerated]
	public class Class758
	{
		public ChatRoomMember member;

		public UpdatableChatMember method_0(string id)
		{
			UpdatableChatMember updatableChatMember = new UpdatableChatMember(id);
			updatableChatMember.UpdateFromChatMember(member);
			return updatableChatMember;
		}
	}

	[CompilerGenerated]
	public class Class759
	{
		public string[] members;

		public bool method_0(UpdatableChatMember member)
		{
			return members.Contains(member.Id);
		}
	}

	[CompilerGenerated]
	public class Class760
	{
		public string memberId;

		public bool method_0(UpdatableChatMember x)
		{
			return x.Id == memberId;
		}
	}

	[CompilerGenerated]
	public class Class761
	{
		public string memberId;

		public bool method_0(UpdatableChatMember x)
		{
			return x.Id == memberId;
		}
	}

	private ChatClient chatClient_0;

	private SocialNetworkClass socialNetworkClass;

	private IChatHandle ichatHandle_0;

	private DialogueClass dialogueClass;

	[CompilerGenerated]
	private string string_0;

	public string ChatId
	{
		[CompilerGenerated]
		get
		{
			return string_0;
		}
		[CompilerGenerated]
		set
		{
			string_0 = value;
		}
	}

	public static ChatController Create(ChatClient chatClient)
	{
		ChatController chatController = chatClient.gameObject.AddComponent<ChatController>();
		chatController.chatClient_0 = chatClient;
		return chatController;
	}

	public void SessionStart(SocialNetworkClass socialNetwork)
	{
		socialNetworkClass = socialNetwork;
		if (string.IsNullOrEmpty(ChatId))
		{
			chatClient_0.Session.Start(this, method_0);
		}
		else
		{
			chatClient_0.Session.Start(this, ChatId, method_0);
		}
	}

	public void SessionRestart(string chatId, string ip, int port)
	{
		ChatId = chatId;
		chatClient_0.Reconnect(ip, port);
	}

	public void SessionStop()
	{
		if (socialNetworkClass != null)
		{
			socialNetworkClass.GDelegate33_0 = null;
			if (dialogueClass != null)
			{
				socialNetworkClass.RemoveDialogue(dialogueClass);
			}
			socialNetworkClass = null;
		}
		dialogueClass = null;
		ichatHandle_0 = null;
	}

	public void method_0(Result<IChatHandle, ChatInfo> result)
	{
		if (socialNetworkClass == null)
		{
			if (result.Succeed)
			{
				Debug.LogError("IChatHandle created in closed ChatController");
			}
		}
		else if (result.Succeed)
		{
			method_1(result);
		}
		else
		{
			Debug.LogError(result.Error);
			ChatId = null;
			chatClient_0.Session.Start(this, method_0);
		}
	}

	public void method_1(Result<IChatHandle, ChatInfo> result)
	{
		socialNetworkClass.GDelegate33_0 = method_2;
		ichatHandle_0 = result.Value0;
		ChatInfo value = result.Value1;
		UpdatableChatMember[] users = value.Members.Select(delegate(ChatRoomMember info)
		{
			UpdatableChatMember updatableChatMember = UpdatableChatMember.FindOrCreate(info.Id, (string id) => new UpdatableChatMember(id));
			updatableChatMember.UpdateFromChatMember(info);
			return updatableChatMember;
		}).ToArray();
		GClass1059 roomInformation = new GClass1059
		{
			type = 9,
			Id = value.Id,
			Name = value.Name,
			@new = 0,
			attachmentsNew = 0,
			pinned = true,
			message = new ChatMessageClass
			{
				LocalDateTime = EFTDateTimeClass.Now,
				UtcDateTime = EFTDateTimeClass.UtcNow,
				Text = "",
				Type = EMessageType.UserMessage,
				_id = Guid.NewGuid().ToString()
			},
			Users = users
		};
		dialogueClass = socialNetworkClass.CreateDialogue(roomInformation, select: true);
	}

	public void method_2(ChatMessageClass message, ChatMessageClass replayToMessage, Action callback)
	{
		if (ichatHandle_0 != null)
		{
			if (replayToMessage != null)
			{
				ichatHandle_0.Replay(message.Text, replayToMessage._id);
			}
			else
			{
				ichatHandle_0.Send(message.Text);
			}
		}
		else
		{
			Debug.LogError("Invalid chat pointer");
		}
	}

	public void Receive(Message message)
	{
		ChatMessageClass message2 = new ChatMessageClass
		{
			LocalDateTime = EFTDateTimeClass.Now,
			UtcDateTime = EFTDateTimeClass.UtcNow,
			Text = message.Text,
			Type = EMessageType.UserMessage,
			_id = message.Id,
			Member = dialogueClass.ActiveUsers.Where((UpdatableChatMember x) => x != null).SingleOrDefault((UpdatableChatMember member) => member.Id == message.SenderId)
		};
		socialNetworkClass.DisplayMessage(message2, dialogueClass);
	}

	void IChatMember.Receive(Message message)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Receive
		this.Receive(message);
	}

	public void ReceiveReplay(Message message, Message replayMessage)
	{
		if (dialogueClass != null)
		{
			ChatMessageClass replyTo = new ChatMessageClass
			{
				LocalDateTime = EFTDateTimeClass.Now,
				UtcDateTime = EFTDateTimeClass.UtcNow,
				Text = replayMessage.Text,
				Type = EMessageType.UserMessage,
				Member = dialogueClass.SessionMentionedUsers.Where((UpdatableChatMember x) => x != null).FirstOrDefault((UpdatableChatMember member) => member.Id == replayMessage.SenderId),
				_id = replayMessage.Id
			};
			ChatMessageClass message2 = new ChatMessageClass
			{
				LocalDateTime = EFTDateTimeClass.Now,
				UtcDateTime = EFTDateTimeClass.UtcNow,
				Text = message.Text,
				Type = EMessageType.UserMessage,
				_id = message.Id,
				Member = dialogueClass.ActiveUsers.Where((UpdatableChatMember x) => x != null).FirstOrDefault((UpdatableChatMember member) => member.Id == message.SenderId),
				ReplyTo = replyTo
			};
			socialNetworkClass.DisplayMessage(message2, dialogueClass);
		}
	}

	void IChatMember.ReceiveReplay(Message message, Message replayMessage)
	{
		//ILSpy generated this explicit interface implementation from .override directive in ReceiveReplay
		this.ReceiveReplay(message, replayMessage);
	}

	public void Drop()
	{
		SessionStop();
	}

	void IChatMember.Drop()
	{
		//ILSpy generated this explicit interface implementation from .override directive in Drop
		this.Drop();
	}

	public void Add(ChatRoomMember[] members)
	{
		if (dialogueClass == null)
		{
			return;
		}
		if (dialogueClass.ActiveUsers == null)
		{
			Debug.LogError("ChatController.Add _dialogue.ActiveUsers == null");
			return;
		}
		if (dialogueClass.SessionMentionedUsers == null)
		{
			Debug.LogError("ChatController.Add _dialogue.SessionMentionedUsers == null");
			return;
		}
		if (members == null)
		{
			Debug.LogError("ChatController.Add members == null");
			return;
		}
		foreach (ChatRoomMember member in members)
		{
			if (member == null)
			{
				Debug.LogError("ChatController.Add members.member == null");
				continue;
			}
			UpdatableChatMember updatableChatMember = UpdatableChatMember.FindOrCreate(member.Id, delegate(string id)
			{
				UpdatableChatMember updatableChatMember2 = new UpdatableChatMember(id);
				updatableChatMember2.UpdateFromChatMember(member);
				return updatableChatMember2;
			});
			if (!dialogueClass.ActiveUsers.Contains(updatableChatMember))
			{
				dialogueClass.ActiveUsers.Add(updatableChatMember);
			}
			if (dialogueClass.SessionMentionedUsers.Contains(updatableChatMember))
			{
				dialogueClass.SessionMentionedUsers.Remove(updatableChatMember);
			}
			dialogueClass.SessionMentionedUsers.AddLast(updatableChatMember);
		}
	}

	void IChatMember.Add(ChatRoomMember[] members)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Add
		this.Add(members);
	}

	public void Remove(string[] members)
	{
		if (dialogueClass != null)
		{
			UpdatableChatMember[] array = (from member in dialogueClass.ActiveUsers
				where member != null
				where members.Contains(member.Id)
				select member).ToArray();
			foreach (UpdatableChatMember existingItem in array)
			{
				dialogueClass.ActiveUsers.Remove(existingItem);
			}
		}
	}

	void IChatMember.Remove(string[] members)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Remove
		this.Remove(members);
	}

	public void SetBanned(string memberId, DateTime toDateTime)
	{
		socialNetworkClass.SetBannedPlayerStatus(dialogueClass.ActiveUsers.Where((UpdatableChatMember x) => x != null).FirstOrDefault((UpdatableChatMember x) => x.Id == memberId), toDateTime);
	}

	void IChatMember.SetBanned(string memberId, DateTime toDateTime)
	{
		//ILSpy generated this explicit interface implementation from .override directive in SetBanned
		this.SetBanned(memberId, toDateTime);
	}

	public void SetUnbanned(string memberId)
	{
		socialNetworkClass.SetUnbannedPlayerStatus(dialogueClass.ActiveUsers.Where((UpdatableChatMember x) => x != null).FirstOrDefault((UpdatableChatMember x) => x.Id == memberId));
	}

	void IChatMember.SetUnbanned(string memberId)
	{
		//ILSpy generated this explicit interface implementation from .override directive in SetUnbanned
		this.SetUnbanned(memberId);
	}

	public void OnDestroy()
	{
		Interlocked.Exchange(ref ichatHandle_0, null)?.Close(delegate
		{
		});
	}
}
