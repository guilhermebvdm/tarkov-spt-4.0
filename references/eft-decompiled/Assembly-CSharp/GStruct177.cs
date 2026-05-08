using Dissonance.Networking;

public struct GStruct177
{
	public uint Packets;

	public uint Bytes;

	public uint BytesPerSecond;

	public override string ToString()
	{
		return TrafficCounter.Format(Packets, Bytes, BytesPerSecond);
	}
}
