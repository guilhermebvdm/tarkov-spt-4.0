using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass957
{
	[NonSerialized]
	public Vector2 Vector2_0 = new Vector2(1.5f, 2.2f);

	[NonSerialized]
	public Vector2 Vector2_1 = new Vector2(0.3f, 0.7f);

	[NonSerialized]
	public Vector2 Vector2_2 = new Vector2(5f, 10f);

	[NonSerialized]
	public Vector2 Vector2_3 = new Vector2(0.15f, 0.4f);

	[NonSerialized]
	public Vector2 Vector2_4 = new Vector2(0f, 0.075f);

	[NonSerialized]
	public Vector2 Vector2_5 = new Vector2(0f, 0.075f);

	[NonSerialized]
	public Vector2 Vector2_6 = new Vector2(0.05f, 0.3f);

	[NonSerialized]
	public Vector2 Vector2_7 = new Vector2(0.05f, 0.3f);

	[NonSerialized]
	public float Float_0 = Mathf.Cos(45f);

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2 = 1f;

	[NonSerialized]
	public float Float_3 = 1f;

	[NonSerialized]
	public const float Float_4 = 2f;

	public GClass957()
	{
		Float_1 = (new Vector2(0.5f, 1f) - new Vector2(0.5f, 0.5f)).magnitude;
	}

	public void Setup(Vector2 startScaleDimension, Vector2 endScaleDimension, Vector2 dropCountRange, Vector2 maxRayLength, Vector2 startXdistribution, Vector2 startYdistribution, Vector2 endXdistribution, Vector2 endYdistribution, float offGlassScaleMultiplier)
	{
		Vector2_0 = startScaleDimension;
		Vector2_1 = endScaleDimension;
		Vector2_2 = dropCountRange;
		Vector2_3 = maxRayLength;
		Vector2_4 = startXdistribution;
		Vector2_5 = startYdistribution;
		Vector2_6 = endXdistribution;
		Vector2_7 = endYdistribution;
		Float_2 = offGlassScaleMultiplier;
	}

	public Queue<GClass956> GetDropsPositions(Vector3 direction, Queue<GClass956> freeDrops)
	{
		if (direction.sqrMagnitude < Mathf.Epsilon)
		{
			direction = new Vector3(0f, 0f, -1f);
		}
		Queue<GClass956> queue = new Queue<GClass956>();
		float hypotenuse;
		Vector2 vector = method_1(direction, out hypotenuse);
		hypotenuse *= UnityEngine.Random.Range(Vector2_3.x, Vector2_3.y);
		int num = 1;
		Vector2 vector2 = new Vector2(0f - direction.x, 0f - direction.z);
		int num2 = 0;
		while (true)
		{
			if (num2 < num)
			{
				float num3 = hypotenuse / (float)num * ((float)num2 + 1f);
				float percent = ((float)num2 + 1f) / (float)num;
				float x = method_0(Vector2_4, Vector2_6, percent);
				float y = method_0(Vector2_5, Vector2_7, percent);
				float num4 = method_0(Vector2_0, Vector2_1, percent, signed: false);
				Vector2 position = vector + vector2 * num3 + new Vector2(x, y);
				position.x = Mathf.Clamp01(position.x) * 2f - 1f;
				position.y = Mathf.Clamp01(position.y) * 2f - 1f;
				num4 *= 0.05f;
				num4 *= Float_3;
				num4 = Mathf.Max(num4, 0.15f);
				if (freeDrops.Count == 0)
				{
					break;
				}
				GClass956 gClass = freeDrops.Dequeue();
				gClass.Setup(position, new Vector3(num4, num4, num4), num2);
				queue.Enqueue(gClass);
				num2++;
				continue;
			}
			return queue;
		}
		return queue;
	}

	public Queue<GClass956> GetDropsPositionsBleeding(Vector3 direction, Queue<GClass956> freeDrops, bool isArterial)
	{
		if (direction.sqrMagnitude < Mathf.Epsilon)
		{
			direction = new Vector3(0f, 0f, -1f);
		}
		Queue<GClass956> queue = new Queue<GClass956>();
		float hypotenuse;
		Vector2 vector = method_1(direction, out hypotenuse);
		hypotenuse *= UnityEngine.Random.Range(Vector2_3.x, Vector2_3.y);
		float num = (isArterial ? 2f : 1f);
		int num2 = UnityEngine.Random.Range(Mathf.FloorToInt(num * Vector2_2.x), Mathf.CeilToInt(num * Vector2_2.y));
		Vector2 vector2 = new Vector2(0f - direction.x, 0f - direction.z);
		int num3 = 0;
		while (true)
		{
			if (num3 < num2)
			{
				float num4 = hypotenuse / (float)num2 * ((float)num3 + 1f);
				float percent = ((float)num3 + 1f) / (float)num2;
				float x = method_0(Vector2_4, Vector2_6, percent);
				float y = method_0(Vector2_5, Vector2_7, percent);
				float num5 = method_0(Vector2_0, Vector2_1, percent, signed: false);
				Vector2 position = vector + vector2 * num4 + new Vector2(x, y);
				position.x = Mathf.Clamp01(position.x) * 2f - 1f;
				position.y = Mathf.Clamp01(position.y) * 2f - 1f;
				num5 *= 0.05f;
				num5 *= Float_3;
				num5 = Mathf.Max(num5, 0.15f);
				if (freeDrops.Count == 0)
				{
					break;
				}
				GClass956 gClass = freeDrops.Dequeue();
				gClass.Setup(position, new Vector3(num5, num5, num5), num3);
				queue.Enqueue(gClass);
				num3++;
				continue;
			}
			return queue;
		}
		return queue;
	}

	public float method_0(Vector2 startDistribution, Vector2 endDistribution, float percent, bool signed = true)
	{
		Vector2 vector = percent * (endDistribution - startDistribution) + startDistribution;
		if (signed)
		{
			vector *= Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));
		}
		return UnityEngine.Random.Range(vector.x, vector.y);
	}

	public Vector2 method_1(Vector3 direction, out float hypotenuse)
	{
		Vector2 rhs = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.z));
		rhs.Normalize();
		float num = Vector2.Dot(new Vector2(0f, 1f), rhs);
		if (num < Float_0)
		{
			num = Vector2.Dot(new Vector2(1f, 0f), rhs);
		}
		hypotenuse = Float_1 / num;
		return new Vector2(0.5f, 0.5f) + hypotenuse * new Vector2(direction.x, direction.z);
	}

	public void ChangeGlassState(bool hasGlasses)
	{
		Float_3 = (hasGlasses ? 1f : Float_2);
	}
}
