using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu]
public class EFTBackendSettings : ScriptableObject
{
	public enum EBackendLabel
	{
		Production,
		Stage,
		Development,
		Custom,
		Unknown
	}

	[Serializable]
	public struct BackendUrl(string url, EBackendLabel label) : IComparable<BackendUrl>
	{
		public string Url = url;

		public EBackendLabel Label = label;

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}", "Url", Url, "Label", Label);
		}

		public bool Equals(BackendUrl other)
		{
			if (string.Equals(Url, other.Url, StringComparison.Ordinal))
			{
				if (Label != EBackendLabel.Unknown && other.Label != EBackendLabel.Unknown)
				{
					return Label == other.Label;
				}
				return true;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is BackendUrl other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((Url != null) ? Url.GetHashCode() : 0) * 397) ^ (int)Label;
		}

		public int CompareTo(BackendUrl other)
		{
			int num = Label.CompareTo(other.Label);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(Url, other.Url, StringComparison.Ordinal);
		}
	}

	private const string string_0 = "EFTBackendURLs";

	private const string string_1 = "EscapeFromTarkov/selectedBackendUrl";

	[SerializeField]
	public BackendUrl[] AvailableUrls = new BackendUrl[1]
	{
		new BackendUrl("https://prod.escapefromtarkov.com", EBackendLabel.Development)
	};

	[CompilerGenerated]
	private static readonly EFTBackendSettings eftbackendSettings_0;

	[CompilerGenerated]
	private string string_2;

	public string String_0 => Application.dataPath + "EscapeFromTarkov/selectedBackendUrl-url";

	public string String_1 => Application.dataPath + "EscapeFromTarkov/selectedBackendUrl-label";

	public static EFTBackendSettings Instance
	{
		[CompilerGenerated]
		get
		{
			return eftbackendSettings_0;
		}
	}

	public string SelectedBackendUrl
	{
		[CompilerGenerated]
		get
		{
			return string_2;
		}
		[CompilerGenerated]
		set
		{
			string_2 = value;
		}
	}

	public void Reset()
	{
		AvailableUrls = new BackendUrl[1]
		{
			new BackendUrl("https://prod.escapefromtarkov.com", EBackendLabel.Development)
		};
		SelectedBackendUrl = AvailableUrls[0].Url;
	}
}
