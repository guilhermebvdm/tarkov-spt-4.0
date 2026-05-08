using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Diz.Utils;

public abstract class GClass1812
{
	[CompilerGenerated]
	public class Class1126
	{
		public GClass1408 itemTemplates;

		public void method_0()
		{
			itemTemplates.Init();
		}
	}

	[CompilerGenerated]
	public class Class1127
	{
		public GClass1408 itemTemplates;

		public void method_0()
		{
			itemTemplates.Init();
		}
	}

	[NonSerialized]
	public static Task<ItemFactoryClass> Task_0;

	public static async Task<ItemFactoryClass> CreateItemFactory(this GInterface19 backendSession, ItemFactoryClass itemFactory = null)
	{
		GClass1408 itemTemplates = await backendSession.GetTemplates(itemFactory?.ItemTemplates);
		if (itemFactory?.ItemTemplates == itemTemplates)
		{
			return itemFactory;
		}
		await AsyncWorker.RunOnBackgroundThread(delegate
		{
			itemTemplates.Init();
		});
		return new ItemFactoryClass(itemTemplates);
	}

	public static async Task<ItemFactoryClass> CreateItemFactory(this global::IBackendInterface<ISession> backendSession, ItemFactoryClass itemFactory = null)
	{
		GClass1408 itemTemplates = await backendSession.GetTemplates(itemFactory?.ItemTemplates);
		if (itemFactory?.ItemTemplates == itemTemplates)
		{
			return itemFactory;
		}
		await AsyncWorker.RunOnBackgroundThread(delegate
		{
			itemTemplates.Init();
		});
		return new ItemFactoryClass(itemTemplates);
	}

	public static Task<ItemFactoryClass> GetItemFactoryCreationTask(this global::IBackendInterface<ISession> backendSession, ItemFactoryClass itemFactory = null)
	{
		if (Task_0 == null)
		{
			PrepareItemTemplatesForItemFactory(backendSession, itemFactory);
		}
		return Task_0;
	}

	public static void FreeItemFactoryCreationTaskLink(this global::IBackendInterface<ISession> backendSession)
	{
		Task_0 = null;
	}

	public static async Task PrepareItemTemplatesForItemFactory(this global::IBackendInterface<ISession> backendSession, ItemFactoryClass itemFactory = null)
	{
		Task_0 = null;
		Task_0 = CreateItemFactory(backendSession, itemFactory);
	}
}
