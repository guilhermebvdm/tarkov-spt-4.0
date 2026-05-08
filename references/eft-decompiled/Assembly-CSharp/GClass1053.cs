using System;
using System.Reflection;
using ChatShared;
using Comfort.Communication;

public class GClass1053 : Proxy4Interface<IChatMember>, IChatMember
{
	public static GClass1053 Create(Delegate[] delegates)
	{
		return new GClass1053(delegates);
	}

	public GClass1053(Delegate[] delegates)
		: base(delegates)
	{
	}

	public GClass1053(Delegate[] delegates, object obj)
		: base(delegates, obj)
	{
	}

	public void Add(ChatRoomMember[] members)
	{
		Call(MethodBase.GetCurrentMethod(), new object[1] { members });
	}

	void IChatMember.Add(ChatRoomMember[] members)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Add
		this.Add(members);
	}

	public void Remove(string[] members)
	{
		Call(MethodBase.GetCurrentMethod(), new object[1] { members });
	}

	void IChatMember.Remove(string[] members)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Remove
		this.Remove(members);
	}

	public void Receive(Message message)
	{
		Call(MethodBase.GetCurrentMethod(), message);
	}

	void IChatMember.Receive(Message message)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Receive
		this.Receive(message);
	}

	public void ReceiveReplay(Message message, Message replayMessage)
	{
		Call(MethodBase.GetCurrentMethod(), message, replayMessage);
	}

	void IChatMember.ReceiveReplay(Message message, Message replayMessage)
	{
		//ILSpy generated this explicit interface implementation from .override directive in ReceiveReplay
		this.ReceiveReplay(message, replayMessage);
	}

	public void SetBanned(string memberId, DateTime toDateTime)
	{
		Call(MethodBase.GetCurrentMethod(), memberId, toDateTime);
	}

	void IChatMember.SetBanned(string memberId, DateTime toDateTime)
	{
		//ILSpy generated this explicit interface implementation from .override directive in SetBanned
		this.SetBanned(memberId, toDateTime);
	}

	public void SetUnbanned(string memberId)
	{
		Call(MethodBase.GetCurrentMethod(), memberId);
	}

	void IChatMember.SetUnbanned(string memberId)
	{
		//ILSpy generated this explicit interface implementation from .override directive in SetUnbanned
		this.SetUnbanned(memberId);
	}

	public void Drop()
	{
		Call(MethodBase.GetCurrentMethod());
	}

	void IChatMember.Drop()
	{
		//ILSpy generated this explicit interface implementation from .override directive in Drop
		this.Drop();
	}
}
