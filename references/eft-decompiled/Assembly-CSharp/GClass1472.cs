using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using FlyingWormConsole3.LiteNetLib;

public class GClass1472 : INATIntroductionListener
{
	public delegate void GDelegate60(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token);

	public delegate void GDelegate61(IPEndPoint targetEndPoint, NatAddressType type, string token);

	[CompilerGenerated]
	private GDelegate60 gdelegate60_0;

	[CompilerGenerated]
	private GDelegate61 gdelegate61_0;

	public event GDelegate60 NatIntroductionRequest
	{
		[CompilerGenerated]
		add
		{
			GDelegate60 gDelegate = gdelegate60_0;
			GDelegate60 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate60 value2 = (GDelegate60)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate60_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate60 gDelegate = gdelegate60_0;
			GDelegate60 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate60 value2 = (GDelegate60)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate60_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public event GDelegate61 NatIntroductionSuccess
	{
		[CompilerGenerated]
		add
		{
			GDelegate61 gDelegate = gdelegate61_0;
			GDelegate61 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate61 value2 = (GDelegate61)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate61_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate61 gDelegate = gdelegate61_0;
			GDelegate61 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate61 value2 = (GDelegate61)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate61_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
	{
		if (gdelegate60_0 != null)
		{
			gdelegate60_0(localEndPoint, remoteEndPoint, token);
		}
	}

	void INATIntroductionListener.OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnNatIntroductionRequest
		this.OnNatIntroductionRequest(localEndPoint, remoteEndPoint, token);
	}

	public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
	{
		if (gdelegate61_0 != null)
		{
			gdelegate61_0(targetEndPoint, type, token);
		}
	}

	void INATIntroductionListener.OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnNatIntroductionSuccess
		this.OnNatIntroductionSuccess(targetEndPoint, type, token);
	}
}
