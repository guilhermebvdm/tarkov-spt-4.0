using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using EFT;
using NLog;
using UnityEngine;

public class GClass542
{
	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public EEnemyPartVisibleType EenemyPartVisibleType_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public ELookObstacleType ElookObstacleType_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public byte Byte_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	public RaycastHit BotToTargetHit;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public SphereCollider SphereCollider_0;

	[NonSerialized]
	public float Float_6;

	public bool Visible
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public EEnemyPartVisibleType VisibleType
	{
		[CompilerGenerated]
		get
		{
			return EenemyPartVisibleType_0;
		}
		[CompilerGenerated]
		set
		{
			EenemyPartVisibleType_0 = value;
		}
	}

	public bool HasLineOfSight
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public ELookObstacleType BotToTargetObstacleType
	{
		[CompilerGenerated]
		get
		{
			return ElookObstacleType_0;
		}
		[CompilerGenerated]
		set
		{
			ElookObstacleType_0 = value;
		}
	}

	public float VisibilityDuration
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public float ObstaclePenetrationDepth
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public byte ObstacleType
	{
		[CompilerGenerated]
		get
		{
			return Byte_0;
		}
		[CompilerGenerated]
		set
		{
			Byte_0 = value;
		}
	}

	public float VisibilityLevel
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public void CalculatePartVisibility(BotOwner owner, EnemyPart enemyPart, LayerMask lookSensorMask, bool onSense, bool onSenceGreen, float addSensorDistance, float visibilityChangeSpeedK, float deltaTime)
	{
		method_4(owner, enemyPart, lookSensorMask, addSensorDistance, ref visibilityChangeSpeedK);
		Float_6 = visibilityChangeSpeedK;
		bool onSenceGreen2 = onSenceGreen || (Bool_2 && Bool_4);
		method_0(owner, onSense, onSenceGreen2, deltaTime, visibilityChangeSpeedK);
	}

	public bool IsFullDisappearForShoot(BotOwner owner)
	{
		return Time.time - Float_3 > owner.Settings.FileSettings.Look.GOAL_TO_FULL_DISSAPEAR_SHOOT;
	}

	public void method_0(BotOwner owner, bool onSence, bool onSenceGreen, float deltaTime, float visibilityChangeSpeedK = 1f)
	{
		VisibilityDuration = method_2(owner, deltaTime, onSence || onSenceGreen);
		VisibilityLevel = method_1(owner, visibilityChangeSpeedK, VisibilityDuration, deltaTime);
		if (Visible)
		{
			Float_3 = Time.time;
			if (VisibilityLevel <= 0f)
			{
				Visible = false;
			}
		}
		else if (VisibilityLevel >= 1f)
		{
			Visible = true;
		}
		VisibleType = method_3(onSence, onSenceGreen);
	}

	public float method_1(BotOwner owner, float visibilityChangeSpeedK, float visibleTime, float deltaTime)
	{
		float num = (Float_4 = ((visibleTime > 0f) ? (deltaTime * visibilityChangeSpeedK * owner.Settings.FileSettings.Look.VISIBILITY_CHANGE_SPEED) : ((0f - deltaTime) * owner.Settings.FileSettings.Look.VISIBILITY_CHANGE_DECREASE_SPEED)));
		return Mathf.Clamp01(VisibilityLevel + num);
	}

	public float method_2(BotOwner owner, float deltaTime, bool isOnSenceAny)
	{
		if (HasLineOfSight)
		{
			return VisibilityDuration + deltaTime;
		}
		if (isOnSenceAny)
		{
			return VisibilityDuration + deltaTime;
		}
		if (VisibilityDuration > 0f && VisibilityDuration < owner.Settings.FileSettings.Look.POSIBLE_VISION_SPACE + Mathf.Epsilon)
		{
			return VisibilityDuration + deltaTime;
		}
		return 0f;
	}

	public EEnemyPartVisibleType method_3(bool onSence, bool onSenceGreen)
	{
		if (HasLineOfSight)
		{
			if (!Visible)
			{
				return VisibleType;
			}
			return EEnemyPartVisibleType.Visible;
		}
		if (onSence)
		{
			if (!Visible)
			{
				return EEnemyPartVisibleType.NotVisible;
			}
			return EEnemyPartVisibleType.Sence;
		}
		if (onSenceGreen)
		{
			if (!Visible)
			{
				return EEnemyPartVisibleType.NotVisible;
			}
			return EEnemyPartVisibleType.GreenSence;
		}
		if (!Visible)
		{
			return EEnemyPartVisibleType.NotVisible;
		}
		return EEnemyPartVisibleType.Sence;
	}

