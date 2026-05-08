using System;
using System.Collections.Generic;
using AnimationEventSystem;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass756
{
	public delegate void Delegate1(List<IActorEvents> eventsConsumers, AnimationEventParameter parameter);

	[NonSerialized]
	public static Dictionary<int, Delegate1> Dictionary_0;

	[NonSerialized]
	public static Dictionary<int, string> Dictionary_1;

	static GClass756()
	{
		Dictionary_0 = new Dictionary<int, Delegate1>
		{
			{ -508605542, smethod_1 },
			{ -1583670125, smethod_2 },
			{ 386854108, smethod_3 },
			{ 963907342, smethod_4 },
			{ -945517627, smethod_5 },
			{ 1434365820, smethod_6 },
			{ 1239352288, smethod_7 },
			{ -1677976749, smethod_8 },
			{ -58181185, smethod_9 },
			{ -1051941490, smethod_10 },
			{ 1704018560, smethod_11 },
			{ 198060848, smethod_12 },
			{ 1390148366, smethod_13 },
			{ 148436330, smethod_14 },
			{ 798565163, smethod_15 },
			{ 1258896930, smethod_16 },
			{ 1947938901, smethod_17 },
			{ 1551431816, smethod_18 },
			{ 1724675694, smethod_19 },
			{ 1041383721, smethod_20 },
			{ 1199378086, smethod_21 },
			{ 2091345647, smethod_22 },
			{ -1518700811, smethod_23 },
			{ 2146162989, smethod_24 },
			{ 211630556, smethod_25 },
			{ 443236115, smethod_26 },
			{ -1819682913, smethod_27 },
			{ -1662010579, smethod_28 },
			{ 903577754, smethod_29 },
			{ -1349190221, smethod_30 },
			{ 1554795451, smethod_31 },
			{ -833254918, smethod_32 },
			{ 1134400241, smethod_33 },
			{ -612501071, smethod_34 },
			{ -1376281788, smethod_35 },
			{ 67499002, smethod_36 },
			{ -224219248, smethod_37 },
			{ 1174204865, smethod_38 },
			{ 224242560, smethod_39 },
			{ 1777082373, smethod_40 },
			{ 1051598138, smethod_41 },
			{ 1350263048, smethod_42 },
			{ 1537525950, smethod_43 },
			{ -1429466465, smethod_44 },
			{ 1527071002, smethod_45 },
			{ -378724418, smethod_46 },
			{ 1865652397, smethod_47 },
			{ -1708037274, smethod_48 },
			{ 540737411, smethod_49 },
			{ 410839395, smethod_50 }
		};
		Dictionary_1 = new Dictionary<int, string>
		{
			{ -508605542, "AddAmmoInChamber" },
			{ -1583670125, "AddAmmoInMag" },
			{ 386854108, "Arm" },
			{ 963907342, "Cook" },
			{ -945517627, "DelAmmoChamber" },
			{ 1434365820, "DelAmmoFromMag" },
			{ 1239352288, "Disarm" },
			{ -1677976749, "FireEnd" },
			{ -58181185, "FiringBullet" },
			{ -1051941490, "FoldOff" },
			{ 1704018560, "FoldOn" },
			{ 198060848, "IdleStart" },
			{ 1390148366, "LauncherAppeared" },
			{ 148436330, "LauncherDisappeared" },
			{ 798565163, "MagHide" },
			{ 1258896930, "MagIn" },
			{ 1947938901, "MagOut" },
			{ 1551431816, "MagShow" },
			{ 1724675694, "MessageName" },
			{ 1041383721, "MalfunctionOff" },
			{ 1199378086, "ModChanged" },
			{ 2091345647, "OffBoltCatch" },
			{ -1518700811, "OnBoltCatch" },
			{ 2146162989, "PutMagToRig" },
			{ 211630556, "RemoveShell" },
			{ 443236115, "ReplaceSecondMag" },
			{ -1819682913, "ShellEject" },
			{ -1662010579, "ShowAmmo" },
			{ 903577754, "ShowMag" },
			{ -1349190221, "SliderOut" },
			{ 1554795451, "Sound" },
			{ -833254918, "SoundAtPoint" },
			{ 1134400241, "StartUtilityOperation" },
			{ -612501071, "ThirdAction" },
			{ -1376281788, "UseProp" },
			{ 67499002, "UseSecondMagForReload" },
			{ -224219248, "WeapIn" },
			{ 1174204865, "WeapOut" },
			{ 1051598138, "OnCurrentAnimStateEnded" },
			{ 1350263048, "OnSetActiveObject" },
			{ 1537525950, "OnDeactivateObject" },
			{ -1429466465, "ReloadTest" },
			{ 1527071002, "BipodOpen" },
			{ -378724418, "BipodClose" },
			{ 1865652397, "OutUse" },
			{ -1708037274, "AimReady" },
			{ 540737411, "IdleReady" },
			{ 410839395, "DropWeapon" }
		};
	}

	public static void AnimatorEventHandler(List<IActorEvents> eventsConsumers, int functionNameHash, AnimationEventParameter parameter)
	{
		Delegate1 value = null;
		string value2;
		if (Dictionary_0.TryGetValue(functionNameHash, out value))
		{
			value(eventsConsumers, parameter);
		}
		else if (Dictionary_1.TryGetValue(functionNameHash, out value2))
		{
			Debug.LogErrorFormat("FATAL There is no handler for <b>{0}</b>", value2);
		}
		else
		{
			Debug.LogErrorFormat("FATAL There is no handler for hash:<b>{0}</b>", functionNameHash);
		}
	}

	public static void smethod_0(List<IActorEvents> eventsConsumers, string functionName, AnimationEventParameter parameter)
	{
		AnimatorEventHandler(eventsConsumers, functionName.GetHashCode(), parameter);
	}

	public static void smethod_1([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnAddAmmoInChamber();
		}
	}

	public static void smethod_2([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnAddAmmoInMag();
		}
	}

	public static void smethod_3([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnArm();
		}
	}

	public static void smethod_4([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnCook();
		}
	}

	public static void smethod_5([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnDelAmmoChamber();
		}
	}

	public static void smethod_6([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnDelAmmoFromMag();
		}
	}

	public static void smethod_7([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnDisarm();
		}
	}

	public static void smethod_8([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnFireEnd();
		}
	}

	public static void smethod_9([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnFiringBullet();
		}
	}

	public static void smethod_10([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnFoldOff();
		}
	}

	public static void smethod_11([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnFoldOn();
		}
	}

	public static void smethod_12([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnIdleStart();
		}
	}

	public static void smethod_13([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnLauncherAppeared();
		}
	}

	public static void smethod_14([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnLauncherDisappeared();
		}
	}

	public static void smethod_15([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnMagHide();
		}
	}

	public static void smethod_16([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnMagIn();
		}
	}

	public static void smethod_17([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnMagOut();
		}
	}

	public static void smethod_18([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnMagShow();
		}
	}

	public static void smethod_19([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnMessageName();
		}
	}

	public static void smethod_20([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnMalfunctionOff();
		}
	}

	public static void smethod_21([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnModChanged();
		}
	}

	public static void smethod_22([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnOffBoltCatch();
		}
	}

	public static void smethod_23([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnOnBoltCatch();
		}
	}

	public static void smethod_24([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnPutMagToRig();
		}
	}

	public static void smethod_25([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnRemoveShell();
		}
	}

	public static void smethod_26([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnReplaceSecondMag();
		}
	}

	public static void smethod_27([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnShellEject();
		}
	}

	public static void smethod_28([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnShowAmmo(parameter.BoolParam);
		}
	}

	public static void smethod_29([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnShowMag();
		}
	}

	public static void smethod_30([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnSliderOut();
		}
	}

	public static void smethod_31([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnSound(parameter.StringParam);
		}
	}

	public static void smethod_32([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnSoundAtPoint(parameter.StringParam);
		}
	}

	public static void smethod_33([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnStartUtilityOperation();
		}
	}

	public static void smethod_34([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnThirdAction(parameter.IntParam);
		}
	}

	public static void smethod_35([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnUseProp(parameter.BoolParam);
		}
	}

	public static void smethod_36([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnUseSecondMagForReload();
		}
	}

	public static void smethod_37([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnWeapIn();
		}
	}

	public static void smethod_38([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnWeapOut();
		}
	}

	public static void smethod_39([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnBackpackDrop();
		}
	}

	public static void smethod_40([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnComboPlanning();
		}
	}

	public static void smethod_41([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnCurrentAnimStateEnded();
		}
	}

	public static void smethod_42([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnSetActiveObject(parameter.IntParam);
		}
	}

	public static void smethod_43([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OnDeactivateObject(parameter.IntParam);
		}
	}

	public static void smethod_44([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].ReloadTest();
		}
	}

	public static void smethod_45([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].BipodOpen();
		}
	}

	public static void smethod_46([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].BipodClose();
		}
	}

	public static void smethod_47([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].OutUse();
		}
	}

	public static void smethod_48([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].AimReady();
		}
	}

	public static void smethod_49([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].IdleReady();
		}
	}

	public static void smethod_50([NotNull] List<IActorEvents> eventsConsumers, AnimationEventParameter parameter)
	{
		for (int num = eventsConsumers.Count - 1; num >= 0; num--)
		{
			eventsConsumers[num].DropWeapon();
		}
	}
}
