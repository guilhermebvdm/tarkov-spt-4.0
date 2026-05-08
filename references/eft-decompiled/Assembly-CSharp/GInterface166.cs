using System;
using EFT;
using EFT.Network;

public interface GInterface166
{
	GClass3071 Connection { get; }

	GClass3066 NetworkCryptography { get; }

	void AddMessageListener(NetworkMessageType messageType, Action listener);

	void AddMessageListener(NetworkMessageType messageType, Action<byte[], int> listener);

	void AddMessageListener(NetworkMessageType messageType, Action<GStruct416> listener);

	void AddMessageListener(NetworkMessageType messageType, Action<NetworkChannel, short, byte[], int> listener);

	void RemoveMessageListener(NetworkMessageType messageType);

	void Send(NetworkChannel channel, NetworkMessageType messageType);

	void Send(NetworkChannel channel, NetworkMessageType messageType, GInterface382 message);

	void Send(NetworkChannel channel, NetworkMessageType messageType, ArraySegment<byte> buffer);

	void Send(NetworkChannel channel, NetworkMessageType messageType, byte[] buffer, int bufferOffset, int bufferCount);
}
