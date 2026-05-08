using System;
using System.Runtime.CompilerServices;
using ChatShared;
using EFT;

public class GClass1058 : ChatRoomMember, ISerializer<UpdatableChatMember>
{
	[Serializable]
	[CompilerGenerated]
	public class Class763
	{
		public static readonly Class763 class763_0 = new Class763();

		public static Func<string, UpdatableChatMember> func_0;

		public UpdatableChatMember method_0(string id)
		{
			return new UpdatableChatMember(id);
		}
	}

	public UpdatableChatMember Deserialize()
	{
		UpdatableChatMember updatableChatMember = UpdatableChatMember.FindOrCreate(Id, (string id) => new UpdatableChatMember(id));
		updatableChatMember.UpdateFromChatMember(this);
		return updatableChatMember;
	}

	UpdatableChatMember ISerializer<UpdatableChatMember>.Deserialize()
	{
		//ILSpy generated this explicit interface implementation from .override directive in Deserialize
		return this.Deserialize();
	}

	public object Serialize(UpdatableChatMember t)
	{
		throw new NotImplementedException();
	}
}
