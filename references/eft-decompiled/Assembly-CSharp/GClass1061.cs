using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ChatShared;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using Sirenix.Utilities;

public class GClass1061
{
	public class GClass1062 : ISerializer<ChatMessageClass.GClass1067>
	{
		[Serializable]
		[CompilerGenerated]
		public class Class764
		{
			public static readonly Class764 class764_0 = new Class764();

			public static Func<string, UpdatableChatMember> func_0;

			public UpdatableChatMember method_0(string id)
			{
				return new UpdatableChatMember(id);
			}
		}

		public string Action;

		public string uid;

		public string uid2;

		public string[] users;

		public UpdatableChatMember profile;

		public ChatMessageClass.GClass1067 Deserialize()
		{
			return new ChatMessageClass.GClass1067
			{
				Action = GClass856.ParseEnum<EMessageParamsAction>(Action),
				Member = method_0(uid),
				Users = ((users == null) ? new UpdatableChatMember[0] : users.Select(method_0).ToArray())
			};
		}

		public UpdatableChatMember method_0(string memberId)
		{
			return profile ?? UpdatableChatMember.FindOrCreate(memberId, (string id) => new UpdatableChatMember(id));
		}

		public object Serialize(ChatMessageClass.GClass1067 t)
		{
			throw new NotImplementedException();
		}
	}

	public class GClass1063 : ISerializer<GClass1065>
	{
		[Serializable]
		[CompilerGenerated]
		public class Class765
		{
			public static readonly Class765 class765_0 = new Class765();

			public static Func<FlatItemsDataClass, bool> func_0;

			public bool method_0(FlatItemsDataClass x)
			{
				return x != null;
			}
		}

		[NonSerialized]
		[CompilerGenerated]
		public MongoID MongoID_0;

		[NonSerialized]
		[CompilerGenerated]
		public FlatItemsDataClass[] Gclass1390_0;

		[JsonProperty("stash")]
		public MongoID StashId
		{
			[CompilerGenerated]
			get
			{
				return MongoID_0;
			}
			[CompilerGenerated]
			set
			{
				MongoID_0 = value;
			}
		}

		[JsonProperty("data")]
		public FlatItemsDataClass[] Items
		{
			[CompilerGenerated]
			get
			{
				return Gclass1390_0;
			}
			[CompilerGenerated]
			set
			{
				Gclass1390_0 = value;
			}
		}

		public GClass1065 Deserialize()
		{
			StashItemClass stashItemClass = Singleton<ItemFactoryClass>.Instance.CreateFakeStash(StashId);
			stashItemClass.Grids[0] = new StashGridClass("main", 10, 25, canStretchVertically: true, new ItemFilter[0], stashItemClass);
			if (Items != null)
			{
				Singleton<ItemFactoryClass>.Instance.FlatItemsToTree(Items.Where((FlatItemsDataClass x) => x != null).ToArray(), silentMode: false, new Dictionary<string, Item> { { stashItemClass.Id, stashItemClass } });
			}
			return new GClass1065
			{
				Stash = stashItemClass
			};
		}

		public object Serialize(GClass1065 t)
		{
			throw new NotImplementedException();
		}
	}

	public class GClass1064 : ISerializer<ChatMessageClass>
	{
		[Serializable]
		[CompilerGenerated]
		public class Class766
		{
			public static readonly Class766 class766_0 = new Class766();

			public static Func<string, UpdatableChatMember> func_0;

			public UpdatableChatMember method_0(string id)
			{
				return new UpdatableChatMember(id);
			}
		}

		public string _id;

		public string templateId;

		public GClass1060 systemData;

		public int type;

		public string text;

		public double dt;

		public string uid;

		public bool hasRewards;

		public long maxStorageTime;

		public ChatMessageClass replyTo;

		public ChatMessageClass.GClass1067 @params;

		public GClass1065 items;

		public ProfileChangeEvent[] ProfileChangeEvents;

		public ChatMessageClass Deserialize()
		{
			return ChatMessageClass.FindOrCreate(_id, delegate
			{
				if (items != null)
				{
					new TraderControllerClass(items.Stash, _id, Guid.NewGuid().ToString(), canBeLocalized: false, EOwnerType.Mail);
				}
				ChatMessageClass obj = new ChatMessageClass
				{
					_id = _id,
					templateId = templateId,
					systemData = systemData,
					Type = (EMessageType)type,
					Text = text,
					LocalDateTime = EFTDateTimeClass.LocalDateTimeFromUnixTime(dt),
					UtcDateTime = EFTDateTimeClass.UniversalDateTimeFromUnixTime(dt),
					Member = UpdatableChatMember.FindOrCreate(uid, (string id) => new UpdatableChatMember(id)),
					ReplyTo = replyTo,
					Params = @params,
					HasRewards = hasRewards,
					MaxStorageTime = maxStorageTime,
					Items = items
				};
				ProfileChangeEvent[] profileChangeEvents = ProfileChangeEvents;
				object obj2;
				if (profileChangeEvents == null)
				{
					obj2 = null;
				}
				else
				{
					obj2 = profileChangeEvents.ForEach(delegate(ProfileChangeEvent @event)
					{
						@event.MessageId = _id;
					}).ToArray();
					if (obj2 != null)
					{
						goto IL_0131;
					}
				}
				obj2 = new ProfileChangeEvent[0];
				goto IL_0131;
				IL_0131:
				obj.ProfileChangeEvents = (ProfileChangeEvent[])obj2;
				return obj;
			});
		}

		public object Serialize(ChatMessageClass t)
		{
			throw new NotImplementedException();
		}

		[CompilerGenerated]
		public ChatMessageClass method_0()
		{
			if (items != null)
			{
				new TraderControllerClass(items.Stash, _id, Guid.NewGuid().ToString(), canBeLocalized: false, EOwnerType.Mail);
			}
			ChatMessageClass obj = new ChatMessageClass
			{
				_id = _id,
				templateId = templateId,
				systemData = systemData,
				Type = (EMessageType)type,
				Text = text,
				LocalDateTime = EFTDateTimeClass.LocalDateTimeFromUnixTime(dt),
				UtcDateTime = EFTDateTimeClass.UniversalDateTimeFromUnixTime(dt),
				Member = UpdatableChatMember.FindOrCreate(uid, (string id) => new UpdatableChatMember(id)),
				ReplyTo = replyTo,
				Params = @params,
				HasRewards = hasRewards,
				MaxStorageTime = maxStorageTime,
				Items = items
			};
			ProfileChangeEvent[] profileChangeEvents = ProfileChangeEvents;
			object obj2;
			if (profileChangeEvents == null)
			{
				obj2 = null;
			}
			else
			{
				obj2 = profileChangeEvents.ForEach(delegate(ProfileChangeEvent @event)
				{
					@event.MessageId = _id;
				}).ToArray();
				if (obj2 != null)
				{
					goto IL_0131;
				}
			}
			obj2 = new ProfileChangeEvent[0];
			goto IL_0131;
			IL_0131:
			obj.ProfileChangeEvents = (ProfileChangeEvent[])obj2;
			return obj;
		}

		[CompilerGenerated]
		public void method_1(ProfileChangeEvent @event)
		{
			@event.MessageId = _id;
		}
	}

	public ChatMessageClass[] messages;

	public UpdatableChatMember[] profiles;

	public bool hasMessagesWithRewards;
}
