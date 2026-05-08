using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIController : MonoBehaviour
{
	public int controlWindowWidth = 250;

	public float sceneFadeLength = 0.5f;

	private bool bool_0 = true;

	private string string_0 = "";

	private float float_0;

	private int int_0;

	private Color color_0;

	private Trigger[] trigger_0;

	public void Start()
	{
		StartCoroutine(method_0());
		trigger_0 = GClass870.FindUnityObjectsOfType(typeof(Trigger)) as Trigger[];
		StartCoroutine(method_4());
	}

	public IEnumerator method_0()
	{
		yield break;
	}

	public void Update()
	{
		float_0 += Time.timeScale / Time.deltaTime;
		int_0++;
		if (Input.GetKeyDown(KeyCode.F1))
		{
			bool_0 = !bool_0;
		}
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Camera.main.SendMessage("FireCannon");
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	public void OnGUI()
	{
		GUI.color = new Color(color_0.r, color_0.g, color_0.b, GUI.color.a);
		GUI.Window(1, new Rect(10f, 10f, 75f, 50f), method_5, "FPS");
		GUI.color = new Color(1f, 1f, 1f, GUI.color.a);
		GUILayout.Window(2, new Rect(Screen.width - controlWindowWidth - 10, 10f, controlWindowWidth, 10f), method_1, "Controls", GUILayout.Width(controlWindowWidth));
	}

	public void method_1(int id)
	{
		Toggle(ref bool_0, "Show");
		if (bool_0)
		{
			GUILayout.Space(15f);
			GUILayout.BeginVertical("box");
			GUILayout.Label("Hold RMB to orbit scene.");
			GUILayout.Label("Press LMB over environment for destruction.");
			GUILayout.EndVertical();
			GUILayout.Space(15f);
			GUILayout.BeginVertical("box");
			GUILayout.Label("Shards : " + ShardPool.ShardsUsed);
			GUILayout.EndVertical();
			GUILayout.Space(15f);
			GUI.backgroundColor = new Color(1f, 1f, 1f, GUI.color.a);
			if (GUILayout.Button("Destroy Foundations", GUILayout.Width(controlWindowWidth - 20)))
			{
				StartCoroutine(method_2());
			}
			GUI.backgroundColor = new Color(1f, 0f, 0f, GUI.color.a);
			if (GUILayout.Button("Flatten", GUILayout.Width(controlWindowWidth - 20)))
			{
				StartCoroutine(method_3());
			}
			GUILayout.Space(15f);
			GUI.backgroundColor = new Color(0f, 1f, 0f, GUI.color.a);
			if (GUILayout.Button("Reset Scene", GUILayout.Width(controlWindowWidth - 20)))
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
			GUI.backgroundColor = new Color(1f, 1f, 1f, GUI.color.a);
		}
	}

	public IEnumerator method_2()
	{
		int num = 0;
		Trigger[] array = trigger_0;
		foreach (Trigger trigger in array)
		{
			if (!(trigger == null))
			{
				if ((double)trigger.transform.localPosition.y < -7.0)
				{
					trigger.GetComponent<Rigidbody>().AddForce(Vector3.up * 10000000f);
					trigger.TriggerDestruction(trigger.transform.position, Random.value);
				}
				int num2 = num + 1;
				num = num2;
				if (num % 2 == 0)
				{
					yield return null;
				}
			}
		}
	}

	public IEnumerator method_3()
	{
		int num = 0;
		Trigger[] array = trigger_0;
		foreach (Trigger trigger in array)
		{
			if (!(trigger == null))
			{
				trigger.TriggerDestruction(trigger.transform.position, Random.value);
				int num2 = num + 1;
				num = num2;
				if (num % 2 == 0)
				{
					yield return null;
				}
			}
		}
	}

	public IEnumerator method_4()
	{
		while (base.enabled)
		{
			float num = float_0 / (float)int_0;
			string_0 = num.ToString("f2");
			color_0 = ((!((double)num >= 10.0)) ? Color.red : (((double)num >= 25.0) ? Color.green : Color.yellow));
			float_0 = 0f;
			int_0 = 0;
			yield return new WaitForSeconds(0.5f);
		}
	}

	public void method_5(int windowID)
	{
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(string_0);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	public static void Toggle(ref bool value, string name)
	{
		GUILayout.BeginHorizontal("box");
		GUILayout.Label(name + " : ");
		value = GUILayout.Toggle(value, "");
		GUILayout.EndHorizontal();
	}
}
