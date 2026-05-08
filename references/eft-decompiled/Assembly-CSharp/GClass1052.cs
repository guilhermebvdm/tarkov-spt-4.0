using System;
using System.Reflection;
using ChatShared;
using Comfort.Communication;

public class GClass1052 : Proxy4Interface<IChatHandle>, IChatHandle
{
	public static GClass1052 Create(Delegate[] delegates)
	{
		return new GClass1052(delegates);
	}

	public GClass1052(Delegate[] delegates)
		: base(delegates)
	{
	}

	public GClass1052(Delegate[] delegates, object obj)
		: base(delegates, obj)
	{
	}

	public void Send(string text)
	{
		Call(MethodBase.GetCurrentMethod(), text);
	}

	void IChatHandle.Send(string text)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Send
		this.Send(text);
	}

	public void Replay(string text, string replayMessageId)
	{
		Call(MethodBase.GetCurrentMethod(), text, replayMessageId);
	}

	void IChatHandle.Replay(string text, string replayMessageId)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Replay
		this.Replay(text, replayMessageId);
	}

	public void Close(Action callback)
	{
		Call(MethodBase.GetCurrentMethod(), callback);
	}

	void IChatHandle.Close(Action callback)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Close
		this.Close(callback);
	}
}
