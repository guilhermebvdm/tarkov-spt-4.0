using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.Ballistics;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.UI;

public class BulletSimulator : MonoBehaviour
{
	private BallisticsCalculator ballisticsCalculator_0;

	private EftBulletClass eftBulletClass;

	private Text text_0;

	[Header("Ammo data")]
	public AmmoTemplate AmmoTemplate;

	public AmmoItemClass Ammo;

	[Header("Aiming data")]
	public float MaxDistance = 2000f;

	public float CurrentDistance = 10f;

	public float DistanceStep = 5f;

	[Header("Simulation data")]
	public float SimulationStep = 1E-05f;

	public float SimulationTime = 4f;

	public float MarkerScaleStep = 0.003f;

	private GameObject gameObject_0;

	public async Task Start()
	{
		Ammo = new AmmoItemClass("", AmmoTemplate);
		await smethod_0();
		method_1();
		method_2();
		SimulateShot();
		method_0();
		method_3();
	}

	public void method_0()
	{
		gameObject_0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		gameObject_0.transform.position = base.transform.position;
		gameObject_0.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
	}

	public static async Task smethod_0()
	{
		IOperation<IAssetsManager> operation = await GClass1805.CreateAssetsManager();
		if (operation.Succeed)
		{
			AssetsManagerSingletonClass.Manager = operation.Result;
		}
	}

	public void method_1()
	{
		Canvas canvas = new GameObject("BulletSimulator Canvas").AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		Text text = new GameObject("bullet distance Info").AddComponent<Text>();
		text.transform.SetParent(canvas.transform);
		text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		text.color = Color.green;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		text.rectTransform.anchorMin = new Vector2(0.7f, 0f);
		text.rectTransform.anchorMax = new Vector2(1f, 0.5f);
		text.rectTransform.offsetMax = Vector2.zero;
		text.rectTransform.offsetMin = Vector2.zero;
		text_0 = text;
	}

	public void method_2()
	{
		ballisticsCalculator_0 = BallisticsCalculator.Create(base.gameObject, 0, OnShotFinishedHandler);
	}

	public void SimulateShot()
	{
		Debug.Log("Shot started");
		eftBulletClass = ballisticsCalculator_0.CreateShot(Ammo, base.transform.position, base.transform.forward, 1, null, null);
		ballisticsCalculator_0.SimulateShot(eftBulletClass, SimulationTime, SimulationStep);
	}

	public void OnShotFinishedHandler(EftBulletClass shot)
	{
		Debug.Log("ShotFinished");
	}

	public void Update()
	{
		if (eftBulletClass != null)
		{
			method_4();
			method_6();
		}
	}

	public void method_3()
	{
		if (eftBulletClass == null)
		{
			gameObject_0.SetActive(value: false);
			return;
		}
		gameObject_0.SetActive(value: true);
		gameObject_0.transform.position = base.transform.position + base.transform.forward * CurrentDistance;
		List<Vector3> positionHistory = eftBulletClass.PositionHistory;
		int i;
		for (i = 0; i < positionHistory.Count && !(Vector3.Dot(positionHistory[i], base.transform.forward) > CurrentDistance); i++)
		{
		}
		if (i == 0)
		{
			gameObject_0.transform.position = positionHistory[i];
		}
		else if (i == positionHistory.Count)
		{
			gameObject_0.transform.position = positionHistory[positionHistory.Count - 1];
		}
		else
		{
			Vector3 lhs = positionHistory[i - 1];
			Vector3 lhs2 = positionHistory[i];
			float num = Vector3.Dot(lhs2, base.transform.forward) - Vector3.Dot(lhs, base.transform.forward);
			float num2 = (CurrentDistance - Vector3.Dot(lhs, base.transform.forward)) / num * (Vector3.Dot(lhs2, base.transform.up) - Vector3.Dot(lhs, base.transform.up));
			gameObject_0.transform.position = new Vector3(base.transform.position.x, lhs.y + num2, CurrentDistance);
		}
		float num3 = Vector3.Distance(gameObject_0.transform.position, base.transform.position) * MarkerScaleStep;
		gameObject_0.transform.localScale = num3 * Vector3.one;
	}

	public void method_4()
	{
		if (Input.GetKey(KeyCode.UpArrow))
		{
			method_5(Mathf.Min(CurrentDistance + DistanceStep, MaxDistance));
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			method_5(Mathf.Max(0f, CurrentDistance - DistanceStep));
		}
		else if (Input.GetKeyDown(KeyCode.Space))
		{
			SimulateShot();
			method_3();
		}
	}

	public void method_5(float value)
	{
		CurrentDistance = value;
		method_3();
	}

	public void method_6()
	{
		text_0.text = "Distance: " + (Vector3.Dot(gameObject_0.transform.position, base.transform.forward) - base.transform.position.z) + " m\nHeight: " + (Vector3.Dot(gameObject_0.transform.position, base.transform.up) - base.transform.position.y) + " m\nTarget scale: " + gameObject_0.transform.localScale.x;
	}

	public void OnDrawGizmos()
	{
		if (GClass842.DisabledForNow)
		{
			return;
		}
		Gizmos.color = Color.magenta;
		if (eftBulletClass != null)
		{
			List<Vector3> positionHistory = eftBulletClass.PositionHistory;
			for (int i = 1; i < positionHistory.Count; i++)
			{
				Gizmos.DrawLine(positionHistory[i - 1], positionHistory[i]);
			}
		}
	}
}
