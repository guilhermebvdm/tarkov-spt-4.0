using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public abstract class EFTItemSerializerClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class1200
	{
		public static readonly Class1200 class1200_0 = new Class1200();

		public static Func<string, MongoID> func_0;

		public static Func<string, MongoID> func_1;

		public static Func<KeyValuePair<string, byte>, MongoID> func_2;

		public static Func<KeyValuePair<string, byte>, byte> func_3;

		public static Func<KeyValuePair<EBodyModelPart, MongoID>, int> func_4;

		public static Func<KeyValuePair<EBodyModelPart, MongoID>, MongoID> func_5;

		public static Func<KeyValuePair<int, MongoID>, EBodyModelPart> func_6;

		public static Func<KeyValuePair<int, MongoID>, MongoID> func_7;

		public MongoID method_0(string id)
		{
			return new MongoID(id);
		}

		public MongoID method_1(string id)
		{
			return new MongoID(id);
		}

		public MongoID method_2(KeyValuePair<string, byte> pair)
		{
			return new MongoID(pair.Key);
		}

		public byte method_3(KeyValuePair<string, byte> pair)
		{
			return pair.Value;
		}

		public int method_4(KeyValuePair<EBodyModelPart, MongoID> x)
		{
			return (int)x.Key;
		}

		public MongoID method_5(KeyValuePair<EBodyModelPart, MongoID> x)
		{
			return x.Value;
		}

		public EBodyModelPart method_6(KeyValuePair<int, MongoID> x)
		{
			return (EBodyModelPart)x.Key;
		}

		public MongoID method_7(KeyValuePair<int, MongoID> x)
		{
			return x.Value;
		}
	}

	[CompilerGenerated]
	public class Class1201
	{
		public ISearchController searchController;

		public GClass1945 method_0(LootItemPositionClass item)
		{
			return smethod_3(item, searchController);
		}
	}

	public static GClass1921 SerializeNestedItem([NotNull] Item item, ItemAddress newAddress, ISearchController searchController)
	{
		ItemAddress itemAddress = newAddress ?? item.Parent;
		return new GClass1921
		{
			Item = smethod_0(item, itemAddress, searchController),
			Address = GClass2061.FromItemAddress(itemAddress)
		};
	}

	public static InventoryDescriptorClass smethod_0(Item item, ItemAddress address, ISearchController searchController)
	{
		return SerializeItem(item, searchController);
	}

	[CanBeNull]
	public static InventoryDescriptorClass SerializeItem([CanBeNull] Item item, ISearchController searchController)
	{
		if (item == null)
		{
			return null;
		}
		List<GClass1923> list = null;
		if (item.Components.Count > 0)
		{
			list = new List<GClass1923>(item.Components.Count);
			foreach (IItemComponent component in item.Components)
			{
				GClass1923 gClass = smethod_2(component);
				if (gClass != null)
				{
					list.Add(gClass);
				}
			}
		}
		List<GClass1915> list2 = null;
		List<GClass1919> list3 = null;
		List<GClass1920> list4 = null;
		GClass1924 unsearchedInfo = null;
		int num = 0;
		bool isUnderBarrelDeviceActive = false;
		List<GClass1916> list5 = null;
		if (item is GClass3248 gClass2)
		{
			byte b = 0;
			bool flag = true;
			foreach (EFT.InventoryLogic.IContainer container2 in gClass2.Containers)
			{
				byte b2 = b++;
				if (!container2.Items.Any())
				{
					continue;
				}
				EFT.InventoryLogic.IContainer container = container2;
				Slot slot;
				StashGridClass stashGridClass;
				if (!(container is GInterface215))
				{
					slot = container as Slot;
					if (slot == null)
					{
						stashGridClass = container as StashGridClass;
						if (stashGridClass == null)
						{
							if (!(container is StackSlot stackSlot))
							{
								continue;
							}
							List<InventoryDescriptorClass> list6 = new List<InventoryDescriptorClass>();
							foreach (Item item5 in stackSlot.Items)
							{
								InventoryDescriptorClass item2 = smethod_0(item5, item5.CurrentAddress, searchController);
								list6.Add(item2);
							}
							list4 = GClass853.AddElement(list4, new GClass1920(b2, list6));
							continue;
						}
						goto IL_0191;
					}
				}
				else
				{
					if (!flag)
					{
						continue;
					}
					slot = container as Slot;
					if (slot == null)
					{
						stashGridClass = container as StashGridClass;
						if (stashGridClass == null)
						{
							continue;
						}
						goto IL_0191;
					}
				}
				InventoryDescriptorClass containedItem = smethod_0(slot.ContainedItem, slot.ContainedItem.CurrentAddress, searchController);
				list2 = GClass853.AddElement(list2, new GClass1915(b2, containedItem));
				continue;
				IL_0191:
				List<GClass1918> list7 = new List<GClass1918>(stashGridClass.ItemCollection.Values.Count);
				foreach (KeyValuePair<Item, LocationInGrid> containedItem2 in stashGridClass.ContainedItems)
				{
					containedItem2.Deconstruct(out var key, out var value);
					Item item3 = key;
					LocationInGrid location = value;
					InventoryDescriptorClass item4 = smethod_0(item3, item3.CurrentAddress, searchController);
					list7.Add(new GClass1918(item4, location));
				}
				list3 = GClass853.AddElement(list3, new GClass1919(b2, list7));
			}
		}
		GClass1917 malfunction = null;
		if (item is Weapon weapon)
		{
			MongoID? ammoToFireTemplateId = weapon.MalfState.AmmoToFire?.TemplateId;
			MongoID? ammoWillBeLoadedToChamberTemplateId = weapon.MalfState.AmmoWillBeLoadedToChamber?.TemplateId;
			MongoID? ammoMalfunctionedTemplateId = weapon.MalfState.MalfunctionedAmmo?.TemplateId;
			Dictionary<string, byte> dictionary = new Dictionary<string, byte>();
			isUnderBarrelDeviceActive = weapon.IsUnderBarrelDeviceActive;
			foreach (KeyValuePair<string, Weapon.EMalfunctionSource> playersReducedMalfChance in weapon.MalfState.PlayersReducedMalfChances)
			{
				dictionary.Add(playersReducedMalfChance.Key, (byte)playersReducedMalfChance.Value);
			}
			malfunction = new GClass1917
			{
				Malfunction = (byte)weapon.MalfState.State,
				SlideOnOverheatReached = weapon.MalfState.SlideOnOverheatReached,
				LastShotOverheat = weapon.MalfState.LastShotOverheat,
				LastShotTime = weapon.MalfState.LastShotTime,
				PlayersWhoKnowAboutMalfunction = weapon.MalfState.PlayersWhoKnowAboutMalfunction.Select((string id) => new MongoID(id)).ToList(),
				PlayersWhoKnowMalfType = weapon.MalfState.PlayersWhoKnowMalfType.Select((string id) => new MongoID(id)).ToList(),
				PlayersReducedMalfChances = dictionary.ToDictionary((KeyValuePair<string, byte> pair) => new MongoID(pair.Key), (KeyValuePair<string, byte> pair) => pair.Value),
				AmmoToFireTemplateId = ammoToFireTemplateId,
				AmmoWillBeLoadedToChamberTemplateId = ammoWillBeLoadedToChamberTemplateId,
				AmmoMalfunctionedTemplateId = ammoMalfunctionedTemplateId
			};
			AmmoTemplate[] shellsInChambers = weapon.ShellsInChambers;
			foreach (AmmoTemplate ammoTemplate in shellsInChambers)
			{
				if (ammoTemplate != null)
				{
					list5 = list5 ?? new List<GClass1916>();
					list5.Add(new GClass1916
					{
						AmmoTemplateId = ammoTemplate._id
					});
				}
			}
			if (weapon is RevolverItemClass && weapon.GetCurrentMagazine() is CylinderMagazineItemClass cylinderMagazineItemClass)
			{
				num = cylinderMagazineItemClass.CurrentCamoraIndex;
			}
		}
		return new InventoryDescriptorClass
		{
			Id = item.Id,
			TemplateId = item.TemplateId,
			StackCount = item.StackObjectsCount,
			SpawnedInSession = item.SpawnedInSession,
			Components = list,
			UnsearchedInfo = unsearchedInfo,
			Slots = list2,
			Grids = list3,
			StackSlots = list4,
			ShellsInWeapon = list5,
			Malfunction = malfunction,
			ActiveCamora = (byte)num,
			IsUnderBarrelDeviceActive = isUnderBarrelDeviceActive
		};
	}

	[CanBeNull]
	public static Item DeserializeItem(InventoryDescriptorClass itemDescriptor, ItemFactoryClass itemFactory, Dictionary<MongoID, Item> items)
	{
		try
		{
			return smethod_1(itemFactory, items, itemDescriptor);
		}
		catch (Exception ex)
		{
			MongoID mongoID = itemDescriptor?.Id ?? ((MongoID)"NullDescriptor");
			MongoID mongoID2 = itemDescriptor?.TemplateId ?? ((MongoID)"NullDescriptor");
			Debug.LogError(string.Format("{0} while {1} itemId:{2} templateId:{3}", ex.Message, "DeserializeItem", mongoID, mongoID2));
			throw;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[CanBeNull]
	public static Item smethod_1(ItemFactoryClass itemFactory, Dictionary<MongoID, Item> items, [CanBeNull] InventoryDescriptorClass itemDescriptor)
	{
		if (itemDescriptor == null)
		{
			return null;
		}
		if (itemDescriptor.UnsearchedInfo != null && itemDescriptor.TemplateId == ItemFactoryClass.UNKNOWN_TEMPLATE_ID)
		{
			return new GClass3367(new GClass1802(itemDescriptor.UnsearchedInfo));
		}
		Item item = itemFactory.CreateItem(itemDescriptor.Id, itemDescriptor.TemplateId, null);
		items?.Add(itemDescriptor.Id, item);
		List<GClass1923> components = itemDescriptor.Components;
		if (components != null)
		{
			foreach (GClass1923 item5 in components)
			{
				DeserializeComponent(item5, item);
			}
		}
		item.StackObjectsCount = itemDescriptor.StackCount;
		item.SpawnedInSession = itemDescriptor.SpawnedInSession;
		List<GClass1916> shellsInWeapon = itemDescriptor.ShellsInWeapon;
		if (shellsInWeapon != null && shellsInWeapon.Count > 0 && item is Weapon weapon)
		{
			int count = shellsInWeapon.Count;
			if (weapon.ReloadMode != Weapon.EReloadMode.OnlyBarrel && !(weapon is RevolverItemClass))
			{
				Debug.LogErrorFormat("Attempting to add shells to non-onlybarrel weapon {0}({1})", item.Id, item.Template._name);
			}
			AmmoTemplate[] array = weapon.ShellsInChambers;
			if (array.Length < count)
			{
				Debug.LogWarning("ShellsInWeapon mismatch for " + item.Id + "(" + item.Template.Name + ")! " + $"ShellsInChambers:{array.Length} " + $"DescriptorCount:{count}");
				Array.Resize(ref array, count);
				weapon.ShellsInChambers = array;
			}
			for (int i = 0; i < shellsInWeapon.Count; i++)
			{
				MongoID ammoTemplateId = shellsInWeapon[i].AmmoTemplateId;
				array[i] = (AmmoTemplate)itemFactory.ItemTemplates[ammoTemplateId];
			}
		}
		if (itemDescriptor.Malfunction != null && item is Weapon weapon2)
		{
			weapon2.MalfState.CopyFrom(itemDescriptor.Malfunction, itemFactory);
		}
		List<GClass1915> slots = itemDescriptor.Slots;
		if (slots != null)
		{
			foreach (GClass1915 item6 in slots)
			{
				if (item is Weapon weapon3)
				{
					if (weapon3 is RevolverItemClass && weapon3.GetCurrentMagazine() is CylinderMagazineItemClass cylinderMagazineItemClass)
					{
						cylinderMagazineItemClass.SetCurrentCamoraIndex(itemDescriptor.ActiveCamora);
					}
					weapon3.IsUnderBarrelDeviceActive = itemDescriptor.IsUnderBarrelDeviceActive;
				}
				if (!(((GClass3248)item).Containers.ElementAtOrDefault(item6.SlotNumber) is Slot slot))
				{
					Debug.LogErrorFormat("Binary deserializer error: Could not find slot with number: {0}", item6.SlotNumber);
				}
				else
				{
					Item item2 = DeserializeItem(item6.ContainedItem, itemFactory, items);
					slot.AddWithoutRestrictions(item2);
				}
			}
		}
		List<GClass1919> grids = itemDescriptor.Grids;
		if (grids != null)
		{
			foreach (GClass1919 item7 in grids)
			{
				StashGridClass stashGridClass = ((GClass3248)item).Containers.ElementAtOrDefault(item7.GridNumber) as StashGridClass;
				if (stashGridClass == null)
				{
					Debug.LogError("Binary deserializer error: Could not find grid number: " + item7.GridNumber);
				}
				foreach (GClass1918 containedItem in item7.ContainedItems)
				{
					Item item3 = DeserializeItem(containedItem.Item, itemFactory, items);
					stashGridClass.AddItemWithoutRestrictions(item3, containedItem.Location);
				}
			}
		}
		List<GClass1920> stackSlots = itemDescriptor.StackSlots;
		if (stackSlots != null)
		{
			foreach (GClass1920 item8 in stackSlots)
			{
				StackSlot stackSlot = ((GClass3248)item).Containers.ElementAtOrDefault(item8.SlotNumber) as StackSlot;
				if (stackSlot == null)
				{
					Debug.LogErrorFormat("Binary deserializer error: Could not find stackslot number: {0}", item8.SlotNumber);
				}
				foreach (InventoryDescriptorClass containedItem2 in item8.ContainedItems)
				{
					AmmoItemClass item4 = containedItem2.Deserialize<AmmoItemClass>(items);
					stackSlot.Add(item4, simulate: false);
				}
			}
		}
		if (itemDescriptor.UnsearchedInfo != null)
		{
			((GInterface171)item).SetItemInfo(new GClass1802(itemDescriptor.UnsearchedInfo));
		}
		return item;
	}

	[CanBeNull]
	public static GClass1923 smethod_2(IItemComponent component)
	{
		if (!component.Serialized)
		{
			return null;
		}
		if (component is IReadonlyItemComponent)
		{
			return null;
		}
		if (component is ArmorComponent)
		{
			return null;
		}
		if (component is ArmorHolderComponent)
		{
			return null;
		}
		if (component is EquipmentPenaltyComponent)
		{
			return null;
		}
		if (component is ProtrudableComponent)
		{
			return null;
		}
		if (component is NightVisionComponent)
		{
			return null;
		}
		if (component is ThermalVisionComponent)
		{
			return null;
		}
		if (component is FireModeComponent fireModeComponent)
		{
			return new GClass1937
			{
				FireMode = fireModeComponent.FireMode
			};
		}
		if (component is GridLayoutComponent)
		{
			return null;
		}
		if (component is SlotBlockerComponent)
		{
			return null;
		}
		if (component is HelmetComponent)
		{
			return null;
		}
		if (component is MuzzleComponent)
		{
			return null;
		}
		if (component is BarrelComponent)
		{
			return null;
		}
		if (component is CantPutIntoDuringRaidComponent)
		{
			return null;
		}
		if (component is CantRemoveFromSlotsDuringRaidComponent)
		{
			return null;
		}
		if (component is SilencerComponent)
		{
			return null;
		}
		if (component is KeyComponent keyComponent)
		{
			return new GClass1940
			{
				NumberOfUsages = keyComponent.NumberOfUsages
			};
		}
		if (component is KeycardComponent)
		{
			return null;
		}
		if (component is HealthEffectsComponent)
		{
			return null;
		}
		if (component is KnifeComponent)
		{
			return null;
		}
		if (component is UnlootableComponent)
		{
			return null;
		}
		if (component is FoodDrinkComponent foodDrinkComponent)
		{
			return new GClass1925
			{
				HpPercent = foodDrinkComponent.HpPercent
			};
		}
		if (component is LightComponent lightComponent)
		{
			return new GClass1928
			{
				IsActive = lightComponent.IsActive,
				SelectedMode = lightComponent.SelectedMode
			};
		}
		if (component is LockableComponent lockableComponent)
		{
			return new GClass1929
			{
				Locked = lockableComponent.Locked
			};
		}
		if (component is MapComponent mapComponent)
		{
			return new GClass1930
			{
				Markers = mapComponent.Markers
			};
		}
		if (component is MedKitComponent medKitComponent)
		{
			return new GClass1931
			{
				HpResource = medKitComponent.HpResource
			};
		}
		if (component is SideEffectComponent sideEffectComponent)
		{
			return new GClass1926
			{
				Resource = sideEffectComponent.Value
			};
		}
		if (component is ResourceComponent resourceComponent)
		{
			return new GClass1927
			{
				Resource = resourceComponent.Value
			};
		}
		if (component is StimulatorBuffsComponent)
		{
			return null;
		}
		if (component is RepairableComponent repairableComponent)
		{
			return new GClass1932
			{
				Durability = repairableComponent.Durability,
				MaxDurability = repairableComponent.MaxDurability
			};
		}
		if (component is SightComponent sightComponent)
		{
			GClass1933 gClass = new GClass1933
			{
				SelectedSightScope = sightComponent.SelectedScope,
				ScopesSelectedModes = new int[sightComponent.ScopesSelectedModes.Length],
				ScopesSelectedCalibPoints = new int[sightComponent.ScopesCurrentCalibPointIndexes.Length],
				ScopeZoomValue = sightComponent.ScopeZoomValue
			};
			Array.Copy(sightComponent.ScopesSelectedModes, gClass.ScopesSelectedModes, sightComponent.ScopesSelectedModes.Length);
			Array.Copy(sightComponent.ScopesCurrentCalibPointIndexes, gClass.ScopesSelectedCalibPoints, sightComponent.ScopesCurrentCalibPointIndexes.Length);
			return gClass;
		}
		if (component is TogglableComponent togglableComponent)
		{
			return new GClass1934
			{
				IsOn = togglableComponent.On
			};
		}
		if (component is FaceShieldComponent faceShieldComponent)
		{
			return new GClass1935
			{
				Hits = faceShieldComponent.Hits,
				HitSeed = faceShieldComponent.HitSeed
			};
		}
		if (component is FoldableComponent foldableComponent)
		{
			return new GClass1936
			{
				Folded = foldableComponent.Folded
			};
		}
		if (component is DogtagComponent dogtagComponent)
		{
			return new GClass1938
			{
				AccountId = dogtagComponent.AccountId,
				ProfileId = dogtagComponent.ProfileId,
				Nickname = dogtagComponent.Nickname,
				Side = dogtagComponent.Side,
				Level = dogtagComponent.Level,
				Time = EFTDateTimeClass.ToUnixTime(dogtagComponent.Time),
				Status = dogtagComponent.Status,
				KillerAccountId = dogtagComponent.KillerAccountId,
				KillerProfileId = dogtagComponent.KillerProfileId,
				KillerName = dogtagComponent.KillerName,
				WeaponName = dogtagComponent.WeaponName,
				CarriedByGroupMember = dogtagComponent.CarriedByGroupMember
			};
		}
		if (component is TagComponent tagComponent)
		{
			return new GClass1939
			{
				Name = tagComponent.Name,
				Color = tagComponent.Color
			};
		}
		if (component is AnimationVariantsComponent)
		{
			return null;
		}
		if (component is EyeGuardComponent)
		{
			return null;
		}
		if (component is RepairKitComponent repairKitComponent)
		{
			return new GClass1941
			{
				Resource = repairKitComponent.Resource
			};
		}
		if (component is BuffComponent { BuffType: var buffType, Rarity: var rarity } buffComponent)
		{
			return new GClass1942
			{
				BuffType = (GClass866<ERepairBuffType>.IsDefined(buffType) ? new ERepairBuffType?(buffType) : ((ERepairBuffType?)null)),
				BuffRarity = (GClass866<EBuffRarity>.IsDefined(rarity) ? new EBuffRarity?(rarity) : ((EBuffRarity?)null)),
				Value = (float)buffComponent.Value,
				ThresholdDurability = (float)buffComponent.ThresholdDurability
			};
		}
		if (component is RecodableComponent recodableComponent)
		{
			return new GClass1943
			{
				IsEncoded = recodableComponent.IsEncoded
			};
		}
		if (component is CultistAmuletComponent cultistAmuletComponent)
		{
			return new GClass1944
			{
				NumberOfUsages = cultistAmuletComponent.NumberOfUsages
			};
		}
		if (!(component is SecretExitRequirementComponent))
		{
			throw new ArgumentException("Unknown component type " + component.GetType());
		}
		return null;
	}

	public static void DeserializeComponent(GClass1923 descriptor, Item item)
	{
		if (!(descriptor is GClass1926 gClass))
		{
			if (!(descriptor is GClass1925 gClass2))
			{
				if (!(descriptor is GClass1927 gClass3))
				{
					if (!(descriptor is GClass1928 gClass4))
					{
						if (!(descriptor is GClass1929 gClass5))
						{
							if (!(descriptor is GClass1937 gClass6))
							{
								if (!(descriptor is GClass1930 gClass7))
								{
									if (!(descriptor is GClass1931 gClass8))
									{
										if (!(descriptor is GClass1932 gClass9))
										{
											if (!(descriptor is GClass1933 gClass10))
											{
												if (!(descriptor is GClass1934 gClass11))
												{
													if (!(descriptor is GClass1935 gClass12))
													{
														if (!(descriptor is GClass1936 gClass13))
														{
															if (!(descriptor is GClass1938 gClass14))
															{
																if (!(descriptor is GClass1939 gClass15))
																{
																	if (!(descriptor is GClass1940 gClass16))
																	{
																		if (!(descriptor is GClass1941 gClass17))
																		{
																			if (!(descriptor is GClass1942 gClass18))
																			{
																				if (!(descriptor is GClass1943 gClass19))
																				{
																					if (!(descriptor is GClass1944 gClass20))
																					{
																						throw new ArgumentException("unknown component type " + descriptor.GetType());
																					}
																					item.GetItemComponent<CultistAmuletComponent>().NumberOfUsages = gClass20.NumberOfUsages;
																				}
																				else
																				{
																					item.GetItemComponent<RecodableComponent>().IsEncoded = gClass19.IsEncoded;
																				}
																				return;
																			}
																			BuffComponent itemComponent = item.GetItemComponent<BuffComponent>();
																			if (gClass18.BuffType.HasValue)
																			{
																				itemComponent.BuffType = gClass18.BuffType.Value;
																			}
																			if (gClass18.BuffRarity.HasValue)
																			{
																				itemComponent.Rarity = gClass18.BuffRarity.Value;
																			}
																			itemComponent.Value = gClass18.Value;
																			itemComponent.ThresholdDurability = gClass18.ThresholdDurability;
																		}
																		else
																		{
																			item.GetItemComponent<RepairKitComponent>().Resource = gClass17.Resource;
																		}
																	}
																	else
																	{
																		item.GetItemComponent<KeyComponent>().NumberOfUsages = gClass16.NumberOfUsages;
																	}
																}
																else
																{
																	TagComponent itemComponent2 = item.GetItemComponent<TagComponent>();
																	itemComponent2.Name = gClass15.Name;
																	itemComponent2.Color = gClass15.Color;
																}
															}
															else
															{
																DogtagComponent itemComponent3 = item.GetItemComponent<DogtagComponent>();
																itemComponent3.AccountId = gClass14.AccountId;
																itemComponent3.ProfileId = gClass14.ProfileId;
																itemComponent3.Nickname = gClass14.Nickname;
																itemComponent3.Side = gClass14.Side;
																itemComponent3.Level = gClass14.Level;
																itemComponent3.Time = EFTDateTimeClass.UniversalDateTimeFromUnixTime(gClass14.Time);
																itemComponent3.Status = gClass14.Status;
																itemComponent3.KillerAccountId = gClass14.KillerAccountId;
																itemComponent3.KillerProfileId = gClass14.KillerProfileId;
																itemComponent3.KillerName = gClass14.KillerName;
																itemComponent3.WeaponName = gClass14.WeaponName;
																itemComponent3.CarriedByGroupMember = gClass14.CarriedByGroupMember;
															}
														}
														else
														{
															item.GetItemComponent<FoldableComponent>().Folded = gClass13.Folded;
														}
													}
													else
													{
														FaceShieldComponent itemComponent4 = item.GetItemComponent<FaceShieldComponent>();
														itemComponent4.Hits = gClass12.Hits;
														itemComponent4.HitSeed = gClass12.HitSeed;
													}
												}
												else
												{
													item.GetItemComponent<TogglableComponent>().On = gClass11.IsOn;
												}
											}
											else
											{
												SightComponent itemComponent5 = item.GetItemComponent<SightComponent>();
												int[] scopesSelectedModes = gClass10.ScopesSelectedModes;
												int[] scopesSelectedCalibPoints = gClass10.ScopesSelectedCalibPoints;
												itemComponent5.ScopesSelectedModes = new int[scopesSelectedModes.Length];
												itemComponent5.ScopesCurrentCalibPointIndexes = new int[scopesSelectedCalibPoints.Length];
												Array.Copy(scopesSelectedModes, itemComponent5.ScopesSelectedModes, scopesSelectedModes.Length);
												Array.Copy(scopesSelectedCalibPoints, itemComponent5.ScopesCurrentCalibPointIndexes, scopesSelectedCalibPoints.Length);
												itemComponent5.SelectedScope = gClass10.SelectedSightScope;
												itemComponent5.ScopeZoomValue = gClass10.ScopeZoomValue;
											}
										}
										else
										{
											RepairableComponent itemComponent6 = item.GetItemComponent<RepairableComponent>();
											itemComponent6.Durability = gClass9.Durability;
											itemComponent6.MaxDurability = gClass9.MaxDurability;
										}
									}
									else
									{
										item.GetItemComponent<MedKitComponent>().HpResource = gClass8.HpResource;
									}
								}
								else
								{
									item.GetItemComponent<MapComponent>().Markers = gClass7.Markers;
								}
							}
							else
							{
								item.GetItemComponent<FireModeComponent>().FireMode = gClass6.FireMode;
							}
						}
						else
						{
							item.GetItemComponent<LockableComponent>().Locked = gClass5.Locked;
						}
					}
					else
					{
						LightComponent itemComponent7 = item.GetItemComponent<LightComponent>();
						itemComponent7.IsActive = gClass4.IsActive;
						itemComponent7.SelectedMode = gClass4.SelectedMode;
					}
				}
				else
				{
					item.GetItemComponent<ResourceComponent>().Value = gClass3.Resource;
				}
			}
			else
			{
				item.GetItemComponent<FoodDrinkComponent>().HpPercent = gClass2.HpPercent;
			}
		}
		else
		{
			item.GetItemComponent<SideEffectComponent>().Value = gClass.Resource;
		}
	}

	public static GClass1947 SerializeLootData(IEnumerable<LootItemPositionClass> lootData, ISearchController searchController)
	{
		return new GClass1947
		{
			Items = lootData.Select((LootItemPositionClass item) => smethod_3(item, searchController)).ToList()
		};
	}

	public static GClass1404 DeserializeLootData(GClass1947 lootDataDescriptor)
	{
		return new GClass1404(lootDataDescriptor.Items.Select(smethod_4).ToList());
	}

	public static GClass1945 smethod_3(LootItemPositionClass t, ISearchController searchController)
	{
		if (!(t is GClass1402 gClass))
		{
			return new GClass1945
			{
				Id = t.Id,
				Position = t.Position,
				Rotation = t.Rotation,
				Item = SerializeItem(t.Item, searchController),
				ValidProfiles = t.ValidProfiles,
				IsContainer = t.IsContainer,
				UseGravity = t.useGravity,
				RandomRotation = t.randomRotation,
				Shift = t.Shift,
				PlatformId = t.PlatformId
			};
		}
		return new GClass1946
		{
			Id = gClass.Id,
			Position = gClass.Position,
			Rotation = gClass.Rotation,
			Item = SerializeItem(gClass.Item, searchController),
			IsContainer = gClass.IsContainer,
			ValidProfiles = gClass.ValidProfiles,
			UseGravity = gClass.useGravity,
			RandomRotation = gClass.randomRotation,
			Customization = gClass.Customization.ToDictionary((KeyValuePair<EBodyModelPart, MongoID> x) => (int)x.Key, (KeyValuePair<EBodyModelPart, MongoID> x) => x.Value),
			Side = gClass.Side,
			PlayerProfileID = gClass.ProfileID,
			Bones = ClassTransformSync.FromUnity(gClass.Bones),
			PlatformId = gClass.PlatformId,
			IsZombieCorpse = gClass.IsZombieCorpse
		};
	}

	public static LootItemPositionClass smethod_4(GClass1945 t)
	{
		if (!(t is GClass1946 gClass))
		{
			return new LootItemPositionClass
			{
				Id = t.Id,
				Position = t.Position,
				Rotation = t.Rotation,
				Item = t.Item.Deserialize(),
				ValidProfiles = t.ValidProfiles,
				IsContainer = t.IsContainer,
				useGravity = t.UseGravity,
				randomRotation = t.RandomRotation,
				Shift = t.Shift,
				PlatformId = t.PlatformId
			};
		}
		return new GClass1402
		{
			Id = gClass.Id,
			Position = gClass.Position,
			Rotation = gClass.Rotation,
			Item = gClass.Item.Deserialize(),
			IsContainer = gClass.IsContainer,
			ValidProfiles = gClass.ValidProfiles,
			useGravity = gClass.UseGravity,
			randomRotation = gClass.RandomRotation,
			Customization = new GClass2197(gClass.Customization.ToDictionary((KeyValuePair<int, MongoID> x) => (EBodyModelPart)x.Key, (KeyValuePair<int, MongoID> x) => x.Value)),
			Side = gClass.Side,
			ProfileID = gClass.PlayerProfileID,
			Bones = ClassTransformSync.ToUnity(gClass.Bones),
			PlatformId = gClass.PlatformId,
			IsZombieCorpse = gClass.IsZombieCorpse
		};
	}
}
