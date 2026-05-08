using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Customization;
using JetBrains.Annotations;

public class CustomizationSolverClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class1136
	{
		public static readonly Class1136 class1136_0 = new Class1136();

		public static Func<GClass1852, bool> func_0;

		public static Func<GClass3679, bool> func_1;

		public static Func<GClass3680, bool> func_2;

		public bool method_0(GClass1852 c)
		{
			ECustomizationType type = c.Type;
			return type == ECustomizationType.Floor || type == ECustomizationType.Wall || type == ECustomizationType.Ceiling || type == ECustomizationType.ShootingRangeMark;
		}

		public bool method_1(GClass3679 item)
		{
			return item != null;
		}

		public bool method_2(GClass3680 pose)
		{
			return pose != null;
		}
	}

	[CompilerGenerated]
	public class Class1137
	{
		public CustomizationSolverClass CustomizationSolverClass;

		public EPlayerSide playerSide;

		public GClass3682 method_0(GClass1852 suite)
		{
			return CustomizationSolverClass.method_0(suite.Id, CustomizationSolverClass.Dictionary_2);
		}

		public bool method_1(GClass3682 suite)
		{
			return suite?.Side.Contains(playerSide) ?? false;
		}
	}

	[CompilerGenerated]
	public class Class1138
	{
		public CustomizationSolverClass CustomizationSolverClass;

		public EPlayerSide playerSide;

		public GClass3674 method_0(GClass1852 dogTag)
		{
			return CustomizationSolverClass.method_0(dogTag.Id, CustomizationSolverClass.Dictionary_4);
		}

		public bool method_1(GClass3674 dogTag)
		{
			return dogTag?.Side.Contains(playerSide) ?? false;
		}
	}

	[CompilerGenerated]
	public class Class1139
	{
		public CustomizationSolverClass CustomizationSolverClass;

		public EPlayerSide playerSide;

		public GClass3675 method_0(GClass1852 gesture)
		{
			return CustomizationSolverClass.method_0(gesture.Id, CustomizationSolverClass.Dictionary_6);
		}

		public bool method_1(GClass3675 gesture)
		{
			return gesture?.Side.Contains(playerSide) ?? false;
		}
	}

	[CompilerGenerated]
	public class Class1140
	{
		public CustomizationSolverClass CustomizationSolverClass;

		public EPlayerSide playerSide;

		public GClass3676 method_0(GClass1852 environmentUI)
		{
			return CustomizationSolverClass.method_0(environmentUI.Id, CustomizationSolverClass.Dictionary_7);
		}

		public bool method_1(GClass3676 environmentUI)
		{
			return environmentUI?.Side.Contains(playerSide) ?? false;
		}
	}

	[CompilerGenerated]
	public class Class1141
	{
		public CustomizationSolverClass CustomizationSolverClass;

		public EPlayerSide playerSide;

		public GClass3678 method_0(GClass1852 head)
		{
			return CustomizationSolverClass.method_0(head.Id, CustomizationSolverClass.Dictionary_5);
		}

		public bool method_1(GClass3678 head)
		{
			return head?.Side.Contains(playerSide) ?? false;
		}
	}

	[CompilerGenerated]
	public class Class1142
	{
		public CustomizationSolverClass CustomizationSolverClass;

		public EPlayerSide playerSide;

		public GClass3681 method_0(GClass1852 voice)
		{
			return CustomizationSolverClass.method_0(voice.Id, CustomizationSolverClass.Dictionary_3);
		}

		public bool method_1(GClass3681 voice)
		{
			return voice?.Side.Contains(playerSide) ?? false;
		}
	}

	[CompilerGenerated]
	public class Class1143
	{
		public MongoID suiteId;

		public bool method_0(GClass1852 customization)
		{
			return customization.Id == suiteId;
		}
	}

	[CompilerGenerated]
	public class Class1144
	{
		public ECustomizationType type;

		public bool method_0(GClass1852 x)
		{
			return x.Type == type;
		}
	}

	[NonSerialized]
	public Dictionary<MongoID, ECustomizationItemCategory> Dictionary_0 = new Dictionary<MongoID, ECustomizationItemCategory>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3677> Dictionary_1 = new Dictionary<MongoID, GClass3677>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3682> Dictionary_2 = new Dictionary<MongoID, GClass3682>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3681> Dictionary_3 = new Dictionary<MongoID, GClass3681>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3674> Dictionary_4 = new Dictionary<MongoID, GClass3674>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3678> Dictionary_5 = new Dictionary<MongoID, GClass3678>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3675> Dictionary_6 = new Dictionary<MongoID, GClass3675>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3676> Dictionary_7 = new Dictionary<MongoID, GClass3676>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3679> Dictionary_8 = new Dictionary<MongoID, GClass3679>();

	[NonSerialized]
	public Dictionary<MongoID, GClass3680> Dictionary_9 = new Dictionary<MongoID, GClass3680>();

	[NonSerialized]
	public HashSet<GClass1852> HashSet_0 = new HashSet<GClass1852>();

	public GClass3677[] ClothingTemplates => Dictionary_1.Values.ToArray();

	public IReadOnlyCollection<GClass1852> AvailableCustomizations => HashSet_0;

	public IEnumerable<GClass3679> HideoutCustomizationItems => Dictionary_8.Values;

	public IEnumerable<GClass3680> HideoutMannequinPoses => Dictionary_9.Values;

	public CustomizationSolverClass(Dictionary<MongoID, GClass3672> customizationTemplates)
	{
		foreach (var (key, gClass2) in customizationTemplates)
		{
			if (!(gClass2 is GClass3678 value))
			{
				if (!(gClass2 is GClass3677 value2))
				{
					if (!(gClass2 is GClass3682 value3))
					{
						if (!(gClass2 is GClass3681 value4))
						{
							if (!(gClass2 is GClass3674 value5))
							{
								if (!(gClass2 is GClass3675 value6))
								{
									if (!(gClass2 is GClass3679 value7))
									{
										if (!(gClass2 is GClass3680 value8))
										{
											if (gClass2 is GClass3676 value9)
											{
												Dictionary_7.Add(key, value9);
											}
										}
										else
										{
											Dictionary_9.Add(key, value8);
										}
									}
									else
									{
										Dictionary_8.Add(key, value7);
									}
								}
								else
								{
									Dictionary_6.Add(key, value6);
								}
							}
							else
							{
								Dictionary_4.Add(key, value5);
							}
						}
						else
						{
							Dictionary_3.Add(key, value4);
						}
					}
					else
					{
						Dictionary_2.Add(key, value3);
					}
				}
				else
				{
					Dictionary_1.Add(key, value2);
				}
			}
			else
			{
				Dictionary_5.Add(key, value);
			}
			Dictionary_0.Add(key, gClass2.Category);
		}
	}

	public IEnumerable<GClass3682> GetAvailableSuites(EPlayerSide playerSide)
	{
		return from suite in method_1(ECustomizationType.Suite)
			select method_0(suite.Id, Dictionary_2) into suite
			where suite?.Side.Contains(playerSide) ?? false
			select suite;
	}

	public IEnumerable<GClass3674> GetAvailableDogTags(EPlayerSide playerSide)
	{
		return from dogTag in method_1(ECustomizationType.DogTag)
			select method_0(dogTag.Id, Dictionary_4) into dogTag
			where dogTag?.Side.Contains(playerSide) ?? false
			select dogTag;
	}

	public IEnumerable<GClass3675> GetAvailableGestures(EPlayerSide playerSide)
	{
		return from gesture in method_1(ECustomizationType.Gesture)
			select method_0(gesture.Id, Dictionary_6) into gesture
			where gesture?.Side.Contains(playerSide) ?? false
			select gesture;
	}

	public IEnumerable<GClass3676> GetAvailableEnvironmentUIs(EPlayerSide playerSide)
	{
		return from environmentUI in method_1(ECustomizationType.Environment)
			select method_0(environmentUI.Id, Dictionary_7) into environmentUI
			where environmentUI?.Side.Contains(playerSide) ?? false
			select environmentUI;
	}

	public IEnumerable<GClass3678> GetAvailableHeads(EPlayerSide playerSide)
	{
		return from head in method_1(ECustomizationType.Head)
			select method_0(head.Id, Dictionary_5) into head
			where head?.Side.Contains(playerSide) ?? false
			select head;
	}

	public IEnumerable<GClass3681> GetAvailableVoices(EPlayerSide playerSide)
	{
		return from voice in method_1(ECustomizationType.Voice)
			select method_0(voice.Id, Dictionary_3) into voice
			where voice?.Side.Contains(playerSide) ?? false
			select voice;
	}

	public IEnumerable<GClass3679> GetAvailableHideoutCustomizationItems()
	{
		return from item in HashSet_0.Where(delegate(GClass1852 c)
			{
				ECustomizationType type = c.Type;
				return type == ECustomizationType.Floor || type == ECustomizationType.Wall || type == ECustomizationType.Ceiling || type == ECustomizationType.ShootingRangeMark;
			})
			select method_0(item.Id, Dictionary_8) into item
			where item != null
			select item;
	}

	public IEnumerable<GClass3680> GetAvailableHideoutMannequinPoses()
	{
		return from pose in method_1(ECustomizationType.MannequinPose)
			select method_0(pose.Id, Dictionary_9) into pose
			where pose != null
			select pose;
	}

	[CanBeNull]
	public ResourceKey GetBundle(MongoID templateId)
	{
		return GetItem(templateId)?.Prefab;
	}

	public GStruct431 GetWatchBundle(MongoID templateId)
	{
		if (!Dictionary_1.TryGetValue(templateId, out var value))
		{
			return default(GStruct431);
		}
		return new GStruct431
		{
			WatchPrefab = value.WatchPrefab,
			WatchPosition = value.WatchPosition,
			WatchRotation = value.WatchRotation
		};
	}

	public bool HasIntegratedArmor(MongoID templateId)
	{
		if (Dictionary_1.TryGetValue(templateId, out var value))
		{
			return value.IntegratedArmorVest;
		}
		return false;
	}

	[CanBeNull]
	public GClass3677 GetItem(MongoID templateId)
	{
		if (Dictionary_1.TryGetValue(templateId, out var value))
		{
			return value;
		}
		if (Dictionary_5.TryGetValue(templateId, out var value2))
		{
			return value2;
		}
		return null;
	}

	[CanBeNull]
	public GClass3682 GetSuite(MongoID suiteId)
	{
		return method_0(suiteId, Dictionary_2);
	}

	public GClass3674 GetDogTag(MongoID dogTagId)
	{
		return method_0(dogTagId, Dictionary_4);
	}

	public T method_0<T>(MongoID id, Dictionary<MongoID, T> dictionary) where T : GClass3672
	{
		dictionary.TryGetValue(id, out var value);
		return value;
	}

	public void SetAvailableCustomizations(IEnumerable<GClass1852> availableSuites, bool overrideExisting = false)
	{
		if (overrideExisting)
		{
			HashSet_0.Clear();
		}
		foreach (GClass1852 item in GClass853.EmptyIfNull(availableSuites))
		{
			HashSet_0.Add(item);
		}
	}

	public bool IsCustomizationAvailable(MongoID suiteId)
	{
		return HashSet_0.Any((GClass1852 customization) => customization.Id == suiteId);
	}

	public IEnumerable<GClass1852> method_1(ECustomizationType type)
	{
		return HashSet_0.Where((GClass1852 x) => x.Type == type);
	}

	public GClass3672 GetAnyCustomizationItem(MongoID id)
	{
		if (Dictionary_1.TryGetValue(id, out var value))
		{
			return value;
		}
		if (Dictionary_3.TryGetValue(id, out var value2))
		{
			return value2;
		}
		if (Dictionary_2.TryGetValue(id, out var value3))
		{
			return value3;
		}
		if (Dictionary_4.TryGetValue(id, out var value4))
		{
			return value4;
		}
		if (Dictionary_8.TryGetValue(id, out var value5))
		{
			return value5;
		}
		if (Dictionary_9.TryGetValue(id, out var value6))
		{
			return value6;
		}
		if (Dictionary_6.TryGetValue(id, out var value7))
		{
			return value7;
		}
		if (Dictionary_5.TryGetValue(id, out var value8))
		{
			return value8;
		}
		if (Dictionary_7.TryGetValue(id, out var value9))
		{
			return value9;
		}
		return null;
	}

	public bool GetCustomizationCategory(MongoID itemId, out ECustomizationItemCategory category)
	{
		return Dictionary_0.TryGetValue(itemId, out category);
	}

	public GClass3681 GetVoice(MongoID voiceId)
	{
		return Dictionary_3.GetValueOrDefault(voiceId);
	}

	[CompilerGenerated]
	public GClass3679 method_2(GClass1852 item)
	{
		return method_0(item.Id, Dictionary_8);
	}

	[CompilerGenerated]
	public GClass3680 method_3(GClass1852 pose)
	{
		return method_0(pose.Id, Dictionary_9);
	}
}
