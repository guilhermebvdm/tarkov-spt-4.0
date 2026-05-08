using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public abstract class GClass2080
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct332
	{
		public GStruct230 playerEmitterCache;
	}

	public const float MIN_CONTUSION_TIME = 2f;

	public static void ApplyLightAndSoundHealthEffects(List<IPlayerOwner> players, Dictionary<IPlayerOwner, GStruct230> playerEmitterCaches, Vector3 emitterPosition, Vector3 blindnessSettings, Vector3 contusionSettings = default(Vector3))
	{
		Struct332 struct332_ = default(Struct332);
		for (int i = 0; i < players.Count; i++)
		{
			IPlayerOwner playerOwner = players[i];
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(playerOwner.iPlayer.ProfileId);
			struct332_.playerEmitterCache = playerEmitterCaches[playerOwner];
			if ((!struct332_.playerEmitterCache.TryToApplyStun && !struct332_.playerEmitterCache.TryToApplyBurnEyes && !struct332_.playerEmitterCache.TryToApplyContusion) || Physics.Linecast(emitterPosition, playerOwner.iPlayer.PlayerBones.Head.position, LayerMaskClass.GrenadeObstaclesColliderMask))
			{
				continue;
			}
			float num = Mathf.Clamp01(Vector3.Dot(struct332_.playerEmitterCache.DirectionToEmitter, playerOwner.iPlayer.LookDirection));
			float num2 = smethod_0(blindnessSettings, ref struct332_);
			float num3 = num2 * num;
			if (alivePlayerByProfileID == null || alivePlayerByProfileID.ActiveHealthController == null)
			{
				continue;
			}
			if (alivePlayerByProfileID.ActiveHealthController.IsAlive && playerOwner.AIData != null && playerOwner.AIData.IsAI)
			{
				BotOwner botOwner = alivePlayerByProfileID.AIData.BotOwner;
				if (botOwner.BotState == EBotState.Active)
				{
					float num4 = blindnessSettings.z * botOwner.Settings.FileSettings.Grenade.FLASH_GRENADE_TIME_COEF;
					float time = num4 * num2;
					botOwner.FlashGrenade.AddSoundEffect(time, emitterPosition);
					if (GClass855.Positive(num3))
					{
						botOwner.FlashGrenade.AddBlindEffect(num4 * num3, emitterPosition);
					}
				}
			}
			if (GClass855.Positive(num3))
			{
				if (struct332_.playerEmitterCache.TryToApplyStun)
				{
					alivePlayerByProfileID.ActiveHealthController.DoStun(blindnessSettings.z, num3);
				}
				if (struct332_.playerEmitterCache.TryToApplyBurnEyes)
				{
					alivePlayerByProfileID.ActiveHealthController.DoBurnEyes(emitterPosition, smethod_0(new Vector2(0f, blindnessSettings.y), ref struct332_), num3, blindnessSettings.z);
				}
			}
			if (struct332_.playerEmitterCache.TryToApplyContusion && contusionSettings != Vector3.zero)
			{
				float num5 = smethod_0(contusionSettings, ref struct332_) * contusionSettings.z;
				if (num5 > 2f)
				{
					alivePlayerByProfileID.ActiveHealthController.DoContusion(num5, 1f);
				}
			}
		}
	}

	[CompilerGenerated]
	public static float smethod_0(Vector2 settings, ref Struct332 struct332_0)
	{
		return Mathf.InverseLerp(settings.y, settings.x, struct332_0.playerEmitterCache.Distance);
	}
}
