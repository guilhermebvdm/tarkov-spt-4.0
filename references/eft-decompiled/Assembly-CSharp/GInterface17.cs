using System;
using JetBrains.Annotations;

public interface GInterface17 : IDisposable
{
	bool Exists(string url);

	[CanBeNull]
	string Load(string url);

	void Save(string url, string data);
}