	public void method_4(BotOwner owner, EnemyPart part, LayerMask lookSensorMask, float addSensorDistance, ref float visibilityChangeSpeedK)
	{
		LookSensor lookSensor = owner.LookSensor;
		if (!lookSensor.IsPointInVisibleSector(part.Position))
		{
			BotToTargetHit = default(RaycastHit);
			HasLineOfSight = false;
			return;
		}
		Vector3 partRaycastTarget = part.GetPartRaycastTarget(HasLineOfSight, lookSensor.HeadPoint);
		Vector3 castDirection = partRaycastTarget - lookSensor.HeadPoint;
		float magnitude = castDirection.magnitude;
		if (lookSensor.VisibleDist + addSensorDistance < magnitude)
		{
			BotToTargetHit = default(RaycastHit);
			HasLineOfSight = false;
		}
		else
		{
			method_5(owner, lookSensorMask, lookSensor.HeadPoint, partRaycastTarget);
			method_6(owner, part.EnemyPlayer, lookSensor.HeadPoint, partRaycastTarget, castDirection, magnitude, ref visibilityChangeSpeedK);
		}
	}

	public void method_5(BotOwner botOwner, LayerMask lookSensorMask, Vector3 sensorOrigin, Vector3 partRaycastTarget)
	{
		HasLineOfSight = !Physics.Linecast(sensorOrigin, partRaycastTarget, out BotToTargetHit, lookSensorMask);
		bool flag = !HasLineOfSight && GClass1458.Contains(LayerMaskClass.AI, BotToTargetHit.collider.gameObject.layer);
		Vector3 to = (HasLineOfSight ? partRaycastTarget : BotToTargetHit.point);
		SphereCollider hitCollider;
		Vector3 intersectionPoint;
		int grenadesAlongWayCount;
		bool flag2 = botOwner.BotsController.BotSmokesVisionSystem.IsRayIntersectAnySmoke(sensorOrigin, to, out hitCollider, out intersectionPoint, out grenadesAlongWayCount);
		Int_0 = grenadesAlongWayCount;
		SphereCollider_0 = (flag2 ? hitCollider : null);
		if (flag2 && flag)
		{
			if (GClass856.SqrDistance(sensorOrigin, intersectionPoint) < BotToTargetHit.distance * BotToTargetHit.distance)
			{
				BotToTargetObstacleType = smethod_0(botOwner.BotsController.AILayerLookObstaclesCache, hitCollider);
			}
			else
			{
				BotToTargetObstacleType = smethod_0(botOwner.BotsController.AILayerLookObstaclesCache, BotToTargetHit.collider);
			}
		}
		else if (flag2)
		{
			BotToTargetObstacleType = smethod_0(botOwner.BotsController.AILayerLookObstaclesCache, hitCollider);
		}
		else if (flag)
		{
			BotToTargetObstacleType = smethod_0(botOwner.BotsController.AILayerLookObstaclesCache, BotToTargetHit.collider);
		}
		else if (HasLineOfSight)
		{
			BotToTargetObstacleType = ((botOwner.AIData.FoliageIntersectionPercent >= 0.95f) ? ELookObstacleType.Tree : ELookObstacleType.None);
		}
		else
		{
			BotToTargetObstacleType = ELookObstacleType.None;
		}
	}

	public static ELookObstacleType smethod_0(Dictionary<GameObject, ELookObstacleType> lookObstaclesCache, Collider hittedCollider)
	{
		if (hittedCollider.CompareTag("SmokeGrenade"))
		{
			return ELookObstacleType.Smoke;
		}
		GameObject gameObject = hittedCollider.gameObject;
		if (lookObstaclesCache.TryGetValue(gameObject, out var value))
		{
			return value;
		}
		value = (GClass1458.Contains(LayerMaskClass.Grass, gameObject.layer) ? ELookObstacleType.Grass : (GClass1458.Contains(LayerMaskClass.Foliage, gameObject.layer) ? ELookObstacleType.Tree : ELookObstacleType.None));
		lookObstaclesCache.Add(gameObject, value);
		return value;
	}

