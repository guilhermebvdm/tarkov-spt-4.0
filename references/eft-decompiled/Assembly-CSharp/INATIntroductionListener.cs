using System.Net;
using FlyingWormConsole3.LiteNetLib;

public interface INATIntroductionListener
{
	void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token);

	void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token);
}
