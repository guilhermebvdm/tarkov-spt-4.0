using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GClass577
{
	public static float tan45 = Mathf.Tan(MathF.PI / 4f);

	[NonSerialized]
	public static Dictionary<AIGreandeAng, GClass576> Dictionary_0 = new Dictionary<AIGreandeAng, GClass576>
	{
		{
			AIGreandeAng.ang25,
			new GClass576(25f)
		},
		{
			AIGreandeAng.ang45,
			new GClass576(45f)
		},
		{
			AIGreandeAng.ang65,
			new GClass576(65f)
		},
		{
			AIGreandeAng.ang5,
			new GClass576(5f)
		},
		{
			AIGreandeAng.ang15,
			new GClass576(15f)
		},
		{
			AIGreandeAng.ang35,
			new GClass576(35f)
		},
		{
			AIGreandeAng.ang55,
			new GClass576(55f)
		}
	};

	public static AIGreanageThrowData CanThrowGrenade2(Vector3 from, Vector3 trg, BotGrenadeController bot, AIGreandeAng greandeAng, float maxThrowDistSqrt = -1f)
	{
		if (bot.HaveGrenade)
		{
			float maxPower = bot.MaxPower;
			return CanThrowGrenade2(from, trg, maxPower, greandeAng, maxThrowDistSqrt);
		}
		return new AIGreanageThrowData();
	}

	public static AIGreanageThrowData GetSuicideData(Vector3 from, Vector3 curLookDir)
	{
		return new AIGreanageThrowData(45f, 1E-05f, curLookDir, from, from + curLookDir, alwaysGood: true);
	}

	public static AIGreanageThrowData CanThrowGrenade2(Vector3 from, Vector3 trg, float maxPower, AIGreandeAng greandeAng, float minThrowDistSqrt = -1f, float maxPercent = 0.5f)
	{
		if (minThrowDistSqrt > 0f && (from - trg).sqrMagnitude < minThrowDistSqrt)
		{
			return new AIGreanageThrowData();
		}
		GClass576 gClass = Dictionary_0[greandeAng];
		float cos = gClass.Cos;
		float sin = gClass.Sin;
		float sin2 = gClass.Sin2;
		float tan = gClass.Tan;
		float ang = gClass.Ang;
		Vector3 vector = trg - from;
		float y = from.y;
		float num = Mathf.Abs(Physics.gravity.y);
		float x = vector.x;
		float z = vector.z;
		float y2 = vector.y;
		float num2 = Mathf.Sqrt(x * x + z * z);
		float num3 = num * num2 * num2;
		float num4 = num2 * tan - y2;
		float num5 = 2f * cos * cos;
		float num6 = num3 / (num4 * num5);
		if (num6 < 0f)
		{
			return new AIGreanageThrowData();
		}
		float num7 = Mathf.Sqrt(num6);
		if (num7 > maxPower)
		{
			return new AIGreanageThrowData();
		}
		float num8 = num7 * num7 * sin2 / num;
		Vector3 v = vector;
		v.y = 0f;
		v = GClass855.NormalizeFastSelf(v);
		Vector3 vector2 = v * num8;
		if ((from - trg).magnitude / vector2.magnitude < maxPercent)
		{
			return new AIGreanageThrowData();
		}
		float num9 = num7 * num7 * sin * sin / (2f * num);
		Vector3 vector3 = from + vector2 / 2f;
		vector3.y = y + num9;
		float duration = 4f;
		int num10 = (Application.isPlaying ? LocalBotSettingsProviderClass.Core.GRENADE_PRECISION : 10);
		Vector3[] array = new Vector3[num10 + 2];
		for (int i = 0; i < num10; i++)
		{
			float num11 = (float)(i + 1) * num2 / (float)num10;
			float num12 = smethod_0(num11, num7, tan, cos);
			Vector3 vector4 = from + num11 * v;
			vector4.y += num12;
			array[i + 1] = vector4;
		}
		array[0] = from;
		array[^1] = trg;
		int num13 = 0;
		RaycastHit hitInfo;
		while (true)
		{
			if (num13 < array.Length - 1)
			{
				Vector3 vector5 = array[num13];
				Vector3 vector6 = array[num13 + 1];
				Vector3 direction = vector6 - vector5;
				float magnitude = direction.magnitude;
				bool flag = Physics.Raycast(new Ray(vector5, direction), out hitInfo, magnitude, LayerMaskClass.HighPolyWithTerrainMask);
				Debug.DrawLine(vector5, vector6, flag ? Color.red : Color.green, duration);
				if (flag)
				{
					break;
				}
				num13++;
				continue;
			}
			return new AIGreanageThrowData(ang, num7 * 1.3f, vector, from, trg, alwaysGood: false);
		}
		return new AIGreanageThrowData(hitInfo.collider.gameObject);
	}

	public static Vector3 FindDangerPoint(Vector3 position, Vector3 force, float mass)
	{
		force /= mass;
		Vector3 vector = smethod_3(position, force);
		Vector3 midPoint = (position + vector) / 2f;
		CheckThreePoints(position, midPoint, vector, out var hitPos);
		return hitPos;
	}

	public static bool CheckThreePoints(Vector3 from, Vector3 midPoint, Vector3 target, out Vector3 hitPos)
	{
		Vector3 direction = midPoint - from;
		if (Physics.Raycast(new Ray(from, direction), out var hitInfo, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
		{
			Debug.DrawLine(from, hitInfo.point, Color.red, 6f);
			hitPos = hitInfo.point;
			return false;
		}
		Vector3 direction2 = midPoint - target;
		if (Physics.Raycast(new Ray(midPoint, direction2), out hitInfo, direction2.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
		{
			Debug.DrawLine(midPoint, hitInfo.point, Color.red, 6f);
			hitPos = hitInfo.point;
			return false;
		}
		hitPos = target;
		return true;
	}

	public static float smethod_0(float x, float power, float tan, float cos)
	{
		return tan * x - LocalBotSettingsProviderClass.Core.G * x * x / (2f * power * power * cos * cos);
	}

	public static Vector3 smethod_1(Vector3 trg, float dist, float grenadePerMeter)
	{
		float num = grenadePerMeter * dist;
		float num2 = GClass856.Random(0f - num, num);
		float num3 = GClass856.Random(0f - num, num);
		Vector3 result = trg;
		result.x += num2;
		result.z += num3;
		return result;
	}

	public static float smethod_2(float angRad, float power, float xDist, float percent)
	{
		float num = xDist * percent;
		float num2 = Mathf.Cos(angRad);
		return num * Mathf.Tan(angRad) - LocalBotSettingsProviderClass.Core.G * num * num / (2f * power * power * num2 * num2);
	}

	public static Vector3 smethod_3(Vector3 from, Vector3 force)
	{
		Vector3 v = new Vector3(force.x, 0f, force.z);
		if (v.sqrMagnitude <= 1E-05f)
		{
			v = new Vector3(0.01f, 0f, 0f);
		}
		Vector2 vector = new Vector2(v.magnitude, force.y);
		float num = 2f * vector.x * vector.y / LocalBotSettingsProviderClass.Core.G;
		if (vector.y < 0f)
		{
			num = 0f - num;
		}
		return GClass855.NormalizeFastSelf(v) * num + from;
	}
}
