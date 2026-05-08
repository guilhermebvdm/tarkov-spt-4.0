using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ComponentAce.Compression.Libs.zlib;

public class Class312
{
	public static readonly Dictionary<ETransportProtocolType, string> TransportPrefixes = new Dictionary<ETransportProtocolType, string>
	{
		{
			ETransportProtocolType.HTTPS,
			"https://"
		},
		{
			ETransportProtocolType.WSS,
			"wss://"
		}
	};

	public string BackendName;

	public string BackendMethod;

	public string[] FallbackBackendNames;

	public bool ShouldUseCache;

	public string OverrideCacheId;

	public object DefaultObjectInCaseOfCache;

	public object Params;

	public byte? Retries;

	public int TimeoutSeconds;

	public ERequestHandlingMode HandlingMode;

	public bool RunInMainThread;

	[NonSerialized]
	public GClass648 Gclass648_0;

	[NonSerialized]
	public GInterface17 Ginterface17_0;

	[NonSerialized]
	public byte[] Byte_0;

	[NonSerialized]
	public string String_0 = "";

	[NonSerialized]
	public byte Byte_1;

	[NonSerialized]
	public byte Byte_2;

	[NonSerialized]
	public byte Byte_3;

	[NonSerialized]
	[CompilerGenerated]
	public long Long_0;

	[NonSerialized]
	[CompilerGenerated]
	public ETransportProtocolType EtransportProtocolType_0;

	public long RequestId
	{
		[CompilerGenerated]
		get
		{
			return Long_0;
		}
		[CompilerGenerated]
		set
		{
			Long_0 = value;
		}
	}

	public ETransportProtocolType ProtocolType
	{
		[CompilerGenerated]
		get
		{
			return EtransportProtocolType_0;
		}
		[CompilerGenerated]
		set
		{
			EtransportProtocolType_0 = value;
		}
	}

	public uint? Crc => Gclass648_0?.crc;

	public GClass648 CachedResponse => Gclass648_0;

	public bool CacheValidationConfigured => Gclass648_0 != null;

	public byte[] Data => Byte_0;

	public string ParamsJsonText => String_0;

	public string MainURLNamePath => BackendName + BackendMethod;

	public string MainURLFull => TransportPrefixes[ProtocolType] + MainURLNamePath;

	public string CurSelectedURLNamePath
	{
		get
		{
			if (Byte_3 == 0)
			{
				return MainURLNamePath;
			}
			return ((Byte_1 == 0) ? BackendName : FallbackBackendNames[Byte_1 - 1]) + BackendMethod + $"?retry={Byte_3}";
		}
	}

	public string CurSelectedURLFull => TransportPrefixes[ProtocolType] + CurSelectedURLNamePath;

	public byte CurrentRetryCycle => Byte_2;

	public string CacheID
	{
		get
		{
			if (!ShouldUseCache)
			{
				return null;
			}
			return OverrideCacheId ?? BackendMethod;
		}
	}

	public static Class312 CreateFromLegacyParams(LegacyParamsStruct legacyParams)
	{
		string[] array = legacyParams.Url.Replace("https://", "").Split(new char[1] { '/' }, 2);
		string backendName = array[0];
		string backendMethod = "/" + array[1];
		return new Class312
		{
			BackendName = backendName,
			BackendMethod = backendMethod,
			FallbackBackendNames = null,
			ShouldUseCache = legacyParams.UseCache,
			OverrideCacheId = legacyParams.CacheId,
			DefaultObjectInCaseOfCache = legacyParams.DefaultObjectInCaseOfCache,
			Params = legacyParams.Params,
			Retries = legacyParams.Retries,
			TimeoutSeconds = ((legacyParams.TimeoutSeconds > 0) ? legacyParams.TimeoutSeconds : LegacyParamsStruct.DefaultTimeoutSeconds),
			HandlingMode = legacyParams.HandlingMode,
			RunInMainThread = legacyParams.RunInMainThread
		};
	}

	public void SetupIdAndProtocol(long id, ETransportProtocolType protocol)
	{
		RequestId = id;
		ProtocolType = protocol;
	}

	public void SetupCacheValidation(Class310 cache)
	{
		if (ShouldUseCache)
		{
			Ginterface17_0 = cache;
			GClass648 gClass = Class311.smethod_0(cache, CacheID);
			if (gClass != null)
			{
				Gclass648_0 = gClass;
			}
		}
	}

	public void SetupDataBlock(bool zipped)
	{
		String_0 = ((Params != null) ? JsonParserClass.ToJson(Params) : "");
		Byte_0 = (zipped ? SimpleZlib.CompressToBytes(String_0, 9) : Encoding.UTF8.GetBytes(String_0));
	}

	public void SaveResponseToCache(string responseJsonData)
	{
		if (!ShouldUseCache)
		{
			throw new Exception("BackendRequestData: request ShouldUseCache is false, but you try to cache response for some reason!");
		}
		Ginterface17_0.Save(CacheID, responseJsonData);
	}

	public void IncreaseRetryIndex()
	{
		Byte_3++;
		if (FallbackBackendNames != null && FallbackBackendNames.Length != 0)
		{
			Byte_1++;
			int num = 1 + FallbackBackendNames.Length;
			if (Byte_1 >= num)
			{
				Byte_1 = 0;
				Byte_2++;
			}
		}
		else
		{
			Byte_2++;
		}
	}
}
