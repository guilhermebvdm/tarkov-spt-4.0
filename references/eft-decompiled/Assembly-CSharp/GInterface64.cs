using System.Threading.Tasks;
using JetBrains.Annotations;

public interface GInterface64
{
	Task<T> Load<T>([NotNull] T settingsGroup) where T : GClass1081<T>;

	Task Save<T>([NotNull] T settingsGroup) where T : GClass1081<T>;
}
