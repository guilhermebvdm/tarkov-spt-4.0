using System;
using System.Runtime.CompilerServices;
using FlyingWormConsole3.LiteNetLib.Utils;

public class GClass1487
{
	[NonSerialized]
	public static DateTime DateTime_0 = new DateTime(1900, 1, 1);

	[NonSerialized]
	[CompilerGenerated]
	public byte[] Byte_0;

	[NonSerialized]
	[CompilerGenerated]
	public DateTime? Nullable_0;

	public byte[] Bytes
	{
		[CompilerGenerated]
		get
		{
			return Byte_0;
		}
		[CompilerGenerated]
		set
		{
			Byte_0 = value;
		}
	}

	public NtpLeapIndicator LeapIndicator => (NtpLeapIndicator)((Bytes[0] & 0xC0) >> 6);

	public int VersionNumber
	{
		get
		{
			return (Bytes[0] & 0x38) >> 3;
		}
		set
		{
			Bytes[0] = (byte)((Bytes[0] & -57) | (value << 3));
		}
	}

	public NtpMode Mode
	{
		get
		{
			return (NtpMode)(Bytes[0] & 7);
		}
		set
		{
			Bytes[0] = (byte)((uint)(Bytes[0] & -8) | (uint)value);
		}
	}

	public int Stratum => Bytes[1];

	public int Poll => Bytes[2];

	public int Precision => (sbyte)Bytes[3];

	public TimeSpan RootDelay => method_5(4);

	public TimeSpan RootDispersion => method_5(8);

	public uint ReferenceId => method_9(12);

	public DateTime? ReferenceTimestamp => method_3(16);

	public DateTime? OriginTimestamp => method_3(24);

	public DateTime? ReceiveTimestamp => method_3(32);

	public DateTime? TransmitTimestamp
	{
		get
		{
			return method_3(40);
		}
		set
		{
			method_4(40, value);
		}
	}

	public DateTime? DestinationTimestamp
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
		[CompilerGenerated]
		set
		{
			Nullable_0 = value;
		}
	}

	public TimeSpan RoundTripTime
	{
		get
		{
			method_2();
			return ReceiveTimestamp.Value - OriginTimestamp.Value + (DestinationTimestamp.Value - TransmitTimestamp.Value);
		}
	}

	public TimeSpan CorrectionOffset
	{
		get
		{
			method_2();
			return TimeSpan.FromTicks((ReceiveTimestamp.Value - OriginTimestamp.Value - (DestinationTimestamp.Value - TransmitTimestamp.Value)).Ticks / 2L);
		}
	}

	public GClass1487()
		: this(new byte[48])
	{
		Mode = NtpMode.Client;
		VersionNumber = 4;
		TransmitTimestamp = DateTime.UtcNow;
	}

	public GClass1487(byte[] bytes)
	{
		if (bytes.Length < 48)
		{
			throw new ArgumentException("SNTP reply packet must be at least 48 bytes long.", "bytes");
		}
		Bytes = bytes;
	}

	public static GClass1487 FromServerResponse(byte[] bytes, DateTime destinationTimestamp)
	{
		return new GClass1487(bytes)
		{
			DestinationTimestamp = destinationTimestamp
		};
	}

	public void method_0()
	{
		if (Mode != NtpMode.Client)
		{
			throw new InvalidOperationException("This is not a request SNTP packet.");
		}
		if (VersionNumber == 0)
		{
			throw new InvalidOperationException("Protocol version of the request is not specified.");
		}
		if (!TransmitTimestamp.HasValue)
		{
			throw new InvalidOperationException("TransmitTimestamp must be set in request packet.");
		}
	}

	public void method_1()
	{
		if (Mode != NtpMode.Server)
		{
			throw new InvalidOperationException("This is not a reply SNTP packet.");
		}
		if (VersionNumber == 0)
		{
			throw new InvalidOperationException("Protocol version of the reply is not specified.");
		}
		if (Stratum == 0)
		{
			throw new InvalidOperationException($"Received Kiss-o'-Death SNTP packet with code 0x{ReferenceId:x}.");
		}
		if (LeapIndicator == NtpLeapIndicator.AlarmCondition)
		{
			throw new InvalidOperationException("SNTP server has unsynchronized clock.");
		}
		method_2();
	}

	public void method_2()
	{
		if (!OriginTimestamp.HasValue)
		{
			throw new InvalidOperationException("Origin timestamp is missing.");
		}
		if (!ReceiveTimestamp.HasValue)
		{
			throw new InvalidOperationException("Receive timestamp is missing.");
		}
		if (!TransmitTimestamp.HasValue)
		{
			throw new InvalidOperationException("Transmit timestamp is missing.");
		}
		if (!DestinationTimestamp.HasValue)
		{
			throw new InvalidOperationException("Destination timestamp is missing.");
		}
	}

	public DateTime? method_3(int offset)
	{
		ulong num = method_6(offset);
		if (num == 0L)
		{
			return null;
		}
		DateTime dateTime_ = DateTime_0;
		return new DateTime(dateTime_.Ticks + Convert.ToInt64((double)num * 0.0023283064365386963));
	}

	public void method_4(int offset, DateTime? value)
	{
		long value2;
		if (value.HasValue)
		{
			long ticks = value.Value.Ticks;
			DateTime dateTime_ = DateTime_0;
			value2 = (long)Convert.ToUInt64((double)(ticks - dateTime_.Ticks) * 429.4967296);
		}
		else
		{
			value2 = 0L;
		}
		method_7(offset, (ulong)value2);
	}

	public TimeSpan method_5(int offset)
	{
		return TimeSpan.FromSeconds((double)method_8(offset) / 65536.0);
	}

	public ulong method_6(int offset)
	{
		return smethod_1(BitConverter.ToUInt64(Bytes, offset));
	}

	public void method_7(int offset, ulong value)
	{
		GClass1481.GetBytes(Bytes, offset, smethod_1(value));
	}

	public int method_8(int offset)
	{
		return (int)method_9(offset);
	}

	public uint method_9(int offset)
	{
		return smethod_0(BitConverter.ToUInt32(Bytes, offset));
	}

	public static uint smethod_0(uint x)
	{
		return ((x & 0xFF) << 24) | ((x & 0xFF00) << 8) | ((x & 0xFF0000) >> 8) | ((x & 0xFF000000u) >> 24);
	}

	public static ulong smethod_1(ulong x)
	{
		return ((ulong)smethod_0((uint)x) << 32) | smethod_0((uint)(x >> 32));
	}
}
