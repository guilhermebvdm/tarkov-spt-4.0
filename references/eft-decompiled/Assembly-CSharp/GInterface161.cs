using System.Collections.Generic;
using Diz.DependencyManager;

public interface GInterface161
{
	global::BindableStateClass<ELoadState> LoadState { get; }

	float Progress { get; }

	string Key { get; }

	IEnumerable<string> DependencyKeys { get; }

	void Load();

	void Unload();
}
