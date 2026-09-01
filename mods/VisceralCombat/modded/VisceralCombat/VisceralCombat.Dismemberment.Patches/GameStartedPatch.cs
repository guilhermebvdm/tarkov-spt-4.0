using System;
using System.Reflection;
using Systems.Effects;
using Comfort.Common;
using DeferredDecals;
using EFT;
using Fika.Core.Main.Utils;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Dismemberment.Classes;

namespace VisceralCombat.Dismemberment.Patches;

public class GameStartedPatch : ModulePatch
{
	private static readonly FieldInfo _decalSizeField = typeof(TextureDecalsPainter).GetField("_decalSize", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _bloodDecalTextureField = typeof(TextureDecalsPainter).GetField("_bloodDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _vestDecalTextureField = typeof(TextureDecalsPainter).GetField("_vestDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo _backDecalTextureField = typeof(TextureDecalsPainter).GetField("_backDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);

	protected override MethodBase GetTargetMethod()
	{
		return typeof(GameWorld).GetMethod("OnGameStarted");
	}

	[PatchPostfix]
	private static void Postfix(GameWorld __instance)
	{
		if (FikaBackendUtils.IsHeadless)
		{
			return;
		}

		if (Singleton<Effects>.Instantiated)
		{
			VisceralEntry.Instance.effectContainer = GClass6.GetOrAddComponent<EffectContainer>(((Component)Singleton<Effects>.Instance).gameObject);
			if (VisceralEntry.Instance.EnableBloodEffects.Value)
			{
				TextureDecalsPainter texDecals = Singleton<Effects>.Instance.TexDecals;
				if (texDecals != null)
				{
					Vector2 val = new Vector2(0.25f, 0.35f);
					_decalSizeField?.SetValue(texDecals, val);
					object bloodTex = _bloodDecalTextureField?.GetValue(texDecals);
					if (bloodTex != null)
					{
						_vestDecalTextureField?.SetValue(texDecals, bloodTex);
						_backDecalTextureField?.SetValue(texDecals, bloodTex);
					}
				}

				DeferredDecalRenderer ddr = Singleton<Effects>.Instance.DeferredDecals;
				if (ddr != null)
				{
					ddr.SetMaxStaticDecals(VisceralEntry.Instance.MaxDecals.Value);
					ddr.SetMaxDynamicDecals(VisceralEntry.Instance.MaxDecals.Value);
					GClass855.WaitSeconds((MonoBehaviour)(object)StaticManager.Instance, 2.5f, (Action)delegate
					{
						ddr.Clear();
					});
				}
			}
		}
	}
}
