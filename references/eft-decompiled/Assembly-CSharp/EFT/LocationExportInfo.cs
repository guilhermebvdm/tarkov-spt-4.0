using System;
using UnityEngine;

namespace EFT;

public class LocationExportInfo : MonoBehaviour
{
	[SerializeField]
	public long _unixDateTime;

	public DateTime DateTime
	{
		get
		{
			return EFTDateTimeClass.LocalDateTimeFromUnixTime(_unixDateTime);
		}
		set
		{
			_unixDateTime = (long)EFTDateTimeClass.ToUnixTime(value.ToUniversalTime());
		}
	}
}
