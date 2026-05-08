using System;
using System.IO;
using JetBrains.Annotations;

namespace EFT;

[Serializable]
[GAttribute29]
public class ResourceKey
{
	public static readonly ResourceKey EmptyResourceKey = new ResourceKey();

	[GAttribute30]
	public string path;

	[GAttribute30]
	public string rcid;

	public string FileName => Path.GetFileNameWithoutExtension(path);

	[CanBeNull]
	public string ToAssetName()
	{
		if (!string.IsNullOrEmpty(rcid))
		{
			return rcid;
		}
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		string text = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(text))
		{
			text += "/";
		}
		return (text + Path.GetFileNameWithoutExtension(path)).ToLower();
	}

	public bool Equals(ResourceKey other)
	{
		if (string.Equals(path, other.path))
		{
			if (!string.Equals(rcid, other.rcid))
			{
				if (string.IsNullOrEmpty(rcid))
				{
					return string.IsNullOrEmpty(other.rcid);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((ResourceKey)obj);
	}

	public override int GetHashCode()
	{
		return (((!string.IsNullOrEmpty(path)) ? path.GetHashCode() : 0) * 397) ^ ((!string.IsNullOrEmpty(rcid)) ? rcid.GetHashCode() : 0);
	}

	public static bool operator ==(ResourceKey left, ResourceKey right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(ResourceKey left, ResourceKey right)
	{
		return !object.Equals(left, right);
	}

	public override string ToString()
	{
		return $"{GetType()} path='{path}', rcid='{rcid}'";
	}

	public bool IsEmpty()
	{
		return string.IsNullOrEmpty(path);
	}
}
