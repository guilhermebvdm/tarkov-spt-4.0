using System;
using ChatShared;
using Comfort.Communication;

public abstract class GClass1049
{
	public static CommunicatorFactory Create(Action<string> errorCallback = null)
	{
		CommunicatorFactory communicatorFactory = CommunicatorFactory.Create<ChatRPC>(errorCallback);
		communicatorFactory.RegisterProxyFactory((InterfaceFactory<IChatHandle>)GClass1052.Create);
		communicatorFactory.RegisterProxyFactory((InterfaceFactory<IChatMember>)GClass1053.Create);
		communicatorFactory.RegisterProxyFactory((InterfaceFactory<IChatsSession>)GClass1054.Create);
		return communicatorFactory;
	}
}
