using System.Collections.Generic;
using EFT.Game.Spawning;
using UnityEngine;

public class SpawnResultDebug : MonoBehaviour
{
	private ESpawnCategory espawnCategory_0;

	private LocalGameLoggerClass.GClass1886 gclass1886_0;

	public List<BotDistanceInfo> BotDistances = new List<BotDistanceInfo>();

	public SpawnPointMarker NearestSpawnPoint;

	public float NearestSpawnPointDistance = -1f;

	public float SpawnPointDistance;

	public string NearestSpawnPointInfo;

	public void Init(LocalGameLoggerClass.GClass1886 data)
	{
		gclass1886_0 = data;
		espawnCategory_0 = data.Category;
		base.transform.position = data.Position;
		UpdateDistances();
	}

	public void UpdateDistances()
	{
		method_0();
		UpdateNearestSpawnPoint();
	}

	public void UpdateNearestSpawnPoint()
	{
		SpawnPointMarker[] array = Object.FindObjectsOfType<SpawnPointMarker>();
		float num = float.MaxValue;
		SpawnPointMarker spawnPointMarker = null;
		SpawnPointMarker[] array2 = array;
		foreach (SpawnPointMarker spawnPointMarker2 in array2)
		{
			if (!(spawnPointMarker2.GetComponent<SphereCollider>() == null))
			{
				float num2 = Vector3.Distance(base.transform.position, spawnPointMarker2.transform.position);
				if (num2 < num)
				{
					num = num2;
					spawnPointMarker = spawnPointMarker2;
				}
			}
		}
		if (spawnPointMarker != null)
		{
			NearestSpawnPoint = spawnPointMarker;
			SphereCollider component = spawnPointMarker.GetComponent<SphereCollider>();
			if (component != null)
			{
				NearestSpawnPointInfo = $"Расстояние: {num:F1}м, Радиус: {component.radius:F1}м";
				SpawnPointDistance = component.radius;
				NearestSpawnPointDistance = num;
			}
		}
		else
		{
			NearestSpawnPoint = null;
			NearestSpawnPointInfo = "Не найден";
		}
	}

	public void method_0()
	{
		if (gclass1886_0 != null && gclass1886_0.Bots != null)
		{
			BotDistances.Clear();
			LocalGameLoggerClass.GClass1887[] bots = gclass1886_0.Bots;
			foreach (LocalGameLoggerClass.GClass1887 gClass in bots)
			{
				BotDistances.Add(new BotDistanceInfo
				{
					BotPosition = gClass.Position,
					Distance = Vector3.Distance(base.transform.position, gClass.Position),
					Side = gClass.Side
				});
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		float a = 0.9f;
		switch (espawnCategory_0)
		{
		case ESpawnCategory.BotPmc:
			Gizmos.color = new Color(0.2f, 0.8f, 0.9f, a);
			break;
		case ESpawnCategory.Coop:
			Gizmos.color = new Color(0f, 0f, 0f, a);
			break;
		case ESpawnCategory.Player:
			Gizmos.color = new Color(0.9f, 0.4f, 0.2f, a);
			break;
		case ESpawnCategory.Bot:
			Gizmos.color = new Color(0.4f, 0.9f, 0.2f, a);
			break;
		default:
			Gizmos.color = new Color(0.4f, 0.4f, 0.4f, 0.5f);
			break;
		case ESpawnCategory.Boss:
			Gizmos.color = new Color(0.2f, 0.4f, 0.9f, a);
			break;
		}
		GClass850.DrawCube(base.transform.position + Vector3.up * 150f * 0.5f, base.transform.rotation, new Vector3(0.7f, 150f, 0.7f));
		if (gclass1886_0 != null)
		{
			if (NearestSpawnPoint != null)
			{
				Gizmos.color = Color.magenta;
				GClass850.DrawCube(NearestSpawnPoint.transform.position + Vector3.up * 50f * 0.5f, NearestSpawnPoint.transform.rotation, new Vector3(0.7f, 50f, 0.7f));
			}
			Gizmos.color = Color.yellow;
			LocalGameLoggerClass.GClass1887[] bots = gclass1886_0.Bots;
			foreach (LocalGameLoggerClass.GClass1887 obj in bots)
			{
				Gizmos.DrawLine(obj.Position, gclass1886_0.Position);
				Gizmos.DrawSphere(obj.Position, 1f);
			}
			Gizmos.color = Color.red;
			bots = gclass1886_0.Players;
			foreach (LocalGameLoggerClass.GClass1887 obj2 in bots)
			{
				Gizmos.DrawLine(obj2.Position, gclass1886_0.Position);
				Gizmos.DrawSphere(obj2.Position, 1f);
			}
		}
	}

	public void OnGUI()
	{
		if (gclass1886_0 == null || gclass1886_0.Bots == null)
		{
			return;
		}
		Camera current = Camera.current;
		if (current == null)
		{
			return;
		}
		LocalGameLoggerClass.GClass1887[] bots = gclass1886_0.Bots;
		foreach (LocalGameLoggerClass.GClass1887 gClass in bots)
		{
			Vector3 vector = current.WorldToScreenPoint(gClass.Position);
			if (!(vector.z < 0f))
			{
				float num = Vector3.Distance(base.transform.position, gClass.Position);
				string text = $"Расстояние до бота: {num:F1}м";
				GUIStyle gUIStyle = new GUIStyle();
				gUIStyle.normal.textColor = Color.yellow;
				gUIStyle.fontSize = 12;
				gUIStyle.alignment = TextAnchor.MiddleCenter;
				Vector2 vector2 = gUIStyle.CalcSize(new GUIContent(text));
				GUI.Label(new Rect(vector.x - vector2.x / 2f, (float)Screen.height - vector.y - vector2.y / 2f, vector2.x, vector2.y), text, gUIStyle);
			}
		}
	}
}
