using System;
using System.Reflection;
using ChatShared;
using Comfort.Common;
using Comfort.Communication;

public class GClass1054 : Proxy4Interface<IChatsSession>, IChatsSession
{
	public static GClass1054 Create(Delegate[] delegates)
	{
		return new GClass1054(delegates);
	}

	public GClass1054(Delegate[] delegates)
		: base(delegates)
	{
	}

	public GClass1054(Delegate[] delegates, object obj)
		: base(delegates, obj)
	{
	}

	public void Start(IChatMember member, Callback<IChatHandle, ChatInfo> callback)
	{
		Call(MethodBase.GetCurrentMethod(), member, callback);
	}

	void IChatsSession.Start(IChatMember member, Callback<IChatHandle, ChatInfo> callback)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Start
		this.Start(member, callback);
	}

	public void Start_1(IChatMember member, string chatId, Callback<IChatHandle, ChatInfo> callback)
	{
		Call(MethodBase.GetCurrentMethod(), member, chatId, callback);
	}

	void IChatsSession.Start(IChatMember member, string chatId, Callback<IChatHandle, ChatInfo> callback)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Start_1
		this.Start_1(member, chatId, callback);
	}

	public void Close(Action callback)
	{
		Call(MethodBase.GetCurrentMethod(), callback);
	}

	void IChatsSession.Close(Action callback)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Close
		this.Close(callback);
	}

	public void Ban(string profileId, TimeSpan timeSpan, string description, Callback<DateTime> callback)
	{
		Call(MethodBase.GetCurrentMethod(), profileId, timeSpan, description, callback);
	}

	void IChatsSession.Ban(string profileId, TimeSpan timeSpan, string description, Callback<DateTime> callback)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Ban
		this.Ban(profileId, timeSpan, description, callback);
	}

	public void Unban(string profileId, string description, Callback callback)
	{
		Call(MethodBase.GetCurrentMethod(), profileId, description, callback);
	}

	void IChatsSession.Unban(string profileId, string description, Callback callback)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Unban
		this.Unban(profileId, description, callback);
	}
}