	public void method_6(BotOwner botOwner, IPlayer enemyPlayer, Vector3 sensorOrigin, Vector3 raycastTargetPosition, Vector3 castDirection, float fullCastDistance, ref float visibilityChangeSpeedK)
	{
		if (BotToTargetObstacleType == ELookObstacleType.None)
		{
			ObstaclePenetrationDepth = 0f;
			ObstacleType = 0;
			Float_5 = 1f;
			Bool_2 = false;
			return;
		}
		ELookObstacleType botToTargetObstacleType = BotToTargetObstacleType;
		Bool_2 = botToTargetObstacleType == ELookObstacleType.Grass || botToTargetObstacleType == ELookObstacleType.Tree || botToTargetObstacleType == ELookObstacleType.Smoke;
		float a = 0f;
		bool bool_ = false;
		switch (BotToTargetObstacleType)
		{
		case ELookObstacleType.Tree:
			if (BotToTargetHit.collider is SphereCollider { bounds: var bounds } sphereCollider)
			{
				Vector3 center2 = bounds.center;
				float radius = sphereCollider.radius * sphereCollider.transform.lossyScale.x;
				a = smethod_1(center2, radius, sensorOrigin, raycastTargetPosition, castDirection, fullCastDistance);
			}
			else
			{
				a = 1f;
			}
			break;
		case ELookObstacleType.Grass:
			a = 0.6f;
			break;
		case ELookObstacleType.Smoke:
		{
			Vector3 center = SphereCollider_0.bounds.center;
			float num = SphereCollider_0.radius * SphereCollider_0.transform.lossyScale.x;
			a = smethod_1(center, num, sensorOrigin, raycastTargetPosition, castDirection, fullCastDistance);
			bool_ = (raycastTargetPosition - center).sqrMagnitude <= num * num;
			break;
		}
		}
		a = Mathf.Min(a, fullCastDistance);
		if (a < 0f)
		{
			a = 0f;
		}
		bool enemyFireFlareActive = false;
		if (enemyPlayer.AIData != null)
		{
			enemyFireFlareActive = enemyPlayer.AIData.GetFlare;
		}
		Float_5 = method_7(botOwner, enemyFireFlareActive, a, out Bool_4);
		visibilityChangeSpeedK *= Float_5;
		ObstaclePenetrationDepth = a;
		ObstacleType = BotToTargetObstacleType switch
		{
			ELookObstacleType.None => 0, 
			ELookObstacleType.Tree => 2, 
			ELookObstacleType.Grass => 1, 
			ELookObstacleType.Smoke => 3, 
			_ => ObstacleType, 
		};
		if (Bool_4 && BotToTargetObstacleType == ELookObstacleType.Smoke)
		{
			Bool_4 = bool_;
		}
		if (Bool_4 && BotToTargetObstacleType == ELookObstacleType.Tree && enemyPlayer.AIData != null)
		{
			Transform transform = ((BotToTargetHit.collider != null) ? BotToTargetHit.collider.transform.root : null);
			float foliageIntersectionPercent = enemyPlayer.AIData.FoliageIntersectionPercent;
			Transform currentFoliageRoot = enemyPlayer.AIData.CurrentFoliageRoot;
			bool flag = transform != null && currentFoliageRoot != null && transform == currentFoliageRoot;
			Bool_4 = flag && foliageIntersectionPercent > 0f;
		}
	}

	public static float smethod_1(Vector3 center, float radius, Vector3 segmentStart, Vector3 segmentEnd, Vector3 castDirection, float castLength)
	{
		if (castLength < Mathf.Epsilon)
		{
			return 0f;
		}
		float num = radius * radius;
		Vector3 lhs = segmentStart - center;
		Vector3 vector = segmentEnd - center;
		float sqrMagnitude = lhs.sqrMagnitude;
		float sqrMagnitude2 = vector.sqrMagnitude;
		bool flag = sqrMagnitude < num;
		bool flag2 = sqrMagnitude2 < num;
		if (flag && flag2)
		{
			return castLength;
		}
		Vector3 rhs = castDirection / castLength;
		float num2 = 2f * Vector3.Dot(lhs, rhs);
		float num3 = sqrMagnitude - num;
		float num4 = num2 * num2 - 4f * num3;
		if (flag)
		{
			float num5 = Mathf.Sqrt(num4);
			float num6 = (0f - num2 + num5) * 0.5f;
			if (!(num6 > castLength))
			{
				return num6;
			}
			return castLength;
		}
		if (flag2)
		{
			if (num3 > 0f && num4 < 0f)
			{
				return 0f;
			}
			float num7 = Mathf.Sqrt(num4);
			float num8 = (0f - num2 - num7) * 0.5f;
			float num9 = ((num8 < 0f) ? 0f : num8);
			return castLength - num9;
		}
		if (num4 < 0f)
		{
			return 0f;
		}
		float num10 = Mathf.Sqrt(num4);
		float num11 = (0f - num2 - num10) * 0.5f;
		float num12 = (0f - num2 + num10) * 0.5f;
		float num13 = ((num11 < 0f) ? 0f : ((num11 > castLength) ? castLength : num11));
		float num14 = ((num12 < 0f) ? 0f : ((num12 > castLength) ? castLength : num12));
		if (num14 < num13)
		{
			return 0f;
		}
		return num14 - num13;
	}

