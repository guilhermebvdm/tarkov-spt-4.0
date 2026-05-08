using UnityEngine;

public class GClass1154
{
	public float Compute(Vector3 listenerPos, BetterSource source, LoggerClass logger)
	{
		Vector3 position = source.transform.position;
		float maxDistance = source.MaxDistance;
		if (maxDistance <= 0f)
		{
			logger.LogWarn($"Incorrect MaxDistance ({maxDistance}) for source: {source.name}");
			return 0f;
		}
		float num = Vector3.Distance(position, listenerPos) / maxDistance;
		return Mathf.Clamp01(1f - num * num);
	}
}
