using System.Collections.Generic;
using System.Threading.Tasks;

public interface GInterface63<T> where T : GClass1081<T>, new()
{
	IReadOnlyList<GClass1080<T>> Presets { get; }

	Task RefreshPresetList();

	Task<GClass1080<T>> CreateNewPreset(string name, T settingsGroup);

	Task RemovePreset(GClass1080<T> preset);
}