	public float method_7(BotOwner botOwner, bool enemyFireFlareActive, float dist, out bool greenTransparent)
	{
		float num = (enemyFireFlareActive ? botOwner.Settings.FileSettings.Look.MAX_VISION_GRASS_METERS_FLARE_OPT : botOwner.Settings.FileSettings.Look.MAX_VISION_GRASS_METERS_OPT);
		float num2 = 1f - num * dist;
		float num3 = 1f + dist * 1.5f;
		float num4 = num2 / num3;
		bool flag = true;
		if (num4 <= 0.001f)
		{
			num4 = 0.001f;
			flag = false;
		}
		else if (num4 > 1f)
		{
			num4 = 1f;
		}
		float num5 = 1f / Mathf.Max(num, 0.0001f);
		bool flag2 = dist >= num5 * 0.1f;
		greenTransparent = flag && flag2;
		return num4;
	}

	public void AppendLogInfo(StringBuilder builder, LogLevel level)
	{
		if (level == LogLevel.Info)
		{
			builder.Append("vis:").Append(Visible.ToString().PadRight(5)).Append("|")
				.Append("K:")
				.Append(Float_6.ToString("F2", CultureInfo.InvariantCulture).PadLeft(5))
				.Append("|")
				.Append("lvl:")
				.Append(VisibilityLevel.ToString("F2", CultureInfo.InvariantCulture).PadLeft(5));
		}
		else if (level == LogLevel.Debug)
		{
			builder.Append("vis:").Append(Visible.ToString().PadRight(5)).Append("|")
				.Append("K:")
				.Append(Float_6.ToString("F2", CultureInfo.InvariantCulture).PadLeft(5))
				.Append("|")
				.Append("type:")
				.Append(GClass856.GetSymbol(VisibleType).PadRight(4))
				.Append("|")
				.Append("lvl:")
				.Append(VisibilityLevel.ToString("F2").PadLeft(5))
				.Append("|")
				.Append("los:")
				.Append(HasLineOfSight.ToString().PadRight(5))
				.Append("|")
				.Append("obsT:")
				.Append(((int)BotToTargetObstacleType).ToString().PadLeft(2))
				.Append("|")
				.Append("grnK:")
				.Append(Float_5.ToString("F1").PadLeft(3))
				.Append("|")
				.Append("grens:")
				.Append(Int_0.ToString().PadLeft(2));
		}
		else if (level == LogLevel.Trace)
		{
			builder.Append("vis:").Append(Visible.ToString().PadRight(5)).Append("|")
				.Append("K:")
				.Append(Float_6.ToString("F2", CultureInfo.InvariantCulture).PadLeft(5))
				.Append("|")
				.Append("type:")
				.Append(GClass856.GetSymbol(VisibleType).PadRight(4))
				.Append("|")
				.Append("lvl:")
				.Append(VisibilityLevel.ToString("F2").PadLeft(5))
				.Append("|")
				.Append("Δ:")
				.Append(Float_4.ToString("F2").PadLeft(5))
				.Append("|")
				.Append("dur:")
				.Append(VisibilityDuration.ToString("F1").PadLeft(4))
				.Append("|")
				.Append("los:")
				.Append(HasLineOfSight.ToString().PadRight(5))
				.Append("|")
				.Append("obsT:")
				.Append(((int)BotToTargetObstacleType).ToString().PadLeft(2))
				.Append("|")
				.Append("green:")
				.Append(Bool_2.ToString().PadRight(5))
				.Append("|")
				.Append("grnK:")
				.Append(Float_5.ToString("F1").PadLeft(3))
				.Append("|")
				.Append("obsPenDep:")
				.Append(ObstaclePenetrationDepth.ToString("F2").PadLeft(4))
				.Append("|")
				.Append("grens:")
				.Append(Int_0.ToString().PadLeft(2));
		}
	}
}
