using EFT;
using UnityEngine;

public class StaminaDebug : MonoBehaviour
{
	public BasePhysicalClass Physical;

	private GUIStyle guistyle_0;

	private Texture2D texture2D_0;

	private Player player_0;

	private RollingMedian rollingMedian_0 = new RollingMedian(5);

	private int int_0;

	private Vector3 vector3_0;

	public void SetDebugObject(Player toDebug)
	{
		player_0 = toDebug;
		Physical = toDebug.Physical;
		method_0();
	}

	public void method_0()
	{
		if (guistyle_0 == null)
		{
			guistyle_0 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			texture2D_0 = method_2(2, 2, new Color(0.2f, 0.2f, 0.3f, 0.9f));
			guistyle_0.normal.background = texture2D_0;
			guistyle_0.normal.textColor = Color.white;
		}
	}

	public void OnGUI()
	{
		if (player_0 != null)
		{
			GUI.Box(new Rect(10f, 10f, 220f, 120f), method_1(Physical.Stamina, "Stamina"), guistyle_0);
			GUI.Box(new Rect(230f, 10f, 220f, 120f), method_1(Physical.HandsStamina, "Hands"), guistyle_0);
			GUI.Box(new Rect(450f, 10f, 220f, 120f), method_1(Physical.Oxygen, "Oxygen"), guistyle_0);
			GUI.Box(new Rect(10f, 140f, 220f, 80f), $"{player_0.Profile.Nickname}\nWeight:{player_0.Physical.PreviousWeight:F1} kg;\nFatigue:{player_0.Physical.Fatigue:F1}\nSpeed: {Mathf.Round(rollingMedian_0.Avarage * 10f) / 10f} m/s", guistyle_0);
			GUI.Box(new Rect(230f, 140f, 220f, 80f), $"Inertia: {player_0.Physical.Inertia}", guistyle_0);
		}
		if (GUI.Button(new Rect(620f, 10f, 50f, 20f), "Close", guistyle_0))
		{
			Physical = null;
			Object.Destroy(this);
		}
		if (GUI.Button(new Rect(620f, 30f, 50f, 20f), "Scroll", guistyle_0))
		{
			Player[] array = Object.FindObjectsOfType<Player>();
			if (array.Length >= 1)
			{
				SetDebugObject(array[int_0 % array.Length]);
				int_0++;
			}
		}
	}

	public void Update()
	{
		if (player_0 != null)
		{
			rollingMedian_0.AddValue(Vector3.Distance(player_0.Transform.position, vector3_0) / Time.deltaTime);
			vector3_0 = player_0.Transform.position;
		}
	}

	public string method_1(GClass774 parameter, string title)
	{
		string text = $"{title}:\n{parameter.Current:F2}/{parameter.TotalCapacity.Value:F2}";
		foreach (PlayerPhysicalClass.GClass773 consumption in parameter.Consumptions)
		{
			text += $"\n–{consumption.Delta.Value:F2} ({consumption.ConsumptionType})";
		}
		return text + "\nRestoration: " + ((Time.time > parameter.DisableRestoration) ? parameter.SelfRestoration.Value.ToString("F2") : ("Disabled for: " + (parameter.DisableRestoration - Time.time).ToString("F2")));
	}

	public Texture2D method_2(int width, int height, Color col)
	{
		Color[] array = new Color[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = col;
		}
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public void OnDestroy()
	{
		Object.DestroyImmediate(texture2D_0);
	}
}
