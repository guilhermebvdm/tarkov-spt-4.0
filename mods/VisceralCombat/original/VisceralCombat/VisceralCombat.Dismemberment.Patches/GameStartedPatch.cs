using System;
using System.Reflection;
using Systems.Effects;
using Comfort.Common;
using DeferredDecals;
using EFT;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Combined.Patches;
using VisceralCombat.Dismemberment.Classes;

namespace VisceralCombat.Dismemberment.Patches;

public class GameStartedPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(GameWorld).GetMethod("OnGameStarted");
	}

	[PatchPostfix]
	private static void Postfix(GameWorld __instance)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (FikaBackendUtils.IsHeadless)
		{
			return;
		}
		VisceralEntry.Instance.effectContainer = GClass6.GetOrAddComponent<EffectContainer>(((Component)Singleton<Effects>.Instance).gameObject);
		if (VisceralEntry.Instance.EnableBloodEffects.Value)
		{
			TextureDecalsPainter texDecals = Singleton<Effects>.Instance.TexDecals;
			FieldInfo field = typeof(TextureDecalsPainter).GetField("_decalSize", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo field2 = typeof(TextureDecalsPainter).GetField("_bloodDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo field3 = typeof(TextureDecalsPainter).GetField("_vestDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo field4 = typeof(TextureDecalsPainter).GetField("_backDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(0.25f, 0.35f);
			field.SetValue(texDecals, val);
			object value = field2.GetValue(texDecals);
			field3.SetValue(texDecals, value);
			field4.SetValue(texDecals, value);
			DeferredDecalRenderer ddr = Singleton<Effects>.Instance.DeferredDecals;
			ddr.SetMaxStaticDecals(VisceralEntry.Instance.MaxDecals.Value);
			ddr.SetMaxDynamicDecals(VisceralEntry.Instance.MaxDecals.Value);
			GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 2.5f, (Action)delegate
			{
				ddr.Clear();
			});
		}
		if (FikaBackendUtils.IsServer)
		{
			((ModulePatch)new KillPatch()).Enable();
			((ModulePatch)new KillClientPatch()).Enable();
		}
		else
		{
			((ModulePatch)new KillPatch()).Enable();
			((ModulePatch)new KillClientPatch()).Enable();
		}
	}
}
