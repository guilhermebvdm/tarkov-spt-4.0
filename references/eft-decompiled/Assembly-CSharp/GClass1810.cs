using System;
using System.Collections.Generic;
using EFT;

public abstract class GClass1810
{
	[NonSerialized]
	public const string String_0 = "http";

	[NonSerialized]
	public static HashSet<int> HashSet_0;

	static GClass1810()
	{
		HashSet_0 = new HashSet<int>();
		string[] names = Enum.GetNames(typeof(EBackendErrorCode));
		foreach (string text in names)
		{
			if (text.ToLower().StartsWith("http") && Enum.TryParse<EBackendErrorCode>(text, out var result))
			{
				HashSet_0.Add(GetIntCode(result));
			}
		}
	}

	public static void RemoveCodeFromImportantErrorsList(EBackendErrorCode code)
	{
		HashSet_0.Remove((int)code);
	}

	public static void AddCodeToImportantErrorsList(EBackendErrorCode code)
	{
		HashSet_0.Add((int)code);
	}

	public static bool IsHTTPError(int errorCode)
	{
		return HashSet_0.Contains(errorCode);
	}

	public static bool IsUnknownError(int errorCode)
	{
		if (!IsHTTPError(errorCode))
		{
			return EqualsToInt(EBackendErrorCode.UnknownNginxError, errorCode);
		}
		return true;
	}

	public static int GetIntCode(this EBackendErrorCode code)
	{
		return (int)code;
	}

	public static bool EqualsToInt(this EBackendErrorCode code, int errorCode)
	{
		return errorCode == GetIntCode(code);
	}

	public static int GetIntCode(string errorCode)
	{
		if (Enum.TryParse<EBackendErrorCode>(errorCode, out var result))
		{
			return GetIntCode(result);
		}
		if (int.TryParse(errorCode, out var result2))
		{
			return result2;
		}
		return GetIntCode(EBackendErrorCode.UnknownError);
	}

	public static bool TryParse(string errorCode, out EBackendErrorCode error)
	{
		if (Enum.TryParse<EBackendErrorCode>(errorCode, out error) && Enum.IsDefined(typeof(EBackendErrorCode), error))
		{
			return true;
		}
		error = EBackendErrorCode.UnknownError;
		return false;
	}
}
