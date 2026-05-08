using System.Threading.Tasks;
using Comfort.Common;

public abstract class GClass1811
{
	public static async Task LoadCustomization(this IBackEndSession session)
	{
		using (CounterCreatorAbstractClass.StartWithToken("LoadCustomization"))
		{
			await smethod_0(session);
			GClass1851 availableSuites = await session.GetAvailableCustomizationsStorage();
			Singleton<CustomizationSolverClass>.Instance.SetAvailableCustomizations(availableSuites);
		}
	}

	public static async Task smethod_0(GInterface19 session)
	{
		CustomizationSolverClass instance = new CustomizationSolverClass(await session.GetAllCustomizations());
		if (Singleton<CustomizationSolverClass>.Instantiated)
		{
			Singleton<CustomizationSolverClass>.Release(Singleton<CustomizationSolverClass>.Instance);
		}
		Singleton<CustomizationSolverClass>.Create(instance);
	}
}
