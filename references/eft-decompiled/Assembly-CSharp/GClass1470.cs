using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using FlyingWormConsole3.LiteNetLib;

public class GClass1470
{
	[NonSerialized]
	public GClass1476 Gclass1476_0;

	[NonSerialized]
	public int Int_0;

	public readonly GClass1482 Data;

	[NonSerialized]
	[CompilerGenerated]
	public ConnectionRequestResult ConnectionRequestResult_0_1;

	[NonSerialized]
	public long Long_0;

	[NonSerialized]
	public byte Byte_0;

	public readonly IPEndPoint RemoteEndPoint;

	public ConnectionRequestResult ConnectionRequestResult_0
	{
		[CompilerGenerated]
		get
		{
			return ConnectionRequestResult_0_1;
		}
		[CompilerGenerated]
		set
		{
			ConnectionRequestResult_0_1 = value;
		}
	}

	public bool method_0()
	{
		return Interlocked.CompareExchange(ref Int_0, 1, 0) == 0;
	}

	public void method_1(Class942 connRequest)
	{
		if (connRequest.ConnectionTime >= Long_0)
		{
			Long_0 = connRequest.ConnectionTime;
			Byte_0 = connRequest.ConnectionNumber;
		}
	}

	public GClass1470(long connectionId, byte connectionNumber, GClass1482 netDataReader, IPEndPoint endPoint, GClass1476 listener)
	{
		Long_0 = connectionId;
		Byte_0 = connectionNumber;
		RemoteEndPoint = endPoint;
		Data = netDataReader;
		Gclass1476_0 = listener;
	}

	public GClass1477 AcceptIfKey(string key)
	{
		if (!method_0())
		{
			return null;
		}
		try
		{
			if (Data.GetString() == key)
			{
				ConnectionRequestResult_0 = ConnectionRequestResult.Accept;
			}
		}
		catch
		{
			GClass1475.smethod_5("[AC] Invalid incoming data");
		}
		if (ConnectionRequestResult_0 == ConnectionRequestResult.Accept)
		{
			return Gclass1476_0.method_18(this, null, 0, 0);
		}
		ConnectionRequestResult_0 = ConnectionRequestResult.Reject;
		Gclass1476_0.method_18(this, null, 0, 0);
		return null;
	}

	public GClass1477 Accept()
	{
		if (!method_0())
		{
			return null;
		}
		ConnectionRequestResult_0 = ConnectionRequestResult.Accept;
		return Gclass1476_0.method_18(this, null, 0, 0);
	}

	public void Reject(byte[] rejectData, int start, int length, bool force)
	{
		if (method_0())
		{
			ConnectionRequestResult_0 = (force ? ConnectionRequestResult.RejectForce : ConnectionRequestResult.Reject);
			Gclass1476_0.method_18(this, rejectData, start, length);
		}
	}

	public void Reject(byte[] rejectData, int start, int length)
	{
		Reject(rejectData, start, length, force: false);
	}

	public void RejectForce(byte[] rejectData, int start, int length)
	{
		Reject(rejectData, start, length, force: true);
	}

	public void RejectForce()
	{
		Reject(null, 0, 0, force: true);
	}

	public void RejectForce(byte[] rejectData)
	{
		Reject(rejectData, 0, rejectData.Length, force: true);
	}

	public void RejectForce(GClass1484 rejectData)
	{
		Reject(rejectData.Data, 0, rejectData.Length, force: true);
	}

	public void Reject()
	{
		Reject(null, 0, 0, force: false);
	}

	public void Reject(byte[] rejectData)
	{
		Reject(rejectData, 0, rejectData.Length, force: false);
	}

	public void Reject(GClass1484 rejectData)
	{
		Reject(rejectData.Data, 0, rejectData.Length, force: false);
	}
}
