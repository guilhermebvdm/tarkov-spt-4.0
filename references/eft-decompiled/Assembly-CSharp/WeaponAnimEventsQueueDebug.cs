using AnimationEventSystem;
using EFT;
using EFT.UI;
using UnityEngine;

public class WeaponAnimEventsQueueDebug : MonoBehaviour
{
	private GUIStyle guistyle_0;

	private GUIStyle guistyle_1;

	private GUIStyle guistyle_2;

	private Texture2D texture2D_0;

	private Player player_0;

	private float float_0;

	private float float_1;

	private float float_2;

	public void SetDebugObject(Player toDebug)
	{
		player_0 = toDebug;
		method_0();
	}

	public void OnGUI()
	{
		if (player_0 == null || MonoBehaviourSingleton<PreloaderUI>.Instance.Console.IsConsoleVisible)
		{
			return;
		}
		Player.AbstractHandsController handsController = player_0.HandsController;
		if (handsController == null || handsController.AnimationEventsEmitter == null)
		{
			return;
		}
		int num = 0;
		foreach (AnimationEventsSequenceData.GStruct142 item in handsController.AnimationEventsEmitter.EventsSequenceData.AnimationEventsDebugQueue)
		{
			num++;
			string animStateByNameHash = GClass758.GetAnimStateByNameHash(item.StateNameShortHash);
			Rect position = new Rect(690f, 15 + num * 20, 320f, 20f);
			if (item.ConditionPassed)
			{
				GUI.Box(position, animStateByNameHash + "." + item.EventName, guistyle_0);
			}
			else
			{
				GUI.Box(position, animStateByNameHash + "." + item.EventName + " (Conditions failed)", guistyle_1);
			}
		}
	}

	public void method_0()
	{
		if (guistyle_0 == null)
		{
			guistyle_0 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			texture2D_0 = method_1(2, 2, new Color(0.2f, 0.2f, 0.3f, 0.9f));
			guistyle_0.normal.background = texture2D_0;
			guistyle_0.normal.textColor = Color.white;
		}
		if (guistyle_1 == null)
		{
			guistyle_1 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			guistyle_1.normal.background = texture2D_0;
			guistyle_1.normal.textColor = Color.red;
		}
		if (guistyle_2 == null)
		{
			guistyle_2 = new GUIStyle
			{
				alignment = TextAnchor.UpperLeft
			};
			guistyle_2.normal.background = texture2D_0;
			guistyle_2.normal.textColor = Color.green;
		}
	}

	public Texture2D method_1(int width, int height, Color col)
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
